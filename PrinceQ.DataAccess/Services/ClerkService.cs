﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using System.Net;
using static PrinceQ.DataAccess.Response.ServiceResponses;

namespace PrinceQ.DataAccess.Services
{
    public class ClerkService : IClerk
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<QueueHub> _hubContext;

        public ClerkService(IUnitOfWork unitOfWork, IHubContext<QueueHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        //Filling AND Releasing VIEW
        public async Task<GeneralResponse> ServingVM(string userId, string ipAddress)
        {
            var clerkVM = new ClerkVM
            {
                Users = await _unitOfWork.users.Get(u => u.Id == userId),
            };
            if (clerkVM is null) return new GeneralResponse(false, null, "failed"); 

            return new GeneralResponse(true, clerkVM, "Success");
        }
        //Generate Queue Number VIEW
        public async Task<GeneralResponse> GenerateVM(string userId)
        {
            var viewModel = new RPVM
            {
                Categories = await _unitOfWork.category.GetAll(),
                Users = await _unitOfWork.users.Get(u => u.Id == userId),
            };
            if (viewModel is null) return new GeneralResponse(false, null, "failed");

            return new GeneralResponse(true, viewModel, "Success");
        }
        public async Task<DualResponse> GenerateQueue(int categoryId)
        {
            if (categoryId == 0) return new DualResponse(false, null, null, "there is no an error!");

            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var selectedCategory = await _unitOfWork.category.Get(c => c.CategoryId == categoryId);

            var queuesResult = await _unitOfWork.queueNumbers.GetAll(q => q.CategoryId == categoryId && q.QueueId == currentDate);
            var lastQueueNumber = queuesResult.OrderByDescending(q => q.QueueNumber).FirstOrDefault();

            var queueCount = lastQueueNumber != null && lastQueueNumber.QueueId == currentDate ? (int)lastQueueNumber.QueueNumber! + 1 : 1;

            var queueNumber = new Queues
            {
                QueueId = currentDate,
                CategoryId = categoryId,
                StatusId = 1,
                QueueNumber = queueCount,
                Generate_At = DateTime.Now,
            };

            _unitOfWork.queueNumbers.Add(queueNumber);
            await _unitOfWork.SaveAsync();
            await _hubContext.Clients.All.SendAsync("UpdateQueueFromPersonnel");
            await _hubContext.Clients.All.SendAsync("updateGenerateCard");
            return new DualResponse(true, queueNumber, selectedCategory, "Generation successful");
        }
        //Get Queue
        public async Task<GeneralResponse> GetQueue(string date, int categoryId, int queueNumber)
        {
            var queue = await _unitOfWork.queueNumbers.Get(a => a.CategoryId == categoryId && a.QueueNumber == queueNumber && a.QueueId == date);
            if (queue == null) return new GeneralResponse(false, null, "Not found!");

            return new GeneralResponse(true, queue, "Print Successful!");
        }
        //Get Recenelty Generated
        public async Task<GeneralResponse> RecentDataQueue()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var queueNumbers = await _unitOfWork.queueNumbers.GetAll(q => q.QueueId == currentDate);

            if (queueNumbers == null || !queueNumbers.Any())
                return new GeneralResponse(false, null, "No data yet.");

            var data = queueNumbers.Select(q => new
            {
                generateDate = q.QueueId,
                generate_at = q.Generate_At,
                categoryId = q.CategoryId,
                queueNumber = q.QueueNumber ?? 0,
                category = GetCategoryString((int)q.CategoryId),
            }).ToList(); 

            var qData = queueNumbers.GroupBy(q => q.CategoryId).ToDictionary(g => g.Key, g => g.Count());

            var categoryCounts = new
            {
                categoryA = qData.TryGetValue(1, out var catA) ? catA : 0,
                categoryB = qData.TryGetValue(2, out var catB) ? catB : 0,
                categoryC = qData.TryGetValue(3, out var catC) ? catC : 0,
                categoryD = qData.TryGetValue(4, out var catD) ? catD : 0
            };

            return new GeneralResponse(true, new { data, categoryCounts }, "Successfully fetched.");
        }

