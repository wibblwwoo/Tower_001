using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

// Panel Registry to manage all panel instances
public class DynamicPanelRegistry
{
	private static DynamicPanelRegistry _instance;
	private readonly Dictionary<string, BaseDynamicPanel> _activePanels;
	private readonly Dictionary<PanelType, PackedScene> _panelScenes;

	public static DynamicPanelRegistry Instance
	{
		get
		{
			_instance ??= new DynamicPanelRegistry();
			return _instance;
		}
	}

	private DynamicPanelRegistry()
	{
		_activePanels = new Dictionary<string, BaseDynamicPanel>();
		_panelScenes = new Dictionary<PanelType, PackedScene>();
		LoadPanelScenes();
	}

	private void LoadPanelScenes()
	{
		// Load all panel scene resources
		_panelScenes[PanelType.Single] = Globals.Instance.PanelType_Single;
		_panelScenes[PanelType.Dual_Horizontal] = Globals.Instance.PanelType_Dual_Horizontal;
		_panelScenes[PanelType.Multi_Horizontal] = Globals.Instance.PanelType_Multi_Horizontal;
		_panelScenes[PanelType.Dual_Vertical] = Globals.Instance.PanelType_Dual_Vertical;
		_panelScenes[PanelType.Multi_Vertical] = Globals.Instance.PanelType_Multi_Vertical;

		//_panelScenes[PanelType.Single] = GD.Load<PackedScene>("res://Scenes/UI/Panels/SinglePanel.tscn");
		//_panelScenes[PanelType.Dual_Horizontal] = GD.Load<PackedScene>("res://Scenes/UI/Panels/DualPanel.tscn");
		//_panelScenes[PanelType.Multi_Horizontal] = GD.Load<PackedScene>("res://Scenes/UI/Panels/MultiPanel.tscn");
		//_panelScenes[PanelType.Dual_Vertical] = GD.Load<PackedScene>("res://Scenes/UI/Panels/VerticalSplitPanel.tscn");
		//_panelScenes[PanelType.Multi_Vertical] = GD.Load<PackedScene>("res://Scenes/UI/Panels/TripleVerticalPanel.tscn");
	}

	public void RegisterPanelScene(PanelType type, PackedScene scene)
	{
		_panelScenes[type] = scene;
	}

	public BaseDynamicPanel CreatePanel(PanelConfig config)
	{
		if (!_panelScenes.TryGetValue(config.Type, out var scene))
		{
			GD.PrintErr($"Panel scene not found for type {config.Type}");
			return null;
		}

		var panel = CreatePanelInstance(scene, config);
		if (panel != null)
		{
			_activePanels[config.PanelId] = panel;
		}

		return panel;
	}

	private BaseDynamicPanel CreatePanelInstance(PackedScene scene, PanelConfig config)
	{
		var instance = scene.Instantiate<Control>();


		if (instance == null)
		{
			GD.PrintErr($"Failed to instantiate panel scene for type {config.Type}");
			return null;
		}

		// Create appropriate panel wrapper based on type
		BaseDynamicPanel panel = config.Type switch
		{
			PanelType.Single => new SingleDynamicPanel(config, instance),
			PanelType.Dual_Horizontal => new DualHorizontalPanel(config, instance),
			PanelType.Multi_Horizontal => new MultiHorizontalPanel(config, instance),
			PanelType.Dual_Vertical => new DualVerticalPanel(config, instance),
			PanelType.Multi_Vertical => new MultiVerticalPanel(config, instance),
			_ => throw new ArgumentException($"Unsupported panel type: {config.Type}")
		};

		panel.Initialize();
		return panel;
	}

	public BaseDynamicPanel GetPanel(string panelId)
	{
		return _activePanels.TryGetValue(panelId, out var panel) ? panel : null;
	}

	public void DestroyPanel(string panelId)
	{
		if (_activePanels.TryGetValue(panelId, out var panel))
		{
			panel.Cleanup();
			_activePanels.Remove(panelId);
		}
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

