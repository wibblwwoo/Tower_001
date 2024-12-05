using Godot;
using System;

public static partial class GlobalEnums
{
	public enum ManagerState
	{
		Uninitialized,
		Initializing,
		Ready,
		Error,
		ShuttingDown,
		Disabled
	}

}