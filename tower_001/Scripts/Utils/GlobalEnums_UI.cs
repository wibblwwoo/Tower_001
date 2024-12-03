using Godot;
using System;

public static partial class GlobalEnums
{
	// Called when the node enters the scene tree for the first time.

	public enum GameAction
	{
		None = 0,
		Exit =1,
	}
	public enum MenuAction
	{
		NewGame,
		Settings,
		LoadGame,
		SaveGame,
		Exit,
		Back,
		Controls,
		Graphics,
		Audio,
		MainMenu
	}
	public enum PanelType
	{
		Single,     // Full middle section
		Dual_Horizontal, // Split middle section horizontal
		Multi_Horizontal, // 3+ panels horizontal
		Dual_Vertical, // Split middle section horizontal
		Multi_Vertical // 3+ panels horizontal	
	}

	public enum PanelPosition
	{
		Top,
		Middle,
		Bottom
	}

}
