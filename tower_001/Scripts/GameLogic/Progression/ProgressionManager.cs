using Godot;
using System;
using System.Collections.Generic;
using Tower_001.Scripts.Events;
using static GlobalEnums;

/// <summary>
/// Core manager class for handling all progression-related functionality.
/// Manages experience, leveling, prestige, ascension, and related events.
/// </summary>
public partial class ProgressionManager : IManager
{
    // Tracks progression data for each character by their unique ID
    private readonly Dictionary<string, ProgressionData> _characterProgress;
    
    // Manager for handling character ascension mechanics
    private AscensionManager ascensionManager;
    
    // Required dependencies for this manager's functionality
    public IEnumerable<Type> Dependencies { get; }

    // Configuration settings for progression mechanics
    private ProgressionConfig _config;
    
    // Core system managers
    private readonly EventManager _eventManager;      // Handles game-wide events
    private readonly PlayerManager _playerManager;    // Manages player/character state

    // Public access to managers and config
    public AscensionManager AscensionManager { get => ascensionManager; set => ascensionManager = value; }
    public ProgressionConfig Config { get => _config; set => _config = value; }

    /// <summary>
    /// Initializes a new instance of the ProgressionManager
    /// </summary>
    /// <param name="eventManager">Reference to the game's event manager</param>
    /// <param name="playerManager">Reference to the player manager</param>
    public ProgressionManager(PlayerManager playerManager)
    {
        // Initialize character progression tracking
        _characterProgress = new Dictionary<string, ProgressionData>();
        
        // Set up default progression configuration
        _config = new ProgressionConfig
        {
            BaseExpForLevel = 1000f,          // Base experience needed for first level
            ExpScalingFactor = 1.15f,         // Experience requirement growth per level
            LevelsForPrestige = 100000,       // Level threshold for prestige
            PrestigePowerMultiplier = 2.0f    // Power boost from prestiging
        };

        // Store manager references
        _eventManager = Globals.Instance.gameMangers.Events;
        _playerManager = playerManager;
        
        // Initialize ascension system
        AscensionManager = new AscensionManager(_config, _playerManager, _characterProgress);
    }

    /// <summary>
    /// Sets up the progression manager and registers event handlers
    /// </summary>
    public void Setup()
    {
        RegisterEventHandlers();
        DebugLogger.Log("Progression Manager initialized", DebugLogger.LogCategory.Progress);
        AscensionManager.Setup();
    }

    /// <summary>
    /// Registers all progression-related event handlers
    /// </summary>
    private void RegisterEventHandlers()
    {
        // Character lifecycle events
        _eventManager.AddHandler<CharacterEventArgs>(
            EventType.CharacterCreated,
            OnCharacterCreated);

        _eventManager.AddHandler<CharacterLevelEventArgs>(
            EventType.CharacterLevelUp,
            OnCharacterLevelUp);

        // Content completion events - each awards experience
        _eventManager.AddHandler<RoomCompletionEventArgs>(
            EventType.RoomEventCompleted,
            OnRoomCompleted);

        _eventManager.AddHandler<FloorEventArgs>(
            EventType.FloorCompleted,
            OnFloorCompleted);

        _eventManager.AddHandler<TowerEventArgs>(
            EventType.TowerCompleted,
            OnTowerCompleted);
    }

    /// <summary>
    /// Adds experience to a character from a specific source
    /// </summary>
    public void AddExperience(string characterId, float amount, ExperienceSource source)
    {
        // Validate character exists
        if (!_characterProgress.TryGetValue(characterId, out var progress))
        {
            DebugLogger.LogWarning($"No progression data found for character {characterId}");
            return;
        }

        // Validate experience amount
        if (amount < 0)
        {
            DebugLogger.LogWarning($"Negative Experience Supplied - Ignoring {characterId}");
            return;
        }

        // Apply experience multipliers based on source and character state
        float multiplier = progress.GetEffectiveMultiplier(source);
        float finalExperience = amount * multiplier;

        // Store current state for change detection
        float oldExp = progress.CurrentExp;
        int oldLevel = progress.Level;
        
        // Update experience totals
        progress.CurrentExp += finalExperience;
        progress.LifetimeExperience[source] += finalExperience;

        // Notify systems of experience gain
        RaiseExperienceGainedEvent(characterId, progress, finalExperience, source, multiplier, amount);

        // Process any level ups from this experience gain
        CheckLevelUp(characterId, progress, oldLevel);

        // Log for debugging
        DebugLogger.Log(
            $"Experience added - Character: {characterId}, Amount: {finalExperience:F0}, Source: {source}",
            DebugLogger.LogCategory.Progress);
    }

