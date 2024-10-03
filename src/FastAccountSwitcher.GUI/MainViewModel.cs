using System.Windows.Controls;

using FastAccountSwitcher.CLI.Services;
using FastAccountSwitcher.GUI.Properties;

namespace FastAccountSwitcher.GUI;

public partial class MainViewModel : ObservableObject
{
    private readonly MainModel model;
    private readonly AccountService accountService;
    private readonly PasswordService passwordService;

    public MainViewModel()
    {
        model = new MainModel();
        accountService = new AccountService();
        passwordService = new PasswordService();
        Accounts.CollectionChanged += (sender, e) => BuildMenu();

        var accounts = MainModel.Accounts.Select(account => new AccountViewModel(account, accountService, passwordService));

        foreach (var account in accounts)
        {
            Accounts.Add(account);
        }

        BuildMenu();
    }

    private void BuildMenu()
    {
        MenuItems.Clear();

        IEnumerable<AccountViewModel> accounts = Accounts.Where(a => !a.Active);

        foreach (var account in accounts)
        {
            MenuItems.Add(new MenuItem
            {
                Header = $"{account.UserName}{(account.CanExecuteSwitchAccount ? "" : " (not logged in)")}",
                Command = new RelayCommand(account.SwitchAccount),
            });
        }

        if (!accounts.Any())
        {
            MenuItems.Add(new MenuItem
            {
                Header = "No other logged in accounts",
                IsEnabled = false
            });
        }

        MenuItems.Add(new MenuItem { Header = new Separator(), IsEnabled = false });

        MenuItems.Add(new()
        {
            Header = "Exit",
            Command = new RelayCommand(Application.Current.Shutdown),
            Icon = new TextBlock
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe Fluent Icons"),
                Text = "\uE711",
                FontSize = 16
            }
        });
    }

    public ObservableCollection<AccountViewModel> Accounts { get; private set; } = [];

    public void RefreshAccounts()
    {
        Accounts.Clear();

        foreach (var account in MainModel.Accounts)
        {
            Accounts.Add(new AccountViewModel(account, accountService, passwordService));
        }
    }

    public void UltraFastSwitch()
    {
        GetUltraFastSwitchAccount()?.SwitchAccount();
    }

    private AccountViewModel? GetUltraFastSwitchAccount()
    {
        if (Accounts.Count == 1 && Accounts[0].CanExecuteSwitchAccount)
        {
            return Accounts[0];
        }

        string? lastSwitchedUsername = null;
        try
        {
            lastSwitchedUsername = Settings.Default.LastSwitchedUsername;
        }
        catch (Exception ex)
        {
            LoggingService.LogException(ex);
            LoggingService.LogInfo("Resetting settings");
            Settings.Default.Reset();
            Settings.Default.Save();

            // probably need to delete "%localappdata%\Fast_Account_Switcher" folder?

            try
            {
                lastSwitchedUsername = Settings.Default.LastSwitchedUsername;
            }
            catch (Exception ex2)
            {
                LoggingService.LogException(ex2);
                LoggingService.LogInfo("Resetting settings failed");
                return null;
            }
        }

        if (string.IsNullOrEmpty(lastSwitchedUsername))
        {
            return null;
        }

        return Accounts.FirstOrDefault(a => !a.Active && a.UserName.Equals(lastSwitchedUsername, StringComparison.InvariantCultureIgnoreCase) && a.CanExecuteSwitchAccount);
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = [];

    [ObservableProperty]
    private string taskBarIconPath = "";
}