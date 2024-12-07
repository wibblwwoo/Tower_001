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
		/// - Used by: IdleCharacterManager
		/// - Related Systems: Idle character progression
		/// </summary>
		public static class IdleCharacterStats
		{
			// Base Values
			public const float DefaultStatValue = 10f;           // Starting value for all basic stats
			public const float BaseMultiplier = 1.0f;           // Base multiplier for all idle gains
			public const int InitialLevel = 1;                  // Starting level for new characters

			// Progression
			public const float LevelUpStatMultiplier = 1.1f;    // 10% stat increase per level
			public const float ExperienceCurveExponent = 1.5f;  // Exponential scaling for XP requirements
			public const float BaseExperienceRequired = 100f;   // Base XP needed for first level
			public const float BaseIdleGainRate = 1.0f;        // Base rate for idle progression
			public const float IdleGainMultiplier = 1.0f;      // Global multiplier for idle gains

			// Individual Stats
			public static class Strength
			{
				public const float BaseValue = DefaultStatValue;  // Base strength value (10)
				public const float IdleGainRate = 1.0f;          // Rate of passive strength gain
			}

			public static class Dexterity
			{
				public const float BaseValue = DefaultStatValue;  // Base dexterity value (10)
				public const float IdleGainRate = 1.0f;          // Rate of passive dexterity gain
			}

			public static class Intelligence
			{
				public const float BaseValue = DefaultStatValue;  // Base intelligence value (10)
				public const float IdleGainRate = 1.0f;          // Rate of passive intelligence gain
			}

			public static class Stamina
			{
				public const float BaseValue = DefaultStatValue;  // Base stamina value (10)
				public const float IdleGainRate = 1.0f;          // Rate of passive stamina gain
			}

			public static class DerivedStats
			{
				public const float HealthPerStamina = 10f;      // Each point of stamina gives 10 health
				public const float AttackPerStrength = 2f;      // Each point of strength gives 2 attack
				public const float DefensePerStamina = 1f;      // Each point of stamina gives 1 defense
				public const float SpeedPerDexterity = 1.5f;    // Each point of dexterity gives 1.5 speed
			}
		}
	}

}