using System.DirectoryServices.AccountManagement;

using FastAccountSwitcher.CLI.Services;

using Microsoft.Win32.TaskScheduler;

namespace FastAccountSwitcher.GUI;

public class StartupManager
{
    private const string AppPath = @"C:\Program Files\Fast Account Switcher\Fast Account Switcher.exe";
    private const string TaskName = "FastAccountSwitcher";

    public StartupManager()
    {
    }

    public static void AddToStartup()
    {
        RemoveFromStartup();

        using TaskService ts = new();

        TaskFolder folder = ts.RootFolder.CreateFolder(TaskName);

        List<(string ConnectedServer, string AccountName)> users = GetUsers();

        foreach (var user in users)
        {
            using var task = CreateTask(folder, ts, user);
        }
    }

    private static List<(string ConnectedServer, string AccountName)> GetUsers()
    {
        using PrincipalContext context = new(ContextType.Machine);
        using var searcher = new PrincipalSearcher(new UserPrincipal(context));

        return searcher.FindAll()
                       .OfType<UserPrincipal>()
                       .Where(userPrincipal => !userPrincipal.IsAccountLockedOut() && userPrincipal.Enabled == true)
                       .Select(userPrincipal => (context.ConnectedServer, userPrincipal.SamAccountName)).ToList();
    }

    private static Microsoft.Win32.TaskScheduler.Task CreateTask(TaskFolder folder, TaskService ts, (string ConnectedServer, string AccountName) user)
    {
        string userId = user.ConnectedServer + "\\" + user.AccountName;

        TaskDefinition td = ts.NewTask();
        td.RegistrationInfo.Description = "Start Fast Account Switcher with elevated rights at startup.";

        LogonTrigger logonTrigger = new()
        {
            UserId = userId,
            Delay = TimeSpan.FromSeconds(3)
        };

        td.Triggers.Add(logonTrigger);

        ExecAction action = new(AppPath);
        td.Actions.Add(action);

        td.Principal.Id = userId;
        td.Principal.LogonType = TaskLogonType.Group;
        td.Principal.RunLevel = TaskRunLevel.Highest;

        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.StopIfGoingOnBatteries = false;
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
        td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

        return folder.RegisterTaskDefinition("Autorun for " + user.AccountName, td, TaskCreation.CreateOrUpdate, userId, null, TaskLogonType.None);
    }

    private static void RemoveFromStartup()
    {
        using TaskService ts = new();
        if (ts.RootFolder.SubFolders.Exists(TaskName))
        {
            var folder = ts.RootFolder.SubFolders[TaskName];
            DeleteFolderAndItsContent(folder);
        }
    }

    private static void DeleteFolderAndItsContent(TaskFolder folder)
    {
        foreach (var task in folder.Tasks)
        {
            folder.DeleteTask(task.Name);
        }

        foreach (var subFolder in folder.SubFolders)
        {
            DeleteFolderAndItsContent(subFolder);
        }

        folder.Parent?.DeleteFolder(folder.Name);
    }
}
