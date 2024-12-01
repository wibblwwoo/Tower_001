using Godot;
using System.Collections.Generic;
using static GlobalEnums;
using Tower_001.Scripts.UI.Layout;
using System.Diagnostics;
using System;
using System.Security.Cryptography;

namespace Tower_001.Scripts.UI.Menus
{
    public partial class SettingsMenu : BaseMenu
    {
        private List<Button> _menuButtons;
        private Control _currentSettingsPanel;
        private ScalableMenuLayout _layout;
		private Random _random;


		// Constants for UI layout
		private const int BUTTON_WIDTH = 154;
        private const int BUTTON_HEIGHT = 31;
        private const int BUTTON_SPACING = 4;
        private const int FONT_SIZE = 16;

		
		public SettingsMenu() : base("SettingsMenu") 
        {
            // Create the layout immediately to avoid null reference
            _layout = new ScalableMenuLayout();
		}

        public override void Initialize(Control uiRoot)
        {

			GD.Print($"Initialize called from:\n{System.Environment.StackTrace}");

			base.Initialize(uiRoot);

			_random = new Random(1221313);

            


			GD.Print($"Settings Menu Process Mode: {uiRoot}");

			// Add the layout to the container
			_container.AddChild(_layout);
            
            // Create navigation buttons in left panel
            CreateNavigationButtons();


            foreach (var button in _menuButtons)
            {
                GD.Print($"Button {button.Text} has {button.GetSignalConnectionList("pressed").Count} pressed connections");
            }


            // Hide the layout initially
            _layout.Hide();
		}

        public override void Show()
        {
            base.Show();
            if (_layout != null)
            {

				_layout.Show();
            }
        }
		private void PrintNodeHierarchy(Node node, string indent)
		{
			GD.Print($"{indent}{node.Name} ({node.GetType()})");
			foreach (var child in node.GetChildren())
			{
				PrintNodeHierarchy(child, indent + "  ");
			}
		}
		public override void Hide()
        {
            base.Hide();
            if (_layout != null)
            {
                _layout.Hide();
            }
        }

		private void CreateNavigationButtons()
        {
            GD.Print("\n=== Creating Navigation Buttons ===");
            _layout.ClearLeftPanel();

            // Add a fun message to the top section
            GD.Print("Creating top section message...");
            var messageLabel = new Label { 
                Text = "Time to tweak some settings! ",
                Name = "SettingsMessageLabel"
            };
            messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            messageLabel.VerticalAlignment = VerticalAlignment.Center;
            _layout.AddToLeftPanelTop(messageLabel);

			// Create menu buttons for the middle section
			GD.Print("Creating middle section buttons...");
            var menuButtonData = new[]
            {
                (text: "Graphics", action: MenuAction.Graphics),
                (text: "Audio", action: MenuAction.Audio),
                (text: "Controls", action: MenuAction.Controls)
            };

            _menuButtons = new List<Button>();

			var buttonContainer = new VBoxContainer
			{
				Name = "ButtonContainer",
				// Make it use expand but not fill horizontally
				SizeFlagsHorizontal = Control.SizeFlags.Expand,
				SizeFlagsVertical = Control.SizeFlags.Fill,
				// Add some spacing between buttons
				CustomMinimumSize = new Vector2(120, 0), // Set a minimum width for the container
				MouseFilter = Control.MouseFilterEnum.Ignore  // Add this to let input pass through
			};
			_layout.AddToLeftPanelMiddle(buttonContainer);

			// Create and add back button directly
			GD.Print("Creating bottom section with back button...");

            // Add menu buttons to middle section

            foreach (var (text, action) in menuButtonData)
            {
                GD.Print($"Creating button: {text}");
                var button = CreateMenuButton(text);
                buttonContainer.AddChild(button);

                _menuButtons.Add(button);
            }

			var backButton = CreateMenuButton("Back");
			backButton.Name = "BackButton";

			// Only add it to the bottom center, not the button container
			_layout.AddToLeftPanelBottom(backButton);
			//buttonContainer.AddChild(backButton);
			_menuButtons.Add(backButton);
			backButton.Pressed += () => HandleMenuButtonPressed(MenuAction.Back);

			_random = new Random(155513);


			GD.Print($"Total buttons created: {_menuButtons.Count}");
            GD.Print("=== Navigation Buttons Creation Complete ===\n");
        }
		private MenuAction GetActionForButton(string text)
		{
			return text switch
			{
				"Graphics" => MenuAction.Graphics,
				"Audio" => MenuAction.Audio,
				"Controls" => MenuAction.Controls,
				"Back" => MenuAction.Back,
				_ => throw new ArgumentException($"Unknown button text: {text}")
			};
		}

