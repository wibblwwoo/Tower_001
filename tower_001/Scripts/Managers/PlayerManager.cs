using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Globalization;

public partial class PlayerManager : BaseManager
{

	private readonly Dictionary<string, Character> _characters = new();

	private Character _currentCharacter;
	public ProgressionManager progressionManager { get; set; }

	public PlayerManager()
	{
		
	}
	public override void Setup()
	{
		base.Setup(); 
	}

	protected override void RegisterEventHandlers()
	{
		// Register character-related event handlers
		

		progressionManager = new ProgressionManager(this);
		progressionManager.Setup();



		EventManager.AddHandler<CharacterStatEventArgs>(EventType.CharacterStatChanged, OnCharacterStatChanged);

		EventManager.AddHandler<CharacterStatBuffEventArgs>(EventType.CharacterStatBuffApplied, OnCharacterStatBuffApplied);
		// Add more event handlers as needed

		// Register stat change handler
		EventManager.AddHandler<CharacterStatEventArgs>(
			EventType.CharacterStatChanged,
			OnCharacterStatChanged
		);

		// Register buff handlers
		EventManager.AddHandler<CharacterStatBuffEventArgs>(
			EventType.CharacterStatBuffApplied,
			OnCharacterStatBuffApplied
		);
		EventManager.AddHandler<CharacterStatBuffEventArgs>(
			EventType.CharacterStatBuffRemoved,
			OnCharacterStatBuffRemoved
		);
		EventManager.AddHandler<CharacterStatBuffEventArgs>(
			EventType.CharacterStatBuffExpired,
			OnCharacterStatBuffExpired
		);

		// Register threshold handler
		//events.AddHandler<CharacterStatThresholdEventArgs>(
		//	EventType.CharacterStatThresholdCrossed,
		//	OnCharacterStatThresholdCrossed
		//);
	}

	/// <summary>
	/// Retrieves a character from the character manager by their unique ID.
	/// </summary>
	/// <param name="characterId">The ID of the character to retrieve.</param>
	/// <returns>The character object if found, or null if no character matches the provided ID.</returns>
	public Character GetCharacter(string characterId)
	{
		// Attempt to retrieve the character from the dictionary using their ID.
		// If the character exists, return it; otherwise, return null.
		return _characters.TryGetValue(characterId, out var character) ? character : null;
	}

	/// <summary>
	/// Creates a new character based on the specified class name and adds it to the character manager.
	/// </summary>
	/// <param name="className">The class name of the character to create (e.g., "knight", "mage").</param>
	/// <returns>The unique ID of the newly created character.</returns>
	/// <exception cref="ArgumentException">Thrown if an invalid class name is provided.</exception>
	public string CreateCharacter(string className)
	{
		// Create a new character instance based on the provided class name.
		// Throws an exception if the class name is not recognized.
		Character character = className.ToLower() switch
		{
			"knight" => new Knight(), // Create a new Knight instance
			"mage" => new Mage(),     // Create a new Mage instance
			_ => throw new ArgumentException($"Unknown character class: {className}") // Handle invalid class names
		};

		// Add the character to the dictionary using their unique ID as the key.
		_characters[character.Id] = character;

		// Return the unique ID of the newly created character.
		return character.Id;
	}

	/// <summary>
	/// Core logic for applying a buff to a character's stat.
	/// </summary>
	private void ApplyBuffInternal(Character character,StatType statType,StatModifier modifier)
	{
		var stat = character.GetStat(statType);
		stat.AddModifier(modifier);
	}

	/// <summary>
	/// Applies a buff or modifier to a specific character's stat.
	/// </summary>
	/// <param name="characterId">The ID of the character to apply the buff to.</param>
	/// <param name="statType">The type of stat to modify (e.g., health, attack).</param>
	/// <param name="source">The source of the buff (e.g., an ability or item).</param>
	/// <param name="buffType">The type of buff (e.g., flat increase, percentage increase).</param>
	/// <param name="value">The value of the buff to apply.</param>
	/// <param name="duration">Optional duration for the buff. If null, the buff is permanent.</param>
	public void ApplyBuff(string characterId,StatType statType,string source,BuffType buffType,float value,TimeSpan? duration = null)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Retrieve the specified stat from the character
			var stat = character.GetStat(statType);

