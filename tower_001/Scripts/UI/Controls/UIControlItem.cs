using Godot;
using System;

/// <summary>
/// Base class for UI control items in the dynamic menu system.
/// Implements IUIObject interface and extends BasePanel to provide core UI control functionality.
/// Controls can be registered independently or as part of a menu structure.
/// </summary>
public partial class UIControlItem : BasePanel, IUIObject
{
    /// <summary>
    /// Initializes a new instance of UIControlItem.
    /// Sets up the panel ID using the ObjectID property.
    /// </summary>
	public UIControlItem()
	{
		base.SetPanelID(ObjectID);
	}

    /// <summary>
    /// Unique identifier for the UI control.
    /// Exposed to Godot editor through Export attribute.
    /// </summary>
	[Export]
	public string ObjectID { get; set; }

    /// <summary>
    /// Defines the position of this control in the UI layout (e.g., Left, Right, Top).
    /// Used for automatic layout management.
    /// </summary>
	[Export]
	public GlobalEnums.PanelPosition PanelLocation { get; set; }

    /// <summary>
    /// Specifies which section of the panel this control belongs to.
    /// Used for organizing controls within their container panels.
    /// </summary>
	[Export]
	public GlobalEnums.PanelSection PanelSection { get; set; }

    /// <summary>
    /// Defines whether this is a container or individual control.
    /// Affects how the UI system handles this item in the layout.
    /// </summary>
	[Export]
	public GlobalEnums.UIControlType ControlType { get; set; }


	/// <summary>
	/// is this control visible by default. or is it turned off by default
	/// </summary>
	[Export]
	public bool IsVisibleByDefault { get; set; }



	/// <summary>
	/// Registers this control with the UI system without a parent menu.
	/// Typically used for container controls or standalone UI elements.
	/// </summary>
	public virtual void RegisterControl()
	{
		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register,this, ObjectID, PanelLocation, PanelSection, ControlType);
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);

	}

    /// <summary>
    /// Registers this control with the UI system as part of a specific menu.
    /// Used for controls that belong to a menu hierarchy.
    /// </summary>
    /// <param name="menu">The parent menu that owns this control</param>
	public virtual void RegisterControl(IMenu menu)
	{
		var uiControlEvent = new UIControlEventArgs(GlobalEnums.UIPanelEvent.Register, this, ObjectID, PanelLocation, PanelSection, ControlType);
		uiControlEvent.ControlParent = menu;
		Globals.Instance.gameMangers.Events.RaiseEvent(GlobalEnums.EventType.UIControlRegister, uiControlEvent);
	}
}
