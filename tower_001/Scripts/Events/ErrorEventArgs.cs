using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Arguments for error events
/// </summary>
public class ErrorEventArgs : GameEventArgs
{
	public EventType OriginalEventType { get; set; }
	public GameEventArgs OriginalEventArgs { get; set; }
	public Exception Error { get; set; }
}
