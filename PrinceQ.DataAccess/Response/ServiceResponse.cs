namespace PrinceQ.DataAccess.Response
{
    public class ServiceResponses
    {
        public record class CommonResponse(bool IsSuccess, string Message);
        public record class GetResponse(bool IsSuccess, int? CategoryId, int? QueueNumber, string Message);
        public record class UserRolesResponse(bool IsSuccess, object? User, object? Roles, string Message);
        public record class GeneralResponse(bool IsSuccess, object? Obj, string Message);
        public record class DualResponse(bool IsSuccess, object? Obj1, object? Obj2, string Message);
        public record class UserCategoryResponse(object? User, object? Categories, object? UserCategories);
        public record class UserCategoriesResponse(object? UserCategories);
        public record class videoFilesResponse(bool IsSuccess, string[]? VideoFiles, string Message);

    }
}
