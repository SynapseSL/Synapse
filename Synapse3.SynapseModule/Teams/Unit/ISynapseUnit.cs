using System.Collections.ObjectModel;

namespace Synapse3.SynapseModule.Teams.Unit;

public interface ISynapseUnit
{
    public ReadOnlyCollection<string> Units { get; }
    
    public UnitAttribute Attribute { get; set; }
    
    public string DefaultUnit { get; }

    public string GetNewUnit();

    public void AddUnit(string unit);

    public void AddUnit(string unit, int index);

    public void Load();
}