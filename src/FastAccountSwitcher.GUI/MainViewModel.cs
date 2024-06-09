using System.Windows.Controls;

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

        var accounts = model.Accounts.Select(account => new AccountViewModel(account, accountService, passwordService));

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

        SetTooltip();
    }

    private void SetTooltip()
    {
        var lastSwitchedAccount = GetLastSwitchedAccount();

        ToolTip = lastSwitchedAccount is null
            ? "Fast Account Switcher"
            : $"Click the left mouse button to switch to {lastSwitchedAccount.UserName}";
    }

    public ObservableCollection<AccountViewModel> Accounts { get; private set; } = [];

    public void RefreshAccounts()
    {
        Accounts.Clear();

        foreach (var account in model.Accounts)
        {
            Accounts.Add(new AccountViewModel(account, accountService, passwordService));
        }
    }

    public void UltraFastSwitch()
    {
        GetLastSwitchedAccount()?.SwitchAccount();
    }

    private AccountViewModel? GetLastSwitchedAccount()
    {
        var lastSwitchedUsername = Settings.Default.LastSwitchedUsername;
        if (string.IsNullOrEmpty(lastSwitchedUsername))
        {
            return null;
        }

        return Accounts.FirstOrDefault(a => !a.Active && a.UserName.Equals(lastSwitchedUsername, StringComparison.InvariantCultureIgnoreCase) && a.CanExecuteSwitchAccount);
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = [];

    [ObservableProperty]
    private string toolTip = "";

    [ObservableProperty]
    private string taskBarIconPath = "";
}