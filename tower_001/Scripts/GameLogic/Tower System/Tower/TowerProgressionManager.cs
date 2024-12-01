using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Manages tower progression, including entering/exiting towers and handling tower completion or failure.
/// </summary>
public class TowerProgressionManager
{
    private readonly EventManager _eventManager;
    private readonly Dictionary<string, TowerData> _towers;

    public TowerProgressionManager(
        EventManager eventManager,
        Dictionary<string, TowerData> towers)
    {
        _eventManager = eventManager;
        _towers = towers;
    }

    /// <summary>
    /// Checks if the character meets the requirements to enter the specified tower.
    /// Validates the tower's state and the character's level against the tower's requirements.
    /// </summary>
    /// <param name="tower">The tower the character is attempting to enter.</param>
    /// <param name="character">The character attempting to enter the tower.</param>
    /// <returns>True if the character meets the requirements to enter the tower; otherwise, false.</returns>
    private bool CanEnterTower(TowerData tower, Character character)
    {
        // Check if the tower is in the available state, which means it can be entered
        if (tower.CurrentState != TowerState.Available)
        {
            GD.PrintErr($"Tower {tower.Id} is not available (Current State: {tower.CurrentState})"); // Log an error if the tower is not available
            return false; // Return false if the tower is not in the available state
        }

        //// Debug output to check the character's level and tower requirements
        DebugLogger.Log($"Checking requirements for tower {tower.Id}:", DebugLogger.LogCategory.Tower);
        DebugLogger.Log($"Character Level: {character.Level}", DebugLogger.LogCategory.Tower);
        DebugLogger.Log($"Required Level Range: {tower.Requirements.MinLevel} - {tower.Requirements.MaxLevel}", DebugLogger.LogCategory.Tower);

        // Check if the character's level is lower than the minimum required level for the tower
        if (character.Level < tower.Requirements.MinLevel)
        {
            GD.PrintErr($"Character level too low: {character.Level} < {tower.Requirements.MinLevel}"); // Log an error if the character's level is too low
            return false; // Return false if the character's level is insufficient
        }

        return true; // Return true if the character meets the tower's requirements
    }

    /// <summary>
    /// Handles the process of entering the tower, including updating the tower's state and calculating the difficulty.
    /// This method transitions the tower's state to "InProgress" and raises an event notifying of the entry.
    /// </summary>
    /// <param name="tower">The tower the character is entering.</param>
    /// <param name="character">The character entering the tower.</param>
    private void EnterTower(TowerData tower, Character character)
    {
        // Store the previous state of the tower for event purposes
        var previousState = tower.CurrentState;

        // Update the tower's state to "InProgress" to signify that the character is now in the tower
        tower.CurrentState = TowerState.InProgress;

        // Record the time of the character's attempt to enter the tower
        tower.LastAttemptTime = DateTime.UtcNow;

        // Calculate the final difficulty of the tower based on the character's level and elemental affinity
        float difficulty = tower.Difficulty.CalculateFinalDifficulty(
            character.Level,
            character.Element
        );

        //// Log the difficulty and the state change
        DebugLogger.Log($"Tower {tower.Id} entered with difficulty: {difficulty}", DebugLogger.LogCategory.Tower);
        DebugLogger.Log($"State changed from {previousState} to {tower.CurrentState}", DebugLogger.LogCategory.Tower);

        // Raise an event indicating the tower's state has changed and it has been entered by the character
        if (_eventManager != null)
        {
            _eventManager.RaiseEvent(EventType.TowerStateChanged,
                new TowerEventArgs(tower.Id, previousState, tower.CurrentState,
                    $"Entered by character {character.Id}", difficulty)); // Event includes the tower ID, previous and new states, and difficulty
        }
    }

