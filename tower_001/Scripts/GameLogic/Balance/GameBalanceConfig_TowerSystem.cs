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
		/// - Used by: RoomManager, FloorManager
		/// - Related Systems: Combat system, Room generation
		/// - Config Files: RoomConfiguration
		/// </summary>
		public static class RoomDifficulty
		{
			public const float BossMultiplier = 3.0f;              // Boss rooms are 3x harder
			public const float MiniBossMultiplier = 2.0f;          // Mini-boss rooms are 2x harder
			public const float CombatMultiplier = 1.0f;            // Standard difficulty for combat rooms
			public const float EventMultiplier = 0.8f;             // Event rooms are 20% easier
			public const float RestMultiplier = 0.5f;              // Rest rooms are 50% easier
			public const float PositionScalingBase = 1.1f;         // 10% increase per room position
			public const float FloorNumberScaling = 0.02f;         // 2% increase per floor number
			public const float BaseRoomDifficultyIncrement = 0.1f; // 10% increase per room
			public const float BossRoomDifficultyMultiplier = 2.0f;// Boss rooms are 2x harder
			public const float MiniBossRoomDifficultyMultiplier = 1.5f; // Mini-boss rooms are 50% harder
			public const float MaxFloorDifficulty = 10.0f;         // Maximum difficulty cap
		}

		/// <summary>
		/// Dependencies and Usage:
		/// - Used by: FloorManager, TowerManager
		/// - Related Systems: Floor generation, Tower progression
		/// - Config Files: FloorConfiguration
		/// </summary>
		public static class FloorDifficulty
		{
			public const float BaseScalingFactor = 1.15f;          // 15% increase per floor
			public const float PositionBonus = 0.05f;              // 5% bonus per position
			public const float MilestoneMultiplier = 2.0f;         // 50% bonus at milestone floors
		}

		/// <summary>
		/// Dependencies and Usage:
		/// - Used by: RoomManager, RoomConfiguration
		/// - Related Systems: Room generation, Floor layout
		/// - Config Files: RoomConfiguration
		/// </summary>
		public static class RoomGeneration
		{
			public const int MinRoomsPerFloor = 20;               // Minimum rooms required per floor
			public const int MaxRoomsPerFloor = 100;              // Maximum rooms allowed per floor
			public const int BossFloorInterval = 100;             // Boss floor appears every 100 floors
		}

		/// <summary>
		/// Dependencies and Usage:
		/// - Used by: RoomManager, RoomConfiguration
		/// - Related Systems: Room type selection, Floor layout
		/// - Config Files: RoomConfiguration
		/// </summary>
		public static class RoomDistribution
		{
			public const float CombatRoomPercentage = 70f;        // 70% chance for combat rooms
			public const float RestRoomPercentage = 16f;          // 16% chance for rest rooms
			public const float EventRoomPercentage = 4f;          // 4% chance for event rooms
			public const float MiniBossRoomPercentage = 3f;       // 3% chance for mini-boss rooms
			public const float EncounterRoomPercentage = 3f;      // 3% chance for encounter rooms
			public const float BossRoomPercentage = 2f;           // 2% chance for boss rooms
			public const float RewardRoomPercentage = 1f;         // 1% chance for reward rooms
		}
	}

}