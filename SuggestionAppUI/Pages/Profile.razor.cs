namespace SuggestionAppUI.Pages;

public partial class Profile
{
    private UserModel _loggedInUser;
    private List<SuggestionModel> _submissions;
    private List<SuggestionModel> _approved;
    private List<SuggestionModel> _archived;
    private List<SuggestionModel> _pending;
    private List<SuggestionModel> _rejected;

    protected override async Task OnInitializedAsync()
    {
        _loggedInUser = await authProvider.GetUserFromAuth(UserData);

        var results = await SuggestionData.GetUsersSuggestions(_loggedInUser.Id);

        if (_loggedInUser is not null && results is not null)
        {
            _submissions = results.OrderByDescending(s => s.DateCreated).ToList();
            _approved = _submissions.Where(s => s.ApprovedForRelease && s.Archived == false && s.Rejected == false).ToList();
            _archived = _submissions.Where(s => s.Archived && s.Rejected == false).ToList();
            _rejected = _submissions.Where(s => s.Rejected).ToList();
            _pending = _submissions.Where(s => s.Archived == false && s.ApprovedForRelease == false && s.Rejected == false).ToList();
        }
    }

    private void ClosePage()
    {
        NavManager.NavigateTo("/");
    }
}

