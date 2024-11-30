using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Manages tower instances and their states, including floor generation and tower progression.
/// Handles player interactions with towers and raises events for significant changes.
/// </summary>

public partial class TowerManager : BaseManager
{
	private Dictionary<string, TowerData> _towers = new();
	


	/// <summary>
	/// Sets up the TowerManager, including event registration and data loading.
	/// </summary>

	public override void Setup()
	{
		base.Setup();
		RegisterEventHandlers();
		InitializeSubManagers();
		LoadTowerData();
	}

	/// <summary>
	/// Registers event handlers for tower-related and floor-related events.
	/// Ensures the TowerManager listens for state changes, initialization, and floor generation events.
	/// </summary>

	protected override void RegisterEventHandlers()
	{

		EventManager.AddHandler<TowerEventArgs>(EventType.TowerStateChanged, HandleTowerStateChanged);

		// Handle initialization events for towers
		EventManager.AddHandler<TowerEventArgs>(EventType.TowerInitialized, HandleTowerInitialized);

		// Handle events when floors are generated for a tower
		EventManager.AddHandler<FloorEventArgs>(EventType.FloorsGenerated, HandleFloorsGenerated);

		// Handle initialization events for individual floors
		EventManager.AddHandler<FloorEventArgs>(EventType.FloorInitialized, HandleFloorInitialized);

		// Add other event handlers
	}

	private void InitializeSubManagers()
	{
		//_floorManagers = new Dictionary<string, FloorManager>();
	}

	/// <summary>
	/// Loads tower data from an external source or creates a sample tower if no data exists.
	/// This method is currently a placeholder for implementing persistent tower data storage.
	/// </summary>
	private void LoadTowerData()
	{
		// TODO: Implement loading from file or save system
		// Currently, a placeholder to create a sample tower for demonstration purposes

		// Create a sample tower with default configurations for testing
		CreateSampleTower();
	}
	/// <summary>
	/// Generates a specified number of floors for a given tower using the provided configuration.
	/// This method initializes the floor manager for the tower if necessary and handles floor generation logic.
	/// </summary>
	/// <param name="towerId">The ID of the tower for which floors are to be generated.</param>
	/// <param name="floorCount">The number of floors to generate.</param>
	/// <param name="config">
	/// Optional configuration for floor generation, such as difficulty scaling and default floor type.
	/// If no configuration is provided, a default configuration is created based on the tower's settings.
	/// </param>
	/// <returns>True if the floors are successfully generated; otherwise, false.</returns>
	public bool GenerateFloorsForTower(string towerId, int floorCount, FloorGenerationConfig config = null)
	{
		if (!_towers.TryGetValue(towerId, out var tower))
		{
			GD.PrintErr($"Tower {towerId} not found");
			return false;
		}

		try
		{
			// Generate floors using FloorManager
			bool success = Globals.Instance.gameMangers.World.Floors.GenerateFloorsForTower(tower, floorCount, config);

			if (success)
			{
				// Update tower's floor collection
				tower.Floors = Globals.Instance.gameMangers.World.Floors.GetFloorsForTower(towerId);

				EventManager.RaiseEvent(EventType.FloorsGenerated,
					FloorEventArgs.CreateGenerationComplete(towerId, floorCount, "Floor generation complete"));
			}

			return success;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error generating floors: {ex.Message}");
			return false;
		}
	}

	//public bool GenerateFloorsForTower(string towerId, int floorCount, FloorGenerationConfig config = null)
	//{
	//	// Ensure the tower exists in the tower manager
	//	if (!_towers.TryGetValue(towerId, out var tower))
	//	{
	//		GD.PrintErr($"Tower {towerId} not found"); // Log an error if the tower is not found
	//		return false; // Abort floor generation
	//	}

	//	// Check if the tower is in a valid state to generate floors
	//	if (tower.CurrentState != TowerState.Locked)
	//	{
	//		GD.PrintErr($"Cannot generate floors for tower {towerId} in state {tower.CurrentState}"); // Log an error
	//		return false; // Abort floor generation
	//	}

