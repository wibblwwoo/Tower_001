using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using static GlobalEnums;

/// <summary>
/// Represents a single character statistic with support for modifiers, buffs, and debuffs.
/// </summary>
/// <remarks>
/// Dependencies and Usage:
/// - Used by: Character, StatSystem, BuffSystem
/// - Related Systems: Combat system, Progression system, Equipment system
/// - Configuration: Uses GameBalanceConfig for stat scaling and growth rates
/// - Data Flow: Manages stat calculations and modifier applications
/// </remarks>
public class CharacterStat
{
    // Core identifiers and base values
    private readonly string _characterId;          // Unique identifier for the owning character
    private readonly StatType _statType;           // Type of stat (Health, Attack, etc.)
    private float _baseValue;                      // Base value before any modifications
    private float _growthRate;                     // How much the stat grows per level
    private readonly Dictionary<string, StatModifier> _modifiers = new();  // Active modifiers by ID
    private readonly List<float> _thresholds;      // Percentage thresholds for triggering events

    // Caching system for performance optimization
    private bool _isDirty = true;                  // Indicates if cache needs recalculation
    private float _cachedValue;                    // Last calculated total value
    private float _lastValue;                      // Previous value for change detection
    private float _ascensionBonus = 0f;           // Bonus from character ascension

    /// <summary>
    /// Gets the base value of the stat before any modifiers are applied
    /// </summary>
    public float BaseValue => _baseValue;

    /// <summary>
    /// Gets the current value of the stat after all modifiers are applied.
    /// Automatically recalculates when modifiers change.
    /// </summary>
    public float CurrentValue
    {
        get
        {
            // Return cached value if no changes have occurred
            if (!_isDirty) return _cachedValue;
            
            // Store last value for threshold checking
            _lastValue = _cachedValue;

            // Initialize modification values
            float flatBonus = 0f;                  // Sum of all flat bonuses
            float percentageBonus = _ascensionBonus;  // Start with ascension bonus

            // Process all active modifiers
            foreach (var mod in _modifiers.Values.Where(m => !m.IsExpired))
            {
                // Check the type of modifier and apply it accordingly
                switch (mod.Type)
                {
                    case BuffType.Flat:
                        // Add flat bonuses directly to the total value
                        flatBonus += mod.Value;     
                        break;
                    case BuffType.Percentage:
                        // Add percentage bonuses to the total percentage bonus
                        percentageBonus += mod.Value;  
                        break;
                }
            }

            // Calculate final value: (base + flat) * (1 + percentage)
            _cachedValue = (_baseValue + flatBonus) * (1 + percentageBonus);
            // Ensure the value doesn't go negative
            _cachedValue = Math.Max(0, _cachedValue);  
            _isDirty = false;

            // Check if we crossed any thresholds
            if (Math.Abs(_lastValue - _cachedValue) > 0.001f)
            {
                CheckThresholds(_lastValue, _cachedValue);
            }

            return _cachedValue;
        }
    }

    /// <summary>
    /// Initializes a new instance of the CharacterStat class
    /// </summary>
    public CharacterStat(string characterId, StatType statType, float baseValue, float growthRate, List<float> thresholds = null)
    {
        // Initialize core values
        _characterId = characterId;
        _statType = statType;
        _baseValue = baseValue;
        _growthRate = growthRate;
        _thresholds = thresholds ?? new List<float>();
        
        // Initialize cache values
        _lastValue = baseValue;
        _cachedValue = baseValue;
    }

    /// <summary>
    /// Updates the ascension bonus for this stat
    /// </summary>
    public void UpdateAscensionBonus(string statName, long ascensionLevel, float bonus)
    {
        // Store the current value for change detection
        float oldValue = CurrentValue;             
        // Update the ascension bonus
        _ascensionBonus = bonus;                  
        // Mark the cache as dirty for recalculation
        _isDirty = true;                          

        // Notify if the value changed significantly
        float newValue = CurrentValue;
        if (Math.Abs(oldValue - newValue) > 0.001f)
        {
            RaiseStatChangedEvent(oldValue, newValue);
        }
    }

    /// <summary>
    /// Handles stat increases when the character levels up
    /// </summary>
    public void OnLevelUp(int newLevel, int levelDifference)
    {
        // Ignore invalid level changes
        if (levelDifference <= 0) return;         
        
        // Store the old base value for change detection
        float oldValue = _baseValue;
        // Increase the base value by the growth rate times the number of levels
        _baseValue += _baseValue * (_growthRate * levelDifference);
        // Mark the cache as dirty for recalculation
        _isDirty = true;

        // Get the new current value
        float newValue = CurrentValue;
        // Raise the stat changed event
        RaiseStatChangedEvent(oldValue, newValue);
    }

