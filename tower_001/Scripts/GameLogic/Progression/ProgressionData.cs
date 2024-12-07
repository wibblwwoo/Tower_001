using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Stores and manages all progression-related data for a character, including experience,
/// levels, prestige, ascension, achievements, and various progression multipliers.
/// This class serves as the central data structure for tracking character advancement
/// and meta-progression systems.
/// </summary>
/// <remarks>
/// The progression system includes multiple layers:
/// - Basic leveling through experience gain
/// - Prestige system for meta-progression
/// - Ascension system for long-term advancement
/// - Achievement system for bonus rewards
/// - Time-based bonuses for consistent play
/// - Milestone system for reaching significant achievements
/// </remarks>
public partial class ProgressionData
{
    #region Core Progression Properties
    /// <summary>
    /// Current level of the character. Starts at 1 and increases through experience gain.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Current experience points towards next level
    /// </summary>
    public float CurrentExp { get; set; } = 0;

    /// <summary>
    /// Current prestige level. Each prestige level provides permanent bonuses
    /// </summary>
    public int PrestigeLevel { get; set; } = 0;

    /// <summary>
    /// Current ascension level. Represents the highest tier of progression
    /// </summary>
    public long AscensionLevel { get; set; } = 0;
    #endregion

    #region Experience Tracking
    /// <summary>
    /// Tracks total experience gained from each source throughout character's lifetime
    /// Used for analytics and achievement tracking
    /// </summary>
    public Dictionary<ExperienceSource, float> LifetimeExperience { get; private set; } = new();

    /// <summary>
    /// Records the highest single experience gain for achievements and records
    /// </summary>
    public float HighestExperienceGain { get; private set; } = 0;
    #endregion

    #region Achievement System
    /// <summary>
    /// Set of achievement IDs that have been completed
    /// </summary>
    public HashSet<string> CompletedAchievements { get; private set; } = new();

    /// <summary>
    /// Tracks progress towards incomplete achievements
    /// Key: Achievement ID, Value: Current progress value
    /// </summary>
    public Dictionary<string, float> AchievementProgress { get; private set; } = new();
    #endregion

    #region Milestone System
    /// <summary>
    /// Tracks which progression milestones have been reached
    /// </summary>
    public HashSet<ProgressionMilestoneType> ReachedMilestones { get; private set; } = new();

    /// <summary>
    /// Records when each milestone was achieved
    /// Key: Milestone ID, Value: Timestamp of completion
    /// </summary>
    public Dictionary<string, DateTime> MilestoneTimestamps { get; private set; } = new();
    #endregion

    #region Multiplier System
    /// <summary>
    /// Currently active bonus multipliers
    /// Key: Multiplier source ID, Value: Multiplier value
    /// </summary>
    public Dictionary<string, float> ActiveMultipliers { get; private set; } = new();

    /// <summary>
    /// Tracks when temporary multipliers will expire
    /// Key: Multiplier ID, Value: Expiration time
    /// </summary>
    public Dictionary<string, DateTime> MultiplierExpirations { get; private set; } = new();
    #endregion

    #region Time Tracking
    /// <summary>
    /// Total time spent playing this character
    /// </summary>
    public TimeSpan TotalPlayTime { get; private set; } = TimeSpan.Zero;

    /// <summary>
    /// Timestamp of the last login
    /// Used for calculating offline progress and daily bonuses
    /// </summary>
    public DateTime LastLoginTime { get; private set; }

    /// <summary>
    /// Time spent in different game activities
    /// Key: Activity name, Value: Time spent
    /// </summary>
    public Dictionary<string, TimeSpan> ActivityPlayTimes { get; private set; } = new();
    #endregion

    private readonly ProgressionConfig _config;

    /// <summary>
    /// Initializes a new instance of ProgressionData with the specified configuration
    /// </summary>
    /// <param name="config">Configuration settings for progression mechanics</param>
    public ProgressionData(ProgressionConfig config)
    {
        _config = config;
        LastLoginTime = DateTime.UtcNow;
        InitializeCollections();
    }

    /// <summary>
    /// Initializes all collection properties with default values
    /// </summary>
    private void InitializeCollections()
    {
        foreach (ExperienceSource source in Enum.GetValues(typeof(ExperienceSource)))
        {
            LifetimeExperience[source] = 0f;
        }

        ActivityPlayTimes = new Dictionary<string, TimeSpan>
        {
            { "Combat", TimeSpan.Zero },
            { "Exploration", TimeSpan.Zero },
            { "Crafting", TimeSpan.Zero },
            { "Questing", TimeSpan.Zero }
        };
    }

