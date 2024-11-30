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
    }
}
