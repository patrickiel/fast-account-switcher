using FastAccountSwitcher.CLI.Services;

namespace FastAccountSwitcher.GUI;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, e) =>
        {
            LoggingService.LogException(e.Exception);
            e.Handled = true;
            Current.Shutdown();
        };
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LoggingService.LogException(exception);
        }
        Current.Shutdown();
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LoggingService.LogException(e.Exception);
        e.SetObserved();
        Current.Shutdown();
    }
}