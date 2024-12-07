using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Represents a progression-related achievement in the game's achievement system.
/// Achievements are goals that players can complete to earn rewards and track their progress.
/// </summary>
/// <remarks>
/// Achievements can be of different types (defined in ProgressionAchievementType) and include:
/// - Level-based achievements (reaching specific levels)
/// - Resource-based achievements (collecting certain amounts)
/// - Combat-based achievements (defeating enemies, completing battles)
/// - Time-based achievements (playing for specific durations)
/// - Meta-progression achievements (prestige, ascension milestones)
/// </remarks>
public class ProgressionAchievement
{
    /// <summary>
    /// Unique identifier for the achievement
    /// </summary>
    /// <remarks>
    /// Format: [Category]_[Action]_[Tier]
    /// Examples: 
    /// - LEVEL_REACH_100
    /// - PRESTIGE_UNLOCK_1
    /// - RESOURCE_GOLD_1000000
    /// </remarks>
    public string Id { get; set; }

    /// <summary>
    /// Type of the achievement, determining how progress is tracked and measured
    /// </summary>
    /// <remarks>
    /// Types are defined in the ProgressionAchievementType enum and determine:
    /// - How progress is calculated
    /// - What events trigger progress updates
    /// - How the achievement is displayed to the player
    /// </remarks>
    public ProgressionAchievementType Type { get; set; }

    /// <summary>
    /// The value that must be reached to complete this achievement
    /// </summary>
    /// <remarks>
    /// The meaning of this value depends on the achievement type:
    /// - For level achievements: the target level
    /// - For resource achievements: the amount needed
    /// - For time achievements: duration in seconds
    /// - For combat achievements: number of victories/completions
    /// </remarks>
    public float TargetValue { get; set; }

    /// <summary>
    /// Dictionary of rewards granted upon achievement completion
    /// </summary>
    /// <remarks>
    /// Key: Reward type identifier (e.g., "experience", "gold", "prestige_points")
    /// Value: Amount of the reward to grant
    /// 
    /// Rewards are processed by the ProgressionManager when the achievement
    /// is completed and automatically granted to the player.
    /// </remarks>
    public Dictionary<string, float> Rewards { get; set; }
}