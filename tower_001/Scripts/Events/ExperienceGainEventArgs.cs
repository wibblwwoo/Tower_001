using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for when experience is gained from any source in the game.
/// This class tracks both raw and modified experience values, along with their source.
/// </summary>
/// <remarks>
/// This event is raised whenever a character gains experience from:
/// - Combat encounters
/// - Quest completion
/// - Training activities
/// - Idle progression
/// - Achievement rewards
/// - Special events
/// 
/// The experience calculation follows this formula:
/// Final Experience = Base Experience * Experience Multiplier * Power Multiplier
/// 
/// Related systems:
/// - Character progression
/// - Level-up system
/// - Power scaling
/// - Reward calculation
/// </remarks>
public class ExperienceGainEventArgs : ProgressionEventArgs
{
    /// <summary>
    /// The final amount of experience gained after all multipliers are applied.
    /// This is the actual amount added to the character's experience pool.
    /// </summary>
    /// <remarks>
    /// Calculated as: BaseExperience * ExperienceMultiplier * PowerMultiplier
    /// </remarks>
    public float ExperienceGained { get; }

    /// <summary>
    /// The source activity that generated this experience gain.
    /// Used for analytics, achievements, and conditional bonuses.
    /// </summary>
    /// <remarks>
    /// Different sources may have different base multipliers defined in GameBalanceConfig.
    /// For example, boss fights might give more experience than standard encounters.
    /// </remarks>
    public ExperienceSource Source { get; }

    /// <summary>
    /// Aggregate of all percentage-based experience multipliers.
    /// Includes bonuses from:
    /// - Equipment
    /// - Buffs/Potions
    /// - Event bonuses
    /// - Guild/Party bonuses
    /// </summary>
    /// <remarks>
    /// A multiplier of 1.0 means no modification
    /// Values > 1.0 increase experience gain
    /// Values < 1.0 decrease experience gain
    /// </remarks>
    public float ExperienceMultiplier { get; }

    /// <summary>
    /// The initial experience amount before any multipliers are applied.
    /// Used for tracking raw rewards and balancing purposes.
    /// </summary>
    /// <remarks>
    /// This value is determined by the experience source and difficulty.
    /// It serves as the base value for all experience calculations.
    /// Useful for analytics and balance adjustments.
    /// </remarks>
    public float BaseExperience { get; }

    /// <summary>
    /// Creates a new instance of ExperienceGainEventArgs with detailed gain information.
    /// </summary>
    /// <param name="characterId">Unique identifier of the character gaining experience</param>
    /// <param name="currentLevel">Character's level when experience was gained</param>
    /// <param name="currentExperience">Character's current experience points before this gain</param>
    /// <param name="powerMultiplier">Scaling factor based on character's power level</param>
    /// <param name="experienceGained">Final amount of experience gained after all multipliers</param>
    /// <param name="source">Activity or source that generated this experience</param>
    /// <param name="experienceMultiplier">Combined multiplier from all percentage-based bonuses</param>
    /// <param name="baseExperience">Raw experience amount before multipliers</param>
    public ExperienceGainEventArgs(
        string characterId,
        int currentLevel,
        float currentExperience,
        float powerMultiplier,
        float experienceGained,
        ExperienceSource source,
        float experienceMultiplier,
        float baseExperience)
        : base(characterId, currentLevel, currentExperience, powerMultiplier)
    {
        ExperienceGained = experienceGained;      // Store final experience amount
        Source = source;                          // Record experience source
        ExperienceMultiplier = experienceMultiplier;  // Save combined multiplier
        BaseExperience = baseExperience;          // Keep raw value for reference
    }
}