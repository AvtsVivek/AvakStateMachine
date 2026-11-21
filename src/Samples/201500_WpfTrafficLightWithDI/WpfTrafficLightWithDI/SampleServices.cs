namespace WpfTrafficLightWithDI;

using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Configuration;
using Avak.StateMachine.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

public static class SampleServices
{
    public static IServiceCollection AddWpfTrafficLightServices(
        this IServiceCollection services,
        string configurationPath)
    {
        var configuration = new XmlConfigurationLoader().LoadFromFile(configurationPath);
        services.AddSingleton(configuration);

        services.AddScoped<TrafficLightSession>();
        services.AddScoped<SignalOperationalGuard>();
        services.AddScoped<RecordTransitionCallback>();
        services.AddScoped<IGuardRegistry>(provider =>
        {
            var registry = new GuardRegistry();
            registry.Register("SignalOperational", provider.GetRequiredService<SignalOperationalGuard>());
            return registry;
        });
        services.AddScoped<ICallbackRegistry>(provider =>
        {
            var registry = new CallbackRegistry();
            registry.Register("RecordTransition", provider.GetRequiredService<RecordTransitionCallback>());
            return registry;
        });
        services.AddScoped<StateMachine>();
        services.AddScoped<MainWindowViewModel>();
        return services;
    }
}

public sealed class TrafficLightSession
{
    public bool IsOperational { get; set; } = true;
    public List<string> TransitionHistory { get; } = [];
}

public sealed class SignalOperationalGuard(TrafficLightSession session) : IGuard
{
    public bool Evaluate(TransitionContext context) => session.IsOperational;
}

public sealed class RecordTransitionCallback(TrafficLightSession session) : ITransitionCallback
{
    public void Execute(TransitionContext context)
    {
        session.TransitionHistory.Add($"{context.FromState} -> {context.ToState}");
    }
}