	//	// Initialize the floor manager for the tower if it does not already exist
	//	if (!_floorManagers.ContainsKey(towerId))
	//	{
	//		_floorManagers[towerId] = new FloorManager(towerId, EventManager); // Create a new floor manager
	//	}

	//	// Use the provided configuration or create a default configuration based on the tower's settings
	//	config ??= CreateDefaultConfig(tower);

	//	// Create a floor generator to handle the floor generation process
	//	var generator = new FloorGenerator(_floorManagers[towerId], tower);

	//	// Attempt to generate floors for the tower
	//	bool success = generator.GenerateFloorsForTower(floorCount, config);

	//	if (success)
	//	{
	//		// Retrieve the generated floors from the floor manager
	//		tower.Floors = _floorManagers[towerId].GetAllFloors().ToList();

	//		// Update the tower's unlocked floors based on the current state of each floor
	//		foreach (var floor in tower.Floors)
	//		{
	//			tower.UnlockedFloors[floor.Id] = floor.CurrentState == FloorState.Available;
	//		}

	//		// Raise an event indicating that floors have been successfully generated
	//		EventManager?.RaiseEvent(EventType.FloorsGenerated,
	//			FloorEventArgs.CreateGenerationComplete(towerId, floorCount, "Floor generation complete"));
	//	}

	//	return success; // Return whether floor generation was successful
	//}

	private FloorGenerationConfig CreateDefaultConfig(TowerData tower) => new()
	{
		BaseDifficulty = tower.Difficulty.BaseValue,
		DefaultFloorType = FloorType.None, // Could be derived from tower element
		DifficultyScalingFactor = tower.Difficulty.LevelScalingFactor
	};

	/// <summary>
	/// Creates a sample tower with predefined settings for testing and demonstration purposes.
	/// Includes default requirements, difficulty configurations, and floor generation logic.
	/// </summary>
	private void CreateSampleTower()
	{
		// Initialize a new tower with basic details
		var tower = new TowerData
		{
			Id = "tower_001",
			Name = "Tutorial Tower",
			Description = "A beginner-friendly tower to learn the basics",
			Version = "1.0.0",
			LastUpdated = DateTime.UtcNow,
			CurrentState = TowerState.Locked // Tower starts in the locked state
		};

		// Set up level requirements for entering the tower
		tower.Requirements = new LevelRequirement
		{
			MinLevel = 1, // Minimum level required to enter the tower
			MaxLevel = 10, // Maximum level allowed to enter the tower
			RecommendedLevel = 1, // Recommended level for balanced difficulty
			IsScaling = true, // Indicates if requirements scale dynamically
			ScalingFactor = 0.1f // Scaling factor for level-based requirements
		};

		// Configure the difficulty settings for the tower
		tower.Difficulty = new FloorDifficulty
		{
			BaseValue = 1.0f, // Base difficulty for floors in the tower
			ScalesWithLevel = true, // Indicates if difficulty scales with character level
			LevelScalingFactor = 0.15f, // Scaling factor for level-to-difficulty adjustments
			ReferenceLevel = 1, // The base level used for scaling calculations
			MinimumDifficulty = 0.5f, // The minimum difficulty floors can have
			MaximumDifficulty = 10.0f, // The maximum difficulty floors can reach
			ScalingFactor = 0.1f // Incremental scaling between floors
		};

		// Add a default difficulty modifier for level scaling
		tower.Difficulty.AddModifier(new DifficultyModifier(
			ModifierType.Multiplicative,
			1.1f, // Difficulty multiplier for level scaling
			"level_scaling"
		));

		// Add the tower to the manager's dictionary
		_towers[tower.Id] = tower;

		// Raise an event to indicate the tower has been initialized
		EventManager?.RaiseEvent(EventType.TowerInitialized,
			new TowerEventArgs(tower.Id, TowerState.Locked, TowerState.Locked));

		// Configure floor generation for the sample tower
		var floorConfig = new FloorGenerationConfig
		{
			BaseDifficulty = tower.Difficulty.BaseValue,
			DefaultFloorType = FloorType.None, // Default floor type for the tower
			DifficultyScalingFactor = tower.Difficulty.ScalingFactor,
			MilestoneDifficultySpikes = true, // Enable milestone difficulty spikes
			MilestoneDifficultyMultiplier = 1.5f // Apply a 1.5x multiplier for milestone floors
		};

		// Generate 100 floors for the sample tower
		GenerateFloorsForTower(tower.Id, 10000, floorConfig);
	}




