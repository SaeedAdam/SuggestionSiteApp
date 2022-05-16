namespace SuggestionAppLibrary.DataAccess;

public class MongoUserData : IUserData
{
    private readonly IMongoCollection<UserModel> _users;

    public MongoUserData(IDbConnection db)
    {
        _users = db.UserCollection;
    }

    async public Task<List<UserModel>> GetUsersAsync()
    {
        var results = await _users.FindAsync(_ => true);
        return results.ToList();
    }

    async public Task<UserModel> GetUserByIdAsync(string id)
    {
        var results = await _users.FindAsync(u => u.Id == id);
        return results.FirstOrDefault();
    }

    async public Task<UserModel> GetUserFromAuthentication(string objectId)
    {
        var results = await _users.FindAsync(u => u.ObjectIdentifier == objectId);
        return results.FirstOrDefault();
    }

    public Task CreateUser(UserModel user)
    {
        return _users.InsertOneAsync(user);
    }

    public Task UpdateUser(UserModel user)
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);
        return _users.ReplaceOneAsync(filter, user, new ReplaceOptions {IsUpsert = true});
    }
}