using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Stores and manages all progression-related data for a character
/// </summary>
public partial class ProgressionData
{
	// Existing properties
	public int Level { get; set; } = 1;
	public float CurrentExp { get; set; } = 0;
	public int PrestigeLevel { get; set; } = 0;
	public long AscensionLevel { get; set; } = 0;

	// New properties for enhanced progression tracking

	// Experience tracking
	public Dictionary<ExperienceSource, float> LifetimeExperience { get; private set; } = new();
	public float HighestExperienceGain { get; private set; } = 0;

	// Achievement tracking
	public HashSet<string> CompletedAchievements { get; private set; } = new();
	public Dictionary<string, float> AchievementProgress { get; private set; } = new();

	// Milestone tracking
	public HashSet<ProgressionMilestoneType> ReachedMilestones { get; private set; } = new();
	public Dictionary<string, DateTime> MilestoneTimestamps { get; private set; } = new();

	// Multiplier tracking
	public Dictionary<string, float> ActiveMultipliers { get; private set; } = new();
	public Dictionary<string, DateTime> MultiplierExpirations { get; private set; } = new();

	// Time tracking
	public TimeSpan TotalPlayTime { get; private set; } = TimeSpan.Zero;
	public DateTime LastLoginTime { get; private set; }
	public Dictionary<string, TimeSpan> ActivityPlayTimes { get; private set; } = new();

	

	private readonly ProgressionConfig _config;

	// Constructor
	public ProgressionData(ProgressionConfig config)
	{
		_config = config;
		LastLoginTime = DateTime.UtcNow;
		InitializeCollections();
	}

	private void InitializeCollections()
	{
		foreach (ExperienceSource source in Enum.GetValues(typeof(ExperienceSource)))
		{
			LifetimeExperience[source] = 0f;
		}

		// Initialize activity play times
		ActivityPlayTimes = new Dictionary<string, TimeSpan>
		{
			{ "Combat", TimeSpan.Zero },
			{ "Exploration", TimeSpan.Zero },
			{ "Crafting", TimeSpan.Zero },
			{ "Questing", TimeSpan.Zero }
		};
	}

	// Enhanced calculation methods
	public float GetEffectiveMultiplier(ExperienceSource source)
	{
		float baseMultiplier = _config.ExperienceMultipliers.GetValueOrDefault(source, 1.0f);
		float achievementBonus = CalculateAchievementBonus();
		float timeBonus = CalculateTimeBonus();
		float milestoneBonus = CalculateMilestoneBonus();

		return baseMultiplier * (1 + achievementBonus + timeBonus + milestoneBonus);
	}

	private float CalculateAchievementBonus()
	{
		return CompletedAchievements.Count * _config.AchievementBonusMultiplier;
	}

	private float CalculateTimeBonus()
	{
		float hoursPlayed = (float)TotalPlayTime.TotalHours;
		return Math.Min(_config.TimeBasedMultiplierCap,
			hoursPlayed * _config.TimeBasedMultiplierBase);
	}

	private float CalculateMilestoneBonus()
	{
		float bonus = 0f;

		// Add level milestone bonuses
		if (_config.LevelMilestoneMultipliers.TryGetValue(Level, out float levelBonus))
			bonus += levelBonus;

		// Add prestige milestone bonuses
		if (_config.PrestigeMilestoneMultipliers.TryGetValue(PrestigeLevel, out float prestigeBonus))
			bonus += prestigeBonus;

		// Add ascension milestone bonuses
		if (_config.AscensionMilestoneMultipliers.TryGetValue(AscensionLevel, out float ascensionBonus))
			bonus += ascensionBonus;

		return bonus;
	}

	// Existing methods with enhancements
	public float TotalPowerMultiplier => CalculateTotalMultiplier();

	public float PrestigeMultiplierGained => CalculatePrestigeMultiplier();

	private float CalculatePrestigeMultiplier()
	{
		return (float)Math.Pow(_config.PrestigePowerMultiplier,PrestigeLevel);
	}

	private float CalculateTotalMultiplier()
	{
		float prestigeMultiplier = (float)Math.Pow(_config.PrestigePowerMultiplier, PrestigeLevel);
		float ascensionMultiplier = (float)Math.Pow(_config.AscensionPowerMultiplier, AscensionLevel);
		float bonusMultiplier = ActiveMultipliers.Values.Aggregate(1.0f, (current, next) => current * next);

		return prestigeMultiplier * ascensionMultiplier * bonusMultiplier;
	}

	public float GetExpForNextLevel()
	{
		return _config.BaseExpForLevel * (float)Math.Pow(_config.ExpScalingFactor, Level - 1);
	}

	// Requirement checks
	public bool CanPrestige => Level >= _config.LevelsForPrestige && CheckPrestigeCost(PrestigeLevel+1);

	public bool CanAscend => PrestigeLevel >= _config.PrestigeForAscension &&
							CheckAscensionCost(AscensionLevel+1);

	private bool CheckPrestigeCost(int targetLevel)
	{
		return _config.PrestigeCosts.TryGetValue(targetLevel, out long cost);
	}

	private bool CheckAscensionCost(long targetLevel)
	{
		return _config.AscensionCosts.TryGetValue(targetLevel, out long cost);
	}
}