using System.Runtime.InteropServices;

namespace FastAccountSwitcher.GUI;
public partial class WorkstationHelper
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LockWorkStation();

    public static void Lock()
        => LockWorkStation();
}
