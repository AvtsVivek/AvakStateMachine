namespace ConsoleWithDependencyInjection;

using Avak.StateMachine.Core;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    private static void Main()
    {
        var services = new ServiceCollection();
        string configurationPath = Path.Combine(AppContext.BaseDirectory, "StateMachineConfiguration.xml");
        services.AddSampleServices(configurationPath);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        using var scope = provider.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<SessionData>();
        var machine = scope.ServiceProvider.GetRequiredService<StateMachine>();

        session.IsReady = true;
        var result = machine.Trigger("Start", source: "Console");
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        Console.WriteLine($"Current state: {machine.CurrentState}");
        Console.WriteLine($"Recorded event: {session.Events.Single()}");
    }
}
