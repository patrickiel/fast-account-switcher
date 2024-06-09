using System.Text.Json;

namespace FastAccountSwitcher;

public record Credential(string UserName, string Password);

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
        File.WriteAllText(credentialsFilePath, JsonSerializer.Serialize(credentials));
    }

    public string? GetPassword(string userName)
    {
        List<Credential> credentials = ReadCredentials();
        var credential = credentials.FirstOrDefault(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

        return credential?.Password;
    }

    public void SetPassword(string userName, string password)
    {
        List<Credential> credentials = ReadCredentials();

        var existingCredential = credentials.FirstOrDefault(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (existingCredential != null)
        {
            credentials.Remove(existingCredential);
        }

        credentials.Add(new Credential(userName, password));
        credentials = credentials.Distinct().OrderBy(c => c.UserName).ToList();

        var directoryPath = Path.GetDirectoryName(credentialsFilePath);

        if (directoryPath is not null && !Directory.Exists(Path.GetDirectoryName(credentialsFilePath)))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(credentialsFilePath, JsonSerializer.Serialize(credentials));
    }

    private List<Credential> ReadCredentials()
        => File.Exists(credentialsFilePath)
            ? JsonSerializer.Deserialize<List<Credential>>(File.ReadAllText(credentialsFilePath)) ?? []
            : [];
}