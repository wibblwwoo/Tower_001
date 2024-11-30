using Godot;
using System;
using System.Collections.Generic;

// Event arguments classes
public class GameEventArgs : EventArgs
{
	/// <summary>
	/// Timestamp when the event was created
	/// </summary>
	public DateTime Timestamp { get; } = DateTime.UtcNow;

	/// <summary>
	/// Optional context data for the event
	/// </summary>
	public Dictionary<string, object> Context { get; } = new();
}


#region UI WIndows EventArgs
public class UIPanel_Visibility_EventArgs : GameEventArgs 
{
	// Base class for all game event arguments
	public UIPanel window;

	public UIPanel_Visibility_EventArgs(UIPanel window)
	{
		this.window = window;
	}
}

#endregion

//#region UI Button Event Args
public partial class UIButton_Click_EventArgs : GameEventArgs
{
	// Called when the node enters the scene tree for the first time.
	public GlobalEnums.EnumUIPanelParentType ShowPanels;

	public UIButton_Click_EventArgs(GlobalEnums.EnumUIPanelParentType showPanels)
	{
		ShowPanels = showPanels;
	}
}


//public partial class UIButton_Visible_EventArgs : GameEventArgs
//{
//	// Called when the node enters the scene tree for the first time.
//	public List<UIButton_Item> MakeVisible;
//	public bool Visibility;

//	public UIButton_Visible_EventArgs(List<UIButton_Item> makeVisible, bool visibility)
//	{
//		MakeVisible = makeVisible;
//		Visibility = visibility;
//	}

//}
//#endregion
