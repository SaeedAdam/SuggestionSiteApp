namespace SuggestionAppLibrary.DataAccess;

public interface IStatusData
{
    Task<List<StatusModel>> GetAllStatuses();
    Task CreateStatus(StatusModel status);
}