			// Create a new stat modifier with a unique ID, source, type, value, and optional duration
			var modifier = new StatModifier(
				Guid.NewGuid().ToString(), // Generate a unique ID for the modifier
				source,                    // Set the source of the modifier
				buffType,                  // Set the type of buff (e.g., flat, percentage)
				value,                     // Set the buff value
				duration                   // Set the optional duration
			);

			// Add the modifier to the character's stat
			//stat.AddModifier(modifier);
			ApplyBuffInternal(character, statType, modifier);
		}
	}

	/// <summary>
	/// Applies a buff or modifier to a specific character's stat, allowing for a custom buff ID.
	/// </summary>
	/// <param name="characterId">The ID of the character to apply the buff to.</param>
	/// <param name="statType">The type of stat to modify (e.g., health, attack).</param>
	/// <param name="source">The source of the buff (e.g., an ability or item).</param>
	/// <param name="buffType">The type of buff (e.g., flat increase, percentage increase).</param>
	/// <param name="value">The value of the buff to apply.</param>
	/// <param name="buffId">The unique ID of the buff being applied.</param>
	/// <param name="duration">Optional duration for the buff. If null, the buff is permanent.</param>
	public void ApplyBuff(string characterId,StatType statType,string source,BuffType buffType,float value,string buffId,TimeSpan? duration = null)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Retrieve the specified stat from the character
			var stat = character.GetStat(statType);

			// Create a new stat modifier with the provided buff ID, source, type, value, and optional duration
			var modifier = new StatModifier(
				buffId,    // Use the provided buff ID
				source,    // Set the source of the modifier
				buffType,  // Set the type of buff (e.g., flat, percentage)
				value,     // Set the buff value
				duration   // Set the optional duration
			);

			// Add the modifier to the character's stat
			ApplyBuffInternal(character, statType, modifier);
			//stat.AddModifier(modifier);
		}
	}

	/// <summary>
	/// Removes a specific buff from a character's stat using the buff's unique ID.
	/// </summary>
	/// <param name="characterId">The ID of the character whose buff should be removed.</param>
	/// <param name="statType">The type of stat from which the buff should be removed (e.g., health, attack).</param>
	/// <param name="buffId">The unique ID of the buff to remove.</param>
	public void RemoveBuff(string characterId, StatType statType, string buffId)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Retrieve the specified stat from the character
			var stat = character.GetStat(statType);

			// Remove the modifier (buff) from the stat using its unique ID
			stat.RemoveModifier(buffId);
		}
	}

	/// <summary>
	/// Updates all characters, processing any expired buffs or other time-sensitive changes.
	/// This method should be called periodically (e.g., every game tick or frame).
	/// </summary>
	public void Update()
	{
		// Iterate through all characters in the dictionary
		foreach (var character in _characters.Values)
		{
			// Update each character, allowing them to process expired buffs and other updates
			character.Update();
		}
	}

	/// <summary>
	/// Handles the event when a character's stat changes.
	/// Logs the stat change for debugging or display purposes.
	/// </summary>
	/// <param name="args">Event arguments containing details about the stat change (character ID, stat type, old value, new value).</param>
	private void OnCharacterStatChanged(CharacterStatEventArgs args)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(args.CharacterId, out var character))
		{
			// Log the character's stat change with details about the old and new values
			DebugLogger.Log(
				$"{character.Name} ({args.CharacterId}): {args.StatType} changed from {args.OldValue:F1} to {args.NewValue:F1}",
				DebugLogger.LogCategory.Stats // Categorize the log entry under "Stats"
			);
		}
	}
	/// <summary>
	/// Retrieves a dictionary of all stats and their current values for a specific character.
	/// </summary>
	/// <param name="characterId">The ID of the character whose stats should be retrieved.</param>
	/// <returns>
	/// A dictionary where keys are stat types (e.g., health, attack) and values are the current values of those stats.
	/// Returns an empty dictionary if the character is not found.
	/// </returns>
	public Dictionary<StatType, float> GetCharacterStats(string characterId)
	{
		// Check if the character exists in the dictionary
		if (!_characters.TryGetValue(characterId, out var character))
		{
			// Return an empty dictionary if the character does not exist
			return new Dictionary<StatType, float>();
		}

		// Generate a dictionary of all stat types and their current values for the character
		return Enum.GetValues<StatType>() // Get all stat types defined in the StatType enum
			.ToDictionary(
				statType => statType, // Key: The stat type
				statType => character.GetStat(statType)?.CurrentValue ?? 0 // Value: The stat's current value or 0 if the stat is null
			);
	}

	/// <summary>
	/// Handles the event when a stat buff is applied to a character.
	/// Logs the details of the applied buff for debugging or tracking purposes.
	/// </summary>
	/// <param name="args">The event arguments containing details about the buff application (character ID, stat type, and modifier).</param>
	private void OnCharacterStatBuffApplied(CharacterStatBuffEventArgs args)
	{
		// Log the buff application with details about the character, buff value, affected stat, and the source of the buff
		DebugLogger.Log(
			$"Character {args.CharacterId} received {args.Modifier.Value} {args.StatType} buff from {args.Modifier.Source}",
			DebugLogger.LogCategory.Stats // Categorize the log entry under "Stats"
		);
	}

	/// <summary>
	/// Prints all the stats of a specific character to the debug console.
	/// </summary>
	/// <param name="characterId">The ID of the character whose stats should be printed.</param>
	public void PrintCharacterStats(string characterId)
	{
		// Check if the character exists in the dictionary
		if (!_characters.TryGetValue(characterId, out var character))
			return; // Exit if the character does not exist

		// Print the character's name and ID
		GD.Print($"=== {character.Name} ({characterId}) Stats ===");

		// Iterate through each stat and print its current value
		foreach (var stat in GetCharacterStats(characterId))
		{
			GD.Print($"{stat.Key}: {stat.Value:F1}"); // Format the value to 1 decimal place
		}
	}

	/// <summary>
	/// Retrieves the current value of a specific stat for the currently active character.
	/// </summary>
	/// <param name="statType">The type of stat to retrieve (e.g., health, attack).</param>
	/// <returns>The current value of the specified stat, or 0 if the stat or character is not found.</returns>
	public float GetStatValue(StatType statType)
	{
		// Safely retrieve the stat value from the current character or return 0 if unavailable
		return _currentCharacter?.GetStat(statType)?.CurrentValue ?? 0;
	}


	// Helper method to get all buffs on a character
	/// <summary>
	/// Retrieves all active stat modifiers (buffs) for a specific character's stat.
	/// </summary>
	/// <param name="characterId">The ID of the character whose stat modifiers should be retrieved.</param>
	/// <param name="statType">The type of stat to retrieve modifiers for (e.g., health, attack).</param>
	/// <returns>A read-only list of active stat modifiers for the specified stat, or an empty list if the character or stat is not found.</returns>
	public IReadOnlyList<StatModifier> GetStatModifiers(string characterId, StatType statType)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Retrieve the specified stat from the character
			var stat = character.GetStat(statType);

			// Return all active modifiers (buffs) for the stat
			return stat.GetActiveModifiers();
		}

		// Return an empty list if the character or stat is not found
		return new List<StatModifier>();
	}

	/// <summary>
	/// Sets the current active character by their ID
	/// </summary>
	/// <param name="characterId">The ID of the character to set as active</param>
	/// <returns>True if character was found and set as active, false otherwise</returns>
	public bool SetCurrentCharacter(string characterId)
	{
		if (_characters.TryGetValue(characterId, out var character))
		{
			_currentCharacter = character;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Gets the currently active character
	/// </summary>
	/// <returns>The current character or null if none is set</returns>
	public Character GetCurrentCharacter()
	{
		return _currentCharacter;
	}

	/// <summary>
	/// Sets the level of a specific character by calling their LevelUp method.
	/// </summary>
	/// <param name="characterId">The ID of the character whose level should be set.</param>
	/// <param name="level">The new level to set for the character.</param>
	/// <returns>True if the character was found and their level was set, false otherwise.</returns>
	public bool SetCharacterLevel(string characterId, int level)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Call the character's LevelUp method to set the new level
			character.LevelUp(level);

			// Return true to indicate the level was successfully set
			return true;
		}

		// Return false if the character does not exist
		return false;
	}


	/// <summary>
	/// Raises a stat-related event for a character.
	/// </summary>
	private void RaiseCharacterStatEvent(string characterId,StatType statType,float oldValue,float newValue,EventType eventType){
		if (Globals.Instance?.gameMangers?.Events != null)
		{
			var args = new CharacterStatEventArgs(characterId, statType, oldValue, newValue);
			Globals.Instance.gameMangers.Events.RaiseEvent(eventType, args);
		}
	}

	/// <summary>
	/// Sets the elemental type of a specific character and raises an event to notify about the change.
	/// </summary>
	/// <param name="characterId">The ID of the character whose element is being set.</param>
	/// <param name="element">The new elemental type to assign to the character.</param>
	/// <returns>True if the character was found and the element was successfully set, false otherwise.</returns>
	public bool SetCharacterElement(string characterId, ElementType element)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(characterId, out var character))
		{
			// Store the old element type for event logging
			ElementType oldElement = character.Element;

			// Set the new element type for the character
			character.Element = element;

			// Using Health as a proxy stat for element change
			RaiseCharacterStatEvent(characterId,StatType.Health, (float)oldElement,(float)element,EventType.CharacterStatChanged);

			// Raise an event if the event manager is available
			//if (Globals.Instance?.gameMangers?.Events != null)
			//{
			//	// Create an event argument to describe the element change
			//	var args = new CharacterStatEventArgs(
			//		characterId,
			//		StatType.Health, // Using Health as a proxy stat for element change
			//		(float)oldElement,
			//		(float)element
			//	);

			//	// Raise the event to notify about the stat change
			//	Globals.Instance.gameMangers.Events.RaiseEvent(EventType.CharacterStatChanged, args);
			//}

			// Return true to indicate the element was successfully set
			return true;
		}

		// Return false if the character does not exist
		return false;
	}

	/// <summary>
	/// Handles the event when a stat buff is removed from a character.
	/// Logs the details of the removed buff for debugging or tracking purposes.
	/// </summary>
	/// <param name="args">The event arguments containing details about the removed buff.</param>
	private void OnCharacterStatBuffRemoved(CharacterStatBuffEventArgs args)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(args.CharacterId, out var character))
		{
			// Log the removal of the buff with details about the character, stat type, and source of the buff
			DebugLogger.Log(
				$"{character.Name} had {args.StatType} buff from {args.Modifier.Source} removed",
				DebugLogger.LogCategory.Stats // Categorize the log entry under "Stats"
			);
		}
	}
	/// <summary>
	/// Handles the event when a stat buff expires for a character.
	/// Logs the expiration details for debugging or tracking purposes.
	/// </summary>
	/// <param name="args">The event arguments containing details about the expired buff.</param>
	private void OnCharacterStatBuffExpired(CharacterStatBuffEventArgs args)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(args.CharacterId, out var character))
		{
			// Log the expiration of the buff with details about the character, stat type, and source of the buff
			DebugLogger.Log(
				$"{character.Name} {args.StatType} buff from {args.Modifier.Source} expired",
				DebugLogger.LogCategory.Stats // Categorize the log entry under "Stats"
			);
		}
	}

	/// <summary>
	/// Handles the event when a character's stat crosses a defined threshold.
	/// Logs the threshold crossing details for debugging or tracking purposes.
	/// </summary>
	/// <param name="args">The event arguments containing details about the stat threshold crossing.</param>
	private void OnCharacterStatThresholdCrossed(CharacterStatThresholdEventArgs args)
	{
		// Check if the character exists in the dictionary
		if (_characters.TryGetValue(args.CharacterId, out var character))
		{
			// Log the threshold crossing with details about the character, stat type, threshold, and current value
			DebugLogger.Log(
				$"{character.Name} {args.StatType} crossed {args.Threshold}% threshold. Current value: {args.CurrentValue:F1}",
				DebugLogger.LogCategory.Stats // Categorize the log entry under "Stats"
			);
		}
	}

}
