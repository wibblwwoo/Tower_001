using Godot;
using System.Collections.Generic;
using Tower_001.Scripts.UI.Layout;
using static GlobalEnums;

namespace Tower_001.Scripts.UI.Menus
{
    public class MainMenu : CenteredMenu
    {
        private List<Button> _menuButtons;

		// Constants for UI layout
		private const int BUTTON_WIDTH = 154;
        private const int BUTTON_HEIGHT = 31;
        private const int BUTTON_SPACING = 4;
        private const int FONT_SIZE = 16;

        public MainMenu() : base("MainMenu") { }

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
                (text: "New Game", action: MenuAction.NewGame),
                (text: "Settings", action: MenuAction.Settings),
                (text: "Load Game", action: MenuAction.LoadGame),
                (text: "Save Game", action: MenuAction.SaveGame),
                (text: "Exit", action: MenuAction.Exit)
            };

            _menuButtons = new List<Button>();

            foreach (var (text, action) in buttonData)
            {
                var button = CreateMenuButton(text);
                AddButton(button);  
                _menuButtons.Add(button);

                // Connect button pressed signal
                button.Pressed += () => RaiseMenuEvent(action);
            }
        }

        private Button CreateMenuButton(string text)
        {
            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT)
            };

            // Style the button
            button.AddThemeFontSizeOverride("font_size", FONT_SIZE);
            button.Alignment = HorizontalAlignment.Center;

            return button;
        }

        private void RaiseMenuEvent(MenuAction action)
        {
            var eventArgs = new MenuEventArgs(action);
            Globals.Instance.gameMangers.Events.RaiseEvent(EventType.MenuAction, eventArgs);
            this.Hide();
        }

		public override void Show()
		{
			base.Show();
			_menuRoot.Show();
			_menuRoot.MoveToFront(); // Ensure this menu is on top
			_container.Visible = true;
		}
		public override void Hide()
		{
			_menuRoot.Hide();
            _container.Visible = false;
		}

	}
}
