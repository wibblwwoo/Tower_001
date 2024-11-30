using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Represents the difficulty configuration for a floor, including scaling factors, modifiers, and adjustments for specific conditions.
/// Provides functionality to calculate final difficulty values for rooms, floors, and bosses based on various parameters.
/// </summary>
public class FloorDifficulty
{
	// Properties

	/// <summary>
	/// The base difficulty value for the floor. All calculations start with this value.
	/// </summary>
	public float BaseValue { get; set; }

	/// <summary>
	/// The scaling factor applied to the difficulty for each floor increment.
	/// </summary>
	public float ScalingFactor { get; set; }

	/// <summary>
	/// A list of global difficulty modifiers applied to the floor.
	/// </summary>
	public List<DifficultyModifier> Modifiers { get; set; } = new();

	/// <summary>
	/// A dictionary of modifiers specific to room types (e.g., Combat, Boss, MiniBoss).
	/// </summary>
	public Dictionary<RoomType, float> RoomTypeModifiers { get; set; } = new();

	/// <summary>
	/// A multiplier applied to boss room difficulties.
	/// </summary>
	public float BossModifier { get; set; } = 1.5f;

	/// <summary>
	/// Indicates whether the difficulty scales with the player's level.
	/// </summary>
	public bool ScalesWithLevel { get; set; }

	/// <summary>
	/// The factor by which the difficulty increases or decreases based on level difference.
	/// </summary>
	public float LevelScalingFactor { get; set; } = 0.1f;

	/// <summary>
	/// The reference level used to calculate difficulty scaling relative to the player's level.
	/// </summary>
	public int ReferenceLevel { get; set; } = 1;

	/// <summary>
	/// The minimum allowable difficulty for the floor.
	/// </summary>
	public float MinimumDifficulty { get; set; } = 0.5f;

	/// <summary>
	/// The maximum allowable difficulty for the floor.
	/// </summary>
	public float MaximumDifficulty { get; set; } = 5.0f;

	/// <summary>
	/// Element-specific modifiers applied to the difficulty (e.g., Fire, Water, Earth).
	/// </summary>
	public Dictionary<string, float> ElementalModifiers { get; set; } = new();

	/// <summary>
	/// The number of the floor for which this difficulty configuration applies.
	/// </summary>
	public int FloorNumber { get; set; } = 1;

	/// <summary>
	/// Indicates if the floor is a milestone floor (e.g., every 10th floor).
	/// </summary>
	public bool IsMilestoneFloor => FloorNumber % 10 == 0;

	/// <summary>
	/// The multiplier applied to milestone floors (e.g., every 10th floor).
	/// </summary>
	public float MilestoneDifficultyMultiplier { get; set; } = 1.5f;

	// Methods

	/// <summary>
	/// Calculates the final difficulty for the floor, taking into account modifiers, level scaling, and elemental factors.
	/// </summary>
	/// <param name="playerLevel">The level of the player, used for scaling calculations.</param>
	/// <param name="playerElement">The element of the player, used for elemental modifiers.</param>
	/// <returns>The final difficulty value for the floor, clamped to the minimum and maximum difficulty limits.</returns>
	public float CalculateFinalDifficulty(int playerLevel = 1, ElementType? playerElement = null)
	{
		// Start with the base difficulty adjusted by the floor number
		float difficulty = CalculateDifficulty(FloorNumber);

		// Apply all global difficulty modifiers
		foreach (var modifier in Modifiers)
		{
			difficulty = modifier.Apply(difficulty);
		}

		// Apply level scaling if enabled
		if (ScalesWithLevel)
		{
			float levelDifference = playerLevel - ReferenceLevel; // Calculate difference between player and reference levels
			difficulty *= (1 + (levelDifference * LevelScalingFactor)); // Adjust difficulty based on level difference
		}

		// Apply elemental modifiers if a player element is provided
		if (playerElement.HasValue && ElementalModifiers.TryGetValue(playerElement.Value.ToString(), out float elementMod))
		{
			difficulty *= elementMod; // Apply the element modifier to the difficulty
		}

		// Clamp the difficulty to the allowable range
		return Math.Clamp(difficulty, MinimumDifficulty, MaximumDifficulty);
	}

	/// <summary>
	/// Calculates the base difficulty for a room type, applying room-specific modifiers if defined.
	/// </summary>
	/// <param name="roomType">The type of room (e.g., Boss, Combat, Reward).</param>
	/// <param name="baseDifficulty">The base difficulty value for the calculation.</param>
	/// <returns>The adjusted difficulty for the room type.</returns>
	public float GetRoomDifficulty(RoomType roomType, float baseDifficulty)
	{
		// Check if a modifier exists for the specified room type
		if (RoomTypeModifiers.TryGetValue(roomType, out float modifier))
		{
			return baseDifficulty * modifier; // Apply the room-specific modifier
		}
		return baseDifficulty; // Return base difficulty if no modifier exists
	}

