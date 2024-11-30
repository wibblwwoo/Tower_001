using Godot;
using System;

public partial class UIGeneric_Item : Node, IUIItem
{
	[Export]
	public bool IsEnabled { get; set; }
	[Export] 
	public bool IsVisibleonStartup { get; set; }


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
		string _t = base.GetPath();
		
	}
	public void SetVisibility(bool visible)
	{
		//// Check if the node is a CanvasItem to safely set visibility
		//if (base.GetNodeOrNull() is CanvasItem canvasItem)
		//{
		//	canvasItem.Visible = visible;
		//}
		//else
		//{
		//	GD.PrintErr($"Node '{Name}' does not inherit from CanvasItem, visibility cannot be set.");
		//}
	}
	public void CallEvent()
	{
		throw new NotImplementedException();
	}
}
