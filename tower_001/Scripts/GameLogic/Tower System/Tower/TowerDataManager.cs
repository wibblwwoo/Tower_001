using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Manages tower data creation, loading, and persistence
/// </summary>
public class TowerDataManager
{
    private readonly Dictionary<string, TowerData> _towers;
    private readonly EventManager _eventManager;

    public TowerDataManager(EventManager eventManager, Dictionary<string, TowerData> towers)
    {
        _eventManager = eventManager;
        _towers = towers;
    }

    /// <summary>
    /// Creates a sample tower with predefined settings for testing and demonstration purposes.
    /// Includes default requirements, difficulty configurations, and floor generation logic.
    /// </summary>
    public void CreateSampleTower()
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
        _eventManager?.RaiseEvent(EventType.TowerInitialized,
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

        // Generate a reasonable number of floors for the sample tower
        // Changed from 10000 to 10 floors for better startup performance
        Globals.Instance.gameMangers.World.Floors.GenerateFloorsForTower(tower, 10, floorConfig);
    }

    /// <summary>
    /// Loads tower data from an external source or creates a sample tower if no data exists.
    /// </summary>
    public void LoadTowerData()
    {
        // TODO: Implement loading from file or save system
        // Currently, a placeholder to create a sample tower for demonstration purposes
        CreateSampleTower();
    }
}
