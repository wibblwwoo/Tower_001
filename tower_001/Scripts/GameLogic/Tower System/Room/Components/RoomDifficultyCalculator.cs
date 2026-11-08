using Godot;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Tower_System.Room
{
    /// <summary>
    /// Handles all difficulty calculations for rooms
    /// </summary>
    public class RoomDifficultyCalculator
    {
        /// <summary>
        /// Calculates the difficulty of a room based on its position, floor number, and type.
        /// </summary>
        public float CalculateRoomDifficulty(RoomType roomType, int roomPosition, int floorNumber)
        {
            float typeMultiplier = roomType switch
            {
                RoomType.Boss => GameBalanceConfig.RoomDifficulty.BossMultiplier,
                RoomType.MiniBoss => GameBalanceConfig.RoomDifficulty.MiniBossMultiplier,
                RoomType.Combat => GameBalanceConfig.RoomDifficulty.CombatMultiplier,
                RoomType.Event => GameBalanceConfig.RoomDifficulty.EventMultiplier,
                RoomType.Rest => GameBalanceConfig.RoomDifficulty.RestMultiplier,
                _ => GameBalanceConfig.RoomDifficulty.CombatMultiplier
            };

            float positionScaling = Mathf.Pow(GameBalanceConfig.RoomDifficulty.PositionScalingBase, roomPosition);
            float floorScaling = 1 + (floorNumber * GameBalanceConfig.RoomDifficulty.FloorNumberScaling);

            return typeMultiplier * positionScaling * floorScaling;
        }
    }
}