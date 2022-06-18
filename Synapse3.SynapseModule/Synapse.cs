using System.Collections.Generic;
using System.Linq;
using Neuron.Core;
using UnityEngine;

namespace Synapse3.SynapseModule;

public class Synapse
{
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
    public static List<TObject> GetObjectsOf<TObject>() where TObject : Object
    {
        return Object.FindObjectsOfType<TObject>().ToList();
    }

    /// <summary>
    /// Returns an instance of the specified object from Unity
    /// </summary>
    public static TObject GetObjectOf<TObject>() where TObject : Object
    {
        return Object.FindObjectOfType<TObject>();
    }
}