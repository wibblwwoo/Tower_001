using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower.GameLogic.StatSystem;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
using Tower_001.Scripts.GameLogic.Character;
using Tower_001.Scripts.GameLogic.StatSystem.Bonus;
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
        private readonly Dictionary<StatType, List<StatThreshold>> _statThresholds = new();
        private float _idleTimeAccumulated;
        private readonly PermanentBonusRegistry _bonusRegistry;

        /// <summary>
        /// Initializes a new character stat system
        /// </summary>
        /// <param name="characterId">Unique identifier for the character</param>
        public CharacterStatSystem(string characterId)
        {
            _characterId = characterId;
            _stats = new Dictionary<StatType, StatData>();
            _bonusRegistry = new PermanentBonusRegistry(characterId);
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
        public void InitializeStat(StatType statType, float baseValue)
        {
            // Initialize stat data
            _stats[statType] = new StatData(
                characterId: _characterId,
                baseValue: baseValue,
                multiplier: GameBalanceConfig.IdleCharacterStats.BaseMultiplier,
                experience: 0,
                level: GameBalanceConfig.IdleCharacterStats.InitialLevel,
                statType: statType,
                bonusRegistry: _bonusRegistry
            );

            // Initialize modifier dictionary for this stat
            if (!_modifiers.ContainsKey(statType))
            {
                _modifiers[statType] = new Dictionary<string, StatModifier>();
            }
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
            float totalMultiplier = stat.Multiplier;  // Start with base multiplier

            if (_modifiers.TryGetValue(statType, out var modifiers))
            {
                foreach (var mod in modifiers.Values.Where(m => !m.IsExpired))
                {
                    switch (mod.Type)
                    {
                        case BuffType.Flat:
                            flatBonus += mod.Value;
                            DebugLogger.Log($"Adding flat bonus: {mod.Value} from {mod.Source}", DebugLogger.LogCategory.Stats_Debug);
                            break;
                        case BuffType.Percentage:
                            // Multiply each percentage bonus separately
                            totalMultiplier *= (1 + mod.Value);
                            DebugLogger.Log($"Multiplying by {mod.Source} bonus: (1 + {mod.Value:P2}) = {1 + mod.Value:F3}, total now {totalMultiplier:F3}", DebugLogger.LogCategory.Stats_Debug);
                            break;
                    }
                }
            }

            // Get permanent bonuses
            var permanentBonuses = _bonusRegistry.GetBonusesForStat(statType);
            foreach (var bonus in permanentBonuses)
            {
                // Multiply each permanent bonus separately
                totalMultiplier *= (1 + bonus.Value);
                DebugLogger.Log($"Multiplying by permanent bonus from {bonus.Name}: (1 + {bonus.Value:P2}) = {1 + bonus.Value:F3}, total now {totalMultiplier:F3}", DebugLogger.LogCategory.Stats_Debug);
            }

            // Apply base value and modifiers
            float baseValue = stat.BaseValue + flatBonus;
            float finalValue = baseValue * totalMultiplier;
            DebugLogger.Log($"Final calculation for {statType}: baseValue={baseValue:F2} * totalMultiplier={totalMultiplier:F3} = {finalValue:F2}", DebugLogger.LogCategory.Stats_Debug);
            return Math.Max(0, finalValue);
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
                //+
                //                GetStatValue(StatType.Intelligence) * GameBalanceConfig.IdleCharacterStats.DerivedStats.SpeedPerIntelligence,

                //StatType.CriticalChance => GetStatValue(StatType.Dexterity) * GameBalanceConfig.IdleCharacterStats.DerivedStats.CritChancePerDexterity,

                //StatType.CriticalDamage => GetStatValue(StatType.Strength) * GameBalanceConfig.IdleCharacterStats.DerivedStats.CritDamagePerStrength,

                //StatType.Mana => GetStatValue(StatType.Intelligence) * GameBalanceConfig.IdleCharacterStats.DerivedStats.ManaPerIntelligence,

                //StatType.ManaRegen => GetStatValue(StatType.Intelligence) * GameBalanceConfig.IdleCharacterStats.DerivedStats.ManaRegenPerIntelligence,

                // Return base value for non-derived stats
                _ => GetStatValue(statType)
            };
        }

        /// <summary>
        /// Adds a modifier to a stat
        /// </summary>
        public void AddModifier(StatType statType, string id, StatModifier modifier)
        {
            // Get the current value before adding the modifier
            float oldValue = GetStatValue(statType);

            if (!_modifiers.TryGetValue(statType, out var statModifiers))
            {
                statModifiers = new Dictionary<string, StatModifier>();
                _modifiers[statType] = statModifiers;
            }

            statModifiers[id] = modifier;

            // Get the new value after adding the modifier
            float newValue = GetStatValue(statType);

            // Only raise event if the value actually changed
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                    EventType.CharacterStatChanged,
                    new CharacterStatEventArgs(_characterId, statType, oldValue, newValue)
                );
            }
        }

        /// <summary>
        /// Removes a modifier from a stat
        /// </summary>
        public void RemoveModifier(StatType statType, string id)
        {
            if (!_modifiers.TryGetValue(statType, out var statModifiers))
                return;

            // Get the current value before removing the modifier
            float oldValue = GetStatValue(statType);

            if (statModifiers.Remove(id))
            {
                // Get the new value after removing the modifier
                float newValue = GetStatValue(statType);

                // Only raise event if the value actually changed
                if (Math.Abs(oldValue - newValue) > 0.001f)
                {
                    Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                        EventType.CharacterStatChanged,
                        new CharacterStatEventArgs(_characterId, statType, oldValue, newValue)
                    );
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
            if (Globals.Instance?.gameMangers?.Events == null) return;

            // Get the current value before any modification
            float oldValue = GetStatValue(statType);

            // After modification
            float newValue = GetStatValue(statType);

            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                Globals.Instance.gameMangers.Events.RaiseEvent(
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
        /// Calculate idle gains for a specific stat
        /// </summary>
        /// <param name="statType">The stat to calculate gains for</param>
        /// <param name="timeInMinutes">Time elapsed in minutes</param>
        /// <param name="isOffline">Whether this is offline progression</param>
        /// <returns>The amount of experience gained</returns>
        public float CalculateIdleGains(StatType statType, float timeInMinutes, bool isOffline = false)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                return 0f;

            // Get the base rate for this stat type
            float baseRate = GameBalanceConfig.IdleProgression.BaseExperienceRate;
            float statMultiplier = statType switch
            {
                StatType.Strength => GameBalanceConfig.IdleProgression.StatRates.Strength,
                StatType.Dexterity => GameBalanceConfig.IdleProgression.StatRates.Dexterity,
                StatType.Intelligence => GameBalanceConfig.IdleProgression.StatRates.Intelligence,
                StatType.Stamina => GameBalanceConfig.IdleProgression.StatRates.Stamina,
                _ => 0f // Only primary stats gain experience from idle progression
            };

            if (statMultiplier == 0f)
                return 0f;

            // Apply level penalty
            float levelPenalty = MathF.Pow(
                GameBalanceConfig.IdleProgression.LevelPenaltyFactor,
                stat.Level
            );
            levelPenalty = MathF.Max(
                levelPenalty,
                GameBalanceConfig.IdleProgression.MinLevelMultiplier
            );

            // Calculate catch-up bonus if applicable
            float catchupMultiplier = 1f;
            float expectedLevel = timeInMinutes * GameBalanceConfig.IdleProgression.BaseStatGrowthRate;
            if (stat.Level < expectedLevel * GameBalanceConfig.IdleProgression.CatchupThreshold)
            {
                catchupMultiplier = GameBalanceConfig.IdleProgression.CatchupMultiplier;
            }

            // Calculate final experience gain
            float experienceGain = baseRate
                * statMultiplier
                * levelPenalty
                * catchupMultiplier
                * timeInMinutes;

            // Apply offline penalty if applicable
            if (isOffline)
            {
                experienceGain *= GameBalanceConfig.IdleProgression.OfflineRateMultiplier;
            }

            return experienceGain;
        }

        /// <summary>
        /// Process idle progression for all stats
        /// </summary>
        /// <param name="timeInMinutes">Time elapsed in minutes</param>
        /// <param name="isOffline">Whether this is offline progression</param>
        public void ProcessIdleProgression(float timeInMinutes, bool isOffline = false)
        {
            _idleTimeAccumulated += timeInMinutes;

            foreach (var stat in _stats.Keys.Where(IsPrimaryStat))
            {
                float gain = CalculateIdleGains(stat, timeInMinutes, isOffline);
                if (gain > 0)
                {
                    AddExperience(stat, gain);
                }
            }
        }

        /// <summary>
        /// Sets the level of all stats to the specified level
        /// </summary>
        /// <param name="newLevel">The target level to set</param>
        public void SetLevel(int newLevel)
        {
            foreach (var stat in _stats.Values)
            {
                var oldValue = GetStatValue(stat.Type);
                stat.SetLevel(newLevel);
                stat.SetBaseValue(stat.BaseValue * (float)Math.Pow(GameBalanceConfig.IdleCharacterStats.LevelUpStatMultiplier, newLevel - stat.Level));

                // Raise level up event with proper event args
                Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                    EventType.CharacterStatLevelUp,
                    new CharacterStatLevelEventArgs(
                        _characterId,
                        stat.Type,
                        newLevel
                    )
                );
            }
        }

        /// <summary>
        /// Updates the stat system, processing modifiers and other time-based changes
        /// </summary>
        public void Update()
        {
            UpdateModifiers();
        }

        /// <summary>
        /// Gets all modifiers for all stats
        /// </summary>
        public IEnumerable<StatModifier> GetAllModifiers()
        {
            return _modifiers.Values.SelectMany(modifiers => modifiers.Values);
        }

        /// <summary>
        /// Gets all stats for the character
        /// </summary>
        public Dictionary<StatType, float> GetAllStats()
        {
            return _stats.ToDictionary(
                kvp => kvp.Key,
                kvp => GetStatValue(kvp.Key)
            );
        }

        /// <summary>
        /// Resets all stats to their initial values from GameBalanceConfig and clears all modifiers
        /// </summary>
        public void ResetToInitialValues()
        {
            // Clear all modifiers
            _modifiers.Clear();

            // Reinitialize stats with base values
            InitializeStats();

            // Raise events for all stats that were reset
            foreach (var statType in _stats.Keys)
            {
                RaiseStatChangedEvent(statType);
            }
        }

        /// <summary>
        /// Adds a threshold to a stat
        /// </summary>
        public void AddThreshold(StatType statType, float value, Action<StatThresholdEventArgs> onThresholdReached)
        {
            if (!_statThresholds.ContainsKey(statType))
            {
                _statThresholds[statType] = new List<StatThreshold>();
            }

            _statThresholds[statType].Add(new StatThreshold(statType, value, onThresholdReached));
        }

        /// <summary>
        /// Removes a threshold from a stat
        /// </summary>
        public void RemoveThreshold(StatType statType, float value)
        {
            if (_statThresholds.TryGetValue(statType, out var thresholds))
            {
                thresholds.RemoveAll(t => Math.Abs(t.Value - value) < float.Epsilon);
            }
        }

        /// <summary>
        /// Clears all thresholds for a stat
        /// </summary>
        public void ClearThresholds(StatType statType)
        {
            if (_statThresholds.ContainsKey(statType))
            {
                _statThresholds[statType].Clear();
            }
        }

        /// <summary>
        /// Resets all thresholds for a stat
        /// </summary>
        public void ResetThresholds(StatType statType)
        {
            if (_statThresholds.TryGetValue(statType, out var thresholds))
            {
                foreach (var threshold in thresholds)
                {
                    threshold.Reset();
                }
            }
        }

        /// <summary>
        /// Checks if any thresholds have been reached for a stat
        /// </summary>
        private void CheckThresholds(StatType statType, float previousValue, float newValue)
        {
            if (_statThresholds.TryGetValue(statType, out var thresholds))
            {
                foreach (var threshold in thresholds)
                {
                    threshold.CheckThreshold(previousValue, newValue);
                }
            }
        }

        /// <summary>
        /// Sets the base value of a stat
        /// </summary>
        public void SetBaseValue(StatType statType, float value)
        {
            float previousValue = GetStatValue(statType);
            _stats[statType].SetBaseValue(value);
            float newValue = GetStatValue(statType);

            CheckThresholds(statType, previousValue, newValue);
            RaiseStatChangedEvent(statType);
        }

        /// <summary>
        /// Gets all modifiers for a stat
        /// </summary>
        private IEnumerable<StatModifier> GetModifiers(StatType statType)
        {
            if (_modifiers.TryGetValue(statType, out var modifierDict))
            {
                return modifierDict.Values;
            }
            return Enumerable.Empty<StatModifier>();
        }

        /// <summary>
        /// Gets all derived stat types
        /// </summary>
        private IEnumerable<StatType> GetDerivedStatTypes()
        {
            yield return StatType.Health;
            yield return StatType.Attack;
            yield return StatType.Defense;
            yield return StatType.Speed;
            yield return StatType.CriticalChance;
            yield return StatType.CriticalDamage;
            yield return StatType.Mana;
            yield return StatType.ManaRegen;
        }

        /// <summary>
        /// Checks if a stat is a primary stat
        /// </summary>
        private bool IsPrimaryStat(StatType statType)
        {
            return statType == StatType.Strength ||
                   statType == StatType.Dexterity ||
                   statType == StatType.Intelligence ||
                   statType == StatType.Stamina;
        }

        /// <summary>
        /// Gets a summary of the character's current state
        /// </summary>
        public CharacterStateSummary GetStateSummary()
        {
            var summary = new CharacterStateSummary(_characterId);

            // Calculate average primary level and total power
            float totalLevel = 0;
            int primaryStatCount = 0;

            foreach (var stat in _stats)
            {
                if (IsPrimaryStat(stat.Key))
                {
                    var statSummary = new StatSummary
                    {
                        BaseValue = stat.Value.BaseValue,
                        CurrentValue = GetStatValue(stat.Key),
                        Level = stat.Value.Level,
                        Experience = stat.Value.Experience,
                        ExperienceToNextLevel = CalculateExpNeeded(stat.Key)
                    };

                    // Add active modifiers
                    foreach (var modifier in GetModifiers(stat.Key))
                    {
                        statSummary.ActiveModifiers.Add(new ModifierSummary
                        {
                            Id = modifier.Id,
                            Source = modifier.Source,
                            Type = (ModifierType)modifier.Type,  // Explicit cast from BuffType to ModifierType
                            Value = modifier.Value,
                            Duration = (float)modifier.Duration.TotalSeconds,  // Convert TimeSpan to float seconds
                            TimeRemaining = (float)modifier.Duration.TotalSeconds  // For now, use full duration
                        });
                    }

                    summary.PrimaryStats[stat.Key] = statSummary;
                    totalLevel += stat.Value.Level;
                    primaryStatCount++;
                }
            }

            // Calculate derived stats
            foreach (StatType derivedStat in GetDerivedStatTypes())
            {
                summary.DerivedStats[derivedStat] = GetDerivedStatValue(derivedStat);
            }

            // Calculate idle gains per hour
            foreach (var stat in _stats.Keys)
            {
                if (IsPrimaryStat(stat))
                {
                    summary.IdleGainsPerHour[stat] = CalculateIdleGains(stat, 60f);
                }
            }

            summary.AveragePrimaryLevel = primaryStatCount > 0 ? totalLevel / primaryStatCount : 0;
            summary.IdleTimeAccumulated = _idleTimeAccumulated;

            // Calculate total power based on derived stats
            float power = 0;
            if (summary.DerivedStats.TryGetValue(StatType.Health, out float health))
                power += health * GameBalanceConfig.PowerCalculation.HealthWeight;
            if (summary.DerivedStats.TryGetValue(StatType.Attack, out float attack))
                power += attack * GameBalanceConfig.PowerCalculation.AttackWeight;
            if (summary.DerivedStats.TryGetValue(StatType.Defense, out float defense))
                power += defense * GameBalanceConfig.PowerCalculation.DefenseWeight;
            if (summary.DerivedStats.TryGetValue(StatType.Speed, out float speed))
                power += speed * GameBalanceConfig.PowerCalculation.SpeedWeight;

            summary.TotalPower = power;

            return summary;
        }

        /// <summary>
        /// Updates the ascension bonus for a stat
        /// </summary>
        /// <param name="statType">The stat type to update</param>
        /// <param name="ascensionLevel">Current ascension level</param>
        /// <param name="bonusValue">Bonus value to apply</param>
        public void UpdateAscensionBonus(StatType statType, int ascensionLevel, float bonusValue)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                return;

            // Create a unique bonus for this ascension level
            var bonus = new PermanentBonus(
                name: $"Ascension Level {ascensionLevel}",
                statType: statType,
                source: BonusSource.Ascension,
                value: bonusValue,
                description: $"Bonus from reaching Ascension Level {ascensionLevel}"
            );

            // Add the bonus to the registry
            _bonusRegistry.AddBonus(bonus);

            // Mark the stat as dirty to trigger recalculation
            stat.MarkDirty();

            // Log the update for debugging
            DebugLogger.Log($"Added ascension bonus for {statType}: {bonusValue:P2} at level {ascensionLevel}",
                DebugLogger.LogCategory.Stats);
        }

        /// <summary>
        /// Gets the base value of a stat without any modifiers
        /// </summary>
        /// <param name="statType">The stat type to retrieve</param>
        /// <returns>The base value of the stat</returns>
        public float GetBaseStatValue(StatType statType)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                return 0f;
            return stat.BaseValue;
        }

        /// <summary>
        /// Adds a permanent bonus to a stat
        /// </summary>
        public void AddPermanentBonus(PermanentBonus bonus)
        {
            _bonusRegistry.AddBonus(bonus);
            if (_stats.ContainsKey(bonus.StatType))
            {
                _stats[bonus.StatType].MarkDirty();
            }
        }

        /// <summary>
        /// Removes a permanent bonus from a stat
        /// </summary>
        public void RemovePermanentBonus(PermanentBonus bonus)
        {
            _bonusRegistry.RemoveBonus(bonus);
            if (_stats.ContainsKey(bonus.StatType))
            {
                _stats[bonus.StatType].MarkDirty();
            }
        }

        /// <summary>
        /// Gets all permanent bonuses for a specific stat type
        /// </summary>
        public IEnumerable<PermanentBonus> GetPermanentBonuses(StatType statType)
        {
            return _bonusRegistry.GetBonusesForStat(statType);
        }

        /// <summary>
        /// Gets all permanent bonuses from a specific source
        /// </summary>
        public IEnumerable<PermanentBonus> GetPermanentBonusesFromSource(BonusSource source)
        {
            return _bonusRegistry.GetBonusesFromSource(source);
        }
    }
}