    /// <summary>
    /// Adds a new modifier to the stat
    /// </summary>
    public void AddModifier(StatModifier modifier)
    {
        // Check if the modifier is null
        if (modifier == null) return;

        // Store the current value for change detection
        float oldValue = CurrentValue;            
        // Add or update the modifier
        _modifiers[modifier.Id] = modifier;       
        // Mark the cache as dirty for recalculation
        _isDirty = true;                         

        // Notify if the value changed significantly
        float newValue = CurrentValue;
        if (Math.Abs(oldValue - newValue) > 0.001f)
        {
            RaiseStatChangedEvent(oldValue, newValue);
        }
    }

    /// <summary>
    /// Removes a modifier from the stat by its ID
    /// </summary>
    public void RemoveModifier(string modifierId)
    {
        // Check if the modifier ID is null or empty
        if (string.IsNullOrEmpty(modifierId)) return;

        // Store the current value for change detection
        float oldValue = CurrentValue;           
        // Remove the modifier if it exists
        if (_modifiers.Remove(modifierId))       
        {
            // Mark the cache as dirty for recalculation
            _isDirty = true;
            // Get the new current value
            float newValue = CurrentValue;
            // Notify if the value changed significantly
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                RaiseStatChangedEvent(oldValue, newValue);
            }
        }
    }

    /// <summary>
    /// Gets a list of all currently active modifiers
    /// </summary>
    public IReadOnlyList<StatModifier> GetActiveModifiers()
    {
        // Return only non-expired modifiers
        return _modifiers.Values.Where(m => !m.IsExpired).ToList();
    }

    /// <summary>
    /// Updates the stat by removing expired modifiers
    /// </summary>
    public void Update()
    {
        // Find all expired modifiers
        var expiredMods = _modifiers.Values
            .Where(m => m.IsExpired)
            .Select(m => m.Id)
            .ToList();

        // Check if there are any expired modifiers
        if (expiredMods.Any())
        {
            // Store the current value for change detection
            float oldValue = CurrentValue;
            // Remove the expired modifiers
            foreach (var id in expiredMods)
            {
                _modifiers.Remove(id);           
            }
            // Mark the cache as dirty for recalculation
            _isDirty = true;
            
            // Get the new current value
            float newValue = CurrentValue;
            // Notify if the value changed significantly
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                RaiseStatChangedEvent(oldValue, newValue);
            }
        }
    }

    /// <summary>
    /// Checks if any threshold values have been crossed
    /// </summary>
    private void CheckThresholds(float oldValue, float newValue)
    {
        // Check if there are any thresholds
        if (_thresholds == null || !_thresholds.Any()) return;
        
        // Use the base value for percentage calculations
        float baseForPercentage = _baseValue;    
    
        // Check each threshold
        foreach (float thresholdPercent in _thresholds)
        {
            // Calculate the actual value for this threshold percentage
            float thresholdValue = baseForPercentage * (thresholdPercent / 100f);
            
            // Check if we crossed below the threshold
            if (oldValue >= thresholdValue && newValue < thresholdValue)
            {
                RaiseThresholdEvent(thresholdPercent, newValue, false);
            }
            // Check if we crossed above the threshold
            else if (oldValue < thresholdValue && newValue >= thresholdValue)
            {
                RaiseThresholdEvent(thresholdPercent, newValue, true);
            }
        }
    }

    /// <summary>
    /// Raises an event when the stat value changes
    /// </summary>
    private void RaiseStatChangedEvent(float oldValue, float newValue)
    {
        // Only raise the event if the change is significant
        if (Math.Abs(oldValue - newValue) < 0.001f) return;
        
        // Raise the event
        Globals.Instance?.gameMangers?.Events?.RaiseEvent(
            EventType.CharacterStatChanged,
            new CharacterStatEventArgs(_characterId, _statType, oldValue, newValue)
        );
    }

    /// <summary>
    /// Raises an event when a stat threshold is crossed
    /// </summary>
    private void RaiseThresholdEvent(float thresholdPercent, float newValue, bool crossingUp)
    {
        // Notify listeners about the threshold crossing
        Globals.Instance?.gameMangers?.Events?.RaiseEvent(
            EventType.CharacterStatThresholdReached,
            new CharacterStatThresholdEventArgs(
                _characterId,
                _statType,
                thresholdPercent,
                newValue,
                crossingUp
            )
        );
    }
}