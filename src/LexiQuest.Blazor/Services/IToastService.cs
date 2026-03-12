namespace LexiQuest.Blazor.Services;

public interface IToastService
{
    void ShowSuccess(string message, Action? onClick = null);
    void ShowError(string message, Action? onClick = null);
    void ShowWarning(string message, Action? onClick = null);
    void ShowInfo(string message, Action? onClick = null);
}
