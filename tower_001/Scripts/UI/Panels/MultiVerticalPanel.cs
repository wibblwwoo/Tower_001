using Godot;
using System;

public partial class MultiVerticalPanel : BaseDynamicPanel
{
	private Control _baseInstance;
	protected VBoxContainer _topContent;
	protected VBoxContainer _middleContent;
	protected VBoxContainer _bottomContent;

	public MultiVerticalPanel(PanelConfig config, Control baseInstance) : base(config)
	{
		_baseInstance = baseInstance;
	}

	protected override void LoadPanelScene()
	{
		AddChild(_baseInstance);

		var topContainer = _baseInstance.GetNode<Container>("MarginContainer/VBoxContainer/Container_Top");
		var middleContainer = _baseInstance.GetNode<Container>("MarginContainer/VBoxContainer/Container_Middle");
		var bottomContainer = _baseInstance.GetNode<Container>("MarginContainer/VBoxContainer/Container_Bottom");

		_topContent = topContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
		_middleContent = middleContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
		_bottomContent = bottomContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
	}

	protected override void SetupSections()
	{
		RegisterSection("top", _topContent);
		RegisterSection("middle", _middleContent);
		RegisterSection("bottom", _bottomContent);
	}
}