using Godot;
using System;
using System.Collections.Generic;

// Event arguments classes
public class GameEventArgs : EventArgs
{
	/// <summary>
	/// Timestamp when the event was created
	/// </summary>
	public DateTime Timestamp { get; } = DateTime.UtcNow;

	/// <summary>
	/// Optional context data for the event
	/// </summary>
	public Dictionary<string, object> Context { get; } = new();
}



