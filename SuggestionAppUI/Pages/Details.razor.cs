using Microsoft.AspNetCore.Components;

namespace SuggestionAppUI.Pages;

public partial class Details
{
    [Parameter]
    public string Id { get; set; }

    private SuggestionModel _suggestion;
    private UserModel _loggedInUser;

    private List<StatusModel> _statuses;
    private string _settingStatus = "";
    private string urlText = "";

    protected override async Task OnInitializedAsync()
    {
        _suggestion = await SuggestionData.GetSuggestion(Id);
        _loggedInUser = await authProvider.GetUserFromAuth(UserData);
        _statuses = await StatusData.GetAllStatuses();
    }

    private async Task CompleteSetStatus()
    {
        switch (_settingStatus)
        {
            case "completed":
                if (string.IsNullOrWhiteSpace(urlText))
                {
                    return;
                }
                _suggestion.SuggestionStatus = _statuses.First(s => s.StatusName.ToLower() == _settingStatus.ToLower());
                _suggestion.OwnerNotes = $"You are right, this is an important topic for developers. We created a resource about it here: <a id=\"url-color\" href='{urlText}' target='_blank'>{urlText}</a>";
                break;
            case "watching":
                _suggestion.SuggestionStatus = _statuses.First(s => s.StatusName.ToLower() == _settingStatus.ToLower());
                _suggestion.OwnerNotes = "We noticed the interest this suggestion is getting! If more people are interested we will address this topic in an upcoming resource.";
                break;
            case "upcoming":
                _suggestion.SuggestionStatus = _statuses.First(s => s.StatusName.ToLower() == _settingStatus.ToLower());
                _suggestion.OwnerNotes = "Great suggestion! We have a resource in the pipeline to address this topic.";
                break;
            case "dismissed":
                _suggestion.SuggestionStatus = _statuses.First(s => s.StatusName.ToLower() == _settingStatus.ToLower());
                _suggestion.OwnerNotes = "Sometimes a good idea doesn't fit within our scope and vision. This is one of those ideas.";
                break;
            default:
                return;
        }

        _settingStatus = null;
        await SuggestionData.UpdateSuggestion(_suggestion);
    }

    private void ClosePage()
    {
        NavManager.NavigateTo("/");
    }

    private string GetUpvoteTopText()
    {
        if (_suggestion.UserVotes?.Count > 0)
        {
            return _suggestion.UserVotes.Count.ToString("00");
        }
        if (_suggestion.Author.Id == _loggedInUser?.Id)
        {
            return "Awaiting";
        }

        return "Click To";
    }

    private string GetUpvoteBottomText()
    {
        if (_suggestion.UserVotes?.Count > 1)
        {
            return "Upvotes";
        }
        return "Upvotes";
    }

    private async Task VoteUp()
    {
        if (_loggedInUser is not null)
        {
            if (_suggestion.Author.Id == _loggedInUser.Id)
            {
                // Can't Vote on your own suggestion
                return;
            }
            if (_suggestion.UserVotes.Add(_loggedInUser.Id) == false)
            {
                _suggestion.UserVotes.Remove(_loggedInUser.Id);
            }

            await SuggestionData.UpvoteSuggestion(_suggestion.Id, _loggedInUser.Id);
        }
        else
        {
            NavManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string GetVoteClass()
    {
        if (_suggestion.UserVotes is null || _suggestion.UserVotes.Count == 0)
        {
            return "suggestion-entry-no-votes";
        }
        if (_suggestion.UserVotes.Contains(_loggedInUser?.Id))
        {
            return "suggestion-entry-voted";
        }
        return "suggestion-entry-not-voted";
    }

    private string GetStatusClass()
    {
        if (_suggestion is null || _suggestion.SuggestionStatus is null)
        {
            return "suggestion-detail-status-none";
        }

        var output = _suggestion.SuggestionStatus.StatusName switch
        {
            "Completed" => "suggestion-detail-status-completed",
            "Watching" => "suggestion-detail-status-watching",
            "Upcoming" => "suggestion-detail-status-upcoming",
            "Dismissed" => "suggestion-detail-status-dismissed",
            _ => "suggestion-detail-status-none"

        };
        return output;
    }
}

