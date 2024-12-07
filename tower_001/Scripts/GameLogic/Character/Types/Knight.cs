using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using Tower_001.Scripts.GameLogic.Balance;

/// <summary>
/// A specific implementation of the Character class representing a Knight.
/// The Knight specializes in durability and physical strength, with stats like Health and Attack.
/// Implements traits and abilities unique to the Knight archetype.
/// </summary>
public class Knight : Character
{
    /// <summary>
    /// Constructor for initializing the Knight class with default properties.
    /// </summary>
    public Knight() : base()
    {
        Name = "Knight"; // Default name
    }

    /// <summary>
    /// Initializes stats specific to the Knight, such as Health and Attack.
    /// </summary>
    protected override void InitializeStats()
    {
        // Initialize base health
        _statSystem.InitializeStat(
            StatType.Health,
            GameBalanceConfig.CharacterStats.Knight.BaseHealth
        );

        // Initialize base attack
        _statSystem.InitializeStat(
            StatType.Attack,
            GameBalanceConfig.CharacterStats.Knight.BaseAttack
        );

        // Initialize base defense
        _statSystem.InitializeStat(
            StatType.Defense,
            GameBalanceConfig.CharacterStats.Knight.BaseDefense
        );

        // Initialize base speed
        _statSystem.InitializeStat(
            StatType.Speed,
            GameBalanceConfig.CharacterStats.Knight.BaseSpeed
        );
    }

    /// <summary>
    /// Initializes abilities unique to the Knight.
    /// </summary>
    public override void InitializeAbilities()
    {
        // Implement Knight-specific abilities
    }

    /// <summary>
    /// Initializes traits unique to the Knight.
    /// </summary>
    public override void InitializeTraits()
    {
        // Implement Knight-specific traits
    }

    /// <summary>
    /// Retrieves a stat value using a string key.
    /// Implementation is currently incomplete.
    /// </summary>
    internal override object GetStat(string key)
    {
        // Implement stat lookup by string key
        return null;
    }
}
