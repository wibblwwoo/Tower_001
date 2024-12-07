using System.Collections.Generic;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Balance
{
	/// <summary>
	/// Static configuration class containing all game balance values.
	/// This centralizes magic numbers used throughout the codebase for easier maintenance and tweaking.
	/// </summary>
	public static partial class GameBalanceConfig
	{

		/// <summary>
		/// Dependencies and Usage:
		/// - Used by: CharacterManager, PlayerManager
		/// - Related Systems: Character progression, Ranking system, Prestige system
		/// </summary>
		public static class Progression
		{
			// Experience and Leveling
			public const float BaseExperienceRequired = 100f;        // Starting XP needed for first level
			public const float ExperienceScalingFactor = 1.15f;     // 15% increase in XP per level
			public const int MaxLevel = 100;                        // Maximum achievable level
			public const float ExperienceShareRatio = 0.1f;         // 10% of XP shared to other characters
			public const float MinExperienceGain = 1.0f;           // Minimum experience gain per action
			public const float MaxExperienceMultiplier = 1000.0f;   // Maximum experience multiplier cap

			// Experience Source Multipliers
			public const float CombatExpMultiplier = 1.0f;         // Base combat experience multiplier
			public const float RoomCompletionExpMultiplier = 1.2f; // Room completion bonus
			public const float FloorCompletionExpMultiplier = 1.5f;// Floor completion bonus
			public const float TowerCompletionExpMultiplier = 2.0f;// Tower completion bonus
			public const float AchievementExpMultiplier = 1.3f;    // Achievement completion bonus
			public const float QuestExpMultiplier = 1.4f;          // Quest completion bonus
			public const float BonusExpMultiplier = 1.1f;         // Generic bonus experience
			public const float EventExpMultiplier = 1.25f;        // Event participation bonus

			// Rank System
			public const int MaxRank = 6;                          // SSS Rank maximum
			public const int MaxStars = 5;                         // Maximum stars per rank
			public const float RankUpStatBonus = 0.2f;             // 20% stat increase per rank
			public const float StarUpStatBonus = 0.05f;            // 5% stat increase per star

			// Prestige System
			public const int LevelsRequiredForPrestige = 10000;    // Levels needed before prestige
			public const float PrestigePointBase = 100f;           // Base points needed for first prestige
			public const float PrestigeScalingFactor = 1.5f;       // 50% increase in points needed per prestige
			public const float PrestigeBonusBase = 0.1f;           // 10% bonus per prestige level
			public const float PrestigePowerBase = 2.0f;          // Base power multiplier for prestige
			public const long BasePrestigeCost = 1000000;         // Base cost for prestige
			public const int PrestigeCostScaling = 10;            // Cost scaling factor for prestige
			public const int MaxPrestigeMilestone = 10;           // Maximum prestige milestone level
			public const float PrestigeMilestoneBaseBonus = 0.5f; // Base bonus for prestige milestones

			// Ascension System
			public const int MaxAscensionLevel = 10;               // Maximum ascension level
			public const float AscensionStatMultiplier = 0.25f;    // 25% permanent stat bonus per ascension
			public const float AscensionCostBase = 1000f;          // Base cost for first ascension
			public const float AscensionCostScaling = 2.0f;        // Cost doubles per ascension level
			public const int PrestigeRequiredForAscension = 10000; // Prestige levels needed for ascension
			public const float AscensionPowerBase = 10.0f;        // Base power multiplier for ascension
			public const long BaseAscensionCost = 1000000000;     // Base cost for ascension
			public const int AscensionCostScalingFactor = 100;    // Cost scaling factor for ascension
			public const int MaxAscensionMilestone = 5;           // Maximum ascension milestone level
			public const float AscensionMilestoneBaseBonus = 2.0f;// Base bonus for ascension milestones

			// Ascension Bonus Calculations
			public const float BaseStatBonusPerLevel = 0.02f;     // 2% bonus per ascension level
			public const float HealthScaling = 1.0f;              // Health stat scaling factor
			public const float AttackScaling = 1.0f;              // Attack stat scaling factor
			public const float DefenseScaling = 1.0f;             // Defense stat scaling factor
			public const float SpeedScaling = 1.0f;               // Speed stat scaling factor
			public const float ManaScaling = 1.0f;                // Mana stat scaling factor
			public const float DefaultStatScaling = 1.0f;         // Default scaling for new stats

			// Ascension Milestone Bonuses
			public static readonly Dictionary<int, float> AscensionMilestoneBonuses = new()
			{
				{ 5, 0.05f },    // +5% bonus at ascension level 5
                { 10, 0.10f },   // +10% bonus at ascension level 10
                { 25, 0.15f },   // +15% bonus at ascension level 25
                { 50, 0.25f },   // +25% bonus at ascension level 50
                { 100, 0.50f }   // +50% bonus at ascension level 100
            };

			// Time-Based Progression
			public const float TimeMultiplierBase = 0.01f;        // Base time-based multiplier
			public const float TimeMultiplierCap = 2.0f;          // Maximum time-based multiplier
			public const int TimeMultiplierHours = 1;             // Hours between time-based updates

			// Achievement System
			public const float AchievementBonus = 0.1f;           // Achievement completion bonus
			public const int MaxConcurrentAchievements = 100;     // Maximum active achievements
			public const int LevelMilestoneInterval = 100;        // Levels between milestones
			public const int MaxLevelMilestone = 1000;            // Maximum level milestone
			public const float LevelMilestoneBaseBonus = 1.0f;    // Base bonus for level milestones
		}


	}

}