using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class UIRoomNavigation : PanelContainer, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }
	[Export] public int DisplayCount { get; set; } = 5; // Number of rooms to show on each side

	[Export]
	private HBoxContainer Parent { get; set; }


	private List<UIRoom_Item> _roomItems = new List<UIRoom_Item>();

	private Panel _tooltipPanel;
	private VBoxContainer _tooltipContent;

	private Label _alternatePathIndicator;
	private string _floorId;

	public override void _Ready()
	{
		InitializeComponents();
		RegisterEvents();
	}

	private void InitializeComponents()
	{

		// Create room items
		for (int i = 0; i < (DisplayCount * 2 + 1); i++)
		{
			UIRoom_Item item = new UIRoom_Item();
			_roomItems.Add(item);
			Parent.AddChild(item);

			item.Text = "122312";

			// Middle item gets special styling
			//if (i == DisplayCount)
			//{
			//	item.AddThemeConstantOverride("font_size", 16);
			//}
		}

		// Alternate path indicator
		//_alternatePathIndicator = new Label
		//{
		//	Text = "↳ Alternate Path Available",
		//	Modulate = Colors.Yellow,
		//	Visible = false
		//};
		// Initialize tooltip
		InitializeTooltip();
	}

	//public void UpdateDisplay(string floorId, RoomData currentRoom, List<RoomData> floorRooms, bool hasAlternatePath)
	//{
	//	_floorId = floorId;
	//	int currentIndex = floorRooms.IndexOf(currentRoom);

	//	for (int i = 0; i < _roomItems.ToList().Count; i++)
	//	{
	//		int roomIndex = currentIndex - DisplayCount + i;
	//		var item = _roomItems[i];

	//		if (roomIndex < 0 || roomIndex >= floorRooms.Count)
	//		{
	//			item.Clear();
	//			continue;
	//		}

	//		var room = floorRooms[roomIndex];
	//		bool isCurrent = roomIndex == currentIndex;
	//		bool isCompleted = roomIndex < currentIndex;

	//		item.UpdateRoom(room, isCurrent, isCompleted);
	//	}

	//	_alternatePathIndicator.Visible = hasAlternatePath;
	//}

	private void InitializeTooltip()
	{
		_tooltipPanel = new Panel { Visible = false };
		_tooltipContent = new VBoxContainer();
		_tooltipPanel.AddChild(_tooltipContent);
		AddChild(_tooltipPanel);
	}

	private void RegisterEvents()
	{
		//var events = Globals.Instance.gameMangers.Events;

		//events.AddHandler<RoomEventArgs>(
		//	EventType.RoomEntered,
		//	OnRoomChanged);

		//events.AddHandler<RoomEventArgs>(
		//	EventType.RoomEventCompleted,
		//	OnRoomCompleted);
	}

	//private void OnRoomChanged(RoomEventArgs args)
	//{
	//	throw new NotImplementedException();
	//}

	//private void OnRoomCompleted(RoomEventArgs args)
	//{
	//	throw new NotImplementedException();
	//}

	public void Setup()
	{
		throw new NotImplementedException();
	}

	public void EventFire()
	{
		throw new NotImplementedException();
	}

	public void Refresh()
	{
		throw new NotImplementedException();
	}

	public void Visiblity(bool visible)
	{

	}

	public void CallEvent()
	{
		throw new NotImplementedException();
	}

	// Event handlers...
}