using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

/// <summary>
/// Calculates stat bonuses and milestone rewards for character ascension.
/// Uses configuration values from GameBalanceConfig for consistent game balance.
/// </summary>
public class AscensionBonusCalculator
{
    /// <summary>
    /// Calculates the total stat bonuses for a given ascension level
    /// </summary>
    /// <param name="progress">Character's progression data</param>
    /// <returns>Dictionary of stat bonuses (stat name -> bonus value)</returns>
    public static Dictionary<string, float> CalculateAscensionBonuses(ProgressionData progress)
    {
        // Initialize dictionary to store bonuses for each stat type
        var bonuses = new Dictionary<string, float>();
        
        // Calculate base percentage bonus based on ascension level
        // Example: At level 5 with 2% per level = 10% base bonus
        float baseBonus = GameBalanceConfig.Progression.BaseStatBonusPerLevel * progress.AscensionLevel;

        // Iterate through each stat type and calculate its specific bonus
        // Each stat type can have a different scaling factor to balance progression
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            // Get the scaling multiplier for this stat type (e.g., Health might scale at 100%, Attack at 80%)
            float scalingFactor = GetScalingFactor(statType);
            
            // Apply scaling factor to base bonus
            // Example: 10% base bonus * 0.8 scaling = 8% actual bonus for this stat
            float bonus = baseBonus * scalingFactor;
            
            // Store the calculated bonus in our dictionary
            bonuses[statType.ToString()] = bonus;
        }

        // Log all calculations for debugging and balance analysis
        LogBonusCalculations(progress.AscensionLevel, bonuses);

        return bonuses;
    }

    /// <summary>
    /// Gets the scaling factor for a specific stat type from GameBalanceConfig
    /// </summary>
    private static float GetScalingFactor(StatType statType)
    {
        // Map each stat type to its corresponding scaling factor from config
        // This allows fine-tuning of how each stat scales with ascension
        return statType switch
        {
            StatType.Health => GameBalanceConfig.Progression.HealthScaling,   // Health scaling (e.g., 100%)
            StatType.Attack => GameBalanceConfig.Progression.AttackScaling,   // Attack scaling (e.g., 80%)
            StatType.Defense => GameBalanceConfig.Progression.DefenseScaling, // Defense scaling (e.g., 90%)
            StatType.Speed => GameBalanceConfig.Progression.SpeedScaling,     // Speed scaling (e.g., 70%)
            StatType.Mana => GameBalanceConfig.Progression.ManaScaling,       // Mana scaling (e.g., 85%)
            _ => GameBalanceConfig.Progression.DefaultStatScaling            // Default for any new stats
        };
    }

    /// <summary>
    /// Logs detailed bonus calculations for debugging and balance analysis
    /// </summary>
    private static void LogBonusCalculations(long ascensionLevel, Dictionary<string, float> bonuses)
    {
        // Log individual stat bonuses with percentage format
        // Example: "Health: +10.00%"
        DebugLogger.Log($"\nAscension Level {ascensionLevel} Bonus Calculations:", 
                       DebugLogger.LogCategory.Progress);
        foreach (var bonus in bonuses)
        {
            DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", 
                          DebugLogger.LogCategory.Progress);
        }

        // Get all milestone bonuses that apply at the current ascension level
        // Ordered by level to show progression of bonuses
        var appliedMilestones = GameBalanceConfig.Progression.AscensionMilestoneBonuses
            .Where(m => ascensionLevel >= m.Key)
            .OrderBy(m => m.Key);

        // Log milestone bonuses if any are active
        // Example: "Level 5: +5.00%"
        if (appliedMilestones.Any())
        {
            DebugLogger.Log("\nApplied Milestone Bonuses:", 
                          DebugLogger.LogCategory.Progress);
            foreach (var milestone in appliedMilestones)
            {
                DebugLogger.Log($"Level {milestone.Key}: +{milestone.Value:P2}", 
                              DebugLogger.LogCategory.Progress);
            }
        }
    }

    /// <summary>
    /// Calculates the total bonus from all achieved milestones
    /// </summary>
    /// <param name="ascensionLevel">Current ascension level</param>
    /// <returns>Sum of all applicable milestone bonuses</returns>
    public static float GetMilestoneBonusTotal(long ascensionLevel)
    {
        // Sum up all milestone bonuses for levels we've reached or passed
        // Example: At level 12, sum bonuses for levels 5 (5%) and 10 (10%) = 15% total
        return GameBalanceConfig.Progression.AscensionMilestoneBonuses
            .Where(m => ascensionLevel >= m.Key)
            .Sum(m => m.Value);
    }
}