        //Designated Clerk Number
        public async Task<GeneralResponse> DesignatedClerk(string ipAddress, string userId)
        {
            var device = await _unitOfWork.device.Get(u => u.IPAddress == ipAddress && u.UserId == userId);

            if (device is null)
            {
                var user = await _unitOfWork.users.Get(u=>u.Id == userId);

                var clerkUser = await _unitOfWork.device.Get(u => u.IPAddress == ipAddress);
                if (clerkUser != null)
                {
                    clerkUser.UserId = user?.Id;
                    _unitOfWork.device.Update(clerkUser);
                    await _unitOfWork.SaveAsync();
                }

                return new GeneralResponse(true, device = clerkUser, "Success");
            }

            return new GeneralResponse(true, device, "Success");
        }
        //Get All Waiting Queue Number on Each Categories
        public async Task<GeneralResponse> GetAllWaitingQueue(string userId)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var queueNumbers = await _unitOfWork.queueNumbers.GetAll(q => q.StatusId == 1 && q.StageId == null && q.QueueId == currentDate);

            if (queueNumbers is null) return new GeneralResponse(false, null, "queueNumbers not found");

            var userCate = await _unitOfWork.userCategories.GetAll(uc => uc.UserId == userId);
            var userCategories = userCate.OrderBy(uc => uc.CategoryId).ToList();
            var categories = await _unitOfWork.category.GetAll();

            var userResult = userCategories.Select(uc => new
            {
                categoryId = uc.CategoryId,
                queueCount = queueNumbers.Count(q => q.CategoryId == uc.CategoryId),
                category = categories.FirstOrDefault(c=>c.CategoryId == uc.CategoryId)?.Description,
            }).ToList();

            if (userResult is null) return new GeneralResponse(false, null, "categories not found");

