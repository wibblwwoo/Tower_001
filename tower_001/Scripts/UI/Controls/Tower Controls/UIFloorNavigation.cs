using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class UIFloorNavigation : PanelContainer, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }
	[Export] public int DisplayCount { get; set; } = 5; // Number of rooms to show on each side

	[Export]
	private HBoxContainer Parent { get; set; }


	private List<UIFloor_Item> _floorItems = new List<UIFloor_Item>();

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
			var item = new UIFloor_Item();
			_floorItems.Add(item);
			Parent.AddChild(item);

			// Middle item gets special styling
			if (i == DisplayCount)
			{
				item.AddThemeColorOverride("font_color", Colors.Green);
				item.AddThemeConstantOverride("font_size", 16); // Slightly larger
			}
			item.Text = "0001";
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

	private void InitializeTooltip()
	{
		_tooltipPanel = new Panel
		{
			Visible = false
		};

		_tooltipContent = new VBoxContainer();
		_tooltipPanel.AddChild(_tooltipContent);

		AddChild(_tooltipPanel);
	}

	//public void Initialize(string towerId)
	//{
	//	_towerId = towerId;
	//	Refresh();
	//}

	//public void UpdateDisplay(FloorData currentFloor, List<FloorData> completedFloors)
	//{
	//	int currentIndex = currentFloor.FloorNumber - 1;
	//	bool hasAlternatePath = currentFloor.HasAlternatePath;

	//	for (int i = 0; i < _floorItems.Count; i++)
	//	{
	//		int floorNumber = currentIndex - DisplayCount + i;
	//		var item = _floorItems[i];

	//		if (floorNumber < 0 || floorNumber >= completedFloors.Count)
	//		{
	//			item.Clear();
	//			continue;
	//		}

	//		var floor = completedFloors[floorNumber];
	//		bool isCurrent = floorNumber == currentIndex;
	//		bool isCompleted = floorNumber < currentIndex;

	//		item.UpdateFloor(floor, isCurrent, isCompleted);
	//	}

	//	//_branchIndicator.Visible = hasAlternatePath;
	//}

	private void RegisterEvents()
	{
		//var events = Globals.Instance.gameMangers.Events;

		//events.AddHandler<FloorEventArgs>(
		//	EventType.FloorEntered,
		//	OnFloorChanged);

		//events.AddHandler<FloorPathEventArgs>(
		//	EventType.FloorPathAvailable,
		//	OnPathsAvailable);
	}

	//private void OnFloorChanged(FloorEventArgs args)
	//{
	//	if (args.TowerId != _towerId) return;
	//	Refresh();
	//}

	//private void OnPathsAvailable(FloorPathEventArgs args)
	//{
	//	if (args.TowerId != _towerId) return;
	//	_branchIndicator.Visible = args.AvailablePaths.Count > 0;
	//}

	// IUIItem implementation
	public void Setup() { }
	public void EventFire() { }
	public void Refresh() { /* Implementation */ }
	public void Visiblity(bool visible)
	{
		Visible = visible;
	}
	public void CallEvent() { }

}