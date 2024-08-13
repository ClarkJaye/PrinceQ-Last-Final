using ExternalLogin.Extensions;
using ExternalLogin.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Utility;

namespace PrinceQueuing.Controllers
{
    public class ExternalController : Controller
    {
        private readonly IExternalLoginService externalLoginService;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IUnitOfWork unitOfWork;
        

        public ExternalController(
            IExternalLoginService externalLoginService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUnitOfWork unitOfWork)
        {
            this.externalLoginService = externalLoginService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.unitOfWork = unitOfWork;
        }


        [HttpGet]
        public async Task<IActionResult> Login(string token) // make sure to add the parameter in the endpoint
        {
            var ipaddress = HttpContext.IpAddress();
            var usercode = string.Empty;

            if (externalLoginService.TryValidateToken(token, ipaddress, out usercode))
                return await AuthenticateUser(usercode); // if validated, this is where you setup the user session 


            return Redirect(externalLoginService.PortalUrl);
        }

        public IActionResult Logout()
        {
            return Redirect(externalLoginService.PortalUrl);
        }

        // sample app user session setup 
        private async Task<IActionResult> AuthenticateUser(string usercode)
        {
            var user = await userManager.FindByNameAsync(usercode);

            if (user != null)
            {
                await signInManager.SignInAsync(user, isPersistent: false);

                var roles = await userManager.GetRolesAsync(user!);
                var ipAddress = HttpContext.IpAddress();

                var clerkUser = await unitOfWork.device.Get(u => u.IPAddress == ipAddress);
                if (clerkUser != null)
                {
                    clerkUser.UserId = user?.Id;
                    unitOfWork.device.Update(clerkUser);
                    await unitOfWork.SaveAsync();
                }
                return RedirectToAction("Dashboard", "Admin");
            }
            return Redirect(externalLoginService.PortalUrl);
        }



    }
}

