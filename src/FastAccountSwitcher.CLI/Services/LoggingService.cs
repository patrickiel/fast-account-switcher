namespace FastAccountSwitcher.CLI.Services;

public class LoggingService
{
    private static string logFilePath => Path.Combine(Path.GetTempPath(), "FastAccountSwitcher.log");

    public static void LogException(Exception ex)
    {
        if (ex != null)
        {
            string logMessage = $"{DateTime.Now} [ERROR]: {ex}\n";

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

    public static void LogInfo(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            string logMessage = $"{DateTime.Now} [INFO]: {message}\n";

            try
            {
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to log info message: " + ex.ToString());
            }
        }
    }
}
