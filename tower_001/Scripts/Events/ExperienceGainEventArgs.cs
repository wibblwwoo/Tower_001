using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for when experience is gained from any source
/// </summary>
public class ExperienceGainEventArgs : ProgressionEventArgs
{
	/// <summary>
	/// The amount of experience gained
	/// </summary>
	public float ExperienceGained { get; }

	/// <summary>
	/// The source of the experience gain
	/// </summary>
	public ExperienceSource Source { get; }

	/// <summary>
	/// Any multipliers applied to the experience gain
	/// </summary>
	public float ExperienceMultiplier { get; }

	/// <summary>
	/// The raw experience amount before multipliers
	/// </summary>
	public float BaseExperience { get; }

	public ExperienceGainEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier,
		float experienceGained,
		ExperienceSource source,
		float experienceMultiplier,
		float baseExperience)
		: base(characterId, currentLevel, currentExperience, powerMultiplier)
	{
		ExperienceGained = experienceGained;
		Source = source;
		ExperienceMultiplier = experienceMultiplier;
		BaseExperience = baseExperience;
	}
}