using SuggestionAppUI.Models;

namespace SuggestionAppUI.Pages;

public partial class Create
{
    private CreateSuggestionModel _suggestion = new();
    public List<CategoryModel> _categories { get; set; }
    public UserModel _loggedInUser { get; set; }


    protected override async Task OnInitializedAsync()
    {
        _categories = await CategoryData.GetAllCategories();
        _loggedInUser = await authProvider.GetUserFromAuth(UserData);
    }

    private void ClosePage()
    {
        NavManager.NavigateTo("/");
    }

    private async Task CreateSuggestion()
    {
        SuggestionModel s = new();
        s.Suggestion = _suggestion.Suggestion;
        s.ApprovedForRelease = false;
        if (string.IsNullOrWhiteSpace(_suggestion.Description))
        {
            s.Description = "";
        }
        else
        {
            s.Description = _suggestion.Description;
        }
        s.Author = new BasicUserModel(_loggedInUser);
        s.Category = _categories.FirstOrDefault(c => c.CategoryId == _suggestion.CategoryId);

        if (s.Category is null)
        {
            _suggestion.CategoryId = "";
            return;
        }

        await SuggestionData.CreateSuggestion(s);
        _suggestion = new CreateSuggestionModel();
        ClosePage();
    }
}
