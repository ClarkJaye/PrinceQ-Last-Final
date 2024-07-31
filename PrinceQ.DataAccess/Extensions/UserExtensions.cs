using System.Security.Claims;

namespace PrinceQ.DataAccess.Extensions
{
    public static class UserExtensions
    {
        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)!.Value;
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }

        public static string GetEmailAddress(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)!.Value;
        }
    }
}
