using Godot;
using System.Collections.Generic;
using static GlobalEnums;
using Tower_001.Scripts.UI.Layout;
using System;

namespace Tower_001.Scripts.UI.Menus
{
    public partial class SettingsMenu : GameLayoutMenu
    {
        private List<Button> _menuButtons;
        private Control _currentSettingsPanel;

        // Constants for UI layout
        private const int BUTTON_WIDTH = 154;
        private const int BUTTON_HEIGHT = 31;
        private const int BUTTON_SPACING = 4;
        private const int FONT_SIZE = 16;

        public SettingsMenu() : base("SettingsMenu") 
        {
        }

        public override void Initialize(Control uiRoot)
        {
            base.Initialize(uiRoot);

            SetupSettingsMenu();
        }

        private void SetupSettingsMenu()
        {
            _menuButtons = new List<Button>();

            // Add message to top panel
            var messageLabel = new Label 
            { 
                Text = "Time to tweak some settings!",
                Name = "SettingsMessageLabel",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
            };
            Globals.Instance.MenuPanelContainer_Left.AddToTop(messageLabel);

            // Add menu buttons
            var menuButtonData = new[]
            {
                (text: "Graphics", action: MenuAction.Graphics),
                (text: "Audio", action: MenuAction.Audio),
                (text: "Controls", action: MenuAction.Controls)
            };

            foreach (var (text, action) in menuButtonData)
            {
                var button = CreateMenuButton(text);

				Globals.Instance.MenuPanelContainer_Left.AddButton(button);
                _menuButtons.Add(button);
            }
			
			// Add back button to bottom
			var backButton = CreateMenuButton("Back");
            backButton.Name = "BackButton";
            Globals.Instance.MenuPanelContainer_Left.AddToBottom(backButton);
            _menuButtons.Add(backButton);
            backButton.Pressed += () => HandleMenuButtonPressed(MenuAction.Back);
        }

        private Button CreateMenuButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Name = $"{text}Button",
            };

            button.AddThemeFontSizeOverride("font_size", FONT_SIZE);
            button.Alignment = HorizontalAlignment.Center;
            button.Pressed += () => HandleMenuButtonPressed(GetActionForButton(text));

            return button;
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

        private void HandleMenuButtonPressed(MenuAction action)
        {
            if (action == MenuAction.Back)
            {
                RaiseMenuEvent(action);
                return;
            }

            Globals.Instance.MenuPanelContainer_Right.ClearMiddle();
            Globals.Instance.MenuPanelContainer_Right.ClearTop();
            Globals.Instance.MenuPanelContainer_Right.ClearBottom();

            switch (action)
            {
                case MenuAction.Graphics:
                    var graphicsPanel = CreateGraphicsPanel();
                    Globals.Instance.MenuPanelContainer_Right.AddToMiddle(graphicsPanel);
                    break;
                case MenuAction.Audio:
                    var audioPanel = CreateAudioPanel();
                    Globals.Instance.MenuPanelContainer_Right.AddToMiddle(audioPanel);
                    break;
                case MenuAction.Controls:
                    var controlsPanel = CreateControlsPanel();
                    Globals.Instance.MenuPanelContainer_Right.AddToMiddle(controlsPanel);
                    break;
            }
        }

        private void RaiseMenuEvent(MenuAction action)
        {
            var eventArgs = new MenuEventArgs(action);
            Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
            if (action == MenuAction.Back)
            {
                this.Hide();
            }
        }

        private Control CreateGraphicsPanel()
        {
            var container = new VBoxContainer();
            
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
            
            return container;
        }

        private Control CreateAudioPanel()
        {
            var container = new VBoxContainer();
            
            var masterLabel = new Label { 
                Text = "Master Volume",
                Name = "MasterVolumeLabel"
            };
            var masterSlider = new HSlider { 
                Name = "MasterVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 80
            };
            
            var musicLabel = new Label { 
                Text = "Music Volume",
                Name = "MusicVolumeLabel"
            };
            var musicSlider = new HSlider { 
                Name = "MusicVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 70
            };
            
            var sfxLabel = new Label { 
                Text = "SFX Volume",
                Name = "SFXVolumeLabel"
            };
            var sfxSlider = new HSlider { 
                Name = "SFXVolumeSlider",
                MinValue = 0,
                MaxValue = 100,
                Value = 90
            };
            
            container.AddChild(masterLabel);
            container.AddChild(masterSlider);
            container.AddChild(musicLabel);
            container.AddChild(musicSlider);
            container.AddChild(sfxLabel);
            container.AddChild(sfxSlider);
            
            return container;
        }

        private Control CreateControlsPanel()
        {
            var container = new VBoxContainer();
            
            var mouseSensLabel = new Label { 
                Text = "Mouse Sensitivity",
                Name = "MouseSensitivityLabel"
            };
            var mouseSensSlider = new HSlider { 
                Name = "MouseSensitivitySlider",
                MinValue = 1,
                MaxValue = 10,
                Value = 5
            };
            
            var invertYCheck = new CheckBox { 
                Text = "Invert Y-Axis",
                Name = "InvertYCheck"
            };
            
            container.AddChild(mouseSensLabel);
            container.AddChild(mouseSensSlider);
            container.AddChild(invertYCheck);
            
            return container;
        }

		public override void Show()
		{
			_menuRoot.Show();
			_menuRoot.MoveToFront(); // Ensure this menu is on top
            Globals.Instance.MenuPanelContainer_Left.ParentPanel.Visible = true;
			Globals.Instance.MenuPanelContainer_Right.ParentPanel.Visible = true;
		}
		public override void Hide()
		{
			_menuRoot.Hide();
			Globals.Instance.MenuPanelContainer_Left.ParentPanel.Visible = false;
			Globals.Instance.MenuPanelContainer_Right.ParentPanel.Visible = false;
		}


	}
}
