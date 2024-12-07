using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem
{
    /// <summary>
    /// New unified system for managing character statistics.
    /// Handles stat initialization, modification, buffs, and events.
    /// Running in parallel with existing system for testing and validation.
    /// </summary>
    /// <remarks>
    /// Design Goals:
    /// - Simplified stat management in one place
    /// - Cleaner modifier handling
    /// - Consistent event raising
    /// - Better encapsulation of stat data
    /// 
    /// Testing Strategy:
    /// - Run alongside existing system
    /// - Compare values and behavior
    /// - Validate event handling
    /// - Ensure backward compatibility
    /// </remarks>
    public class CharacterStatSystem
    {
        // Core Data
        private readonly string _characterId;
        private Dictionary<StatType, StatData> _stats;
        private readonly Dictionary<StatType, Dictionary<string, StatModifier>> _modifiers = new();

        /// <summary>
        /// Initializes a new character stat system
        /// </summary>
        /// <param name="characterId">Unique identifier for the character</param>
        public CharacterStatSystem(string characterId)
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
            // Initialize stat data
            _stats[statType] = new StatData(
                characterId: _characterId,
                baseValue: baseValue,
                multiplier: GameBalanceConfig.IdleCharacterStats.BaseMultiplier,
                experience: 0,
                level: GameBalanceConfig.IdleCharacterStats.InitialLevel, statType
			);

            // Initialize modifier dictionary for this stat
        }

        /// <summary>
        /// Gets the current value of a stat including all modifiers
        /// </summary>
        public float GetStatValue(StatType statType)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                return 0f;

            // Calculate flat and percentage modifiers
            float flatBonus = 0f;
            float percentageBonus = 0f;

            if (_modifiers.TryGetValue(statType, out var modifiers))
            {
                foreach (var mod in modifiers.Values.Where(m => !m.IsExpired))
                {
                    switch (mod.Type)
                    {
                        case BuffType.Flat:
                            flatBonus += mod.Value;
                            break;
                        case BuffType.Percentage:
                            percentageBonus += mod.Value;
                            break;
                    }
                }
            }

            // Apply base value and modifiers
            float baseValue = stat.BaseValue + flatBonus;
            float totalMultiplier = stat.Multiplier * (1 + percentageBonus);
            return Math.Max(0, baseValue * totalMultiplier);
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
        /// Adds a modifier to a stat
        /// </summary>
        public void AddModifier(StatType statType, string id, StatModifier modifier)
        {
            if (!_modifiers.TryGetValue(statType, out var statModifiers))
            {
                statModifiers = new Dictionary<string, StatModifier>();
                _modifiers[statType] = statModifiers;
            }

            statModifiers[id] = modifier;
            RaiseStatChangedEvent(statType);
        }

        /// <summary>
        /// Removes a modifier from a stat
        /// </summary>
        public void RemoveModifier(StatType statType, string id)
        {
            if (_modifiers.TryGetValue(statType, out var statModifiers))
            {
                if (statModifiers.Remove(id))
                {
                    RaiseStatChangedEvent(statType);
                }
            }
        }

        /// <summary>
        /// Updates all modifiers and removes expired ones
        /// </summary>
        public void UpdateModifiers()
        {
            bool anyRemoved = false;
            foreach (var (statType, modifiers) in _modifiers)
            {
                var expiredModifiers = modifiers.Where(m => m.Value.IsExpired).Select(m => m.Key).ToList();
                foreach (var id in expiredModifiers)
                {
                    modifiers.Remove(id);
                    anyRemoved = true;
                }

                if (anyRemoved)
                {
                    RaiseStatChangedEvent(statType);
                }
            }
        }

        /// <summary>
        /// Raises an event when a stat value changes
        /// </summary>
        private void RaiseStatChangedEvent(StatType statType)
        {
            float oldValue = GetStatValue(statType);
            float newValue = GetStatValue(statType);

            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                    EventType.CharacterStatChanged,
                    new CharacterStatEventArgs(_characterId, statType, oldValue, newValue)
                );
            }
        }

        /// <summary>
        /// Adds experience points to a stat and checks for level ups
        /// </summary>
        public void AddExperience(StatType statType, float expAmount)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                return;
            
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
            Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                EventType.CharacterStatLevelUp,
                new CharacterStatLevelEventArgs(
                    _characterId,
                    statType,
                    stat.Level
                )
            );
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
            if (!_stats.TryGetValue(statType, out var stat))
                return 0;
            
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

        // TODO: Add remaining functionality
        // - Derived stat calculations
        // - Idle progression
        // - Threshold events
        // - State summary
    }
}
