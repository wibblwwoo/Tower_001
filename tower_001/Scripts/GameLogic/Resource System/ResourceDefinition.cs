using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Defines the characteristics and behavior of a resource in the game's resource management system.
/// This class serves as a comprehensive blueprint for each resource type, containing all the
/// necessary information for resource generation, storage, dependencies, and unlock conditions.
/// </summary>
public partial class ResourceDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for this resource type.
    /// This is used throughout the system to reference this specific resource.
    /// </summary>
    public ResourceType Type { get; set; }

    /// <summary>
    /// Gets or sets the tier level of this resource.
    /// Resources are categorized into tiers (e.g., Basic, Advanced, Premium) which affect
    /// their availability and unlock requirements in the game progression.
    /// </summary>
    public ResourceTier Tier { get; set; }

    /// <summary>
    /// Gets or sets the display name of the resource.
    /// This is the human-readable name shown in the game's UI.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the detailed description of the resource.
    /// Provides additional information about the resource's purpose and usage in the game.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the resource dependencies required for generation or processing.
    /// Key: The type of resource required
    /// Value: The amount of that resource needed per operation
    /// For example, if creating advanced materials requires basic resources.
    /// </summary>
    public Dictionary<ResourceType, float> Dependencies { get; set; }

    /// <summary>
    /// Gets or sets the conditions that must be met to unlock this resource.
    /// Key: The type of condition (e.g., character level, prestige level)
    /// Value: The threshold value required to meet the condition
    /// Used to gate resource availability based on game progression.
    /// </summary>
    public Dictionary<UnlockCondition, float> UnlockRequirements { get; set; }

    /// <summary>
    /// Gets or sets the base rate at which this resource is collected.
    /// This value represents the amount collected per time unit before any
    /// modifiers or bonuses are applied.
    /// </summary>
    public float BaseCollectionRate { get; set; }

    /// <summary>
    /// Gets or sets the base storage capacity for this resource.
    /// Determines how much of this resource can be stored before reaching capacity.
    /// This value may be modified by upgrades or other game mechanics.
    /// </summary>
    public float BaseStorageCapacity { get; set; }

    /// <summary>
    /// Gets or sets the percentage of storage capacity that can be exceeded.
    /// When storage is full, additional resources can still be collected up to
    /// this percentage over the base capacity. Default is 10%.
    /// </summary>
    public float OverflowPercentage { get; set; }

    /// <summary>
    /// Gets or sets whether this resource has been discovered by the player.
    /// Resources may exist in the game but remain hidden until certain
    /// conditions are met or they are discovered through gameplay.
    /// </summary>
    public bool IsDiscovered { get; set; }
}