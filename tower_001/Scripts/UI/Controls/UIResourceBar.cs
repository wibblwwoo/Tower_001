using Godot;
using System;
using static GlobalEnums;

// Resource bar specialized for health/mana display
public partial class UIResourceBar : UIStatProgressBar
{
	[Export] public ResourceType ResourceType { get; set; }
	[Export] public bool ShowRegenRate { get; set; } = true;

	private Label _regenLabel;

	public override void _Ready()
	{
		base._Ready();

		if (ShowRegenRate)
		{
			InitializeRegenLabel();
		}
	}

	private void InitializeRegenLabel()
	{
		_regenLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center
		};
		AddChild(_regenLabel);
	}

	public void UpdateRegenRate(float regenPerSecond)
	{
		if (_regenLabel != null)
		{
			_regenLabel.Text = $"+{regenPerSecond:F1}/s";
			_regenLabel.Modulate = regenPerSecond > 0 ? Colors.Green : Colors.White;
		}
	}
}