	/// <summary>
	/// Attempts to enter a tower if the character meets the required conditions.
	/// This method checks if the tower exists, if the character is eligible to enter, 
	/// and triggers the entry process if all requirements are met.
	/// </summary>
	/// <param name="towerId">The ID of the tower the character is attempting to enter.</param>
	/// <returns>True if the character successfully enters the tower; otherwise, false.</returns>
	public bool TryEnterFloor(string towerId, string floorId, Character character)
	{
		return _towers.TryGetValue(towerId, out var tower) &&
			   Globals.Instance.gameMangers.World.Floors.TryEnterFloor(tower, floorId, character);

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
		EventManager?.RaiseEvent(EventType.TowerStateChanged,
			new TowerEventArgs(tower.Id, previousState, TowerState.Locked, "Tower manually locked"));

		return true; // Return true indicating the tower was successfully locked
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
		if (EventManager != null)
		{
			EventManager.RaiseEvent(EventType.TowerStateChanged,
				new TowerEventArgs(tower.Id, previousState, tower.CurrentState,
					$"Entered by character {character.Id}", difficulty)); // Event includes the tower ID, previous and new states, and difficulty
		}
	}

	/// <summary>
	/// Calculates the current difficulty of a tower based on the character's level, element, and the tower's difficulty configuration.
	/// Adjusts the difficulty according to the level difference between the character and the tower's recommended level.
	/// Also applies difficulty clamping to ensure it stays within the defined minimum and maximum values.
	/// </summary>
	/// <param name="towerId">The ID of the tower for which the difficulty is being calculated.</param>
	/// <returns>The calculated difficulty for the tower based on the current character's level and the tower's settings.</returns>
	public float GetTowerDifficulty(string towerId)
	{
		// Attempt to retrieve the tower data from the dictionary
		if (!_towers.TryGetValue(towerId, out var tower))
			return 0f; // Return 0 if the tower is not found

		// Retrieve the current character from the game manager
		var character = Globals.Instance?.gameMangers?.Player?.GetCurrentCharacter();

		// If no character is found, return the base difficulty of the tower
		if (character == null) return tower.Difficulty.BaseValue;

		// Calculate the level difference between the character's level and the tower's recommended level
		float levelDifference = character.Level - tower.Requirements.RecommendedLevel;

		// Apply the level difference to create a smoother scaling multiplier
		float levelMultiplier = 1 + (levelDifference * tower.Difficulty.LevelScalingFactor);

		// Get the base difficulty for the tower, including any scaling based on the character's level and element
		float difficulty = tower.Difficulty.CalculateFinalDifficulty(
			character.Level,
			character.Element
		);

		// Apply the level multiplier to adjust the difficulty based on the character's level
		difficulty *= levelMultiplier;

		// Store the raw difficulty before applying any clamping
		float rawDifficulty = difficulty;

		// Clamp the difficulty to ensure it stays within the defined minimum and maximum difficulty limits
		difficulty = Math.Clamp(difficulty,
			tower.Difficulty.MinimumDifficulty,
			tower.Difficulty.MaximumDifficulty);

		// Log if the difficulty was clamped to show the adjustment
		if (rawDifficulty != difficulty)
		{
			//GD.Print($"Difficulty clamped from {rawDifficulty:F2} to {difficulty:F2}");
		}

		// Return the final calculated difficulty
		return difficulty;
	}


