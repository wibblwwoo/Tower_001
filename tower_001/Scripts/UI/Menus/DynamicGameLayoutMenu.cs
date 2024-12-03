using Godot;
using System;
using System.Collections.Generic;

// Extension of GameLayoutMenu to support dynamic panels
public abstract partial class DynamicGameLayoutMenu : Control
{
	protected PanelMenuIntegration _panelIntegration;
	protected Dictionary<string, BaseDynamicPanel> _menuPanels;
	protected Control _menuRoot;
	protected string _menuName;
	protected bool _initialized;

	protected DynamicGameLayoutMenu(string menuName)
	{
		_menuName = menuName;
		_panelIntegration = new PanelMenuIntegration();
		_menuPanels = new Dictionary<string, BaseDynamicPanel>();
	}

	public virtual void Initialize(Control uiRoot)
	{
		if (_initialized) return;

		GD.Print($"Initializing {_menuName}...");

		InitializeMenuRoot(uiRoot);
		RegisterContainers(uiRoot);
		InitializePanels();

		_initialized = true;
		Hide();
	}
	protected abstract void InitializePanels();


	protected BaseDynamicPanel CreatePanel(string menuId)

	{

		var panel = _panelIntegration.CreateMenuPanel(menuId);
		if (panel != null)

		{
			_menuPanels[menuId] = panel;
		}
		return panel;
	}


	private void InitializeMenuRoot(Control uiRoot)
	{
		var control2 = uiRoot.GetNode<Control>("Control2");
		if (control2 == null)
		{
			GD.PrintErr($"{_menuName}: Control2 node not found in UI root!");
			return;
		}

		_menuRoot = new Control { Name = $"{_menuName}Root" };
		_menuRoot.SetAnchorsPreset(LayoutPreset.FullRect);
		control2.AddChild(_menuRoot);
	}

	protected virtual void RegisterContainers(Control uiRoot)
	{
		var containerPaths = new Dictionary<string, string>
		{
			{ "Middle_Middle_CenterContainer", "MarginContainer/MiddleControl_Panel/HBoxContainer_Middle_Panel/Panel/VBoxContainer/Middle/Middle_Middle_VBoxContainer" },
			{ "LeftPanel", "LeftControl/MarginContainer/LeftControl_Panel/HBoxContainer_Left_Panel/Panel/VBoxContainer/Middle/VBoxContainer" },
			{ "RightPanel", "MarginContainer/MiddleControl_Panel/HBoxContainer_Middle_Panel/Panel/VBoxContainer" }
		};

		var control2 = uiRoot.GetNode<Control>("Control2");
		if (control2 == null) return;

		foreach (var container in containerPaths)
		{
			var node = control2.GetNode<Control>(container.Value);
			if (node != null)
			{
				_panelIntegration.RegisterMenuContainer(container.Key, node);
				GD.Print($"Registered {container.Key}");
			}
			else
			{
				GD.PrintErr($"Failed to find {container.Key}");
			}
		}
	}

	protected BaseDynamicPanel CreateMenuPanel(string menuId)
	{
		if (_menuPanels.ContainsKey(menuId))
		{
			GD.PrintErr($"Panel {menuId} already exists!");
			return _menuPanels[menuId];
		}

		var panel = _panelIntegration.CreateMenuPanel(menuId);
		if (panel != null)
		{
			_menuPanels[menuId] = panel;
			GD.Print($"Created panel: {menuId}");
		}
		return panel;
	}

	public virtual void Show()
	{
		if (!_initialized)
		{
			GD.PrintErr($"Attempting to show uninitialized menu: {_menuName}");
			return;
		}

		foreach (var panel in _menuPanels.Values)
		{
			panel.Show();
		}

		_menuRoot?.Show();
		GD.Print($"Showing menu: {_menuName}");
	}

	public virtual void Hide()
	{
		foreach (var panel in _menuPanels.Values)
		{
			panel.Hide();
		}

		_menuRoot?.Hide();
		GD.Print($"Hiding menu: {_menuName}");
	}

	public virtual void Cleanup()
	{
		foreach (var panel in _menuPanels.Values)
		{
			panel.Cleanup();
		}
		_menuPanels.Clear();

		_menuRoot?.QueueFree();
		_menuRoot = null;

		_initialized = false;
	}

	private void PrintNodeHierarchy(Node node, string indent = "")
	{
		GD.Print($"{indent}{node.Name} ({node.GetType()})");
		foreach (var child in node.GetChildren())
		{
			PrintNodeHierarchy(child, indent + "  ");
		}
	}
}