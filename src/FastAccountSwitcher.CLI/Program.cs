﻿namespace FastAccountSwitcher.CLI;

internal class Program
{
    static void Main(string[] args)
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
                    string? passwordInput = null;
                    while (string.IsNullOrEmpty(passwordInput))
                    {
                        Console.Write($"Password: ");
                        passwordInput = Console.ReadLine();
                    }

                    password = passwordInput;


                    while (rememberPassword.Key != ConsoleKey.Y && rememberPassword.Key != ConsoleKey.N)
                    {
                        Console.Write($"Remember password (Please be aware of the security risk)? (Y/n): ");
                        rememberPassword = Console.ReadKey();
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

    private static bool IsAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
