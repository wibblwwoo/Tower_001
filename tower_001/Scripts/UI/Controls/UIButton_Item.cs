using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UIButton_Item : Button, IUIItem
{
	[Export]
	public GlobalEnums.EnumUIPanelParentType ShowButtonByParent { get; set; }

	[Export]
	public GlobalEnums.EnumUIPanelParentType ShowPanelByClick { get; set; }
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

		Globals.Instance.gameMangers.Events.RaiseHandleUIButton_Click(ShowPanelByClick);
		//if (DisableOnClick)
		//{
		//	base.Disabled = true;
		//}
		//if (EnableButtons.Count > 0)
		//{
		//	Globals.Instance.gameMangers.Events.RaiseHandleUiButton_Visibility(EnableButtons.ToList(),true);
		//}
		//if (HideButtons != null)
		//{
		//	Globals.Instance.gameMangers.Events.RaiseHandleUiButton_Visibility(HideButtons.ToList(), false);
		//}
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
