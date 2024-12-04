using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;
using static Godot.Label;
using Tower_001.Scripts.UI;
using System.Linq;

/// <summary>
/// Core UI manager for the idle game.
/// Handles the display and interaction with game elements in a streamlined manner.
/// </summary>
public partial class UIManager : BaseManager
{
    // The main UI scene root
    private Control _uiRoot;
    private Dictionary<string, IUIObject> _UIControls;
    private Dictionary<string, List<IUIObject>> _UIMenus;

	//private DynamicGameLayoutMenu _currentMenu;
	private MenuManager _menuManager;


    public UIManager()
    {
		// Event registration moved to Setup()

	    List<IPanel> _uiitems = new List<IPanel>();
		_UIControls = new Dictionary<string, IUIObject>();
        _UIMenus = new Dictionary<string, List<IUIObject>>();

		RegisterEventHandlers();

		Utils.FindNodesWithClass(Globals.Instance.RootNode, _uiitems);

        foreach (var item in _uiitems)
        {

            if (item is UIControlItem _ucontrol)
            {
                if (_ucontrol.ControlType == UIControlType.Container)
                {
                    _ucontrol.RegisterControl();
                }
            }
        }
	}


    //once we have found the controls. we want to register them with the control manager.
	private void OnUIControlRegister(UIControlEventArgs args)
	{
        try
        {
            if (args.UIControlItem != null)
            {
                if (args.UIControlItem.ControlType == UIControlType.Container)
                {
                    _UIControls.Add(args.ControlID, args.UIControlItem);


                }
                else if (args.UIControlItem.ControlType == UIControlType.Control)
                {

                    if (!_UIMenus.ContainsKey(args.ControlParent.menuParentID))
                    {
                        _UIMenus[args.ControlParent.menuParentID] = new List<IUIObject>();
                    }
                    _UIMenus[args.ControlParent.menuParentID].Add(args.UIControlItem);
                }
            }
		}
        catch (Exception ex)
        {
            GD.Print(ex.Message);

        }
	}
	

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

        _menuManager = new MenuManager();
        _menuManager.Initialize();

        //_menuManager.ShowMenu("DynamicMainMenu");

		//InitializeMenus();

		// Register event handlers after initialization
		//RegisterEventHandlers();

		// Show main menu and verify it worked
		//ShowMenu<MainMenu>();
		//if (_currentMenu == null || _currentMenu.GetType() != typeof(DynamicGameLayoutMenu))
  //      {
  //          GD.PrintErr("UIManager: Failed to show main menu!");
  //      }
    }

    protected override void RegisterEventHandlers()
    {
        // Move event registration to Setup() where we know the event system exists
        if (Globals.Instance?.gameMangers?.Events == null)
        {
            GD.PrintErr("UIManager: Events system not available for registration!");
            return;
        }

		if (Globals.Instance?.gameMangers?.Events != null)
		{
			Globals.Instance.gameMangers.Events.AddHandler<UIControlEventArgs>(EventType.UIControlRegister, OnUIControlRegister);
			Globals.Instance.gameMangers.Events.AddHandler<UIControlEventArgs>(EventType.UIControlAssignToContainer, OnUiControlAssignToContainer);
			
		}
	
	}

	private void OnUiControlAssignToContainer(UIControlEventArgs args)
	{
		PanelPosition panelpostion = args.PanelLocation;
		PanelSection panelSection= args.PanelSection;

		foreach (KeyValuePair<string, IUIObject> _t in _UIControls.Where(p => p.Value.PanelLocation == panelpostion && p.Value.PanelSection == panelSection))
		{
			Node _n = (Node)_t.Value;
			Control _b = (Control)args.UIControlItem;
			_n.AddChild(_b);
		}

	}

	
}
