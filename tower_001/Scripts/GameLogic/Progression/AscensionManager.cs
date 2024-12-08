using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Manages the ascension system, a meta-progression mechanic that allows characters
/// to reset their progress in exchange for permanent bonuses and increased power.
/// </summary>
/// <remarks>
/// The ascension system provides:
/// - Permanent stat bonuses based on ascension level
/// - Reset of character level and prestige progress
/// - Increased power multipliers for faster subsequent progression
/// - Achievement and milestone tracking
/// 
/// Dependencies:
/// - ProgressionConfig: For ascension requirements and bonuses
/// - PlayerManager: For character stat management
/// - EventManager: For broadcasting ascension events
/// - ProgressionData: For tracking character progress
/// </remarks>
public class AscensionManager : IManager
{
    private readonly Dictionary<string, ProgressionData> _characterProgress;
    private readonly ProgressionConfig _config;
    private readonly EventManager _eventManager;
    private readonly PlayerManager _playerManager;

    /// <summary>
    /// Gets the required dependencies for the AscensionManager
    /// </summary>
    public IEnumerable<Type> Dependencies { get; }

    /// <summary>
    /// Initializes a new instance of the AscensionManager class
    /// </summary>
    /// <param name="config">Configuration settings for progression and ascension</param>
    /// <param name="playerManager">Manager for accessing and modifying character data</param>
    /// <param name="characterProgress">Shared dictionary of character progression data</param>
    public AscensionManager(ProgressionConfig config, PlayerManager playerManager,
                          Dictionary<string, ProgressionData> characterProgress)
    {
        _config = config;
        _eventManager = Globals.Instance.gameMangers.Events;
        _playerManager = playerManager;
        _characterProgress = characterProgress; // Share the same progress dictionary
    }

    /// <summary>
    /// Sets up the AscensionManager and registers necessary event handlers
    /// </summary>
    public void Setup()
    {
        RegisterEventHandlers();
        DebugLogger.Log("Ascension Manager initialized", DebugLogger.LogCategory.Progress);
    }

    /// <summary>
    /// Registers event handlers for ascension-related events
    /// </summary>
    private void RegisterEventHandlers()
    {
        // Register any necessary event handlers
    }

    /// <summary>
    /// Applies ascension bonuses to a character's stats
    /// </summary>
    /// <param name="characterId">ID of the character to receive bonuses</param>
    /// <param name="bonuses">Dictionary of stat bonuses to apply (stat type -> bonus value)</param>
    /// <remarks>
    /// Bonuses are applied as permanent multipliers to character stats.
    /// Each stat type is processed separately and the bonus is scaled by ascension level.
    /// </remarks>
    private void ApplyAscensionBonuses(string characterId, Dictionary<StatType, float> bonuses)
    {
        // Get the character instance from the player manager
        var character = _playerManager.GetCharacter(characterId);
        if (character == null) return;

        foreach (var bonus in bonuses)
        {
            // Get the character's stat system
            if (character is Character actualCharacter)
            {
                // Apply the bonus scaled by ascension level
                actualCharacter.UpdateAscensionBonus(bonus.Key, 
                                        (int)_characterProgress[characterId].AscensionLevel, 
                                        bonus.Value);

                // Log the applied bonus for debugging and analytics
                DebugLogger.Log(
                    $"Applied ascension bonus to {bonus.Key}: +{bonus.Value:P2}",
                    DebugLogger.LogCategory.Progress
                );
            }
        }
    }

    /// <summary>
    /// Calculates the bonuses granted by ascending to a new level
    /// </summary>
    /// <param name="progress">Character's progression data</param>
    /// <returns>Dictionary of stat bonuses (stat type -> bonus value)</returns>
    /// <remarks>
    /// Base bonus is 2% per ascension level, applied to all stats.
    /// Future implementations may include different scaling for different stats
    /// or additional bonuses based on milestones.
    /// </remarks>
    private Dictionary<StatType, float> CalculateAscensionBonuses(ProgressionData progress)
    {
        var bonuses = new Dictionary<StatType, float>();

        // Calculate the base bonus percentage
        // Each ascension level grants a 2% increase to all stats
        float baseBonus = progress.AscensionLevel * 0.02f;

        // Apply the base bonus to all available stat types
        // This ensures uniform progression across all character attributes
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (statType != StatType.None)  // Skip the None value
            {
                bonuses[statType] = baseBonus;
            }
        }

        // Log calculated bonuses for debugging and balance analysis
        DebugLogger.Log($"Calculated Ascension Bonuses for Level {progress.AscensionLevel}:",
                       DebugLogger.LogCategory.Progress);
        foreach (var bonus in bonuses)
        {
            DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", DebugLogger.LogCategory.Progress);
        }

