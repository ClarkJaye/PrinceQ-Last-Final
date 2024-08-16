using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Repository;

namespace PrinceQueuing.Controllers
{
    public class DisplayController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DisplayController(IUnitOfWork unitOfWork, IHubContext<QueueHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Home()
        {
            return View();
        }

        //GET Announcement
        public async Task<IActionResult> GetServings()
        {
            try
            {
                var queueServing = await _unitOfWork.servings.GetAll(s => s.Served_At.Date == DateTime.Today);
                var queueServe = queueServing.OrderByDescending(s => s.Served_At);
                var clerkDevices = await _unitOfWork.device.GetAll();

                var result = queueServe.Select(s => new
                {
                    s.QueueNumberServe,
                    s.CategoryId,
                    clerkNumber = clerkDevices.FirstOrDefault(d => d.UserId == s.UserId)?.ClerkNumber,
                }).ToList();

                return Json(new { queues = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //GET Announcement
        public async Task<IActionResult> GetAnnouncement()
        {
            var announce = await _unitOfWork.announcement.Get(a => a.IsActive == true);

            if (announce == null) return Json(new { IsSuccess = false });

            return Json(new { IsSuccess = true, announce });
        }


    }
}
