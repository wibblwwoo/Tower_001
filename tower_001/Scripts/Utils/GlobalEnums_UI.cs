using Godot;
using System;

public static partial class GlobalEnums 
{
	// Called when the node enters the scene tree for the first time.


	public enum EnumUIPanelType
	{
		None = 0,
		LeftPanel = 1,
		MiddlePanel = 2,
		RightPanel = 3,
		TownPanel = 4,
		CharacterPanel = 5,
		InventoryPanel = 6,
		SkillsPanel = 7,
		StatsPanel = 8,
		QuestPanel = 9,
		LeftPanel_Top = 10,
		LeftPanel_Middle = 11,
		LeftPanel_Bottom = 12,
		LeftPanel_BottomBackPanel = 13,
		MiddlePanel_Background = 14,
		MiddlePanel_Top = 15,
		MiddlePanel_Middle = 16,
		MiddlePanel_Bottom = 17,
		MiddlePanel_Resource = 15,
		MiddlePanel_Data = 16,
	}

	[Flags]
	public enum EnumUIPanelParentType
	{

		None = 0,
		Menu_Start = 1 << 0,
		Menu_NewGame = 1 << 1,
		Menu_CharacterSelect = 1 << 2,
		Menu_Setting = 1 << 3,
		Game_LeftMenu = 1 << 4,	
		Game_MiddlePanel = 1 << 5,
		Game_ResourcePanel = 1 << 6,
	}


	public enum EnumUIButtonType
	{
		None = 0,
		Character = 1,
		Town = 2,
		Inventory = 3,
		Skills = 4,
		Quest = 5
	}

}
