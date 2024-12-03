// Controller for managing panels
using Godot;
using System.Collections.Generic;
using static GlobalEnums;

public class PanelController
{
	private readonly Dictionary<PanelPosition, Control> _panelContainers;

	public PanelController()
	{
		_panelContainers = new Dictionary<PanelPosition, Control>();
	}

	public void RegisterContainer(PanelPosition position, Control container)
	{
		_panelContainers[position] = container;
	}

	public void ShowPanel(string panelId, PanelPosition position)
	{
		var panel = PanelRegistry.Instance.GetPanel(panelId);
		if (panel == null || !_panelContainers.ContainsKey(position)) return;

		var container = _panelContainers[position];
		if (panel is Control control)
		{
			container.AddChild(control);
			panel.Show();
		}
	}

	public void HidePanel(string panelId)
	{
		var panel = PanelRegistry.Instance.GetPanel(panelId);
		panel?.Hide();
	}

	public void CleanupPanel(string panelId)
	{
		PanelRegistry.Instance.UnregisterPanel(panelId);
	}
}