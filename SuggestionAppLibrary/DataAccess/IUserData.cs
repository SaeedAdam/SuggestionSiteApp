namespace SuggestionAppLibrary.DataAccess;

public interface IUserData
{
    Task<List<UserModel>> GetUsersAsync();
    Task<UserModel> GetUserByIdAsync(string id);
    Task<UserModel> GetUserFromAuthentication(string objectId);
    Task CreateUser(UserModel user);
    Task UpdateUser(UserModel user);
}