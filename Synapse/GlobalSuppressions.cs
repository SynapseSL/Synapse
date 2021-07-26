// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Benennungsstile", Justification = "The field was intented to be private but was changed later and would now break some plugins when changed", Scope = "member", Target = "~P:Synapse.Api.Items.SynapseItem.pickup")]
[assembly: SuppressMessage("Style", "IDE1006:Benennungsstile", Justification = "The field was intented to be private but was changed later and would now break some plugins when changed", Scope = "member", Target = "~P:Synapse.Api.Items.SynapseItem.itemInfo")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Called by Unity", Scope = "member", Target = "~M:Synapse.Api.Events.EventHandler.KeyPress(Synapse.Api.Events.SynapseEventArguments.PlayerKeyPressEventArgs)")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "For Debug Only", Scope = "member", Target = "~M:Synapse.Api.Player.Update")]
