using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
    private const string CacheName = "SuggestionData";
    private readonly IMemoryCache _cache;
    private readonly IDbConnection _db;
    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private readonly IUserData _userData;

    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _db = db;
        _userData = userData;
        _cache = cache;
        _suggestions = db.SuggestionCollection;
    }

    async public Task<List<SuggestionModel>> GetAllSuggestions()
    {
        var output = _cache.Get<List<SuggestionModel>>(CacheName);

        if (output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Archived == false);
            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    async public Task<List<SuggestionModel>> GetUsersSuggestions(string userId)
    {
        var output = _cache.Get<List<SuggestionModel>>(userId);
        if (output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
            output = results.ToList();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    async public Task<List<SuggestionModel>> GetAllApprovedSuggestion()
    {
        var output = await GetAllSuggestions();
        return output.Where(s => s.ApprovedForRelease = true).ToList();
    }

    async public Task<SuggestionModel> GetSuggestion(string id)
    {
        var results = await _suggestions.FindAsync(s => s.Id == id);
        return results.FirstOrDefault();
    }

    async public Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
    {
        var output = await GetAllSuggestions();
        return output.Where(x => x.ApprovedForRelease == false && x.Rejected == false).ToList();
    }

    async public Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(CacheName);
    }

    async public Task UpvoteSuggestion(string suggestionId, string userId)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            var suggestion = (await suggestionInTransaction.FindAsync(s => s.Id == suggestionId)).First();

            var isUpvote = suggestion.UserVotes.Add(userId);
            if (isUpvote == false)
            {
                suggestion.UserVotes.Remove(userId);
            }

            await suggestionInTransaction.ReplaceOneAsync(session, s => s.Id == suggestionId, suggestion);

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUserByIdAsync(userId);

            if (isUpvote)
            {
                user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestions.First(s => s.Id == suggestionId);
                user.VotedOnSuggestions.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(session, u => u.Id == userId, user);

            await session.CommitTransactionAsync();

            _cache.Remove(CacheName);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    async public Task CreateSuggestion(SuggestionModel suggestion)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            await suggestionInTransaction.InsertOneAsync(session, suggestion);

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUserByIdAsync(suggestion.Author.Id);

            user.AuthorSuggestions.Add(new BasicSuggestionModel(suggestion));
            await usersInTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);

            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}