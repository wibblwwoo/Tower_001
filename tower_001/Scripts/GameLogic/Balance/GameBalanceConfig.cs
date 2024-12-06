using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Balance
{
    /// <summary>
    /// Static configuration class containing all game balance values.
    /// This centralizes magic numbers used throughout the codebase for easier maintenance and tweaking.
    /// </summary>
    public static class GameBalanceConfig
    {
        public static class Progression
        {
            // Experience and Leveling
            public const float BaseExperienceRequired = 100f;
            public const float ExperienceScalingFactor = 1.15f;
            public const int MaxLevel = 100;
            public const float ExperienceShareRatio = 0.1f; // 10% of exp shared to other characters

            // Rank System
            public const int MaxRank = 6;  // SSS Rank
            public const int MaxStars = 5;
            public const float RankUpStatBonus = 0.2f;  // 20% stat increase per rank
            public const float StarUpStatBonus = 0.05f; // 5% stat increase per star

            // Prestige System
            public const float PrestigePointBase = 100f;
            public const float PrestigeScalingFactor = 1.5f;
            public const float PrestigeBonusBase = 0.1f; // 10% bonus per prestige level

            // Ascension System
            public const int MaxAscensionLevel = 10;
            public const float AscensionStatMultiplier = 0.25f; // 25% permanent stat bonus per ascension
            public const float AscensionCostBase = 1000f;
            public const float AscensionCostScaling = 2.0f;
        }

        public static class RoomDifficulty
        {
            public const float BossMultiplier = 3.0f;
            public const float MiniBossMultiplier = 2.0f;
            public const float CombatMultiplier = 1.0f;
            public const float EventMultiplier = 0.8f;
            public const float RestMultiplier = 0.5f;
            public const float PositionScalingBase = 1.1f;
            public const float FloorNumberScaling = 0.02f;
        }

        public static class FloorDifficulty
        {
            public const float BaseScalingFactor = 1.15f;
            public const float PositionBonus = 0.05f;
            public const float MilestoneMultiplier = 1.5f;
        }

        public static class CharacterStats
        {
            // Base stat modifiers
            public const float BaseStatMultiplier = 1.0f;
            public const float LevelStatScaling = 0.1f;     // 10% per level
            public const float RankStatScaling = 0.2f;      // 20% per rank
            public const float AscensionStatScaling = 0.25f;// 25% per ascension

            public static class Knight
            {
                // Base stats maintained and enhanced with scaling
                public const float BaseHealth = 150f;
                public const float HealthGrowth = 0.15f;
                public const float BaseAttack = 15f;
                public const float AttackGrowth = 0.08f;
                public const float BaseDefense = 12f;
                public const float DefenseGrowth = 0.12f;
                public const float BaseSpeed = 8f;
                public const float SpeedGrowth = 0.10f;

                // Class-specific bonuses
                public const float DefenseRankBonus = 0.25f;    // Extra 25% defense per rank
                public const float HealthAscensionBonus = 0.3f; // Extra 30% health per ascension
            }

            public static class Mage
            {
                // Base stats maintained and enhanced with scaling
                public const float BaseHealth = 120f;
                public const float HealthGrowth = 0.12f;
                public const float BaseMana = 150f;
                public const float ManaGrowth = 0.15f;
                public const float BaseDefense = 8f;
                public const float DefenseGrowth = 0.10f;
                public const float BaseSpeed = 10f;
                public const float SpeedGrowth = 0.11f;

                // Class-specific bonuses
                public const float ManaRankBonus = 0.25f;      // Extra 25% mana per rank
                public const float SpellAscensionBonus = 0.3f; // Extra 30% spell power per ascension
            }
        }

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

        public static class PowerCalculation
        {
            // Power level calculation weights
            public const float HealthWeight = 1.0f;
            public const float AttackWeight = 2.0f;
            public const float DefenseWeight = 1.5f;
            public const float SpeedWeight = 1.0f;
            
            // Additional power factors
            public const float RankMultiplier = 1.2f;
            public const float AscensionMultiplier = 1.5f;
            public const float LevelScaling = 0.1f;
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