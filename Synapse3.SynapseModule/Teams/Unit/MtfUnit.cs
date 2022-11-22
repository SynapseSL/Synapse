using Respawning.NamingRules;
using UnityEngine;

namespace Synapse3.SynapseModule.Teams.Unit;

[Unit(
    Id = 2,
    AssignedTeam = 2,
    DefaultRolesInUnit = new []
    {
        (uint)RoleType.FacilityGuard,
        (uint)RoleType.NtfCaptain,
        (uint)RoleType.NtfPrivate,
        (uint)RoleType.NtfSergeant,
        (uint)RoleType.NtfSpecialist,
    }
    )]
public class MtfUnit : SynapseUnit
{
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
            var code = NineTailedFoxNamingRule.PossibleCodes[Random.Range(0, NineTailedFoxNamingRule.PossibleCodes.Length)];
            var unite = Random.Range(1, 21).ToString("00");

            unit = $"{code}-{unite}";
        } while (UnitNamingRule.UsedCombinations.Contains(unit));

        UnitNamingRule.UsedCombinations.Add(unit);
        return unit;
    }
}