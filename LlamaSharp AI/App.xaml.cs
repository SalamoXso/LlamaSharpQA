using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handling
        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Unhandled error: {args.Exception.Message}", "Error");
            args.Handled = true;
        };
    }
}