using static GlobalEnums;
using System.Collections.Generic;
using System;
/// <summary>
/// Event arguments for room-related events in a floor.
/// Provides details about the room's state, type, and associated tower and floor data.
/// </summary>
public class RoomEventArgs : FloorEventArgs
{
	/// <summary>
	/// The unique identifier of the room within the floor.
	/// </summary>
	public string RoomId { get; }

	/// <summary>
	/// The type of the room (e.g., combat, reward, boss).
	/// </summary>
	public RoomType Type { get; }

	/// <summary>
	/// The current state of the room (e.g., locked, in progress, cleared).
	/// </summary>
	public RoomState State { get; }

	/// <summary>
	/// Additional state information for the room stored as key-value pairs.
	/// Useful for custom data or extensions.
	/// </summary>
	public Dictionary<string, object> RoomState { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoomEventArgs"/> class.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower containing the room.</param>
	/// <param name="towerName">The name of the tower containing the room.</param>
	/// <param name="floorNumber">The number of the floor containing the room.</param>
	/// <param name="floorId">The unique identifier of the floor containing the room.</param>
	/// <param name="roomId">The unique identifier of the room.</param>
	/// <param name="type">The type of the room.</param>
	/// <param name="state">The current state of the room.</param>
	public RoomEventArgs(
		string towerId,
		string towerName,
		int floorNumber,
		string floorId,
		string roomId,
		RoomType type,
		RoomState state)
		: base(towerId, floorId, floorNumber, 0, 0) // Call the base class constructor
	{
		RoomId = roomId; // Set the room's unique ID
		Type = type; // Assign the room type
		State = state; // Assign the room state
		RoomState = new Dictionary<string, object>(); // Initialize an empty dictionary for additional state information
	}
}

/// <summary>
/// Event arguments for when a room's state changes.
/// Provides details about the old state and new state of the room.
/// </summary>
public class RoomStateChangedArgs : RoomEventArgs
{
	/// <summary>
	/// The state of the room before the change.
	/// </summary>
	public RoomState OldState { get; }

	/// <summary>
	/// The state of the room after the change.
	/// </summary>
	public new RoomState NewState { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoomStateChangedArgs"/> class.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower containing the room.</param>
	/// <param name="towerName">The name of the tower containing the room.</param>
	/// <param name="floorNumber">The number of the floor containing the room.</param>
	/// <param name="floorId">The unique identifier of the floor containing the room.</param>
	/// <param name="roomId">The unique identifier of the room.</param>
	/// <param name="oldState">The state of the room before the change.</param>
	/// <param name="newState">The state of the room after the change.</param>
	public RoomStateChangedArgs(
		string towerId,
		string towerName,
		int floorNumber,
		string floorId,
		string roomId,
		RoomState oldState,
		RoomState newState)
		: base(towerId, towerName, floorNumber, floorId, roomId, RoomType.None, newState) // Call base constructor
	{
		OldState = oldState; // Assign the previous room state
		NewState = newState; // Assign the new room state
	}
}

/// <summary>
/// Event arguments for when a room is completed.
/// Provides details about the success of the completion and the time taken to complete the room.
/// </summary>
public class RoomCompletionEventArgs : RoomEventArgs
{
	/// <summary>
	/// Indicates whether the room was successfully completed.
	/// </summary>
	public bool Success { get; }

	/// <summary>
	/// The time taken to complete the room.
	/// </summary>
	public TimeSpan CompletionTime { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoomCompletionEventArgs"/> class.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower containing the room.</param>
	/// <param name="towerName">The name of the tower containing the room.</param>
	/// <param name="floorNumber">The number of the floor containing the room.</param>
	/// <param name="floorId">The unique identifier of the floor containing the room.</param>
	/// <param name="roomId">The unique identifier of the room.</param>
	/// <param name="success">Indicates whether the room was successfully completed.</param>
	/// <param name="completionTime">The time taken to complete the room.</param>
	public RoomCompletionEventArgs(
		string towerId,
		string towerName,
		int floorNumber,
		string floorId,
		string roomId,
		bool success,
		TimeSpan completionTime)
		: base(towerId, towerName, floorNumber, floorId, roomId, RoomType.None, GlobalEnums.RoomState.Cleared) // Call base constructor
	{
		Success = success; // Assign the success status
		CompletionTime = completionTime; // Assign the completion time
	}
}

/// <summary>
/// Event arguments for when new room paths are discovered or updated.
/// Provides details about the room and the available paths leading from it.
/// </summary>
public class RoomPathEventArgs : RoomEventArgs
{
	/// <summary>
	/// A list of available paths leading from the room.
	/// </summary>
	public List<string> AvailablePaths { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoomPathEventArgs"/> class.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower containing the room.</param>
	/// <param name="towerName">The name of the tower containing the room.</param>
	/// <param name="floorNumber">The number of the floor containing the room.</param>
	/// <param name="floorId">The unique identifier of the floor containing the room.</param>
	/// <param name="roomId">The unique identifier of the room.</param>
	/// <param name="type">The type of the room.</param>
	/// <param name="state">The current state of the room.</param>
	/// <param name="paths">A list of paths leading from the room.</param>
	public RoomPathEventArgs(
		string towerId,
		string towerName,
		int floorNumber,
		string floorId,
		string roomId,
		RoomType type,
		RoomState state,
		List<string> paths)
		: base(towerId, towerName, floorNumber, floorId, roomId, type, state) // Call base constructor
	{
		AvailablePaths = paths; // Assign the available paths
	}
}