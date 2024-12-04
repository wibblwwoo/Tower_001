using Godot;
using System;


public partial class UIControlItem : BasePanel, IUIObject
{



	public UIControlItem()
	{
		base.SetPanelID(ObjectID);
		
	}
	[Export]
	public string ObjectID { get; set; }
	[Export]
	public GlobalEnums.PanelPosition PanelLocation { get; set; }
	[Export]
	public GlobalEnums.PanelSection PanelSection { get; set; }
	[Export]
	public GlobalEnums.UIControlType ControlType { get; set; }

	public void RegisterControl()
	{

		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register,this, ObjectID, PanelLocation, PanelSection, ControlType);
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);

	}

	public void RegisterControl(IMenu menu)
	{

		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register, this, ObjectID, PanelLocation, PanelSection, ControlType);
		uiControlEvent.ControlParent = menu;
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);

	}
}
