using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
using static Godot.Label;
using Tower_001.Scripts.UI.Menus;

/// <summary>
/// Core UI manager for the idle game.
/// Handles the display and interaction with game elements in a streamlined manner.
/// </summary>
public partial class UIManager : BaseManager
{
    // The main UI scene root
    private Control _uiRoot;
    private Dictionary<Type, BaseMenu> _menus;
    private BaseMenu _currentMenu;

    // OLD CODE - For rollback if needed
    /*
    public UIManager()
    {
        RegisterEventHandlers();
    }

    public override void Setup()
    {
        base.Setup();
        
        // Get the root node from game manager
        _uiRoot = (Control)Globals.Instance.RootNode;
        if (_uiRoot == null)
        {
            GD.PrintErr("UIManager: Root node not found!");
            return;
        }

        InitializeMenus();
        ShowMenu<MainMenu>();
    }

    protected override void RegisterEventHandlers()
    {
        if (Globals.Instance?.gameMangers?.Events != null)
        {
            Globals.Instance.gameMangers.Events.AddHandler<MenuEventArgs>(EventType.MenuAction, OnMenuAction);
        }
    }
    */

    public UIManager()
    {
        // Event registration moved to Setup()
    }

    // OLD CODE - For rollback if needed
    /*
    public override void Setup()
    {
        base.Setup();
        
        // Get the root node from game manager
        _uiRoot = (Control)Globals.Instance.RootNode;
        if (_uiRoot == null)
        {
            GD.PrintErr("UIManager: Root node not found!");
            return;
        }

        InitializeMenus();
        ShowMenu<MainMenu>();
    }

    protected override void RegisterEventHandlers()
    {
        if (Globals.Instance?.gameMangers?.Events != null)
        {
            Globals.Instance.gameMangers.Events.AddHandler<MenuEventArgs>(EventType.MenuAction, OnMenuAction);
        }
    }
    */

    // NEW CODE - Current implementation
    public override void Setup()
    {
        base.Setup();
        
        // Get the root node from game manager
        _uiRoot = (Control)Globals.Instance.RootNode;
        if (_uiRoot == null)
        {
            GD.PrintErr("UIManager: Root node not found!");
            return;
        }

        InitializeMenus();
        
        // Register event handlers after initialization
        RegisterEventHandlers();
        
        // Show main menu and verify it worked
        ShowMenu<MainMenu>();
        if (_currentMenu == null || _currentMenu.GetType() != typeof(MainMenu))
        {
            GD.PrintErr("UIManager: Failed to show main menu!");
        }
    }

    protected override void RegisterEventHandlers()
    {
        // Move event registration to Setup() where we know the event system exists
        if (Globals.Instance?.gameMangers?.Events == null)
        {
            GD.PrintErr("UIManager: Events system not available for registration!");
            return;
        }
        
        Globals.Instance.gameMangers.Events.AddHandler<MenuEventArgs>(EventType.MenuAction, OnMenuAction);
        GD.Print("UIManager: Successfully registered menu event handlers");
    }

    private void InitializeMenus()
    {
        _menus = new Dictionary<Type, BaseMenu>
        {
            { typeof(MainMenu), new MainMenu() },
            { typeof(SettingsMenu), new SettingsMenu() }
        };

        foreach (var menu in _menus.Values)
        {
            menu.Initialize(_uiRoot);
            menu.Hide();
        }
    }

    private void ShowMenu<T>() where T : BaseMenu
    {
        if (_currentMenu != null)
        {
            _currentMenu.Hide();
        }

        if (_menus.TryGetValue(typeof(T), out BaseMenu menu))
        {
            _currentMenu = menu;
            _currentMenu.Show();
        }
    }


    private void OnMenuAction(EventArgs args)
    {
        if (args is MenuEventArgs menuArgs)
        {
            switch (menuArgs.Action)
            {
                case MenuAction.NewGame:
                    HandleNewGame();
                    break;
                case MenuAction.Settings:
                    ShowMenu<SettingsMenu>();
                    break;
                case MenuAction.LoadGame:
                    HandleLoadGame();
                    break;
                case MenuAction.SaveGame:
                    HandleSaveGame();
                    break;
                case MenuAction.Exit:
                    HandleExit();
                    break;
                case MenuAction.Back:
                    ShowMenu<MainMenu>();
                    break;
            }
        }
    }

    private void HandleNewGame()
    {
        // TODO: Implement new game logic
    }

    private void HandleSettings()
    {
        // Already handled by ShowMenu<SettingsMenu>()
    }

    private void HandleLoadGame()
    {
        // TODO: Implement load game logic
    }

    private void HandleSaveGame()
    {
        // TODO: Implement save game logic
    }

    private void HandleExit()
    {
        // TODO: Add cleanup logic if needed
        // 1. Save settings
        // 2. Clean up resources
        //(GetNode("/root") as SceneTree).Quit();
    }
}
