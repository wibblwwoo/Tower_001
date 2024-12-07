using static GlobalEnums;
using System.Collections.Generic;
using System;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
using static Tower_001.Scripts.GameLogic.Balance.GameBalanceConfig;
using Tower.GameLogic.Core;

/// <summary>
/// Calculates and generates rewards for room completion in the tower system.
/// </summary>
/// <remarks>
/// Dependencies and Usage:
/// - Uses: GameBalanceConfig for reward scaling and probabilities
/// - Used by: RoomManager, TowerProgressionSystem
/// - Related Systems: Tower system, Progression system, Inventory system
/// - Configuration: Uses RewardConfig for base values and multipliers
/// 
/// The reward calculation takes into account:
/// - Room type (Boss, MiniBoss, Standard)
/// - Floor number (for progression scaling)
/// - Character power (for experience scaling)
/// - Room difficulty (for challenge bonuses)
/// </remarks>
public static partial class RewardCalculator
{
    /// <summary>
    /// Calculates the rewards for completing a room, including experience and items.
    /// </summary>
    /// <param name="room">The room being completed</param>
    /// <param name="floor">The floor containing the room</param>
    /// <param name="character">The character completing the room</param>
    /// <param name="config">Reward configuration settings</param>
    /// <returns>A RoomReward containing experience and item rewards</returns>
    public static RoomReward CalculateRewards(RoomData room, FloorData floor, Character character, RewardConfig config)
    {
        var reward = new RoomReward();

        // Get multiplier with fallback for undefined room types
        float roomMultiplier = config.RoomMultipliers.GetValueOrDefault(room.Type, 1.0f);

        // Calculate experience based on room difficulty and character power
        float expMultiplier = CalculateExpMultiplier(room, character);
        reward.Experience = config.BaseExperience * expMultiplier * roomMultiplier;

        // Generate item rewards based on room type and floor number
        int baseQuantity = CalculateBaseQuantity(room, floor.FloorNumber);
        reward.Rewards = GenerateRewards(baseQuantity, floor.FloorNumber, config.TierChances);

        return reward;
    }

    /// <summary>
    /// Calculates the experience multiplier based on the power difference between room and character.
    /// </summary>
    /// <remarks>
    /// Experience is scaled based on challenge level:
    /// - Overpowered characters receive reduced experience
    /// - Challenging rooms provide bonus experience
    /// - All values are capped within configured limits
    /// </remarks>
    private static float CalculateExpMultiplier(RoomData room, Character character)
    {
        // Calculate the ratio between room difficulty and character power
        float powerRatio = room.BaseDifficulty / character.Power;

        // If player is overpowered, reduce experience (minimum 10% of base)
        if (powerRatio < RewardConstants.ExpScaling.PowerRatioThreshold)
        {
            return Math.Max(RewardConstants.ExpScaling.MinMultiplier, powerRatio);
        }

        // If room is challenging, increase experience (maximum 300% of base)
        return Math.Min(RewardConstants.ExpScaling.MaxMultiplier, powerRatio);
    }

    /// <summary>
    /// Calculates the base quantity of rewards based on room type and floor number.
    /// </summary>
    /// <remarks>
    /// Reward quantities scale with:
    /// - Room type (Boss rooms give most, standard rooms give least)
    /// - Floor number (higher floors give more rewards)
    /// - Always ensures at least minimum reward quantity
    /// </remarks>
    private static int CalculateBaseQuantity(RoomData room, int floorNumber)
    {
        // Determine base quantity by room type
        float baseQuantity = room.Type switch
        {
            RoomType.Boss => RewardConstants.BaseQuantities.BossRoom,      // Boss rooms give most rewards
            RoomType.MiniBoss => RewardConstants.BaseQuantities.MiniBossRoom,  // Mini-bosses give medium rewards
            RoomType.Reward => RewardConstants.BaseQuantities.RewardRoom,   // Treasure rooms give bonus rewards
            _ => RewardConstants.BaseQuantities.StandardRoom                // Standard rooms give base rewards
        };

        // Scale reward quantity with floor number (10% increase per floor)
        baseQuantity *= (1 + (floorNumber * RewardConstants.BaseQuantities.FloorScaling));

        // Ensure at least minimum reward quantity
        return Math.Max(RewardConstants.BaseQuantities.MinimumReward, (int)baseQuantity);
    }

    /// <summary>
    /// Generates a list of rewards with their tiers and quantities.
    /// </summary>
    /// <remarks>
    /// The reward generation process:
    /// 1. Adjusts tier chances based on floor progression
    /// 2. Rolls for each reward's tier individually
    /// 3. Combines same-tier rewards into stacks
    /// </remarks>
    private static List<(RewardTier tier, int quantity)> GenerateRewards(int baseQuantity, int floorNumber, Dictionary<RewardTier, float> tierChances)
    {
        var rewards = new List<(RewardTier tier, int quantity)>();

        // Adjust tier chances based on floor progression
        var adjustedChances = AdjustTierChances(tierChances, floorNumber);

        // Generate each reward individually
        for (int i = 0; i < baseQuantity; i++)
        {
            // Roll for reward tier
            var tier = RollRewardTier(adjustedChances);
            var existing = rewards.FirstOrDefault(r => r.tier == tier);

            // Stack same-tier rewards together
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

    /// <summary>
    /// Adjusts the tier chances based on floor progression.
    /// </summary>
    /// <remarks>
    /// Higher floors provide better chances for rare items through:
    /// - Progressive improvement every N floors
    /// - Bonus chances scaled by tier value
    /// - Maximum bonus cap to prevent excessive scaling
    /// </remarks>
    private static Dictionary<RewardTier, float> AdjustTierChances(Dictionary<RewardTier, float> baseChances, int floorNumber)
    {
        // Calculate progression tier based on floor interval
        int tier = floorNumber / RewardConstants.TierScaling.FloorInterval;
        var adjusted = new Dictionary<RewardTier, float>(baseChances);

        if (tier > 0)
        {
            // Improve chances for better rewards based on progression
            foreach (var kvp in baseChances)
            {
                int tierValue = (int)kvp.Key;
                // Calculate bonus chance with tier scaling and maximum cap
                float bonus = Math.Min(
                    tierValue * tier * RewardConstants.TierScaling.TierBonusMultiplier, 
                    RewardConstants.TierScaling.MaxBonusPercent
                );
                adjusted[kvp.Key] += bonus;
            }
        }

        return adjusted;
    }

    /// <summary>
    /// Rolls for a reward tier based on the provided chances.
    /// </summary>
    /// <remarks>
    /// Uses weighted random selection where:
    /// - Higher chances mean more likely to be selected
    /// - Total chances don't need to sum to 100
    /// - Returns lowest tier if no tier is selected (fallback)
    /// </remarks>
    private static RewardTier RollRewardTier(Dictionary<RewardTier, float> tierChances)
    {
        // Calculate total chance for normalization
        float totalChance = tierChances.Values.Sum();
        float roll = (float)RandomManager.Instance.NextDouble() * totalChance;

        // Roll for tier using weighted random selection
        float currentTotal = 0;
        foreach (var kvp in tierChances.OrderByDescending(x => (int)x.Key))
        {
            currentTotal += kvp.Value;
            if (roll <= currentTotal)
            {
                return kvp.Key;
            }
        }

        // Fallback to lowest tier if no tier selected
        return RewardTier.Common;
    }
}