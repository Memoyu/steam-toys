namespace SteamToys.Wpfui.ViewModels;

public class SettingsViewModel : ObservableObject, INavigationAware
{
    public SettingsViewModel()
    {
        
    }

    public void OnNavigatedTo()
    {
        System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Setting");
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Setting");
    }
}
