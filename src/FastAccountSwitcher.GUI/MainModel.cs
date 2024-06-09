using System.Linq;

namespace FastAccountSwitcher.GUI;

public class MainModel
{
    private readonly AccountService accountService = new();

    public MainModel()
    {
        AllowOnlyOneInstancePerUser();
    }

    public IReadOnlyList<Account> Accounts => AccountService.GetAccounts();

    public StartupManager StartupManager { get; } = new StartupManager();

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
