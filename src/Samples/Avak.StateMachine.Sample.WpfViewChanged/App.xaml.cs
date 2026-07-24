using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Sample.WpfViewChanged.Infra;
using Avak.StateMachine.Sample.WpfViewChanged.ViewModels;
using Avak.StateMachine.Sample.WpfViewChanged.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Avak.StateMachine.Sample.WpfViewChanged
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppHost = Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<IXmlKeys, XmlKeys>();

                // 2. Register the delegate as a dependency
                // Use the [IServiceProvider](https://microsoft.com) to resolve the service first
                services.AddTransient<StateDependencyObjectFinder>(serviceProvider =>
                {
                    StateDependencyProvider stateDependencyProvider = serviceProvider.GetRequiredService<StateDependencyProvider>();
                    return stateDependencyProvider.StateDependencyTypeFinderImplimentation; // Returns the method group
                });

                services.AddSingleton<IStateMachineManager, StateMachineManager>();
                services.AddSingleton<StateDependencyProvider>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<UserControl1ViewModel>();
                services.AddSingleton<UserControl2ViewModel>();
                services.AddSingleton<UserControl3ViewModel>();
                services.AddSingleton<UserControl4ViewModel>();

            }).Build();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();
            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm!.DataContext = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            startupForm!.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
        public static IHost? AppHost { get; private set; }
    }
}
