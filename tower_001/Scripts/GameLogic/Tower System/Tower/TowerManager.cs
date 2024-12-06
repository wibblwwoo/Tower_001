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

	public override IEnumerable<Type> Dependencies => new[]
	{
		typeof(EventManager)
	};


	private Dictionary<string, TowerData> _towers = new();
	private TowerProgressionManager _progressionManager;
    private TowerEventManager _TowerEventManager; // changed name as it was confusing with 2 eventmanagers
    private TowerDifficultyManager _difficultyManager;
    private TowerDataManager _dataManager;

	public TowerManager(EventManager eventManager)
	{
		this.EventManager = eventManager;	
	}

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

	private void InitializeSubManagers()
	{
		_progressionManager = new TowerProgressionManager(EventManager, _towers);
		_TowerEventManager = new TowerEventManager(EventManager, _towers);
        _difficultyManager = new TowerDifficultyManager(_towers);
        _dataManager = new TowerDataManager(EventManager, _towers);
		_TowerEventManager.RegisterEventHandlers();
	}

	/// <summary>
	/// Loads tower data from an external source or creates a sample tower if no data exists.
	/// This method is currently a placeholder for implementing persistent tower data storage.
	/// </summary>
	private void LoadTowerData()
	{
		_dataManager.LoadTowerData();
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

	private FloorGenerationConfig CreateDefaultConfig(TowerData tower) => new()
	{
		BaseDifficulty = tower.Difficulty.BaseValue,
		DefaultFloorType = FloorType.None, // Could be derived from tower element
		DifficultyScalingFactor = tower.Difficulty.LevelScalingFactor
	};

	/// <summary>
	/// Attempts to enter a tower if the character meets the necessary requirements.
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
		return _progressionManager.TryEnterTower(towerId);
	}

	/// <summary>
	/// Gets the current difficulty of a tower based on character level and other factors
	/// </summary>
	/// <param name="towerId">The ID of the tower to get difficulty for</param>
	/// <returns>The calculated difficulty value</returns>
	public float GetTowerDifficulty(string towerId)
	{
		return _difficultyManager.GetTowerDifficulty(towerId);
	}

    // MOVED TO TowerDifficultyManager
    /*
    /// <summary>
    /// Calculates the current difficulty of a tower based on the character's level, element, and the tower's difficulty configuration.
    /// Adjusts the difficulty according to the level difference between the character and the tower's recommended level.
    /// Also applies difficulty clamping to ensure it stays within the defined minimum and maximum values.
    /// </summary>
    /// <param name="towerId">The ID of the tower for which the difficulty is being calculated.</param>
    /// <returns>The calculated difficulty for the tower based on the current character's level and the tower's settings.</returns>
    public float GetTowerDifficulty(string towerId)
    {
        // Check if the tower exists in the _towers dictionary
        if (!_towers.TryGetValue(towerId, out var tower))
        {
            GD.PrintErr($"Tower {towerId} not found"); // Log an error if the tower is not found
            return 0f; // Return 0 if the tower is not found
        }

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
    */

	/// <summary>
	/// Gets a collection of all towers that are currently available for interaction.
	/// </summary>
	/// <returns>A collection of towers that are currently available for interaction.</returns>
	public IEnumerable<TowerData> GetAvailableTowers()
	{
		return GetTowersByState(TowerState.Available);
	}

	/// <summary>
	/// Gets all towers that are in a locked state
	/// </summary>
	/// <returns>A collection of towers that are currently locked</returns>
	public IEnumerable<TowerData> GetLockedTowers()
	{
		return GetTowersByState(TowerState.Locked);
	}

	/// <summary>
	/// Gets all towers that are in a specific state
	/// </summary>
	/// <param name="state">The state to filter by. If null, returns all towers.</param>
	/// <returns>A collection of towers in the specified state</returns>
	public IEnumerable<TowerData> GetTowersByState(TowerState? state = null)
	{
		return state.HasValue
			? _towers.Values.Where(t => t.CurrentState == state.Value)
			: GetAllTowers();
	}

	/// <summary>
	/// Gets all towers in the system
	/// </summary>
	/// <returns>A collection of all towers</returns>
	public IEnumerable<TowerData> GetAllTowers()
	{
		return _towers.Values;
	}

	/// <summary>
	/// Locks a tower, preventing entry
	/// </summary>
	/// <param name="towerId">The ID of the tower to lock</param>
	/// <returns>True if successfully locked, false otherwise</returns>
	public bool LockTower(string towerId)
	{
		return _progressionManager.LockTower(towerId);
	}

	/// <summary>
	/// Marks a floor as completed in a tower
	/// </summary>
	/// <param name="towerId">The ID of the tower</param>
	/// <param name="floorId">The ID of the floor to complete</param>
	/// <returns>True if successfully completed, false otherwise</returns>
	public bool CompleteFloor(string towerId, string floorId)
	{
		return _progressionManager.CompleteFloor(towerId, floorId);
	}

	/// <summary>
	/// Marks a tower as failed
	/// </summary>
	/// <param name="towerId">The ID of the tower that failed</param>
	/// <returns>True if successfully marked as failed, false otherwise</returns>
	public bool FailTower(string towerId)
	{
		return _progressionManager.FailTower(towerId);
	}

	/// <summary>
	/// Resets the state of a specific floor in a tower.
	/// </summary>
	/// <param name="towerId">The ID of the tower containing the floor.</param>
	/// <param name="floorId">The ID of the floor to reset.</param>
	/// <returns>True if the floor's state was successfully reset; false otherwise.</returns>
	public bool ResetFloorState(string towerId, string floorId)
	{
		if (_towers.TryGetValue(towerId, out var tower))
		{
			return Globals.Instance.gameMangers.World.Floors.ResetFloorState(tower, floorId);
		}
		return false;
	}

	/// <summary>
	/// Gets the current state of a specific floor in a tower.
	/// </summary>
	/// <param name="towerId">The ID of the tower containing the floor.</param>
	/// <param name="floorId">The ID of the floor to get the state of.</param>
	/// <returns>The current state of the floor, or FloorState.Locked if the tower or floor is not found.</returns>
	public FloorState GetFloorState(string towerId, string floorId)
	{
		if (_towers.TryGetValue(towerId, out var tower))
		{
			var floor = tower.Floors.FirstOrDefault(f => f.Id == floorId);
			return floor?.CurrentState ?? FloorState.Locked;
		}
		return FloorState.Locked;
	}

	/// <summary>
	/// Unlocks a tower, making it available for entry.
	/// </summary>
	/// <param name="towerId">The ID of the tower to unlock.</param>
	/// <returns>True if the tower was successfully unlocked; otherwise, false.</returns>
	public bool UnlockTower(string towerId)
	{
		return _progressionManager.UnlockTower(towerId);
	}

	/// <summary>
	/// Marks a floor as failed in a tower
	/// </summary>
	/// <param name="towerId">The ID of the tower</param>
	/// <param name="floorId">The ID of the floor that failed</param>
	/// <returns>True if successfully marked as failed, false otherwise</returns>
	public bool FailFloor(string towerId, string floorId)
	{
		return _progressionManager.FailFloor(towerId, floorId);
	}

	/// <summary>
	/// Exits the current tower, making it available again
	/// </summary>
	/// <param name="towerId">The ID of the tower to exit</param>
	/// <returns>True if successfully exited, false otherwise</returns>
	public bool ExitTower(string towerId)
	{
		return _progressionManager.ExitTower(towerId);
	}

	protected override void RegisterEventHandlers()
    {
        
    }
}
