using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for UI control-related events in the dynamic menu system.
/// This class handles the communication of UI control states and actions between different parts of the system.
/// </summary>
public class UIControlEventArgs : GameEventArgs
{
    /// <summary>
    /// The type of action being performed on the UI control (e.g., Register, Update)
    /// </summary>
	public UIPanelEvent Action { get; }

    /// <summary>
    /// The position of the panel in the UI layout (e.g., Left, Right, Top)
    /// </summary>
	public PanelPosition PanelLocation { get; }

    /// <summary>
    /// The section within the panel where the control should be placed
    /// </summary>
	public PanelSection PanelSection { get; }

    /// <summary>
    /// The UI control object that is the subject of this event
    /// Implements IUIObject interface for flexibility in control types
    /// </summary>
	public IUIObject UIControlItem { get; }
	
    /// <summary>
    /// Specifies the type of UI control (e.g., Container, Control)
    /// Used to determine how the control should be handled in the UI system
    /// </summary>
	public UIControlType uIControlType { get; }

    /// <summary>
    /// Unique identifier for the control
    /// Used for tracking and managing controls in collections
    /// </summary>
	public string ControlID { get; }

    /// <summary>
    /// Reference to the parent menu that owns this control
    /// Can be null for top-level or container controls
    /// </summary>
	public IMenu ControlParent { get; set; }


    /// <summary>
    /// Initializes a new instance of UIControlEventArgs
    /// </summary>
    /// <param name="action">The action being performed on the control</param>
    /// <param name="uIControlItem">The UI control object</param>
    /// <param name="controlID">Unique identifier for the control</param>
    /// <param name="postion">Position in the UI layout</param>
    /// <param name="panelSection">Section within the panel</param>
    /// <param name="controlType">Type of UI control</param>
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
