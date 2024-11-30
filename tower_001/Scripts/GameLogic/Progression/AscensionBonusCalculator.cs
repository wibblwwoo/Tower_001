using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public class AscensionBonusCalculator
{
	// Base bonus percentages per ascension level
	private const float BASE_STAT_BONUS = 0.02f;         // 2% per level
	private const float HEALTH_SCALING = 1.0f;           // 100% of base bonus
	private const float ATTACK_SCALING = 1.0f;           // 100% of base bonus
	private const float DEFENSE_SCALING = 1.0f;          // 100% of base bonus
	private const float SPEED_SCALING = 1.0f;            // 100% of base bonus
	private const float MANA_SCALING = 1.0f;             // 100% of base bonus

	// Milestone bonuses at specific ascension levels
	private static readonly Dictionary<int, float> MILESTONE_BONUSES = new()
	{
		{ 5, 0.05f },   // +5% at ascension level 5
        { 10, 0.10f },  // +10% at ascension level 10
        { 25, 0.15f },  // +15% at ascension level 25
        { 50, 0.25f },  // +25% at ascension level 50
        { 100, 0.50f }  // +50% at ascension level 100
    };

	public static Dictionary<string, float> CalculateAscensionBonuses(ProgressionData progress)
	{
		var bonuses = new Dictionary<string, float>();
		float baseBonus = BASE_STAT_BONUS * progress.AscensionLevel;

		// Apply base bonus to all stats
		foreach (StatType statType in Enum.GetValues(typeof(StatType)))
		{
			float scalingFactor = GetScalingFactor(statType);
			float bonus = baseBonus * scalingFactor;
			bonuses[statType.ToString()] = bonus;
		}

		// Log the calculated bonuses
		LogBonusCalculations(progress.AscensionLevel, bonuses);

		return bonuses;
	}

	private static float GetScalingFactor(StatType statType)
	{
		return statType switch
		{
			StatType.Health => HEALTH_SCALING,
			StatType.Attack => ATTACK_SCALING,
			StatType.Defense => DEFENSE_SCALING,
			StatType.Speed => SPEED_SCALING,
			StatType.Mana => MANA_SCALING,
			_ => 1.0f  // Default scaling for any new stats
		};
	}

	private static void LogBonusCalculations(long ascensionLevel, Dictionary<string, float> bonuses)
	{
		DebugLogger.Log($"\nAscension Level {ascensionLevel} Bonus Calculations:", DebugLogger.LogCategory.Progress);
		foreach (var bonus in bonuses)
		{
			DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", DebugLogger.LogCategory.Progress);
		}

		// Log milestone bonuses if applicable
		var appliedMilestones = MILESTONE_BONUSES
			.Where(m => ascensionLevel >= m.Key)
			.OrderBy(m => m.Key);

		if (appliedMilestones.Any())
		{
			DebugLogger.Log("\nApplied Milestone Bonuses:", DebugLogger.LogCategory.Progress);
			foreach (var milestone in appliedMilestones)
			{
				DebugLogger.Log($"Level {milestone.Key}: +{milestone.Value:P2}", DebugLogger.LogCategory.Progress);
			}
		}
	}

	// Helper method to calculate cumulative milestone bonus
	public static float GetMilestoneBonusTotal(long ascensionLevel)
	{
		return MILESTONE_BONUSES
			.Where(m => ascensionLevel >= m.Key)
			.Sum(m => m.Value);
	}
}