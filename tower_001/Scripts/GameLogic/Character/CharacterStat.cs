using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Represents a single stat for a character (e.g., Health, Attack).
/// Handles growth, modifiers (buffs/debuffs), and threshold crossing logic.
/// Automatically raises events when the stat value changes or thresholds are crossed.
/// </summary>
public partial class CharacterStat {

	// The character ID associated with this stat
	private readonly string _characterId;

	// The type of stat this instance represents
	private readonly StatType _statType;

	// Base value for the stat
	private readonly float _baseValue;


	// Growth rate for the stat over time or levels
	private readonly float _baseGrowthRate;

	// Dictionary of active modifiers applied to this stat
	private readonly Dictionary<string, StatModifier> _modifiers = new();
	private StatModifier _ascensionModifier; // Single modifier to track total ascension bonus

	// Threshold values for events triggered by this stat
	private readonly List<float> _thresholds = new();

	public IReadOnlyList<float> Thresholds => _thresholds;

	// Last calculated value for the stat
	private float _lastValue;

	// Current base value after level scaling
	private float _currentBaseValue;
	private int _currentLevel = 1;

	
	/// <summary>
	/// Constructor for initializing a CharacterStat with base values, growth rates, and optional thresholds.
	/// </summary>
	/// <param name="characterId">The ID of the character this stat belongs to.</param>
	/// <param name="statType">The type of stat (e.g., Health, Attack).</param>
	/// <param name="baseValue">The initial base value of the stat.</param>
	/// <param name="baseGrowthRate">The growth rate of the stat over time or levels.</param>
	/// <param name="thresholds">Optional thresholds for triggering events when crossed.</param>
	public CharacterStat(string characterId, StatType statType, float baseValue, float baseGrowthRate, List<float> thresholds = null)
	{
		_characterId = characterId;
		_statType = statType;
		_baseValue = baseValue;
		_currentBaseValue = baseValue;
		_baseGrowthRate = baseGrowthRate;
		_thresholds = thresholds ?? new List<float>();
		_thresholds.Sort(); // Ensure thresholds are sorted in ascending order
		_lastValue = baseValue; // Set the initial value to the base value

		RaiseStatChangedEvent(0, _currentBaseValue);
	}

	public void UpdateAscensionBonus(string statType, long ascensionLevel, float newBonusValue)
	{
		float oldBonus = _ascensionModifier?.Value ?? 0f;
		string modifierId = $"ascension_{statType.ToLower()}_total";

		// Create or update the single ascension modifier
		_ascensionModifier = new StatModifier(
			modifierId,
			"Ascension Total",
			BuffType.Percentage,
			newBonusValue
		);

		// Log the change for debugging
		DebugLogger.Log(
			$"Updated ascension bonus for {statType} from {oldBonus:P2} to {newBonusValue:P2} at level {ascensionLevel}",
			DebugLogger.LogCategory.Progress
		);

		// Update the base value with new modifier
		UpdateBaseValue();
	}

	
	private void UpdateBaseValue()
	{
		float ascensionBonus = _ascensionModifier?.Value ?? 0f;
		_currentBaseValue = _baseValue * (1f + ascensionBonus);
	}


/// <summary>
/// Gets the current value of the stat, including all modifiers and growth rates.
/// Automatically raises an event if the calculated value differs from the last recorded value.
/// </summary>
public float CurrentValue
	{
		get
		{
			// Calculate current value including all modifiers
			var (flatBonus, percentBonus, growthBonus) = CalculateModifiers();

			// Apply bonuses in correct order
			var baseWithGrowth = _currentBaseValue * (1 + growthBonus);
			var withFlatBonus = baseWithGrowth + flatBonus;
			var finalValue = withFlatBonus * (1 + percentBonus);

			// Check for significant changes
			if (Math.Abs(finalValue - _lastValue) > 0.01f)
			{
				CheckThresholds(_lastValue, finalValue);
				RaiseStatChangedEvent(_lastValue, finalValue);
				_lastValue = finalValue;
			}

			return finalValue;
		}
	}




	/// <summary>
	/// Adds a modifier (buff or debuff) to this stat.
	/// Automatically raises an event and checks for threshold crossings.
	/// </summary>
	/// <param name="modifier">The stat modifier to apply.</param>
	public void AddModifier(StatModifier modifier)
	{
		var oldValue = CurrentValue; // Record the current value before applying the modifier
		_modifiers[modifier.Id] = modifier; // Add or update the modifier

		// Raise an event indicating the modifier was applied
		Globals.Instance.gameMangers.Events.RaiseEvent(
			EventType.CharacterStatBuffApplied,
			new CharacterStatBuffEventArgs(_characterId, _statType, modifier)
		);

		// Check for any threshold crossings caused by the modifier
		CheckThresholds(oldValue, CurrentValue);
	}

	/// <summary>
	/// Removes a modifier (buff or debuff) from this stat by its ID.
	/// Automatically raises an event and checks for threshold crossings.
	/// </summary>
	/// <param name="modifierId">The unique ID of the modifier to remove.</param>
	public void RemoveModifier(string modifierId)
	{
		if (_modifiers.TryGetValue(modifierId, out var modifier))
		{
			var oldValue = CurrentValue; // Record the current value before removing the modifier
			_modifiers.Remove(modifierId); // Remove the modifier

			// Raise an event indicating the modifier was removed
			Globals.Instance.gameMangers.Events.RaiseEvent(
				EventType.CharacterStatBuffRemoved,
				new CharacterStatBuffEventArgs(_characterId, _statType, modifier)
			);

			// Check for any threshold crossings caused by the removal
			CheckThresholds(oldValue, CurrentValue);
		}
	}


