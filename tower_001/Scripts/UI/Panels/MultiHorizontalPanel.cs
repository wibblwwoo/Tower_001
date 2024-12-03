using Godot;
using System;
using System.Collections.Generic;

// Multi Horizontal panel implementation
public partial class MultiHorizontalPanel : BaseDynamicPanel
{
	private Control _baseInstance;
	protected Dictionary<string, VBoxContainer> _quadrants;

	public MultiHorizontalPanel(PanelConfig config, Control baseInstance) : base(config)
	{
		_baseInstance = baseInstance;
		_quadrants = new Dictionary<string, VBoxContainer>();
	}

	protected override void LoadPanelScene()
	{
		AddChild(_baseInstance);

		// Get references to all quadrants
		var positions = new[] { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
		foreach (var pos in positions)
		{
			string rowType = pos.StartsWith("Top") ? "TopRow" : "BottomRow";
			var container = _baseInstance.GetNode<Container>($"MarginContainer/VBoxContainer/HBoxContainer_{rowType}/Container_{pos}");
			var content = container.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
			_quadrants[pos.ToLower()] = content;
		}
	}

	protected override void SetupSections()
	{
		foreach (var kvp in _quadrants)
		{
			RegisterSection(kvp.Key, kvp.Value);
		}
	}
}
