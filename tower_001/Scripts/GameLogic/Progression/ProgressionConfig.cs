using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Configuration class for all progression-related settings and formulas
/// </summary>
public partial class ProgressionConfig
{
    // Level settings
    public int MaxLevel { get; set; } = 100;
    public int MinLevel { get; set; } = 1;

    // Experience settings
    public float BaseExpForLevel { get; set; } = 1000f;
    public float ExpScalingFactor { get; set; } = 1.15f;

    // Prestige settings
    public int LevelsForPrestige { get; set; } = 10000;
    public float PrestigePowerMultiplier { get; set; } = 2.0f;
    public Dictionary<int, long> PrestigeCosts { get; set; } = new();

    // Ascension settings
    public int PrestigeForAscension { get; set; } = 10000;
    public float AscensionPowerMultiplier { get; set; } = 10.0f;
    public long MaxAscensionLevel { get; set; } = 1000000000;

    // Time-based progression
    public float TimeBasedMultiplierBase { get; set; } = 0.01f;
    public float TimeBasedMultiplierCap { get; set; } = 2.0f;
    public TimeSpan TimeBasedMultiplierInterval { get; set; } = TimeSpan.FromHours(1);

    // Experience source multipliers
    public Dictionary<ExperienceSource, float> ExperienceMultipliers { get; set; } = new();

    // Achievement settings
    public float AchievementBonusMultiplier { get; set; } = 0.1f;
    public int MaxConcurrentAchievements { get; set; } = 100;

    // Milestone multipliers
    public Dictionary<int, float> LevelMilestoneMultipliers { get; set; } = new();
    public Dictionary<int, float> PrestigeMilestoneMultipliers { get; set; } = new();
    public Dictionary<long, float> AscensionMilestoneMultipliers { get; set; } = new();

    public ProgressionConfig()
    {
        InitializeDefaultValues();
    }

    private void InitializeDefaultValues()
    {
        // Initialize experience multipliers
        foreach (ExperienceSource source in Enum.GetValues(typeof(ExperienceSource)))
        {
            ExperienceMultipliers[source] = 1.0f;
        }

        // Initialize prestige costs
        for (int i = 1; i <= 10; i++)
        {
            PrestigeCosts[i] = (long)(1000000 * Math.Pow(10, i - 1));
        }

        // Initialize milestone multipliers
        for (int level = 10; level <= 100; level += 10)
        {
            LevelMilestoneMultipliers[level] = 1.0f + (level / 100.0f);
        }

        for (int prestige = 1; prestige <= 10; prestige++)
        {
            PrestigeMilestoneMultipliers[prestige] = 1.0f + (prestige * 0.1f);
        }

        for (int ascension = 1; ascension <= 5; ascension++)
        {
            AscensionMilestoneMultipliers[ascension] = 1.0f + (ascension * 0.5f);
        }
    }
}