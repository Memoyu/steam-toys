using SteamToys.Controls;
using System.Collections.ObjectModel;

namespace SteamToys.ViewModels;

public partial class MainWindowViewModel : ObservableObject, INavigationAware
{
    bool _dataInitialized = false;

    [ObservableProperty] ObservableCollection<object> navigationItems = new();
    [ObservableProperty] ObservableCollection<object> navigationFooter = new();

    public MainWindowViewModel()
    {
        InitData();
    }

    private void InitData()
    {
        NavigationItems = new ObservableCollection<object>
        {
            new NavigationViewItem()
            {
                Content = "主页",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Dashboard)
            },
            new NavigationViewItem()
            {
                Content = "参数",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataArea24 },
                TargetPageType = typeof(Parameter),
            }
        };

        NavigationFooter = new ObservableCollection<object>
        {
            new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Settings)
            }
        };

        _dataInitialized = true;
    }

    public void OnNavigatedTo()
    {
        if (!_dataInitialized)
            InitData();
    }

    public void OnNavigatedFrom()
    {
    }
}
