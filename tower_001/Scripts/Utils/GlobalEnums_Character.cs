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

	public enum StatType
    {
        // Primary Stats
        Strength,       // Base physical power
        Dexterity,     // Agility and precision
        Intelligence,  // Mental and magical power
        Stamina,      // Physical endurance
        
        // Derived Combat Stats
        Attack,        // Offensive power
        Defense,       // Damage reduction
        Health,        // Total hit points
        Speed,         // Action speed and evasion
        
        // Critical Stats
        CriticalChance,    // Chance to land critical hits
        CriticalDamage,    // Bonus damage on critical hits
        
        // Resource Stats
        Mana,             // Magic resource pool
        ManaRegen,        // Mana regeneration rate
        Energy,           // Physical resource pool
        Shield,           // Damage absorption
		None
	}
}
