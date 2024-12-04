using Godot;
using System;
using static GlobalEnums;

public class UIControlEventArgs : GameEventArgs
{
	public UIPanelEvent Action { get; }
	public PanelPosition PanelLocation { get; }

	public PanelSection PanelSection { get; }
	public IUIObject UIControlItem { get; }
	
	public UIControlType uIControlType { get; }
	public string ControlID { get; }

	public IMenu ControlParent { get; set; }


	public UIControlEventArgs(UIPanelEvent action, IUIObject uIControlItem, string controlID, PanelPosition postion, PanelSection panelSection, UIControlType controlType)
	{
		Action = action;
		UIControlItem = uIControlItem;
		ControlID = controlID;
		PanelLocation = postion;
		PanelSection = panelSection;
		uIControlType = controlType;
	}
}
