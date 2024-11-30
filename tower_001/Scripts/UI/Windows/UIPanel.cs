using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class UIPanel : Panel, IUIItem, IPanel
{
	[Export]
	public bool IsEnabled { get; set; }
	[Export]
	public bool IsVisibleonStartup { get; set; }

	[Export]
	public GlobalEnums.EnumUIPanelType PanelType { get; set; }

	[Export]
	public GlobalEnums.EnumUIPanelParentType PanelParent { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
		this.Visible = visible;
	}

	public void CallEvent()
	{
		throw new NotImplementedException();
	}
}

public class UIPanelState
{
	public HashSet<GlobalEnums.EnumUIPanelType> PanelsToShow { get; }
	public HashSet<GlobalEnums.EnumUIPanelType> RequiredPanels { get; }

	public UIPanelState(HashSet<GlobalEnums.EnumUIPanelType> panelsToShow, HashSet<GlobalEnums.EnumUIPanelType> requiredPanels = null)
	{
		PanelsToShow = panelsToShow;
		RequiredPanels = requiredPanels ?? new HashSet<GlobalEnums.EnumUIPanelType> { GlobalEnums.EnumUIPanelType.LeftPanel, GlobalEnums.EnumUIPanelType.MiddlePanel };
	}
}