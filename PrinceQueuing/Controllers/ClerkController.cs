using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.Utility;
using PrinceQueuing.Extensions;
using System.Net;

namespace PrinceQueuing.Controllers
{
    [Authorize(Policy = SD.Policy_Staff_Admin)]
    public class ClerkController : Controller
    {
        private readonly IClerk _clerk;
        private readonly ILogger<ClerkController> _logger;

        public ClerkController(IClerk clerk, ILogger<ClerkController> logger)
        {
            _clerk = clerk;
            _logger = logger;
        }

        //--------------- Views  -------------//
        public async Task<IActionResult> Serving()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress(); 
                var response = await _clerk.ServingVM(userId, ipAddress);

                if (response.IsSuccess)
                {
                    return View(response.Obj);
                }
                return RedirectToAction("Login", "Account");
                //return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NextQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred in NextQueue." });
            }
        }

        public async Task<IActionResult> Filling()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress();
                var response = await _clerk.ServingVM(userId, ipAddress);

                if (response.IsSuccess)
                {
                    return View(response.Obj);
                }
                return RedirectToAction("Login", "Account");
                //return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NextQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred in NextQueue." });
            }
        }

        public async Task<IActionResult> Releasing()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress();
                var response = await _clerk.ServingVM(userId, ipAddress);

                if (response.IsSuccess)
                {
                    return View(response.Obj);
                }
                return RedirectToAction("Login", "Account");
                //return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NextQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred in NextQueue." });
            }
        }


        //Dashboard
        public async Task<IActionResult> Generate()
        {
            try
            {
                var response = await _clerk.GenerateVM();

                if (response.IsSuccess)
                {
                    return View(response.Obj);
                }
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home action");
                return Json(new { IsSuccess = false, Message = "There is an error!" });
            }
        }




        //--------------- Functionalities -------------//

        // ------------ PRINTT ----------- //

        //FOr Printing the Queue
        public async Task<IActionResult> Print_Form(string date, int categoryId, int queueNumber)
        {
            try
            {
                var response = await _clerk.GetQueue(date, categoryId, queueNumber);

                if (response.IsSuccess)
                {
                    return View(response.Obj);
                }
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Print_Form action");
                return Json(new { IsSuccess = false, Message = "There is an error!" });
            }

        }

        //Process
        public async Task<IActionResult> Print_QueueNumber(string date, int categoryId, int queueNumber)
        {
            try
            {
                var response = await _clerk.GetQueue(date, categoryId, queueNumber);

                if (response.IsSuccess)
                {
                    return Json(response);
                }
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Print_QueueNumber action");
                return Json(new { IsSuccess = false, Message = "There is an error!" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> GenerateQueueNumber(int categoryId)
        {
            try
            {
                var response = await _clerk.GenerateQueue(categoryId);
                if (response.IsSuccess)
                {
                    return Json(response);
                }
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateQueueNumber action");
                return Json(new { IsSuccess = false, Message = "There is an error!" });
            }
        }



        //Get the designated Clerk Number
        public async Task<IActionResult> DesignatedDeviceId()
        {
            var ipAddress = GetUserIpAddress();
            var userId = GetCurrentUserId();
            var response = await _clerk.DesignatedClerk(ipAddress, userId);

            return Json(response);
        }


        //Get All Categories
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _clerk.GetAllWaitingQueue(userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategories action");
                return Json(new { IsSuccess = false, message = "An error occurred while fetching the categories." });
            }
        }

        //Get Clerk Serving
        public async Task<IActionResult> GetServings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress();
                var response = await _clerk.GetServings(userId, ipAddress);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetServings action");
                return Json(new { IsSuccess = false, message = "An error occurred while fetching the GetServings." });
            }
        }

        //Get All Reserve Queue 
        public async Task<IActionResult> GetReservedQueues()
        {
            try
            {
                // Get the userId of the current user
                var userId = GetCurrentUserId();
                var response = await _clerk.GetReservedQueues(userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReservedQueues action");
                return Json(new { IsSuccess = false, message = "An error occurred while fetching the GetReservedQueues." });
            }
        }

        //Get All Filling Queue 
        public async Task<IActionResult> GetAllFillingUpQueues()
        {
            try
            {
                // Get the userId of the current user
                var userId = GetCurrentUserId();
                var response = await _clerk.GetFillingUpQueues(userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReservedQueues action");
                return Json(new { IsSuccess = false, message = "An error occurred while fetching the GetReservedQueues." });
            }
        }
        
        //Get All Releasing Queue 
        public async Task<IActionResult> GetAllReleasingQueues()
        {
            try
            {
                // Get the userId of the current user
                var userId = GetCurrentUserId();
                var response = await _clerk.GetReleasingQueues(userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReservedQueues action");
                return Json(new { IsSuccess = false, message = "An error occurred while fetching the GetReservedQueues." });
            }
        }


        //NEXT QUEUE
        public async Task<IActionResult> NextQueue(int id)
        {
            try
            {
                //User ID
                var userId = GetCurrentUserId();
                var response = await _clerk.NextQueueNumber(id, userId);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NextQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred in NextQueue." });
            }
        }

        //RESERVE QUEUE
        public async Task<IActionResult> ReserveQueue()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress();
                var response = await _clerk.ReserveQueueNumber(userId, ipAddress);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReserveQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred while processing the ReserveQueue." });
            }
        }


        //CANCEL QUEUE
        public async Task<IActionResult> CancelQueue()
        {
            try
            {
                var userId = GetCurrentUserId();
                var ipAddress = GetUserIpAddress();
                var response = await _clerk.CancelQueueNumber(userId, ipAddress);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CancelQueue action");
                return Json(new { IsSuccess = false, message = "An error occurred in CancelQueue." });
            }
        }


        ////// -------------QUEUE RESERVED-------------- //////


        //Serve IN RESERVE QUEUE
        public async Task<JsonResult> ServeQueueFromTable(string generateDate, int categoryId, int qNumber )
        {
            try
            {
                //User ID
                var userId = GetCurrentUserId();
                var response = await _clerk.ServeQueueFromTable(generateDate, categoryId, qNumber, userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ServeInReserve action");
                return Json(new { IsSuccess = false, message = "An error occurred while processing the ServeInReserve." });
            }
        }


        //CANCEL IN RESERVE QUEUE
        public async Task<JsonResult> CancelQueueNumber(string generateDate, int categoryId, int qNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _clerk.CancelQueueFromTable(generateDate, categoryId, qNumber, userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReserveCancel action");
                return Json(new { IsSuccess = false, message = "An error occurred in ReserveCancel." });
            }
        }



        //QUEUE FROM FILLING TRANSFER TO RELEASING TABLE
        public async Task<JsonResult> FillingToReleaseQueue(string generateDate, int categoryId, int qNumber)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _clerk.ToReleaseQueue(generateDate, categoryId, qNumber, userId);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReserveCancel action");
                return Json(new { IsSuccess = false, message = "An error occurred in ReserveCancel." });
            }
        }

        // Update QueueNumber
        public async Task <IActionResult> UpdateQueueNumber(string generateDate, int categoryId, int qNumber, int cheque)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _clerk.ToUpdateQueue(generateDate, categoryId, qNumber, userId, cheque);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateQueueNumber action");
                return Json(new { IsSuccess = false, message = "An error occurred in UpdateQueueNumber." });
            }
        }

        //HELPER
        private string GetCurrentUserId()
        {
            return User.GetUserId();
        }


        //private string GetUserIpAddress()
        //{
        //    string ipAddress = HttpContext.IpAddress();         
        //    return ipAddress ?? "";
        //}

        //Temporary
        private string GetUserIpAddress()
        {
            string userIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(userIpAddress) || userIpAddress == "::1")
            {
                try
                {
                    string hostName = Dns.GetHostName();
                    IPHostEntry ipHostEntries = Dns.GetHostEntry(hostName);
                    IPAddress[] ipAddresses = ipHostEntries.AddressList;

                    foreach (IPAddress ipAddress in ipAddresses)
                    {
                        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            userIpAddress = ipAddress.ToString();
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(userIpAddress))
                    {
                        userIpAddress = ipAddresses.Length > 0 ? ipAddresses[0].ToString() : "127.0.0.1";
                    }
                }
                catch
                {
                    userIpAddress = "127.0.0.1";
                }
            }

            return userIpAddress;
        }



    }


}
