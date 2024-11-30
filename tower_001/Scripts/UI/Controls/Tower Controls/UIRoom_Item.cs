using Godot;
using System;

public partial class UIRoom_Item : Label
{
	//private RoomData _roomData;
	private Panel _tooltipPanel;
	private bool _isCompleted;
	private bool _isCurrent;

	public override void _Ready()
	{
		MouseEntered += ShowTooltip;
		MouseExited += HideTooltip;
		InitializeTooltip();
	}

	private void InitializeTooltip()
	{
		//throw new NotImplementedException();
	}

	//public void UpdateRoom(RoomData room, bool isCurrent, bool isCompleted)
	//{
	//	//_roomData = room;
	//	//_isCompleted = isCompleted;
	//	//_isCurrent = isCurrent;

	//	//// Get display configuration
	//	//var color = RoomDisplayConfig.GetRoomColor(room.Type);
	//	//var symbol = RoomDisplayConfig.GetEventSymbol(room.RoomEvent?.Type.ToString() ?? "");

	//	//// Update display
	//	//Text = symbol;
	//	//Modulate = color;

	//	//if (isCurrent)
	//	//{
	//	//	AddThemeColorOverride("font_outline_color", Colors.White);
	//	//	AddThemeConstantOverride("outline_size", 2);
	//	//}
	//	//else if (isCompleted)
	//	//{
	//	//	Modulate = Modulate with { A = 0.5f }; // Dim completed rooms
	//	//}
	//}

	public void Clear()
	{
		Text = " ";
		Modulate = Colors.Gray;
		//_roomData = null;
	}

	private void ShowTooltip()
	{
		//if (_roomData == null) return;

		//_tooltipContent.GetChildren().ForEach(child => child.QueueFree());

		if (_isCompleted)
		{
			AddCompletedRoomTooltip();
		}
		else if (_isCurrent)
		{
			AddCurrentRoomTooltip();
		}
		else
		{
			AddUpcomingRoomTooltip();
		}

		_tooltipPanel.Visible = true;
		PositionTooltip();
	}

	private void AddCompletedRoomTooltip()
	{
		//AddTooltipSection("Completed: " + _roomData.Name);
		//AddTooltipSection(_roomData.Description);

		//if (_roomData.RoomEvent != null)
		//{
		//	AddTooltipSection("\nResults:");
		//	// Add event results (combat outcome, rewards gained, etc.)
		//	// This would come from room state data
		//}
	}

	private void AddCurrentRoomTooltip()
	{
		//AddTooltipSection("Current Room: " + _roomData.Name);
		//AddTooltipSection(_roomData.Description);

		//if (_roomData.RoomEvent != null)
		//{
		//	AddTooltipSection("\nEvent Details:");
		//	AddTooltipSection(GetEventDetails(_roomData.RoomEvent));
		//}

		//if (_roomData.Choices.Any())
		//{
		//	AddTooltipSection("\nChoices Available");
		//}
	}

	private void AddUpcomingRoomTooltip()
	{
		// Show only basic information for upcoming rooms
		//AddTooltipSection("Unknown Room");
		//AddTooltipSection("Approach to discover more...");
	}

	//private string GetEventDetails(RoomEvent evt)
	//{
	//	return evt.Type switch
	//	{
	//		EventType.Combat => GetCombatDetails(evt.Encounter),
	//		EventType.Reward => GetRewardDetails(evt.Reward),
	//		EventType.Rest => GetRestDetails(evt.Rest),
	//		_ => "Unknown event type"
	//	};
	//}

	//private string GetCombatDetails(EncounterData encounter)
	//{
	//	if (encounter == null) return "Combat information unavailable";
	//	return $"Combat Encounter\nDifficulty: {encounter.Difficulty:F1}";
	//}

	//private string GetRewardDetails(RewardData reward)
	//{
	//	if (reward == null) return "Reward information unavailable";
	//	return $"Reward: {reward.Type}";
	//}

	//private string GetRestDetails(RestData rest)
	//{
	//	if (rest == null) return "Rest information unavailable";
	//	return $"Rest Site\nHealing: {rest.HealAmount}";
	//}

	//private void AddTooltipSection(string text)
	//{
	//	var label = new Label { Text = text };
	//	_tooltipContent.AddChild(label);
	//}

	private void HideTooltip()
	{
		_tooltipPanel.Visible = false;
	}

	private void PositionTooltip()
	{
		var mousePos = GetViewport().GetMousePosition();
		_tooltipPanel.Position = new Vector2(
			mousePos.X + 10,
			mousePos.Y - _tooltipPanel.Size.Y - 10
		);
	}

}
