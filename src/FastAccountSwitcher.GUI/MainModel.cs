namespace FastAccountSwitcher.GUI;

public class MainModel
{
    public MainModel()
    {
        AllowOnlyOneInstancePerUser();
        StartupManager.AddToStartup();
    }

    public static IReadOnlyList<Account> Accounts => AccountService.GetAccounts();

    private static void AllowOnlyOneInstancePerUser()
    {
        var currentProcess = Process.GetCurrentProcess();

        foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName)
                                       .Where(process => process.Id != currentProcess.Id && process.SessionId == currentProcess.SessionId))
        {
            process.Kill();
        }
    }
}
