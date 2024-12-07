using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Configuration class that defines the static properties and behavior settings for resources in the game.
/// This class serves as a data container for resource-specific settings that are loaded from configuration
/// files or set during initialization. Unlike ResourceDefinition, which handles runtime state,
/// ResourceConfig focuses on immutable configuration data.
/// </summary>
public class ResourceConfig
{
    /// <summary>
    /// Gets or sets the unique identifier for this resource configuration.
    /// This type is used to match the configuration with its corresponding resource instance
    /// and must be unique across all resource configurations.
    /// </summary>
    public ResourceType Type { get; set; }

    /// <summary>
    /// Gets or sets the display name of the resource.
    /// This is the localized, human-readable name that appears in the game's user interface.
    /// Should be clear and concise while accurately representing the resource.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the detailed description of the resource.
    /// This text provides players with information about the resource's purpose,
    /// how it's obtained, and its role in the game's economy.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the file path to the resource's icon asset.
    /// This path is relative to the game's asset directory and points to the
    /// visual representation used in the UI for this resource.
    /// </summary>
    public string IconPath { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount of this resource that can be stored.
    /// This represents the base storage limit before any modifiers or upgrades
    /// are applied. A value of 0 indicates unlimited storage.
    /// </summary>
    public float StorageLimit { get; set; }

    /// <summary>
    /// Gets or sets the requirements that must be met to unlock this resource.
    /// Key: The type of resource needed as a prerequisite
    /// Value: The amount of that resource required
    /// The resource becomes available when all requirements are satisfied.
    /// Initialized as an empty dictionary by default.
    /// </summary>
    public Dictionary<ResourceType, float> UnlockRequirements { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of collector types that can gather or generate this resource.
    /// This defines which collection mechanisms (e.g., miners, farmers, automatons) are valid
    /// for this particular resource type. Initialized as an empty hash set by default.
    /// </summary>
    public HashSet<CollectorType> ValidCollectors { get; set; } = new();
}
