namespace SuggestionAppLibrary.Models;

public class CategoryModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
}

