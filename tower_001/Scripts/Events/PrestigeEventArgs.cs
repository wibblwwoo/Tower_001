using Godot;
using System;

/// <summary>
/// Event arguments for when a prestige milestone is reached
/// </summary>
public class PrestigeEventArgs : ProgressionEventArgs
{
	/// <summary>
	/// The previous prestige level
	/// </summary>
	public int PreviousPrestigeLevel { get; }

	/// <summary>
	/// The new prestige level
	/// </summary>
	public int NewPrestigeLevel { get; }

	/// <summary>
	/// Additional power multiplier gained from this prestige
	/// </summary>
	public float PrestigeMultiplierGained { get; }

	/// <summary>
	/// Total accumulated prestige multiplier
	/// </summary>
	public float TotalPrestigeMultiplier { get; }

	public PrestigeEventArgs(
		string characterId,
		int currentLevel,
		float currentExperience,
		float powerMultiplier,
		int previousPrestigeLevel,
		int newPrestigeLevel,
		float prestigeMultiplierGained,
		float totalPrestigeMultiplier)
		: base(characterId, currentLevel, currentExperience, powerMultiplier)
	{
		PreviousPrestigeLevel = previousPrestigeLevel;
		NewPrestigeLevel = newPrestigeLevel;
		PrestigeMultiplierGained = prestigeMultiplierGained;
		TotalPrestigeMultiplier = totalPrestigeMultiplier;
	}
}