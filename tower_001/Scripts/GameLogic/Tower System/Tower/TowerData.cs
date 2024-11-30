using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
/// <summary>
/// Represents a tower, including its floors, difficulty scaling, and overall state.
/// Tracks progress, requirements, and historical attempts for the tower.
/// </summary>
public partial class TowerData
{
	// Basic Properties
	/// <summary>
	/// Unique identifier for the tower.
	/// Used to reference the tower in the system.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// The name of the tower.
	/// Used for display and identification purposes.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// A description of the tower.
	/// Provides additional information or lore about the tower.
	/// </summary>
	public string Description { get; set; }

	// Requirements and Difficulty
	/// <summary>
	/// The level requirements for entering the tower.
	/// Includes minimum, maximum, and recommended character levels for access.
	/// </summary>
	public LevelRequirement Requirements { get; set; }

	/// <summary>
	/// The difficulty settings for the tower, including difficulty scaling based on the character's level.
	/// </summary>
	public FloorDifficulty Difficulty { get; set; }

	// Tower Structure
	/// <summary>
	/// The list of floors within the tower.
	/// Contains data for each individual floor (e.g., type, state, difficulty).
	/// </summary>
	public List<FloorData> Floors { get; set; }

	/// <summary>
	/// A dictionary tracking which floors are unlocked.
	/// The key is the floor ID, and the value indicates whether the floor is unlocked (true/false).
	/// </summary>
	public Dictionary<string, bool> UnlockedFloors { get; set; }

	// State Tracking
	/// <summary>
	/// The current state of the tower (e.g., Locked, Available, InProgress, Failed).
	/// Tracks the progression status of the tower.
	/// </summary>
	public TowerState CurrentState { get; set; }

	/// <summary>
	/// The time of the last attempt to enter or interact with the tower.
	/// Useful for tracking when the last progress was made or when the tower was last accessed.
	/// </summary>
	public DateTime LastAttemptTime { get; set; }

	/// <summary>
	/// The number of times the tower has been completed.
	/// Tracks how many successful attempts have been made to complete the tower.
	/// </summary>
	public int CompletionCount { get; set; }

	// Version Control
	/// <summary>
	/// The version of the tower.
	/// Tracks updates or changes to the tower (e.g., for managing updates or compatibility).
	/// </summary>
	public string Version { get; set; }

	/// <summary>
	/// The date and time when the tower was last updated.
	/// Useful for tracking when the tower's data was last modified or updated.
	/// </summary>
	public DateTime LastUpdated { get; set; }

	/// <summary>
	/// Default constructor for initializing a new TowerData object.
	/// Initializes default values for the tower properties.
	/// </summary>
	public TowerData()
	{
		Requirements = new LevelRequirement(); // Initializes the level requirements for the tower
		Difficulty = new FloorDifficulty(); // Initializes the difficulty settings for the tower
		Floors = new List<FloorData>(); // Initializes an empty list of floors
		UnlockedFloors = new Dictionary<string, bool>(); // Initializes an empty dictionary for unlocked floors
		CurrentState = TowerState.Locked; // Sets the default state of the tower to "Locked"
	}


}