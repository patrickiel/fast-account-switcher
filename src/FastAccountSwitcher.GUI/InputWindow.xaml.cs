using Microsoft.Win32;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace FastAccountSwitcher.GUI;
/// <summary>
/// Interaction logic for InputBox.xaml
/// </summary>
public partial class InputWindow : FluentWindow
{
    public InputWindow()
    {
        InitializeComponent();

        SystemEvents.UserPreferenceChanged += (_, _) => ApplyThemeChanges();

        SystemThemeWatcher.Watch(this, WindowBackdropType.Auto, true);
        ViewModel.CancelAction = Close;
    }

    private static void ApplyThemeChanges()
    {
        ApplicationTheme theme = ThemeInfo.IsLightTheme() ? ApplicationTheme.Light : ApplicationTheme.Dark;
        ApplicationThemeManager.Apply(theme, WindowBackdropType.Auto, true);
    }

    public InputViewModel ViewModel => (InputViewModel)DataContext;
}