    /// <summary>
    /// Allows a character to exit a tower and returns the tower to the "Available" state.
    /// This method is called when the player exits the tower normally.
    /// </summary>
    /// <param name="towerId">The ID of the tower the player is exiting.</param>
    /// <returns>True if the player successfully exits the tower; false otherwise.</returns>
    public bool ExitTower(string towerId)
    {
        // Check if the tower exists in the _towers dictionary
        if (!_towers.TryGetValue(towerId, out var tower))
        {
            GD.PrintErr($"Tower {towerId} not found"); // Log an error if the tower is not found
            return false; // Return false if the tower does not exist
        }

        // Check if the tower is currently in the "InProgress" state, indicating the player is inside
        if (tower.CurrentState != TowerState.InProgress)
        {
            GD.PrintErr($"Tower {towerId} is not in progress"); // Log an error if the tower is not in progress
            return false; // Return false if the tower is not in progress
        }

        // Store the previous state of the tower for event purposes
        var previousState = tower.CurrentState;
        tower.CurrentState = TowerState.Available; // Set the tower state to "Available" upon exit

        // Raise an event to notify that the player has exited the tower
        if (_eventManager != null)
        {
            _eventManager.RaiseEvent(EventType.TowerExited,
                new TowerEventArgs(
                    tower.Id,
                    previousState,
                    TowerState.Available,
                    "Player exited tower" // Message indicating the player exited the tower
                ));
        }

        return true; // Return true indicating the player has successfully exited the tower
    }

    /// <summary>
    /// Handles the failure of a tower. Changes the tower's state to "Failed" and resets all its floors.
    /// This method is called when the player fails a tower, and the tower must be reset.
    /// </summary>
    /// <param name="towerId">The ID of the tower that failed.</param>
    /// <returns>True if the tower was successfully marked as failed and reset; false otherwise.</returns>
    public bool FailTower(string towerId)
    {
        if (!_towers.TryGetValue(towerId, out var tower))
        {
            GD.PrintErr($"Tower {towerId} not found");
            return false;
        }

        var previousState = tower.CurrentState;
        tower.CurrentState = TowerState.Failed;

        // Reset all floors through FloorManager
        foreach (var floor in tower.Floors)
        {
            Globals.Instance.gameMangers.World.Floors.ResetFloorState(tower, floor.Id);
        }

        _eventManager.RaiseEvent(EventType.TowerFailed,
            new TowerEventArgs(
                tower.Id,
                previousState,
                TowerState.Failed,
                "Tower failed"
            ));

        // Reset tower to Available state
        tower.CurrentState = TowerState.Available;
        _eventManager.RaiseEvent(EventType.TowerStateChanged,
            new TowerEventArgs(
                tower.Id,
                TowerState.Failed,
                TowerState.Available,
                "Tower reset after failure"
            ));

        return true;
    }

    /// <summary>
    /// Marks a floor as completed for a specified tower.
    /// This method is called when a character successfully completes a floor.
    /// </summary>
    /// <param name="towerId">The ID of the tower containing the floor.</param>
    /// <param name="floorId">The ID of the floor to mark as completed.</param>
    /// <returns>True if the floor was successfully marked as completed; false otherwise.</returns>
    public bool CompleteFloor(string towerId, string floorId)
    {
        return _towers.TryGetValue(towerId, out var tower) &&
               Globals.Instance.gameMangers.World.Floors.CompleteFloor(tower, floorId);
    }

    /// <summary>
    /// Marks a floor as failed for a specified tower.
    /// This method is called when a character fails a floor and the floor needs to be reset.
    /// </summary>
    /// <param name="towerId">The ID of the tower containing the floor.</param>
    /// <param name="floorId">The ID of the floor to mark as failed.</param>
    /// <returns>True if the floor was successfully marked as failed; false otherwise.</returns>
    public bool FailFloor(string towerId, string floorId)
    {
        return _towers.TryGetValue(towerId, out var tower) &&
               Globals.Instance.gameMangers.World.Floors.FailFloor(tower, floorId);
    }

    public bool TryEnterTower(string towerId)
    {
        // Check if the tower exists in the _towers dictionary
        if (!_towers.TryGetValue(towerId, out var tower))
        {
            GD.PrintErr($"Tower {towerId} not found"); // Log error if tower not found
            return false; // Return false if tower doesn't exist
        }

        // Get the current character from the game manager
        var character = Globals.Instance?.gameMangers?.Player?.GetCurrentCharacter();
        if (character == null)
        {
            GD.PrintErr("No active character found"); // Log error if no active character
            return false; // Return false if no character is selected
        }
        DebugLogger.Log($"Attempting to enter tower {towerId} with character level {character.Level}", DebugLogger.LogCategory.Tower);

        // Check if the character meets the requirements to enter the tower
        if (!CanEnterTower(tower, character))
        {
            return false; // Return false if character can't enter the tower
        }

        // Proceed to enter the tower if all checks pass
        EnterTower(tower, character);
        return true; // Return true if the character successfully enters the tower
    }

