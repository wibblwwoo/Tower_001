using Godot;
using System;

public partial class SingleDynamicPanel : BaseDynamicPanel
{
	private Control _container;

	public SingleDynamicPanel(PanelConfig config, Control container) : base(config)
	{
		_container = container;
		_sections["main"] = _container;
	}

	public virtual void AddContent(string sectionId, Control content)
	{
		if (!_sections.TryGetValue(sectionId, out var section))
		{
			GD.PrintErr($"Section {sectionId} not found in panel {PanelId}");
			return;
		}

		section.AddChild(content);
		_contentNodes[content.Name] = content;
		GD.Print($"Added content to section {sectionId} in {PanelId}");
	}


	public override void Show()
	{
		if (_container != null)
		{
			_container.Show();
		}
	}

	public override void Hide()
	{
		if (_container != null)
		{
			_container.Hide();
		}
	}

	public override void Cleanup()
	{
		if (_container != null)
		{
			foreach (var child in _container.GetChildren())
			{
				child.QueueFree();
			}
		}
	}

	protected override void LoadPanelScene()
	{
		
	}

	protected override void SetupSections()
	{
		
	}
}