    /// <summary>
    /// Attempts to prestige a character if requirements are met
    /// </summary>
    public bool TryPrestige(string characterId)
    {
        // Validate character exists and can prestige
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return false;

        if (!progress.CanPrestige)
            return false;

        // Store pre-prestige state
        int oldPrestige = progress.PrestigeLevel;
        progress.PrestigeLevel++;

        // Calculate power changes
        float powerMultiplier = progress.TotalPowerMultiplier;
        
        // Reset character to prestige state
        HandlePrestigeReset(characterId);

        // Notify systems of prestige
        RaisePrestigeEvent(characterId, progress, oldPrestige, powerMultiplier);

        // Log prestige success
        DebugLogger.Log(
            $"Character {characterId} prestiged to level {progress.PrestigeLevel}",
            DebugLogger.LogCategory.Progress);

        return true;
    }

    /// <summary>
    /// Updates progression-related achievements for a character
    /// </summary>
    public void UpdateAchievements(string characterId)
    {
        // Validate character exists
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return;

        // Check each tracked achievement
        foreach (var achievement in GetTrackedAchievements(progress))
        {
            // Calculate current progress towards achievement goal
            float currentProgress = CalculateAchievementProgress(achievement, progress);
            bool wasCompleted = progress.CompletedAchievements.Contains(achievement.Id);

            // Update achievement progress
            progress.AchievementProgress[achievement.Id] = currentProgress;

            // Check for achievement completion
            if (currentProgress >= achievement.TargetValue && !wasCompleted)
            {
                progress.CompletedAchievements.Add(achievement.Id);
                RaiseAchievementEvent(characterId, progress, achievement, currentProgress);
            }
        }
    }

    #region Event Raising Methods

    // These methods handle notifying the game systems about progression changes

    private void RaiseExperienceGainedEvent(
        string characterId,
        ProgressionData progress,
        float experienceGained,
        ExperienceSource source,
        float multiplier,
        float baseAmount)
    {
        // Notify systems of experience gain with all relevant details
        _eventManager.RaiseEvent(
            EventType.ExperienceGained,
            new ExperienceGainEventArgs(
                characterId,
                progress.Level,
                progress.CurrentExp,
                progress.TotalPowerMultiplier,
                experienceGained,
                source,
                multiplier,
                baseAmount
            )
        );
    }

    private void RaisePrestigeEvent(
        string characterId,
        ProgressionData progress,
        int oldPrestigeLevel,
        float oldPowerMultiplier)
    {
        // Notify systems of prestige level gain
        _eventManager.RaiseEvent(
            EventType.PrestigeLevelGained,
            new PrestigeEventArgs(
                characterId,
                progress.Level,
                progress.CurrentExp,
                progress.TotalPowerMultiplier,
                oldPrestigeLevel,
                progress.PrestigeLevel,
                progress.TotalPowerMultiplier - oldPowerMultiplier,
                progress.TotalPowerMultiplier
            )
        );
    }

    private void RaiseAscensionEvent(
        string characterId,
        ProgressionData progress,
        long oldAscensionLevel,
        float oldPowerMultiplier,
        Dictionary<string, float> unlockedBonuses)
    {
        // Notify systems of ascension level gain
        _eventManager.RaiseEvent(
            EventType.AscensionLevelGained,
            new AscensionEventArgs(
                characterId,
                progress.Level,
                progress.CurrentExp,
                progress.TotalPowerMultiplier,
                oldAscensionLevel,
                progress.AscensionLevel,
                progress.TotalPowerMultiplier - oldPowerMultiplier,
                progress.TotalPowerMultiplier,
                unlockedBonuses
            )
        );
    }

    private void RaiseAchievementEvent(
        string characterId,
        ProgressionData progress,
        ProgressionAchievement achievement,
        float currentProgress)
    {
        // Notify systems of achievement progress/completion
        _eventManager.RaiseEvent(
            EventType.ProgressionAchievementUpdated,
            new ProgressionAchievementEventArgs(
                characterId,
                progress.Level,
                progress.CurrentExp,
                progress.TotalPowerMultiplier,
                achievement.Id,
                achievement.Type.ToString(),
                currentProgress,
                achievement.TargetValue,
                true,
                achievement.Rewards
            )
        );
    }

    #endregion

    #region Event Handlers

    private void OnCharacterCreated(CharacterEventArgs args)
    {
        // Initialize new character's progression tracking
        _characterProgress[args.CharacterId] = new ProgressionData(_config);
        DebugLogger.Log($"Initialized progression for character {args.CharacterId}",
            DebugLogger.LogCategory.Progress);
    }

    private void OnCharacterLevelUp(CharacterLevelEventArgs args)
    {
        if (_characterProgress.TryGetValue(args.CharacterId, out var progress))
        {
            // Update achievements and check for milestone rewards
            UpdateAchievements(args.CharacterId);
            CheckProgressionMilestones(args.CharacterId, progress);
        }
    }

    private void OnRoomCompleted(RoomCompletionEventArgs args)
    {
        // Award experience for room completion
        var character = _playerManager.GetCurrentCharacter();
        if (character == null) return;

        float baseExp = CalculateRoomExperience(args);
        AddExperience(character.Id, baseExp, ExperienceSource.RoomCompletion);
    }

