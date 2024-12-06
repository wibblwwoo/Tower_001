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
		MainMenu,
		GameSettings,
		TowerSettings,
		CharacterSettings,




		TowerSettings_Floor,
		TowerSettings_Room,
		TowerSettings_Events,
		TowerSettings_Style,
		TowerSettings_Tower,
	}
	public enum UIPanelEvent
	{
		Register,
		Visible,
		Hide,
		Refresh,
	}

	public enum PanelPosition
	{
		Top,
		Middle,
		Bottom
	}

	public enum PanelSection
	{
		Left,
		Right,
	}

	public enum UIControlType
	{
		Container,
		Control,
	}

	public enum UIControlDataContainer
	{

		Resource_Top_HBox
	}
	public enum UIElementType
	{
		ResourcePanel,
		InventorySlot,
		MenuButton,
		DialogWindow,
		// Add other types as needed
	}

}
