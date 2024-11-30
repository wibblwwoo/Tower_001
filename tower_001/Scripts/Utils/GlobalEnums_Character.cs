using Godot;
using System;

public static partial class GlobalEnums
{
	
	/// <summary>
	/// Categories of progression achievements
	/// </summary>
	public enum ProgressionAchievementType
	{
		LevelReached,
		ExperienceGained,
		PrestigeAttained,
		AscensionAttained,
		PowerMultiplier,
		TimeInGame,
		CombatVictories,
		RoomsCleared,
		FloorsCompleted,
		TowersConquered
	}

	/// <summary>
	/// Types of progression bonuses that can be unlocked
	/// </summary>
	public enum ProgressionBonusType
	{
		ExperienceRate,
		PowerMultiplier,
		StatBoost,
		ResourceGain,
		UnlockFeature,
		SkillEnhancement,
		ItemQuality,
		RewardQuantity
	}

	/// <summary>
	/// Milestones in progression that can trigger events
	/// </summary>
	public enum ProgressionMilestoneType
	{
		Level,
		Prestige,
		Ascension,
		Achievement,
		PowerLevel,
		TimeInGame,
		CombatRating,
		ResourceAccumulation
	}

	/// <summary>
	/// Enum defining possible sources of experience gain
	/// </summary>
	public enum ExperienceSource
	{
		Combat,
		RoomCompletion,
		FloorCompletion,
		TowerCompletion,
		Achievement,
		Quest,
		Bonus,
		Event
	}
}
