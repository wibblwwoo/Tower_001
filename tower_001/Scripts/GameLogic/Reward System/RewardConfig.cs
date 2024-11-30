using System.Collections.Generic;
using static GlobalEnums;

public class RewardConfig
{
	public float BaseExperience { get; set; } = 1000f;  // Increased base experience
	public float BaseRoomReward { get; set; } = 10f;
	public Dictionary<RoomType, float> RoomMultipliers { get; set; } = new() {
		{ RoomType.Combat, 1.0f },
		{ RoomType.MiniBoss, 5.0f },     // Increased from 3.0f
        { RoomType.Boss, 10.0f },        // Increased from 5.0f
        { RoomType.Event, 1.5f },
		{ RoomType.Rest, 0.5f },
		{ RoomType.Reward, 2.0f },
		{ RoomType.Encounter, 1.2f }
	};

	public Dictionary<RewardTier, float> TierChances { get; set; } = new() {
		{ RewardTier.Common, 75f },      // Increased slightly
        { RewardTier.Uncommon, 15f },    // Reduced
        { RewardTier.Rare, 7f },
		{ RewardTier.Epic, 2.5f },
		{ RewardTier.Legendary, 0.5f }   // Reduced from 1f to make legendary items more rare
    };
}
