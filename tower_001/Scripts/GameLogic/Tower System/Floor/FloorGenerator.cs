
using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Handles the generation of floors for a tower.
/// Uses configuration settings and progression rules to create floors dynamically.
/// </summary>
public partial class FloorGenerator
{
	// References to essential components.
	private readonly FloorManager _floorManager; // Responsible for creating and managing floor data.
	private readonly TowerData _towerData; // Contains information about the tower being generated.
	private readonly Random _random; // Utility for randomized logic.

	/// <summary>
	/// Initializes the floor generator with necessary dependencies.
	/// </summary>
	/// <param name="floorManager">Manages floor creation and lifecycle.</param>
	/// <param name="towerData">Data representing the target tower structure.</param>
	public FloorGenerator(FloorManager floorManager, TowerData towerData)
	{
		_floorManager = floorManager;
		_towerData = towerData;
		_random = new Random(); // Initialize the random number generator.
	}

	/// <summary>
	/// Generates a specified number of floors for the current tower using the given configuration.
	/// </summary>
	/// <param name="floorCount">Total number of floors to generate.</param>
	/// <param name="config">Configuration that governs floor generation rules.</param>
	/// <returns>True if all floors are successfully created; false otherwise.</returns>
	public bool GenerateFloorsForTower(int floorCount, FloorGenerationConfig config)
	{
		// Validate the input number of floors.
		if (floorCount <= 0 || floorCount > 100000)
		{
			GD.PrintErr($"Invalid floor count: {floorCount}"); // Log error for invalid input.
			return false;
		}

		try
		{
			// Iterate through each floor to generate it.
			for (int i = 1; i <= floorCount; i++)
			{
				// Determine the type of floor based on its position and configuration rules.
				var floorType = DetermineFloorType(i, config);

				// Calculate the base difficulty for this floor, factoring in progression and milestones.
				float baseDifficulty = CalculateBaseDifficulty(i, config);

				// Special handling for every 100th floor, treating it as a milestone boss floor.
				bool isBossFloor = i % 100 == 0;
				if (isBossFloor)
				{
					baseDifficulty *= config.MilestoneDifficultyMultiplier; // Increase difficulty for boss floors.
				}

				// Create the floor using the floor manager, passing all calculated properties.
				bool created = _floorManager.CreateFloor(
					_towerData.Id, // The ID of the tower being generated.
					i, // Current floor number.
					floorType, // Floor type as determined earlier.
					baseDifficulty, // The calculated difficulty for this floor.
					_towerData.Difficulty, // Overall difficulty level of the tower.
					isBossFloor // Whether this floor is designated as a boss floor.
				);

				// Check if the floor creation failed.
				if (!created)
				{
					GD.PrintErr($"Failed to create floor {i}"); // Log the failure.
					return false; // Abort generation if any floor creation fails.
				}
			}

			// All floors created successfully.
			return true;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error generating floors: {ex.Message}"); // Log unexpected errors during generation.
			return false;
		}
	}

	/// <summary>
	/// Determines the type of floor to generate based on configuration and floor number.
	/// </summary>
	/// <param name="floorNumber">The number of the floor being generated.</param>
	/// <param name="config">Configuration settings for floor type patterns.</param>
	/// <returns>The calculated floor type.</returns>
	private FloorType DetermineFloorType(int floorNumber, FloorGenerationConfig config)
	{
		// Check if a specific floor type pattern is provided in the configuration.
		if (config.FloorTypePattern != null && config.FloorTypePattern.Length > 0)
		{
			// Use the modulo operator to cycle through the pattern based on floor number.
			return config.FloorTypePattern[floorNumber % config.FloorTypePattern.Length];
		}

		// Default to the configuration's default floor type if no pattern is provided.
		return config.DefaultFloorType;
	}

	/// <summary>
	/// Calculates the base difficulty for a given floor, incorporating scaling factors and milestones.
	/// </summary>
	/// <param name="floorNumber">The number of the floor being generated.</param>
	/// <param name="config">Configuration settings for difficulty scaling and progression.</param>
	/// <returns>The calculated base difficulty for the floor.</returns>
	private float CalculateBaseDifficulty(int floorNumber, FloorGenerationConfig config)
	{
		// Start with the base difficulty defined in the configuration.
		float baseDifficulty = config.BaseDifficulty;

		// Progressively increase the difficulty based on the floor number.
		float progressionMultiplier = 1 + ((floorNumber - 1) * config.DifficultyScalingFactor);

		// Apply additional difficulty spikes for milestone floors (e.g., every 10th floor).
		if (config.MilestoneDifficultySpikes && floorNumber % 10 == 0)
		{
			progressionMultiplier *= config.MilestoneDifficultyMultiplier;
		}

		// Return the final difficulty after applying all factors.
		return baseDifficulty * progressionMultiplier;
	}
}