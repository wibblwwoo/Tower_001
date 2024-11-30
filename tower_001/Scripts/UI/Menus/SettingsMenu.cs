using Godot;
using System.Collections.Generic;
using static GlobalEnums;

namespace Tower_001.Scripts.UI.Menus
{
    public class SettingsMenu : BaseMenu
    {
        private List<Button> _menuButtons;
        
        // Constants for UI layout
        private const int BUTTON_WIDTH = 154;
        private const int BUTTON_HEIGHT = 31;
        private const int BUTTON_SPACING = 4;
        private const int FONT_SIZE = 16;

        public SettingsMenu() : base("SettingsMenu") { }

        public override void Initialize(Control uiRoot)
        {
            base.Initialize(uiRoot);
            CreateButtons();
        }

        private void CreateButtons()
        {
            ClearButtons();

            var buttonData = new[]
            {
                (text: "Graphics", action: MenuAction.Graphics),
                (text: "Audio", action: MenuAction.Audio),
                (text: "Controls", action: MenuAction.Controls),
                (text: "Back", action: MenuAction.Back)
            };

            _menuButtons = new List<Button>();

            foreach (var (text, action) in buttonData)
            {
                var button = CreateMenuButton(text);
                _buttonContainer.AddChild(button);
                _menuButtons.Add(button);

                // Connect button pressed signal
                button.Pressed += () => RaiseMenuEvent(action);
            }
        }

        private Button CreateMenuButton(string text)
        {
            var button = new Button();
            button.Text = text;
            button.Size = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);
            
            // Style the button
            button.AddThemeFontSizeOverride("font_size", FONT_SIZE);
            button.Alignment = HorizontalAlignment.Center;

            return button;
        }

        private void RaiseMenuEvent(MenuAction action)
        {
            var eventArgs = new MenuEventArgs(action);
            Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
        }
    }
}
