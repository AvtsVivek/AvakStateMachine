namespace WpfTrafficLightWithDI;

using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _scope;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        services.AddWpfTrafficLightServices(configurationPath);
        services.AddScoped<MainWindow>();

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        _scope = _serviceProvider.CreateScope();
        MainWindow = _scope.ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scope?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
