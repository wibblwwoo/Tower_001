using Godot;
using System;

// Dual Horizontal panel implementation
public partial class DualHorizontalPanel : BaseDynamicPanel
{
	private Control _baseInstance;
	protected VBoxContainer _leftContent;
	protected VBoxContainer _rightContent;

	public DualHorizontalPanel(PanelConfig config, Control baseInstance) : base(config)
	{
		_baseInstance = baseInstance;
	}

	protected override void LoadPanelScene()
	{
		AddChild(_baseInstance);

		// Get references to content containers
		var leftContainer = _baseInstance.GetNode<Container>("MarginContainer/HBoxContainer/Container_Left_side");
		var rightContainer = _baseInstance.GetNode<Container>("MarginContainer/HBoxContainer/Container_Right_side");

		_leftContent = leftContainer.GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/VBoxContainer");
		_rightContent = rightContainer.GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/VBoxContainer");
	}

	protected override void SetupSections()
	{
		RegisterSection("left", _leftContent);
		RegisterSection("right", _rightContent);
	}
}

