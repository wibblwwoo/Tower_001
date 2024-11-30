using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class UIStatDisplay : Control, IUIItem
{
	[Export] public bool IsEnabled { get; set; }
	[Export] public bool IsVisibleonStartup { get; set; }
	[Export] public StatType WatchedStat { get; set; }
	[Export] public DisplayFormat Format { get; set; }

	protected Label NameLabel;
	protected Label ValueLabel;
	protected Label BuffLabel;
	protected Panel TooltipPanel;
	protected VBoxContainer TooltipContent;

	private string _characterId;
	private Dictionary<string, StatModifier> _activeModifiers = new();

	public override void _Ready()
	{
		InitializeComponents();
		RegisterEvents();
	}

	private void InitializeComponents()
	{
		// Create and set up child components
		NameLabel = GetNodeOrNull<Label>("StatName");
		ValueLabel = GetNodeOrNull<Label>("StatValue");
		BuffLabel = GetNodeOrNull<Label>("BuffValue");

		// Initialize tooltip (hidden by default)
		TooltipPanel = GetNodeOrNull<Panel>("TooltipPanel");
		if (TooltipPanel != null)
		{
			TooltipPanel.Visible = false;
			TooltipContent = TooltipPanel.GetNodeOrNull<VBoxContainer>("Content");
		}

		// Set up mouse events for tooltip
		MouseEntered += ShowTooltip;
		MouseExited += HideTooltip;
	}

	public void Initialize(string characterId)
	{
		_characterId = characterId;
		Refresh(); // Initial update
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

		UpdateDisplay(args.NewValue, CalculateBuffPercentage());
	}

	private void OnBuffApplied(CharacterStatBuffEventArgs args)
	{
		if (args.CharacterId != _characterId || args.StatType != WatchedStat)
			return;

		_activeModifiers[args.Modifier.Id] = args.Modifier;
		//UpdateBuffDisplay();
	}

	private void OnBuffRemoved(CharacterStatBuffEventArgs args)
	{
		if (args.CharacterId != _characterId || args.StatType != WatchedStat)
			return;

		_activeModifiers.Remove(args.Modifier.Id);
		//;UpdateBuffDisplay();
	}

	private void UpdateDisplay(float value, float buffPercentage)
	{
		if (ValueLabel != null)
		{
			ValueLabel.Text = FormatValue(value);
		}

		if (BuffLabel != null && buffPercentage != 0)
		{
			BuffLabel.Text = $"({(buffPercentage > 0 ? "+" : "")}{buffPercentage:F1}%)";
			BuffLabel.Modulate = buffPercentage > 0 ?
				new Color(0, 1, 0) : // Green for positive
				new Color(1, 0, 0);  // Red for negative
		}
	}

	private string FormatValue(float value)
	{
		return Format switch
		{
			DisplayFormat.WholeNumber => $"{(int)value}",
			DisplayFormat.Decimal => $"{value:F1}",
			DisplayFormat.Percentage => $"{value:F1}%",
			_ => value.ToString()
		};
	}

	private float CalculateBuffPercentage()
	{
		float totalPercentage = 0;
		foreach (var modifier in _activeModifiers.Values)
		{
			if (modifier.Type == BuffType.Percentage)
			{
				totalPercentage += modifier.Value * 100;
			}
		}
		return totalPercentage;
	}

	private void ShowTooltip()
	{
		if (TooltipPanel == null) return;

		UpdateTooltipContent();
		TooltipPanel.Visible = true;

		// Position tooltip above or below based on screen space
		Vector2 mousePos = GetViewport().GetMousePosition();
		TooltipPanel.Position = new Vector2(
			mousePos.X + 10,
			mousePos.Y + 10
		);
	}

	private void HideTooltip()
	{
		if (TooltipPanel != null)
		{
			TooltipPanel.Visible = false;
		}
	}

	private void UpdateTooltipContent()
	{
		if (TooltipContent == null) return;

		// Clear existing content
		foreach (Node child in TooltipContent.GetChildren())
		{
			child.QueueFree();
		}

		// Add base stat info
		var baseValue = GetBaseStat();
		AddTooltipLine($"Base {WatchedStat}: {FormatValue(baseValue)}");

		// Group modifiers by source
		var groupedModifiers = _activeModifiers.Values
			.GroupBy(m => m.Source)
			.ToDictionary(g => g.Key, g => g.ToList());

		foreach (var group in groupedModifiers)
		{
			AddTooltipLine($"\n{group.Key}:");
			foreach (var modifier in group.Value)
			{
				string modText = modifier.Type switch
				{
					BuffType.Flat => $"{(modifier.Value > 0 ? "+" : "")}{FormatValue(modifier.Value)}",
					BuffType.Percentage => $"{(modifier.Value > 0 ? "+" : "")}{modifier.Value * 100:F1}%",
					_ => modifier.Value.ToString()
				};
				AddTooltipLine($"  {modText}");
			}
		}

		// Add total
		AddTooltipLine($"\nTotal: {FormatValue(GetStatValue())}");
	}

	private void AddTooltipLine(string text)
	{
		var label = new Label();
		label.Text = text;
		TooltipContent.AddChild(label);
	}

	private float GetBaseStat()
	{
		// Implementation to get base stat value
		return 0; // Placeholder
	}

	private float GetStatValue()
	{
		// Implementation to get current stat value
		return 0; // Placeholder
	}

	public void Setup() { }
	public void EventFire() { }
	public void Refresh()
	{
		var value = GetStatValue();
		UpdateDisplay(value, CalculateBuffPercentage());
	}
	public void Visiblity(bool visible)
	{
		Visible = visible;
	}
	public void CallEvent() { }
}
