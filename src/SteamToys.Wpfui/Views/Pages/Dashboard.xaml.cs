namespace SteamToys.Wpfui.Views.Pages;

/// <summary>
/// Dashboard.xaml 的交互逻辑
/// </summary>
public partial class Dashboard : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public Dashboard(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
