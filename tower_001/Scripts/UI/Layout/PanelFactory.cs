using Godot;
using System.Collections.Generic;

public static partial class PanelFactory
{
	private static readonly Dictionary<string, PackedScene> _panelScenes = new();

	public static void RegisterPanelScene(string panelType, PackedScene scene)
	{
		_panelScenes[panelType] = scene;
	}

	public static IPanel CreatePanel(string panelType, string panelId)
	{
		if (!_panelScenes.ContainsKey(panelType))
		{
			GD.PrintErr($"Panel type {panelType} not registered");
			return null;
		}

		var scene = _panelScenes[panelType];
		var panel = scene.Instantiate() as BasePanel;

		if (panel == null)
		{
			GD.PrintErr($"Failed to instantiate panel of type {panelType}");
			return null;
		}

		panel.Initialize();
		PanelRegistry.Instance.RegisterPanel(panel);

		return panel;
	}
}