using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
/// <summary>
/// Represents the configuration for generating floors in a tower.
/// Includes parameters such as base difficulty, floor type, scaling factors, and milestones.
/// </summary>
public class FloorGenerationConfig
{
	// Basic Configuration
	// <summary>
	/// The base difficulty of the floors being generated.
	/// This serves as the starting point for difficulty scaling.
	/// </summary>
	public float BaseDifficulty { get; set; }
	/// <summary>
	/// The scaling factor applied to difficulty as floors progress.
	/// Higher values increase the difficulty curve more steeply.
	/// </summary>
	public float DifficultyScalingFactor { get; set; } = 0.1f;
	public FloorType DefaultFloorType { get; set; } = FloorType.None;

	// Advanced Configuration
	public FloorType[] FloorTypePattern { get; set; }
	/// <summary>
	/// Indicates whether milestone floors will have difficulty spikes.
	/// Milestones are typically boss or special floors.
	/// </summary>
	public bool MilestoneDifficultySpikes { get; set; } = true;
	/// <summary>
	/// The multiplier applied to difficulty for milestone floors if spikes are enabled.
	/// </summary>
	public float MilestoneDifficultyMultiplier { get; set; } = 1.5f;

	public Dictionary<ElementType, float> ElementalModifiers { get; set; } = new();

	public FloorGenerationConfig(float baseDifficulty, FloorType defaultType)
	{
		BaseDifficulty = baseDifficulty;
		DefaultFloorType = defaultType;
	}

	public FloorGenerationConfig()
	{
		// Default constructor
	}
}