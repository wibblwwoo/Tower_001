using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower.GameLogic.StatSystem;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
using Tower_001.Scripts.GameLogic.Character;
using Tower_001.Scripts.GameLogic.StatSystem;
using static GlobalEnums;

/// <summary>
/// Abstract base class representing a character in the game.
/// Defines core functionality such as stats, level, traits, and abilities.
/// Responsible for raising events when character properties or stats change.
/// </summary>
/// <remarks>
/// The Character class serves as the foundation for all character types in the game.
/// It manages:
/// - Core stats (Health, Attack, Defense, etc.)
/// - Level progression and power calculation
/// - Event handling for character changes
/// - Trait and ability system integration
/// 
/// Design Notes:
/// - Uses event-driven architecture for stat changes
/// - Implements lazy calculation for power level
/// - Supports abstract factory pattern for character creation
/// - Allows for flexible stat initialization in derived classes
/// </remarks>
public abstract partial class Character
{
    // Core character attributes
    protected readonly CharacterStatSystem _statSystem;  // Unified stat system implementation
    public string Id { get; }                // Unique identifier for tracking and event handling
    public string Name { get; set; }         // Character's display name
    public int Level { get; set; } = 1;      // Current progression level
    public ElementType Element { get; set; } = ElementType.None;  // Elemental affinity affecting abilities and interactions

