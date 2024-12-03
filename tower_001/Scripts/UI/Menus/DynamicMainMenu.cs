using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
using static Godot.Control;

public partial class DynamicMainMenu : DynamicGameLayoutMenu
{
	private List<Button> _menuButtons;
	private const int BUTTON_WIDTH = 154;
	private const int BUTTON_HEIGHT = 31;
	private const int BUTTON_SPACING = 4;
	private const int FONT_SIZE = 16;

	public DynamicMainMenu() : base("DynamicMainMenu")
	{
		_menuButtons = new List<Button>();
	}

	protected override void InitializePanels()
	{
		GD.Print("Initializing DynamicMainMenu panels...");

		// Create the main menu panel using base class functionality
		var mainPanel = CreatePanel("MainMenu");
		if (mainPanel == null)
		{
			GD.PrintErr("Failed to create main menu panel!");
			return;
		}

		GD.Print("Setting up main menu content...");
		CreateButtons(mainPanel);
	}

	private void CreateButtons(BaseDynamicPanel panel)
	{
		var buttonData = new[]
		{
			(text: "New Game", action: MenuAction.NewGame),
			(text: "Settings", action: MenuAction.Settings),
			(text: "Load Game", action: MenuAction.LoadGame),
			(text: "Save Game", action: MenuAction.SaveGame),
			(text: "Exit", action: MenuAction.Exit)
		};

		var buttonContainer = new VBoxContainer
		{
			Name = "ButtonContainer",
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
			SizeFlagsVertical = SizeFlags.ShrinkCenter,
			CustomMinimumSize = new Vector2(0, 0)
		};

		foreach (var (text, action) in buttonData)
		{
			var button = CreateMenuButton(text);
			buttonContainer.AddChild(button);
			button.AddThemeConstantOverride("margin_bottom", 10);
			_menuButtons.Add(button);
			button.Pressed += () => RaiseMenuEvent(action);
		}

		panel.AddContent("main", buttonContainer);
	}

	private Button CreateMenuButton(string text)
	{
		var button = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT)
		};

		button.AddThemeFontSizeOverride("font_size", FONT_SIZE);
		button.Alignment = HorizontalAlignment.Center;

		return button;
	}

	private void RaiseMenuEvent(MenuAction action)
	{
		var eventArgs = new MenuEventArgs(action);
		Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
		Hide();
	}

	public override void Show()
	{
		base.Show();
		foreach (var button in _menuButtons)
		{
			button.Visible = true;
		}
	}

	public override void Hide()
	{
		foreach (var button in _menuButtons)
		{
			button.Visible = false;
		}
		base.Hide();
	}
}