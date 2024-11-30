using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Event arguments for when an ascension milestone is reached
/// </summary>
public class AscensionEventArgs : ProgressionEventArgs
{
	/// <summary>
	/// The previous ascension level
	/// </summary>
	public long PreviousAscensionLevel { get; }

	/// <summary>
	/// The new ascension level
	/// </summary>
	public long NewAscensionLevel { get; }

	/// <summary>
	/// Additional power multiplier gained from this ascension
	/// </summary>
	public float AscensionMultiplierGained { get; }

	/// <summary>
	/// Total accumulated ascension multiplier
	/// </summary>
	public float TotalAscensionMultiplier { get; }

	/// <summary>
	/// Special rewards or bonuses unlocked by this ascension
	/// </summary>
	public Dictionary<string, float> UnlockedBonuses { get; }

	public AscensionEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier,
		long previousAscensionLevel,
		long newAscensionLevel,
		float ascensionMultiplierGained,
		float totalAscensionMultiplier,
		Dictionary<string, float> unlockedBonuses)
		: base(characterId, currentLevel, currentExperience, powerMultiplier)
	{
		PreviousAscensionLevel = previousAscensionLevel;
		NewAscensionLevel = newAscensionLevel;
		AscensionMultiplierGained = ascensionMultiplierGained;
		TotalAscensionMultiplier = totalAscensionMultiplier;
		UnlockedBonuses = unlockedBonuses;
	}
}