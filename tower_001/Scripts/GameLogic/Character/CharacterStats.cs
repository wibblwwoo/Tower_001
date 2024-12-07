using Godot;
using System;
using System.Collections.Generic;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
using Tower_001.Scripts.GameLogic.StatSystem;
using static GlobalEnums;

/// <summary>
/// Manages character statistics for the idle game, including base stats, experience, and levels.
/// Handles stat calculations, leveling, and idle progression.
/// </summary>
public partial class CharacterStats : Node
{
    // Core stat collection for managing character progression
    private Dictionary<StatType, StatData> _stats;    // Contains all stat data (base values, multipliers, exp, levels)
    private readonly string _characterId;             // Unique identifier for the character these stats belong to

    /// <summary>
    /// Initializes a new character stats instance with default values
    /// </summary>
    public CharacterStats(string characterId)
    {
        _characterId = characterId;
        _stats = new Dictionary<StatType, StatData>();
        InitializeStats();
    }
    
    /// <summary>
    /// Sets up initial stats with base values from GameBalanceConfig
    /// </summary>
    private void InitializeStats()
    {
        // Initialize each primary stat with its configured base value
        InitializeStat(StatType.Strength, GameBalanceConfig.IdleCharacterStats.Strength.BaseValue);
        InitializeStat(StatType.Dexterity, GameBalanceConfig.IdleCharacterStats.Dexterity.BaseValue);
        InitializeStat(StatType.Intelligence, GameBalanceConfig.IdleCharacterStats.Intelligence.BaseValue);
        InitializeStat(StatType.Stamina, GameBalanceConfig.IdleCharacterStats.Stamina.BaseValue);
    }
    
    /// <summary>
    /// Initializes a single stat with its starting values
    /// </summary>
    private void InitializeStat(StatType statType, float baseValue)
    {
        _stats[statType] = new StatData(
            characterId: _characterId,
            baseValue: baseValue,
            multiplier: GameBalanceConfig.IdleCharacterStats.BaseMultiplier,
            experience: 0,
            level: GameBalanceConfig.IdleCharacterStats.InitialLevel
        );
    }
    
    /// <summary>
    /// Calculates the current effective value of a primary stat
    /// </summary>
    public float GetStatValue(StatType statType)
    {
        if (!_stats.ContainsKey(statType)) return 0;
        return _stats[statType].CalculateCurrentValue();
    }

    /// <summary>
    /// Calculates derived stats based on primary stats and their scaling factors
    /// </summary>
    public float GetDerivedStatValue(StatType statType)
    {
        return statType switch
        {
            StatType.Health => GetStatValue(StatType.Stamina) * GameBalanceConfig.IdleCharacterStats.DerivedStats.HealthPerStamina,
            StatType.Attack => GetStatValue(StatType.Strength) * GameBalanceConfig.IdleCharacterStats.DerivedStats.AttackPerStrength,
            StatType.Defense => GetStatValue(StatType.Stamina) * GameBalanceConfig.IdleCharacterStats.DerivedStats.DefensePerStamina,
            StatType.Speed => GetStatValue(StatType.Dexterity) * GameBalanceConfig.IdleCharacterStats.DerivedStats.SpeedPerDexterity,
            _ => 0
        };
    }
    
    /// <summary>
    /// Adds experience points to a stat and checks for level ups
    /// </summary>
    public void AddExperience(StatType statType, float expAmount)
    {
        if (!_stats.ContainsKey(statType)) return;
        
        var stat = _stats[statType];
        stat.SetExperience(stat.Experience + expAmount);
        CheckLevelUp(statType);
    }
    
    /// <summary>
    /// Checks if a stat has enough experience to level up and processes multiple level ups if needed
    /// </summary>
    private void CheckLevelUp(StatType statType)
    {
        var stat = _stats[statType];
        float expNeeded = CalculateExpNeeded(statType);
        
        while (stat.Experience >= expNeeded)
        {
            stat.SetExperience(stat.Experience - expNeeded);
            LevelUp(statType);
            expNeeded = CalculateExpNeeded(statType);
        }
    }

    /// <summary>
    /// Processes a level up for a stat, increasing its base value and triggering events
    /// </summary>
    private void LevelUp(StatType statType)
    {
        var stat = _stats[statType];
        var oldValue = GetStatValue(statType);
        
        stat.SetLevel(stat.Level + 1);
        stat.SetBaseValue(stat.BaseValue * GameBalanceConfig.IdleCharacterStats.LevelUpStatMultiplier);
        
        // Raise level up event with proper event args
        Globals.Instance.gameMangers?.Events?.RaiseEvent(EventType.CharacterStatLevelUp, new CharacterStatLevelEventArgs(
            _characterId,
            statType,
            stat.Level
        ));
    }
    
    /// <summary>
    /// Calculates the experience needed for the next level of a stat
    /// </summary>
    private float CalculateExpNeeded(StatType statType)
    {
        var stat = _stats[statType];
        return (float)(Math.Pow(stat.Level, GameBalanceConfig.IdleCharacterStats.ExperienceCurveExponent) 
            * GameBalanceConfig.IdleCharacterStats.BaseExperienceRequired);
    }
    
    /// <summary>
    /// Calculates how much a stat should increase during idle gameplay
    /// </summary>
    public float CalculateIdleGains(StatType statType)
    {
        if (!_stats.ContainsKey(statType)) return 0;
        
        var stat = _stats[statType];
        var baseGainRate = GameBalanceConfig.IdleCharacterStats.BaseIdleGainRate;
        
        var statMultiplier = statType switch
        {
            StatType.Strength => GameBalanceConfig.IdleCharacterStats.Strength.IdleGainRate,
            StatType.Dexterity => GameBalanceConfig.IdleCharacterStats.Dexterity.IdleGainRate,
            StatType.Intelligence => GameBalanceConfig.IdleCharacterStats.Intelligence.IdleGainRate,
            StatType.Stamina => GameBalanceConfig.IdleCharacterStats.Stamina.IdleGainRate,
            _ => 1.0f
        };
        
        return baseGainRate * GetStatValue(statType) * stat.Level * 
               GameBalanceConfig.IdleCharacterStats.IdleGainMultiplier * statMultiplier;
    }

    /// <summary>
    /// Creates a snapshot of the current character stats state
    /// </summary>
    public Dictionary<string, object> GetStateSummary()
    {
        var summary = new Dictionary<string, object>();
        foreach (var (statType, stat) in _stats)
        {
            summary[statType.ToString()] = new Dictionary<string, object>
            {
                { "baseValue", stat.BaseValue },
                { "multiplier", stat.Multiplier },
                { "experience", stat.Experience },
                { "level", stat.Level }
            };
        }
        return summary;
    }

}
