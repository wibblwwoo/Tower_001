using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public partial class ManagerErrorEventArgs : EventArgs
{
	/// <summary>
	/// Timestamp when the event was created
	/// </summary>
	public DateTime Timestamp { get; } = DateTime.UtcNow;

	/// <summary>
	/// Optional context data for the event
	/// </summary>
	public Dictionary<string, object> Context { get; } = new();

	public Object ContextValue { get; } = new();

	public ManagerErrorType errorType;

	public ManagerErrorEventArgs(object contextValue, ManagerErrorType errorType, ManagerException errorMessage)
	{
		ContextValue = contextValue;
		this.errorType = errorType;
		ErrorMessage = errorMessage;
	}

	public ManagerException ErrorMessage { get; set; }	

}
