using Godot;
using System;
using static GlobalEnums;
using System.Linq;
using System.Collections.Generic;

[Tool]
public partial class CharacterStatsPanel : Panel
{
	private Dictionary<StatType, UIStatDisplay> _statDisplays = new();

	public void Initialize(string characterId)
	{
		foreach (var display in GetNode("Stats").GetChildren().OfType<UIStatDisplay>())
		{
			_statDisplays[display.WatchedStat] = display;
			display.Initialize(characterId);
		}
	}
}