	/// <summary>
	/// Handles the event when a tower's state changes.
	/// Updates the tower's state and logs the change.
	/// </summary>
	/// <param name="args">The event arguments containing details about the state change.</param>
	private void HandleTowerStateChanged(TowerEventArgs args)
	{
		// Attempt to find the tower in the _towers dictionary based on the tower ID in the event args
		if (_towers.TryGetValue(args.TowerId, out var tower))
		{
			// Update the tower's current state to the new state provided in the event arguments
			tower.CurrentState = args.NewState;

			// Log the state change
			DebugLogger.Log($"Tower {tower.Name} state changed from {args.PreviousState} to {args.NewState}", DebugLogger.LogCategory.Tower);
		}
	}
	/// <summary>
	/// Handles the event when a tower is initialized.
	/// Logs the initialization of the tower to confirm it has been initialized successfully.
	/// </summary>
	/// <param name="args">The event arguments containing information about the initialized tower.</param>
	private void HandleTowerInitialized(TowerEventArgs args)
	{
		// Attempt to retrieve the tower using the provided tower ID from the event arguments
		if (_towers.TryGetValue(args.TowerId, out var tower))
		{
			// Log the tower initialization message
			DebugLogger.Log($"Tower {tower.Name} initialized", DebugLogger.LogCategory.Tower);
		}
	}
	/// <summary>
	/// Retrieves all towers that match the specified state.
	/// If no state is provided, returns all towers.
	/// </summary>
	/// <param name="state">The state to filter towers by (e.g., Available, Locked). If null, all towers are returned.</param>
	/// <returns>A collection of towers that match the specified state.</returns>
	public IEnumerable<TowerData> GetTowersByState(TowerState? state = null)
	{
		// If a state is provided, filter towers by that state
		return state.HasValue
			? _towers.Values.Where(t => t.CurrentState == state.Value)
			: _towers.Values; // If no state is provided, return all towers
	}

	/// <summary>
	/// Retrieves all towers that are in the 'Available' state.
	/// </summary>
	/// <returns>A collection of towers that are currently available for interaction.</returns>
	public IEnumerable<TowerData> GetAvailableTowers()
	{
		return GetTowersByState(TowerState.Available); // Calls GetTowersByState with 'Available' state filter
	}

	public IEnumerable<TowerData> GetTowers()
	{
		// If a state is provided, filter towers by that state
		return _towers.Values;
			
	}

	private TowerData GetTowerByID(string towerId)
	{
		_towers.TryGetValue(towerId, out var tower);
		return tower;
	}

	/// <summary>
	/// Retrieves all towers 
	/// </summary>
	/// <returns>A collection of towers that exist</returns>
	public IEnumerable<TowerData> GetAllTowers()
	{
		return GetTowers(); // Calls GetTowersByState with 'Available' state filter
	}
	/// <summary>
	/// Retrieves all towers that are in the 'Locked' state.
	/// </summary>
	/// <returns>A collection of towers that are currently locked and cannot be entered.</returns>
	public IEnumerable<TowerData> GetLockedTowers()
	{
		return GetTowersByState(TowerState.Locked); // Calls GetTowersByState with 'Locked' state filter
	}

