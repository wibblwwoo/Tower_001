using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

/// <summary>
/// Configuration for room generation and management, including type distributions, spacing, and difficulty scaling.
/// Provides defaults and customization options for room properties.
/// </summary>
/// <summary>
/// Configuration for room generation and management, including type distributions, spacing, and difficulty scaling.
/// Provides defaults and customization options for room properties.
/// </summary>
public class RoomConfiguration
{
    // Room Generation Settings
    /// <summary>
    /// Minimum number of rooms that must be generated per floor
    /// Default: 20
    /// </summary>
    public int MinRoomsPerFloor { get; set; } = GameBalanceConfig.RoomGeneration.MinRoomsPerFloor;

    /// <summary>
    /// Maximum number of rooms allowed per floor
    /// Default: 100
    /// </summary>
    public int MaxRoomsPerFloor { get; set; } = GameBalanceConfig.RoomGeneration.MaxRoomsPerFloor;

    /// <summary>
    /// Number of floors between boss rooms
    /// Default: 100 (every 100 floors is a boss floor)
    /// </summary>
    public int BossFloorInterval { get; set; } = GameBalanceConfig.RoomGeneration.BossFloorInterval;

    /// <summary>
    /// Percentage distribution of different room types
    /// Key: Room type
    /// Value: Percentage chance (0-100) of generating this type
    /// </summary>
    public Dictionary<RoomType, float> RoomTypeDistribution { get; set; } = new()
    {
        { RoomType.Combat, GameBalanceConfig.RoomDistribution.CombatRoomPercentage },
        { RoomType.Rest, GameBalanceConfig.RoomDistribution.RestRoomPercentage },
        { RoomType.Event, GameBalanceConfig.RoomDistribution.EventRoomPercentage },
        { RoomType.MiniBoss, GameBalanceConfig.RoomDistribution.MiniBossRoomPercentage },
        { RoomType.Encounter, GameBalanceConfig.RoomDistribution.EncounterRoomPercentage },
        { RoomType.Boss, GameBalanceConfig.RoomDistribution.BossRoomPercentage },
        { RoomType.Reward, GameBalanceConfig.RoomDistribution.RewardRoomPercentage }
    };

    /// <summary>
    /// Minimum number of rooms required between rooms of the same type
    /// Used to prevent clustering of similar room types
    /// </summary>
    public Dictionary<RoomType, int> MinimumSpacing { get; set; } = new();

    /// <summary>
    /// Percentage increase in difficulty per room
    /// Default: 0.1 (10% increase per room)
    /// </summary>
    public float BaseRoomDifficultyIncrement { get; set; } = GameBalanceConfig.RoomDifficulty.BaseRoomDifficultyIncrement;

    /// <summary>
    /// Multiplier applied to boss room difficulty
    /// Default: 2.0 (twice as difficult as normal rooms)
    /// </summary>
    public float BossRoomDifficultyMultiplier { get; set; } = GameBalanceConfig.RoomDifficulty.BossRoomDifficultyMultiplier;

    /// <summary>
    /// Multiplier applied to mini-boss room difficulty
    /// Default: 1.5 (50% more difficult than normal rooms)
    /// </summary>
    public float MiniBossRoomDifficultyMultiplier { get; set; } = GameBalanceConfig.RoomDifficulty.MiniBossRoomDifficultyMultiplier;

    /// <summary>
    /// Maximum difficulty level allowed for a floor
    /// Used to cap difficulty scaling
    /// Default: 10.0
    /// </summary>
    public float MaxFloorDifficulty { get; set; } = GameBalanceConfig.RoomDifficulty.MaxFloorDifficulty;
}