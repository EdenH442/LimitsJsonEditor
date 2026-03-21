namespace LimitsEditor.Services;

public interface IConfirmationDialogService
{
    bool ShowConfirmation(string message, string title);
}
