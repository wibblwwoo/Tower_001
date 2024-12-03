using Godot;
using System;

// Base interface for panel events
public interface IPanelEventHandler
{
	void HandlePanelEvent(string eventName, object data);
}