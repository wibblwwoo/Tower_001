using System;
using System.Collections.Generic;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

/// <summary>
/// Configuration class for all progression-related settings and formulas.
/// This class manages the runtime configuration of the progression system,
/// using constants defined in GameBalanceConfig.Progression.
/// </summary>
/// <remarks>
/// The progression system includes:
/// - Experience and leveling mechanics
/// - Prestige system for meta-progression
/// - Ascension system for long-term progression
/// - Achievement and milestone tracking
/// - Time-based progression bonuses
/// </remarks>
public partial class ProgressionConfig
{
	#region Core Progression Properties
	/// <summary>
	/// Number of levels required to unlock prestige ability
	/// </summary>
	public int LevelsForPrestige { get; set; } = GameBalanceConfig.Progression.LevelsRequiredForPrestige;

	/// <summary>
	/// Base experience points required for the first level
	/// </summary>
	public float BaseExpForLevel { get; set; } = GameBalanceConfig.Progression.BaseExperienceRequired;

	/// <summary>
	/// Factor by which experience requirements scale with each level
	/// </summary>
	public float ExpScalingFactor { get; set; } = GameBalanceConfig.Progression.ExperienceScalingFactor;

	/// <summary>
	/// Number of prestige levels required to unlock ascension ability
	/// </summary>
	public int PrestigeForAscension { get; set; } = GameBalanceConfig.Progression.PrestigeRequiredForAscension;

	/// <summary>
	/// Base multiplier for power gains from prestige levels
	/// </summary>
	public float PrestigePowerMultiplier { get; set; } = GameBalanceConfig.Progression.PrestigePowerBase;

	/// <summary>
	/// Base multiplier for power gains from ascension levels
	/// </summary>
	public float AscensionPowerMultiplier { get; set; } = GameBalanceConfig.Progression.AscensionPowerBase;

	/// <summary>
	/// Maximum achievable ascension level
	/// </summary>
	public long MaxAscensionLevel { get; set; } = GameBalanceConfig.Progression.MaxAscensionLevel;
	#endregion

	#region Experience Settings
	/// <summary>
	/// Minimum experience points gained from any action
	/// </summary>
	public float MinExperienceGain { get; set; } = GameBalanceConfig.Progression.MinExperienceGain;

	/// <summary>
	/// Maximum multiplier that can be applied to experience gains
	/// </summary>
	public float MaxExperienceMultiplier { get; set; } = GameBalanceConfig.Progression.MaxExperienceMultiplier;

	/// <summary>
	/// Multipliers for different sources of experience gain
	/// </summary>
	public Dictionary<ExperienceSource, float> ExperienceMultipliers { get; set; } = new();
	#endregion

	#region Achievement Settings
	/// <summary>
	/// Multiplier applied to gains when completing achievements
	/// </summary>
	public float AchievementBonusMultiplier { get; set; } = GameBalanceConfig.Progression.AchievementBonus;

	/// <summary>
	/// Maximum number of achievements that can be active simultaneously
	/// </summary>
	public int MaxConcurrentAchievements { get; set; } = GameBalanceConfig.Progression.MaxConcurrentAchievements;
	#endregion

	#region Milestone Settings
	/// <summary>
	/// Multipliers applied at specific level milestones
	/// </summary>
	public Dictionary<int, float> LevelMilestoneMultipliers { get; set; } = new();

	/// <summary>
	/// Multipliers applied at specific prestige milestones
	/// </summary>
	public Dictionary<int, float> PrestigeMilestoneMultipliers { get; set; } = new();

	/// <summary>
	/// Multipliers applied at specific ascension milestones
	/// </summary>
	public Dictionary<long, float> AscensionMilestoneMultipliers { get; set; } = new();
	#endregion

	#region Time-based Settings
	/// <summary>
	/// Base multiplier for time-based progression bonuses
	/// </summary>
	public float TimeBasedMultiplierBase { get; set; } = GameBalanceConfig.Progression.TimeMultiplierBase;

	/// <summary>
	/// Maximum multiplier that can be achieved through time-based bonuses
	/// </summary>
	public float TimeBasedMultiplierCap { get; set; } = GameBalanceConfig.Progression.TimeMultiplierCap;

