using System.Security;

using FastAccountSwitcher.CLI.Services;

namespace FastAccountSwitcher.CLI;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (!IsAdmin())
            {
                Console.WriteLine("Please run this program as an administrator.");
                return;
            }

            var action = args.Length > 0 ? args[0] : null;

            if (action == null)
            {
                Console.WriteLine("Usage: FastAccountSwitcher.CLI <action>");
                Console.WriteLine("  Actions:");
                Console.WriteLine("    getAccounts");
                Console.WriteLine("    switch <username>");
                return;
            }

            var accountService = new AccountService();
            var passwordService = new PasswordService();

            switch (action)
            {
                case "getAccounts":
                    var accounts = AccountService.GetAccounts();
                    foreach (var account in accounts)
                    {
                        Console.WriteLine($"{account.UserName} ({account.Id}) - {account.State}");
                    }
                    break;

                case "switch":
                    var username = args[1];
                    var password = passwordService.GetPassword(username);
                    ConsoleKeyInfo rememberPassword = default;

                    if (password == null)
                    {
                        password = new SecureString();
                        Console.Write("Password: ");
                        while (true)
                        {
                            var key = Console.ReadKey(intercept: true);
                            if (key.Key == ConsoleKey.Enter) break;

                            if (key.Key == ConsoleKey.Backspace)
                            {
                                if (password.Length > 0)
                                {
                                    password.RemoveAt(password.Length - 1);
                                    Console.Write("\b \b");
                                }
                            }
                            else
                            {
                                password.AppendChar(key.KeyChar);
                                Console.Write("*");
                            }
                        }
                        password.MakeReadOnly();
                        Console.WriteLine();

                        while (rememberPassword.Key != ConsoleKey.Y && rememberPassword.Key != ConsoleKey.N)
                        {
                            Console.Write("Remember password? (Y/n): ");
                            rememberPassword = Console.ReadKey();
                            Console.WriteLine();
                        }
                    }

                    var id = AccountService.GetAccounts().FirstOrDefault(a => a.UserName.Equals(username, StringComparison.OrdinalIgnoreCase))?.Id
                        ?? throw new InvalidOperationException($"Account not found: {username}");

                    accountService.SwitchAccount(id, password);

                    if (rememberPassword.Key == ConsoleKey.Y)
                    {
                        passwordService.SetPassword(username, password);
                    }

                    break;

                default:
                    Console.WriteLine($"Unknown action: {action}");
                    break;
            }

        }
        catch (Exception ex)
        {
            LoggingService.LogException(ex);
            Console.WriteLine(ex.Message);
        }
    }

    private static bool IsAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
