using System.ComponentModel;
using System.Windows.Controls;

using Hardcodet.Wpf.TaskbarNotification;

using Microsoft.Win32;

using Wpf.Ui.Appearance;

namespace FastAccountSwitcher.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IDisposable
{
    public MainWindow()
    {
        InitializeComponent();
        HideWindow();
        InitializeThemeWatcher();
        ApplyThemeChanges();
        SystemEvents.UserPreferenceChanged += (_, _) => ApplyThemeChanges();
    }

    private void InitializeThemeWatcher()
    {
        SystemThemeWatcher.Watch(this, Wpf.Ui.Controls.WindowBackdropType.Auto, true);
    }

    private void ApplyThemeChanges()
    {
        bool isLightTheme = ThemeInfo.IsLightTheme();
        var taskbarIcon = GetIcon();

        if (taskbarIcon != null)
        {
            taskbarIcon.Icon = isLightTheme
                ? Properties.Resources.icon_light
                : Properties.Resources.icon_dark;
        }

        ApplicationTheme theme = isLightTheme ? ApplicationTheme.Light : ApplicationTheme.Dark;
        ApplicationThemeManager.Apply(theme, Wpf.Ui.Controls.WindowBackdropType.Acrylic, true);
    }

    private TaskbarIcon? GetIcon()
    {
        return FindName("mainGrid") is Grid grid
            ? grid.FindName("taskbarIcon") as TaskbarIcon
            : null;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        HideWindow();
    }

    private void TaskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.UltraFastSwitch();
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void HideWindow()
    {
        Visibility = Visibility.Hidden;
        ShowInTaskbar = false;
    }

    private void TaskbarIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.RefreshAccounts();
        }
    }
    public void Dispose()
    {
        SystemEvents.UserPreferenceChanged -= (_, _) => ApplyThemeChanges();
    }
}
