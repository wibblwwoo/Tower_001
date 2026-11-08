using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Handles the generation of floors for towers.
/// </summary>
public class FloorGenerator
{
    private readonly FloorManager _floorManager;
    private readonly TowerData _tower;
    private readonly Random _random;

    public FloorGenerator(FloorManager floorManager, TowerData tower)
    {
        _floorManager = floorManager ?? throw new ArgumentNullException(nameof(floorManager));
        _tower = tower ?? throw new ArgumentNullException(nameof(tower));
        _random = new Random();
    }

    /// <summary>
    /// Generates a specified number of floors for the tower.
    /// </summary>
    public bool GenerateFloorsForTower(int floorCount, FloorGenerationConfig config = null)
    {
        if (config == null)
        {
            config = new FloorGenerationConfig
            {
                BaseDifficulty = _tower.Difficulty.BaseValue,
                DefaultFloorType = FloorType.None,
                DifficultyScalingFactor = _tower.Difficulty.LevelScalingFactor
            };
        }

        try
        {
            // Create floors
            for (int i = 1; i <= floorCount; i++)
            {
                bool isBossFloor = i % 10 == 0;
                bool success = _floorManager.CreateFloor(
                    _tower.Id,
                    i,
                    DetermineFloorType(i, isBossFloor),
                    CalculateFloorDifficulty(i, config, isBossFloor),
                    _tower.Difficulty,
                    isBossFloor
                );

                if (!success)
                {
                    GD.PrintErr($"Failed to create floor {i}");
                    return false;
                }
            }

            // Assign floors to tower in order
            _tower.Floors = _floorManager.GetFloorsForTower(_tower.Id)
                .OrderBy(f => f.FloorNumber)
                .ToList();

            DebugLogger.Log($"Generated {floorCount} floors for tower {_tower.Id}", DebugLogger.LogCategory.Floor);
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error in GenerateFloorsForTower: {ex.Message}");
            return false;
        }
    }

    private FloorType DetermineFloorType(int floorNumber, bool isBossFloor)
    {
        if (isBossFloor)
            return FloorType.Boss;

        return FloorType.Normal;
    }

    private float CalculateFloorDifficulty(int floorNumber, FloorGenerationConfig config, bool isBossFloor)
    {
        float basePower = config.BaseDifficulty;
        float scalingFactor = config.DifficultyScalingFactor;
        
        // Basic difficulty scaling with floor number
        float scaledDifficulty = basePower * (1 + (floorNumber - 1) * scalingFactor);
        
        // Boss floors are harder
        if (isBossFloor)
        {
            scaledDifficulty *= 1.5f;
        }

        return scaledDifficulty;
    }
}