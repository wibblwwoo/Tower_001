using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;


//this controls all the menu items for the main menu

public partial class TowerSettingMenu : Node, IMenu
{

	public GlobalEnums.PanelPosition MenuDefaultPostion;
	public GlobalEnums.PanelSection MenuDefaultSection;
	private List<UIButton> _MenuItems;

	public string MenuName = "TowerSettingsMenu";

	// Called when the 
	public TowerSettingMenu()
	{
		MenuDefaultPostion = GlobalEnums.PanelPosition.Middle;
		MenuDefaultSection = GlobalEnums.PanelSection.Left;
		menuParentID = "SettingsMenu";
		ShowOnStartup = false;
	}

	public string menuParentID { get; set; }
	public bool ShowOnStartup { get; set; }


	public void Setup()
	{
		//just incase we need to do anthing extra
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CreateButtons()
	{


		var buttonData = new[]
		{
				(text: "Floor", action: MenuAction.TowerSettings_Floor),
				(text: "Rooms", action: MenuAction.TowerSettings_Room),
				(text: "Events", action: MenuAction.TowerSettings_Events),
				(text: "Style", action: MenuAction.TowerSettings_Style),
				(text: "Tower", action: MenuAction.TowerSettings_Tower),
				(text: "Back", action: MenuAction.Back),
			};

		_MenuItems = new List<UIButton>();

		foreach (var (text, action) in buttonData)
		{
			var button = CreateMenuButton(text);
			_MenuItems.Add(button);
			button.RegisterControl(this);

			// Connect button pressed signal
			button.Pressed += () => HandleMenuButtonPressed(action);

		}
	}

	private UIButton CreateMenuButton(string text)
	{
		var button = new UIButton();
		button.Text = text;
		button.Alignment = HorizontalAlignment.Center;
		button.menuParentID = MenuName;

		if (text == "Back")
		{

			button.PanelLocation = PanelPosition.Bottom;
			button.PanelSection = PanelSection.Left;
			button.ControlType = UIControlType.Control;
			button.SetAnchorsPreset(Control.LayoutPreset.FullRect);  // Set anchors first
			button.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;  // Changed to Fill | Expand
			button.SizeFlagsVertical = Control.SizeFlags.Fill | Control.SizeFlags.Expand;    // Changed to Fill | Expand
			button.GrowHorizontal = Control.GrowDirection.Both;  // Add these grow directions
			button.GrowVertical = Control.GrowDirection.Both;    // to match TSCN


		}
		else
		{
			button.PanelLocation = PanelPosition.Middle;
			button.PanelSection = PanelSection.Left;
			button.ControlType = UIControlType.Control;

		}
		return button;
	}

	private void RaiseMenuEvent(MenuAction action)
	{
		var eventArgs = new MenuEventArgs(action, menuParentID, MenuName);
		Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
	}
	private void HandleMenuButtonPressed(MenuAction action)
	{
		if (action == MenuAction.Back)
		{
			var eventArgs = new MenuEventArgs(action, "TowerSettingsMenu", menuParentID);
			Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
			return;
		}

		switch (action)
		{
			case MenuAction.TowerSettings:
				break;
			case MenuAction.TowerSettings_Room:
				break;
			case MenuAction.TowerSettings_Events:
				break;
		}
	}


	public void Show()
	{
		if (_MenuItems == null)
		{

			CreateButtons();
		}
		foreach (var menuItem in _MenuItems)
		{
			menuItem.Visible = true;
		}
	}
	public void Hide()
	{
		//_menuRoot.Hide();
		//_container.Visible = false;
		foreach (var menuItem in _MenuItems)
		{
			menuItem.Visible = false;
		}
	}

}


