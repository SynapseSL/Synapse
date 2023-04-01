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

    public const string SubVersion = "1.2";
    public const string BasedGameVersion = "12.0.2";

    /// <summary>
    /// Returns an instance of the specified object by either resolving it using
    /// ninject bindings (I.e. the object is already present in the ninject kernel)
    /// or by creating a new instance of the type using the ninject kernel making
    /// injection usable.
    /// </summary>
    /// <exception cref=""></exception>
    public static T Get<T>()
    {
        if (Exist<T>()) return Globals.Kernel.Get<T>();
        SynapseLogger<Synapse>.Warn(typeof(T).Name + " was requested but doesn't exist inside the kernel.");
        return default;
    }

    public static T GetOrCreate<T>(bool bind = true)
        => Exist<T>() ? Get<T>() : Create<T>(bind);

    public static T Create<T>(bool bind)
    {
        var instance = Globals.Get<T>();
        if (bind) Bind(instance);
        Inject(instance);
        return instance;
    }

    public static void Bind<T>(T instance) => Globals.Kernel.Bind<T>().ToConstant(instance).InSingletonScope();
    
    public static bool Exist<T>() => Globals.Kernel.GetBindings(typeof(T)).Any();

    /// <inheritdoc cref="Get{T}"/>
    public static object Get(Type type)
    {
        if (Exist(type)) return Globals.Kernel.Get(type);
        SynapseLogger<Synapse>.Warn(type.Name + " was requested but doesn't exist inside the kernel.");
        return null;
    }

    public static object GetOrCreate(Type type, bool bind = true)
        => Exist(type) ? Get(type) : Create(type, bind);

    public static object Create(Type type, bool bind)
    {
        var instance = Globals.Kernel.Get(type);
        if (bind) Bind(type, instance);
        Inject(instance);
        return instance;
    }

    public static void Bind(Type type, object instance) =>
        Globals.Kernel.Bind(type).ToConstant(instance).InSingletonScope();

    public static bool Exist(Type type) => Globals.Kernel.GetBindings(type).Any();
    
    public static void Inject(object instance) => Globals.Kernel.Inject(instance);

    /// <summary>
    /// Creates the Listener and Register all EventHandlers
    /// </summary>
    public static T GetEventHandler<T>() where T : Listener
    {
        var listener = GetOrCreate<T>();
        Get<EventManager>().RegisterListener(listener);
        return listener;
    }

    /// <inheritdoc cref="GetEventHandler{T}"/>
    public static Listener GetEventHandler(Type listenerType)
    {
        if (typeof(Listener) == listenerType || !typeof(Listener).IsAssignableFrom(listenerType))
            throw new ArgumentException(
                "listenerType of GetEventHandler was called with an invalid type that can not be casted to a Listener");

        var listenerRaw = GetOrCreate(listenerType);
        if (listenerRaw is not Listener listener)
            throw new ArgumentException(
                "listenerType of GetEventHandler was called with an invalid type that can not be casted to a Listener");
        
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