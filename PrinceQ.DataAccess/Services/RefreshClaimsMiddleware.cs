using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PrinceQ.Models.Entities;
using System.Security.Claims;

namespace PrinceQ.DataAccess.Services
{
    public class RefreshClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public RefreshClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var claimsIdentity = (ClaimsIdentity)context.User.Identity;

                    // Create a new ClaimsIdentity with updated claims
                    var newClaimsIdentity = new ClaimsIdentity(claimsIdentity.AuthenticationType);
                    newClaimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    newClaimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                    newClaimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

                    // Add updated role claims
                    foreach (var role in roles)
                    {
                        newClaimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    // Replace the current ClaimsIdentity with the new one
                    context.User = new ClaimsPrincipal(newClaimsIdentity);
                }
            }

            await _next(context);
        }
    }
}
