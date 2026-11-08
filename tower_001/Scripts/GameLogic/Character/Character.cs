using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
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
    protected readonly Dictionary<StatType, CharacterStat> Stats = new();  // Stores all character stats with their current values and modifiers
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

        // Set up all required stats with default values
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
        // Base power from character level
        float power = Level * GameBalanceConfig.PowerCalculation.LevelScaling;
        
        // Attack contribution (highest weight)
        if (Stats.TryGetValue(StatType.Attack, out var attack))
            power += attack.CurrentValue * GameBalanceConfig.PowerCalculation.AttackWeight;
            
        // Health contribution (base weight)
        if (Stats.TryGetValue(StatType.Health, out var health))
            power += health.CurrentValue * GameBalanceConfig.PowerCalculation.HealthWeight;
            
        // Defense contribution (moderate weight)
        if (Stats.TryGetValue(StatType.Defense, out var defense))
            power += defense.CurrentValue * GameBalanceConfig.PowerCalculation.DefenseWeight;

        // Speed contribution (base weight)
        if (Stats.TryGetValue(StatType.Speed, out var speed))
            power += speed.CurrentValue * GameBalanceConfig.PowerCalculation.SpeedWeight;

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

        // Ensure all possible stats have at least a minimal value
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (!Stats.ContainsKey(statType))
            {
                // Create default stat if not defined by derived class
                Stats[statType] = new CharacterStat(
                    Id,                // Link stat to this character
                    statType,          // Type of stat
                    1.0f,             // Minimum base value
                    0.05f,            // Minimal growth rate
                    null              // No special thresholds
                );
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

        // Calculate how many levels were gained
        int levelDifference = newLevel - oldLevel;

        // Update all stats for the new level
        foreach (var stat in Stats.Values)
        {
            stat.OnLevelUp(newLevel, levelDifference);  // Scale stats based on level gain
        }

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
    /// Retrieves the CharacterStat object for the given stat type.
    /// Provides access to detailed stat information including modifiers.
    /// </summary>
    /// <param name="type">The stat type to retrieve</param>
    /// <returns>The corresponding CharacterStat, or null if not found</returns>
    public CharacterStat GetStat(StatType type)
    {
        return Stats.TryGetValue(type, out var stat) ? stat : null;
    }

    /// <summary>
    /// Updates all stats for the character.
    /// Processes temporary modifiers and handles their expiration.
    /// Should be called each game tick.
    /// </summary>
    public void Update()
    {
        // Update each stat (process modifiers, handle expirations)
        foreach (var stat in Stats.Values)
        {
            stat.Update();
        }
    }

    /// <summary>
    /// Retrieves a stat value based on a string key.
    /// Must be implemented by derived classes for custom stat access.
    /// </summary>
    /// <param name="key">The string identifier for the stat</param>
    /// <returns>The stat value as an object (must be cast to appropriate type)</returns>
    internal abstract object GetStat(string key);
}