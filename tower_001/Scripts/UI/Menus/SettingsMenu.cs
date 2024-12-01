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
        private Random _random;

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
            _random = new Random(1221313);
            CreateNavigationButtons();
            PrintNodeTree();
        }

        private void CreateNavigationButtons()
        {
            ClearLeftPanel();

            // Add a fun message to the top section
            var messageLabel = new Label { 
                Text = "Time to tweak some settings!",
                Name = "SettingsMessageLabel",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
            };
            messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            messageLabel.VerticalAlignment = VerticalAlignment.Center;
            AddToLeftPanelTop(messageLabel);

            var menuButtonData = new[]
            {
                (text: "Graphics", action: MenuAction.Graphics),
                (text: "Audio", action: MenuAction.Audio),
                (text: "Controls", action: MenuAction.Controls)
            };

            _menuButtons = new List<Button>();

            // OLD CODE - For rollback if needed
            /*
            var buttonContainer = new VBoxContainer
            {
                Name = "ButtonContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Expand,
                SizeFlagsVertical = Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(120, 0),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            */

            // NEW CODE - Adjusted container settings
            var buttonContainer = new VBoxContainer
            {
                Name = "ButtonContainer",
                SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand,
                SizeFlagsVertical = Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(154, 0),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            AddToLeftPanelMiddle(buttonContainer);

            foreach (var (text, action) in menuButtonData)
            {
                var button = CreateMenuButton(text);
                buttonContainer.AddChild(button);
                _menuButtons.Add(button);
            }

            var backButton = CreateMenuButton("Back");
            backButton.Name = "BackButton";
            AddToLeftPanelBottom(backButton);
            _menuButtons.Add(backButton);
            backButton.Pressed += () => HandleMenuButtonPressed(MenuAction.Back);
        }

        private Button CreateMenuButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Name = $"{text}Button{_random.Next(1, 99)}",
                CustomMinimumSize = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT),
                SizeFlagsHorizontal = Control.SizeFlags.Expand,
                SizeFlagsVertical = Control.SizeFlags.Expand,
                MouseFilter = Control.MouseFilterEnum.Stop
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

            ClearRightPanel();

            switch (action)
            {
                case MenuAction.Graphics:
                    AddToRightPanelMiddle(CreateGraphicsPanel());
                    break;
                case MenuAction.Audio:
                    AddToRightPanelMiddle(CreateAudioPanel());
                    break;
                case MenuAction.Controls:
                    AddToRightPanelMiddle(CreateControlsPanel());
                    break;
            }
        }

        private Control CreateGraphicsPanel()
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
            
            return container;
        }

        private Control CreateAudioPanel()
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
            
            return container;
        }

        private Control CreateControlsPanel()
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
            
            return container;
        }

        private void PrintNodeTree()
        {
            GD.Print("\n=== SettingsMenu Node Tree ===");
            PrintNodeRecursive(_menuRoot, 0);
            GD.Print("=== End Node Tree ===\n");
        }

        private void PrintNodeRecursive(Node node, int depth)
        {
            var indent = new string(' ', depth * 2);
            var info = $"{node.Name} ({node.GetType()})";
            
            if (node is Control control)
            {
                info += $" - Size: {control.Size}, MinSize: {control.CustomMinimumSize}";
                info += $", Flags: H={control.SizeFlagsHorizontal}, V={control.SizeFlagsVertical}";
            }
            
            GD.Print($"{indent}{info}");
            
            foreach (var child in node.GetChildren())
            {
                PrintNodeRecursive(child, depth + 1);
            }
        }

        private void RaiseMenuEvent(MenuAction action)
        {
            GD.Print($"SettingsMenu: Raising menu event {action}");
			var eventArgs = new MenuEventArgs(action);
			Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);

		}
	}
}
