using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for general character-related events.
/// Provides the unique identifier of the character involved in the event.
/// </summary>
public class CharacterEventArgs : GameEventArgs
{
	public string CharacterId { get; set; }

	public CharacterEventArgs(string characterId)
	{
		CharacterId = characterId;
	}
}
/// <summary>
/// Event arguments for when a character's stat changes.
/// Provides details about the stat type, old value, and new value.
/// </summary>
public class CharacterStatEventArgs : CharacterEventArgs
{
	/// <summary>
	/// The type of stat that changed (e.g., health, attack, defense).
	/// </summary>
	public StatType StatType { get; set; }

	/// <summary>
	/// The value of the stat before the change occurred.
	/// </summary>
	public float OldValue { get; set; }

	/// <summary>
	/// The value of the stat after the change occurred.
	/// </summary>
	public float NewValue { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CharacterStatEventArgs"/> class.
	/// </summary>
	/// <param name="characterId">The ID of the character whose stat changed.</param>
	/// <param name="statType">The type of stat that changed.</param>
	/// <param name="oldValue">The value of the stat before the change.</param>
	/// <param name="newValue">The value of the stat after the change.</param>
	public CharacterStatEventArgs(
		string characterId,
		StatType statType,
		float oldValue,
		float newValue) : base(characterId) // Call base constructor with character ID
	{
		StatType = statType; // Assign the type of stat that changed
		OldValue = oldValue; // Set the old value of the stat
		NewValue = newValue; // Set the new value of the stat
	}
}
/// <summary>
/// Event arguments for the application or removal of a stat buff on a character.
/// Includes details about the stat type and the specific modifier being applied or removed.
/// </summary>
public class CharacterStatBuffEventArgs : CharacterEventArgs
{
	/// <summary>
	/// The type of stat affected by the buff (e.g., health, attack, defense).
	/// </summary>
	public StatType StatType { get; }

	/// <summary>
	/// The modifier being applied or removed from the stat.
	/// </summary>
	public StatModifier Modifier { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CharacterStatBuffEventArgs"/> class.
	/// </summary>
	/// <param name="characterId">The ID of the character affected by the buff.</param>
	/// <param name="statType">The type of stat affected by the buff.</param>
	/// <param name="modifier">The modifier being applied or removed.</param>
	public CharacterStatBuffEventArgs(string characterId,StatType statType,StatModifier modifier) : base(characterId) // Call base constructor with character ID
	{
		StatType = statType; // Assign the type of stat affected by the buff
		Modifier = modifier; // Assign the modifier being applied or removed
	}
}
/// <summary>
/// Event arguments for when a character's stat crosses a predefined threshold.
/// Provides details about the stat type, the threshold crossed, and the current value.
/// </summary>
public class CharacterStatThresholdEventArgs : CharacterEventArgs
{
	/// <summary>
	/// The type of stat that crossed the threshold (e.g., health, attack, defense).
	/// </summary>
	public StatType StatType { get; }

	/// <summary>
	/// The threshold value that was crossed.
	/// </summary>
	public float Threshold { get; }

	/// <summary>
	/// The current value of the stat after crossing the threshold.
	/// </summary>
	public float CurrentValue { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CharacterStatThresholdEventArgs"/> class.
	/// </summary>
	/// <param name="characterId">The ID of the character whose stat crossed the threshold.</param>
	/// <param name="statType">The type of stat that crossed the threshold.</param>
	/// <param name="threshold">The threshold value that was crossed.</param>
	/// <param name="currentValue">The current value of the stat after crossing the threshold.</param>
	public CharacterStatThresholdEventArgs(string characterId,StatType statType,float threshold,float currentValue) : base(characterId) // Call base constructor with character ID
	{
		StatType = statType; // Assign the type of stat that crossed the threshold
		Threshold = threshold; // Set the threshold value that was crossed
		CurrentValue = currentValue; // Set the current value of the stat
	}
}
/// <summary>
/// Event arguments for when a character's level changes.
/// Provides details about the character's old level and new level.
/// </summary>
public class CharacterLevelEventArgs : CharacterEventArgs
{
	/// <summary>
	/// The character's level before the change.
	/// </summary>
	public int OldLevel { get; }

	/// <summary>
	/// The character's level after the change.
	/// </summary>
	public int NewLevel { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CharacterLevelEventArgs"/> class.
	/// </summary>
	/// <param name="characterId">The ID of the character whose level changed.</param>
	/// <param name="oldLevel">The character's level before the change.</param>
	/// <param name="newLevel">The character's level after the change.</param>
	public CharacterLevelEventArgs(string characterId,int oldLevel,int newLevel) : base(characterId) // Call base constructor with the character ID
	{
		OldLevel = oldLevel; // Assign the character's old level
		NewLevel = newLevel; // Assign the character's new level
	}
}

