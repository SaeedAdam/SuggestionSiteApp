namespace SuggestionAppLibrary.Models;

public class BasicUserModel
{
    public BasicUserModel()
    {
    }

    public BasicUserModel(UserModel user)
    {
        Id = user.Id;
        DisplayName = user.DisplayName;
    }

    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string DisplayName { get; set; }
}