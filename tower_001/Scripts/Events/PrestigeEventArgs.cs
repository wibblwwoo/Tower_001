using Godot;
using System;

/// <summary>
/// Event arguments for tracking character prestige milestones and rewards.
/// Prestige represents a mid-tier progression system that allows characters
/// to reset their progress in exchange for permanent bonuses.
/// </summary>
/// <remarks>
/// The Prestige system serves multiple purposes:
/// - Provides mid-term goals for players
/// - Offers power progression through resets
/// - Encourages multiple playthroughs
/// - Maintains engagement through rewards
/// 
/// Prestige differs from Ascension in that it:
/// - Requires less investment per tier
/// - Focuses on incremental improvements
/// - May be performed more frequently
/// - Provides smaller but more regular rewards
/// </remarks>
public class PrestigeEventArgs : ProgressionEventArgs
{
    /// <summary>
    /// Gets the character's prestige level before the event.
    /// Used to track progression and calculate rewards.
    /// </summary>
    /// <remarks>
    /// Previous level is important for:
    /// - Calculating prestige rewards
    /// - Tracking progression rate
    /// - Achievement validation
    /// - UI feedback and animations
    /// </remarks>
    public int PreviousPrestigeLevel { get; }

    /// <summary>
    /// Gets the character's new prestige level after the event.
    /// Determines the strength of prestige bonuses.
    /// </summary>
    /// <remarks>
    /// New prestige level affects:
    /// - Starting resources on reset
    /// - Progression speed bonuses
    /// - Resource generation rates
    /// - Available features and content
    /// - Achievement progress
    /// </remarks>
    public int NewPrestigeLevel { get; }

    /// <summary>
    /// Gets the additional power multiplier earned from this specific prestige.
    /// Represents the immediate benefit of prestiging.
    /// </summary>
    /// <remarks>
    /// The prestige multiplier is used to:
    /// - Boost character power
    /// - Accelerate future progression
    /// - Calculate resource gains
    /// - Balance content difficulty
    /// </remarks>
    public float PrestigeMultiplierGained { get; }

    /// <summary>
    /// Gets the total accumulated power multiplier from all prestiges.
    /// Represents the character's overall prestige-based power.
    /// </summary>
    /// <remarks>
    /// Total prestige multiplier affects:
    /// - Overall character power
    /// - Resource acquisition rates
    /// - Challenge difficulty scaling
    /// - Matchmaking considerations
    /// - Progress milestones
    /// 
    /// This value is crucial for:
    /// - Long-term progression tracking
    /// - Game balance calculations
    /// - Achievement systems
    /// - Player comparisons
    /// </remarks>
    public float TotalPrestigeMultiplier { get; }

    /// <summary>
    /// Initializes a new instance of the PrestigeEventArgs class.
    /// </summary>
    /// <param name="characterId">The unique identifier of the prestiging character.</param>
    /// <param name="currentLevel">The character's current level.</param>
    /// <param name="currentExperience">The character's current experience points.</param>
    /// <param name="powerMultiplier">The total power multiplier from all sources.</param>
    /// <param name="previousPrestigeLevel">The character's prestige level before the event.</param>
    /// <param name="newPrestigeLevel">The character's new prestige level after the event.</param>
    /// <param name="prestigeMultiplierGained">The power multiplier gained from this prestige.</param>
    /// <param name="totalPrestigeMultiplier">The total accumulated prestige power multiplier.</param>
    public PrestigeEventArgs(
        string characterId,
        int currentLevel,
        float currentExperience,
        float powerMultiplier,
        int previousPrestigeLevel,
        int newPrestigeLevel,
        float prestigeMultiplierGained,
        float totalPrestigeMultiplier)
        : base(characterId, currentLevel, currentExperience, powerMultiplier)
    {
        PreviousPrestigeLevel = previousPrestigeLevel;
        NewPrestigeLevel = newPrestigeLevel;
        PrestigeMultiplierGained = prestigeMultiplierGained;
        TotalPrestigeMultiplier = totalPrestigeMultiplier;
    }
}