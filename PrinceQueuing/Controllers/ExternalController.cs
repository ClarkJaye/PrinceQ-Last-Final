using ExternalLogin.Extensions;
using ExternalLogin.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrinceQ.Models.Entities;
using PrinceQ.Utility;

namespace PrinceQueuing.Controllers
{
    public class ExternalController : Controller
    {
        private readonly IExternalLoginService externalLoginService;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public ExternalController(
            IExternalLoginService externalLoginService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.externalLoginService = externalLoginService;
            this.userManager = userManager;
            this.signInManager = signInManager;
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
            //var user = await userManager.FindByNameAsync(usercode + "@princeretail.com");
            var user = await userManager.FindByNameAsync(usercode);

            if (user != null)
            {
                await signInManager.SignInAsync(user, isPersistent: false);


                var roles = await userManager.GetRolesAsync(user);

                //if (roles.Contains(SD.Role_Personnel))
                //{
                //    return RedirectToAction("Home", "RegisterPersonnel");
                //}
                //else if (roles.Contains(SD.Role_Clerk))
                //{
                //    return RedirectToAction("Serving", "Clerk");
                //}
                //else if (roles.Contains(SD.Role_Admin))
                //{
                //    return RedirectToAction("Dashboard", "Admin");
                //}

                return RedirectToAction("Index", "Home");
            }

            return Redirect(externalLoginService.PortalUrl);
        }



    }
}

