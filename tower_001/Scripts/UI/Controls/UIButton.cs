using Godot;
using System;

public partial class UIButton : Button, IUIObject 
{
	public string menuParentID { get; set; }
	public string ObjectID { get; set; }
	public GlobalEnums.PanelPosition PanelLocation { get; set; }
	public GlobalEnums.PanelSection PanelSection { get; set; }
	public GlobalEnums.UIControlType ControlType { get; set; }
	public bool IsVisibleByDefault { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RegisterControl()
	{

		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register, this, ObjectID, PanelLocation, PanelSection, ControlType);
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);

	}

	public void RegisterControl(IMenu menu)
	{

		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register, this, ObjectID, PanelLocation, PanelSection, ControlType);
		uiControlEvent.ControlParent = menu;
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlAssignToContainer , uiControlEvent);

	}
	public void Update() { }

	/// <summary>
	/// we can use this to figure out what it is doing
	/// </summary>
	/// <param name="id"></param>
	public void SetParentID(string id) {
		menuParentID = id;
	}
}
