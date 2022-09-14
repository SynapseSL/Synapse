using System.Collections.ObjectModel;
using System.Linq;
using Respawning;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Teams.Unit;

public abstract class SynapseUnit : ISynapseUnit
{
    private readonly UnitService _unitService;
    
    protected SynapseUnit()
    {
        _unitService = Synapse.Get<UnitService>();
        Synapse.Get<RoundEvents>().Waiting.Subscribe(InitialiseDefaultUnit);
    }

    public ReadOnlyCollection<string> Units => RespawnManager.Singleton.NamingManager.AllUnitNames
        .Where(x => x.SpawnableTeam == Attribute.Id)
        .Select(x => x.UnitName).ToList().AsReadOnly();

    public UnitAttribute Attribute { get; set; }

    public abstract string DefaultUnit { get; }

    public virtual string GetNewUnit()
    {
        var unit = GenerateNewName();
        AddUnit(unit);
        return unit;
    }

    protected abstract string GenerateNewName();

    public void AddUnit(string unit)
        => _unitService.AddUnit(unit, Attribute.Id, int.MaxValue);

    public void AddUnit(string unit, int index)
        => _unitService.AddUnit(unit, Attribute.Id, index);

    public virtual void Load() { }

    protected virtual void InitialiseDefaultUnit(RoundWaitingEvent _)
        => AddUnit(DefaultUnit);
}