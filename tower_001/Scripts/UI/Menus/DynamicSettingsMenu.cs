using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
using static Godot.Control;

// Example implementation of Settings Menu using dynamic panels
public partial class DynamicSettingsMenu : DynamicGameLayoutMenu
{
	private BaseDynamicPanel _settingsPanel;
	private List<Button> _menuButtons;

	public DynamicSettingsMenu() : base("DynamicSettingsMenu")
	{
		_menuButtons = new List<Button>();
	}

	protected override void InitializePanels()
	{
		GD.Print("Initializing DynamicSettingsMenu panels...");

		_settingsPanel = CreatePanel("SettingsMenu");
		if (_settingsPanel == null)
		{
			GD.PrintErr("Failed to create settings panel!");
			return;
		}

		GD.Print("Setting up settings menu content...");
		SetupSettingsMenu();
	}

	private void SetupSettingsMenu()
	{
		//// Add message to top section
		//var messageLabel = new Label
		//{
		//	Text = "Time to tweak some settings!",
		//	HorizontalAlignment = HorizontalAlignment.Center,
		//	VerticalAlignment = VerticalAlignment.Center,
		//	SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
		//};

		//try
		//{
		//	_settingsPanel.AddContent("top", messageLabel);
		//	GD.Print("Added top message label");
		//}
		//catch (Exception e)
		//{
		//	GD.PrintErr($"Failed to add message label: {e.Message}");
		//}

		// Create and add menu buttons
		SetupMenuButtons();
	}

	public override void Show()
	{
		// Call base show first to handle menu root visibility
		base.Show();

		// Show panel (which will handle its own content visibility)
		_settingsPanel?.Show();

		// Additional button visibility if needed
		foreach (var button in _menuButtons)
		{
			button.Visible = true;
		}
	}

	public override void Hide()
	{
		// Hide panel first
		_settingsPanel?.Hide();

		// Hide buttons
		foreach (var button in _menuButtons)
		{
			button.Visible = false;
		}

		// Call base hide last to handle menu root visibility
		base.Hide();
	}
	private void SetupMenuButtons()
	{
		_menuButtons = new List<Button>();

		var buttonData = new[]
		{
				("Graphics", MenuAction.Graphics),
				("Audio", MenuAction.Audio),
				("Controls", MenuAction.Controls)
			};

		foreach (var (text, action) in buttonData)
		{
			var button = CreateMenuButton(text);
			//_settingsPanel.AddContent("left", button);
			Globals.Instance.MenuPanelContainer_Left.AddButton(button);
			_menuButtons.Add(button);
			button.Pressed += () => HandleMenuButtonPressed(action);
		}

		// Add back button
		var backButton = CreateMenuButton("Back");
		//_settingsPanel.AddContent("bottom", backButton);
		Globals.Instance.MenuPanelContainer_Left.AddToBottom(backButton);
		_menuButtons.Add(backButton);
		backButton.Pressed += () => HandleMenuButtonPressed(MenuAction.Back);
	}

	private Button CreateMenuButton(string text)
	{
		return new Button
		{
			Text = text,
			Name = $"{text}Button",
			CustomMinimumSize = new Vector2(154, 31),
		};
	}

	private void HandleMenuButtonPressed(MenuAction action)
	{
		if (action == MenuAction.Back)
		{
			RaiseMenuEvent(action);
			return;
		}

		// Clear existing content
		_settingsPanel.ClearContent();

		// Add new content based on action
		switch (action)
		{
			case MenuAction.Graphics:
				CreateGraphicsPanel();
				break;
			case MenuAction.Audio:
				CreateAudioPanel();
				break;
			case MenuAction.Controls:
				CreateControlsPanel();
				break;
		}
	}

	private void RaiseMenuEvent(MenuAction action)
	{
		var eventArgs = new MenuEventArgs(action);
		Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
		if (action == MenuAction.Back)
		{
			Hide();
		}
	}

	private void CreateGraphicsPanel()
	{
		var container = CreateSettingsContainer();
		AddSettingControl(container, "Resolution", CreateResolutionControl());
		AddSettingControl(container, "Fullscreen", new CheckBox { Text = "Fullscreen" });
		AddSettingControl(container, "VSync", new CheckBox { Text = "VSync" });
		_settingsPanel.AddContent("right", container);
	}

	private void CreateAudioPanel()
	{
		var container = CreateSettingsContainer();
		AddSettingControl(container, "Master Volume", CreateVolumeSlider());
		AddSettingControl(container, "Music Volume", CreateVolumeSlider());
		AddSettingControl(container, "SFX Volume", CreateVolumeSlider());
		_settingsPanel.AddContent("right", container);
	}

	private void CreateControlsPanel()
	{
		var container = CreateSettingsContainer();
		AddSettingControl(container, "Mouse Sensitivity", CreateSensitivitySlider());
		AddSettingControl(container, "Invert Y-Axis", new CheckBox { Text = "Invert Y-Axis" });
		_settingsPanel.AddContent("right", container);
	}

	private VBoxContainer CreateSettingsContainer()
	{
		return new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
	}

	private void AddSettingControl(VBoxContainer container, string labelText, Control control)
	{
		container.AddChild(new Label { Text = labelText });
		container.AddChild(control);
	}

	private OptionButton CreateResolutionControl()
	{
		var option = new OptionButton();
		option.AddItem("1920x1080");
		option.AddItem("1280x720");
		return option;
	}

	private HSlider CreateVolumeSlider()
	{
		return new HSlider
		{
			MinValue = 0,
			MaxValue = 100,
			Value = 80,
			SizeFlagsHorizontal = SizeFlags.Fill
		};
	}

	private HSlider CreateSensitivitySlider()
	{
		return new HSlider
		{
			MinValue = 1,
			MaxValue = 10,
			Value = 5,
			SizeFlagsHorizontal = SizeFlags.Fill
		};
	}
}