    private void OnFloorCompleted(FloorEventArgs args)
    {
        // Award experience for floor completion
        var character = _playerManager.GetCurrentCharacter();
        if (character == null) return;

        float baseExp = CalculateFloorExperience(args);
        AddExperience(character.Id, baseExp, ExperienceSource.FloorCompletion);
    }

    private void OnTowerCompleted(TowerEventArgs args)
    {
        // Award experience for tower completion
        var character = _playerManager.GetCurrentCharacter();
        if (character == null) return;

        float baseExp = CalculateTowerExperience(args);
        AddExperience(character.Id, baseExp, ExperienceSource.TowerCompletion);
    }

    #endregion

    #region Helper Methods

    private void CheckLevelUp(string characterId, ProgressionData progress, int oldLevel)
    {
        // Process any level ups from accumulated experience
        while (progress.CurrentExp >= progress.GetExpForNextLevel())
        {
            // Remove level-up cost and increment level
            progress.CurrentExp -= progress.GetExpForNextLevel();
            progress.Level++;

            // Update character state in player manager
            _playerManager.SetCharacterLevel(characterId, progress.Level);
        }
    }

    public void HandlePrestigeReset(string characterId)
    {
        // Reset character to base state while keeping prestige bonuses
        if (GetProgressData(characterId) is ProgressionData progress)
        {
            progress.Level = 1;
            progress.CurrentExp = 0;
            // Additional resets while maintaining prestige bonuses
        }
    }

    private void HandleAscensionReset(string characterId, ProgressionData progress)
    {
        // Reset character to base state while keeping ascension bonuses
        progress.Level = 1;
        progress.CurrentExp = 0;
        progress.PrestigeLevel = 0;
        // Additional resets while maintaining ascension bonuses
    }

    private Dictionary<string, float> CalculateAscensionBonuses(ProgressionData progress)
    {
        // Calculate bonuses unlocked at current ascension level
        return AscensionBonusCalculator.CalculateAscensionBonuses(progress);
    }

    private IEnumerable<ProgressionAchievement> GetTrackedAchievements(ProgressionData progress)
    {
        // Return list of achievements relevant to current progression
        return new List<ProgressionAchievement>();
    }

    private float CalculateAchievementProgress(ProgressionAchievement achievement, ProgressionData progress)
    {
        // Calculate current progress towards achievement goal
        return 0f;
    }

    private float CalculateRoomExperience(RoomCompletionEventArgs args)
    {
        // Calculate experience reward for room completion
        return 100f;
    }

    private float CalculateFloorExperience(FloorEventArgs args)
    {
        // Calculate experience reward for floor completion
        return 500f;
    }

    private float CalculateTowerExperience(TowerEventArgs args)
    {
        // Calculate experience reward for tower completion
        return 2000f;
    }

    private void CheckProgressionMilestones(string characterId, ProgressionData progress)
    {
        // Check and handle any progression milestones reached
    }

    public float GetCurrentExperience(string characterId)
    {
        // Get character's current experience points
        if (!_characterProgress.TryGetValue(characterId, out var progress))
        {
            return 0f;
        }
        return progress.CurrentExp;
    }

    public float GetExpForNextLevel(string characterId)
    {
        // Calculate experience required for next level
        if (!_characterProgress.TryGetValue(characterId, out var progress))
        {
            return _config.BaseExpForLevel;
        }
        return _config.BaseExpForLevel * (float)Math.Pow(_config.ExpScalingFactor, progress.Level - 1);
    }

    public float GetLifetimeExperience(string characterId, ExperienceSource source)
    {
        // Get total experience earned from specific source
        if (!_characterProgress.TryGetValue(characterId, out var progress))
        {
            return 0f;
        }
        return progress.LifetimeExperience.GetValueOrDefault(source, 0f);
    }

    public int GetMaxLevel()
    {
        // Return maximum achievable level
        return 100; // Default max level
    }

    #endregion

    public ProgressionData GetProgressData(string characterId)
    {
        // Retrieve character's progression data
        if (_characterProgress.TryGetValue(characterId, out var progress))
        {
            return progress;
        }
        return null;
    }

    public bool CanPrestige(string characterId, int prestigeLevel)
    {
        // Check if character meets prestige requirements
        if (_characterProgress.TryGetValue(characterId, out var progress))
        {
            long prestigeCost = GetPrestigeCost(characterId, prestigeLevel);
            // Check level requirement and any resource costs
            return progress.Level >= _config.LevelsForPrestige;
        }
        return false;
    }

    public long GetPrestigeCost(string characterId, int prestigeLevel)
    {
        // Get resource cost for prestige at specified level
        if (_config.PrestigeCosts.TryGetValue(prestigeLevel, out long cost))
        {
            return cost;
        }
        return 0;
    }

    private void HandlePrestigeReset(string characterId, ProgressionData progress)
    {
        // Reset character stats while maintaining prestige bonuses
        progress.Level = 1;
        progress.CurrentExp = 0;
        // Additional resets while preserving prestige multipliers
    }
}