    // Power level calculation with lazy evaluation
    private bool _powerDirty = true;         // Flag indicating power needs recalculation
    private float _cachedPower;              // Cached power value for performance
    public float Power                       // Current total power level of the character
    {
        get
        {
            // Only recalculate power when stats have changed
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
    /// Sets up event handlers and triggers initial character creation event.
    /// </summary>
    protected Character()
    {
        // Generate unique identifier for this character instance
        Id = Guid.NewGuid().ToString();

        // Initialize stat system
        _statSystem = new CharacterStatSystem(Id);
        InitializeAllStats();

        // Register with event system if available
        if (Globals.Instance?.gameMangers?.Events != null)
        {
            // Notify system of new character creation
            Globals.Instance.gameMangers.Events.RaiseEvent(
                EventType.CharacterCreated,
                new CharacterEventArgs(Id)
            );

            // Subscribe to stat change events
            Globals.Instance.gameMangers.Events.AddHandler<CharacterStatEventArgs>(
                EventType.CharacterStatChanged, 
                OnStatChanged
            );
        }
    }

    /// <summary>
    /// Calculates the character's total power level based on stats and level.
    /// Power level is used for quick strength comparisons and matchmaking.
    /// </summary>
    /// <returns>The calculated power level</returns>
    protected float CalculatePower()
    {
        float power = 0;

        // Attack contribution (high weight)
        power += _statSystem.GetStatValue(StatType.Attack) * GameBalanceConfig.PowerCalculation.AttackWeight;
            
        // Health contribution (high weight)
        power += _statSystem.GetStatValue(StatType.Health) * GameBalanceConfig.PowerCalculation.HealthWeight;
            
        // Defense contribution (moderate weight)
        power += _statSystem.GetStatValue(StatType.Defense) * GameBalanceConfig.PowerCalculation.DefenseWeight;

        // Speed contribution (base weight)
        power += _statSystem.GetStatValue(StatType.Speed) * GameBalanceConfig.PowerCalculation.SpeedWeight;

        // Apply rank and ascension multipliers if available
        //if (Stats.TryGetValue(StatType.Rank, out var rank))
            //power *= (1 + rank.CurrentValue * GameBalanceConfig.PowerCalculation.RankMultiplier);

        //if (Stats.TryGetValue(StatType.Ascension, out var ascension))
            //power *= (1 + ascension.CurrentValue * GameBalanceConfig.PowerCalculation.AscensionMultiplier);
            
        return power;
    }

    /// <summary>
    /// Event handler for stat changes. Marks power for recalculation when stats change.
    /// </summary>
    /// <param name="args">Event arguments containing changed stat details</param>
    private void OnStatChanged(CharacterStatEventArgs args)
    {
        // Only process events for this character
        if (args.CharacterId == Id)
        {
            _powerDirty = true;  // Mark power for recalculation on next access
        }
    }

    /// <summary>
    /// Ensures all required stats are initialized with at least default values.
    /// Called during character creation to guarantee a complete stat profile.
    /// </summary>
    private void InitializeAllStats()
    {
        // Let derived class set up its specific stat configuration
        InitializeStats();

        // Initialize any custom stats in the new system
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (!_statSystem.GetAllStats().ContainsKey(statType))
            {
                _statSystem.InitializeStat(statType, 1.0f); // Use default base value
            }
        }
    }

    /// <summary>
    /// Initializes stats specific to the character type.
    /// Must be implemented by derived classes to set up their unique stat profiles.
    /// </summary>
    protected abstract void InitializeStats();

    /// <summary>
    /// Initializes abilities unique to the character type.
    /// Must be implemented by derived classes to define their ability set.
    /// </summary>
    public abstract void InitializeAbilities();

    /// <summary>
    /// Initializes traits unique to the character type.
    /// Must be implemented by derived classes to define their trait configuration.
    /// </summary>
    public abstract void InitializeTraits();

    /// <summary>
    /// Levels up character to specified level, ensuring stats increase appropriately.
    /// Handles stat scaling and event notification for level progression.
    /// </summary>
    /// <param name="newLevel">Target level to reach (must be higher than current level)</param>
    public void LevelUp(int newLevel)
    {
        // Validate level increase
        if (newLevel <= Level) return;

        var oldLevel = Level;
        Level = newLevel;

        // Update stats in the system
        _statSystem.SetLevel(newLevel);

        // Notify system of level up if event system is available
        if (Globals.Instance?.gameMangers?.Events != null)
        {
            Globals.Instance.gameMangers.Events.RaiseEvent(
                EventType.CharacterLevelUp,
                new CharacterLevelEventArgs(Id, oldLevel, Level)
            );
        }
    }

    /// <summary>
    /// Retrieves the current stat value for the given stat type.
    /// </summary>
    /// <param name="statType">The stat type to retrieve</param>
    /// <returns>The current stat value</returns>
    public float GetStatValue(StatType statType)
    {
        return _statSystem.GetStatValue(statType);
    }

    /// <summary>
    /// Adds a modifier to the given stat type.
    /// </summary>
    /// <param name="statType">The stat type to modify</param>
    /// <param name="id">Unique identifier for the modifier</param>
    /// <param name="modifier">The modifier to apply</param>
    public void AddModifier(StatType statType, string id, StatModifier modifier)
    {
        _statSystem.AddModifier(statType, id, modifier);
    }

    /// <summary>
    /// Removes a modifier from the given stat type.
    /// </summary>
    /// <param name="statType">The stat type to modify</param>
    /// <param name="id">Unique identifier for the modifier</param>
    public void RemoveModifier(StatType statType, string id)
    {
        _statSystem.RemoveModifier(statType, id);
    }

    /// <summary>
    /// Process idle progression for this character
    /// </summary>
    /// <param name="timeInMinutes">Time elapsed in minutes</param>
    /// <param name="isOffline">Whether this is offline progression</param>
    public void ProcessIdleProgression(float timeInMinutes, bool isOffline = false)
    {
        _statSystem.ProcessIdleProgression(timeInMinutes, isOffline);
    }

    /// <summary>
    /// Gets a complete summary of this character's current state
    /// </summary>
    public CharacterStateSummary GetStateSummary()
    {
        return _statSystem.GetStateSummary();
    }

    /// <summary>
    /// Gets the current power level of this character
    /// </summary>
    public float GetPowerLevel()
    {
        return GetStateSummary().TotalPower;
    }

    /// <summary>
    /// Gets the average level of primary stats for this character
    /// </summary>
    public float GetAverageLevel()
    {
        return GetStateSummary().AveragePrimaryLevel;
    }

    /// <summary>
    /// Gets the current idle gains per hour for a specific stat
    /// </summary>
    public float GetIdleGainsPerHour(StatType statType)
    {
        var summary = GetStateSummary();
        return summary.IdleGainsPerHour.TryGetValue(statType, out float gains) ? gains : 0f;
    }

    /// <summary>
    /// Gets the total time this character has spent in idle progression
    /// </summary>
    public float GetIdleTimeAccumulated()
    {
        return GetStateSummary().IdleTimeAccumulated;
    }

    /// <summary>
    /// Gets detailed information about a specific stat
    /// </summary>
    public StatSummary GetStatInfo(StatType statType)
    {
        var summary = GetStateSummary();
        return summary.PrimaryStats.TryGetValue(statType, out var statInfo) ? statInfo : null;
    }

    /// <summary>
    /// Gets the current value of a derived stat
    /// </summary>
    public float GetDerivedStatValue(StatType statType)
    {
        var summary = GetStateSummary();
        return summary.DerivedStats.TryGetValue(statType, out float value) ? value : 0f;
    }

    /// <summary>
    /// Gets all active modifiers for a stat
    /// </summary>
    public IEnumerable<ModifierSummary> GetStatModifiers(StatType statType)
    {
        var statInfo = GetStatInfo(statType);
        return statInfo?.ActiveModifiers ?? Enumerable.Empty<ModifierSummary>();
    }

    /// <summary>
    /// Adds a threshold to a stat that triggers when the value is reached
    /// </summary>
    public void AddStatThreshold(StatType statType, float value, Action<StatThresholdEventArgs> onThresholdReached)
    {
        _statSystem.AddThreshold(statType, value, onThresholdReached);
    }

    /// <summary>
    /// Removes a threshold from a stat
    /// </summary>
    public void RemoveStatThreshold(StatType statType, float value)
    {
        _statSystem.RemoveThreshold(statType, value);
    }

    /// <summary>
    /// Clears all thresholds for a stat
    /// </summary>
    public void ClearStatThresholds(StatType statType)
    {
        _statSystem.ClearThresholds(statType);
    }

    /// <summary>
    /// Resets all thresholds for a stat, allowing them to trigger again
    /// </summary>
    public void ResetStatThresholds(StatType statType)
    {
        _statSystem.ResetThresholds(statType);
    }

    /// <summary>
    /// Resets all stats to their initial values from GameBalanceConfig
    /// </summary>
    public void ResetStats()
    {
        _statSystem.ResetToInitialValues();
    }

    /// <summary>
    /// Retrieves a stat value based on a string key.
    /// Must be implemented by derived classes for custom stat access.
    /// </summary>
    /// <param name="key">The string identifier for the stat</param>
    /// <returns>The stat value as an object (must be cast to appropriate type)</returns>
    internal abstract object GetStat(string key);

    /// <summary>
    /// Updates all stats for the character.
    /// Processes temporary modifiers and handles their expiration.
    /// Should be called each game tick.
    /// </summary>
    public void Update()
    {
        // Update stat system
        _statSystem.Update();
    }

    /// <summary>
    /// Validates that both stat systems return the same values
    /// </summary>
    private void ValidateStatSystems()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            var newValue = _statSystem.GetStatValue(statType);

            if (Math.Abs(newValue - GetStatValue(statType)) > 0.001f)
            {
                DebugLogger.Log(
                    $"Stat system mismatch for {Name} ({Id}) - {statType}:\n" +
                    $"New system: {newValue:F2}\n" +
                    $"Difference: {Math.Abs(newValue - GetStatValue(statType)):F2}",
                    DebugLogger.LogCategory.Stats
                );
            }
        }
    }

    /// <summary>
    /// Updates the ascension bonus for a stat
    /// </summary>
    /// <param name="statType">The stat type to update</param>
    /// <param name="ascensionLevel">Current ascension level</param>
    /// <param name="bonusValue">Bonus value to apply</param>
    public void UpdateAscensionBonus(StatType statType, int ascensionLevel, float bonusValue)
    {
        _statSystem.UpdateAscensionBonus(statType, ascensionLevel, bonusValue);
    }

    /// <summary>
    /// Gets the base value of a stat without any modifiers
    /// </summary>
    /// <param name="statType">The stat type to retrieve</param>
    /// <returns>The base value of the stat</returns>
    public float GetBaseStatValue(StatType statType)
    {
        return _statSystem.GetBaseStatValue(statType);
    }
}