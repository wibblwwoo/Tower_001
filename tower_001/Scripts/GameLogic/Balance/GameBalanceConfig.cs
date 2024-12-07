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
        /// - Used by: BuffManager, CombatManager
        /// - Related Systems: Status effects, Combat modifiers
        /// </summary>
        public static class ModifierLimits
        {
            // Buff/Debuff constraints
            public const float MaxBuffPercentage = 2.0f;     // 200% max buff
            public const float MaxDebuffPercentage = -0.75f; // -75% max debuff
            public const float MaxFlatBuff = 1000f;
            public const float MaxFlatDebuff = -1000f;
            
            // Duration limits
            public const float MaxBuffDuration = 300f;       // 5 minutes
            public const float MinBuffDuration = 1f;         // 1 second
        }

       

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: CharacterManager, CombatManager
        /// - Related Systems: Power scaling, Combat balance
        /// </summary>
        public static class PowerCalculation
        {
              // Power level calculation weights
            public const float HealthWeight = 1.0f;      // Base weight for health contribution to power
            public const float AttackWeight = 2.0f;      // Double weight for attack in power calculation
            public const float DefenseWeight = 1.5f;     // 1.5x weight for defense in power calculation
            public const float SpeedWeight = 1.0f;       // Base weight for speed contribution to power

            // Additional power factors
            public const float RankMultiplier = 1.2f;    // 20% power increase per rank
            public const float AscensionMultiplier = 1.5f; // 50% power increase per ascension level
            public const float LevelScaling = 0.1f;      // 10% power increase per character level
        }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: CharacterStatSystem
        /// - Related Systems: Stat calculation, Character progression
        /// </summary>
        public static class DerivedStats
        {
            // Health scaling from base stats
            public const float HealthPerStamina = 10f;       // Each point of Stamina gives 10 Health
            public const float HealthPerStrength = 2f;       // Each point of Strength gives 2 Health
            
            // Attack scaling from base stats
            public const float AttackPerStrength = 2f;       // Each point of Strength gives 2 Attack
            public const float AttackPerDexterity = 1f;      // Each point of Dexterity gives 1 Attack
            
            // Defense scaling from base stats
            public const float DefensePerStamina = 1f;       // Each point of Stamina gives 1 Defense
            public const float DefensePerStrength = 0.5f;    // Each point of Strength gives 0.5 Defense
            
            // Speed scaling from base stats
            public const float SpeedPerDexterity = 1f;       // Each point of Dexterity gives 1 Speed
            public const float SpeedPerIntelligence = 0.5f;  // Each point of Intelligence gives 0.5 Speed
            
            // Critical scaling from base stats
            public const float CritChancePerDexterity = 0.1f;    // Each point of Dexterity gives 0.1% Crit Chance
            public const float CritDamagePerStrength = 0.5f;     // Each point of Strength gives 0.5% Crit Damage
            
            // Resource scaling from base stats
            public const float ManaPerIntelligence = 5f;     // Each point of Intelligence gives 5 Mana
            public const float ManaRegenPerIntelligence = 0.1f;  // Each point of Intelligence gives 0.1 Mana Regen
        }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: CharacterStatSystem
        /// - Related Systems: Idle progression, Character growth
        /// </summary>
        public static class IdleProgression
        {
            // Base progression rates (per minute)
            public const float BaseExperienceRate = 0.05f;    // Base experience gained per minute
            public const float BaseStatGrowthRate = 0.001f;   // Base stat growth per minute
            
            // Time scaling
            public const float MaxOfflineTime = 72f;         // Maximum hours of offline progression
            public const float OfflineRateMultiplier = 0.5f; // Offline progression is 50% as effective
            
            // Level scaling
            public const float LevelPenaltyFactor = 0.98f;   // Each level reduces gains by 2%
            public const float MinLevelMultiplier = 0.2f;    // Minimum multiplier from level penalty
            
            // Stat-specific rates (multiplier to base rate)
            public static class StatRates
            {
                public const float Strength = 1.0f;      // Standard rate
                public const float Dexterity = 1.0f;     // Standard rate
                public const float Intelligence = 1.2f;   // 20% faster
                public const float Stamina = 0.8f;       // 20% slower
            }
            
            // Catch-up mechanics
            public const float CatchupThreshold = 0.5f;      // Start catch-up when below 50% of expected level
            public const float CatchupMultiplier = 1.5f;     // 50% faster progression when catching up
        }

        /// <summary>
        /// Configuration constants for the reward system, controlling experience gains and item drops
        /// </summary>
        /// <remarks>
        /// Dependencies and Usage:
        /// - Used by: RewardConfig, RewardCalculator
        /// - Related Systems: Tower progression, Loot system
        /// - Configuration: Defines base values and scaling factors for rewards
        /// </remarks>
        public static class RewardConstants
        {
            // Base Values
            public const float BaseExperience = 1000f;         // Base experience reward for completing a standard room
            public const float BaseRoomReward = 10f;          // Base number of items rewarded per room

            // Experience Scaling
            public static class ExpScaling
            {
                public const float PowerRatioThreshold = 1.0f;    // Threshold for determining if player is overpowered
                public const float MinMultiplier = 0.1f;         // Minimum exp multiplier when player is overpowered
                public const float MaxMultiplier = 3.0f;         // Maximum exp multiplier for challenging rooms
            }

            // Base Reward Quantities
            public static class BaseQuantities
            {
                public const int BossRoom = 10;                  // Base number of rewards from boss rooms
                public const int MiniBossRoom = 5;              // Base number of rewards from mini-boss rooms
                public const int RewardRoom = 3;                // Base number of rewards from treasure rooms
                public const int StandardRoom = 1;              // Base number of rewards from standard rooms
                public const int MinimumReward = 1;             // Minimum number of rewards per room
                public const float FloorScaling = 0.1f;         // Reward quantity increase per floor (10%)
            }

            // Room Type Multipliers
            public static class RoomMultipliers
            {
                public const float Combat = 1.0f;             // Standard combat room multiplier
                public const float MiniBoss = 5.0f;           // Miniboss rooms give 5x rewards
                public const float Boss = 10.0f;              // Boss rooms give 10x rewards
                public const float Event = 1.5f;              // Special event rooms give 1.5x rewards
                public const float Rest = 0.5f;               // Rest rooms give reduced rewards
                public const float Reward = 2.0f;             // Treasure rooms give double rewards
                public const float Encounter = 1.2f;          // Random encounter rooms give slight bonus
            }

            // Item Tier Drop Chances (in percentage)
            public static class TierChances
            {
                public const float Common = 75f;              // 75% chance for common items
                public const float Uncommon = 15f;            // 15% chance for uncommon items
                public const float Rare = 7f;                 // 7% chance for rare items
                public const float Epic = 2.5f;               // 2.5% chance for epic items
                public const float Legendary = 0.5f;          // 0.5% chance for legendary items
            }

            // Tier Chance Scaling
            public static class TierScaling
            {
                public const float FloorProgression = 0.05f;  // 5% better chances per floor
                public const float BossBonus = 0.2f;         // 20% better chances in boss rooms
                public const float MiniBossBonus = 0.1f;     // 10% better chances in miniboss rooms
                public const int FloorInterval = 10;         // Improve chances every 10 floors
                public const float TierBonusMultiplier = 2f; // Multiplier for tier bonus calculation
                public const float MaxBonusPercent = 20f;    // Maximum bonus percentage for tier chances
            }
        }

       
        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: RandomManager
        /// - Related Systems: All systems using randomization
        /// </summary>
        public static class RandomGeneration
        {
            /// <summary>
            /// The seed string used for random number generation.
            /// This will be converted to an integer using GetHashCode().
            /// Empty or null string will use system time as seed.
            /// </summary>
            public const string DefaultSeed = "";  // Empty string means use system time
        }
    }
}