using Godot;
using System;
using static GlobalEnums;

public class ResourceUnlockEventArgs : GameEventArgs
{
	public ResourceType Type { get; }

	public ResourceUnlockEventArgs(ResourceType type)
	{
		Type = type;
	}
}