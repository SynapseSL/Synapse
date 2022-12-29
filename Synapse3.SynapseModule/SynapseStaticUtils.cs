using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core;
using Neuron.Core.Events;
using Ninject;
using Synapse3.SynapseModule.Enums;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule;

public partial class Synapse
{
    public const int Major = 3;
    public const int Minor = 0;
    public const int Patch = 0;
    
#if CUSTOM_VERSION
public const VersionType Type = VersionType.Beta;
#elif DEBUG
public const VersionType Type = VersionType.Debug;
#elif DEV
public const VersionType Type = VersionType.Dev;
#elif MAIN_RELEASE
    public const VersionType Type = VersionType.None;
#endif

    public const string SubVersion = "";
    public const string BasedGameVersion = "12.0.0";
    
    /// <summary>
    /// Returns an instance of the specified object by either resolving it using
    /// ninject bindings (I.e. the object is already present in the ninject kernel)
    /// or by creating a new instance of the type using the ninject kernel making
    /// injection usable.
    /// </summary>
    /// <exception cref=""></exception>
    public static T Get<T>() => Globals.Get<T>();

    /// <inheritdoc cref="Get{T}"/>
    public static object Get(Type type) => Globals.Kernel.Get(type);

    /// <summary>
    /// Returns an instance of the specified object by either resolving it using
    /// ninject bindings (I.e. the object is already present in the ninject kernel)
    /// or by creating and binding a new instance of the type using the ninject kernel making
    /// injection usable.
    /// </summary>
    public static T GetAndBind<T>()
    {
        var returning = Get<T>();
        Globals.Kernel.Bind<T>().ToConstant(returning).InSingletonScope();
        return returning;
    }
    
    /// <inheritdoc cref="GetAndBind{T}"/>
    public static object GetAndBind(Type type)
    {
        var returning = Get(type);
        Globals.Kernel.Bind(type).ToConstant(returning).InSingletonScope();
        return returning;
    }

    /// <summary>
    /// Creates the Listener and Register all EventHandlers
    /// </summary>
    public static T GetEventHandler<T>() where T : Listener
    {
        var listener = GetAndBind<T>();
        Get<EventManager>().RegisterListener(listener);
        return listener;
    }

    /// <inheritdoc cref="GetEventHandler{T}"/>
    public static Listener GetEventHandler(Type listenerType)
    {
        if (typeof(Listener) == listenerType || !typeof(Listener).IsAssignableFrom(listenerType))
            throw new ArgumentException(
                "listenerType of GetEventHandler was called with an invalid type that can not be casted to a Listener");

        var listenerRaw = Get(listenerType);
        if (listenerRaw is not Listener listener)
            throw new ArgumentException(
                "listenerType of GetEventHandler was called with an invalid type that can not be casted to a Listener");

        Globals.Kernel.Bind(listenerType).ToConstant(listenerRaw).InSingletonScope();
        
        Get<EventManager>().RegisterListener(listener);
        return listener;
    }
    
    
    
    /// <summary>
    /// Returns an List of all instances of the specified object from Unity
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static List<TObject> GetObjects<TObject>() where TObject : Object => Object.FindObjectsOfType<TObject>().ToList();

    /// <summary>
    /// Returns an instance of the specified object from Unity
    /// </summary>
    public static TObject GetObject<TObject>() where TObject : Object => Object.FindObjectOfType<TObject>();

    /// <summary>
    /// Creates a string with the full combined version of Synapse
    /// </summary>
    public static string GetVersion()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        
#if CUSTOM_VERSION
        if (Type != VersionType.None)
            version += $"-{Type}-{SubVersion}";
#endif
#if DEBUG
        version += "-Debug";
#endif

        return version;
    }
}