	private Character GetCurrentCharacter()
	{
		// This should be implemented to get the current character from the PlayerManager
		// For now, returning null as placeholder
		//Globals.Instance.gameMangers.Player.
		return null;
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
		if (!tower.Requirements.MeetsRequirements(character))
		{
			GD.PrintErr($"Character does not meet requirements for tower {towerId}"); // Log an error if the character doesn't meet the requirements
			LockTower(towerId); // Lock the tower again to maintain consistency
			return false; // Return false if the character does not meet the requirements
		}

		try
		{
			// Update the tower state to "Available" as the tower is now unlocked
			var previousState = tower.CurrentState;
			tower.CurrentState = TowerState.Available;

			// Raise an event to notify that the tower's state has changed to "Available"
			if (EventManager != null)
			{
				EventManager.RaiseEvent(EventType.TowerStateChanged,
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
		if (EventManager != null)
		{
			EventManager.RaiseEvent(EventType.TowerExited,
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

		EventManager.RaiseEvent(EventType.TowerFailed,
			new TowerEventArgs(
				tower.Id,
				previousState,
				TowerState.Failed,
				"Tower failed"
			));

		// Reset tower to Available state
		tower.CurrentState = TowerState.Available;
		EventManager.RaiseEvent(EventType.TowerStateChanged,
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
	//public bool CompleteFloor(string towerId, string floorId)
	//{
	//	// Attempt to retrieve the floor manager for the specified tower
	//	return _floorManagers.TryGetValue(towerId, out var floorManager) &&
	//		   floorManager.CompleteFloor(floorId); // Mark the specified floor as completed
	//}
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
	//public bool FailFloor(string towerId, string floorId)
	//{
	//	return _towers.TryGetValue(towerId, out var tower) &&
	//		   _floorManager.FailFloor(tower, floorId);
	//}

	//public bool FailFloor(string towerId, string floorId)
	//{
	//	// Attempt to retrieve the floor manager for the specified tower
	//	return _floorManagers.TryGetValue(towerId, out var floorManager) &&
	//		   floorManager.FailFloor(floorId); // Mark the specified floor as failed
	//}

	// Event handlers
	/// <summary>
	/// Handles the event when floors have been generated for a tower.
	/// If the tower is locked, it updates the tower's state to "Available" and raises an event to notify the system.
	/// </summary>
	/// <param name="args">The event arguments containing details about the floor generation.</param>
	private void HandleFloorsGenerated(FloorEventArgs args)
	{
		// Attempt to retrieve the tower using the FloorId (FloorId contains the towerId in this context)
		if (_towers.TryGetValue(args.TowerId, out var tower))
		{
			// Check if the tower is in the "Locked" state
			if (tower.CurrentState == TowerState.Locked)
			{
				// Store the previous state of the tower
				var previousState = tower.CurrentState;

				// Update the tower state to "Available" since floors have been generated
				tower.CurrentState = TowerState.Available;

				// Raise an event to notify the system that the tower's state has changed
				EventManager?.RaiseEvent(EventType.TowerStateChanged,
					new TowerEventArgs(tower.Id, previousState, TowerState.Available));
			}
		}
	}


	/// <summary>
	/// Resets the state of a specific floor in a tower to its initial state.
	/// This method is called when a floor's state needs to be cleared or reverted.
	/// </summary>
	/// <param name="towerId">The ID of the tower containing the floor.</param>
	/// <param name="floorId">The ID of the floor whose state is to be reset.</param>
	/// <returns>True if the floor's state was successfully reset; false otherwise.</returns>
	public bool ResetFloorState(string towerId, string floorId)
	{
		return _towers.TryGetValue(towerId, out var tower) &&
			   Globals.Instance.gameMangers.World.Floors.ResetFloorState(tower, floorId);
	}
	//public bool ResetFloorState(string towerId, string floorId)
	//{
	//	// Attempt to retrieve the floor manager for the specified tower
	//	return _floorManagers.TryGetValue(towerId, out var floorManager) &&
	//		   floorManager.ResetFloorState(floorId); // Reset the floor's state using the floor manager
	//}

	/// <summary>
	/// Handles the event when an individual floor is initialized.
	/// This method can be extended to perform additional actions when a floor is initialized.
	/// </summary>
	/// <param name="args">The event arguments containing details about the floor initialization.</param>
	private void HandleFloorInitialized(FloorEventArgs args)
	{
		// Additional handling could be added here if needed when individual floors are initialized
		// For now, we log the floor initialization for debugging purposes
		//GD.Print($"Floor initialized in tower {args.FloorId} with number {args.FloorNumber}");
	}



	internal FloorState GetFloorState(string TowerID, string floorID)
	{
		return GetTowerByID(TowerID).Floors.Where(p=>p.Id == floorID).FirstOrDefault().CurrentState;
	}
}