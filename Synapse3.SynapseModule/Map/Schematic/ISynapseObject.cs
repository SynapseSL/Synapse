using System.Collections.Generic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public interface ISynapseObject
{
    /// <summary>
    /// Custom Data that can be stored with a key
    /// </summary>
    public Dictionary<string, object> ObjectData { get; set; }

    /// <summary>
    /// A list of all CustomAttributes that should be applied to the Object
    /// </summary>
    public List<string> CustomAttributes { get; set; }

    /// <summary>
    /// The Current Position of the Object
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// The Current Rotation of the Object
    /// </summary>
    public Quaternion Rotation { get; set; }

    /// <summary>
    /// The Current Scale of the Object
    /// </summary>
    public Vector3 Scale { get; set; }

    /// <summary>
    /// The underlying GameObject which is managed by the SynapseObject
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// The Rigidbody of the Gameobject which calculates the physics
    /// </summary>
    public Rigidbody Rigidbody { get; }

    /// <summary>
    /// The Synapse ObjectType
    /// </summary>
    public ObjectType Type { get; }
    
    public ISynapseObject Parent { get; set; }
    
    public ISynapseObject RootParent { get; }

    /// <summary>
    /// Removes this Object parent
    /// </summary>
    public void RemoveParent();

    /// <summary>
    /// Activates gravity for this Object if possible
    /// </summary>
    public void ApplyPhysics();

    /// <summary>
    /// Destroys the GameObject
    /// </summary>
    public void Destroy();

    /// <summary>
    /// Method which is executed before the Object is destroyed. Don't call it manually
    /// </summary>
    public void OnDestroy();

    public void HideFromAll();

    public void ShowAll();
    
    public void HideFromPlayer(SynapsePlayer player);

    public void ShowPlayer(SynapsePlayer player);
}