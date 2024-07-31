using Microsoft.AspNetCore.Identity;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository.IRepository
{
    public interface IAuthRepo : IRepository<User>
    {
        Task<User?> GetUserByIdAsync(string userId);

        Task<string[]> GetUserRolesAsync(User user);

        IEnumerable<IdentityRole> GetAllRoles();

        public IEnumerable<User> GetAllClerks();

        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        Task<IdentityResult> RemoveUserFromRolesAsync(User user, IEnumerable<string> roles);

        Task<IdentityResult> CreateUserAsync(User user, string password);
    }
}
