using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages difficulty calculations for towers based on character levels and other factors.
/// </summary>
public class TowerDifficultyManager
{
    private readonly Dictionary<string, TowerData> _towers;

    public TowerDifficultyManager(Dictionary<string, TowerData> towers)
    {
        _towers = towers;
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
}
