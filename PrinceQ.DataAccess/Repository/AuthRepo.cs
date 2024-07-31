using Microsoft.AspNetCore.Identity;
using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class AuthRepo : Repository<User>, IAuthRepo
    {
        private readonly AppDbContext _db;
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public AuthRepo(AppDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //Get the UserID
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        //Get The Role
        public async Task<string[]> GetUserRolesAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToArray();
        }


        //Get all roles
        public IEnumerable<IdentityRole> GetAllRoles()
        {
            return _db.Roles.ToList();
        }


        //Get All Clerks
        public IEnumerable<User> GetAllClerks()
        {
            var clerks = _userManager.GetUsersInRoleAsync("clerk").Result;
            return clerks;
        }

        //Add the USER ROLE
        public async Task<IdentityResult> AddUserToRoleAsync(User user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        //remove the USER ROLE
        public async Task<IdentityResult> RemoveUserFromRolesAsync(User user, IEnumerable<string> roles)
        {
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        //ADD the USER with the password
        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }



    }
}