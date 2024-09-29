using FastAccountSwitcher.CLI.Services;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text.Json;

namespace FastAccountSwitcher;

public record Credential(string UserName, string ProtectedPassword);

public class PasswordService
{
    private readonly string credentialsFilePath;

    public PasswordService()
    {
        credentialsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastAccountSwitcher", "credentials.json");
    }

    public void DeletePassword(string userName)
    {
        var credentials = ReadCredentials().Where(c => !c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        SaveCredentials(credentials);
    }

    public SecureString? GetPassword(string userName)
    {
        List<Credential> credentials = ReadCredentials();
        var credential = credentials.FirstOrDefault(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

        if(credential is null)
        {
            return null;
        }

        try
        {
            return UnprotectPassword(credential.ProtectedPassword);
        }
        catch (Exception ex)
        {
            LoggingService.LogException(ex);
            return null;
        }
    }

    public void SetPassword(string userName, SecureString password)
    {
        List<Credential> credentials = ReadCredentials();

        var existingCredential = credentials.FirstOrDefault(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (existingCredential != null)
        {
            credentials.Remove(existingCredential);
        }

        var protectedPassword = ProtectPassword(password);
        credentials.Add(new Credential(userName, protectedPassword));
        credentials = credentials.Distinct().OrderBy(c => c.UserName).ToList();

        SaveCredentials(credentials);
    }

    private List<Credential> ReadCredentials()
        => File.Exists(credentialsFilePath)
            ? JsonSerializer.Deserialize<List<Credential>>(File.ReadAllText(credentialsFilePath)) ?? new List<Credential>()
            : [];

    private void SaveCredentials(IEnumerable<Credential> credentials)
    {
        var directoryPath = Path.GetDirectoryName(credentialsFilePath);

        if (directoryPath is not null && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(credentialsFilePath, JsonSerializer.Serialize(credentials));
    }

    private static string ProtectPassword(SecureString securePassword)
    {
        IntPtr bstr = IntPtr.Zero;
        try
        {
            bstr = Marshal.SecureStringToBSTR(securePassword);
            byte[] plainBytes = new byte[securePassword.Length * 2];
            Marshal.Copy(bstr, plainBytes, 0, plainBytes.Length);
            byte[] protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }
        finally
        {
            if (bstr != IntPtr.Zero)
            {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }
    }

    private static SecureString UnprotectPassword(string protectedText)
    {
        byte[] protectedBytes = Convert.FromBase64String(protectedText);
        byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);

        SecureString securePassword = new();
        foreach (char c in Encoding.Unicode.GetChars(plainBytes))
        {
            securePassword.AppendChar(c);
        }

        securePassword.MakeReadOnly();
        Array.Clear(plainBytes, 0, plainBytes.Length);
        return securePassword;
    }
}
