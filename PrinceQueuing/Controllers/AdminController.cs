using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrinceQ.DataAccess.Extensions;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using PrinceQ.Utility;
using System.Data;

namespace PrinceQueuing.Controllers
{
    [Authorize(Policy = SD.Policy_Staff_Admin)]
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

        //[HttpGet]
        //public async Task<IActionResult> HighestServePerDay()
        //{
        //    try
        //    {
        //        var year = DateTime.Now.Year.ToString();
        //        var data = await _unitOfWork.queueNumbers.GetAll(d => d.StageId != null && d.QueueId!.Substring(0, 4) == year);

        //        var highestServed = data
        //            .GroupBy(item => item.QueueId!.Substring(0, 8)) // Group by year-month-day (YYYYMMDD)
        //            .Select(g => new
        //            {
        //                Date = g.Key,
        //                CategoryASum = g.Count(i => i.CategoryId == 1),
        //                CategoryBSum = g.Count(i => i.CategoryId == 2),
        //                CategoryCSum = g.Count(i => i.CategoryId == 3),
        //                CategoryDSum = g.Count(i => i.CategoryId == 4)
        //            })
        //            .Select(g => new
        //            {
        //                g.Date,
        //                Category = new[]
        //                {
        //            new { Name = "Category A", Sum = g.CategoryASum },
        //            new { Name = "Category B", Sum = g.CategoryBSum },
        //            new { Name = "Category C", Sum = g.CategoryCSum },
        //            new { Name = "Category D", Sum = g.CategoryDSum }
        //                }.OrderByDescending(c => c.Sum).FirstOrDefault()
        //            })
        //            .OrderByDescending(x => x.Category!.Sum) 
        //            .FirstOrDefault();

        //        if(highestServed == null) return Json(new { IsSuccess = false, value = highestServed });

        //        return Json(new { IsSuccess = true, value = highestServed });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in HighestServePerDay action");
        //        return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
        //    }
        //}




        //-----USERS-----//
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
                int totalUsers = await _unitOfWork.users.Count();
                int activeUsers = await _unitOfWork.users.Count(u => u.IsActiveId == 1);
                int inactiveUsers = await _unitOfWork.users.Count(u => u.IsActiveId == 2);

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
        public IActionResult ServingReport()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetClerks_Categories()
        {
            try
            {
                var categ = await _unitOfWork.category.GetAll();
                var categories = categ.OrderBy(c => c.Created_At).ToList();
                var users = await _unitOfWork.users.GetAll();
                var clerks = new List<dynamic>();

                foreach (var user in users)
                {
                    var roles = await _unitOfWork.auth.GetUserRolesAsync(user);
                    if (roles.Contains("Clerk") || roles.Contains("clerk"))
                    {
                        clerks.Add(new
                        {
                            user.Id,
                            user.UserName,
                            user.Email,
                            user.PhoneNumber,
                            user.Created_At,
                            user.IsActiveId,
                            Roles = roles
                        });
                    }
                }

                if (clerks == null) return Json(new { IsSuccess = false, Message = "Retrieved clerks failed." });

                // Sort the clerks based on their Created_At
                var sortedClerks = clerks.OrderBy(c => c.Created_At).ToList();

                return Json(new { IsSuccess = true, clerks = sortedClerks, categories });
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
                    var data = await _unitOfWork.clerkForFilling.GetAll(d => d.ClerkId == clerkId && d.GenerateDate!.Substring(0, 4) == year);
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
                    var data = await _unitOfWork.clerkForFilling.GetAll(d => d.GenerateDate!.Substring(0, 4) == year);
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
                    var data = await _unitOfWork.clerkForFilling.GetAll(d => d.ClerkId == clerkId && d.GenerateDate!.Substring(0, 6) == year + month);
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
                    var data = await _unitOfWork.clerkForFilling.GetAll(d => d.GenerateDate!.Substring(0, 6) == year + month);
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
        public IActionResult WaitingReport()
        {
            return View();
        }
        public async Task<IActionResult> GetAllWaitingTimeData(string range, string year, string month)
        {
            try
            {
                if (range is null || range == "fr")
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 2 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.ForFilling_start.HasValue &&
                            d.ForFilling_end.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 2 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.ForFilling_start.HasValue &&
                           d.ForFilling_end.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
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
                else if (range != null && range == "ff")
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 2 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.ForFilling_start.HasValue &&
                            d.ForFilling_end.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryDAvg = g.Where(i => i.CategoryId == 4).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 4).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 2 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.ForFilling_start.HasValue &&
                           d.ForFilling_end.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
                                CategoryDAvg = g.Where(i => i.CategoryId == 4).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 4).Average(i => (i.ForFilling_end!.Value - i.ForFilling_start!.Value).TotalSeconds)) : "00",
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
                else if (range != null && range == "rr")
                {
                    if (year != null && month == null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                            d.StatusId == 2 &&
                            d.QueueId!.Substring(0, 4) == year &&
                            d.ForFilling_start.HasValue &&
                            d.ForFilling_end.HasValue);

                        var monthlyData = data
                            .GroupBy(item => item.QueueId!.Substring(4, 2))
                            .Select(g => new
                            {
                                Month = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
                            })
                            .ToList();

                        return Json(new { ByMonth = true, value = monthlyData });
                    }
                    else if (year != null && month != null)
                    {
                        var data = await _unitOfWork.queueNumbers.GetAll(d =>
                           d.StatusId == 2 &&
                           d.QueueId!.Substring(0, 6) == year + month &&
                           d.ForFilling_start.HasValue &&
                           d.ForFilling_end.HasValue);

                        var dailyData = data
                            .GroupBy(item => item.QueueId!.Substring(6, 2))
                            .Select(g => new
                            {
                                Day = g.Key,
                                CategoryAAvg = g.Where(i => i.CategoryId == 1).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 1).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
                                CategoryBAvg = g.Where(i => i.CategoryId == 2).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 2).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
                                CategoryCAvg = g.Where(i => i.CategoryId == 3).Any() ? FormatSeconds(g.Where(i => i.CategoryId == 3).Average(i => (i.Releasing_end!.Value - i.Releasing_start!.Value).TotalSeconds)) : "00",
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
                    return Json(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUser action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetUser." });
            }
        }

        private string FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)timeSpan.TotalMinutes}.{timeSpan.Seconds}";
        }

        //-----Announcement-----//
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