	/// <summary>
	/// Time interval between updates to time-based multipliers
	/// </summary>
	public TimeSpan TimeBasedMultiplierInterval { get; set; } = TimeSpan.FromHours(GameBalanceConfig.Progression.TimeMultiplierHours);
	#endregion

	#region Resource Costs
	/// <summary>
	/// Resource costs required for each prestige level
	/// </summary>
	public Dictionary<int, long> PrestigeCosts { get; set; } = new();

	/// <summary>
	/// Resource costs required for each ascension level
	/// </summary>
	public Dictionary<long, long> AscensionCosts { get; set; } = new();
	#endregion

	/// <summary>
	/// Initializes a new instance of the ProgressionConfig class with default values
	/// from GameBalanceConfig.Progression
	/// </summary>
	public ProgressionConfig()
	{
		InitializeDefaultMultipliers();
		InitializeDefaultMilestones();
		InitializeDefaultCosts();
	}

	/// <summary>
	/// Initializes the default experience multipliers for different sources
	/// using values from GameBalanceConfig.Progression
	/// </summary>
	private void InitializeDefaultMultipliers()
	{
		ExperienceMultipliers = new Dictionary<ExperienceSource, float>
		{
			{ ExperienceSource.Combat, GameBalanceConfig.Progression.CombatExpMultiplier },
			{ ExperienceSource.RoomCompletion, GameBalanceConfig.Progression.RoomCompletionExpMultiplier },
			{ ExperienceSource.FloorCompletion, GameBalanceConfig.Progression.FloorCompletionExpMultiplier },
			{ ExperienceSource.TowerCompletion, GameBalanceConfig.Progression.TowerCompletionExpMultiplier },
			{ ExperienceSource.Achievement, GameBalanceConfig.Progression.AchievementExpMultiplier },
			{ ExperienceSource.Quest, GameBalanceConfig.Progression.QuestExpMultiplier },
			{ ExperienceSource.Bonus, GameBalanceConfig.Progression.BonusExpMultiplier },
			{ ExperienceSource.Event, GameBalanceConfig.Progression.EventExpMultiplier }
		};
	}

	/// <summary>
	/// Initializes the default milestone multipliers for levels, prestige, and ascension
	/// using values from GameBalanceConfig.Progression
	/// </summary>
	private void InitializeDefaultMilestones()
	{
		// Initialize level milestones with scaling bonuses
		for (int level = GameBalanceConfig.Progression.LevelMilestoneInterval; 
			 level <= GameBalanceConfig.Progression.MaxLevelMilestone; 
			 level += GameBalanceConfig.Progression.LevelMilestoneInterval)
		{
			LevelMilestoneMultipliers[level] = GameBalanceConfig.Progression.LevelMilestoneBaseBonus + (level / 1000.0f);
		}

		// Initialize prestige milestones with compounding bonuses
		for (int prestige = 1; prestige <= GameBalanceConfig.Progression.MaxPrestigeMilestone; prestige++)
		{
			PrestigeMilestoneMultipliers[prestige] = GameBalanceConfig.Progression.LevelMilestoneBaseBonus + 
												  (prestige * GameBalanceConfig.Progression.PrestigeMilestoneBaseBonus);
		}

		// Initialize ascension milestones with multiplicative bonuses
		for (int ascension = 1; ascension <= GameBalanceConfig.Progression.MaxAscensionMilestone; ascension++)
		{
			AscensionMilestoneMultipliers[ascension] = GameBalanceConfig.Progression.AscensionMilestoneBaseBonus * ascension;
		}
	}

	/// <summary>
	/// Initializes the default resource costs for prestige and ascension levels
	/// using values from GameBalanceConfig.Progression
	/// </summary>
	private void InitializeDefaultCosts()
	{
		// Initialize ascension costs with exponential scaling
		for (int level = 1; level <= GameBalanceConfig.Progression.MaxAscensionMilestone; level++)
		{
			AscensionCosts[level] = (long)(GameBalanceConfig.Progression.BaseAscensionCost * 
										Math.Pow(GameBalanceConfig.Progression.AscensionCostScalingFactor, level - 1));
		}
	}
}
