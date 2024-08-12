using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using static PrinceQ.DataAccess.Response.ServiceResponses;

namespace PrinceQ.DataAccess.Services
{
    public class AdminService : IAdmin
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<QueueHub> _hubContext;

        public AdminService(IUnitOfWork unitOfWork, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment, IHubContext<QueueHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }
         

        //-----DASHBOARD-----//


        //-----USERS-----//
        public async Task<GeneralResponse> UserPage()
        {
            var roles = _unitOfWork.auth.GetAllRoles();
            if (roles is null) return new GeneralResponse(false, null, "failed");

            var roleList = roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });
            var model = new RegisterVM
            {
                RoleList = roleList
            };
            return new GeneralResponse(true, model, "Success");

        }
        public async Task<GeneralResponse> AllUsers(string userId)
        {
            var users = await _unitOfWork.users.GetAll(u=>u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd");
            if (users is null) return new GeneralResponse(false, null, "Retrieved users failed." );
            var usersWithRole = users.Select(async user =>
            {
                var roles = await _unitOfWork.auth.GetUserRolesAsync(user);
                return new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.Created_At,
                    user.IsActive,
                    Roles = roles
                };
            }).Select(t => t.Result).ToList();
            var sortedUsers = usersWithRole.OrderByDescending(u => u.Roles.Contains("Clerk"))
                                          .ThenByDescending(u => u.Roles.Contains("RegisterPersonnel"))
                                          .ThenByDescending(u => u.Roles.Contains("Admin"))
                                          .ThenBy(u => u.Created_At)
                                          .ToList();
            return new GeneralResponse(true, sortedUsers, "Retrieved users failed.");
        }
        public async Task<GeneralResponse> AllRoles()
        {
            var roles = _unitOfWork.auth.GetAllRoles();

            if (roles is null) return new GeneralResponse(false, null, "There is no roles.");

            return new GeneralResponse(false, roles, "Roles fetch successfully.");

        }
        public async Task<UserRolesResponse> UserDetail(string? id)
        {
            var userToBeEdit = await _unitOfWork.users.Get(u => u.Id == id);
            //var activeList = await _unitOfWork.active.GetAll();
            if (userToBeEdit == null) return new UserRolesResponse(false, null, null, "Get user detail failed.");
            var userRole = await _userManager.GetRolesAsync(userToBeEdit);
            var user = new
            {
                id = userToBeEdit.Id,
                userName = userToBeEdit.UserName,
                email = userToBeEdit.Email,
                role = userRole,
                isActive = userToBeEdit.IsActive,
                //active = activeList,
            };
            var roles = await _roleManager.Roles.ToListAsync();
            return new UserRolesResponse(true, user, roles, "User get successful");
        }
        public async Task<CommonResponse> DeleteUser(string? id)
        {
            var userToBeDelete = await _unitOfWork.users.Get(u => u.Id == id);
            if (userToBeDelete == null) return new CommonResponse(false, "User delete failed");

            _unitOfWork.users.Remove(userToBeDelete);
            await _unitOfWork.SaveAsync();

            return new CommonResponse (true, "User delete successful" );
        }
        public async Task<CommonResponse> AddUser(UserVM model, string[] roleIds)
        {
            // Check if the username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUsername != null)
            {
                return new CommonResponse(false, "Username is already taken.");
            }

            // Check if the email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return new CommonResponse(false, "Email is already taken.");
            }

            User user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (roleIds.Length > 0)
                {
                    // Get the user's existing roles
                    var existingRoles = await _userManager.GetRolesAsync(user);
                    var existingRoleIds = existingRoles.Select(roleName =>
                        _roleManager.Roles.FirstOrDefault(role => role.Name == roleName)?.Id
                    ).Where(id => id != null).ToList();

                    // Determine roles to add
                    var roleIdsToAdd = roleIds.Except(existingRoleIds).ToArray();

                    // Handle role add
                    foreach (var roleId in roleIdsToAdd)
                    {
                        var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                        if (role != null)
                        {
                            await _userManager.AddToRoleAsync(user, role.Name);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
                return new CommonResponse(true, "User added successfully");
            }

            return new CommonResponse(false, "Failed to add user.");
        }

        public async Task<CommonResponse> UpdateUser(UserVM model, string[] roleIds)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return new CommonResponse(false, "User not found");
            }

            // Update user details
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            // Get the user's existing roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            var existingRoleIds = existingRoles.Select(roleName =>
                _roleManager.Roles.FirstOrDefault(role => role.Name == roleName)?.Id
            ).Where(id => id != null).ToList();

            // Determine roles to add and roles to remove
            var roleIdsToAdd = roleIds.Except(existingRoleIds).ToArray();
            var roleIdsToRemove = existingRoleIds.Except(roleIds).ToArray();

            // Handle role remove
            foreach (var roleId in roleIdsToRemove)
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    var roleName = role.Name;
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }
            }

            // Handle role add
            foreach (var roleId in roleIdsToAdd)
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            // Update user information
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new CommonResponse(false, "Failed to update user information");
            }

            // Update security stamp to refresh claims
            await _userManager.UpdateSecurityStampAsync(user);

            // Refresh claims based on updated roles
            var currentClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in currentClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role));
                foreach (var roleClaim in roleClaims)
                {
                    await _userManager.AddClaimAsync(user, roleClaim);
                }
            }
            var currentClaiasms = await _userManager.GetClaimsAsync(user);
            await _unitOfWork.SaveAsync();
            return new CommonResponse(true, "User updated successfully");
        }

        public async Task<UserCategoryResponse> UserCategory(string? userId)
        {
            var user = await _unitOfWork.users.Get(u => u.Id == userId);
            var categories = await _unitOfWork.category.GetAll();
            var userCategories = await _unitOfWork.userCategories.GetAll(uc => uc.UserId == userId);

            return new UserCategoryResponse( user, categories, userCategories );
        }
        public async Task<UserCategoriesResponse> GetAssignCategory(string? userId)
        {
            var user = await _unitOfWork.users.Get(u => u.Id == userId);
            var categories = await _unitOfWork.category.GetAll();
            var userCategories = await _unitOfWork.userCategories.GetAll(uc => uc.UserId == userId);
            return new UserCategoriesResponse(userCategories);
        }
        public async Task<CommonResponse> AddAssignUserCategories(int[] categoryId, string userId)
        {
            if (categoryId.Length == 0 || userId == null) return new CommonResponse(false, "User assign failed");

            var existingUserCategories = await _unitOfWork.userCategories.GetAll(c => c.UserId == userId);
            foreach (int catId in categoryId)
            {
                if (!existingUserCategories.Any(uc => uc.CategoryId == catId))
                {
                    User_Category userCat = new User_Category()
                    {
                        CategoryId = catId,
                        UserId = userId
                    };
                    _unitOfWork.userCategories.Add(userCat);
                }
            }
            await _unitOfWork.SaveAsync();
            await _hubContext.Clients.All.SendAsync("UpdateQueue");
            return new CommonResponse(true, "User assign successful");

        }
        public async Task<CommonResponse> RemoveAssignUserCategories(int categoryId, string userId)
        {
            if (categoryId == 0 || userId == null) return new CommonResponse(false, "User assign failed");

            var userCategory = await _unitOfWork.userCategories.Get(uc => uc.CategoryId == categoryId && uc.UserId == userId);
            _unitOfWork.userCategories.Remove(userCategory!);
            await _unitOfWork.SaveAsync();
            await _hubContext.Clients.All.SendAsync("UpdateQueue");
            return new CommonResponse(true, "User remove successful");
        }

        //-----MANAGE VIDEO-----//
        public async Task<videoFilesResponse> AllVideos()
        {
            string[] videoFiles = Directory.GetFiles(Path.Combine(_webHostEnvironment.WebRootPath, "Videos"))
               .Select(f => new FileInfo(f))
               .OrderByDescending(f => f.CreationTime)
               .Select(f => f.FullName.Replace(_webHostEnvironment.WebRootPath, string.Empty))
               .ToArray();

            return new videoFilesResponse(true, videoFiles, "Video files fetch successfully");
        }
        public async Task<videoFilesResponse> PlayVideo(string videoName)
        {
            var videoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, videoName);

            if (System.IO.File.Exists(videoFilePath))
            {
                var currentTime = DateTime.Now;
                System.IO.File.SetCreationTime(videoFilePath, currentTime);

                // Optional: Change modification time as well
                System.IO.File.SetLastWriteTime(videoFilePath, currentTime);

                // Return the updated video list
                string[] videoFiles = Directory.GetFiles(Path.Combine(_webHostEnvironment.WebRootPath, "Videos"))
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => f.FullName.Replace(_webHostEnvironment.WebRootPath, string.Empty))
                    .ToArray();
                // For REALTIME update
                await _hubContext.Clients.All.SendAsync("DisplayVideo");
                return new videoFilesResponse(true, videoFiles, "Video Play in Monitor.");
            }
            return new videoFilesResponse(false, null, "Video not found.");
        }
        public async Task<GeneralResponse> DeleteVideo(string videoName)
        {
            var videoFilePath = Path.Combine("wwwroot/", videoName);

            if (videoFilePath != null)
            {
                if (System.IO.File.Exists(videoFilePath))
                {
                    System.IO.File.Delete(videoFilePath);

                    return new GeneralResponse( true, null, "Video Successfully Deleted!" );
                }
            }
            return new GeneralResponse(false, null, "Video delete failed!");
        }
        public async Task<GeneralResponse> UploadVideo(IFormFile videoFile)
        {
            if (videoFile != null && videoFile.Length > 0)
            {
                var videoDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Videos");
                if (!Directory.Exists(videoDirectory))
                {
                    Directory.CreateDirectory(videoDirectory);
                }
                var videoFilePath = Path.Combine(videoDirectory, videoFile.FileName);

                using (var fileStream = new FileStream(videoFilePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(fileStream);
                }
                return new GeneralResponse(true, null, "Video upload Successfully!");
            }
            return new GeneralResponse(false, null, "Video upload failed!");
        }

        //-----Announcement-----//
        public async Task<GeneralResponse> AnnouncementDetail(int? id)
        {
            //var active = await _unitOfWork.active.GetAll();
            var announcement = await _unitOfWork.announcement.Get(u => u.Id == id);

            if (announcement == null) return new GeneralResponse(false, announcement,"Get announcement failed." );

            return new GeneralResponse(true, announcement, "Get announcement successful" );
        }
        public async Task<GeneralResponse> AllAnnouncement()
        {
            var announcement = await _unitOfWork.announcement.GetAll();
            var data = announcement.OrderByDescending(a => a.Created_At).ToList();

            if (data == null) return new GeneralResponse( false, null, "Retrieved announcement failed." );

            return new GeneralResponse( true, data, "Fetch successfully.");
        }
        public async Task<CommonResponse> AddAnnouncement(Announcement model)
        {
            Announcement announce = new()
            {
                Name = model.Name,
                Description = model.Description,
                Created_At = DateTime.Now,
                IsActive = false
            };
            _unitOfWork.announcement.Add(announce);
            await _unitOfWork.SaveAsync();
            return new CommonResponse( true, "Announcement added successfully");
        }
        public async Task<CommonResponse> UpdateAnnouncement(Announcement model)
        {
            var prevAnnounceActive = await _unitOfWork.announcement.Get(a => a.IsActive == true);
            if (prevAnnounceActive != null && prevAnnounceActive.Id == model.Id)
            {
                _unitOfWork.announcement.Update(model);
                await _unitOfWork.SaveAsync();
                await _hubContext.Clients.All.SendAsync("LoadAnnouncement");
                return new CommonResponse( true, "Announcement updated successfully." );
            }
            if (prevAnnounceActive != null)
            {
                prevAnnounceActive.IsActive = false;
                _unitOfWork.announcement.Update(prevAnnounceActive);
            }
            _unitOfWork.announcement.Update(model);
            await _unitOfWork.SaveAsync();
            await _hubContext.Clients.All.SendAsync("LoadAnnouncement");
            return new CommonResponse(true, "Announcement updated successfully");
        }
        public async Task<CommonResponse> DeleteAnnouncement(int? id)
        {
            var announcementToBeDeleted = await _unitOfWork.announcement.Get(u => u.Id == id);
            if (announcementToBeDeleted == null) return new CommonResponse( false, "Error occurred while deleting" );

            _unitOfWork.announcement.Remove(announcementToBeDeleted);
            await _unitOfWork.SaveAsync();
            return new CommonResponse(true, "Announcement deleted successful");
        }
        public async Task<GeneralResponse> GetTotalAnnounce()
        {
            var announcement = await _unitOfWork.announcement.GetAll();
            var totalAnnounce = announcement.Count();

            return new GeneralResponse( true, totalAnnounce, "");
        }
    }
}
