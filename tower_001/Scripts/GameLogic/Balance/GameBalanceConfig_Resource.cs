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
		/// - Used by: ResourceManager, ResourceStorage
		/// - Related Systems: Resource collection, Storage management
		/// </summary>
		public static class ResourceSystem
		{
			// Base resource configuration
			public const float BaseCollectionRate = 1.0f;         // Base rate for resource collection
			public const float BaseStorageCapacity = 1000f;       // Base storage capacity for resources
			public const float DefaultOverflowPercentage = 0.1f;  // Default 10% overflow allowance

			/// <summary>
			/// Configuration for resource collectors
			/// </summary>
			public static class Collectors
			{
				public const float BaseUnlockLevel = 5f;          // Base character level required to unlock collectors
				public const float DualCollectionBonus = 0.3f;    // 30% bonus for collecting two resources
				public const float TripleCollectionBonus = 0.5f;  // 50% bonus for collecting three resources
			}

			// Resource tier unlock requirements
			public static class UnlockRequirements
			{
				// Basic tier resources
				public static class Basic
				{
					public const float CharacterLevel = 1f;     // Level 1 required
				}

				// Advanced tier resources
				public static class Advanced
				{
					public const float CharacterLevel = 10f;    // Level 10 required
					public const float Prestige = 1f;           // 1 prestige required
				}

				// Premium tier resources
				public static class Premium
				{
					public const float CharacterLevel = 20f;    // Level 20 required
					public const float Ascension = 1f;          // 1 ascension required
				}
			}
		}
	}

}