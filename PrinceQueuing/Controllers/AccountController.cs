using ExternalLogin.Extensions;
using ExternalLogin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using PrinceQ.Utility;
using System.Net;
using System.Security.Claims;

namespace PrinceQueuing.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalLoginService _externalLoginService;
        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IExternalLoginService externalLoginService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _externalLoginService = externalLoginService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            try
            {
                if (User.Identity!.IsAuthenticated)
                {
                    string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userManager.FindByIdAsync(userId!);

                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // Login
                var result = await _signInManager.PasswordSignInAsync(model.Username!, model.Password!, false, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username!);
                    var roles = await _userManager.GetRolesAsync(user!);

                    //var ipAddress = HttpContext.IpAddress();

                    var ipAddress = GetUserIpAddress();

                    var clerkUser = await _unitOfWork.device.Get(u => u.IPAddress == ipAddress);
                    if (clerkUser != null)
                    {
                        clerkUser.UserId = user?.Id;
                        _unitOfWork.device.Update(clerkUser);
                        await _unitOfWork.SaveAsync();
                    }

                    return RedirectToAction("Dashboard", "Admin");
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }



        public IActionResult Register()
        {
            // Create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_GenerateNumber).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_GenerateNumber)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Filling)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Releasing)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Reports)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Users)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Videos)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Announcement)).GetAwaiter().GetResult();
            }

            var roleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });

            var model = new RegisterVM
            {
                RoleList = roleList
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!String.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }
            return View(model);
        }


        public IActionResult AccessDenied()
        {
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

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
