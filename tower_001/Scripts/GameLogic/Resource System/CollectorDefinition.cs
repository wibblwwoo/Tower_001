using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;

/// <summary>
/// Defines the configuration and capabilities of a resource collector in the game.
/// A collector definition serves as a template for creating resource collectors,
/// specifying their collection rates, unlock conditions, and available upgrades.
/// </summary>
/// <remarks>
/// Collector definitions are used by the ResourceCollectorFactory to instantiate
/// new collectors. They support multi-resource collection with bonuses for
/// collecting multiple resource types simultaneously.
/// </remarks>
public partial class CollectorDefinition
{
    /// <summary>
    /// Unique identifier for the collector definition
    /// Used to reference this specific collector type in the game
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the collector shown in the UI
    /// Used for player-facing interfaces and tooltips
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Requirements that must be met to unlock this collector
    /// Maps unlock conditions (e.g., level, resources, achievements) to their required values
    /// All conditions must be satisfied for the collector to become available
    /// </summary>
    /// <example>
    /// UnlockRequirements = {
    ///     { UnlockCondition.Level, 5 },        // Requires player level 5
    ///     { UnlockCondition.Gold, 1000 }       // Requires 1000 gold
    /// }
    /// </example>
    public Dictionary<UnlockCondition, float> UnlockRequirements { get; set; }

    /// <summary>
    /// Base collection rates for different resource types
    /// Maps resource types to their collection rate per time unit
    /// These rates can be modified by upgrades and bonuses
    /// </summary>
    /// <example>
    /// BaseCollectionRates = {
    ///     { ResourceType.Gold, 10 },    // Collects 10 gold per time unit
    ///     { ResourceType.Wood, 5 }      // Collects 5 wood per time unit
    /// }
    /// </example>
    public Dictionary<ResourceType, float> BaseCollectionRates { get; set; }

    /// <summary>
    /// Bonus multipliers applied when collecting multiple resources simultaneously
    /// The key is a composite string of resource types (e.g., "Gold_Wood")
    /// The value is the bonus multiplier (e.g., 1.5 for 50% bonus)
    /// </summary>
    /// <remarks>
    /// Multi-resource bonuses encourage strategic placement and upgrade choices
    /// These bonuses are multiplicative with other collection rate modifiers
    /// </remarks>
    public Dictionary<string, float> MultiResourceBonuses { get; set; }

    /// <summary>
    /// List of upgrades that can be applied to collectors of this type
    /// Each upgrade can modify collection rates, storage capacity, or other attributes
    /// Players can purchase these upgrades to improve collector performance
    /// </summary>
    /// <seealso cref="CollectorUpgrade"/>
    public List<CollectorUpgrade> AvailableUpgrades { get; set; }
}