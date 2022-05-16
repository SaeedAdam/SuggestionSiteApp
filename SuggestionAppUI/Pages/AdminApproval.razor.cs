namespace SuggestionAppUI.Pages;

public partial class AdminApproval
{
    private List<SuggestionModel> submissions;
    private SuggestionModel editingModel;
    private string currentEditingTitle = "";
    private string editedTitle = "";
    private string currentEditingDescription = "";
    private string editedDescription = "";

    protected override async Task OnInitializedAsync()
    {
        submissions = await SuggestionData.GetAllSuggestionsWaitingForApproval();
    }

    private async Task ApproveSubmission(SuggestionModel submission)
    {
        submission.ApprovedForRelease = true;
        submissions.Remove(submission);
        await SuggestionData.UpdateSuggestion(submission);
    }

    private async Task RejectSubmission(SuggestionModel submission)
    {
        submission.Rejected = true;
        submissions.Remove(submission);
        await SuggestionData.UpdateSuggestion(submission);
    }

    private void EditTitle(SuggestionModel model)
    {
        editingModel = model;
        editedTitle = model.Suggestion;
        currentEditingTitle = model.Id;
        currentEditingDescription = "";
    }

    private async Task SaveTitle(SuggestionModel model)
    {
        currentEditingTitle = string.Empty;
        model.Suggestion = editedTitle;
        await SuggestionData.UpdateSuggestion(model);
    }

    private void EditDescription(SuggestionModel model)
    {
        editingModel = model;
        editedDescription = model.Description;
        currentEditingDescription = model.Id;
        currentEditingTitle = "";
    }

    private async Task SaveDescription(SuggestionModel model)
    {
        currentEditingDescription = string.Empty;
        model.Description = editedDescription;
        await SuggestionData.UpdateSuggestion(model);
    }


    private void ClosePage()
    {
        NavManager.NavigateTo("/");
    }
}
