namespace SteamToys.Wpfui.Services;

public class CustomSnackbarService : SnackbarService, ICustomSnackbarService
{
    private TextBlock _textBlock;

    public void SetTextBlockControl(TextBlock textBlock)
    {
        _textBlock = textBlock;
    }

    public bool Info(string message)
    {
        _textBlock.Text = message;
        return Show(string.Empty, string.Empty, SymbolRegular.Comment16, ControlAppearance.Success);
    }

    public bool Warning(string message)
    {
        _textBlock.Text = message;
        return Show(string.Empty, string.Empty, SymbolRegular.ErrorCircle16, ControlAppearance.Caution);
    }

    public bool Error(string message)
    {
        _textBlock.Text = message;
        return Show(string.Empty, string.Empty, SymbolRegular.ShieldError16, ControlAppearance.Danger);
    }

}
