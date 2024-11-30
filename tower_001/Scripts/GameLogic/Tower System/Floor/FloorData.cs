using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
/// <summary>
/// Represents a single floor in the tower, including its rooms and difficulty settings.
/// Tracks the floor's state, type, and connections to other floors.
/// </summary>
public partial class FloorData
{
	// Basic Properties

	/// <summary>
	/// A unique identifier for the floor.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// The name of the floor, used for display or identification purposes.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The type of floor (e.g., normal, boss, special event).
	/// </summary>
	public FloorType Type { get; set; }

	/// <summary>
	/// The current state of the floor (e.g., locked, in progress, completed).
	/// </summary>
	public FloorState CurrentState { get; set; }

	/// <summary>
	/// The difficulty settings for the floor, including base difficulty and scaling factors.
	/// </summary>
	public FloorDifficulty Difficulty { get; set; }

	/// <summary>
	/// The ID of the next floor connected to this one, if any.
	/// </summary>
	public string NextFloorId { get; set; }

	/// <summary>
	/// The numerical position of the floor in the tower (e.g., 1 for the first floor).
	/// </summary>
	public int FloorNumber { get; set; }

	// Room-Related Properties

	/// <summary>
	/// A list of rooms that belong to this floor.
	/// Each room contains its own state, type, and difficulty.
	/// </summary>
	public List<RoomData> Rooms { get; set; } = new();

	/// <summary>
	/// Indicates whether this floor is designated as a boss floor.
	/// </summary>
	public bool IsBossFloor { get; set; }

	/// <summary>
	/// The ID of the boss associated with this floor, if applicable.
	/// </summary>
	public string BossId { get; set; }


	/// <summary>
	/// Initializes a new instance of the <see cref="FloorData"/> class with default values.
	/// </summary>
	public FloorData()
	{
		// Set default difficulty settings for the floor
		Difficulty = new FloorDifficulty();

		// Initialize the floor's state to locked
		CurrentState = FloorState.Locked;

		// Initialize an empty list of rooms
		Rooms = new List<RoomData>();
	}
}
