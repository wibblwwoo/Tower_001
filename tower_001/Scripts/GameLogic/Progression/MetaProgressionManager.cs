using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Progression
{
    /// <summary>
    /// Manages meta-progression mechanics (Ascension and Prestige) that allow characters
    /// to reset their progress in exchange for permanent bonuses and increased power.
    /// </summary>
    /// <remarks>
    /// The meta-progression system provides:
    /// - Permanent stat bonuses based on progression level
    /// - Reset of character progress (level and/or prestige)
    /// - Increased power multipliers for faster subsequent progression
    /// - Achievement and milestone tracking
    /// 
    /// Dependencies:
    /// - ProgressionConfig: For progression requirements and bonuses
    /// - PlayerManager: For character stat management
    /// - EventManager: For broadcasting progression events
    /// - ProgressionData: For tracking character progress
    /// </remarks>
    public class MetaProgressionManager : IManager
    {
        private readonly Dictionary<string, ProgressionData> _characterProgress;
        private readonly ProgressionConfig _config;
        private readonly EventManager _eventManager;
        private readonly PlayerManager _playerManager;

        /// <summary>
        /// Gets the required dependencies for the MetaProgressionManager
        /// </summary>
        public IEnumerable<Type> Dependencies { get; }

        /// <summary>
        /// Initializes a new instance of the MetaProgressionManager class
        /// </summary>
        public MetaProgressionManager(ProgressionConfig config, PlayerManager playerManager,
                              Dictionary<string, ProgressionData> characterProgress)
        {
            _config = config;
            _eventManager = Globals.Instance.gameMangers.Events;
            _playerManager = playerManager;
            _characterProgress = characterProgress;
        }

        /// <summary>
        /// Sets up the MetaProgressionManager and registers necessary event handlers
        /// </summary>
        public void Setup()
        {
            RegisterEventHandlers();
            DebugLogger.Log("Meta Progression Manager initialized", DebugLogger.LogCategory.Progress);
        }

        private void RegisterEventHandlers()
        {
            // Register event handlers for progression-related events
        }

        /// <summary>
        /// Applies progression bonuses to a character's stats based on the progression type
        /// </summary>
        private void ApplyProgressionBonuses(string characterId, Dictionary<StatType, float> bonuses, ProgressionType type)
        {
            var actualCharacter = _playerManager.GetCharacter(characterId);
            if (actualCharacter == null) return;

            foreach (var bonus in bonuses)
            {
                switch (type)
                {
                    case ProgressionType.Ascension:
                        actualCharacter.UpdateAscensionBonus(bonus.Key, 
                            (int)_characterProgress[characterId].AscensionLevel, 
                            bonus.Value);
                        break;
                    case ProgressionType.Prestige:
                        actualCharacter.UpdatePrestigeBonus(bonus.Key,
                            (int)_characterProgress[characterId].PrestigeLevel,
                            bonus.Value);
                        break;
                }

                DebugLogger.Log(
                    $"Applied {type} bonus to {bonus.Key}: +{bonus.Value:P2}",
                    DebugLogger.LogCategory.Progress
                );
            }
        }

        /// <summary>
        /// Calculates bonuses based on progression type and level
        /// </summary>
        private Dictionary<StatType, float> CalculateProgressionBonuses(ProgressionData progress, ProgressionType type)
        {
            var bonuses = new Dictionary<StatType, float>();
            float baseBonus = type switch
            {
                ProgressionType.Ascension => progress.AscensionLevel * Balance.GameBalanceConfig.Progression.BaseAscensionStatBonusPerLevel,
                ProgressionType.Prestige => progress.PrestigeLevel * Balance.GameBalanceConfig.Progression.BasicPrestigeStatBonusPerLevel,
                _ => 0f
            };

            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                if (statType != StatType.None)
                {
                    bonuses[statType] = baseBonus;
                }
            }

            DebugLogger.Log($"Calculated {type} Bonuses for Level {GetProgressionLevel(progress, type)}:",
                           DebugLogger.LogCategory.Progress);
            foreach (var bonus in bonuses)
            {
                DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", DebugLogger.LogCategory.Progress);
            }

            return bonuses;
        }

        /// <summary>
        /// Gets the current level for the specified progression type
        /// </summary>
        private long GetProgressionLevel(ProgressionData progress, ProgressionType type)
        {
            return type switch
            {
                ProgressionType.Ascension => progress.AscensionLevel,
                ProgressionType.Prestige => progress.PrestigeLevel,
                _ => 0
            };
        }

        /// <summary>
        /// Attempts to progress a character to the next level of the specified type
        /// </summary>
        public bool TryProgress(string characterId, ProgressionType type)
        {
            if (!_characterProgress.TryGetValue(characterId, out var progress))
                return false;

            if (!CanProgress(characterId, type))
                return false;

            long oldLevel = GetProgressionLevel(progress, type);
            
            // Increment the appropriate level
            switch (type)
            {
                case ProgressionType.Ascension:
                    progress.AscensionLevel++;
                    break;
                case ProgressionType.Prestige:
                    progress.PrestigeLevel++;
                    break;
            }

            float powerMultiplier = progress.TotalPowerMultiplier;
            
            // Handle progress reset
            HandleProgressionReset(characterId, progress, type);

            // Calculate and apply new bonuses
            var unlockedBonuses = CalculateProgressionBonuses(progress, type);
            ApplyProgressionBonuses(characterId, unlockedBonuses, type);

            // Raise appropriate event
            RaiseProgressionEvent(characterId, progress, type, oldLevel, powerMultiplier, unlockedBonuses);

            DebugLogger.Log(
                $"Character {characterId} {type}ed to level {GetProgressionLevel(progress, type)}. Applied bonuses: " +
                string.Join(", ", unlockedBonuses.Select(b => $"{b.Key}: +{b.Value:P0}")),
                DebugLogger.LogCategory.Progress
            );

            return true;
        }

        /// <summary>
        /// Checks if a character meets the requirements for the specified progression type
        /// </summary>
        private bool CanProgress(string characterId, ProgressionType type)
        {
            if (!_characterProgress.TryGetValue(characterId, out var progress))
                return false;

            return type switch
            {
                ProgressionType.Ascension => progress.PrestigeLevel >= _config.PrestigeLevelsForAscension &&
                                           progress.AscensionLevel < _config.MaxAscensionLevel,
                ProgressionType.Prestige => progress.Level >= _config.LevelsForPrestige,
                _ => false
            };
        }

        /// <summary>
        /// Handles resetting appropriate progress when advancing in progression
        /// </summary>
        private void HandleProgressionReset(string characterId, ProgressionData progress, ProgressionType type)
        {
            // Reset level and experience
            progress.Level = 1;
            progress.CurrentExp  = 0;

            // Ascension also resets prestige
            if (type == ProgressionType.Ascension)
            {
                progress.PrestigeLevel = 0;
            }

            // Reset any temporary bonuses or modifiers
            var character = _playerManager.GetCharacter(characterId);
            if (character != null)
            {
                // Note: We don't need to reset temporary bonuses as they are handled by the stat system
                // and will be recalculated when stats are marked dirty
                character.GetStatValue(StatType.Strength); // Trigger recalculation
                character.GetStatValue(StatType.Dexterity); // Trigger recalculation
            }
        }

        /// <summary>
        /// Raises the appropriate event for the progression type
        /// </summary>
        private void RaiseProgressionEvent(string characterId, ProgressionData progress, ProgressionType type,
                                         long oldLevel, float powerMultiplier, Dictionary<StatType, float> bonuses)
        {
            switch (type)
            {
                case ProgressionType.Ascension:
                    float ascensionMultiplierGained = CalculateAscensionMultiplier(progress) - powerMultiplier;
                    float totalAscensionMultiplier = CalculateAscensionMultiplier(progress);
                    
                    
                    _eventManager?.RaiseEvent(EventType.AscensionLevelGained,new AscensionEventArgs(
                        characterId,
                        progress.Level,
                        progress.CurrentExp,
                        powerMultiplier,
                        oldLevel,
                        progress.AscensionLevel,
                        ascensionMultiplierGained,
                        totalAscensionMultiplier,
                        bonuses
                    ));
                    break;

                case ProgressionType.Prestige:
                    _eventManager.RaiseEvent(EventType.PrestigeLevelGained, new PrestigeEventArgs(
                        characterId,
                        progress.Level,
                        progress.CurrentExp,
                        powerMultiplier,
                        oldLevel,
                        progress.PrestigeLevel,
                        Balance.GameBalanceConfig.Progression.BasicPrestigeStatBonusPerLevel,
                        CalculatePrestigeMultiplier(progress)
                    ));
                    break;
            }
        }

        /// <summary>
        /// Calculates the total prestige multiplier based on the character's prestige level
        /// </summary>
        private float CalculatePrestigeMultiplier(ProgressionData progress)
        {
            return (float)Math.Pow(_config.PrestigePowerMultiplier, progress.PrestigeLevel);
        }

        /// <summary>
        /// Calculates the total ascension multiplier based on the character's ascension level
        /// </summary>
        private float CalculateAscensionMultiplier(ProgressionData progress)
        {
            return (float)Math.Pow(_config.AscensionPowerMultiplier, progress.AscensionLevel);
        }
    }
}
