using Godot;
using System;

public partial class UIContainer : PanelContainer, IUIItem
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
