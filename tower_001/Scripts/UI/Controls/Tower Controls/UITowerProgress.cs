using Godot;
using System;

// Progress tracking for tower and floor
public partial class UITowerProgress : Control, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }

	private ProgressBar _towerProgress;
	private ProgressBar _floorProgress;
	private Label _towerLabel;
	private Label _floorLabel;

	public override void _Ready()
	{
		InitializeComponents();
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		throw new NotImplementedException();
	}

	private void InitializeComponents()
	{
		var container = new VBoxContainer();
		AddChild(container);

		// Tower progress
		var towerContainer = new HBoxContainer();
		_towerLabel = new Label { Text = "Tower Progress:" };
		_towerProgress = new ProgressBar
		{
			CustomMinimumSize = new Vector2(200, 20),
			Step = 0.01f
		};
		towerContainer.AddChild(_towerLabel);
		towerContainer.AddChild(_towerProgress);
		container.AddChild(towerContainer);

		// Floor progress
		var floorContainer = new HBoxContainer();
		_floorLabel = new Label { Text = "Floor Progress:" };
		_floorProgress = new ProgressBar
		{
			CustomMinimumSize = new Vector2(200, 20),
			Step = 0.01f
		};
		floorContainer.AddChild(_floorLabel);
		floorContainer.AddChild(_floorProgress);
		container.AddChild(floorContainer);
	}

	public void UpdateProgress(
		int currentFloor, int totalFloors,
		int currentRoom, int totalRooms)
	{
		_towerProgress.MaxValue = totalFloors;
		_towerProgress.Value = currentFloor;
		_towerLabel.Text = $"Tower Progress: {currentFloor}/{totalFloors}";

		_floorProgress.MaxValue = totalRooms;
		_floorProgress.Value = currentRoom;
		_floorLabel.Text = $"Floor Progress: {currentRoom}/{totalRooms}";
	}

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
		throw new NotImplementedException();
	}

	public void CallEvent()
	{
		throw new NotImplementedException();
	}

	// IUIItem implementation...
}
