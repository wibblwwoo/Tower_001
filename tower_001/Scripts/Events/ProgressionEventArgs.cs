using Godot;
using System;

/// <summary>
/// Base class for progression-related events in the game's advancement system.
/// This class serves as the foundation for tracking character growth through levels,
/// experience, and power scaling mechanics.
/// </summary>
/// <remarks>
/// The progression system is a core mechanic that encompasses:
/// - Character leveling and experience gain
/// - Power scaling and multipliers
/// - Prestige and Ascension systems
/// - Resource accumulation tracking
/// 
/// This base class provides essential metrics used across all progression events to:
/// - Track character growth over time
/// - Calculate power increases
/// - Trigger milestone rewards
/// - Update UI elements
/// - Log progression analytics
/// </remarks>
public class ProgressionEventArgs : GameEventArgs
{
    /// <summary>
    /// Gets the unique identifier of the character experiencing progression.
    /// Used to track and update the specific character's advancement.
    /// </summary>
    /// <remarks>
    /// This ID is crucial for:
    /// - Updating the correct character's stats
    /// - Tracking individual progression paths
    /// - Managing multiple character progressions
    /// - Analytics and leaderboards
    /// </remarks>
    public string CharacterId { get; }

    /// <summary>
    /// Gets the character's current level after the progression event.
    /// Represents the primary measure of character advancement.
    /// </summary>
    /// <remarks>
    /// The level is used to:
    /// - Determine character power and capabilities
    /// - Gate content and features
    /// - Calculate rewards and challenges
    /// - Influence matchmaking and balancing
    /// </remarks>
    public int CurrentLevel { get; }

    /// <summary>
    /// Gets the character's current experience points.
    /// Tracks progress towards the next level milestone.
    /// </summary>
    /// <remarks>
    /// Experience points are used to:
    /// - Calculate progress to next level
    /// - Determine prestige eligibility
    /// - Influence reward calculations
    /// - Track player engagement metrics
    /// </remarks>
    public float CurrentExperience { get; }

    /// <summary>
    /// Gets the total power multiplier affecting the character.
    /// This combines all sources of power enhancement.
    /// </summary>
    /// <remarks>
    /// The power multiplier aggregates bonuses from:
    /// - Prestige levels
    /// - Ascension ranks
    /// - Equipment bonuses
    /// - Temporary buffs
    /// - Team synergies
    /// 
    /// This value is crucial for:
    /// - Combat calculations
    /// - Challenge scaling
    /// - Reward scaling
    /// - Progression balancing
    /// </remarks>
    public float PowerMultiplier { get; }

    /// <summary>
    /// Initializes a new instance of the ProgressionEventArgs class.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character experiencing progression.</param>
    /// <param name="currentLevel">The character's current level after the progression event.</param>
    /// <param name="currentExperience">The character's current experience points.</param>
    /// <param name="powerMultiplier">The total accumulated power multiplier from all sources.</param>
    public ProgressionEventArgs(
        string characterId,
        int currentLevel,
        float currentExperience,
        float powerMultiplier)
    {
        CharacterId = characterId;
        CurrentLevel = currentLevel;
        CurrentExperience = currentExperience;
        PowerMultiplier = powerMultiplier;
    }
}