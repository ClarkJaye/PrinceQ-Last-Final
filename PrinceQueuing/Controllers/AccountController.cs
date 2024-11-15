using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using PrinceQueuing.External;
using System.DirectoryServices.AccountManagement;
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
        private readonly LDAPSettings _ldapSettings;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IOptions<LDAPSettings> ldapSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;

            if (ldapSettings == null || ldapSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(ldapSettings));
            }
            _ldapSettings = ldapSettings.Value;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            ViewData["UseADAuthentication"] = TempData["UseADAuthentication"];
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            TempData["UseADAuthentication"] = model.UseADAuthentication;

            if (!ValidateLoginModel(model)) return View(model);

            if (model.UseADAuthentication)
            {
                return await HandleADAuthentication(model);
            }
            return await HandleLocalAuthentication(model);
        }


        private bool ValidateLoginModel(LoginVM model)
        {
            if (model.UseADAuthentication)
            {
                ModelState.Remove("UserCode");
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "Email is required for AD authentication.");
                    return false;
                }
            }
            else
            {
                ModelState.Remove("Email");
                if (string.IsNullOrWhiteSpace(model.UserCode))
                {
                    ModelState.AddModelError("UserCode", "UserCode is required for local authentication.");
                    return false;
                }
            }
            return ModelState.IsValid;
        }

        // AD Login
        private async Task<IActionResult> HandleADAuthentication(LoginVM model)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, null, _ldapSettings.Path))
                {
                    if (context.ValidateCredentials(model.Email, model.Password))
                    {
                        var userAD = await _unitOfWork.users.Get(u => u.Email == model.Email);
                        if (userAD == null)
                        {
                            ModelState.AddModelError("", "You're not registered in this application.");
                            return View(model);
                        }

                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user == null)
                        {
                            user = new User { UserName = model.Email, Email = model.Email };
                            var result = await _userManager.CreateAsync(user);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", "Failed to create local user for AD authentication.");
                                return View(model);
                            }
                        }

                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
                ModelState.AddModelError("", "Invalid AD credentials.");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred during AD authentication.");
            }
            return View(model);
        }


        // Local Login
        private async Task<IActionResult> HandleLocalAuthentication(LoginVM model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserCode, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserCode);
                if (user != null)
                {
                    var ipAddress = GetUserIpAddress();
                    var clerkUser = await _unitOfWork.device.Get(u => u.IPAddress == ipAddress);
                    if (clerkUser != null)
                    {
                        clerkUser.UserId = user.Id;
                        _unitOfWork.device.Update(clerkUser);
                        await _unitOfWork.SaveAsync();
                    }
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }



        public IActionResult AccessDenied() => View();

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        private string GetUserIpAddress()
        {
            string userIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "::1";

            if (userIpAddress == "::1")
            {
                try
                {
                    string hostName = Dns.GetHostName();
                    IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;
                    userIpAddress = ipAddresses.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
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
