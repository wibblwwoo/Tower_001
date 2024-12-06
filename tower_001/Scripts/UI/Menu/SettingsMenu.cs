using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using static GlobalEnums;


//this controls all the menu items for the main menu

public partial class SettingsMenu : Node, IMenu
{

	public GlobalEnums.PanelPosition MenuDefaultPostion;
	public GlobalEnums.PanelSection MenuDefaultSection;
	private List<UIButton> _MenuItems;

	public string MenuName = "SettingsMenu";

	// Called when the 
	public SettingsMenu()
	{
		MenuDefaultPostion = GlobalEnums.PanelPosition.Middle;
		MenuDefaultSection = GlobalEnums.PanelSection.Left;
		menuParentID = "MainMenu";
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
				(text: "Controls", action: MenuAction.Controls),
				(text: "Graphics", action: MenuAction.Graphics),
				(text: "Audio", action: MenuAction.Audio),
				(text: "Game", action: MenuAction.GameSettings),
				(text: "Tower", action: MenuAction.TowerSettings),
				(text: "Character", action: MenuAction.CharacterSettings),
				
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
		var eventArgs = new MenuEventArgs(action, MenuName, menuParentID);
		Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
	}
	private void HandleMenuButtonPressed(MenuAction action)
	{
		if (action == MenuAction.Back)
		{
			RaiseMenuEvent(action);
			return;
		}

		switch (action)
		{
			case MenuAction.Graphics:
				break;
			case MenuAction.Audio:
				break;
			case MenuAction.GameSettings:
				break;
			case MenuAction.TowerSettings:
				var eventArgs = new MenuEventArgs(action, MenuName, "TowerSettingsMenu");
				Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
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
		foreach (var menuItem in _MenuItems)
		{
			menuItem.Visible = false;
		}
	}

}


