using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrinceQ.DataAccess.Extensions;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using PrinceQ.Utility;

namespace PrinceQueuing.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdmin _admin;


        public AdminController(IAdmin admin, ILogger<AdminController> logger)
        {
            _admin = admin;
            _logger = logger;
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
                var response = await _admin.GetDataByYearAndMonth(year, month);
                return Json(response);
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
                var response = await _admin.TotalQueueNumberPerDay();
                return Json(response);
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
                var response = await _admin.GetQueueServed();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQueueServed action");
                return Json(new { IsSuccess = false, Message = "An error occurred in GetQueueServed." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TotalServed()
        {
            try
            {
                var response = await _admin.TotalServed();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> totalWaitingNumber()
        {
            try
            {
                var response = await _admin.TotalWaitingNumberPerDay();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TotalReservedNumber()
        {
            try
            {
                var response = await _admin.TotalReservedNumberPerDay();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HighestServePerDay action");
                return Json(new { IsSuccess = false, Message = "An error occurred in HighestServePerDay." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TotalCancelNumber()
        {
            try
            {
                var response = await _admin.TotalCancelNumberPerDay();
                return Json(response);
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
                var response = await _admin.RecentlyServed();
                return Json(response);
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
                var response = await _admin.totalUserCount();
                return Json(response);
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
                var response = await _admin.Serving_GetAllServedData();
                return Json(response);
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
                var response = await _admin.GetDetailedData(clerkId, generateDate);
                return Json(response);
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
                var response = await _admin.GetClerks_Categories();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllClerks action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllClerks." });
            }
        }
        //[HttpGet]
        public async Task<IActionResult> GetServingDataClerk(string clerkId, string year, string month)
        {
            try
            {
                var response = await _admin.GetServingDataClerk(clerkId, year, month);
                return Json(response);         
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
        [Authorize(Roles = SD.Role_Reports)]
        public IActionResult WaitingReport_Details()
        {
            return View();
        }
        public async Task<IActionResult> GetAllWaitingTimeData(string year, string month)
        {
            try
            {
                var response = await _admin.GetAllWaitingTimeData(year, month);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllWaitingTimeData action");
                return Json(new { IsSuccess = false, message = "An error occurred in GetAllWaitingTimeData." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Waiting_GetAllServedData()
        {
            try
            {
                var response = await _admin.Waiting_GetAllServedData();
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Waiting_GetAllServedData action");
                return Json(new { IsSuccess = false, message = "An error occurred in Waiting_GetAllServedData." });
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
                    var addedBy = User.GetUsername();
                    var response = await _admin.AddAnnouncement(model, addedBy);
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