using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Event arguments for when progression multipliers change
/// </summary>
public class ProgressionMultiplierEventArgs : ProgressionEventArgs
{
	/// <summary>
	/// Previous total multiplier value
	/// </summary>
	public float PreviousMultiplier { get; }

	/// <summary>
	/// All active multiplier sources and their values
	/// </summary>
	public Dictionary<string, float> ActiveMultipliers { get; }

	/// <summary>
	/// Duration remaining for temporary multipliers
	/// </summary>
	public Dictionary<string, TimeSpan> MultiplierDurations { get; }

	/// <summary>
	/// Source of the multiplier change
	/// </summary>
	public string ChangeSource { get; }

	public ProgressionMultiplierEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier,
		float previousMultiplier,
		Dictionary<string, float> activeMultipliers,
		Dictionary<string, TimeSpan> multiplierDurations,
		string changeSource)
		: base(characterId, currentLevel, currentExperience, powerMultiplier)
	{
		PreviousMultiplier = previousMultiplier;
		ActiveMultipliers = activeMultipliers;
		MultiplierDurations = multiplierDurations;
		ChangeSource = changeSource;
	}
}