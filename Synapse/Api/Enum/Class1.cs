using System.Diagnostics.CodeAnalysis;

namespace Synapse.Api.Enums
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Effect
    {
        /// <summary>
        /// The Player can't open his inventory and reload his weapons
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Amnesia,
        /// <summary>
        /// Quickly drains stamina then health if there is none left
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Asphyxiated,
        /// <summary>
        /// Damage-over time starting high and getting low. Ticks every 5s.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Bleeding,
        /// <summary>
        /// Applies extreme screen blur
        /// </summary>
        Blinded,
        /// <summary>
        /// Slightly increases all damage taken
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Burned,
        /// <summary>
        /// Blurs the screen as the Player turns
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Concussed,
        /// <summary>
        /// Teleports to the pocket dimension and drains hp until he escapes
        /// </summary>
        /// <remarks>1 = Enabled</remarks>
        Corroding,
        /// <summary>
        /// Heavily muffles all sounds
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Deafened,
        /// <summary>
        /// Remove 10% of max health each second
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Decontaminating,
        /// <summary>
        /// Slows all movement
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Disabled,
        /// <summary>
        /// Prevents all movement
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Ensnared,
        /// <summary>
        /// Laves stamina capacity and regeneration rate
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Exhausted,
        /// <summary>
        /// Flash the Player
        /// </summary>
        /// <remarks>0 = Disabled, 1-244 = time in ms 255 = forever</remarks>
        Flashed,
        /// <summary>
        /// Sprinting drains 2 hp/s
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Hemorrhage,
        /// <summary>
        /// Infinite stamina
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Invigorated,
        /// <summary>
        /// Slightly increases stamina consumption
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Panic,
        /// <summary>
        /// Damage-over time starting low and rising high. Ticks every 5s.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Poisoned,
        /// <summary>
        /// The Player will walk faster
        /// </summary>
        /// <remarks>0 = Disabled, 1 = 1xCola, 2 = 2xCola, 3 = 3xCola, 4 = 4xCola</remarks>
        Scp207,
        /// <summary>
        /// The Player cant be seen by other entities. He need do activate Scp268 in his inventory
        /// </summary>
        /// <remarks>0 = Disabled,1 = Enabled</remarks>
        Scp268,
        /// <summary>
        /// Slows down players but not SCP's
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        SinkHole,
        /// <summary>
        /// The vision of SCP-939
        /// </summary>
        /// <remarks>0 = Disabled, 1 = OnlyMarker, 2 = Only Screen, 3 = Everything</remarks>
        Visuals939
    }
}
