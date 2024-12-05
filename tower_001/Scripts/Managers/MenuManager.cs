using Godot;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;
using static Godot.Control;

namespace Tower_001.Scripts.UI
{
	public partial class MenuManager : Control
	{
		
		private Dictionary<string, IMenu> _menus;


		public void Initialize()
		{
			// Initialize the panel system first
			//Globals.Instance.InitializePanelSystem();

			_menus = new Dictionary<string, IMenu>();
			// Create and initialize all menus
			InitializeMenus();

			// Subscribe to menu events
			SubscribeToEvents();
		}

		private void InitializeMenus()
		{

			_menus.Add("MainMenu", new MainMenu());
			_menus.Add("SettingsMenu", new SettingsMenu());
			_menus.Add("TowerSettingsMenu", new TowerSettingMenu());



			foreach (var menu in _menus.Values.Where(p => p.ShowOnStartup)) {
				menu.Show();
			}
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

			HideMenu(args.MenuParent);
			ShowMenu(args.Menu);
		}

		public void HideMenu(string menuName)
		{
			if (_menus.TryGetValue(menuName, out var menu))
			{
				menu.Hide();
			}
			else
			{
				GD.PrintErr($"Menu not found: {menuName}");
			}
		}
		public void ShowMenu(string menuName)
		{
			if (_menus.TryGetValue(menuName, out var menu))
			{
				menu.Show();
			}
			else
			{
				GD.PrintErr($"Menu not found: {menuName}");
			}
		}

		
	}
}