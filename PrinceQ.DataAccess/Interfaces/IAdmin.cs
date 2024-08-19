using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PrinceQ.Models.Entities;
using PrinceQ.Models.ViewModel;
using static PrinceQ.DataAccess.Response.ServiceResponses;

namespace PrinceQ.DataAccess.Interfaces
{
    public interface IAdmin
    {

        //DASHBOARD
        Task<ChartDataResponse> GetDataByYearAndMonth(string year, string month);
        Task<GeneralResponse> TotalQueueNumberPerDay();
        Task<GeneralResponse> TotalReservedNumberPerDay();
        Task<GeneralResponse> TotalCancelNumberPerDay();
        Task<GeneralResponse> GetQueueServed();
        Task<GeneralResponse> TotalServed();
        Task<GeneralResponse> RecentlyServed();


        //SERVING REPORT
        Task<GeneralResponse> Serving_GetAllServedData();
        Task<GeneralResponse> GetDetailedData(string clerkId, string generateDate);
        Task<GeneralResponse> GetClerks_Categories();

        //Waiting REPORT
        Task<ChartDataResponse> GetServingDataClerk(string clerkId, string year, string month);
        Task<GeneralResponse> Waiting_GetAllServedData();



        //-----USERS-----//
        Task<GeneralResponse> UserPage();
        Task<GeneralResponse> AllUsers(string userId);
        Task<GeneralResponse> AllRoles();
        Task<UserRolesResponse> UserDetail(string? id);
        Task<CommonResponse> DeleteUser(string? id);
        Task<CommonResponse> AddUser(UserVM model, string[] roles);
        Task<CommonResponse> UpdateUser(UserVM model, string[] roles);
        Task<UserCategoryResponse> UserCategory(string? userId);
        Task<UserCategoriesResponse> GetAssignCategory(string? userId);
        Task<CommonResponse> AddAssignUserCategories(int[] categoryId, string userId);
        Task<CommonResponse> RemoveAssignUserCategories(int categoryId, string userId);
        Task<GeneralResponse> totalUserCount();

        //-----MANAGE VIDEO-----//
        Task<videoFilesResponse> AllVideos();
        Task<videoFilesResponse> PlayVideo(string videoName);
        Task<GeneralResponse> DeleteVideo(string videoName);
        Task<GeneralResponse> UploadVideo(IFormFile videoFile);


        //-----Announcement-----//
        Task<GeneralResponse> AnnouncementDetail(int? id);
        Task<GeneralResponse> AllAnnouncement();
        Task<CommonResponse> AddAnnouncement(Announcement model);
        Task<CommonResponse> UpdateAnnouncement(Announcement model);
        Task<CommonResponse> DeleteAnnouncement(int? id);
        Task<GeneralResponse> GetTotalAnnounce();




    }
}
