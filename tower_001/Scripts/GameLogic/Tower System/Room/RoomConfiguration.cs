using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Configuration for room generation and management, including type distributions, spacing, and difficulty scaling.
/// Provides defaults and customization options for room properties.
/// </summary>
public class RoomConfiguration
{
	// Room Generation Settings
	public int MinRoomsPerFloor { get; set; } = 20;
	public int MaxRoomsPerFloor { get; set; } = 100;
	public int BossFloorInterval { get; set; } = 100;  // Every 100 floors is a boss floor

	// Room Type Distribution (percentages)
	public Dictionary<RoomType, float> RoomTypeDistribution { get; set; } = new()
	{
		{ RoomType.Combat, 70f },
		{ RoomType.Rest, 16f },
		{ RoomType.Event, 4f },
		{ RoomType.MiniBoss, 3f },
		{ RoomType.Encounter, 3f },
		{ RoomType.Boss, 2f },
		{ RoomType.Reward, 1f }
	};

	// Room Type Spacing Requirements
	public Dictionary<RoomType, int> MinimumSpacing { get; set; } = new();

	// Difficulty Settings
	public float BaseRoomDifficultyIncrement { get; set; } = 0.1f;  // 10% increase per room
	public float BossRoomDifficultyMultiplier { get; set; } = 2.0f;
	public float MiniBossRoomDifficultyMultiplier { get; set; } = 1.5f;
	public float MaxFloorDifficulty { get; set; } = 10.0f;
}
