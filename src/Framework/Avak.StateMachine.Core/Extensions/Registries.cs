namespace Avak.StateMachine.Core.Extensions;

/// <summary>
/// Registry of named guard implementations.
/// Applications inject this to provide the actual guard logic referenced by XML configuration.
/// </summary>
public interface IGuardRegistry
{
    /// <summary>
    /// Resolves a guard by name.
    /// </summary>
    /// <returns>The guard implementation, or null if not found (treat as allow).</returns>
    IGuard? Resolve(string guardName);
}

/// <summary>
/// Registry of named permission implementations.
/// Applications inject this to provide the actual permission logic referenced by XML configuration.
/// </summary>
public interface IPermissionRegistry
{
    /// <summary>
    /// Resolves a permission by name.
    /// </summary>
    /// <returns>The permission implementation, or null if not found (treat as allow).</returns>
    IPermission? Resolve(string permissionName);
}

/// <summary>
/// Registry of named callback implementations.
/// Applications inject this to provide the actual callback logic referenced by XML configuration.
/// </summary>
public interface ICallbackRegistry
{
    /// <summary>
    /// Resolves a callback by name.
    /// </summary>
    /// <returns>The callback implementation, or null if not found (no-op).</returns>
    ITransitionCallback? Resolve(string callbackName);
}

/// <summary>
/// Simple in-memory implementation of IGuardRegistry.
/// </summary>
public class GuardRegistry : IGuardRegistry
{
    private readonly Dictionary<string, IGuard> _guards = [];

    public void Register(string name, IGuard guard)
    {
        _guards[name] = guard ?? throw new ArgumentNullException(nameof(guard));
    }

    public IGuard? Resolve(string guardName)
    {
        _guards.TryGetValue(guardName, out var guard);
        return guard;
    }
}

/// <summary>
/// Simple in-memory implementation of IPermissionRegistry.
/// </summary>
public class PermissionRegistry : IPermissionRegistry
{
    private readonly Dictionary<string, IPermission> _permissions = [];

    public void Register(string name, IPermission permission)
    {
        _permissions[name] = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public IPermission? Resolve(string permissionName)
    {
        _permissions.TryGetValue(permissionName, out var permission);
        return permission;
    }
}

/// <summary>
/// Simple in-memory implementation of ICallbackRegistry.
/// </summary>
public class CallbackRegistry : ICallbackRegistry
{
    private readonly Dictionary<string, ITransitionCallback> _callbacks = [];

    public void Register(string name, ITransitionCallback callback)
    {
        _callbacks[name] = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public ITransitionCallback? Resolve(string callbackName)
    {
        _callbacks.TryGetValue(callbackName, out var callback);
        return callback;
    }
}