        return bonuses;
    }

    /// <summary>
    /// Attempts to ascend a character to the next ascension level
    /// </summary>
    /// <param name="characterId">ID of the character attempting to ascend</param>
    /// <returns>True if ascension was successful, false otherwise</returns>
    /// <remarks>
    /// Ascension process:
    /// 1. Verify character can ascend (meets prestige requirement)
    /// 2. Increment ascension level
    /// 3. Reset character progress (level, exp, prestige)
    /// 4. Calculate and apply permanent bonuses
    /// 5. Broadcast ascension event
    /// </remarks>
    public bool TryAscend(string characterId)
    {
        // Validate character exists and has progression data
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return false;

        // Check if character meets ascension requirements
        if (!CanAscend(characterId))
            return false;

        // Store old values for event data and power calculations
        long oldAscensionLevel = progress.AscensionLevel;
        progress.AscensionLevel++;

        // Store current power multiplier before reset
        float powerMultiplier = progress.TotalPowerMultiplier;
        
        // Reset character progress but maintain ascension bonuses
        HandleAscensionReset(characterId, progress);

        // Calculate and apply new permanent stat bonuses
        var unlockedBonuses = CalculateAscensionBonuses(progress);
        ApplyAscensionBonuses(characterId, unlockedBonuses);

        // Notify the system of successful ascension
        // This triggers UI updates and achievement checks
        RaiseAscensionEvent(characterId, progress, oldAscensionLevel, powerMultiplier, unlockedBonuses);

        // Log ascension details for debugging and analytics
        DebugLogger.Log(
            $"Character {characterId} ascended to level {progress.AscensionLevel}. Applied permanent bonuses: " +
            string.Join(", ", unlockedBonuses.Select(b => $"{b.Key}: +{b.Value:P0}")),
            DebugLogger.LogCategory.Progress
        );

        return true;
    }

    /// <summary>
    /// Checks if a character meets the requirements for ascension
    /// </summary>
    /// <param name="characterId">ID of the character to check</param>
    /// <returns>True if the character can ascend, false otherwise</returns>
    private bool CanAscend(string characterId)
    {
        // Verify character exists and has progression data
        if (!_characterProgress.TryGetValue(characterId, out var progress))
            return false;

        // Check if character has reached the required prestige level
        // This is configured in ProgressionConfig
        return progress.PrestigeLevel >= _config.PrestigeLevelsForAscension;
    }

    /// <summary>
    /// Handles the reset of character progress after ascending
    /// </summary>
    /// <param name="characterId">ID of the character being reset</param>
    /// <param name="progress">Character's progression data</param>
    /// <remarks>
    /// Resets:
    /// - Character level to 1
    /// - Current experience to 0
    /// - Prestige level to 0
    /// Recalculates ascension bonuses after reset
    /// </remarks>
    private void HandleAscensionReset(string characterId, ProgressionData progress)
    {
        // Reset character to base level
        progress.Level = 1;
        
        // Clear accumulated experience
        progress.CurrentExp = 0;
        
        // Reset prestige level while maintaining ascension level
        progress.PrestigeLevel = 0;
        
        // Recalculate bonuses with new ascension level
        // This ensures all multipliers are properly updated
        CalculateAscensionBonuses(progress);
    }

    /// <summary>
    /// Raises an event to notify the system of a successful ascension
    /// </summary>
    /// <param name="characterId">ID of the character that ascended</param>
    /// <param name="progress">Current progression data</param>
    /// <param name="oldAscensionLevel">Previous ascension level</param>
    /// <param name="oldPowerMultiplier">Previous power multiplier</param>
    /// <param name="unlockedBonuses">Newly unlocked stat bonuses</param>
    private void RaiseAscensionEvent(
        string characterId,
        ProgressionData progress,
        long oldAscensionLevel,
        float oldPowerMultiplier,
        Dictionary<StatType, float> unlockedBonuses)
    {
        // Convert StatType dictionary to string dictionary for event args
        var bonusesForEvent = unlockedBonuses.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value
        );

        // Create and broadcast an ascension event with all relevant data
        // This event is used by UI, achievements, and other systems
        _eventManager.RaiseEvent(
            EventType.AscensionLevelGained,
            new AscensionEventArgs(
                characterId,              // Identifies the character
                progress.Level,           // New base level (usually 1)
                progress.CurrentExp,      // Reset experience
                progress.TotalPowerMultiplier,  // New total power
                oldAscensionLevel,        // Previous ascension level
                progress.AscensionLevel,  // New ascension level
                progress.TotalPowerMultiplier - oldPowerMultiplier,  // Power gain
                progress.TotalPowerMultiplier,  // Final power value
                bonusesForEvent          // Newly unlocked stat bonuses as strings
            )
        );
    }
}