	/// <summary>
	/// Called when character levels up to properly scale the stat
	/// </summary>
	public void OnLevelUp(int newLevel, int levelDifference)
	{
		if (newLevel <= _currentLevel) return;

		float oldValue = CurrentValue;

		// Calculate new base value with level scaling
		float oldBaseValue = _currentBaseValue;
		_currentBaseValue = _baseValue * (1 + (_baseGrowthRate * (newLevel - 1)));

		// Ensure minimum increase per level
		float minimumIncrease = _baseValue * _baseGrowthRate * levelDifference;
		if (_currentBaseValue - oldBaseValue < minimumIncrease)
		{
			_currentBaseValue = oldBaseValue + minimumIncrease;
		}

		_currentLevel = newLevel;

		// Get final value after all modifiers
		float newValue = CurrentValue;

		// Only raise event if there was an actual change
		if (Math.Abs(newValue - oldValue) > 0.01f)
		{
			RaiseStatChangedEvent(oldValue, newValue);
		}
	}
	/// <summary>
	/// Updates the stat by removing expired modifiers and raising events for expiration.
	/// </summary>
	public void Update()
	{
		// Find all modifiers that have expired
		var expiredModifiers = _modifiers.Values
			.Where(m => m.IsExpired)
			.ToList();

		// Remove each expired modifier and raise an expiration event
		foreach (var modifier in expiredModifiers)
		{
			RemoveModifier(modifier.Id);

			Globals.Instance.gameMangers.Events.RaiseEvent(
				EventType.CharacterStatBuffExpired,
				new CharacterStatBuffEventArgs(_characterId, _statType, modifier)
			);
		}
	}

	/// <summary>
	/// Checks if stat value has crossed any defined thresholds
	/// </summary>
	private void CheckThresholds(float oldValue, float newValue)
	{
		if (_thresholds == null || !_thresholds.Any()) return;

		// Get max possible value for percentage calculations
		float maxValue = _currentBaseValue * (1 + _baseGrowthRate);

		DebugLogger.Log(
			$"Checking thresholds for {_statType}: Old={oldValue:F1}, New={newValue:F1}, Max={maxValue:F1}",
			DebugLogger.LogCategory.Stats
		);

		foreach (var thresholdPercent in _thresholds)
		{
			// Convert percentage threshold to actual value
			float thresholdValue = maxValue * (thresholdPercent / 100f);

			DebugLogger.Log(
				$"Checking {_statType} threshold {thresholdPercent}% ({thresholdValue:F1})",
				DebugLogger.LogCategory.Stats
			);

			// Check if we've crossed the threshold in either direction
			bool crossedDown = oldValue > thresholdValue && newValue <= thresholdValue;
			bool crossedUp = oldValue < thresholdValue && newValue >= thresholdValue;

			if (crossedDown || crossedUp)
			{
				DebugLogger.Log(
					$"Threshold crossed for {_statType}: {thresholdPercent}% ({thresholdValue:F1})",
					DebugLogger.LogCategory.Stats
				);

				Globals.Instance?.gameMangers?.Events?.RaiseEvent(
					EventType.CharacterStatThresholdCrossed,
					new CharacterStatThresholdEventArgs(
						_characterId,
						_statType,
						thresholdPercent,
						newValue
					)
				);
			}
		}
	}
	/// <summary>
	/// Raises stat changed event and checks thresholds
	/// </summary>
	private void RaiseStatChangedEvent(float oldValue, float newValue)
	{
		// Always check thresholds, even for small changes
		CheckThresholds(oldValue, newValue);

		// Only raise regular stat change event for significant changes
		if (Math.Abs(oldValue - newValue) > 0.01f)
		{
			Globals.Instance?.gameMangers?.Events?.RaiseEvent(
				EventType.CharacterStatChanged,
				new CharacterStatEventArgs(
					_characterId,
					_statType,
					oldValue,
					newValue
				)
			);
		}
	}

	/// <summary>
	/// Calculates all modifiers applied to the stat, categorized into flat, percentage, and growth bonuses.
	/// </summary>
	/// <returns>A tuple containing the flat bonus, percentage bonus, and growth bonus.</returns>
	private (float flatBonus, float percentBonus, float growthBonus) CalculateModifiers()
	{
		float flatBonus = 0;
		float percentBonus = 0;
		float growthBonus = 0;

		// Iterate through all active (non-expired) modifiers and categorize them
		foreach (var modifier in _modifiers.Values.Where(m => !m.IsExpired))
		{
			switch (modifier.Type)
			{
				case BuffType.Flat:
					flatBonus += modifier.Value; // Add flat bonuses directly
					break;
				case BuffType.Percentage:
					percentBonus += modifier.Value; // Add percentage bonuses
					break;
				case BuffType.Growth:
					growthBonus += modifier.Value; // Add growth bonuses
					break;
			}
		}

		return (flatBonus, percentBonus, growthBonus);
	}

	/// <summary>
	/// Retrieves a list of all active modifiers currently applied to this stat.
	/// </summary>
	/// <returns>A list of active stat modifiers.</returns>
	public IReadOnlyList<StatModifier> GetActiveModifiers()
	{
		return _modifiers.Values
			.Where(m => !m.IsExpired) // Filter out expired modifiers
			.ToList();
	}


}
