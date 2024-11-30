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
        Stats[StatType.Health] = new CharacterStat(
            Id,
            StatType.Health,
            GameBalanceConfig.CharacterStats.Mage.BaseHealth,
            GameBalanceConfig.CharacterStats.Mage.HealthGrowth,
            new List<float> { 40, 20 }
        );

        // Initialize base mana
        Stats[StatType.Mana] = new CharacterStat(
            Id,
            StatType.Mana,
            GameBalanceConfig.CharacterStats.Mage.BaseMana,
            GameBalanceConfig.CharacterStats.Mage.ManaGrowth,
            new List<float> { 50, 25 }
        );

        // Initialize base defense
        Stats[StatType.Defense] = new CharacterStat(
            Id,
            StatType.Defense,
            GameBalanceConfig.CharacterStats.Mage.BaseDefense,
            GameBalanceConfig.CharacterStats.Mage.DefenseGrowth,
            null
        );

        // Initialize base speed
        Stats[StatType.Speed] = new CharacterStat(
            Id,
            StatType.Speed,
            GameBalanceConfig.CharacterStats.Mage.BaseSpeed,
            GameBalanceConfig.CharacterStats.Mage.SpeedGrowth,
            null
        );
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