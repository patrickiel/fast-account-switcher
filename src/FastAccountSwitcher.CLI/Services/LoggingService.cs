namespace FastAccountSwitcher.CLI.Services;

public class LoggingService
{
    public static void LogException(Exception ex)
    {
        if (ex != null)
        {
            string logFilePath = Path.Combine(Path.GetTempPath(), "FastAccountSwitcher.log"); // Change this path as needed
            string logMessage = $"{DateTime.Now}: {ex}\n";

            try
            {
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine("Failed to log exception: " + logEx.ToString());
            }
        }
    }
}
