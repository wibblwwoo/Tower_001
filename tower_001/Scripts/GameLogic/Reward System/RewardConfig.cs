using System.Collections.Generic;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Balance;

using static Tower_001.Scripts.GameLogic.Balance.GameBalanceConfig;


/// <summary>
/// Configuration class for the reward system, using constants defined in GameBalanceConfig
/// </summary>
/// <remarks>
/// Dependencies and Usage:
/// - Uses: GameBalanceConfig.RewardConstants for base values and scaling
/// - Used by: RewardCalculator, RoomManager
/// - Related Systems: Tower progression, Loot system
/// </remarks>
public class RewardConfig
{
	/// <summary>
	/// Base experience points awarded for completing a room
	/// </summary>
	public float BaseExperience { get; set; } = RewardConstants.BaseExperience;

	/// <summary>
	/// Base number of rewards given per room completion
	/// </summary>
	public float BaseRoomReward { get; set; } = RewardConstants.BaseRoomReward;

	/// <summary>
	/// Reward multipliers for different room types
	/// </summary>
	public Dictionary<RoomType, float> RoomMultipliers { get; set; } = new() {
		{ RoomType.Combat, RewardConstants.RoomMultipliers.Combat },
		{ RoomType.MiniBoss, RewardConstants.RoomMultipliers.MiniBoss },
		{ RoomType.Boss, RewardConstants.RoomMultipliers.Boss },
		{ RoomType.Event, RewardConstants.RoomMultipliers.Event },
		{ RoomType.Rest, RewardConstants.RoomMultipliers.Rest },
		{ RoomType.Reward, RewardConstants.RoomMultipliers.Reward },
		{ RoomType.Encounter, RewardConstants.RoomMultipliers.Encounter }
	};

	/// <summary>
	/// Base probability for each reward tier
	/// </summary>
	public Dictionary<RewardTier, float> TierChances { get; set; } = new() {
		{ RewardTier.Common, RewardConstants.TierChances.Common },
		{ RewardTier.Uncommon, RewardConstants.TierChances.Uncommon },
		{ RewardTier.Rare, RewardConstants.TierChances.Rare },
		{ RewardTier.Epic, RewardConstants.TierChances.Epic },
		{ RewardTier.Legendary, RewardConstants.TierChances.Legendary }
	};
}
