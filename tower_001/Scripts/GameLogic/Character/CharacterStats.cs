using Godot;
using System;
using System.Collections.Generic;
using Tower_001.Scripts.GameLogic.Balance;

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
    private Dictionary<string, float> _baseStats;
    private Dictionary<string, float> _multipliers;
    private Dictionary<string, float> _currentExp;
    private Dictionary<string, int> _levels;

    public CharacterStats()
    {
        _baseStats = new Dictionary<string, float>();
        _multipliers = new Dictionary<string, float>();
        _currentExp = new Dictionary<string, float>();
        _levels = new Dictionary<string, int>();
        
        InitializeStats();
    }
    
    private void InitializeStats()
    {
        // Initialize primary stats
        InitializeStat("Strength", GameBalanceConfig.IdleCharacterStats.Strength.BaseValue);
        InitializeStat("Dexterity", GameBalanceConfig.IdleCharacterStats.Dexterity.BaseValue);
        InitializeStat("Intelligence", GameBalanceConfig.IdleCharacterStats.Intelligence.BaseValue);
        InitializeStat("Stamina", GameBalanceConfig.IdleCharacterStats.Stamina.BaseValue);
    }
    
    private void InitializeStat(string statName, float baseValue)
    {
        _baseStats[statName] = baseValue;
        _multipliers[statName] = GameBalanceConfig.IdleCharacterStats.BaseMultiplier;
        _currentExp[statName] = 0;
        _levels[statName] = GameBalanceConfig.IdleCharacterStats.InitialLevel;
    }
    
    public float GetStatValue(string statName)
    {
        if (!_baseStats.ContainsKey(statName)) return 0;
        return _baseStats[statName] * _multipliers[statName];
    }

    public float GetDerivedStatValue(string statName)
    {
        return statName switch
        {
            "Health" => GetStatValue("Stamina") * GameBalanceConfig.IdleCharacterStats.DerivedStats.HealthPerStamina,
            "Attack" => GetStatValue("Strength") * GameBalanceConfig.IdleCharacterStats.DerivedStats.AttackPerStrength,
            "Defense" => GetStatValue("Stamina") * GameBalanceConfig.IdleCharacterStats.DerivedStats.DefensePerStamina,
            "Speed" => GetStatValue("Dexterity") * GameBalanceConfig.IdleCharacterStats.DerivedStats.SpeedPerDexterity,
            _ => 0
        };
    }
    
    public void AddExperience(string statName, float expAmount)
    {
        if (!_currentExp.ContainsKey(statName)) return;
        
        _currentExp[statName] += expAmount;
        CheckLevelUp(statName);
    }
    
    private void CheckLevelUp(string statName)
    {
        float expNeeded = CalculateExpNeeded(statName);
        while (_currentExp[statName] >= expNeeded)
        {
            _currentExp[statName] -= expNeeded;
            LevelUp(statName);
            expNeeded = CalculateExpNeeded(statName);
        }
    }
    //todo: COME BACK AND CLEAN THIS UP
    private void LevelUp(string statName)
    {
        var oldValue = GetStatValue(statName);
        _levels[statName]++;
        _baseStats[statName] *= GameBalanceConfig.IdleCharacterStats.LevelUpStatMultiplier;
        /*
        EventManager.Instance.TriggerEvent(GameEvents.CHARACTER_STAT_LEVEL_UP, new Dictionary<string, object>
        {
            { "statName", statName },
            { "newLevel", _levels[statName] }
        });
        
        EventManager.Instance.TriggerEvent(GameEvents.CHARACTER_STAT_VALUE_CHANGED, new Dictionary<string, object>
        {
            { "statName", statName },
            { "oldValue", oldValue },
            { "newValue", GetStatValue(statName) }
        });
        */
    }
    
    private float CalculateExpNeeded(string statName)
    {
        return (float)(Math.Pow(_levels[statName], GameBalanceConfig.IdleCharacterStats.ExperienceCurveExponent) 
            * GameBalanceConfig.IdleCharacterStats.BaseExperienceRequired);
    }
    
    public float CalculateIdleGains(string statName)
    {
        if (!_baseStats.ContainsKey(statName)) return 0;
        
        var baseGainRate = GameBalanceConfig.IdleCharacterStats.BaseIdleGainRate;
        var statMultiplier = statName switch
        {
            "Strength" => GameBalanceConfig.IdleCharacterStats.Strength.IdleGainRate,
            "Dexterity" => GameBalanceConfig.IdleCharacterStats.Dexterity.IdleGainRate,
            "Intelligence" => GameBalanceConfig.IdleCharacterStats.Intelligence.IdleGainRate,
            "Stamina" => GameBalanceConfig.IdleCharacterStats.Stamina.IdleGainRate,
            _ => 1.0f
        };
        
        return baseGainRate * GetStatValue(statName) * _levels[statName] * 
               GameBalanceConfig.IdleCharacterStats.IdleGainMultiplier * statMultiplier;
    }

    public Dictionary<string, object> GetStateSummary()
    {
        return new Dictionary<string, object>
        {
            { "baseStats", _baseStats },
            { "multipliers", _multipliers },
            { "experience", _currentExp },
            { "levels", _levels }
        };
    }
}