            return new GeneralResponse(true, userResult, "Get categories Success");
        }
        //GET CLERK SERVING
        public async Task<GetResponse> GetServings(string userId, string ipAddress)
        {
            var servingData = await _unitOfWork.servings.Get(u => u.UserId == userId && u.Served_At.Date == DateTime.Today);
            if (servingData is null) return new GetResponse(false, null, "There is no QueueNumber");
            var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
            var clerkNum = device?.ClerkNumber;

            var qNumber = await _unitOfWork.queueNumbers.Get(s => s.QueueId == servingData.Served_At.ToString("yyyyMMdd") && s.CategoryId == servingData.CategoryId
            && s.QueueNumber == servingData.QueueNumberServe);

            return new GetResponse(true, new { categoryId = qNumber?.CategoryId, queueNumber = qNumber?.QueueNumber, qNumber?.QueueId, qNumber?.StageId, qNumber?.Total_Cheques }, "Success");
        }
        //Announce Cut Off
        public async Task<GetResponse> AnnounceCutOff()
        {
            await _hubContext.Clients.All.SendAsync("CutOff");
            return new GetResponse(true, null, "Success");
        }
        //Call Number
        public async Task<GeneralResponse> CallQueueNumber(string userId, string ipAddress)
        {
            var queueServe = await _unitOfWork.servings.Get(u => u.UserId == userId && u.Served_At.Date == DateTime.Today);
            var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
            var clerkNum = device?.ClerkNumber;
            var queues = new[] {
                new
                {
                    queueNumberServe = queueServe?.QueueNumberServe,
                    categoryId = queueServe?.CategoryId,
                    clerkNumber = clerkNum
                }
            };
            await _hubContext.Clients.All.SendAsync("CallQueueNumber", queueServe?.CategoryId, queueServe?.QueueNumberServe, clerkNum);

            return new GeneralResponse(true, queues, "Call Successfully");
        }
        //GET RESERVE QUEUENUMBER
        public async Task<GeneralResponse> GetReservedQueues(string userId)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var queueNumbers = await _unitOfWork.queueNumbers.GetAll(q => q.StatusId == 3 && q.QueueId == currentDate);
            if (queueNumbers != null)
            {
                var reserve = await _unitOfWork.queueNumbers.GetAll(r => r.QueueId != null && r.StatusId == 3 && r.QueueId == currentDate);
                var reserveQueue = reserve.OrderBy(rq => rq.Reserve_At).Where(r => _unitOfWork.userCategories.Any(uc => uc.UserId == userId && uc.CategoryId == r.CategoryId))
                    .OrderByDescending(r => r.Reserve_At)
               .Select((r) => new
               {
                   generateDate = r.QueueId,
                   CategId = r.CategoryId,
                   Category = GetCategoryString((int)r.CategoryId!),
                   QNumber = r.QueueNumber,
                   Reserv_At = r.Reserve_At,
                   stageId = r.StageId,
               })
               .ToList();
                return new GeneralResponse(true, reserveQueue, "Get all reserve queues Success.");
            }
            else
            {
                return new GeneralResponse(false, null, "There is no Queue Number.");
            }
        }
        //GET Filling QUEUENUMBER
        public async Task<GeneralResponse> GetFillingUpQueues(string userId)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var filling = await _unitOfWork.queueNumbers.GetAll(r => r.QueueId != null && r.QueueId == currentDate && r.CategoryId != 4 && r.StageId == 1 && r.StatusId != 1 && r.StatusId != 4 && r.Reserve_At == null);
            
            if(filling is null) return new GeneralResponse(false, null, "There is no Queue Number.");

            var fillingQueue = filling.OrderBy(rq => rq.ForFilling_start)
           .Select((r) => new
           {
               generateDate = r.QueueId,
               CategId = r.CategoryId,
               Category = GetCategoryString((int)r.CategoryId!),
               QNumber = r.QueueNumber,
               Filling_At = r.ForFilling_start,
           })
           .ToList();
            return new GeneralResponse(true, fillingQueue, "Get all filling up queues Success.");
        }
        //GET Releasing QUEUENUMBER
        public async Task<GeneralResponse> GetReleasingQueues(string userId)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var filling = await _unitOfWork.queueNumbers.GetAll(r => r.QueueId != null && r.QueueId == currentDate && r.StageId == 2 && r.StatusId == 2 && r.Reserve_At == null && r.Releasing_end == null);

            if (filling is null) return new GeneralResponse(false, null, "There is no Queue Number.");

            var fillingQueue = filling.OrderBy(rq => rq.Releasing_start)
           .Select((r) => new
           {
               generateDate = r.QueueId,
               CategId = r.CategoryId,
               Category = GetCategoryString((int)r.CategoryId!),
               QNumber = r.QueueNumber,
               Releasing_At = r.Releasing_start,
           })
           .ToList();
            return new GeneralResponse(true, fillingQueue, "Get all filling up queues Success.");
        }
        //NEXT QUEUENUMBER
        public async Task<GeneralResponse> NextQueueNumber(int Id, string userId,string ipAddress)
        {
            var userAccess = await _unitOfWork.users.Get(u=> u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                //Get the queueNumber first
                var queueI = await _unitOfWork.queueNumbers.GetAll(q => q.CategoryId == Id && q.QueueId == currentDate && q.StatusId == 1 && q.StageId == null);
                var queueItem = queueI.OrderBy(q => q.QueueNumber).FirstOrDefault();

                if (queueItem != null)
                {
                    queueItem.StatusId = 5;
                    queueItem.StageId = 1;
                    queueItem.ForFilling_start = DateTime.Now;

                    var servingData = await _unitOfWork.servings.Get(u => u.UserId == userId);
                    var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
                    var clerkNum = device?.ClerkNumber;
                    if (servingData != null)
                    {
                        var prevQ = await _unitOfWork.queueNumbers.Get(q => q.QueueId == servingData.Served_At.ToString("yyyyMMdd") && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe);
                        if (prevQ is not null)
                        {
                            if (prevQ.StageId == 2 && prevQ.Total_Cheques == null)
                            {
                                return new GeneralResponse(false, new { prevQ.Total_Cheques, prevQ.QueueId, prevQ.CategoryId, prevQ.QueueNumber }, "Input the number of cheque first");
                            }
                            if(prevQ.CategoryId != 4)
                            {
                                prevQ.StatusId = 2;
                            }
                            if (prevQ.CategoryId == 4)
                            {
                                prevQ.ForFilling_end = DateTime.Now;
                            }
                            _unitOfWork.queueNumbers.Update(prevQ);
                        }
                        //For Filling Start End
                        var prevQForForFilling = await _unitOfWork.forFilling.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForFilling is not null)
                        {
                            prevQForForFilling.Serve_end = DateTime.Now;
                            _unitOfWork.forFilling.Update(prevQForForFilling);
                        }
                        //For Releasing Start End
                        var prevQForForReleasing = await _unitOfWork.releasing.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForReleasing is not null)
                        {
                            prevQForForReleasing.Serve_end = DateTime.Now;
                            _unitOfWork.releasing.Update(prevQForForReleasing);
                        }


                        servingData.CategoryId = Id;
                        servingData.QueueNumberServe = (int)queueItem.QueueNumber!;
                        servingData.Served_At = DateTime.Now;
                        _unitOfWork.servings.Update(servingData);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", servingData?.CategoryId, servingData?.QueueNumberServe, clerkNum);
                    }
                    else
                    {
                        var serving = new Serving
                        {
                            UserId = userId,
                            CategoryId = (int)queueItem.CategoryId!,
                            QueueNumberServe = (int)queueItem.QueueNumber!,
                            Served_At = DateTime.Now,
                        };
                        _unitOfWork.servings.Add(serving);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", serving?.CategoryId, serving?.QueueNumberServe, clerkNum);

                    }

                    if (queueItem.StageId == 1)
                    {
                        await UpdateClerkServeForFilling(userId, (int)queueItem.CategoryId, (int)queueItem.QueueNumber);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        await UpdateClerkServeReleasing(userId, (int)queueItem.CategoryId, (int)queueItem.QueueNumber);
                    }

                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();

                    //For REALTIME
                    await _hubContext.Clients.All.SendAsync("UpdateQueue", Id);
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                    await _hubContext.Clients.All.SendAsync("RecentServing");
                    await _hubContext.Clients.All.SendAsync("updateServingCard");
                    return new GeneralResponse(true, queueItem, "successful");
                }
                else
                {
                    return new GeneralResponse(false, null, "There is no Queue Number.");
                }
            }
            else 
            {
                return new GeneralResponse(true, null, "Your account is disabled.");
            }         
          
           
        }
        //RESERVE QUEUENUMBER
        public async Task<GeneralResponse> ReserveQueueNumber(string userId, string ipAddress)
        {
            var servingQueue = await _unitOfWork.servings.Get(q => q.UserId == userId && q.Served_At.Date == DateTime.Today);
            if(servingQueue != null) { 
                _unitOfWork.servings.Remove(servingQueue!);
            }

            var servingQueue_Date = servingQueue?.Served_At.Date.ToString("yyyyMMdd");
            var currentDate = DateTime.Today.ToString("yyyyMMdd");

            if (servingQueue != null)
            {
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.CategoryId == servingQueue.CategoryId && q.QueueNumber == servingQueue.QueueNumberServe && q.QueueId == servingQueue_Date);

                if (queueItem != null)
                {
                    queueItem.ClerkId = userId;
                    queueItem.StatusId = 3;
                    queueItem.Reserve_At = DateTime.Now;

                    if (queueItem.StageId == 1)
                    {
                        queueItem.ForFilling_start_Backup = null;
                        queueItem.ForFilling_end = null;
                        await RemoveClerkServeForFilling(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        queueItem.Releasing_start_Backup = null;
                        queueItem.Releasing_end = null;
                        await RemoveClerkServeReleasing(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }

                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();

                    var device = await _unitOfWork.device.Get(d=> d.IPAddress == ipAddress);
                    var clerkNum = device?.ClerkNumber;
                    await _hubContext.Clients.All.SendAsync("QueueDisplayInTvRemove", clerkNum);

                    await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                    await _hubContext.Clients.All.SendAsync("reservedQueue");
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("RecentServing");
                    await _hubContext.Clients.All.SendAsync("updateReservedCard");
                }
                return new GeneralResponse(true, null, "Reserve Success.");
            }
            else
            {
                return new GeneralResponse(false, null, "Reserve failed.");
            }

        }
        //Cancel QUEUENUMBER
        public async Task<GeneralResponse> CancelQueueNumber(string userId, string ipAddress)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var servingQueue = await _unitOfWork.servings.Get(q => q.UserId == userId && q.Served_At.Date == DateTime.Today);

            if (servingQueue != null)
            {
                _unitOfWork.servings.Remove(servingQueue!);

                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.CategoryId == servingQueue.CategoryId && q.QueueNumber == servingQueue.QueueNumberServe && q.QueueId == currentDate);

                if (queueItem != null)
                {
                    queueItem.StatusId = 4;
                    queueItem.ClerkId = userId;
                    queueItem.Reserve_At = null;

                    if (queueItem.StageId == 1)
                    {
                        queueItem.ForFilling_start = null;
                        queueItem.ForFilling_end = null;
                        //Remove For Filling Serve 
                        await RemoveClerkServeForFilling(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        queueItem.Releasing_start = null;
                        queueItem.Releasing_end = null;
                        //Remove For Releasing Serve 
                        await RemoveClerkServeReleasing(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }


                    _unitOfWork.queueNumbers.Update(queueItem!);
                    await _unitOfWork.SaveAsync();

                    var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
                    var clerkNum = device?.ClerkNumber;
                    await _hubContext.Clients.All.SendAsync("QueueDisplayInTvRemove", clerkNum);
                    await _hubContext.Clients.Group(userId).SendAsync("cancelQueueInMenu");
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("RecentServing");
                    await _hubContext.Clients.All.SendAsync("updateCancelCard");
                }

                return new GeneralResponse(true, null, "Cancel successful.");
            }
            else
            {
                return new GeneralResponse(false, null, "There is an error occurred.");
            }
        }

        ////// -------------QUEUE IN TABLES-------------- //////

        //Serve QUEUENUMBER From Releasing Table
        public async Task<GeneralResponse> ServeQueueFromTable(string generateDate, int categoryId, int qNumber, string userId, string ipAddress)
        {
            var userAccess = await _unitOfWork.users.Get(u => u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.QueueId == generateDate && q.CategoryId == categoryId && q.QueueNumber == qNumber);

                if (queueItem != null)
                {
                    queueItem.StatusId = 5;
                    queueItem.ClerkId = null;
                    queueItem.Reserve_At = null;

                    if (queueItem.StageId == 1)
                    {
                        queueItem.ForFilling_start = DateTime.Now;

                        //For Filling Serve Start 
                        await UpdateClerkServeForFilling(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        queueItem.Releasing_start = DateTime.Now;

                        //For Releasing Serve Start 
                        await UpdateClerkServeReleasing(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }

                    //For Serving the QueueNumbers
                    var servingData = await _unitOfWork.servings.Get(u => u.UserId == userId && u.Served_At.Date == DateTime.Today);
                    var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
                    var clerkNum = device?.ClerkNumber;
                    if (servingData != null && servingData.Served_At.Date == DateTime.Today)
                    {
                        var prevQ = await _unitOfWork.queueNumbers.Get(q => q.QueueId == currentDate && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe);
                        if (prevQ is not null)
                        {
                            if (prevQ.StageId == 2 && prevQ.Total_Cheques == null)
                            {
                                return new GeneralResponse(false, new { prevQ.Total_Cheques, prevQ.QueueId, prevQ.CategoryId, prevQ.QueueNumber }, "Input the number of cheque first");
                            }
                            prevQ.StatusId = 2;
                            if (prevQ.CategoryId == 4)
                            {
                                prevQ.ForFilling_end = DateTime.Now;
                            }
                            _unitOfWork.queueNumbers.Update(prevQ);
                        }
                        //For Filling Served End
                        var prevQForForFilling = await _unitOfWork.forFilling.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForFilling is not null)
                        {
                            prevQForForFilling.Serve_end = DateTime.Now;
                            _unitOfWork.forFilling.Update(prevQForForFilling);
                        }
                        //For Releasing Served End
                        var prevQForForReleasing = await _unitOfWork.releasing.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForReleasing is not null)
                        {
                            prevQForForReleasing.Serve_end = DateTime.Now;
                            _unitOfWork.releasing.Update(prevQForForReleasing);
                        }
                        servingData.CategoryId = (int)queueItem.CategoryId!;
                        servingData.QueueNumberServe = (int)queueItem.QueueNumber!;
                        servingData.Served_At = DateTime.Now;
                        _unitOfWork.servings.Update(servingData);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", servingData?.CategoryId, servingData?.QueueNumberServe, clerkNum);
                    }
                    else
                    {
                        var serving = new Serving
                        {
                            UserId = userId,
                            CategoryId = (int)queueItem.CategoryId!,
                            QueueNumberServe = (int)queueItem.QueueNumber!,
                            Served_At = DateTime.Now,
                        };
                        _unitOfWork.servings.Add(serving);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", serving?.CategoryId, serving?.QueueNumberServe, clerkNum);
                    }
                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();

                    await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                    await _hubContext.Clients.All.SendAsync("ServeInReservedQueue", queueItem);
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("releasingQueue");
                    await _hubContext.Clients.All.SendAsync("RecentServing");
                    await _hubContext.Clients.All.SendAsync("updateServingCard");
                    return new GeneralResponse(true, queueItem, "Served from reserve table success.");
                }
                else
                {
                    return new GeneralResponse(false, null, "There is no Queue Number.");
                }
            }
            else
            {
                return new GeneralResponse(false, null, "Your account is disabled.");
            }
        }

        //Serve QUEUENUMBER From Reserve Table
        public async Task<GeneralResponse> ServeQueueFromReserveTable(string generateDate, int categoryId, int qNumber, string userId, string ipAddress)
        {
            var userAccess = await _unitOfWork.users.Get(u => u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.QueueId == generateDate && q.CategoryId == categoryId && q.QueueNumber == qNumber);

                if (queueItem != null)
                {
                    queueItem.StatusId = 5;
                    queueItem.ClerkId = null;
                    queueItem.Reserve_At = null;

                    if (queueItem.StageId == 1)
                    {
                        queueItem.ForFilling_start_Backup = DateTime.Now;

                        //For Filling Serve Start 
                        await UpdateClerkServeForFilling(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        queueItem.Releasing_start_Backup = DateTime.Now;

                        //For Releasing Serve Start 
                        await UpdateClerkServeReleasing(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }

                    //For Serving the QueueNumbers
                    var servingData = await _unitOfWork.servings.Get(u => u.UserId == userId && u.Served_At.Date == DateTime.Today);
                    var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
                    var clerkNum = device?.ClerkNumber;

                    if (servingData != null && servingData.Served_At.Date == DateTime.Today)
                    {
                        var prevQ = await _unitOfWork.queueNumbers.Get(q => q.QueueId == servingData.Served_At.ToString("yyyyMMdd") && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe);
                        if (prevQ is not null)
                        {
                            if (prevQ.StageId == 2 && prevQ.Total_Cheques == null)
                            {
                                return new GeneralResponse(false, new { prevQ.Total_Cheques, prevQ.QueueId, prevQ.CategoryId, prevQ.QueueNumber }, "Input the number of cheque first");
                            }
                            prevQ.StatusId = 2;
                            if (prevQ.CategoryId == 4)
                            {
                                prevQ.ForFilling_end = DateTime.Now;
                            }
                            _unitOfWork.queueNumbers.Update(prevQ);
                        }
                        //For Filling Served End
                        var prevQForForFilling = await _unitOfWork.forFilling.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForFilling is not null)
                        {
                            prevQForForFilling.Serve_end = DateTime.Now;
                            _unitOfWork.forFilling.Update(prevQForForFilling);
                        }
                        //For Releasing Served End
                        var prevQForForReleasing = await _unitOfWork.releasing.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForReleasing is not null)
                        {
                            prevQForForReleasing.Serve_end = DateTime.Now;
                            _unitOfWork.releasing.Update(prevQForForReleasing);
                        }
                        servingData.CategoryId = (int)queueItem.CategoryId!;
                        servingData.QueueNumberServe = (int)queueItem.QueueNumber!;
                        servingData.Served_At = DateTime.Now;
                        _unitOfWork.servings.Update(servingData);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", servingData?.CategoryId, servingData?.QueueNumberServe, clerkNum);
                    }
                    else
                    {
                        var serving = new Serving
                        {
                            UserId = userId,
                            CategoryId = (int)queueItem.CategoryId!,
                            QueueNumberServe = (int)queueItem.QueueNumber!,
                            Served_At = DateTime.Now,
                        };
                        _unitOfWork.servings.Add(serving);
                        await _hubContext.Clients.All.SendAsync("CallQueueNumber", serving?.CategoryId, serving?.QueueNumberServe, clerkNum);
                    }
                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();

                    await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                    //await _hubContext.Clients.All.SendAsync("DisplayQueue");
                    await _hubContext.Clients.All.SendAsync("ServeInReservedQueue", queueItem);
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("releasingQueue");
                    await _hubContext.Clients.All.SendAsync("RecentServing");
                    await _hubContext.Clients.All.SendAsync("updateServingCard");
                    return new GeneralResponse(true, queueItem, "Served from reserve table success.");
                }
                else
                {
                    return new GeneralResponse(false, null, "There is no Queue Number.");
                }
            }
            else
            {
                return new GeneralResponse(false, null, "Your account is disabled.");
            }
        }
        //Cancel QUEUENUMBER From Table
        public async Task<GeneralResponse> CancelQueueFromTable(string generateDate, int categoryId, int qNumber, string userId)
        {
            var userAccess = await _unitOfWork.users.Get(u => u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.QueueId == generateDate && q.CategoryId == categoryId && q.QueueNumber == qNumber);

                //var servingQueue = await _unitOfWork.servings.Get(q => q.UserId == userId && q.Served_At.Date == DateTime.Today);
                var servingQueue = await _unitOfWork.servings.Get(q => q.CategoryId == categoryId && q.QueueNumberServe == qNumber && q.Served_At.Date == DateTime.Today);
                if (servingQueue != null)
                {
                    _unitOfWork.servings.Remove(servingQueue);
                }

                if (queueItem != null)
                {
                    queueItem.StatusId = 4;
                    queueItem.ClerkId = userId;
                    queueItem.Reserve_At = null;

                    if (queueItem.StageId == 1)
                    {
                        queueItem.ForFilling_start = null;
                        queueItem.ForFilling_end = null;
                        //Remove For Filling Serve 
                        await RemoveClerkServeForFilling(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }
                    else if (queueItem.StageId == 2)
                    {
                        queueItem.Releasing_start = null;
                        queueItem.Releasing_end = null;
                        //Remove For Releasing Serve 
                        await RemoveClerkServeReleasing(userId, (int)queueItem.CategoryId!, (int)queueItem.QueueNumber!);
                    }

                    //signalr method
                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("releasingQueue");
                    await _hubContext.Clients.All.SendAsync("CancelReservedQueue");
                    await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                    return new GeneralResponse(true, null, "Transfer to releasing successful.");
                }
                else
                {
                    return new GeneralResponse(true, null, "Cancel failed.");
                }
            }
            else
            {
                return new GeneralResponse(false, null, "Your account is disabled.");
            }

        }
        //Queue From Filling up to Releasing 
        public async Task<GeneralResponse> ToReleaseQueue(string generateDate, int categoryId, int qNumber, string userId)
        {
            var userAccess = await _unitOfWork.users.Get(u => u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.QueueId == generateDate && q.CategoryId == categoryId && q.QueueNumber == qNumber);

                if (queueItem != null)
                {
                    //var servingQueue = await _unitOfWork.servings.Get(q => q.UserId == userId && q.QueueNumberServe == qNumber && q.Served_At.Date == DateTime.Today);
                    var servingQueue = await _unitOfWork.servings.Get(q => q.CategoryId == categoryId && q.QueueNumberServe == qNumber && q.Served_At.Date == DateTime.Today);
                    if (servingQueue != null)
                    {
                        //For Filling Start End
                        var prevQForForFilling = await _unitOfWork.forFilling.Get(q => q.ClerkId == servingQueue.UserId && q.CategoryId == servingQueue.CategoryId && q.QueueNumber == servingQueue.QueueNumberServe && q.GenerateDate == currentDate);
                        if (prevQForForFilling is not null)
                        {
                            prevQForForFilling.Serve_end = DateTime.Now;
                            _unitOfWork.forFilling.Update(prevQForForFilling);
                        }

                        _unitOfWork.servings.Remove(servingQueue);
                    }

                    queueItem.StatusId = 2;
                    queueItem.StageId = 2;
                    queueItem.ForFilling_end = DateTime.Now;

                    _unitOfWork.queueNumbers.Update(queueItem);
                    await _unitOfWork.SaveAsync();

                    await _hubContext.Clients.All.SendAsync("DisplayQueue");
                    await _hubContext.Clients.All.SendAsync("fillingQueue");
                    await _hubContext.Clients.All.SendAsync("releasingQueue");
                    return new GeneralResponse(true, null, "Transfer to releasing successful.");
                }
                else
                {
                    return new GeneralResponse(false, null, "Cancel failed.");
                }
            }
            else
            {
                return new GeneralResponse(false, null, "Your account is disabled.");
            }
        }
        public async Task<CommonResponse> ToUpdateQueue(string generateDate, int categoryId, int qNumber, string userId, int cheque, string ipAddress)
        {
            var userAccess = await _unitOfWork.users.Get(u => u.Id == userId);
            if (userAccess.IsActive == true)
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var queueItem = await _unitOfWork.queueNumbers.Get(q => q.QueueId == generateDate && q.CategoryId == categoryId && q.QueueNumber == qNumber);

                if (queueItem == null) return new CommonResponse(false, "Please enter the total Cheque.");

                var servingData = await _unitOfWork.servings.Get(u => u.UserId == userId && u.Served_At.Date == DateTime.Today);
                if (servingData != null)
                {
                    //For Releasing Start End
                    var prevQForForReleasing = await _unitOfWork.releasing.Get(q => q.ClerkId == servingData.UserId && q.CategoryId == servingData.CategoryId && q.QueueNumber == servingData.QueueNumberServe && q.GenerateDate == currentDate);
                    if (prevQForForReleasing is not null)
                    {
                        prevQForForReleasing.Serve_end = DateTime.Now;
                        _unitOfWork.releasing.Update(prevQForForReleasing);
                    }

                    _unitOfWork.servings.Remove(servingData);
                }
                queueItem.Total_Cheques = cheque;
                queueItem.Releasing_end = DateTime.Now;
                _unitOfWork.queueNumbers.Update(queueItem);
                await _unitOfWork.SaveAsync();

                var device = await _unitOfWork.device.Get(d => d.IPAddress == ipAddress);
                var clerkNum = device?.ClerkNumber;
                await _hubContext.Clients.All.SendAsync("QueueDisplayInTvRemove", clerkNum);
                await _hubContext.Clients.Group(userId).SendAsync("DisplayQueue");
                await _hubContext.Clients.Group(userId).SendAsync("removeCheqBtn");
                await _hubContext.Clients.All.SendAsync("RecentServing");
                return new CommonResponse(true, "Cheques added success.");
            }
            else
            {
                return new CommonResponse(false, "Your account is disabled.");
            }
        }

        //HELPER
        private string GetCategoryString(int categoryId)
        {
            string category;
            switch (categoryId)
            {
                case 1:
                    category = "A";
                    break;
                case 2:
                    category = "B";
                    break;
                case 3:
                    category = "C";
                    break;
                case 4:
                    category = "D";
                    break;
                default:
                    category = "";
                    break;
            }
            return category;
        }
        private async Task UpdateClerkServeForFilling(string userId, int categoryId, int queueNumber)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var forFillingData = await _unitOfWork.forFilling.Get(f => f.ClerkId == userId && f.CategoryId == categoryId && f.QueueNumber == queueNumber && f.GenerateDate == currentDate);
            if (forFillingData != null)
            {
                forFillingData.Serve_start = DateTime.Now;
                _unitOfWork.forFilling.Update(forFillingData);
            }
            else
            {
                var fillingUp = new Serve_ForFilling
                {
                    GenerateDate = currentDate,
                    ClerkId = userId,
                    CategoryId = categoryId,
                    QueueNumber = queueNumber,
                    Serve_start = DateTime.Now,
                };
                _unitOfWork.forFilling.Add(fillingUp);
            }
        }
        private async Task UpdateClerkServeReleasing(string userId, int categoryId, int queueNumber)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var releasingData = await _unitOfWork.releasing.Get(r => r.ClerkId == userId && r.CategoryId == categoryId && r.QueueNumber == queueNumber && r.GenerateDate == currentDate);

            if (releasingData != null)
            {
                releasingData.Serve_start = DateTime.Now;
                _unitOfWork.releasing.Update(releasingData);
            }
            else
            {
                var releasing = new Serve_Releasing
                {
                    GenerateDate = currentDate,
                    ClerkId = userId,
                    CategoryId = categoryId,
                    QueueNumber = queueNumber,
                    Serve_start = DateTime.Now,
                };
                _unitOfWork.releasing.Add(releasing);
            }
        }
        private async Task RemoveClerkServeForFilling(string userId, int categoryId, int queueNumber)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var forFillingData = await _unitOfWork.forFilling.Get(r => r.CategoryId == categoryId && r.QueueNumber == queueNumber && r.GenerateDate == currentDate);

            if (forFillingData != null)
            {
                forFillingData.Serve_start = DateTime.Now;
                _unitOfWork.forFilling.Remove(forFillingData);
            }
        }
        private async Task RemoveClerkServeReleasing(string userId, int categoryId, int queueNumber)
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var releasingData = await _unitOfWork.releasing.Get(r => r.CategoryId == categoryId && r.QueueNumber == queueNumber && r.GenerateDate == currentDate);

            if (releasingData != null)
            {
                releasingData.Serve_start = DateTime.Now;
                _unitOfWork.releasing.Remove(releasingData);
            }

        }

        
    }
}