		private Button CreateMenuButton(string text)
		{
			var button = new Button
			{
				Text = text,
				Name = $"{text}Button{_random.Next(1, 99)}",
				CustomMinimumSize = new Vector2(140, 30),  // Use final size directly
				SizeFlagsHorizontal = Control.SizeFlags.Expand,  // Use final flags directly
				SizeFlagsVertical = Control.SizeFlags.Expand,
				MouseFilter = Control.MouseFilterEnum.Stop
			};

			// Add all the event handlers here
			button.GuiInput += (InputEvent @event) =>
			{
				//GD.Print($"GUI Input received on {text} button: {@event.GetType()}");
			};

			button.MouseEntered += () =>
			{
				//GD.Print($"Mouse entered {text} button");
			};

			button.Pressed += () =>
			{
				GD.Print($"Button {text} was pressed!");
				HandleMenuButtonPressed(GetActionForButton(text));  // Add this line
			};

			button.AddThemeConstantOverride("font_size", FONT_SIZE);

			GD.Print($"Created button '{text}' - Name: {button.Name}, Size: {button.CustomMinimumSize}, Flags: {button.SizeFlagsHorizontal}");

			return button;
		}

	
		private void HandleMenuButtonPressed(MenuAction action)
        {
            if (action == MenuAction.Back)
            {
                RaiseMenuEvent(action);
                return;
            }

            // Clear the right panel
            _layout.ClearRightPanel();

            // Create the appropriate settings panel based on the action
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

        private void CreateGraphicsPanel()
        {
            var container = new VBoxContainer();
            
            // Add graphics settings controls
            var resolutionLabel = new Label { 
                Text = "Resolution",
                Name = "ResolutionLabel"
            };
            var resolutionOptions = new OptionButton
            {
                Name = "ResolutionOptions"
            };
            resolutionOptions.AddItem("1920x1080");
            resolutionOptions.AddItem("1280x720");
            
            var fullscreenCheck = new CheckBox { 
                Text = "Fullscreen",
                Name = "FullscreenCheck"
            };
            var vsyncCheck = new CheckBox { 
                Text = "VSync",
                Name = "VSyncCheck"
            };
            
            container.AddChild(resolutionLabel);
            container.AddChild(resolutionOptions);
            container.AddChild(fullscreenCheck);
            container.AddChild(vsyncCheck);
            
            _layout.AddToRightPanelMiddle(container);
        }

        private void CreateAudioPanel()
        {
            var container = new VBoxContainer();
            
            // Add audio settings controls
            var masterLabel = new Label { 
                Text = "Master Volume",
                Name = "MasterVolumeLabel"
            };
            var masterSlider = new HSlider
            {
                Name = "MasterVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 100
            };
            
            var musicLabel = new Label { 
                Text = "Music Volume",
                Name = "MusicVolumeLabel"
            };
            var musicSlider = new HSlider
            {
                Name = "MusicVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 100
            };
            
            var sfxLabel = new Label { 
                Text = "SFX Volume",
                Name = "SFXVolumeLabel"
            };
            var sfxSlider = new HSlider
            {
                Name = "SFXVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 100
            };
            
            container.AddChild(masterLabel);
            container.AddChild(masterSlider);
            container.AddChild(musicLabel);
            container.AddChild(musicSlider);
            container.AddChild(sfxLabel);
            container.AddChild(sfxSlider);
            
            _layout.AddToRightPanelMiddle(container);
        }

        private void CreateControlsPanel()
        {
            var container = new VBoxContainer();
            
            // Add controls settings
            var mouseSensLabel = new Label { 
                Text = "Mouse Sensitivity",
                Name = "MouseSensitivityLabel"
            };
            var mouseSensSlider = new HSlider
            {
                Name = "MouseSensitivitySlider",
                MinValue = 1,
                MaxValue = 10,
                Value = 5
            };
            
            var invertYCheck = new CheckBox { 
                Text = "Invert Y-Axis",
                Name = "InvertYCheck"
            };
            var showTooltipsCheck = new CheckBox { 
                Text = "Show Tooltips",
                Name = "ShowTooltipsCheck"
            };
            
            container.AddChild(mouseSensLabel);
            container.AddChild(mouseSensSlider);
            container.AddChild(invertYCheck);
            container.AddChild(showTooltipsCheck);
            
            _layout.AddToRightPanelMiddle(container);
        }

        private void RaiseMenuEvent(MenuAction action)
        {
            var eventArgs = new MenuEventArgs(action);
            Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
        }
    }
}
