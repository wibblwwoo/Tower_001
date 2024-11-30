using static GlobalEnums;
using System.Collections.Generic;
using System;
using System.Linq;

public static partial class RewardCalculator
{
	private static readonly Random _random = new Random();

	public static RoomReward CalculateRewards(RoomData room, FloorData floor, Character character, RewardConfig config)
	{
		var reward = new RoomReward();

		// Get multiplier with fallback for undefined room types
		float roomMultiplier = config.RoomMultipliers.GetValueOrDefault(room.Type, 1.0f);

		// Calculate base experience
		float expMultiplier = CalculateExpMultiplier(room, character);
		reward.Experience = config.BaseExperience * expMultiplier * roomMultiplier;

		// Calculate reward quantities and tiers
		int baseQuantity = CalculateBaseQuantity(room, floor.FloorNumber);
		reward.Rewards = GenerateRewards(baseQuantity, floor.FloorNumber, config.TierChances);

		return reward;
	}


	private static float CalculateExpMultiplier(RoomData room, Character character)
	{
		float powerRatio = room.BaseDifficulty / character.Power;

		// If player is overpowered, reduce experience
		if (powerRatio < 1f)
		{
			return Math.Max(0.1f, powerRatio);
		}

		// If room is challenging, increase experience
		return Math.Min(3f, powerRatio);
	}

	private static int CalculateBaseQuantity(RoomData room, int floorNumber)
	{
		float baseQuantity = room.Type switch
		{
			RoomType.Boss => 10,
			RoomType.MiniBoss => 5,
			RoomType.Reward => 3,
			_ => 1
		};

		// Scale with floor number
		baseQuantity *= (1 + (floorNumber * 0.1f));

		return Math.Max(1, (int)baseQuantity);
	}


	private static List<(RewardTier tier, int quantity)> GenerateRewards(int baseQuantity, int floorNumber, Dictionary<RewardTier, float> tierChances)
	{
		var rewards = new List<(RewardTier tier, int quantity)>();

		// Adjust tier chances based on floor number
		var adjustedChances = AdjustTierChances(tierChances, floorNumber);

		// Generate rewards
		for (int i = 0; i < baseQuantity; i++)
		{
			var tier = RollRewardTier(adjustedChances);
			var existing = rewards.FirstOrDefault(r => r.tier == tier);

			if (existing.tier == tier)
			{
				rewards.Remove(existing);
				rewards.Add((tier, existing.quantity + 1));
			}
			else
			{
				rewards.Add((tier, 1));
			}
		}

		return rewards;
	}

	private static Dictionary<RewardTier, float> AdjustTierChances(Dictionary<RewardTier, float> baseChances, int floorNumber)
	{
		// Every 10 floors, improve chances for better rewards
		int tier = floorNumber / 10;
		var adjusted = new Dictionary<RewardTier, float>(baseChances);

		if (tier > 0)
		{
			// Shift chances towards better rewards
			foreach (var kvp in baseChances)
			{
				int tierValue = (int)kvp.Key;
				float bonus = Math.Min(tierValue * tier * 2f, 20f);
				adjusted[kvp.Key] += bonus;
			}
		}

		return adjusted;
	}

	private static RewardTier RollRewardTier(Dictionary<RewardTier, float> chances)
	{
		float total = chances.Values.Sum();
		float roll = (float)_random.NextDouble() * total;
		float current = 0;

		foreach (var kvp in chances)
		{
			current += kvp.Value;
			if (roll <= current)
			{
				return kvp.Key;
			}
		}

		return RewardTier.Common;
	}
}