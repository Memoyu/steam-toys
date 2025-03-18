
namespace SteamToys.Wpfui.Views.Pages;

/// <summary>
/// Settings.xaml 的交互逻辑
/// </summary>
public partial class Settings : INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public Settings(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
