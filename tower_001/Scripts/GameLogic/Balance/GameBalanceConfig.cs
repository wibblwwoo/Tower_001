using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Balance
{
    /// <summary>
    /// Static configuration class containing all game balance values.
    /// This centralizes magic numbers used throughout the codebase for easier maintenance and tweaking.
    /// </summary>
    public static class GameBalanceConfig
    {
        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: CharacterManager, PlayerManager
        /// - Related Systems: Character progression, Ranking system, Prestige system
        /// </summary>
        public static class Progression
        {
           // Experience and Leveling
            public const float BaseExperienceRequired = 100f;        // Starting XP needed for first level
            public const float ExperienceScalingFactor = 1.15f;     // 15% increase in XP per level
            public const int MaxLevel = 100;                        // Maximum achievable level
            public const float ExperienceShareRatio = 0.1f;         // 10% of XP shared to other characters

            // Rank System
            public const int MaxRank = 6;                          // SSS Rank maximum
            public const int MaxStars = 5;                         // Maximum stars per rank
            public const float RankUpStatBonus = 0.2f;             // 20% stat increase per rank
            public const float StarUpStatBonus = 0.05f;            // 5% stat increase per star


            // Prestige System
            public const float PrestigePointBase = 100f;           // Base points needed for first prestige
            public const float PrestigeScalingFactor = 1.5f;       // 50% increase in points needed per prestige
            public const float PrestigeBonusBase = 0.1f;           // 10% bonus per prestige level

            // Ascension System
                public const int MaxAscensionLevel = 10;               // Maximum ascension level
                public const float AscensionStatMultiplier = 0.25f;    // 25% permanent stat bonus per ascension
                public const float AscensionCostBase = 1000f;          // Base cost for first ascension
                public const float AscensionCostScaling = 2.0f;        // Cost doubles per ascension level
        }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: RoomManager, FloorManager
        /// - Related Systems: Combat system, Room generation
        /// - Config Files: RoomConfiguration
        /// </summary>
        public static class RoomDifficulty
        {
            public const float BossMultiplier = 3.0f;              // Boss rooms are 3x harder
            public const float MiniBossMultiplier = 2.0f;          // Mini-boss rooms are 2x harder
            public const float CombatMultiplier = 1.0f;            // Standard difficulty for combat rooms
            public const float EventMultiplier = 0.8f;             // Event rooms are 20% easier
            public const float RestMultiplier = 0.5f;              // Rest rooms are 50% easier
            public const float PositionScalingBase = 1.1f;         // 10% increase per room position
            public const float FloorNumberScaling = 0.02f;         // 2% increase per floor number
            public const float BaseRoomDifficultyIncrement = 0.1f; // 10% increase per room
            public const float BossRoomDifficultyMultiplier = 2.0f;// Boss rooms are 2x harder
            public const float MiniBossRoomDifficultyMultiplier = 1.5f; // Mini-boss rooms are 50% harder
            public const float MaxFloorDifficulty = 10.0f;         // Maximum difficulty cap
       }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: FloorManager, TowerManager
        /// - Related Systems: Floor generation, Tower progression
        /// - Config Files: FloorConfiguration
        /// </summary>
        public static class FloorDifficulty
        {
            public const float BaseScalingFactor = 1.15f;          // 15% increase per floor
            public const float PositionBonus = 0.05f;              // 5% bonus per position
            public const float MilestoneMultiplier = 1.5f;         // 50% bonus at milestone floors
        }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: CharacterManager, CombatManager
        /// - Related Systems: Character creation, Combat calculations
        /// - Config Files: CharacterConfiguration
        /// </summary>
        public static class CharacterStats
        {
            // Base stat modifiers
            public const float BaseStatMultiplier = 1.0f;          // Base multiplier for all stats
            public const float LevelStatScaling = 0.1f;            // 10% increase per level
            public const float RankStatScaling = 0.2f;             // 20% increase per rank
            public const float AscensionStatScaling = 0.25f;       // 25% increase per ascension

        public static class Knight
        {
            public const float BaseHealth = 150f;              // Starting health for Knights
            public const float HealthGrowth = 0.15f;           // 15% health increase per level
            public const float BaseAttack = 15f;               // Starting attack for Knights
            public const float AttackGrowth = 0.08f;           // 8% attack increase per level
            public const float BaseDefense = 12f;              // Starting defense for Knights
            public const float DefenseGrowth = 0.12f;          // 12% defense increase per level
            public const float BaseSpeed = 8f;                 // Starting speed for Knights
            public const float SpeedGrowth = 0.10f;            // 10% speed increase per level
            public const float DefenseRankBonus = 0.25f;       // 25% extra defense per rank
            public const float HealthAscensionBonus = 0.3f;    // 30% extra health per ascension
        }

        public static class Mage
        {
            public const float BaseHealth = 120f;              // Starting health for Mages
            public const float HealthGrowth = 0.12f;           // 12% health increase per level
            public const float BaseMana = 150f;                // Starting mana for Mages
            public const float ManaGrowth = 0.15f;             // 15% mana increase per level
            public const float BaseDefense = 8f;               // Starting defense for Mages
            public const float DefenseGrowth = 0.10f;          // 10% defense increase per level
            public const float BaseSpeed = 10f;                // Starting speed for Mages
            public const float SpeedGrowth = 0.11f;            // 11% speed increase per level
            public const float ManaRankBonus = 0.25f;          // 25% extra mana per rank
            public const float SpellAscensionBonus = 0.3f;     // 30% extra spell power per ascension
        }
    }

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
        /// - Used by: RoomManager, RoomConfiguration
        /// - Related Systems: Room generation, Floor layout
        /// - Config Files: RoomConfiguration
        /// </summary>
        public static class RoomGeneration
        {
            public const int MinRoomsPerFloor = 20;               // Minimum rooms required per floor
            public const int MaxRoomsPerFloor = 100;              // Maximum rooms allowed per floor
            public const int BossFloorInterval = 100;             // Boss floor appears every 100 floors
        }

        /// <summary>
        /// Dependencies and Usage:
        /// - Used by: RoomManager, RoomConfiguration
        /// - Related Systems: Room type selection, Floor layout
        /// - Config Files: RoomConfiguration
        /// </summary>
        public static class RoomDistribution
        {
            public const float CombatRoomPercentage = 70f;        // 70% chance for combat rooms
            public const float RestRoomPercentage = 16f;          // 16% chance for rest rooms
            public const float EventRoomPercentage = 4f;          // 4% chance for event rooms
            public const float MiniBossRoomPercentage = 3f;       // 3% chance for mini-boss rooms
            public const float EncounterRoomPercentage = 3f;      // 3% chance for encounter rooms
            public const float BossRoomPercentage = 2f;           // 2% chance for boss rooms
            public const float RewardRoomPercentage = 1f;         // 1% chance for reward rooms
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
        /// - Used by: IdleCharacterManager
        /// - Related Systems: Idle character progression
        /// </summary>
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
            public const float BaseIdleGainRate = 1.0f;
            public const float IdleGainMultiplier = 1.0f;

            // Individual Stats
            public static class Strength
            {
                public const float BaseValue = DefaultStatValue;
                public const float IdleGainRate = 1.0f;
            }

            public static class Dexterity
            {
                public const float BaseValue = DefaultStatValue;
                public const float IdleGainRate = 1.0f;
            }

            public static class Intelligence
            {
                public const float BaseValue = DefaultStatValue;
                public const float IdleGainRate = 1.0f;
            }

            public static class Stamina
            {
                public const float BaseValue = DefaultStatValue;
                public const float IdleGainRate = 1.0f;
            }

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