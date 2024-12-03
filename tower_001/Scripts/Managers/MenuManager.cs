using Godot;
using System.Collections.Generic;
using static GlobalEnums;
using static Godot.Control;

namespace Tower_001.Scripts.UI
{
	public partial class MenuManager : Control
	{
		private readonly Dictionary<string, DynamicGameLayoutMenu> _menus;
		private readonly Control _uiRoot;
		private DynamicGameLayoutMenu _currentMenu;

		public MenuManager(Control uiRoot)
		{
			_uiRoot = uiRoot;
			_menus = new Dictionary<string, DynamicGameLayoutMenu>();
		}

		public void Initialize()
		{
			// Initialize the panel system first
			//Globals.Instance.InitializePanelSystem();


			// Create and initialize all menus
			InitializeMenus();

			// Subscribe to menu events
			SubscribeToEvents();
		}

		private void InitializeMenus()
		{
			// Create menu instances
			CreateMenu(new DynamicMainMenu());
			CreateMenu(new DynamicSettingsMenu());
			// Add other menus as needed


			

			// Hide all menus initially
			HideAllMenus();
		}

		private void CreateMenu(DynamicGameLayoutMenu menu)
		{
			menu.Initialize(_uiRoot);
			_menus[menu.GetType().Name] = menu;
		}

		private void SubscribeToEvents()
		{
			Globals.Instance.gameMangers.Events.AddHandler<MenuEventArgs>(
				EventType.MenuAction,
				HandleMenuAction
			);
		}

		private void HandleMenuAction(MenuEventArgs args)
		{
			switch (args.Action)
			{
				case MenuAction.MainMenu:
					ShowMenu<DynamicMainMenu>();
					break;

				case MenuAction.Settings:
					ShowMenu<DynamicSettingsMenu>();
					break;

				case MenuAction.Back:
					HandleBackAction();
					break;

				case MenuAction.Exit:
					HandleExitAction();
					break;

					// Add other menu actions as needed
			}
		}

		private void ShowMenu<T>() where T : DynamicGameLayoutMenu
		{
			var menuName = typeof(T).Name;

			if (_menus.TryGetValue(menuName, out var menu))
			{
				// Hide current menu if exists
				_currentMenu?.Hide();

				// Show new menu
				menu.Show();
				_currentMenu = menu;
			}
			else
			{
				GD.PrintErr($"Menu not found: {menuName}");
			}
		}

		private void HandleBackAction()
		{
			ShowMenu<DynamicMainMenu>();
		}

		private void HandleExitAction()
		{
			Globals.Instance.gameMangers.Events.RaiseEvent(
				EventType.GameAction,
				new MenuEventArgs(MenuAction.Exit)
			);
		}

		public void ShowMenu(string menuName)
		{
			if (_menus.TryGetValue(menuName, out var menu))
			{
				_currentMenu?.Hide();
				menu.Show();
				_currentMenu = menu;
			}
			else
			{
				GD.PrintErr($"Menu not found: {menuName}");
			}
		}

		public void HideCurrentMenu()
		{
			_currentMenu?.Hide();
			_currentMenu = null;
		}

		public void HideAllMenus()
		{
			foreach (var menu in _menus.Values)
			{
				menu.Hide();
			}
			_currentMenu = null;
		}

		public DynamicGameLayoutMenu GetMenu(string menuName)
		{
			return _menus.TryGetValue(menuName, out var menu) ? menu : null;
		}

		public void Cleanup()
		{
			// Unsubscribe from events
			Globals.Instance.gameMangers.Events.RemoveHandler<MenuEventArgs>(
				EventType.MenuAction,
				HandleMenuAction
			);

			// Cleanup all menus
			foreach (var menu in _menus.Values)
			{
				menu.Cleanup();
			}
			_menus.Clear();
			_currentMenu = null;
		}
	}
}