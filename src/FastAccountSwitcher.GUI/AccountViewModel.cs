namespace FastAccountSwitcher.GUI;

public partial class AccountViewModel(Account account, AccountService accountService, PasswordService passwordService)
{
    public string UserName => account.UserName;

    public string Id => account.Id;

    public bool Active => account.State == "Active";

    public bool IsLoggedIn => account.IsLoggedIn;

    public bool CanExecuteSwitchAccount => !Active && IsLoggedIn;

    [RelayCommand(CanExecute = nameof(CanExecuteSwitchAccount))]
    public void SwitchAccount()
    {
        if (!account.IsLoggedIn)
        {
            WorkstationHelper.Lock();
            return;
        }

        string? password = passwordService.GetPassword(UserName);

        if (password is null)
        {
            var inputWindow = new InputWindow();
            inputWindow.Show();

            inputWindow.ViewModel.OkAction = (bool rembemberPassword, string inputText) =>
            {
                password = inputText;

                inputWindow.Close();
                Switch(rembemberPassword, password);
            };

            return;
        }

        Switch(false, password);
    }

    private void Switch(bool rembemberPassword, string password)
    {
        try
        {
            accountService.SwitchAccount(Id, password);

            if (rembemberPassword)
            {
                passwordService.SetPassword(UserName, password);
            }

            Properties.Settings.Default.LastSwitchedUsername = UserName;
            Properties.Settings.Default.Save();
        }
        catch (Exception)
        {
            passwordService.DeletePassword(UserName);
            _ = new Wpf.Ui.Controls.MessageBox()
            {
                Content = "Failed to switch account!\nPlease try again and make sure the password is correct.",
                Title = "Failed to switch account!",
                CloseButtonText = "Ok",
            }.ShowDialogAsync();
        }
    }
}