	/// <summary>
	/// Attempts to unlock a tower if the character meets the necessary requirements.
	/// Changes the tower's state to "Available" and raises an event notifying of the state change.
	/// </summary>
	/// <param name="towerId">The ID of the tower to unlock.</param>
	/// <returns>True if the tower was successfully unlocked; false otherwise.</returns>
	public bool UnlockTower(string towerId)
	{
		// Check if the tower exists in the _towers dictionary
		if (!_towers.TryGetValue(towerId, out var tower))
		{
			GD.PrintErr($"Tower {towerId} not found"); // Log an error if the tower is not found
			return false; // Return false if the tower does not exist
		}

		// Check if the tower is in the "Locked" state, as only locked towers can be unlocked
		if (tower.CurrentState != TowerState.Locked)
		{
			GD.PrintErr($"Tower {towerId} is not in locked state"); // Log an error if the tower is not in the locked state
			return false; // Return false if the tower is not locked
		}

		// Retrieve the current character from the game manager
		var character = Globals.Instance?.gameMangers?.Player?.GetCurrentCharacter();
		if (character == null)
		{
			GD.PrintErr("No active character found"); // Log an error if no active character is found
			LockTower(towerId); // Lock the tower again to maintain consistency
			return false; // Return false if no character is available
		}

		// Check if the character meets the tower's requirements to enter
		//if (!tower.Requirements.MeetsRequirements(character))
		//{
		//	GD.PrintErr($"Character does not meet requirements for tower {towerId}"); // Log an error if the character doesn't meet the requirements
		//	LockTower(towerId); // Lock the tower again to maintain consistency
		//	return false; // Return false if the character does not meet the requirements
		//}

		try
		{
			// Update the tower state to "Available" as the tower is now unlocked
			var previousState = tower.CurrentState;
			tower.CurrentState = TowerState.Available;

			// Raise an event to notify that the tower's state has changed to "Available"
			if (_eventManager != null)
			{
				_eventManager.RaiseEvent(EventType.TowerStateChanged,
					new TowerEventArgs(tower.Id, previousState, TowerState.Available));
			}

			return true; // Return true if the tower was successfully unlocked
		}
		catch (Exception ex)
		{
			// Handle any errors that occur during the unlocking process
			GD.PrintErr($"Error unlocking tower: {ex.Message}"); // Log the error message
			LockTower(towerId); // Lock the tower again to maintain consistency
			return false; // Return false if an error occurred
		}
	}

	/// <summary>
	/// Locks a tower, preventing entry and resetting its floors.
	/// This method sets the tower state to "Locked" and resets all floor states within the tower.
	/// </summary>
	/// <param name="towerId">The ID of the tower to lock.</param>
	/// <returns>True if the tower was successfully locked; otherwise, false.</returns>
	public bool LockTower(string towerId)
	{
		// Check if the tower exists in the _towers dictionary
		if (!_towers.TryGetValue(towerId, out var tower))
		{
			GD.PrintErr($"Tower {towerId} not found"); // Log error if tower is not found
			return false; // Return false if tower is not found
		}

		// Store the previous state of the tower for event purposes
		var previousState = tower.CurrentState;
		tower.CurrentState = TowerState.Locked; // Change the state of the tower to "Locked"


		foreach (var floor in tower.Floors)
		{
			Globals.Instance.gameMangers.World.Floors.ResetFloorState(tower, floor.Id);
		}

		//// Lock all floors associated with the tower
		//if (_floorManagers.TryGetValue(towerId, out var floorManager))
		//{
		//	foreach (var floor in tower.Floors)
		//	{
		//		floorManager.ResetFloorState(floor.Id); // Reset the state of each floor
		//	}
		//}

		// Raise an event indicating the tower's state has changed
		_eventManager.RaiseEvent(EventType.TowerStateChanged,
			new TowerEventArgs(tower.Id, previousState, TowerState.Locked, "Tower manually locked"));

		return true; // Return true indicating the tower was successfully locked
	}
}
