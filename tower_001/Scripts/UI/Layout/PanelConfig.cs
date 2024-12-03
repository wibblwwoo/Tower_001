using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;


// Panel Config to define panel requirements and settings
public class PanelConfig
{
	public string PanelId { get; }
	public PanelType Type { get; }
	public bool RequiresTopSection { get; }
	public bool RequiresBottomSection { get; }
	public Dictionary<string, object> Settings { get; }

	public PanelConfig(string panelId, PanelType type, bool requiresTop = false, bool requiresBottom = false)
	{
		PanelId = panelId;
		Type = type;
		RequiresTopSection = requiresTop;
		RequiresBottomSection = requiresBottom;
		Settings = new Dictionary<string, object>();
	}
}