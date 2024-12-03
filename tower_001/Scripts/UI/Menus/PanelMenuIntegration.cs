using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;

public class PanelMenuIntegration
{
	private readonly DynamicPanelController _panelController;
	private readonly Dictionary<string, PanelConfig> _menuPanelConfigs;
	private bool _initialized;

	public PanelMenuIntegration()
	{
		_panelController = new DynamicPanelController();
		_menuPanelConfigs = new Dictionary<string, PanelConfig>();
		InitializeMenuPanelConfigs();
	}

	private void InitializeMenuPanelConfigs()
	{
		if (_initialized) return;

		GD.Print("Initializing menu panel configurations...");

		RegisterPanelConfig(new PanelConfig(
			"MainMenu",
			PanelType.Single,
			requiresTop: false,
			requiresBottom: false
		)
		{ Settings = { ["container"] = "Middle_Middle_CenterContainer" } });

		RegisterPanelConfig(new PanelConfig(
			"SettingsMenu",
			PanelType.Dual_Horizontal,
			requiresTop: true,
			requiresBottom: true
		)
		{ Settings = { ["container"] = "LeftPanel" } });

		LogPanelConfigurations();
		_initialized = true;
	}

	private void RegisterPanelConfig(PanelConfig config)
	{
		_menuPanelConfigs[config.PanelId] = config;
	}

	private void LogPanelConfigurations()
	{
		foreach (var config in _menuPanelConfigs)
		{
			GD.Print($"Configured {config.Key} panel: " +
					 $"Type={config.Value.Type}, " +
					 $"RequiresTop={config.Value.RequiresTopSection}, " +
					 $"RequiresBottom={config.Value.RequiresBottomSection}");
		}
	}

	public void RegisterMenuContainer(string menuId, Control container)
	{
		if (container == null)
		{
			GD.PrintErr($"Attempting to register null container for {menuId}");
			return;
		}

		GD.Print($"Registering container for {menuId}: {container}");
		_panelController.RegisterContainer(menuId, container);
	}

	public BaseDynamicPanel CreateMenuPanel(string menuId)
	{
		GD.Print($"Creating menu panel for {menuId}");

		if (!_menuPanelConfigs.TryGetValue(menuId, out var config))
		{
			GD.PrintErr($"No panel configuration found for menu: {menuId}");
			return null;
		}

		string containerId = config.Settings.GetValueOrDefault("container", "") as string;
		if (string.IsNullOrEmpty(containerId))
		{
			GD.PrintErr($"No container specified for menu: {menuId}");
			return null;
		}

		var panel = _panelController.CreatePanel(config, containerId);
		if (panel == null)
		{
			GD.PrintErr($"Failed to create panel for menu: {menuId}");
			return null;
		}

		GD.Print($"Successfully created panel for {menuId} in container {containerId}");
		return panel;
	}
}