using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;


// Panel Controller for managing panel interactions and state
public class DynamicPanelController
{
	private Dictionary<string, Control> _containers;
	public DynamicPanelController()
	{
		_containers = new Dictionary<string, Control>();
	}


	public void RegisterContainer(string containerId, Control container)
	{
		_containers[containerId] = container;
		GD.Print($"Registered container {containerId}: {container}");
	}

	public BaseDynamicPanel CreatePanel(PanelConfig config, string containerId)
	{
		if (!_containers.TryGetValue(containerId, out var container))
		{
			GD.PrintErr($"Container not found: {containerId}");
			return null;
		}

		BaseDynamicPanel panel = config.Type switch
		{
			PanelType.Single => new SingleDynamicPanel(config, container),
			PanelType.Dual_Horizontal => new DualHorizontalPanel(config, container),
			PanelType.Dual_Vertical => new DualVerticalPanel(config, container),
			_ => null
		};

		if (panel == null)
		{
			GD.PrintErr($"Failed to create panel of type: {config.Type}");
			return null;
		}

		return panel;
	}

	//public void DestroyPanel(string panelId)
	//{
	//	_registry.DestroyPanel(panelId);
	//}

	//public BaseDynamicPanel GetPanel(string panelId)
	//{
	//	return _registry.GetPanel(panelId);
	//}

	//public void Cleanup()
	//{
	//	_registry.CleanupAllPanels();
	//	_containerMap.Clear();
	//}
}
