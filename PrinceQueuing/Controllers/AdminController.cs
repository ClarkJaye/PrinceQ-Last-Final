using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrinceQ.DataAccess.Extensions;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.DTOs;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using PrinceQ.Utility;
using System.Data;
using System.Linq;

namespace PrinceQueuing.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminController> _logger;
        private readonly IHubContext<QueueHub> _hubContext;

        private readonly IAdmin _admin;

        public AdminController(IAdmin admin, IUnitOfWork unitOfWork, ILogger<AdminController> logger, IHubContext<QueueHub> hubContext)
        {
            _admin = admin;

            _unitOfWork = unitOfWork;
            _logger = logger;
            _hubContext = hubContext;
        }

        //-----DASHBOARD-----//
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetDataByYearAndMonth(string year, string month)
        {
            try
            {
                if (year != null && month == null)
                {
                    // Retrieve monthly data
                    var data = await _unitOfWork.queueNumbers.GetAll(d => d.StageId != null && d.QueueId!.Substring(0, 4) == year);
                    var monthlyData = data
                        .GroupBy(item => item.QueueId!.Substring(4, 2))
                        .Select(g => new
                        {
                            Month = g.Key,
                            CategoryASum = g.Sum(i => i.CategoryId == 1 ? 1 : 0),
                            CategoryBSum = g.Sum(i => i.CategoryId == 2 ? 1 : 0),
                            CategoryCSum = g.Sum(i => i.CategoryId == 3 ? 1 : 0),
                            CategoryDSum = g.Sum(i => i.CategoryId == 4 ? 1 : 0),
                            GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                        })
                        .ToList();
                    return Json(new { IsMonth = true, value = monthlyData });
                }
                else if (year != null && month != null)
                {
                    var data = await _unitOfWork.queueNumbers.GetAll(d => d.StageId != null && d.QueueId!.Substring(0, 6) == year + month);
                    var dailyData = data
                        .GroupBy(item => item.QueueId!.Substring(6, 2))
                        .Select(g => new
                        {
                            Day = g.Key,
                            CategoryASum = g.Sum(i => i.CategoryId == 1 ? 1 : 0),
                            CategoryBSum = g.Sum(i => i.CategoryId == 2 ? 1 : 0),
                            CategoryCSum = g.Sum(i => i.CategoryId == 3 ? 1 : 0),
                            CategoryDSum = g.Sum(i => i.CategoryId == 4 ? 1 : 0),
                            GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                        })
                        .ToList();

                    return Json(new { IsMonth = false, value = dailyData });
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDataByYearAndMonth action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetDataByYearAndMonth." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TotalQueueNumber()
        {
            try
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate);

                var totalGenerate = data.Count();

                if (totalGenerate <= 0) return Json(new { IsSuccess = false, value = totalGenerate });

                return Json(new { IsSuccess = true, value = totalGenerate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQueueServed()
        {
            try
            {
                // Fetch servings for the current day
                var servings = await _unitOfWork.servings.GetAll(g => g.Served_At.Date == DateTime.Now.Date);

                if (servings == null || !servings.Any())
                    return Json(new { IsSuccess = false, Message = "No data available." });

                // Get relevant queue numbers and users for these servings
                var queueNumbers = await _unitOfWork.queueNumbers.GetAll();
                var userIds = servings.Select(s => s.UserId).Distinct();
                var users = await _unitOfWork.users.GetAll(u => userIds.Contains(u.Id));

                // Prepare result data
                var result = servings.Select(serving =>
                {
                    var user = users.FirstOrDefault(u => u.Id == serving.UserId);
                    var queueNumber = queueNumbers.FirstOrDefault(q =>
                        q.QueueId == serving.Served_At.ToString("yyyyMMdd") &&
                        q.CategoryId == serving.CategoryId &&
                        q.QueueNumber == serving.QueueNumberServe);

                    return new
                    {
                        category = serving.CategoryId,
                        qNumber = serving.QueueNumberServe,
                        servingBy = user?.UserName ?? "Unknown",
                        stage = queueNumber?.StageId,
                        dateTime = serving.Served_At.ToString("MM/dd/yyyy, hh:mm:ss tt")
                    };
                }).ToList();

                return Json(new { IsSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQueueServed action");
                return Json(new { IsSuccess = false, Message = "An error occurred in GetQueueServed." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> totalServed()
        {
            try
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StageId == 2);

                var totalQ = data.Count();

                if (totalQ <= 0) return Json(new { IsSuccess = false, value = totalQ });

                return Json(new { IsSuccess = true, value = totalQ });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> RecentlyServed()
        {
            try
            {
                var currentDate = DateTime.Today.ToString("yyyyMMdd");
                var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StageId == 2);

                var totalQ = data.Count();

                if (totalQ <= 0) return Json(new { IsSuccess = false, value = totalQ });

                return Json(new { IsSuccess = true, value = totalQ });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }


        //-----USERS-----//
        [Authorize(Roles = SD.Role_Users)]
        public async Task<IActionResult> Users()
        {
            var response = await _admin.UserPage();
            if (response.IsSuccess)
            {
                return View(response.Obj);
            }
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public async Task<IActionResult> GetUser(string? id)
        {
            try
            {
                var response = await _admin.UserDetail(id);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetUser." });
            }
        }      
        [HttpDelete]
        public async Task<IActionResult> RemoveUser(string? id)
        {
            try
            {
                var response = await _admin.DeleteUser(id);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetUser." });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var userId = User.GetUserId();
                var response = await _admin.AllUsers(userId);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllUsers action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllUsers." });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var response = await _admin.AllRoles();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllRoles action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllRoles." });
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddUser(UserVM model, string roles)
        {
            try
            {
                var rolesArray = string.IsNullOrEmpty(roles) ? new string[] { } : roles.Split(',');

                ModelState.Remove("roles");

                if (ModelState.IsValid)
                {
                    var response = await _admin.AddUser(model, rolesArray);
                    return Json(response);
                }
                return Json(new { IsSuccess = false, Message = "Add user failed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddUsers action");
                return Json(new { IsSuccess = false, Message = "An error occurred in AddUsers." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser(UserVM model, string roles)
        {
            try
            {
                var rolesArray = string.IsNullOrEmpty(roles) ? new string[] { } : roles.Split(',');

                ModelState.Remove("roles");

                if (ModelState.IsValid)
                {
                    var response = await _admin.UpdateUser(model, rolesArray);
                    return Json(response);
                }

                return Json(new { IsSuccess = false, Message = "Invalid data. Please check the form inputs." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUser action");
                return Json(new { IsSuccess = false, Message = "An error occurred while updating the user." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> UserCategories(string? userId)
        {
            var response = await _admin.UserCategory(userId);
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetAssignCategories(string? userId)
        {
            var response = await _admin.GetAssignCategory(userId);
            return Json(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddAssignUserCategories(int[] categoryId, string userId)
        {
            var response = await _admin.AddAssignUserCategories(categoryId, userId);
            return Json(response);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveAssignUserCategories(int categoryId, string userId)
        {
            var response = await _admin.RemoveAssignUserCategories(categoryId, userId);
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetUserCounts()
        {
            try
            {
                int totalUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd");
                int activeUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd" && u.IsActive == true);
                int inactiveUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd" && u.IsActive == false);

                var response = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = inactiveUsers
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserCounts action");
                return StatusCode(StatusCodes.Status500InternalServerError, new { IsSuccess = false, Message = "An error occurred while fetching user counts." });
            }
        }


        //-----MANAGE VIDEO-----//
        [Authorize(Roles = SD.Role_Videos)]
        public IActionResult ManageVideo()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult AllVideos()
        {
            var response = _admin.AllVideos();
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> PlayVideo(string videoName)
        {
            var response = await _admin.PlayVideo(videoName);
            return Json(response);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteVideo(string videoName)
        {
            var response = await _admin.DeleteVideo(videoName);
            return Json(response);
        }
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadVideo(IFormFile videoFile)
        {
            var response = await _admin.UploadVideo(videoFile);
            return Json(response);
        }


        //-----SERVING RESPORT-----//
        [Authorize(Roles = SD.Role_Reports)]
        public IActionResult ServingReport()
        {
            return View();
        }

        [Authorize(Roles = SD.Role_Reports)]
        public IActionResult ServingReport_Details()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Serving_GetAllServedData()
        {
            try
            {
                var forFillingData = await _unitOfWork.forFilling.GetAll();
                var releasingData = await _unitOfWork.releasing.GetAll();
                var users = await _unitOfWork.users.GetAll();

                // Create a dictionary for quick username lookup
                var userDictionary = users.ToDictionary(u => u.Id, u => u.UserName);

                var forFillingMapped = forFillingData
                    .Where(f => f.Serve_start.HasValue && f.Serve_end.HasValue)
                    .Select(f => new ServeDataDTO
                    {
                        GenerateDate = f.GenerateDate,
                        ClerkId = f.ClerkId,
                        CategoryId = (int)f.CategoryId,
                        QueueNumber = (int)f.QueueNumber,
                        ServeStart = f.Serve_start.HasValue ? TimeSpan.Parse(f.Serve_start.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                        ServeEnd = f.Serve_end.HasValue ? TimeSpan.Parse(f.Serve_end.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero
                    });

                var releasingMapped = releasingData
                    .Where(r => r.Serve_start.HasValue && r.Serve_end.HasValue)
                    .Select(r => new ServeDataDTO
                    {
                        GenerateDate = r.GenerateDate,
                        ClerkId = r.ClerkId,
                        CategoryId = (int)r.CategoryId,
                        QueueNumber = (int)r.QueueNumber,
                        ServeStart = r.Serve_start.HasValue ? TimeSpan.Parse(r.Serve_start.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                        ServeEnd = r.Serve_end.HasValue ? TimeSpan.Parse(r.Serve_end.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero
                    });

                var combinedData = forFillingMapped.Concat(releasingMapped)
                                    .GroupBy(x => new { x.ClerkId, x.GenerateDate })
                                    .Select(g => new
                                    {
                                        user = g.Key,
                                        Username = userDictionary.ContainsKey(g.Key.ClerkId) ? userDictionary[g.Key.ClerkId] : "Unknown",
                                        TotalNumberServed = g.Count(),
                                        LongestServedTime = g.Max(x => (x.ServeEnd - x.ServeStart).TotalSeconds),
                                        ShortestServedTime = g.Min(x => (x.ServeEnd - x.ServeStart).TotalSeconds),
                                        Date = g.Key.GenerateDate
                                    });

                return Json(new { IsSuccess = true, data = combinedData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Serving_GetAllServedData action");
                return Json(new { IsSuccess = false, message = "An error occurred in Serving_GetAllServedData." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailedData(string clerkId, string generateDate)
        {
            try
            {
                // Fetch forFilling and releasing data
                var forFillingData = await _unitOfWork.forFilling.GetAll();
                var releasingData = await _unitOfWork.releasing.GetAll();

                var queuesData = await _unitOfWork.queueNumbers.GetAll();

                var users = await _unitOfWork.users.GetAll();
                var userDictionary = users.ToDictionary(u => u.Id, u => u.UserName);

                var forFillingMapped = forFillingData
                    .Select(f => new ServeDataDTO
                {
                    GenerateDate = f.GenerateDate,
                    ClerkId = f.ClerkId,
                    CategoryId = (int)f.CategoryId,
                    QueueNumber = (int)f.QueueNumber,
                    Total_Cheque = queuesData.FirstOrDefault(q => q.QueueId == f.GenerateDate && q.CategoryId == f.CategoryId && q.QueueNumber == f.QueueNumber)?.Total_Cheques ?? 0,
                    ServeStart = f.Serve_start.HasValue ? TimeSpan.Parse(f.Serve_start.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                    ServeEnd = f.Serve_end.HasValue ? TimeSpan.Parse(f.Serve_end.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                    stageId =  1,
                });

                var releasingMapped = releasingData.Select(r => new ServeDataDTO
                {
                    GenerateDate = r.GenerateDate,
                    ClerkId = r.ClerkId,
                    CategoryId = (int)r.CategoryId,
                    QueueNumber = (int)r.QueueNumber,
                    Total_Cheque = queuesData.FirstOrDefault(q => q.QueueNumber == r.QueueNumber)?.Total_Cheques ?? 0,
                    ServeStart = r.Serve_start.HasValue ? TimeSpan.Parse(r.Serve_start.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                    ServeEnd = r.Serve_end.HasValue ? TimeSpan.Parse(r.Serve_end.Value.ToString("hh\\:mm\\:ss")) : TimeSpan.Zero,
                    stageId = 2,
                });

                // Combine data from both sources and filter by ClerkId and GenerateDate
                var combinedData = forFillingMapped.Concat(releasingMapped)
                                      .Where(x => x.ClerkId == clerkId && x.GenerateDate == generateDate)
                                      .Select(x => new
                                      {
                                          GenerateDate = x.GenerateDate,
                                          Username = userDictionary.ContainsKey(x.ClerkId) ? userDictionary[x.ClerkId] : "Unknown",
                                          CategoryId = x.CategoryId,
                                          QueueNumber = x.QueueNumber,
                                          Total_Cheque = x.Total_Cheque,
                                          ServeStart = x.ServeStart,
                                          ServeEnd = x.ServeEnd,
                                          StageId = x.stageId,
                                      });

                return Json(new { IsSuccess = true, data = combinedData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailedData action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetDetailedData." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetClerks_Categories()
        {
            try
            {
                var categ = await _unitOfWork.category.GetAll();
                var categories = categ.OrderBy(c => c.Created_At).ToList();
                var users = await _unitOfWork.users.GetAll(u=> u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd");
                var ListUsers = new List<dynamic>();

                foreach (var user in users)
                {

                    ListUsers.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        user.Created_At,
                        user.IsActive,
                    });
                }

                if (ListUsers == null) return Json(new { IsSuccess = false, Message = "Retrieved clerks failed." });

                // Sort the clerks based on their Created_At
                var sortedUsers = ListUsers.OrderBy(c => c.Created_At).ToList();

                return Json(new { IsSuccess = true, users = sortedUsers, categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllClerks action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllClerks." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetServingDataClerk(string clerkId, string year, string month)
        {
            try
            {
                if (clerkId != null && year != null && month == null)
                {
                    var data = await _unitOfWork.forFilling.GetAll(d => d.ClerkId == clerkId && d.GenerateDate!.Substring(0, 4) == year);
                    var monthlyData = data
                        .GroupBy(item => item.GenerateDate!.Substring(4, 2))
                        .Select(g => new
                        {
                            Month = g.Key,
                            CategoryASum = g.Count(i => i.CategoryId == 1),
                            CategoryBSum = g.Count(i => i.CategoryId == 2),
                            CategoryCSum = g.Count(i => i.CategoryId == 3),
                            CategoryDSum = g.Count(i => i.CategoryId == 4),
                        })
                        .ToList();
                    return Json(new { ByMonth = true, value = monthlyData });
                }
                else if (clerkId == null && year != null && month == null)
                {
                    var data = await _unitOfWork.forFilling.GetAll(d => d.GenerateDate!.Substring(0, 4) == year);
                    var monthlyData = data
                        .GroupBy(item => item.GenerateDate!.Substring(4, 2))
                        .Select(g => new
                        {
                            Month = g.Key,
                            CategoryASum = g.Count(i => i.CategoryId == 1),
                            CategoryBSum = g.Count(i => i.CategoryId == 2),
                            CategoryCSum = g.Count(i => i.CategoryId == 3),
                            CategoryDSum = g.Count(i => i.CategoryId == 4),
                        })
                        .ToList();
                    return Json(new { ByMonth = true, value = monthlyData });
                }
                else if (clerkId != null && year != null && month != null)
                {
                    var data = await _unitOfWork.forFilling.GetAll(d => d.ClerkId == clerkId && d.GenerateDate!.Substring(0, 6) == year + month);
                    var dailyData = data
                        .GroupBy(item => item.GenerateDate!.Substring(6, 2))
                        .Select(g => new
                        {
                            Day = g.Key,
                            CategoryASum = g.Count(i => i.CategoryId == 1),
                            CategoryBSum = g.Count(i => i.CategoryId == 2),
                            CategoryCSum = g.Count(i => i.CategoryId == 3),
                            CategoryDSum = g.Count(i => i.CategoryId == 4),
                            GenerateDate = g.Select(i => i.GenerateDate).FirstOrDefault()
                        })
                        .ToList();
                    return Json(new { ByMonth = false, value = dailyData });
                }
                else if (clerkId == null && year != null && month != null)
                {
                    // example data of generateDate = "20240626"
                    var data = await _unitOfWork.forFilling.GetAll(d => d.GenerateDate!.Substring(0, 6) == year + month);
                    var dailyData = data
                        .GroupBy(item => item.GenerateDate!.Substring(6, 2))
                        .Select(g => new
                        {
                            Day = g.Key,
                            CategoryASum = g.Count(i => i.CategoryId == 1),
                            CategoryBSum = g.Count(i => i.CategoryId == 2),
                            CategoryCSum = g.Count(i => i.CategoryId == 3),
                            CategoryDSum = g.Count(i => i.CategoryId == 4),
                            GenerateDate = g.Select(i => i.GenerateDate).FirstOrDefault()
                        })
                        .ToList();

                    return Json(new { ByMonth = false, value = dailyData });

                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServingDataClerk action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetServingDataClerk." });
            }
        }




        //-----Waiting time RESPORT-----//
        [Authorize(Roles = SD.Role_Reports)]
        public IActionResult WaitingReport()
        {
            return View();
        }
        //-----Waiting time RESPORT-----//
        [Authorize(Roles = SD.Role_Reports)]
        public IActionResult WaitingReport_Details()
        {
            return View();
        }
        public async Task<IActionResult> GetAllWaitingTimeData(string range, string year, string month)
        {
            try
            {
                if (range != null && range == "gf")
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 5 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.Generate_At.HasValue &&
                            d.ForFilling_start.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryDAvg = g.Where(i => i.CategoryId == 4).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 4).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 5 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.Generate_At.HasValue &&
                           d.ForFilling_start.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryDAvg = g.Where(i => i.CategoryId == 4).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 4).Average(i => (i.ForFilling_start!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                            })
                            .ToList();

                        return Json(new { ByMonth = false, value = dailyData });
                    }
                    else
                    {
                        return Json(null);
                    }
                }
                else if (range != null && range == "fr")
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 5 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.ForFilling_end.HasValue &&
                            d.Releasing_end.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 5 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.ForFilling_start.HasValue &&
                           d.ForFilling_end.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.ForFilling_end!.Value).TotalSeconds)) : "00",
                                GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                            })
                            .ToList();

                        return Json(new { ByMonth = false, value = dailyData });
                    }
                    else
                    {
                        return Json(null);
                    }
                }
                else
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 5 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.Generate_At.HasValue &&
                            d.Releasing_end.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 5 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.ForFilling_start.HasValue &&
                           d.Releasing_end.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.Generate_At!.Value).TotalSeconds)) : "00",
                                GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                            })
                            .ToList();

                        return Json(new { ByMonth = false, value = dailyData });
                    }
                    else
                    {
                        return Json(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetUser." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Waiting_GetAllServedData()
        {
            try
            {
                var data = await _unitOfWork.queueNumbers.GetAll();

                var result = data.Select(q => new
                {
                    GenerateDate = q.QueueId,
                    CategoryId = q.CategoryId,
                    QueueNumber = q.QueueNumber,
                    GeneratedStart = q.Generate_At.HasValue ? q.Generate_At.Value.ToString("hh:mm tt") : "N/A",
                    CallForFilling = q.ForFilling_start.HasValue ? q.ForFilling_start.Value.ToString("hh:mm tt") : "N/A",
                    CallForReleasing = q.Releasing_start.HasValue ? q.Releasing_start.Value.ToString("hh:mm tt") : "N/A",

                    CallForFilling_Reserved = q.ForFilling_start_Backup.HasValue ? q.ForFilling_start_Backup.Value.ToString("hh:mm tt") : "N/A",
                    CallForReleasing_Reserved = q.Releasing_start_Backup.HasValue ? q.Releasing_start_Backup.Value.ToString("hh:mm tt") : "N/A",

                    ReleasingEnd = q.Releasing_end.HasValue ? q.Releasing_end.Value.ToString("hh:mm tt") : "N/A",
                    AverageTime = CalculateAverageTime(q)
                });

                return Json(new { IsSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Waiting_GetAllServedData action");
                return Json(new { IsSuccess = false, message = "An error occurred in Waiting_GetAllServedData." });
            }
        }

        private string CalculateAverageTime(Queues q)
        {
            var forFillingStart = q.ForFilling_start_Backup ?? q.ForFilling_start;
            var releasingStart = q.Releasing_start_Backup ?? q.Releasing_start;

            if (q.Releasing_end.HasValue && forFillingStart.HasValue && releasingStart.HasValue && q.Generate_At.HasValue)
            {
                var one = q.ForFilling_start.Value - q.Generate_At.Value;
                var two = q.ForFilling_end.HasValue ? q.ForFilling_end.Value - forFillingStart.Value : TimeSpan.Zero;
                var three = q.Releasing_start.HasValue ? q.Releasing_start.Value - q.ForFilling_end.Value : TimeSpan.Zero;
                var four = q.Releasing_end.Value - releasingStart.Value;

                double totalSeconds = (one + two + three + four).TotalSeconds;

                return waiting_FormatSeconds(totalSeconds);
            }
            else
            {
                return "N/A";
            }
        }
        private string waiting_FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalMinutes >= 60)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            }
            else
            {
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            }
        }
        private string FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalMinutes >= 60)
            {
                return $"{(int)timeSpan.TotalHours}.{timeSpan.Minutes}";
            }
            else
            {
                return $"{(int)timeSpan.TotalMinutes}.{timeSpan.Seconds}";
            }
        }






        //-----Announcement-----//
        [Authorize(Roles = SD.Role_Announcement)]
        public IActionResult Announcement()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAnnouncement(int? id)
        {
            try
            {
                var response = await _admin.AnnouncementDetail(id);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAnnouncement action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAnnouncement." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAnnouncement()
        {
            try
            {
                var response = await _admin.AllAnnouncement();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAnnouncement action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllAnnouncement." });
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddAnnounce(Announcement model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _admin.AddAnnouncement(model);
                    return Json(response);
                }
                return Json(new { IsSuccess = false, Message = "Add Announcement failed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Announcement action");
                return Json(new { IsSuccess = false, Message = "An error occurred in Announcement." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAnnouncement(Announcement model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _admin.UpdateAnnouncement(model);
                    return Json(response);
                }
                return Json(new { IsSuccess = false, Message = "updated Announcement failed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetUser." });
            }

        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAnnouncement(int? id)
        {
            try
            {
                var response = await _admin.DeleteAnnouncement(id);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAnnouncement action");
                return Json(new { IsSuccess = false, message = "An error occurred in DeleteAnnouncement." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTotalAnnounce()
        {
            try
            {
                var response = await _admin.GetTotalAnnounce();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAnnouncement action");
                return Json(new { IsSuccess = false, message = "An error occurred in DeleteAnnouncement." });
            }
        }



    }


}