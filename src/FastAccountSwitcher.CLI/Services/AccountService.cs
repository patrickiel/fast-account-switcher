using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security;

namespace FastAccountSwitcher;

public class AccountService
{
    public AccountService()
    {
    }

    public static IReadOnlyList<Account> GetAccounts()
    {
        List<Account> loggedInAccounts = GetLoggedInAccounts();

        List<Account> notLoggedInAccounts = GetWindowsAccountNames()
            .Except(loggedInAccounts.Select(a => a.UserName), StringComparer.OrdinalIgnoreCase)
            .Select(username => new Account(username, "", "", "", "", "", false))
            .ToList();


        return [.. loggedInAccounts, .. notLoggedInAccounts];
    }

    private static List<string> GetWindowsAccountNames()
    {
        using PrincipalContext context = new(ContextType.Machine);

        UserPrincipal userPrincipal = new(context);
        PrincipalSearcher searcher = new(userPrincipal);
        List<string> toExclude = ["Administrator", "Guest", "DefaultAccount", "WDAGUtilityAccount"];

        return searcher.FindAll()
            .OfType<UserPrincipal>()
            .Select(user => user.SamAccountName)
            .Except(toExclude)
            .ToList();
    }

    private static List<Account> GetLoggedInAccounts()
    {
        var accounts = new List<Account>();

        Process process = new()
        {
            StartInfo = new()
            {
                FileName = "query",
                Arguments = "user",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        string header = lines[0];

        int usernameStart = header.IndexOf("USERNAME");
        int usernameEnd = header.IndexOf("SESSIONNAME");

        int sessionNameStart = header.IndexOf("SESSIONNAME");
        int sessionNameEnd = header.IndexOf("ID");

        int idStart = header.IndexOf("ID");
        int idEnd = header.IndexOf("STATE");

        int stateStart = header.IndexOf("STATE");
        int stateEnd = header.IndexOf("IDLE TIME");

        int idleTimeStart = header.IndexOf("IDLE TIME");
        int idleTimeEnd = header.IndexOf("LOGON TIME");

        int logonTimeStart = header.IndexOf("LOGON TIME");
        int logonTimeEnd = header.Length;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string username = line[usernameStart..usernameEnd].Trim();
            string sessionName = line[sessionNameStart..sessionNameEnd].Trim();
            string id = line[idStart..idEnd].Trim();
            string state = line[stateStart..stateEnd].Trim();
            string idleTime = line[idleTimeStart..idleTimeEnd].Trim();
            string logonTime = line[logonTimeStart..logonTimeEnd].Trim();

            accounts.Add(new Account(username, sessionName, id, state, idleTime, logonTime, true));
        }

        return accounts;
    }

    public void SwitchAccount(string id, SecureString password)
    {
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            throw new InvalidOperationException("You must run this program as an administrator to switch accounts.");
        }

        IntPtr passwordPtr = IntPtr.Zero;
        string insecurePassword = string.Empty;

        try
        {
            passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(password);
            insecurePassword = Marshal.PtrToStringUni(passwordPtr);

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "tscon",
                    Arguments = $"{id} /password:{insecurePassword}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read the standard output of the spawned process.
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // Wait for the process to exit.
            process.WaitForExit();

            // Check the exit code.
            if (process.ExitCode != 0)
            {
                throw new Exception($"tscon failed with exit code {process.ExitCode}: {error}");
            }
        }
        finally
        {
            if (passwordPtr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
            }

            // Clear the insecure password string from memory
            if (insecurePassword?.Length > 0)
            {
                insecurePassword = new string('\0', insecurePassword.Length);
            }
        }
    }
}
