using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

using Task = Microsoft.Win32.TaskScheduler.Task;

namespace FastAccountSwitcher.GUI;

public class StartupManager
{
    private const string AppPath = @"C:\Program Files\Fast Account Switcher\Fast Account Switcher.exe";
    private const string TaskName = "FastAccountSwitcherStartup";

    public StartupManager()
    {
        if (!IsRunningAtStartup())
        {
            AddToStartup();
        }
    }

    public static bool IsRunningAtStartup()
    {
        using TaskService ts = new();
        return ts.FindTask(TaskName) is not null;
    }

    public static void AddToStartup()
    {
        using TaskService ts = new();
        TaskDefinition td = ts.NewTask();
        td.RegistrationInfo.Description = "Start Fast Account Switcher with elevated rights at startup.";

        LogonTrigger logonTrigger = new() { UserId = null }; // Null UserId applies to all users
        td.Triggers.Add(logonTrigger);

        ExecAction action = new(AppPath);
        td.Actions.Add(action);

        td.Principal.GroupId = "Users"; // Set to SYSTEM to run for all users
        td.Principal.LogonType = TaskLogonType.Group;
        td.Principal.RunLevel = TaskRunLevel.Highest;

        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
        td.Settings.MultipleInstances = TaskInstancesPolicy.Parallel;

        ts.RootFolder.RegisterTaskDefinition(TaskName, td);
    }
}
