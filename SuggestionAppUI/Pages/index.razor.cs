namespace SuggestionAppUI.Pages;

public partial class Index
{
    private UserModel _loggedInUser;
    private List<SuggestionModel> _suggestions;
    private List<CategoryModel> _categories;
    private List<StatusModel> _statuses;
    private SuggestionModel _archivingSuggestion;
    private string _selectedCategory = "All";
    private string _selectedStatus = "All";
    private string _searchText = "";
    bool _isSortedByNew = true;
    private bool _showCategories;
    private bool _showStatuses;

    protected override async Task OnInitializedAsync()
    {
        _categories = await CategoryData.GetAllCategories();
        _statuses = await StatusData.GetAllStatuses();
        await LoadAndVerifyUser();
    }

    private async Task LoadAndVerifyUser()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;

        if (string.IsNullOrWhiteSpace(objectId) == false)
        {
            _loggedInUser = await UserData.GetUserFromAuthentication(objectId) ?? new UserModel();

            var firstName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value;
            var lastName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("surname"))?.Value;
            var displayName = authState.User.Claims.FirstOrDefault(c => c.Type.Equals("name"))?.Value;
            var email = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value;

            var isDirty = false;
            if (objectId.Equals(_loggedInUser.ObjectIdentifier) == false)
            {
                isDirty = true;
                _loggedInUser.ObjectIdentifier = objectId;
            }
            if (firstName.Equals(_loggedInUser.FirstName) == false)
            {
                isDirty = true;
                _loggedInUser.FirstName = firstName;
            }
            if (lastName.Equals(_loggedInUser.LastName) == false)
            {
                isDirty = true;
                _loggedInUser.LastName = lastName;
            }
            if (displayName.Equals(_loggedInUser.DisplayName) == false)
            {
                isDirty = true;
                _loggedInUser.DisplayName = displayName;
            }
            if (email.Equals(_loggedInUser.EmailAddress) == false)
            {
                isDirty = true;
                _loggedInUser.EmailAddress = email;
            }
            if (isDirty)
            {
                if (string.IsNullOrWhiteSpace(_loggedInUser.Id))
                {
                    await UserData.CreateUser(_loggedInUser);
                }
                else
                {
                    await UserData.UpdateUser(_loggedInUser);
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterSuggestions();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await SessionStorage.GetAsync<string>(nameof(_selectedCategory));
        _selectedCategory = stringResults.Success ? stringResults.Value : "All";

        stringResults = await SessionStorage.GetAsync<string>(nameof(_selectedStatus));
        _selectedStatus = stringResults.Success ? stringResults.Value : "All";

        stringResults = await SessionStorage.GetAsync<string>(nameof(_searchText));
        _searchText = stringResults.Success ? stringResults.Value : "";

        var boolResults = await SessionStorage.GetAsync<bool>(nameof(_isSortedByNew));
        _isSortedByNew = stringResults.Success ? boolResults.Value : true;
    }

    private async Task SaveFilterState()
    {
        await SessionStorage.SetAsync(nameof(_selectedCategory), _selectedCategory);
        await SessionStorage.SetAsync(nameof(_selectedStatus), _selectedStatus);
        await SessionStorage.SetAsync(nameof(_searchText), _searchText);
        await SessionStorage.SetAsync(nameof(_isSortedByNew), _isSortedByNew);
    }

    private async Task FilterSuggestions()
    {
        var output = await SuggestionData.GetAllApprovedSuggestion();

        if (_selectedCategory != "All")
        {
            output = output.Where(s => s.Category?.CategoryName == _selectedCategory).ToList();
        }

        if (_selectedStatus != "All")
        {
            output = output.Where(s => s.SuggestionStatus?.StatusName == _selectedStatus).ToList();
        }

        if (string.IsNullOrWhiteSpace(_searchText) == false)
        {

            output = output.Where(
                s => s.Suggestion.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase) ||
                     s.Description.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase)
            ).ToList();
        }

