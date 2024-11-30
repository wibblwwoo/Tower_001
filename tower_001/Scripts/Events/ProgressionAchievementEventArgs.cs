using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Event arguments for progression milestone achievements
/// </summary>
public class ProgressionAchievementEventArgs : ProgressionEventArgs
{
	/// <summary>
	/// The unique identifier of the achievement
	/// </summary>
	public string AchievementId { get; }

	/// <summary>
	/// The type/category of the achievement
	/// </summary>
	public string AchievementType { get; }

	/// <summary>
	/// The numeric progress value associated with the achievement
	/// </summary>
	public float ProgressValue { get; }

	/// <summary>
	/// The target value needed to complete the achievement
	/// </summary>
	public float TargetValue { get; }

	/// <summary>
	/// Whether this event represents achievement completion
	/// </summary>
	public bool IsCompleted { get; }

	/// <summary>
	/// Any rewards granted by this achievement
	/// </summary>
	public Dictionary<string, float> Rewards { get; }

	public ProgressionAchievementEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier,
		string achievementId,
		string achievementType,
		float progressValue,
		float targetValue,
		bool isCompleted,
		Dictionary<string, float> rewards)
		: base(characterId, currentLevel, currentExperience, powerMultiplier)
	{
		AchievementId = achievementId;
		AchievementType = achievementType;
		ProgressValue = progressValue;
		TargetValue = targetValue;
		IsCompleted = isCompleted;
		Rewards = rewards;
	}
}