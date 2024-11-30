using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for floor-related events, such as initialization, entry, and completion.
/// Includes details about the floor's state and its relation to the tower.
/// </summary>
public class FloorEventArgs : GameEventArgs
{
	/// <summary>
	/// Tower ID that owns this floor
	/// </summary>
	public string TowerId { get; }

	/// <summary>
	/// Floor ID within the tower
	/// </summary>
	public string FloorId { get; }

	/// <summary>
	/// Floor number in the tower sequence
	/// </summary>
	public int FloorNumber { get; }

	/// <summary>
	/// Previous state of the floor before the event
	/// </summary>
	public FloorState PreviousState { get; }

	/// <summary>
	/// New state of the floor after the event
	/// </summary>
	public FloorState NewState { get; }

	/// <summary>
	/// Optional difficulty level at the time of the event
	/// </summary>
	public float? Difficulty { get; }

	/// <summary>
	/// Optional message providing additional context about the event
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="FloorEventArgs"/> class.
	/// Represents an event related to a specific floor within a tower, including its state changes and additional context.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower containing the floor.</param>
	/// <param name="floorId">The unique identifier of the floor associated with the event.</param>
	/// <param name="floorNumber">The number of the floor within the tower.</param>
	/// <param name="previousState">The state of the floor before the event occurred.</param>
	/// <param name="newState">The state of the floor after the event occurred.</param>
	/// <param name="difficulty">Optional difficulty level associated with the floor during the event.</param>
	/// <param name="message">Optional message providing additional context about the event.</param>
	public FloorEventArgs(
		string towerId,
		string floorId,
		int floorNumber,
		FloorState previousState,
		FloorState newState,
		float? difficulty = null,
		string message = "",
		string characterId = "")
	{
		TowerId = towerId; // Set the unique identifier for the tower
		FloorId = floorId; // Set the unique identifier for the floor
		FloorNumber = floorNumber; // Assign the floor's number within the tower
		PreviousState = previousState; // Store the floor's previous state
		NewState = newState; // Store the floor's new state
		Difficulty = difficulty; // Optionally assign the difficulty level for the floor
		Message = message; // Optionally provide additional context about the event

		// Add floor-specific context to the base GameEventArgs context dictionary
		Context["TowerId"] = towerId; // Add the tower ID to the context
		Context["FloorId"] = floorId; // Add the floor ID to the context
		Context["FloorNumber"] = floorNumber; // Add the floor number to the context
		Context["StateTransition"] = $"{previousState} -> {newState}"; // Log the state transition as a string

		// If difficulty is provided, add it to the context
		if (difficulty.HasValue)
		{
			Context["Difficulty"] = difficulty.Value;
		}
	}

	/// <summary>
	/// Creates a <see cref="FloorEventArgs"/> instance for a floor generation completion event.
	/// Used when all floors in a tower are generated and become available.
	/// </summary>
	/// <param name="towerId">The unique identifier of the tower where floors were generated.</param>
	/// <param name="totalFloors">The total number of floors generated for the tower.</param>
	/// <param name="message">Optional message providing additional context about the generation event.</param>
	/// <returns>
	/// A new instance of <see cref="FloorEventArgs"/> with the state transition from locked to available for all floors.
	/// </returns>
	public static FloorEventArgs CreateGenerationComplete(
		string towerId,
		int totalFloors,
		string message = "")
	{
		return new FloorEventArgs(
			towerId, // The tower where the event occurred
			"all", // Indicates the event applies to all floors in the tower
			totalFloors, // Total number of floors generated
			FloorState.Locked, // Floors were previously locked
			FloorState.Available, // Floors are now available
			null, // No specific difficulty value for this event
			message // Additional context or message about the event
		);
	}
}