	/// <summary>
	/// Calculates the difficulty of the floor based on its base value and scaling factors.
	/// Applies milestone multipliers for milestone floors (e.g., every 10th floor).
	/// </summary>
	/// <param name="floorNumber">The floor number for which the difficulty is calculated.</param>
	/// <returns>The calculated base difficulty for the floor.</returns>
	public float CalculateDifficulty(int floorNumber = 1)
	{
		// Calculate the base difficulty adjusted by the floor scaling factor
		float difficulty = BaseValue * (1 + (floorNumber - 1) * ScalingFactor);

		// Apply milestone multiplier for milestone floors
		if (floorNumber % 10 == 0)
		{
			difficulty *= MilestoneDifficultyMultiplier; // Increase difficulty for milestone floors
		}

		return difficulty;
	}

	/// <summary>
	/// Calculates the difficulty for a boss room by applying the boss-specific multiplier.
	/// </summary>
	/// <param name="baseDifficulty">The base difficulty for the calculation.</param>
	/// <returns>The adjusted difficulty for a boss room.</returns>
	public float GetBossDifficulty(float baseDifficulty)
	{
		return baseDifficulty * BossModifier; // Apply the boss-specific multiplier
	}

	/// <summary>
	/// Adds a new global difficulty modifier to the list of modifiers.
	/// </summary>
	/// <param name="modifier">The difficulty modifier to add.</param>
	public void AddModifier(DifficultyModifier modifier)
	{
		Modifiers.Add(modifier); // Add the modifier to the list
	}

	/// <summary>
	/// Sets a modifier for a specific room type.
	/// </summary>
	/// <param name="roomType">The room type to apply the modifier to.</param>
	/// <param name="modifier">The modifier value.</param>
	public void SetRoomTypeModifier(RoomType roomType, float modifier)
	{
		RoomTypeModifiers[roomType] = modifier; // Update or add the modifier for the specified room type
	}

	/// <summary>
	/// Sets a modifier for a specific elemental type.
	/// </summary>
	/// <param name="element">The elemental type (e.g., Fire, Water).</param>
	/// <param name="modifier">The modifier value.</param>
	public void SetElementalModifier(ElementType element, float modifier)
	{
		ElementalModifiers[element.ToString()] = modifier; // Update or add the modifier for the specified element
	}

	/// <summary>
	/// Retrieves a dictionary containing difficulty configuration details for debugging or analysis.
	/// </summary>
	/// <returns>A dictionary containing key difficulty settings.</returns>
	public Dictionary<string, object> GetDifficultyInfo()
	{
		return new Dictionary<string, object>
		{
			{ "BaseValue", BaseValue },
			{ "Modifiers", Modifiers },
			{ "RoomTypeModifiers", RoomTypeModifiers },
			{ "BossModifier", BossModifier },
			{ "ScalesWithLevel", ScalesWithLevel },
			{ "LevelScalingFactor", LevelScalingFactor },
			{ "ReferenceLevel", ReferenceLevel },
			{ "ElementalModifiers", ElementalModifiers }
		};
	}

	public void InheritSettings(FloorDifficulty parentDifficulty)
	{
			BaseValue = parentDifficulty.BaseValue;
			ScalingFactor = parentDifficulty.ScalingFactor;
			BossModifier = parentDifficulty.BossModifier;
			ScalesWithLevel = parentDifficulty.ScalesWithLevel;
			LevelScalingFactor = parentDifficulty.LevelScalingFactor;
			ReferenceLevel = parentDifficulty.ReferenceLevel;
			MinimumDifficulty = parentDifficulty.MinimumDifficulty;
			MaximumDifficulty = parentDifficulty.MaximumDifficulty;
			MilestoneDifficultyMultiplier = parentDifficulty.MilestoneDifficultyMultiplier;

			// Copy modifiers
				Modifiers = parentDifficulty.Modifiers.Select(m => m.Clone()).ToList();

			// Copy room type modifiers
			foreach (var modifier in parentDifficulty.RoomTypeModifiers)
			{
				RoomTypeModifiers[modifier.Key] = modifier.Value;
			}

			// Copy elemental modifiers
			foreach (var modifier in parentDifficulty.ElementalModifiers)
			{
				ElementalModifiers[modifier.Key] = modifier.Value;
			}
		}
}
