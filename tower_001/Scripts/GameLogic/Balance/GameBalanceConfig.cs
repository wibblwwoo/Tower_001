using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Balance
{
    /// <summary>
    /// Static configuration class containing all game balance values.
    /// This centralizes magic numbers used throughout the codebase for easier maintenance and tweaking.
    /// </summary>
    public static class GameBalanceConfig
    {
        public static class RoomDifficulty
        {
            // Used in RoomManager.CalculateRoomDifficulty for Boss room type multiplier
            public const float BossMultiplier = 3.0f;

            // Used in RoomManager.CalculateRoomDifficulty for MiniBoss room type multiplier
            public const float MiniBossMultiplier = 2.0f;

            // Used in RoomManager.CalculateRoomDifficulty for standard Combat room type multiplier
            public const float CombatMultiplier = 1.0f;

            // Used in RoomManager.CalculateRoomDifficulty for Event room type multiplier
            public const float EventMultiplier = 0.8f;

            // Used in RoomManager.CalculateRoomDifficulty for Rest room type multiplier
            public const float RestMultiplier = 0.5f;

            // Used in RoomManager.CalculateRoomDifficulty for exponential scaling based on room position
            public const float PositionScalingBase = 1.1f;

            // Used in RoomManager.CalculateRoomDifficulty for additional scaling per floor number
            public const float FloorNumberScaling = 0.02f;
        }

        public static class FloorDifficulty
        {
            // Used in FloorDifficulty.CalculateDifficulty for exponential base scaling
            public const float BaseScalingFactor = 1.15f;

            // Used in FloorDifficulty.CalculateDifficulty for floor position bonus scaling
            public const float PositionBonus = 0.05f;

            // Used in FloorDifficulty.CalculateDifficulty and TowerManager.CreateSampleTower for milestone floors
            public const float MilestoneMultiplier = 1.5f;
        }

        public static class TowerDifficulty
        {
            // Used in TowerManager.CreateSampleTower for base tower difficulty
            public const float BaseValue = 1.0f;

            // Used in TowerManager.CreateSampleTower and GetTowerDifficulty for minimum allowed difficulty
            public const float MinDifficulty = 0.5f;

            // Used in TowerManager.CreateSampleTower and GetTowerDifficulty for maximum allowed difficulty
            public const float MaxDifficulty = 10.0f;

            // Used in TowerManager.CreateSampleTower and GetTowerDifficulty for level-based scaling
            public const float LevelScalingFactor = 0.15f;

            // Used in TowerManager.CreateSampleTower for tower level requirements
            public const int MinLevel = 1;
            public const int MaxLevel = 10;
            public const int RecommendedLevel = 1;
            public const float RequirementScalingFactor = 0.1f;
        }

        public static class CharacterStats
        {
            public static class Knight
            {
                // Used in Knight.InitializeStats for base health value
                public const float BaseHealth = 150f;
                // Used in Knight.InitializeStats for health growth per level
                public const float HealthGrowth = 0.15f;
                
                // Used in Knight.InitializeStats for base attack value
                public const float BaseAttack = 15f;
                // Used in Knight.InitializeStats for attack growth per level
                public const float AttackGrowth = 0.08f;
                
                // Used in Knight.InitializeStats for base defense value
                public const float BaseDefense = 12f;
                // Used in Knight.InitializeStats for defense growth per level
                public const float DefenseGrowth = 0.12f;
                
                // Used in Knight.InitializeStats for base speed value
                public const float BaseSpeed = 8f;
                // Used in Knight.InitializeStats for speed growth per level
                public const float SpeedGrowth = 0.10f;
            }

            public static class Mage
            {
                // Used in Mage.InitializeStats for base health value
                public const float BaseHealth = 120f;
                // Used in Mage.InitializeStats for health growth per level
                public const float HealthGrowth = 0.12f;
                
                // Used in Mage.InitializeStats for base mana value
                public const float BaseMana = 150f;
                // Used in Mage.InitializeStats for mana growth per level
                public const float ManaGrowth = 0.15f;
                
                // Used in Mage.InitializeStats for base defense value
                public const float BaseDefense = 8f;
                // Used in Mage.InitializeStats for defense growth per level
                public const float DefenseGrowth = 0.10f;
                
                // Used in Mage.InitializeStats for base speed value
                public const float BaseSpeed = 10f;
                // Used in Mage.InitializeStats for speed growth per level
                public const float SpeedGrowth = 0.11f;
            }
        }

        public static class CharacterProgression
        {
            public static class PowerCalculation
            {
                // Base multipliers for power calculation
                public const float LevelMultiplier = 1.0f;
                public const float AttackMultiplier = 0.5f;
                public const float HealthMultiplier = 0.1f;
                public const float DefenseMultiplier = 0.2f;
                public const float SpeedMultiplier = 0.15f;

                // Rank/Star system multipliers
                public const float BaseRankMultiplier = 0.2f;  // 20% increase per rank
                public const float PrestigeMultiplier = 0.5f;  // 50% increase per prestige level
                public const int MaxRank = 5;  // Maximum star/rank level
            }

            public static class ExperienceSharing
            {
                // Active vs Passive character rates
                public const float ActiveCharacterRate = 1.0f;  // 100% for active character
                public const float PassiveCharacterRate = 0.1f; // 10% for passive roster characters
                
                // Team bonuses
                public const float SameClassBonus = 0.05f;     // 5% bonus for same class characters
                public const float ElementalBonus = 0.05f;     // 5% bonus for elemental synergy
                public const int MaxTeamSize = 4;              // Maximum characters in active team
                
                // Catchup mechanics
                public const float LevelGapMultiplier = 0.2f;  // Bonus exp multiplier per level below highest
                public const int MaxLevelGapBonus = 5;         // Maximum levels to consider for gap bonus
            }

            public static class CharacterUnlock
            {
                // Base unlock requirements
                public const int InitialCharacterSlots = 1;
                public const int MaxCharacterSlots = 12;
                public const float SlotCostMultiplier = 2.0f;  // Cost doubles per slot

                // Rank/Star requirements
                public const int BaseRankUpLevel = 20;         // Level required for first rank up
                public const float RankUpLevelMultiplier = 1.5f; // Level requirement scaling per rank
                
                // Power thresholds
                public const float UnlockPowerThreshold = 1000.0f;  // Base power needed for unlocks
                public const float PowerThresholdMultiplier = 2.0f; // Power requirement scaling
            }

            public static class ExperienceBonuses
            {
                // Class-specific bonuses
                public static class Knight
                {
                    public const float CombatBonus = 0.15f;    // 15% bonus from combat
                    public const float DefenseBonus = 0.10f;   // 10% bonus when taking damage
                }

                public static class Mage
                {
                    public const float SkillBonus = 0.15f;     // 15% bonus from skill usage
                    public const float ElementalBonus = 0.10f; // 10% bonus from elemental damage
                }

                // Power level bonuses
                public const float PowerThresholdBase = 1000.0f;
                public const float PowerBonusRate = 0.01f;     // 1% bonus per 1000 power
                public const float MaxPowerBonus = 1.0f;       // Maximum 100% bonus from power
            }
        }

        public static class IdleCharacterStats
        {
            // Base Values
            public const float DefaultStatValue = 10f;
            public const float BaseMultiplier = 1.0f;
            public const int InitialLevel = 1;

            // Progression
            public const float LevelUpStatMultiplier = 1.1f;  // 10% increase
            public const float ExperienceCurveExponent = 1.5f;
            public const float BaseExperienceRequired = 100f;

            // Idle Gains
            public const float BaseIdleGainRate = 1.0f;
            public const float IdleGainMultiplier = 1.0f;

            // Stats
            public static class Strength
            {
                public const float BaseValue = DefaultStatValue;
                public const float GainMultiplier = 1.0f;
                public const float IdleGainRate = 1.0f;
            }

            public static class Dexterity
            {
                public const float BaseValue = DefaultStatValue;
                public const float GainMultiplier = 1.0f;
                public const float IdleGainRate = 1.0f;
            }

            public static class Intelligence
            {
                public const float BaseValue = DefaultStatValue;
                public const float GainMultiplier = 1.0f;
                public const float IdleGainRate = 1.0f;
            }

            public static class Stamina
            {
                public const float BaseValue = DefaultStatValue;
                public const float GainMultiplier = 1.0f;
                public const float IdleGainRate = 1.0f;
            }

            // Derived Stats
            public static class DerivedStats
            {
                public const float HealthPerStamina = 10f;
                public const float AttackPerStrength = 2f;
                public const float DefensePerStamina = 1f;
                public const float SpeedPerDexterity = 1.5f;
            }
        }
    }
}