    #region Multiplier Calculations
    /// <summary>
    /// Calculates the effective experience multiplier for a given source
    /// Combines base multipliers with bonuses from achievements, time played, and milestones
    /// </summary>
    public float GetEffectiveMultiplier(ExperienceSource source)
    {
        float baseMultiplier = _config.ExperienceMultipliers.GetValueOrDefault(source, 1.0f);
        float achievementBonus = CalculateAchievementBonus();
        float timeBonus = CalculateTimeBonus();
        float milestoneBonus = CalculateMilestoneBonus();

        return baseMultiplier * (1 + achievementBonus + timeBonus + milestoneBonus);
    }

    /// <summary>
    /// Calculates bonus multiplier from completed achievements
    /// </summary>
    private float CalculateAchievementBonus()
    {
        return CompletedAchievements.Count * _config.AchievementBonusMultiplier;
    }

    /// <summary>
    /// Calculates time-based bonus multiplier based on total play time
    /// </summary>
    private float CalculateTimeBonus()
    {
        float hoursPlayed = (float)TotalPlayTime.TotalHours;
        return Math.Min(_config.TimeBasedMultiplierCap,
            hoursPlayed * _config.TimeBasedMultiplierBase);
    }

    /// <summary>
    /// Calculates milestone bonus multiplier from level, prestige, and ascension milestones
    /// </summary>
    private float CalculateMilestoneBonus()
    {
        float bonus = 0f;

        if (_config.LevelMilestoneMultipliers.TryGetValue(Level, out float levelBonus))
            bonus += levelBonus;

        if (_config.PrestigeMilestoneMultipliers.TryGetValue(PrestigeLevel, out float prestigeBonus))
            bonus += prestigeBonus;

        if (_config.AscensionMilestoneMultipliers.TryGetValue(AscensionLevel, out float ascensionBonus))
            bonus += ascensionBonus;

        return bonus;
    }
    #endregion

    #region Power Calculations
    /// <summary>
    /// Gets the total power multiplier from all sources
    /// </summary>
    public float TotalPowerMultiplier => CalculateTotalMultiplier();

    /// <summary>
    /// Gets the power multiplier gained from prestige levels
    /// </summary>
    public float PrestigeMultiplierGained => CalculatePrestigeMultiplier();

    /// <summary>
    /// Calculates the multiplier gained from prestige levels
    /// </summary>
    private float CalculatePrestigeMultiplier()
    {
        return (float)Math.Pow(_config.PrestigePowerMultiplier, PrestigeLevel);
    }

    /// <summary>
    /// Calculates the total power multiplier from all sources
    /// Combines prestige, ascension, and temporary bonus multipliers
    /// </summary>
    private float CalculateTotalMultiplier()
    {
        float prestigeMultiplier = (float)Math.Pow(_config.PrestigePowerMultiplier, PrestigeLevel);
        float ascensionMultiplier = (float)Math.Pow(_config.AscensionPowerMultiplier, AscensionLevel);
        float bonusMultiplier = ActiveMultipliers.Values.Aggregate(1.0f, (current, next) => current * next);

        return prestigeMultiplier * ascensionMultiplier * bonusMultiplier;
    }
    #endregion

    #region Level and Progression Checks
    /// <summary>
    /// Calculates required experience for the next level
    /// Uses exponential scaling based on current level
    /// </summary>
    public float GetExpForNextLevel()
    {
        return _config.BaseExpForLevel * (float)Math.Pow(_config.ExpScalingFactor, Level - 1);
    }

    /// <summary>
    /// Checks if character meets requirements for prestige
    /// </summary>
    public bool CanPrestige => Level >= _config.LevelsForPrestige && CheckPrestigeCost(PrestigeLevel + 1);

    /// <summary>
    /// Checks if character meets requirements for ascension
    /// </summary>
    public bool CanAscend => PrestigeLevel >= _config.PrestigeForAscension &&
                            CheckAscensionCost(AscensionLevel + 1);

    /// <summary>
    /// Verifies if prestige cost exists for the target level
    /// </summary>
    private bool CheckPrestigeCost(int targetLevel)
    {
        return _config.PrestigeCosts.TryGetValue(targetLevel, out long cost);
    }

    /// <summary>
    /// Verifies if ascension cost exists for the target level
    /// </summary>
    private bool CheckAscensionCost(long targetLevel)
    {
        return _config.AscensionCosts.TryGetValue(targetLevel, out long cost);
    }
    #endregion
}