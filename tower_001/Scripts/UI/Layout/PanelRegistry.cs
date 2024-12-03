using Godot;
using System;
using System.Collections.Generic;

public partial class PanelRegistry
{
	private static PanelRegistry _instance;
	private readonly Dictionary<string, IPanel> _activePanels;

	public static PanelRegistry Instance
	{
		get
		{
			_instance ??= new PanelRegistry();
			return _instance;
		}
	}

	private PanelRegistry()
	{
		_activePanels = new Dictionary<string, IPanel>();
	}

	public void RegisterPanel(IPanel panel)
	{
		if (!_activePanels.ContainsKey(panel.PanelId))
		{
			_activePanels.Add(panel.PanelId, panel);
		}
	}

	public void UnregisterPanel(string panelId)
	{
		if (_activePanels.ContainsKey(panelId))
		{
			var panel = _activePanels[panelId];
			panel.Cleanup();
			_activePanels.Remove(panelId);
		}
	}

	public IPanel GetPanel(string panelId)
	{
		return _activePanels.TryGetValue(panelId, out var panel) ? panel : null;
	}

	public void CleanupAllPanels()
	{
		foreach (var panel in _activePanels.Values)
		{
			panel.Cleanup();
		}
		_activePanels.Clear();
	}
}