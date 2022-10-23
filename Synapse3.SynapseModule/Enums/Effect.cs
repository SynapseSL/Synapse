﻿namespace Synapse3.SynapseModule.Enums;


        public enum Effect
    {
        /// <summary>
        /// The Player can't open their inventory and reload their weapons
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Amnesia,
        /// <summary>
        /// Quickly drains stamina then health if there is none left
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Asphyxiated,
        /// <summary>
        /// Decreasing damage over time. Ticks every 5s.
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
        /// Reduces player vision and weapon accuracy. Prevents Hume Shield from regenerating. Humans take damage overtime.
        /// </summary>
        Hypothermia,
        /// <summary>
        /// Infinite stamina
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Invigorated,
        /// <summary>
        /// Ascending damage over time. Ticks every 5s.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Poisoned,
        /// <summary>
        /// The Player will walk faster
        /// </summary>
        /// <remarks>0 = Disabled, 1 = 1xCola, 2 = 2xCola, 3 = 3xCola, 4 = 4xCola</remarks>
        Scp207,
        /// <summary>
        /// The Player can't be seen by other entities.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Invisible,
        /// <summary>
        /// The vision of SCP-939
        /// </summary>
        /// <remarks>0 = Disabled, 1 = OnlyMarker, 2 = Only Screen, 3 = Everything</remarks>
        Visuals939,
        /// <summary>
        /// Slows down player (No effect on SCPs)
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        SinkHole,
        /// <summary>
        /// Reduces player speed by 20%; SCPs are immune to this effect.	
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Stained,
        /// <summary>
        /// Turns the screen black.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Visuals173Blink,
        /// <summary>
        /// Removes the player's hands and ability to open inventory or interact; Slowly drains HP.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        SeveredHands,
        /// <summary>
        /// Reduces severity of Amnesia, Bleeding, Burned, Concussed, Hemorrhage, Poisoned and SCP-207.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        RainbowTaste,
        /// <summary>
        /// Reduces damage taken from shots
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        BodyShotReduction,
        /// <summary>
        /// Reduces all forms of damage
        /// </summary>
        /// <remarks>0 = Disabled, 1 = 1 - 1 * 0.005 Damage Multiplier, 255 = 1 - 255 * 0.005 Damage Multiplier </remarks>
        DamageReduction,
        /// <summary>
        /// Increases movement speed
        /// </summary>
        /// <remarks>0 = Disabled, each intensity point adds 1% of movement speed (max 255)</remarks>
        MovementBoost,
        /// <summary>
        /// Immunity to negative status effects except decontamination and pocket dimension.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = Enabled</remarks>
        Vitality,
        /// <summary>
        /// Increases movement speed and reload/draw speed for weapons and Stamina drain rate.
        /// </summary>
        /// <remarks>0 = Disabled, 1 = 1xScp1853, 2 = 2xScp1853...</remarks>
        Scp1853,
    }