        if (_isSortedByNew)
        {
            output = output.OrderByDescending(s => s.DateCreated).ToList();
        }
        else
        {
            output = output.OrderByDescending(s => s.UserVotes.Count)
                .ThenByDescending(s => s.DateCreated)
                .ToList();
        }

        _suggestions = output;

        await SaveFilterState();
    }

    private async Task OrderByNew(bool isNew)
    {
        _isSortedByNew = isNew;
        await FilterSuggestions();
    }

    private async Task OnSearchInput(string? searchInput)
    {
        _searchText = searchInput;
        await FilterSuggestions();
    }

    private async Task OnCategoryClick(string category = "All")
    {
        _selectedCategory = category;
        _showCategories = false;
        await FilterSuggestions();
    }

    private async Task OnStatusClick(string status = "All")
    {
        _selectedStatus = status;
        _showStatuses = false;
        await FilterSuggestions();
    }

    private async Task VoteUp(SuggestionModel suggestion)
    {
        if (_loggedInUser is not null)
        {
            if (suggestion.Author.Id == _loggedInUser.Id)
            {
                // Can't Vote on your own suggestion
                return;
            }
            if (suggestion.UserVotes.Add(_loggedInUser.Id) == false)
            {
                suggestion.UserVotes.Remove(_loggedInUser.Id);
            }

            await SuggestionData.UpvoteSuggestion(suggestion.Id, _loggedInUser.Id);

            if (_isSortedByNew == false)
            {
                _suggestions = _suggestions
                    .OrderByDescending(s => s.UserVotes.Count)
                    .ThenByDescending(s => s.DateCreated)
                    .ToList();
            }
        }
        else
        {
            NavManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string GetUpvoteTopText(SuggestionModel suggestion)
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return suggestion.UserVotes.Count.ToString("00");
        }
        if (suggestion.Author.Id == _loggedInUser?.Id)
        {
            return "Awaiting";
        }
        return "Click To";
    }

    private string GetUpvoteBottomText(SuggestionModel suggestion)
    {
        if (suggestion.UserVotes?.Count > 1)
        {
            return "Upvotes";
        }
        return "Upvotes";
    }

    private void OpenDetails(SuggestionModel suggestion)
    {
        NavManager.NavigateTo($"/Details/{suggestion.Id}");
    }

    private void LoadCreatePage()
    {
        if (_loggedInUser is not null)
        {
            NavManager.NavigateTo("/Create");
        }
        else
        {
            NavManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isNew == _isSortedByNew)
        {
            return "sort-selected";
        }

        return "";
    }

    private string GetVoteClass(SuggestionModel suggestion)
    {
        if (suggestion.UserVotes is null || suggestion.UserVotes.Count == 0)
        {
            return "suggestion-entry-no-votes";
        }
        if (suggestion.UserVotes.Contains(_loggedInUser?.Id))
        {
            return "suggestion-entry-voted";
        }
        return "suggestion-entry-not-voted";
    }

    private string GetSuggestionStatusClass(SuggestionModel suggestion)
    {
        if (suggestion is null || suggestion.SuggestionStatus is null)
        {
            return "suggestion-entry-status-none";
        }

        var output = suggestion.SuggestionStatus.StatusName switch
        {
            "Completed" => "suggestion-entry-status-completed",
            "Watching" => "suggestion-entry-status-watching",
            "Upcoming" => "suggestion-entry-status-upcoming",
            "Dismissed" => "suggestion-entry-status-dismissed",
            _ => "suggestion-entry-status-none"

        };
        return output;
    }

    private string GetSelectedCategory(string category = "All")
    {
        if (category == _selectedCategory)
        {
            return "selected-category";
        }
        return "";
    }

    private string GetSelectedStatus(string status = "All")
    {
        if (status == _selectedStatus)
        {
            return "selected-status";
        }
        return "";
    }

    private async Task ArchiveSuggestion()
    {
        _archivingSuggestion.Archived = true;
        await SuggestionData.UpdateSuggestion(_archivingSuggestion);
        _suggestions.Remove(_archivingSuggestion);
        _archivingSuggestion = null;
        //await FilterSuggestions();
    }
}
