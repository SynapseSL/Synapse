using System.Collections.Generic;
using System.Linq;
using Neuron.Core;
using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule;

public partial class Synapse
{
    public const int Major = 3;
    public const int Minor = 0;
    public const int Patch = 0;
    
    public const VersionType Type = VersionType.Beta;
    public const string SubVersion = "1.0";
    public const string BasedGameVersion = "11.2.1";
    
    /// <summary>
    /// Returns an instance of the specified object by either resolving it using
    /// ninject bindings (I.e. the object is already present in the ninject kernel)
    /// or by creating a new instance of the type using the ninject kernel making
    /// injection usable.
    /// </summary>
    public static T Get<T>() => Globals.Get<T>();
    
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
        
#if DEBUG
        if (Type != VersionType.None)
            version += $"-{Type}-{SubVersion}";
#endif

        return version;
    }
}