using Microsoft.Win32;

namespace FastAccountSwitcher.GUI;

internal static class ThemeInfo
{
    public static bool IsLightTheme()
    {
        var key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        var value = "SystemUsesLightTheme";

        using var registryKey = Registry.CurrentUser.OpenSubKey(key);

        return registryKey?.GetValue(value) is int lightTheme && lightTheme == 1;
    }
}
