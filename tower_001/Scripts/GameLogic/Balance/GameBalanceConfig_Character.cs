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
		/// - Used by: CharacterManager, CombatManager
		/// - Related Systems: Character creation, Combat calculations
		/// - Config Files: CharacterConfiguration
		/// </summary>
		public static class CharacterStats
		{
			// Base stat modifiers
			public const float BaseStatMultiplier = 1.0f;          // Base multiplier for all stats
			public const float LevelStatScaling = 0.1f;            // 10% increase per level
			public const float RankStatScaling = 0.2f;             // 20% increase per rank
			public const float AscensionStatScaling = 0.25f;       // 25% increase per ascension

			public static class Knight
			{
				public const float BaseHealth = 150f;              // Starting health for Knights
				public const float HealthGrowth = 0.15f;           // 15% health increase per level
				public const float BaseAttack = 15f;               // Starting attack for Knights
				public const float AttackGrowth = 0.08f;           // 8% attack increase per level
				public const float BaseDefense = 12f;              // Starting defense for Knights
				public const float DefenseGrowth = 0.12f;          // 12% defense increase per level
				public const float BaseSpeed = 8f;                 // Starting speed for Knights
				public const float SpeedGrowth = 0.10f;            // 10% speed increase per level
				public const float DefenseRankBonus = 0.25f;       // 25% extra defense per rank
				public const float HealthAscensionBonus = 0.3f;    // 30% extra health per ascension
			}

			public static class Mage
			{
				public const float BaseHealth = 120f;              // Starting health for Mages
				public const float HealthGrowth = 0.12f;           // 12% health increase per level
				public const float BaseMana = 150f;                // Starting mana for Mages
				public const float ManaGrowth = 0.15f;             // 15% mana increase per level
				public const float BaseDefense = 8f;               // Starting defense for Mages
				public const float DefenseGrowth = 0.10f;          // 10% defense increase per level
				public const float BaseSpeed = 10f;                // Starting speed for Mages
				public const float SpeedGrowth = 0.11f;            // 11% speed increase per level
				public const float ManaRankBonus = 0.25f;          // 25% extra mana per rank
				public const float SpellAscensionBonus = 0.3f;     // 30% extra spell power per ascension
			}
		}


	}

}