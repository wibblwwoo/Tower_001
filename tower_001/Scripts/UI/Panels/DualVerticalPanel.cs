using Godot;
using System;

public partial class DualVerticalPanel : BaseDynamicPanel
{
	private Control _baseInstance;
	protected VBoxContainer _topContent;
	protected VBoxContainer _bottomContent;

	public DualVerticalPanel(PanelConfig config, Control baseInstance) : base(config)
	{
		_baseInstance = baseInstance;
	}

	protected override void LoadPanelScene()
	{
		AddChild(_baseInstance);

		var topContainer = _baseInstance.GetNode<Container>("MarginContainer/VBoxContainer/Container_Top");
		var bottomContainer = _baseInstance.GetNode<Container>("MarginContainer/VBoxContainer/Container_Bottom");

		_topContent = topContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
		_bottomContent = bottomContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
	}

	protected override void SetupSections()
	{
		RegisterSection("top", _topContent);
		RegisterSection("bottom", _bottomContent);
	}
}