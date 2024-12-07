using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Balance;

/// <summary>
/// A specific implementation of the Character class representing a Mage.
/// The Mage specializes in magical power and resource management, with stats like Mana and Health.
/// Implements traits and abilities unique to the Mage archetype.
/// </summary>
public class Mage : Character
	{
	/// <summary>
	/// Constructor for initializing the Mage class with default properties.
	/// </summary>
	public Mage() : base()
	{
		Name = "Mage"; // Default name
	}

	/// <summary>
	/// Initializes stats specific to the Mage, such as Mana and Health.
	/// </summary>
	protected override void InitializeStats()
	{
		// Initialize base health
        _statSystem.InitializeStat(
            StatType.Health,
            GameBalanceConfig.CharacterStats.Mage.BaseHealth
        );

        // Initialize base mana
        _statSystem.InitializeStat(
            StatType.Mana,
            GameBalanceConfig.CharacterStats.Mage.BaseMana
        );

        // Initialize base defense
        _statSystem.InitializeStat(
            StatType.Defense,
            GameBalanceConfig.CharacterStats.Mage.BaseDefense
        );

        // Initialize base speed
        _statSystem.InitializeStat(
            StatType.Speed,
            GameBalanceConfig.CharacterStats.Mage.BaseSpeed
        );

        // Initialize base magic power
        //_statSystem.InitializeStat(
            //StatType.MagicPower,
            //GameBalanceConfig.CharacterStats.Mage.BaseMagicPower
        //);
		// Additional stats can be added here
		//Power = CalculatePower();
	}

	/// <summary>
	/// Initializes abilities unique to the Mage.
	/// </summary>
	public override void InitializeAbilities()
	{
		// Implement Mage-specific abilities
	}

	/// <summary>
	/// Initializes traits unique to the Mage.
	/// </summary>
	public override void InitializeTraits()
	{
		// Implement Mage-specific traits
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