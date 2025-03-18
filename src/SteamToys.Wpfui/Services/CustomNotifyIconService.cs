using System.Windows.Media.Imaging;

namespace SteamToys.Wpfui.Services;

public class CustomNotifyIconService : NotifyIconService
{
    public CustomNotifyIconService()
    {
        TooltipText = "SteamToys";

        // If this icon is not defined, the application icon will be used.
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/wpfui.png", UriKind.Absolute));

        //ContextMenu = new ContextMenu
        //{
        //    FontSize = 14d,
        //    Items =
        //    {
        //        new Wpf.Ui.Controls.MenuItem
        //        {
        //            Header = "Home",
        //            SymbolIcon = SymbolRegular.Library28,
        //            Tag = "home"
        //        },
        //        new Wpf.Ui.Controls.MenuItem
        //        {
        //            Header = "Save",
        //            SymbolIcon = SymbolRegular.Save28,
        //            Tag = "save"
        //        },
        //        new Wpf.Ui.Controls.MenuItem
        //        {
        //            Header = "Open",
        //            SymbolIcon = SymbolRegular.Folder28,
        //            Tag = "open"
        //        },
        //        new Separator(),
        //        new Wpf.Ui.Controls.MenuItem
        //        {
        //            Header = "Reload",
        //            SymbolIcon = SymbolRegular.ArrowClockwise28,
        //            Tag = "reload"
        //        },
        //    }
        //};

        //foreach (var singleContextMenuItem in ContextMenu.Items)
        //    if (singleContextMenuItem is System.Windows.Controls.MenuItem)
        //        ((System.Windows.Controls.MenuItem)singleContextMenuItem).Click += OnMenuItemClick;
    }

    protected override void OnLeftClick()
    {
        System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Tray event: {nameof(OnLeftClick)}", "Wpf.Ui.Demo");
    }

    private void OnMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Wpf.Ui.Controls.MenuItem menuItem)
            return;

        System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Tray clicked: {menuItem.Tag}", "Wpf.Ui.Demo");
    }
}
