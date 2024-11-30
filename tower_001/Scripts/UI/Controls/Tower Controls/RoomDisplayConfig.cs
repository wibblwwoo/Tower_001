using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public static class RoomDisplayConfig
{
	// Default color mappings
	private static readonly Dictionary<RoomType, Color> _defaultColors = new()
	{
		{ RoomType.Combat, Colors.Red },
		{ RoomType.Event, Colors.Purple },
		{ RoomType.Reward, Colors.Gold },
		{ RoomType.Rest, Colors.Blue }
	};

	// Custom color mappings
	private static Dictionary<RoomType, Color> _customColors = new();

	// Symbol mappings
	private static readonly Dictionary<string, string> _eventSymbols = new()
	{
		{ "StandardCombat", "⚔" },
		{ "EliteCombat", "☠" },
		{ "BossCombat", "⚜" },
		{ "Treasure", "💎" },
		{ "Rest", "🏕" },
		{ "Shop", "💰" },
		{ "Mystery", "?" },
		{ "Choice", "!" },
		{ "Puzzle", "❓" },
		{ "Hidden", "✧" }
	};

	public static void SetRoomColor(RoomType type, Color color)
	{
		_customColors[type] = color;
	}

	public static Color GetRoomColor(RoomType type)
	{
		return _customColors.TryGetValue(type, out var customColor)
			? customColor
			: _defaultColors.TryGetValue(type, out var defaultColor)
				? defaultColor
				: Colors.White;
	}

	public static void AddEventSymbol(string eventType, string symbol)
	{
		_eventSymbols[eventType] = symbol;
	}

	public static string GetEventSymbol(string eventType)
	{
		return _eventSymbols.TryGetValue(eventType, out var symbol) ? symbol : "•";
	}
}