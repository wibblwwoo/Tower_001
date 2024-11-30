using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for tower-related events, such as initialization, entry, and failure.
/// Provides contextual information about the tower's state and progression.
/// </summary>
public class TowerEventArgs : GameEventArgs
{
	/// <summary>
	/// Unique identifier of the tower
	/// </summary>
	public string TowerId { get; set; }

	/// <summary>
	/// Previous state of the tower before the event
	/// </summary>
	public TowerState PreviousState { get; set; }

	/// <summary>
	/// New state of the tower after the event
	/// </summary>
	public TowerState NewState { get; set; }

	/// <summary>
	/// Optional message providing additional context about the event
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Optional difficulty level at the time of the event
	/// </summary>
	public float? Difficulty { get; set; }

	/// <summary>
	/// Constructor for creating tower event arguments
	/// </summary>
	/// <param name="towerId">Unique identifier of the tower</param>
	/// <param name="previousState">Previous state of the tower</param>
	/// <param name="newState">New state of the tower</param>
	/// <param name="message">Optional message providing context</param>
	/// <param name="difficulty">Optional difficulty level</param>
	public TowerEventArgs(
		string towerId,
		TowerState previousState,
		TowerState newState,
		string message = "",
		float? difficulty = null) : base()
	{
		TowerId = towerId; // Set the unique identifier of the tower
		PreviousState = previousState; // Set the previous state of the tower before the event
		NewState = newState; // Set the new state of the tower after the event
		Message = message; // Set the optional message providing context for the event
		Difficulty = difficulty; // Set the optional difficulty level during the event

		// Add tower-specific context to base GameEventArgs (this is useful for event logging or debugging)
		Context["TowerId"] = towerId; // Store the tower ID in the context dictionary
		Context["StateTransition"] = $"{previousState} -> {newState}"; // Store the state transition in the context
	}
}