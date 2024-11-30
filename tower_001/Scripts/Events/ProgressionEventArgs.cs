using Godot;
using System;

/// <summary>
/// Base class for progression-related events.
/// Contains core information relevant to all progression events.
/// </summary>
public class ProgressionEventArgs : GameEventArgs
{
	/// <summary>
	/// The ID of the character whose progression changed
	/// </summary>
	public string CharacterId { get; }

	/// <summary>
	/// The character's current level
	/// </summary>
	public int CurrentLevel { get; }

	/// <summary>
	/// The character's current experience
	/// </summary>
	public float CurrentExperience { get; }

	/// <summary>
	/// The total accumulated power multiplier from all sources
	/// </summary>
	public float PowerMultiplier { get; }

	public ProgressionEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier)
	{
		CharacterId = characterId;
		CurrentLevel = currentLevel;
		CurrentExperience = currentExperience;
		PowerMultiplier = powerMultiplier;
	}
}