using Godot;
using System;
using System.Collections.Generic;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

/// <summary>
/// Manages character statistics for the idle game, including base stats, experience, and levels.
/// Handles stat calculations, leveling, and idle progression.
/// </summary>
/// <remarks>
/// TODO: Future Enhancement - Data-Driven Class System
/// - Replace hardcoded class stats with JSON/XML configuration
/// - Support dynamic class loading and modification
/// - Enable runtime class balancing and modding
/// - Add class-specific progression paths and hybrid classes
/// - Move class definitions to external config files
/// See related TODOs in GameBalanceConfig.cs and PlayerManager.cs
/// TODO: Investigate using a more robust data structure for storing character stats, such as a dictionary of dictionaries.
/// </remarks>
public partial class CharacterStats : Node
{
    // Core stat collections for managing character progression
    private Dictionary<StatType, float> _baseStats;    // Base values for primary stats (Strength, Dexterity, etc.)
    private Dictionary<StatType, float> _multipliers;  // Stat multipliers from equipment, buffs, etc.
    private Dictionary<StatType, float> _currentExp;   // Current experience points for each stat
    private Dictionary<StatType, int> _levels;         // Current level of each stat

    /// <summary>
    /// Initializes a new character stats instance with default values
    /// </summary>
    public CharacterStats()
    {
        // Initialize collections to store various stat attributes
        _baseStats = new Dictionary<StatType, float>();
        _multipliers = new Dictionary<StatType, float>();
        _currentExp = new Dictionary<StatType, float>();
        _levels = new Dictionary<StatType, int>();
        
        InitializeStats();
    }
    
    /// <summary>
    /// Sets up initial stats with base values from GameBalanceConfig
    /// </summary>
    private void InitializeStats()
    {
        // Initialize each primary stat with its configured base value
        // These form the foundation of the character's capabilities
        InitializeStat(StatType.Strength, GameBalanceConfig.IdleCharacterStats.Strength.BaseValue);      // Physical power and carrying capacity
        InitializeStat(StatType.Dexterity, GameBalanceConfig.IdleCharacterStats.Dexterity.BaseValue);   // Agility and precision
        InitializeStat(StatType.Intelligence, GameBalanceConfig.IdleCharacterStats.Intelligence.BaseValue); // Mental capacity and magic power
        InitializeStat(StatType.Stamina, GameBalanceConfig.IdleCharacterStats.Stamina.BaseValue);       // Endurance and vitality
    }
    
    /// <summary>
    /// Initializes a single stat with its starting values
    /// </summary>
    /// <param name="statType">Type of the stat to initialize</param>
    /// <param name="baseValue">Initial base value for the stat</param>
    private void InitializeStat(StatType statType, float baseValue)
    {
        // Set up all aspects of a stat:
        _baseStats[statType] = baseValue;                                           // Base value from config
        _multipliers[statType] = GameBalanceConfig.IdleCharacterStats.BaseMultiplier; // Starting multiplier (usually 1.0)
        _currentExp[statType] = 0;                                                 // Start with no experience
        _levels[statType] = GameBalanceConfig.IdleCharacterStats.InitialLevel;      // Starting level (usually 1)
    }
    
    /// <summary>
    /// Calculates the current effective value of a primary stat
    /// </summary>
    /// <param name="statType">The stat to calculate</param>
    /// <returns>The stat's base value multiplied by its current multiplier</returns>
    public float GetStatValue(StatType statType)
    {
        if (!_baseStats.ContainsKey(statType)) return 0;
        return _baseStats[statType] * _multipliers[statType];  // Apply multipliers to base value
    }

    /// <summary>
    /// Calculates derived stats based on primary stats and their scaling factors
    /// </summary>
    /// <param name="statType">The derived stat to calculate (Health, Attack, Defense, Speed)</param>
    /// <returns>The calculated value of the derived stat</returns>
    public float GetDerivedStatValue(StatType statType)
    {
        // Calculate secondary stats based on primary stats and their scaling factors
        return statType switch
        {
            StatType.Health => GetStatValue(StatType.Stamina) * GameBalanceConfig.IdleCharacterStats.DerivedStats.HealthPerStamina,   // HP scales with Stamina
            StatType.Attack => GetStatValue(StatType.Strength) * GameBalanceConfig.IdleCharacterStats.DerivedStats.AttackPerStrength, // Attack power from Strength
            StatType.Defense => GetStatValue(StatType.Stamina) * GameBalanceConfig.IdleCharacterStats.DerivedStats.DefensePerStamina, // Defense from Stamina
            StatType.Speed => GetStatValue(StatType.Dexterity) * GameBalanceConfig.IdleCharacterStats.DerivedStats.SpeedPerDexterity, // Speed from Dexterity
            _ => 0  // Return 0 for unknown stats
        };
    }
    
