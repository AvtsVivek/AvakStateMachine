namespace ConsoleWithDependencyInjection;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

public static class SampleServices
{
    public static IServiceCollection AddSampleServices(this IServiceCollection services, string configurationPath)
    {
        var configuration = new XmlConfigurationLoader().LoadFromFile(configurationPath);
        services.AddSingleton(configuration);

        services.AddScoped<SessionData>();
        services.AddScoped<ReadyToStartGuard>();
        services.AddScoped<RecordStartedCallback>();
        services.AddScoped<IGuardRegistry>(provider =>
        {
            var registry = new GuardRegistry();
            registry.Register("ReadyToStart", provider.GetRequiredService<ReadyToStartGuard>());
            return registry;
        });
        services.AddScoped<ICallbackRegistry>(provider =>
        {
            var registry = new CallbackRegistry();
            registry.Register("RecordStarted", provider.GetRequiredService<RecordStartedCallback>());
            return registry;
        });
        services.AddScoped<StateMachine>();
        return services;
    }
}

public sealed class SessionData
{
    public bool IsReady { get; set; }
    public List<string> Events { get; } = [];
}

public sealed class ReadyToStartGuard(SessionData session) : IGuard
{
    public bool Evaluate(TransitionContext context) => session.IsReady;
}

public sealed class RecordStartedCallback(SessionData session) : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        session.Events.Add($"{context.FromState} -> {context.ToState}");
    }
}
