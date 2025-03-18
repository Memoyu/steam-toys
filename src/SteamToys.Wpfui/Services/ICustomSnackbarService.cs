namespace SteamToys.Wpfui.Services;

public interface ICustomSnackbarService : ISnackbarService
{
    void SetTextBlockControl(TextBlock textBlock);

     bool Info(string message);

    bool Warning(string message);

    bool Error(string message);
}
