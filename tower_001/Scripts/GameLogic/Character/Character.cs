using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using static GlobalEnums;

/// <summary>
/// Abstract base class representing a character in the game.
/// Defines core functionality such as stats, level, traits, and abilities.
/// Responsible for raising events when character properties or stats change.
/// </summary>
public abstract partial class Character
{
	// Dictionary holding all stats for the character
	protected readonly Dictionary<StatType, CharacterStat> Stats = new();

	// Unique identifier for the character
	public string Id { get; }

	// Name of the character
	public string Name { get; set; }

	// Current level of the character
	public int Level { get; set; } = 1;

	// Elemental affinity of the character (default is None)
	public ElementType Element { get; set; } = ElementType.None;

	// Current power level of the character, calculated based on stats and other factors
	private bool _powerDirty = true;
	private float _cachedPower;
	public float Power
	{
		get
		{
			if (_powerDirty)
			{
				_cachedPower = CalculatePower();
				_powerDirty = false;
			}
			return _cachedPower;
		}
	}


	/// <summary>
	/// Constructor for initializing the character with a unique ID and default stats.
	/// </summary>
	protected Character()
	{
		Id = Guid.NewGuid().ToString();
		//InitializeStats();  // Initialize stats in constructor
		//UpdatePower();      // Calculate initial power

		InitializeAllStats();

		// Calculate initial power value
		

		// Raise character creation event
		if (Globals.Instance?.gameMangers?.Events != null)
		{
			Globals.Instance.gameMangers.Events.RaiseEvent(
				EventType.CharacterCreated,
				new CharacterEventArgs(Id)
			);
			Globals.Instance.gameMangers.Events.AddHandler<CharacterStatEventArgs>(EventType.CharacterStatChanged, OnStatChanged);
		}
	}
	protected float CalculatePower()
    {
        // Base power from level
        float power = Level * 1.0f;
        
        // Add contribution from attack stat
        if (Stats.TryGetValue(StatType.Attack, out var attack))
            power += attack.CurrentValue * 0.5f;
            
        // Add contribution from health stat
        if (Stats.TryGetValue(StatType.Health, out var health))
            power += health.CurrentValue * 0.1f;
            
        // Add small contribution from defense stat
        if (Stats.TryGetValue(StatType.Defense, out var defense))
            power += defense.CurrentValue * 0.2f;
            
        return power;
    }

	private void OnStatChanged(CharacterStatEventArgs args)
	{
		if (args.CharacterId == Id)  // Only handle events for this character
		{
			_powerDirty = true;  // Mark power for recalculation
		}
	}
	/// <summary>
	/// Ensures all required stats are initialized with at least default values
	/// </summary>
	private void InitializeAllStats()
	{
		// Let derived class initialize its specific stats first
		InitializeStats();

		// Ensure all stat types have at least a default value
		foreach (StatType statType in Enum.GetValues(typeof(StatType)))
		{
			if (!Stats.ContainsKey(statType))
			{
				// Create default stat with minimal values if not already defined
				Stats[statType] = new CharacterStat(
					Id,
					statType,
					1.0f,   // Minimum base value
					0.05f,  // Minimum growth rate
					null    // No thresholds for default stats
				);
			}
		}
	}

	/// <summary>
	/// Initializes stats specific to the character type.
	/// Must be implemented by derived classes.
	/// </summary>
	protected abstract void InitializeStats();

	/// <summary>
	/// Initializes abilities unique to the character type.
	/// Must be implemented by derived classes.
	/// </summary>
	public abstract void InitializeAbilities();

	/// <summary>
	/// Initializes traits unique to the character type.
	/// Must be implemented by derived classes.
	/// </summary>
	public abstract void InitializeTraits();

	/// <summary>
	/// Handles level-up logic for the character, including raising events.
	/// </summary>
	/// <summary>
	/// Levels up character to specified level, ensuring stats increase appropriately
	/// </summary>
	public void LevelUp(int newLevel)
	{
		// Validate new level is actually an increase
		if (newLevel <= Level) return;

		var oldLevel = Level;
		Level = newLevel;

		// Calculate level difference for stat scaling
		int levelDifference = newLevel - oldLevel;

		// Update each stat with the new level
		foreach (var stat in Stats.Values)
		{
			// Pass both new level and level difference for proper scaling
			stat.OnLevelUp(newLevel, levelDifference);
		}

		// Update character's total power

		// Raise level up event
		if (Globals.Instance?.gameMangers?.Events != null)
		{
			Globals.Instance.gameMangers.Events.RaiseEvent(
				EventType.CharacterLevelUp,
				new CharacterLevelEventArgs(Id, oldLevel, Level)
			);
		}
	}

	


	/// <summary>
	/// Retrieves the CharacterStat object for the given stat type.
	/// </summary>
	/// <param name="type">The stat type to retrieve.</param>
	/// <returns>The corresponding CharacterStat, or null if not found.</returns>
	public CharacterStat GetStat(StatType type)
	{
		return Stats.TryGetValue(type, out var stat) ? stat : null;
	}
	/// <summary>
	/// Updates all stats for the character.
	/// Handles expiration of temporary modifiers.
	/// </summary>
	public void Update()
	{
		foreach (var stat in Stats.Values)
		{
			stat.Update();
		}
	}

	/// <summary>
	/// Retrieves a stat value based on a string key.
	/// Must be implemented by derived classes.
	/// </summary>
	internal abstract object GetStat(string key);
}