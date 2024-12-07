using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Represents an upgrade that can be applied to a resource collector to enhance its capabilities.
/// Each upgrade has a specific type, base value, and scaling factors that determine its effectiveness
/// as it is leveled up through the game progression.
/// </summary>
/// <remarks>
/// Upgrades follow a linear progression model where each level increases the upgrade's
/// effectiveness by a fixed amount (IncrementPerLevel). The total cost of upgrades
/// scales with level and can require multiple resource types.
/// </remarks>
public partial class CollectorUpgrade
{
    /// <summary>
    /// Unique identifier for the upgrade
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the upgrade shown in the UI
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of upgrade which determines how it affects the collector
    /// (e.g., collection speed, storage capacity, efficiency)
    /// </summary>
    public UpgradeType Type { get; set; }

    /// <summary>
    /// The starting value of the upgrade at level 1
    /// For percentage-based upgrades, this is expressed as a decimal (e.g., 0.1 for 10%)
    /// </summary>
    public float BaseValue { get; set; }

    /// <summary>
    /// The amount by which the upgrade's effect increases per level
    /// Follows a linear progression model: Effect = BaseValue + (Level * IncrementPerLevel)
    /// </summary>
    public float IncrementPerLevel { get; set; }

    /// <summary>
    /// The maximum level this upgrade can reach
    /// Once reached, the upgrade cannot be improved further
    /// </summary>
    public int MaxLevel { get; set; }

    /// <summary>
    /// Dictionary mapping resource types to their cost multipliers for this upgrade
    /// The actual cost for a level is calculated as: CostPerLevel[ResourceType] * CurrentLevel
    /// Multiple resource types can be required for a single upgrade
    /// </summary>
    public Dictionary<ResourceType, float> CostPerLevel { get; set; }
}