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
public partial class UIManager : BaseManager, IDisposable
{
    /// <summary>
    /// The main UI scene root control node
    /// </summary>
    private Control _uiRoot;

    /// <summary>
    /// Dictionary of all registered UI controls, indexed by their control ID
    /// Used for container controls and standalone UI elements
    /// </summary>
    private Dictionary<string, IUIObject> _UIControls;

    /// <summary>
    /// Dictionary of menu-specific UI controls, organized by menu parent ID
    /// Each menu can have multiple child controls stored in a list
    /// </summary>
    private Dictionary<string, List<IUIObject>> _UIMenus;

    /// <summary>
    /// Reference to the menu management system
    /// </summary>
	private MenuManager _menuManager;

    /// <summary>
    /// Initializes a new instance of UIManager.
    /// Sets up control collections and registers initial UI items.
    /// </summary>
	public UIManager()
	{
		
	}


    /// <summary>
    /// Handles the registration of UI controls in the system.
    /// Organizes controls based on their type (Container or Control)
    /// and associates them with their parent menus if applicable.
    /// </summary>
    /// <param name="args">Event arguments containing control information</param>
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


  
	
	/// <summary>
    /// Sets up the UI system, initializing the root control and menu manager.
    /// </summary>
	public override void Setup()
    {
		List<IPanel> _uiitems = new List<IPanel>();
		_UIControls = new Dictionary<string, IUIObject>();
		_UIMenus = new Dictionary<string, List<IUIObject>>();

		base.Setup();


		Utils.FindNodesWithClass(Globals.Instance.RootNode, _uiitems);

		foreach (var item in _uiitems)
		{

			if (item is UIControlItem _ucontrol)
			{
				if (_ucontrol.ControlType == UIControlType.Container)
				{
					_ucontrol.RegisterControl();
					_ucontrol.Visible = _ucontrol.IsVisibleByDefault;

					if (!_ucontrol.Visible)
					{
						GD.PrintErr(_ucontrol.Name + " " + _ucontrol.ObjectID + " is hidden ");

					}
				}
			}
		}

        
        // Get the root node from game manager
        _uiRoot = (Control)Globals.Instance.RootNode;
        if (_uiRoot == null)
        {
            GD.PrintErr("UIManager: Root node not found!");
            return;
        }

        _menuManager = new MenuManager();
        _menuManager.Initialize();

      
    }

    /// <summary>
    /// Registers event handlers for UI-related events.
    /// Sets up handlers for control registration and container assignment.
    /// </summary>
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

    /// <summary>
    /// Handles the assignment of UI controls to their container panels.
    /// Matches controls with containers based on panel position and section.
    /// </summary>
    /// <param name="args">Event arguments containing control and container information</param>
	private void OnUiControlAssignToContainer(UIControlEventArgs args)
	{
		PanelPosition panelpostion = args.PanelLocation;
		PanelSection panelSection= args.PanelSection;

        try
        {

            foreach (KeyValuePair<string, IUIObject> _t in _UIControls.Where(p => p.Value.PanelLocation == panelpostion && p.Value.PanelSection == panelSection))
            {
                Node _n = (Node)_t.Value;
                Control _b = (Control)args.UIControlItem;

                _n.AddChild(_b);
            }

        }catch(Exception ex) 
        {

            GD.Print(ex.Message);
        }

	}

    /// <summary>
    /// Retrieves a list of all UI objects of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of UI object to retrieve.</typeparam>
    /// <param name="resultList">An optional list to store the results in.</param>
    /// <returns>A list of UI objects of the specified type.</returns>
    public List<IUIObject> GetListofAllIUIObjectByType<T>(List<T> resultList) where T : class
    {
        // Create a new list to store matching objects
        List<IUIObject> matchingObjects = new List<IUIObject>();

        // Iterate through all UI controls and check for type match
        foreach (var control in _UIControls.Values)
        {
            // Check if the control can be cast to type T
            if (control is T)
            {
                matchingObjects.Add(control);
                resultList?.Add(control as T);
            }
        }
        return matchingObjects;
    }

	public void Dispose()
	{
		
	}
}
