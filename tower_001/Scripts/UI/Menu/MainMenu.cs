using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;


//this controls all the menu items for the main menu

public partial class MainMenu : Node, IMenu 
{

	public GlobalEnums.PanelPosition MenuDefaultPostion;
	public GlobalEnums.PanelSection MenuDefaultSection;
	private List<UIButton> _MenuItems;

	public string MenuName = "MainMenu";

	// Called when the 
	public MainMenu()
	{
		MenuDefaultPostion = GlobalEnums.PanelPosition.Middle;
		MenuDefaultSection = GlobalEnums.PanelSection.Left;
		menuParentID = "MainMenu";
		ShowOnStartup = true;
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
				(text: "New Game", action: MenuAction.NewGame),
				(text: "Settings", action: MenuAction.Settings),
				(text: "Load Game", action: MenuAction.LoadGame),
				(text: "Save Game", action: MenuAction.SaveGame),
				(text: "Exit", action: MenuAction.Exit)
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
		button.PanelLocation = PanelPosition.Middle;
		button.PanelSection = PanelSection.Left;
		button.ControlType = UIControlType.Control;
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
			RaiseMenuEvent(action);
			return;
		}
		switch (action)
		{
			case MenuAction.NewGame:
				break;
			case MenuAction.Settings:
				var eventArgs = new MenuEventArgs(action, MenuName, "SettingsMenu");
				Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
				break;
			case MenuAction.LoadGame:
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