    /// <summary>
    /// Adds experience points to a stat and checks for level ups
    /// </summary>
    /// <param name="statType">The stat gaining experience</param>
    /// <param name="expAmount">Amount of experience to add</param>
    public void AddExperience(StatType statType, float expAmount)
    {
        if (!_currentExp.ContainsKey(statType)) return;
        
        _currentExp[statType] += expAmount;  // Add the experience
        CheckLevelUp(statType);              // Check if we've gained enough exp to level up
    }
    
    /// <summary>
    /// Checks if a stat has enough experience to level up and processes multiple level ups if needed
    /// </summary>
    /// <param name="statType">The stat to check for level ups</param>
    private void CheckLevelUp(StatType statType)
    {
        float expNeeded = CalculateExpNeeded(statType);
        // Continue leveling up while we have enough experience
        while (_currentExp[statType] >= expNeeded)
        {
            _currentExp[statType] -= expNeeded;  // Deduct the exp needed for this level
            LevelUp(statType);                   // Process the level up
            expNeeded = CalculateExpNeeded(statType);  // Calculate exp needed for next level
        }
    }

    /// <summary>
    /// Processes a level up for a stat, increasing its base value and triggering events
    /// </summary>
    /// <param name="statType">The stat gaining a level</param>
    private void LevelUp(StatType statType)
    {
        var oldValue = GetStatValue(statType);  // Store old value for event system
        _levels[statType]++;                    // Increment level
        // Increase base stat value by the configured multiplier
        _baseStats[statType] *= GameBalanceConfig.IdleCharacterStats.LevelUpStatMultiplier;
        
        // TODO: Uncomment when EventManager is implemented
        /*
        EventManager.Instance.TriggerEvent(GameEvents.CHARACTER_STAT_LEVEL_UP, new Dictionary<string, object>
        {
            { "statType", statType },
            { "newLevel", _levels[statType] }
        });
        
        EventManager.Instance.TriggerEvent(GameEvents.CHARACTER_STAT_VALUE_CHANGED, new Dictionary<string, object>
        {
            { "statType", statType },
            { "oldValue", oldValue },
            { "newValue", GetStatValue(statType) }
        });
        */
    }
    
    /// <summary>
    /// Calculates the experience needed for the next level of a stat
    /// Uses a power curve to make higher levels require more experience
    /// </summary>
    /// <param name="statType">The stat to calculate experience for</param>
    /// <returns>The amount of experience needed for the next level</returns>
    private float CalculateExpNeeded(StatType statType)
    {
        // Experience curve: base_exp * (current_level ^ exponent)
        // This creates an exponential increase in required experience per level
        return (float)(Math.Pow(_levels[statType], GameBalanceConfig.IdleCharacterStats.ExperienceCurveExponent) 
            * GameBalanceConfig.IdleCharacterStats.BaseExperienceRequired);
    }
    
    /// <summary>
    /// Calculates how much a stat should increase during idle gameplay
    /// Takes into account the current stat value, level, and stat-specific multipliers
    /// </summary>
    /// <param name="statType">The stat to calculate idle gains for</param>
    /// <returns>The amount the stat should increase per idle tick</returns>
    public float CalculateIdleGains(StatType statType)
    {
        if (!_baseStats.ContainsKey(statType)) return 0;
        
        // Get the base rate for idle gains
        var baseGainRate = GameBalanceConfig.IdleCharacterStats.BaseIdleGainRate;
        
        // Apply stat-specific idle gain multipliers
        var statMultiplier = statType switch
        {
            StatType.Strength => GameBalanceConfig.IdleCharacterStats.Strength.IdleGainRate,
            StatType.Dexterity => GameBalanceConfig.IdleCharacterStats.Dexterity.IdleGainRate,
            StatType.Intelligence => GameBalanceConfig.IdleCharacterStats.Intelligence.IdleGainRate,
            StatType.Stamina => GameBalanceConfig.IdleCharacterStats.Stamina.IdleGainRate,
            _ => 1.0f  // Default multiplier for unknown stats
        };
        
        // Calculate total idle gain:
        // base_rate * current_stat_value * current_level * global_multiplier * stat_multiplier
        return baseGainRate * GetStatValue(statType) * _levels[statType] * 
               GameBalanceConfig.IdleCharacterStats.IdleGainMultiplier * statMultiplier;
    }

    /// <summary>
    /// Creates a snapshot of the current character stats state
    /// Useful for saving/loading and debugging
    /// </summary>
    /// <returns>Dictionary containing all stat collections</returns>
    public Dictionary<string, object> GetStateSummary()
    {
        return new Dictionary<string, object>
        {
            { "baseStats", _baseStats },     // Raw stat values
            { "multipliers", _multipliers },  // Current stat multipliers
            { "experience", _currentExp },    // Experience points per stat
            { "levels", _levels }            // Current level of each stat
        };
    }
}
