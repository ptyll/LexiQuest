using TempoToastService = Tempo.Blazor.Services.ToastService;

namespace LexiQuest.Blazor.Services;

public class TempoToastServiceAdapter : IToastService
{
    private readonly TempoToastService _toastService;

    public TempoToastServiceAdapter(TempoToastService toastService)
    {
        _toastService = toastService;
    }

    public void ShowSuccess(string message, Action? onClick = null)
    {
        _toastService.ShowSuccess(message);
    }

    public void ShowError(string message, Action? onClick = null)
    {
        _toastService.ShowError(message);
    }

    public void ShowWarning(string message, Action? onClick = null)
    {
        _toastService.ShowWarning(message);
    }

    public void ShowInfo(string message, Action? onClick = null)
    {
        _toastService.ShowInfo(message);
    }
}
