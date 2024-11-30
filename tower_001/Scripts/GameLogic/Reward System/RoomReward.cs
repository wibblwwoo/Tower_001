using static GlobalEnums;
using System.Collections.Generic;

public partial class RoomReward
{
	public float Experience { get; set; }
	public List<(RewardTier tier, int quantity)> Rewards { get; set; } = new();
}
