using Godot;
using System;
using static GlobalEnums;

public partial class UITowerNavigationPanel : Panel
{
	[Export]
	private UIFloorNavigation _floorNav;
	[Export]
	private UIRoomNavigation _roomNav;

	public override void _Ready()
	{
		
		//_roomNav = Globals.Instance.RootNode.GetNode<UIRoomNavigation>("RoomNavigation");

		// Example of adding custom room types and symbols
		RoomDisplayConfig.SetRoomColor(RoomType.MiniBoss, Colors.Cyan);
		RoomDisplayConfig.AddEventSymbol("PuzzleRoom", "🧩");
	}

	public void Initialize(string floorId)
	{
		//_roomNav.Initialize(floorId);
	}
}