using AdminToys;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseLight : SynapseToyObject<LightSourceToy>
{
    public static LightSourceToy Prefab { get; internal set; }

    public override LightSourceToy ToyBase { get; }
    public override ObjectType Type => ObjectType.LightSource;
    public override void OnDestroy()
    {
        Map._synapseLights.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._lights.Remove(this);
    }
    
    public Color LightColor
    {
        get => ToyBase.LightColor;
        set => ToyBase.NetworkLightColor = value;
    }
    public float LightIntensity
    {
        get => ToyBase.LightIntensity;
        set => ToyBase.NetworkLightIntensity = value;
    }
    public float LightRange
    {
        get => ToyBase.LightRange;
        set => ToyBase.NetworkLightRange = value;
    }
    public bool LightShadows
    {
        get => ToyBase.LightShadows;
        set => ToyBase.NetworkLightShadows = value;
    }
    
    
    public SynapseLight(Color color, float lightIntensity, float range, bool shadows, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ToyBase = CreateLightSource(color, lightIntensity, range, shadows, position, rotation, scale);
        SetUp();
    }

    internal SynapseLight(SchematicConfiguration.LightSourceConfiguration configuration, SynapseSchematic schematic) :
        this(configuration.Color, configuration.LightIntensity, configuration.LightRange,
            configuration.LightShadows, configuration.Position, configuration.Rotation, configuration.Scale)
    {
        Parent = schematic;
        schematic._lights.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }
    
    private void SetUp()
    {
        Map._synapseLights.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
    private LightSourceToy CreateLightSource(Color color, float lightIntensity, float range,bool shadows, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var ot = Object.Instantiate(Prefab, position, rotation);
        NetworkServer.Spawn(ot.gameObject);

        ot.NetworkLightColor = color;
        ot.NetworkLightIntensity = lightIntensity;
        ot.NetworkLightRange = range;
        ot.NetworkLightShadows = shadows;

        ot.transform.position = position;
        ot.transform.rotation = rotation;
        ot.transform.localScale = scale;
        ot.NetworkScale = scale;

        return ot;
    }
}