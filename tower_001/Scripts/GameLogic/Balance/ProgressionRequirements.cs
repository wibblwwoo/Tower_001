using System.Collections.Generic;

namespace Tower_001.Scripts.GameLogic.Balance
{
    /// <summary>
    /// Defines all progression requirements and thresholds for character advancement
    /// </summary>
    public static class ProgressionRequirements
    {
        public static class Rank
        {
            // Basic rank requirements
            public static readonly Dictionary<int, RankRequirement> Requirements = new()
            {
                {
                    1, new RankRequirement {
                        Level = 1,
                        Power = 0,
                        PrestigeLevel = 0
                    }
                },
                {
                    2, new RankRequirement {
                        Level = 20,
                        Power = 1000,
                        PrestigeLevel = 0
                    }
                },
                {
                    3, new RankRequirement {
                        Level = 40,
                        Power = 5000,
                        PrestigeLevel = 1
                    }
                },
                {
                    4, new RankRequirement {
                        Level = 60,
                        Power = 20000,
                        PrestigeLevel = 2
                    }
                },
                {
                    5, new RankRequirement {
                        Level = 80,
                        Power = 100000,
                        PrestigeLevel = 3
                    }
                }
            };
        }

        public static class Prestige
        {
            // Prestige level requirements
            public static readonly Dictionary<int, PrestigeRequirement> Requirements = new()
            {
                {
                    1, new PrestigeRequirement {
                        MinimumLevel = 100,
                        MinimumPower = 10000,
                        MinimumRank = 2
                    }
                },
                {
                    2, new PrestigeRequirement {
                        MinimumLevel = 150,
                        MinimumPower = 50000,
                        MinimumRank = 2
                    }
                },
                {
                    3, new PrestigeRequirement {
                        MinimumLevel = 200,
                        MinimumPower = 200000,
                        MinimumRank = 3
                    }
                },
                {
                    4, new PrestigeRequirement {
                        MinimumLevel = 250,
                        MinimumPower = 1000000,
                        MinimumRank = 3
                    }
                },
                {
                    5, new PrestigeRequirement {
                        MinimumLevel = 300,
                        MinimumPower = 5000000,
                        MinimumRank = 4
                    }
                }
            };
        }

        public static class Ascension
        {
            // Ascension level requirements
            public static readonly Dictionary<int, AscensionRequirement> Requirements = new()
            {
                {
                    1, new AscensionRequirement {
                        MinimumPrestigeLevel = 5,
                        MinimumPower = 10000000,
                        MinimumRank = 4,
                        RequiredAchievements = new[] { "FirstPrestige", "PowerMilestone1" }
                    }
                },
                {
                    2, new AscensionRequirement {
                        MinimumPrestigeLevel = 10,
                        MinimumPower = 50000000,
                        MinimumRank = 5,
                        RequiredAchievements = new[] { "PowerMilestone2", "PrestigeMaster" }
                    }
                }
            };
        }

        // Experience multipliers based on rank
        public static readonly Dictionary<int, float> RankExpMultipliers = new()
        {
            { 1, 1.0f },    // Base experience rate
            { 2, 1.2f },    // 20% bonus
            { 3, 1.5f },    // 50% bonus
            { 4, 2.0f },    // 100% bonus
            { 5, 3.0f }     // 200% bonus
        };

        // Experience sharing based on rank difference
        public static readonly Dictionary<int, float> RankShareMultipliers = new()
        {
            { 0, 1.0f },    // Same rank
            { 1, 0.8f },    // 1 rank difference
            { 2, 0.6f },    // 2 ranks difference
            { 3, 0.4f },    // 3 ranks difference
            { 4, 0.2f }     // 4 ranks difference
        };

        // Active vs Passive experience rates based on rank
        public static readonly Dictionary<int, float> PassiveExpRates = new()
        {
            { 1, 0.1f },    // 10% of active rate
            { 2, 0.15f },   // 15% of active rate
            { 3, 0.2f },    // 20% of active rate
            { 4, 0.25f },   // 25% of active rate
            { 5, 0.3f }     // 30% of active rate
        };
    }

    public class RankRequirement
    {
        public int Level { get; set; }
        public float Power { get; set; }
        public int PrestigeLevel { get; set; }
    }

    public class PrestigeRequirement
    {
        public int MinimumLevel { get; set; }
        public float MinimumPower { get; set; }
        public int MinimumRank { get; set; }
    }

    public class AscensionRequirement
    {
        public int MinimumPrestigeLevel { get; set; }
        public float MinimumPower { get; set; }
        public int MinimumRank { get; set; }
        public string[] RequiredAchievements { get; set; }
    }
}