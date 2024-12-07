using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Event arguments for tracking character ascension milestones and rewards.
/// Ascension represents a high-tier progression system that provides permanent
/// power increases and unlocks special features.
/// </summary>
/// <remarks>
/// The Ascension system is an endgame progression mechanic that:
/// - Provides long-term goals for advanced players
/// - Unlocks powerful permanent bonuses
/// - Enables access to exclusive content
/// - Rewards dedicated player investment
/// 
/// Ascension differs from Prestige in that it:
/// - Requires more significant investment
/// - Provides more substantial rewards
/// - Unlocks unique gameplay mechanics
/// - Often gates endgame content
/// </remarks>
public class AscensionEventArgs : ProgressionEventArgs
{
    /// <summary>
    /// Gets the character's ascension level before the event.
    /// Used to track the magnitude of advancement.
    /// </summary>
    /// <remarks>
    /// Previous level is important for:
    /// - Calculating reward differentials
    /// - Tracking progression speed
    /// - Analytics and achievements
    /// - UI animations and effects
    /// </remarks>
    public long PreviousAscensionLevel { get; }

    /// <summary>
    /// Gets the character's new ascension level after the event.
    /// Determines available content and power level.
    /// </summary>
    /// <remarks>
    /// New ascension level influences:
    /// - Available game modes and content
    /// - Power scaling in combat
    /// - Resource generation rates
    /// - Matchmaking parameters
    /// - Achievement tracking
    /// </remarks>
    public long NewAscensionLevel { get; }

    /// <summary>
    /// Gets the additional power multiplier earned from this specific ascension.
    /// Represents the immediate power gain from this milestone.
    /// </summary>
    /// <remarks>
    /// The multiplier gained is used to:
    /// - Calculate immediate power increase
    /// - Display progression feedback
    /// - Balance future challenges
    /// - Track milestone significance
    /// </remarks>
    public float AscensionMultiplierGained { get; }

    /// <summary>
    /// Gets the total accumulated power multiplier from all ascensions.
    /// Represents the character's total power gain from the ascension system.
    /// </summary>
    /// <remarks>
    /// Total multiplier is crucial for:
    /// - Overall power calculations
    /// - Content difficulty scaling
    /// - Matchmaking and balancing
    /// - Progress tracking
    /// - Achievement systems
    /// </remarks>
    public float TotalAscensionMultiplier { get; }

    /// <summary>
    /// Gets the collection of special bonuses unlocked by this ascension level.
    /// Maps bonus identifiers to their magnitude or value.
    /// </summary>
    /// <remarks>
    /// Unlocked bonuses can include:
    /// - Unique abilities or mechanics
    /// - Resource generation boosts
    /// - Special cosmetic effects
    /// - Access to exclusive content
    /// - Permanent stat increases
    /// 
    /// The Dictionary structure allows for:
    /// - Flexible bonus types
    /// - Easy bonus stacking
    /// - Clear bonus tracking
    /// - Simple UI integration
    /// </remarks>
    public Dictionary<string, float> UnlockedBonuses { get; }

    /// <summary>
    /// Initializes a new instance of the AscensionEventArgs class.
    /// </summary>
    /// <param name="characterId">The unique identifier of the ascending character.</param>
    /// <param name="currentLevel">The character's current level.</param>
    /// <param name="currentExperience">The character's current experience points.</param>
    /// <param name="powerMultiplier">The total power multiplier from all sources.</param>
    /// <param name="previousAscensionLevel">The character's ascension level before the event.</param>
    /// <param name="newAscensionLevel">The character's new ascension level after the event.</param>
    /// <param name="ascensionMultiplierGained">The power multiplier gained from this ascension.</param>
    /// <param name="totalAscensionMultiplier">The total accumulated ascension power multiplier.</param>
    /// <param name="unlockedBonuses">Dictionary of special bonuses unlocked by this ascension.</param>
    public AscensionEventArgs(
        string characterId,
        int currentLevel,
        float currentExperience,
        float powerMultiplier,
        long previousAscensionLevel,
        long newAscensionLevel,
        float ascensionMultiplierGained,
        float totalAscensionMultiplier,
        Dictionary<string, float> unlockedBonuses)
        : base(characterId, currentLevel, currentExperience, powerMultiplier)
    {
        PreviousAscensionLevel = previousAscensionLevel;
        NewAscensionLevel = newAscensionLevel;
        AscensionMultiplierGained = ascensionMultiplierGained;
        TotalAscensionMultiplier = totalAscensionMultiplier;
        UnlockedBonuses = unlockedBonuses;
    }
}