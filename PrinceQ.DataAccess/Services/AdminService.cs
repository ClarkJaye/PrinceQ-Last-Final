using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrinceQ.DataAccess.Hubs;
using PrinceQ.DataAccess.Interfaces;
using PrinceQ.DataAccess.Repository;
using PrinceQ.Models.DTOs;
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
        public async Task<ChartDataResponse> GetDataByYearAndMonth(string year, string month)
        {
            if (year != null && month == null)
            {
                // Retrieve monthly data
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StageId != null && d.QueueId!.Substring(0, 4) == year);
                var monthlyData = data
                    .GroupBy(item => item.QueueId!.Substring(4, 2))
                    .Select(g => new
                    {
                        Month = g.Key,
                        CategoryASum = g.Sum(i => i.CategoryId == 1 ? 1 : 0),
                        CategoryBSum = g.Sum(i => i.CategoryId == 2 ? 1 : 0),
                        CategoryCSum = g.Sum(i => i.CategoryId == 3 ? 1 : 0),
                        CategoryDSum = g.Sum(i => i.CategoryId == 4 ? 1 : 0),
                        GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                    })
                    .ToList();
                return new ChartDataResponse( true, monthlyData);
            }
            else if (year != null && month != null)
            {
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StageId != null && d.QueueId!.Substring(0, 6) == year + month);
                var dailyData = data
                    .GroupBy(item => item.QueueId!.Substring(6, 2))
                    .Select(g => new
                    {
                        Day = g.Key,
                        CategoryASum = g.Sum(i => i.CategoryId == 1 ? 1 : 0),
                        CategoryBSum = g.Sum(i => i.CategoryId == 2 ? 1 : 0),
                        CategoryCSum = g.Sum(i => i.CategoryId == 3 ? 1 : 0),
                        CategoryDSum = g.Sum(i => i.CategoryId == 4 ? 1 : 0),
                        GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                    })
                    .ToList();

                return new ChartDataResponse(false, dailyData);
            }
            else
            {
                return null;
            }
        }
        public async Task<GeneralResponse> TotalQueueNumberPerDay()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate);

            var totalGenerate = data.Count();

            if (totalGenerate <= 0) return new GeneralResponse( false, new{ value = totalGenerate }, "failed to fetched");

            return new GeneralResponse(true, new {value = totalGenerate } , "fetched successful");
        }
        public async Task<GeneralResponse> GetQueueServed()
        {
            var servings = await _unitOfWork.servings.GetAll(g => g.Served_At.Date == DateTime.Now.Date);

            if (servings == null || !servings.Any())
                return new GeneralResponse ( false, null ,"No data available." );

            var queueNumbers = await _unitOfWork.queueNumbers.GetAll();
            var userIds = servings.Select(s => s.UserId).Distinct();
            var users = await _unitOfWork.users.GetAll(u => userIds.Contains(u.Id));

            var result = servings.Select(serving =>
            {
                var user = users.FirstOrDefault(u => u.Id == serving.UserId);
                var queueNumber = queueNumbers.FirstOrDefault(q =>
                    q.QueueId == serving.Served_At.ToString("yyyyMMdd") &&
                    q.CategoryId == serving.CategoryId &&
                    q.QueueNumber == serving.QueueNumberServe);

                return new
                {
                    category = serving.CategoryId,
                    qNumber = serving.QueueNumberServe,
                    servingBy = user?.UserName ?? "Unknown",
                    stage = queueNumber?.StageId,
                    dateTime = serving.Served_At.ToString("MM/dd/yyyy, hh:mm:ss tt")
                };
            }).ToList();

            return new GeneralResponse( true, new {data = result } , "successfully get.");
        }
        public async Task<GeneralResponse> TotalServed()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StageId == 2);

            var totalQ = data.Count();

            if (totalQ <= 0) return new GeneralResponse( false, new {value = totalQ }, "fetch failed.");

            return new GeneralResponse( true, new { value = totalQ }, "successfuly get.");
        }
        public async Task<GeneralResponse> TotalReservedNumberPerDay()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StatusId == 3);

            var totalGenerate = data.Count();

            if (totalGenerate <= 0) return new GeneralResponse(false, new { value = totalGenerate }, "failed to fetched");

            return new GeneralResponse(true, new { value = totalGenerate }, "fetched successful");
        }
        public async Task<GeneralResponse> TotalCancelNumberPerDay()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StatusId == 4);

            var totalGenerate = data.Count();

            if (totalGenerate <= 0) return new GeneralResponse(false, new { value = totalGenerate }, "failed to fetched");

            return new GeneralResponse(true, new { value = totalGenerate }, "fetched successful");
        }
        public async Task<GeneralResponse> RecentlyServed()
        {
            var currentDate = DateTime.Today.ToString("yyyyMMdd");
            var data = await _unitOfWork.queueNumbers.GetAll(g => g.QueueId == currentDate && g.StageId == 2);

            var totalQ = data.Count();

            if (totalQ <= 0) return new GeneralResponse( false, new { value = totalQ }, "fetch failed");

            return new GeneralResponse( true, new { value = totalQ }, "successfully get.");
        }

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
        public async Task<GeneralResponse> totalUserCount()
        {
            int totalUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd");
            int activeUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd" && u.IsActive == true);
            int inactiveUsers = await _unitOfWork.users.Count(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd" && u.IsActive == false);

            var response = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers
            };

            if(response.TotalUsers == 0) return new GeneralResponse(false, null, "no user yet.");

            return new GeneralResponse(true, response, "Fetched Successfully.");
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

            if (File.Exists(videoFilePath))
            {
                try
                {
                    var currentTime = DateTime.Now;
                    File.SetCreationTime(videoFilePath, currentTime);

                    // Optional: Change modification time as well
                    File.SetLastWriteTime(videoFilePath, currentTime);

                    string[] videoFiles = Directory.GetFiles(Path.Combine(_webHostEnvironment.WebRootPath, "Videos"))
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.CreationTime)
                        .Select(f => f.FullName.Replace(_webHostEnvironment.WebRootPath, string.Empty).Replace("\\", "/"))
                        .ToArray();

                    // For REALTIME update
                    await _hubContext.Clients.All.SendAsync("DisplayVideo");
                    return new videoFilesResponse(true, videoFiles, "Video Play in Monitor.");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    // For REALTIME update
                    await _hubContext.Clients.All.SendAsync("DisplayVideo");
                }
              
            }
            return new videoFilesResponse(false, null, "Video not found.");
        }
        public async Task<GeneralResponse> DeleteVideo(string videoName)
        {
            var videoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, videoName.Replace("/", "\\"));

            if (File.Exists(videoFilePath))
            {
                const int maxRetries = 3;
                const int delayBetweenRetries = 200; // in milliseconds

                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        File.Delete(videoFilePath);
                        await _hubContext.Clients.All.SendAsync("DisplayVideo");
                        return new GeneralResponse(true, null, "Video Successfully Deleted!");
                    }
                    catch (IOException)
                    {
                        if (retry == maxRetries - 1)
                        {
                            return new GeneralResponse(false, null, "An error occurred while deleting the video. The file is in use.");
                        }
                        await Task.Delay(delayBetweenRetries);
                    }
                    catch (Exception ex)
                    {
                        // Optional: log the exception
                        return new GeneralResponse(false, null, "An unexpected error occurred while deleting the video.");
                    }
                }
            }

            return new GeneralResponse(false, null, "Video delete failed! File not found.");
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
            }
            else
            {
                if (prevAnnounceActive != null)
                {
                    prevAnnounceActive.IsActive = false;
                    _unitOfWork.announcement.Update(prevAnnounceActive);
                }

                _unitOfWork.announcement.Update(model);
            }

            await _unitOfWork.SaveAsync();
            await _hubContext.Clients.All.SendAsync("LoadAnnouncement");
            return new CommonResponse(true, "Announcement updated successfully.");
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

        //----- SERVING Report-----//
        public async Task<GeneralResponse> Serving_GetAllServedData()
        {

            var forFillingData = await _unitOfWork.forFilling.GetAll();
            var releasingData = await _unitOfWork.releasing.GetAll();
            var users = await _unitOfWork.users.GetAll();

            var userDictionary = users.ToDictionary(u => u.Id, u => u.UserName);

            var forFillingMapped = forFillingData
                .Where(f => f.Serve_start.HasValue && f.Serve_end.HasValue)
                .Select(f => new ServeDataDTO
                {
                    GenerateDate = f.GenerateDate,
                    ClerkId = f.ClerkId,
                    CategoryId = f.CategoryId ?? 0,
                    QueueNumber = f.QueueNumber ?? 0,
                    ServeStart = f.Serve_start.Value.TimeOfDay,
                    ServeEnd = f.Serve_end.Value.TimeOfDay
                });
            var releasingMapped = releasingData
                .Where(r => r.Serve_start.HasValue && r.Serve_end.HasValue)
                .Select(r => new ServeDataDTO
                {
                    GenerateDate = r.GenerateDate,
                    ClerkId = r.ClerkId,
                    CategoryId = r.CategoryId ?? 0,
                    QueueNumber = r.QueueNumber ?? 0,
                    ServeStart = r.Serve_start.Value.TimeOfDay,
                    ServeEnd = r.Serve_end.Value.TimeOfDay
                });
            var combinedData = forFillingMapped.Concat(releasingMapped)
                                .GroupBy(x => new { x.ClerkId, x.GenerateDate })
                                .Select(g => new
                                {
                                    User = g.Key,
                                    Username = userDictionary.TryGetValue(g.Key.ClerkId, out var username) ? username : "Unknown",
                                    TotalNumberServed = g.Count(),
                                    LongestServedTime = g.Max(x => (x.ServeEnd - x.ServeStart).TotalSeconds),
                                    ShortestServedTime = g.Min(x => (x.ServeEnd - x.ServeStart).TotalSeconds),
                                    Date = g.Key.GenerateDate
                                });
            return new GeneralResponse(true, combinedData, "Successfully fetched");
        }
        public async Task<GeneralResponse> GetDetailedData(string clerkId, string generateDate)
        {
                var forFillingData = await _unitOfWork.forFilling.GetAll();
                var releasingData = await _unitOfWork.releasing.GetAll();
                var queuesData = await _unitOfWork.queueNumbers.GetAll();
                var users = await _unitOfWork.users.GetAll();

                var userDictionary = users.ToDictionary(u => u.Id, u => u.UserName);

                var forFillingMapped = forFillingData
                    .Select(f => new ServeDataDTO
                    {
                        GenerateDate = f.GenerateDate,
                        ClerkId = f.ClerkId,
                        CategoryId = f.CategoryId ?? 0,
                        QueueNumber = f.QueueNumber ?? 0,
                        Total_Cheque = queuesData.FirstOrDefault(q => q.QueueId == f.GenerateDate && q.CategoryId == f.CategoryId && q.QueueNumber == f.QueueNumber)?.Total_Cheques ?? 0,
                        ServeStart = f.Serve_start.Value.TimeOfDay,
                        ServeEnd = f.Serve_end.Value.TimeOfDay,
                        stageId = 1,
                    });
                var releasingMapped = releasingData
                    .Select(r => new ServeDataDTO
                    {
                        GenerateDate = r.GenerateDate,
                        ClerkId = r.ClerkId,
                        CategoryId = r.CategoryId ?? 0,
                        QueueNumber = r.QueueNumber ?? 0,
                        Total_Cheque = queuesData.FirstOrDefault(q => q.QueueId == r.GenerateDate && q.CategoryId == r.CategoryId && q.QueueNumber == r.QueueNumber)?.Total_Cheques ?? 0,
                        ServeStart = r.Serve_start.Value.TimeOfDay,
                        ServeEnd = r.Serve_end.Value.TimeOfDay,
                        stageId = 2,
                    });
                var combinedData = forFillingMapped.Concat(releasingMapped)
                                      .Where(x => x.ClerkId == clerkId && x.GenerateDate == generateDate)
                                      .Select(x => new
                                      {
                                          x.GenerateDate,
                                          Username = userDictionary.TryGetValue(x.ClerkId, out var username) ? username : "Unknown",
                                          x.CategoryId,
                                          x.QueueNumber,
                                          x.Total_Cheque,
                                          x.ServeStart,
                                          x.ServeEnd,
                                          x.stageId,
                                      });
                return new GeneralResponse(true, combinedData, "Successfully fetched");
        }
        public async Task<GeneralResponse> GetClerks_Categories()
        {
            var categ = await _unitOfWork.category.GetAll();
            var categories = categ.OrderBy(c => c.Created_At).ToList();
            var users = await _unitOfWork.users.GetAll(u => u.Id != "f626b751-35a0-43df-8173-76cb5b4886fd");
            var ListUsers = new List<dynamic>();

            foreach (var user in users)
            {

                ListUsers.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.Created_At,
                    user.IsActive,
                });
            }

            if (ListUsers == null) return new GeneralResponse(false, null, "Retrieved clerks failed." );
            var sortedUsers = ListUsers.OrderBy(c => c.Created_At).ToList();

            return new GeneralResponse( true, new {users = sortedUsers, categories }, "Successfully Fetched");
        }
        public async Task<ChartDataResponse> GetServingDataClerk(string clerkId, string year, string month)
        {
            if (clerkId != null && year != null && month == null)
            {
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StatusId == 5 && d.ClerkId == clerkId && d.QueueId!.Substring(0, 4) == year);
                var monthlyData = data
                    .GroupBy(item => item.QueueId!.Substring(4, 2))
                    .Select(g => new
                    {
                        Month = g.Key,
                        CategoryASum = g.Count(i => i.CategoryId == 1),
                        CategoryBSum = g.Count(i => i.CategoryId == 2),
                        CategoryCSum = g.Count(i => i.CategoryId == 3),
                        CategoryDSum = g.Count(i => i.CategoryId == 4),
                    })
                    .ToList();
                return new ChartDataResponse( true , monthlyData);
            }
            else if (clerkId == null && year != null && month == null)
            {
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StatusId == 5 && d.QueueId!.Substring(0, 4) == year);
                var monthlyData = data
                    .GroupBy(item => item.QueueId!.Substring(4, 2))
                    .Select(g => new
                    {
                        Month = g.Key,
                        CategoryASum = g.Count(i => i.CategoryId == 1),
                        CategoryBSum = g.Count(i => i.CategoryId == 2),
                        CategoryCSum = g.Count(i => i.CategoryId == 3),
                        CategoryDSum = g.Count(i => i.CategoryId == 4),
                    })
                    .ToList();
                return new ChartDataResponse(true, monthlyData);
            }
            else if (clerkId != null && year != null && month != null)
            {
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StatusId == 5 && d.ClerkId == clerkId && d.QueueId!.Substring(0, 6) == year + month);
                var dailyData = data
                    .GroupBy(item => item.QueueId!.Substring(6, 2))
                    .Select(g => new
                    {
                        Day = g.Key,
                        CategoryASum = g.Count(i => i.CategoryId == 1),
                        CategoryBSum = g.Count(i => i.CategoryId == 2),
                        CategoryCSum = g.Count(i => i.CategoryId == 3),
                        CategoryDSum = g.Count(i => i.CategoryId == 4),
                        GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                    })
                    .ToList();
                return new ChartDataResponse(false, dailyData );
            }
            else if (clerkId == null && year != null && month != null)
            {
                // example data of generateDate = "20240626"
                var data = await _unitOfWork.queueNumbers.GetAll(d => d.StatusId == 5 && d.QueueId!.Substring(0, 6) == year + month);
                var dailyData = data
                    .GroupBy(item => item.QueueId!.Substring(6, 2))
                    .Select(g => new
                    {
                        Day = g.Key,
                        CategoryASum = g.Count(i => i.CategoryId == 1),
                        CategoryBSum = g.Count(i => i.CategoryId == 2),
                        CategoryCSum = g.Count(i => i.CategoryId == 3),
                        CategoryDSum = g.Count(i => i.CategoryId == 4),
                        GenerateDate = g.Select(i => i.QueueId).FirstOrDefault()
                    })
                    .ToList();
                return new ChartDataResponse(false, dailyData);
            }
            else
            {
                return null;
            }
        }
        //----- Waiting Report-----//
        public async Task<GeneralResponse> Waiting_GetAllServedData()
        {
            var data = await _unitOfWork.queueNumbers.GetAll();

            var result = data.Select(q => new
            {
                GenerateDate = q.QueueId,
                CategoryId = q.CategoryId,
                QueueNumber = q.QueueNumber,
                Total_Cheque = q.Total_Cheques,
                GeneratedStart = q.Generate_At.HasValue ? q.Generate_At.Value.ToString("hh:mm tt") : "N/A",
                FillingStart = q.ForFilling_start.HasValue ? q.ForFilling_start.Value.ToString("hh:mm tt") : "N/A",
                FillingEnd = q.ForFilling_end.HasValue ? q.ForFilling_end.Value.ToString("hh:mm tt") : "N/A",
                ReleasingStart = q.Releasing_start.HasValue ? q.Releasing_start.Value.ToString("hh:mm tt") : "N/A",
                ReleasingEnd = q.Releasing_end.HasValue ? q.Releasing_end.Value.ToString("hh:mm tt") : "N/A",

                CallFillingStart_Reserved = q.ForFilling_start_Backup.HasValue ? q.ForFilling_start_Backup.Value.ToString("hh:mm tt") : "N/A",
                CallReleasingStart_Reserved = q.Releasing_start_Backup.HasValue ? q.Releasing_start_Backup.Value.ToString("hh:mm tt") : "N/A",

                AverageTime = CalculateAverageTime(q)
            });

            return new GeneralResponse( true, new{ data = result }, "Successfully Fetched");
        }
        private string CalculateAverageTime(Queues q)
        {
            var forFillingStart = q.ForFilling_start_Backup ?? q.ForFilling_start;
            var releasingStart = q.Releasing_start_Backup ?? q.Releasing_start;

            if (q.Releasing_end.HasValue && forFillingStart.HasValue && releasingStart.HasValue && q.Generate_At.HasValue)
            {
                var one = q.ForFilling_start.Value - q.Generate_At.Value;
                var two = q.ForFilling_end.HasValue ? q.ForFilling_end.Value - forFillingStart.Value : TimeSpan.Zero;
                var three = q.Releasing_start.HasValue ? q.Releasing_start.Value - q.ForFilling_end.Value : TimeSpan.Zero;
                var four = q.Releasing_end.Value - releasingStart.Value;

                double totalSeconds = (one + two + three + four).TotalSeconds;

                return waiting_FormatSeconds(totalSeconds);
            }
            else
            {
                return "N/A";
            }
        }
        private string waiting_FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalMinutes >= 60)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            }
            else
            {
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            }
        }
        private string FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalMinutes >= 60)
            {
                return $"{(int)timeSpan.TotalHours}.{timeSpan.Minutes}";
            }
            else
            {
                return $"{(int)timeSpan.TotalMinutes}.{timeSpan.Seconds}";
            }
        }

        
    }
}
