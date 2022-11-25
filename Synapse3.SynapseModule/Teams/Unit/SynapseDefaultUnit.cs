using System.Collections.Generic;

namespace Synapse3.SynapseModule.Teams.Unit;

public abstract class SynapseDefaultUnit : SynapseUnit
{
    private readonly List<string> _usedNames = new();
    private string _defaultUnit;

    public override string DefaultUnit
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_defaultUnit))
                _defaultUnit = GenerateNewName();

            return _defaultUnit;
        }
    }
    
    protected override string GenerateNewName()
    {
        var unit = string.Empty;
        do
        {
            unit = PossibleNames[UnityEngine.Random.Range(0, PossibleNames.Length)];
            if (GenerateNumbers)
            {
                var number = UnityEngine.Random.Range(1, UptoNumber + 1).ToString("00");
                unit += "-" + number;
            }
                
        } while (_usedNames.Contains(unit));
        _usedNames.Add(unit);
        return unit;
    }
    
    protected abstract string[] PossibleNames { get; }
    protected virtual bool GenerateNumbers { get; } = true;
    protected virtual int UptoNumber { get; } = 20;
}