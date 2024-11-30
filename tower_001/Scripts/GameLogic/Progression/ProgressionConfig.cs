using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Configuration class for all progression-related settings and formulas
/// </summary>
public partial class ProgressionConfig
{
	// Existing settings
	public int LevelsForPrestige { get; set; } = 10000;
	public float BaseExpForLevel { get; set; } = 1000f;
	public float ExpScalingFactor { get; set; } = 1.15f;
	public int PrestigeForAscension { get; set; } = 10000;
	public float PrestigePowerMultiplier { get; set; } = 2.0f;
	public float AscensionPowerMultiplier { get; set; } = 10.0f;
	public long MaxAscensionLevel { get; set; } = 1000000000;

	// New configuration settings
	// Experience gain settings
	public float MinExperienceGain { get; set; } = 1.0f;
	public float MaxExperienceMultiplier { get; set; } = 1000.0f;
	public Dictionary<ExperienceSource, float> ExperienceMultipliers { get; set; } = new();

	// Achievement settings
	public float AchievementBonusMultiplier { get; set; } = 0.1f;
	public int MaxConcurrentAchievements { get; set; } = 100;

	// Milestone settings
	public Dictionary<int, float> LevelMilestoneMultipliers { get; set; } = new();
	public Dictionary<int, float> PrestigeMilestoneMultipliers { get; set; } = new();
	public Dictionary<long, float> AscensionMilestoneMultipliers { get; set; } = new();

	// Time-based progression
	public float TimeBasedMultiplierBase { get; set; } = 0.01f;
	public float TimeBasedMultiplierCap { get; set; } = 2.0f;
	public TimeSpan TimeBasedMultiplierInterval { get; set; } = TimeSpan.FromHours(1);

	// Resource costs
	public Dictionary<int, long> PrestigeCosts { get; set; } = new Dictionary<int, long>
	{
		{ 1, 1000000 },
		{ 2, 10000000 },
		{ 3, 100000000 },
		{ 4, 1000000000 },
		{ 5, 10000000000 },
		{ 6, 100000000000 },
		{ 7, 1000000000000 },
		{ 8, 10000000000000 },
		{ 9, 100000000000000 },
		{ 10, 1000000000000000 }
	};
	public Dictionary<long, long> AscensionCosts { get; set; } = new();



	// Constructor with default initialization
	public ProgressionConfig()
	{
		InitializeDefaultMultipliers();
		InitializeDefaultMilestones();
		InitializeDefaultCosts();
	}

	private void InitializeDefaultMultipliers()
	{
		// Set default experience multipliers for different sources
		ExperienceMultipliers = new Dictionary<ExperienceSource, float>
		{
			{ ExperienceSource.Combat, 1.0f },
			{ ExperienceSource.RoomCompletion, 1.2f },
			{ ExperienceSource.FloorCompletion, 1.5f },
			{ ExperienceSource.TowerCompletion, 2.0f },
			{ ExperienceSource.Achievement, 1.3f },
			{ ExperienceSource.Quest, 1.4f },
			{ ExperienceSource.Bonus, 1.1f },
			{ ExperienceSource.Event, 1.25f }
		};
	}

	private void InitializeDefaultMilestones()
	{
		// Initialize level milestones
		for (int level = 100; level <= 1000; level += 100)
		{
			LevelMilestoneMultipliers[level] = 1.0f + (level / 1000.0f);
		}

		// Initialize prestige milestones
		for (int prestige = 1; prestige <= 10; prestige++)
		{
			PrestigeMilestoneMultipliers[prestige] = 1.0f + (prestige * 0.5f);
		}

		// Initialize ascension milestones
		for (int ascension = 1; ascension <= 5; ascension++)
		{
			AscensionMilestoneMultipliers[ascension] = 2.0f * ascension;
		}
	}

	private void InitializeDefaultCosts()
	{
		// Initialize prestige costs
		//for (int level = 1; level <= 10; level++)
		//{
		//	PrestigeCosts[level] = (long)(1000000 * Math.Pow(10, level - 1));
		//}


		// Initialize ascension costs
		for (int level = 1; level <= 5; level++)
		{
			AscensionCosts[level] = (long)(1000000000 * Math.Pow(100, level - 1));
		}
	}
}
