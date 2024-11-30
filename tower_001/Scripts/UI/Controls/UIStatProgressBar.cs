using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class UIStatProgressBar : ProgressBar, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }
	[Export] public StatType WatchedStat { get; set; }
	[Export] public bool ShowTooltip { get; set; } = true;
	[Export] public new bool ShowPercentage { get; set; } = true;
	[Export] public Color NormalColor { get; set; } = Colors.Green;
	[Export] public Color WarningColor { get; set; } = Colors.Yellow;
	[Export] public Color DangerColor { get; set; } = Colors.Red;

	private string _characterId;
	private Label _valueLabel;
	private Panel _tooltipPanel;
	private VBoxContainer _tooltipContent;
	private Dictionary<string, StatModifier> _activeModifiers = new();

	// Thresholds for color changes (as percentage of max value)
	private const float WARNING_THRESHOLD = 0.5f;
	private const float DANGER_THRESHOLD = 0.25f;

	public override void _Ready()
	{
		InitializeComponents();
		RegisterEvents();
		Step = 0.1f; // For smoother visual updates
	}

	private void InitializeComponents()
	{
		// Create value label if showing percentage
		if (ShowPercentage)
		{
			_valueLabel = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			AddChild(_valueLabel);
		}

		// Initialize tooltip if enabled
		if (ShowTooltip)
		{
			InitializeTooltip();
		}

		// Set up mouse events
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	private void InitializeTooltip()
	{
		_tooltipPanel = new Panel
		{
			Visible = false
		};

		_tooltipContent = new VBoxContainer();
		_tooltipPanel.AddChild(_tooltipContent);

		AddChild(_tooltipPanel);
	}

	public void Initialize(string characterId)
	{
		_characterId = characterId;
		Refresh();
	}

	private void RegisterEvents()
	{
		var events = Globals.Instance.gameMangers.Events;

		events.AddHandler<CharacterStatEventArgs>(
			EventType.CharacterStatChanged,
			OnStatChanged);

		events.AddHandler<CharacterStatBuffEventArgs>(
			EventType.CharacterStatBuffApplied,
			OnBuffApplied);

		events.AddHandler<CharacterStatBuffEventArgs>(
			EventType.CharacterStatBuffRemoved,
			OnBuffRemoved);
	}

	private void OnStatChanged(CharacterStatEventArgs args)
	{
		if (args.CharacterId != _characterId || args.StatType != WatchedStat)
			return;

		UpdateProgressBar(args.NewValue);
	}

	private void OnBuffApplied(CharacterStatBuffEventArgs args)
	{
		if (args.CharacterId != _characterId || args.StatType != WatchedStat)
			return;

		_activeModifiers[args.Modifier.Id] = args.Modifier;
		if (ShowTooltip) UpdateTooltip();
	}

	private void OnBuffRemoved(CharacterStatBuffEventArgs args)
	{
		if (args.CharacterId != _characterId || args.StatType != WatchedStat)
			return;

		_activeModifiers.Remove(args.Modifier.Id);
		if (ShowTooltip) UpdateTooltip();
	}

	private void UpdateProgressBar(float value)
	{
		Value = value;

		if (ShowPercentage)
		{
			float percentage = (float)((Value / MaxValue) * 100);
			_valueLabel.Text = $"{percentage:F1}%";
		}

		UpdateProgressColor();
	}

	private void UpdateProgressColor()
	{
		float ratio = (float)(Value / MaxValue);
		Color targetColor;

		if (ratio <= DANGER_THRESHOLD)
			targetColor = DangerColor;
		else if (ratio <= WARNING_THRESHOLD)
			targetColor = WarningColor;
		else
			targetColor = NormalColor;

		// Update progress bar color - method depends on your theme setup
		// You might need to adjust this based on your Godot theme configuration
		AddThemeColorOverride("fill_color", targetColor);
	}

	private void UpdateTooltip()
	{
		if (!ShowTooltip || _tooltipContent == null) return;

		foreach (Node child in _tooltipContent.GetChildren())
		{
			child.QueueFree();
		}

		// Add current value
		AddTooltipLine($"{WatchedStat}: {Value:F0}/{MaxValue:F0}");

		// Add active buffs
		if (_activeModifiers.Any())
		{
			AddTooltipLine("\nActive Effects:");
			foreach (var modifier in _activeModifiers.Values)
			{
				string effectText = modifier.Type switch
				{
					BuffType.Flat => $"{modifier.Source}: {(modifier.Value > 0 ? "+" : "")}{modifier.Value:F0}",
					BuffType.Percentage => $"{modifier.Source}: {(modifier.Value > 0 ? "+" : "")}{modifier.Value * 100:F1}%",
					_ => $"{modifier.Source}: {modifier.Value}"
				};
				AddTooltipLine(effectText);
			}
		}
	}

	private void AddTooltipLine(string text)
	{
		var label = new Label { Text = text };
		_tooltipContent.AddChild(label);
	}

	private void OnMouseEntered()
	{
		if (!ShowTooltip) return;

		UpdateTooltip();
		_tooltipPanel.Visible = true;

		// Position tooltip above the progress bar
		var mousePos = GetViewport().GetMousePosition();
		_tooltipPanel.Position = new Vector2(
			mousePos.X + 10,
			mousePos.Y - _tooltipPanel.Size.Y - 10
		);
	}

	private void OnMouseExited()
	{
		if (_tooltipPanel != null)
		{
			_tooltipPanel.Visible = false;
		}
	}

	// IUIItem implementation
	public void Setup() { }
	public void EventFire() { }
	public void Refresh()
	{
		UpdateProgressBar((float)Value);
		if (ShowTooltip) UpdateTooltip();
	}
	public void Visiblity(bool visible)
	{
		Visible = visible;
	}
	public void CallEvent() { }
}