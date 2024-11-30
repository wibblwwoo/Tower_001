using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static GlobalEnums;

public partial class UIManager : BaseManager
{

	private readonly Dictionary<GlobalEnums.EnumUIPanelParentType, List<Node>> _panelsByParent = new();
	private readonly Dictionary<GlobalEnums.EnumUIButtonType, Button> _buttons = new();
	private readonly HashSet<GlobalEnums.EnumUIPanelType> _visiblePanels = new();
	private Node _parent;
	private List<IUIItem> _uiitems;

	public UIManager()
	{
		base.Setup();
	}

	public override void Setup()
	{
		
	}

	protected override void RegisterEventHandlers()
	{
		Setup_Panels();
	}


	public void Setup_Panels()
	{
		_uiitems = new List<IUIItem>();
		//_leftGameButtons = new List<UILeftButton_Item>();

		//load all the UI items into a collection
		Utils.FindNodesWithClass(Globals.Instance.RootNode, _uiitems);
		//Utils.FindNodesWithClass(Globals.Instance.RootNode, _leftGameButtons);

		//now lets turn on and off any that need to be visible.
		ShowHideMenuItems(useDefaultSetting: true, visible: false);
		//Setup_LeftButtons();

		foreach (var IPanel in _uiitems)
		{
			if (IPanel is UIPanel panel)
			{

				RegisterPanelsFromNode(panel);
			}
			if (IPanel is UIButton_Item button)
			{

				RegisterButtonFromNode(button);
			}
			if (IPanel is UIContainer container)
			{

				RegisterButtonFromNode(container);
			}


		}

	}

	public void ShowHideMenuItems(bool useDefaultSetting, bool visible, List<IPanel> ToShow = null)
	{
		// Iterate over each menu item in GameMenus
		_uiitems.ForEach(item =>
			// Set visibility based on the specified setting
			item.Visiblity(
				// If useDefaultSetting is true, use the item's own IsVisible property
				// Otherwise, use the provided 'visible' parameter
				useDefaultSetting ? item.IsVisibleonStartup : visible
			)
		);
		//if (ToShow != null)
		//{
		//	_uiitems.ForEach(item =>
		//		// Set visibility based on the specified setting
		//		item.Visiblity(ToShow.Contains(item)
		//		)
		//	);
		//}
	}

	public void RegisterPanelsFromNode(UIPanel panel)
	{
		RegisterPanel(panel, panel.PanelParent);
	}

	public void RegisterButtonFromNode(UIButton_Item button)
	{
		RegisterPanel(button, button.ShowButtonByParent);
	}

	public void RegisterButtonFromNode(UIContainer container)
	{
		RegisterPanel(container, container.PanelParent);
	}
	public void RegisterPanel(Node panel, EnumUIPanelParentType parentType)
	{
		// Get all flag values that are set
		foreach (var flag in parentType.GetFlags())
		{
			if (!_panelsByParent.ContainsKey(flag))
			{
				_panelsByParent[flag] = new List<Node>();
			}
			_panelsByParent[flag].Add(panel);
		}
	}



	// Get all panels for a specific parent type
	public List<Node> GetPanelsByParentType(EnumUIPanelParentType parentType)
	{
		return _panelsByParent.TryGetValue(parentType, out var panels)
			? panels
			: new List<Node>();
	}

	// Example method to show/hide panels by parent type
	public void SetPanelsVisibility(EnumUIPanelParentType parentType, bool visible)
	{

		_uiitems.ForEach(item => item.Visiblity(false));

		foreach (var flag in parentType.GetFlags())
		{
			if (_panelsByParent.TryGetValue(flag, out var panels))
			{
				foreach (var _panel in panels)
				{
					if (_panel is UIPanel panel)
					{

						panel.Visiblity(visible);
					}
					else
					if (_panel is UIButton_Item button)
					{

						button.Visiblity(visible);
					}

				}
			}
		}
	}


}
