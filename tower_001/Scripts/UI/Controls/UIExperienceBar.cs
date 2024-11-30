using Godot;
using System;

// Dual progress bar for experience display
public partial class UIExperienceBar : Control, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }

	private ProgressBar _mainBar;
	private ProgressBar _overflowBar;
	private Label _levelLabel;
	private Label _expLabel;

	public override void _Ready()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		// Main XP bar
		_mainBar = new ProgressBar
		{
			CustomMinimumSize = new Vector2(200, 20)
		};
		AddChild(_mainBar);

		// Overflow bar for visual feedback when leveling
		_overflowBar = new ProgressBar
		{
			CustomMinimumSize = new Vector2(200, 20),
			Visible = false
		};
		AddChild(_overflowBar);

		// Level display
		_levelLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center
		};
		AddChild(_levelLabel);

		// Experience values display
		_expLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center
		};
		AddChild(_expLabel);
	}

	//public async Task UpdateExperience(int level, float currentXP, float requiredXP, float gainedXP = 0)
	//{
	//	_levelLabel.Text = $"Level {level}";
	//	_expLabel.Text = $"{currentXP:F0}/{requiredXP:F0}";

	//	if (gainedXP > 0)
	//	{
	//		// Show overflow animation
	//		_overflowBar.Visible = true;
	//		_overflowBar.Value = (currentXP - gainedXP) / requiredXP * 100;
	//		_mainBar.Value = currentXP / requiredXP * 100;

	//		// Animate the overflow bar
	//		var tween = CreateTween();
	//		tween.TweenProperty(_overflowBar, "value", 100, 0.5f);
	//		await ToSignal(tween, "finished");

	//		_overflowBar.Visible = false;
	//	}
	//	else
	//	{
	//		_mainBar.Value = currentXP / requiredXP * 100;
	//	}
	//}

	// IUIItem implementation
	public void Setup() { }
	public void EventFire() { }
	public void Refresh() { }
	public void Visiblity(bool visible)
	{
		Visible = visible;
	}
	public void CallEvent() { }
}