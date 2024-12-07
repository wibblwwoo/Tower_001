using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Base event arguments for all resource storage-related events in the game.
/// This class provides comprehensive information about a resource storage's state,
/// including current amounts, capacities, and overflow conditions.
/// </summary>
/// <remarks>
/// This class serves as the foundation for various storage-related events such as:
/// - Resource overflow events
/// - Capacity change notifications
/// - Storage boost events
/// - Resource loss tracking
/// 
/// The event provides rich context through properties and a context dictionary,
/// allowing systems to:
/// - Monitor storage utilization
/// - Track resource losses
/// - Handle temporary storage boosts
/// - Update UI elements
/// - Trigger appropriate effects
/// </remarks>
/// <seealso cref="ResourceEventArgs"/>
/// <seealso cref="IResourceStorage"/>
/// <seealso cref="StorageCapacityChangedEventArgs"/>
public class ResourceStorageEventArgs : ResourceEventArgs
{
    /// <summary>
    /// Gets the current amount of resources stored in the storage unit.
    /// This represents the actual quantity of resources present at the time of the event.
    /// </summary>
    /// <remarks>
    /// This value is used to:
    /// - Calculate storage utilization
    /// - Determine if storage is full
    /// - Check if resources can be added
    /// - Update resource displays
    /// </remarks>
    public float CurrentAmount { get; }

    /// <summary>
    /// Gets the base storage capacity without any overflow or temporary boosts.
    /// This represents the normal, permanent storage limit of the unit.
    /// </summary>
    /// <remarks>
    /// The base capacity:
    /// - Determines the primary storage limit
    /// - Is used for upgrade calculations
    /// - Affects resource management strategies
    /// - May be increased through permanent upgrades
    /// </remarks>
    public float BaseCapacity { get; }

    /// <summary>
    /// Gets the additional storage capacity beyond the base limit.
    /// This typically represents a percentage (e.g., 10%) above the base capacity
    /// that can temporarily hold resources before they are lost.
    /// </summary>
    /// <remarks>
    /// Overflow capacity:
    /// - Provides a buffer against immediate resource loss
    /// - Gives players time to manage resources
    /// - May trigger warning notifications
    /// - Can be modified by upgrades or effects
    /// </remarks>
    public float OverflowCapacity { get; }

    /// <summary>
    /// Gets the amount of resources that were lost due to exceeding
    /// both base and overflow capacity limits.
    /// </summary>
    /// <remarks>
    /// This value is important for:
    /// - Tracking resource efficiency
    /// - Triggering loss notifications
    /// - Updating statistics
    /// - Providing feedback to players
    /// 
    /// A non-zero value indicates that storage optimization may be needed.
    /// </remarks>
    public float OverflowAmount { get; }

    /// <summary>
    /// Gets the storage unit that triggered this event.
    /// Provides access to the specific storage implementation for detailed operations.
    /// </summary>
    /// <remarks>
    /// The storage reference allows systems to:
    /// - Access additional storage properties
    /// - Modify storage settings
    /// - Query storage state
    /// - Identify the source of storage events
    /// </remarks>
    public IResourceStorage Storage { get; }

    /// <summary>
    /// Gets a value indicating whether this event was triggered by a temporary
    /// storage capacity boost, such as from a buff or temporary upgrade.
    /// </summary>
    /// <remarks>
    /// Used to:
    /// - Determine if capacity changes are permanent
    /// - Schedule boost expiration
    /// - Update UI with temporary status
    /// - Track active effects
    /// </remarks>
    public bool IsTemporaryBoost { get; }

    /// <summary>
    /// Gets the duration of a temporary storage boost, if applicable.
    /// Will be null for permanent storage changes.
    /// </summary>
    /// <remarks>
    /// This value is used to:
    /// - Schedule boost expiration
    /// - Display remaining boost time
    /// - Manage boost effects
    /// - Plan resource management
    /// </remarks>
    public TimeSpan? BoostDuration { get; }

    /// <summary>
    /// Initializes a new instance of ResourceStorageEventArgs with detailed storage information.
    /// </summary>
    /// <param name="type">The type of resource involved in the storage event</param>
    /// <param name="currentAmount">Current quantity of resources in storage</param>
    /// <param name="baseCapacity">Normal storage capacity without overflow</param>
    /// <param name="overflowCapacity">Additional capacity beyond base limit</param>
    /// <param name="overflowAmount">Amount of resources lost to overflow</param>
    /// <param name="storage">Reference to the storage unit</param>
    /// <param name="isTemporaryBoost">Whether this involves a temporary capacity boost</param>
    /// <param name="boostDuration">Duration of temporary boost if applicable</param>
    /// <remarks>
    /// This constructor:
    /// 1. Initializes the base event args with resource type and amount
    /// 2. Sets all storage-specific properties
    /// 3. Populates the context dictionary with useful information
    /// 
    /// The context dictionary includes:
    /// - "StorageUnit": Name of the storage implementation
    /// - "CapacityUtilization": Percentage of base capacity used
    /// - "ResourcesLost": Amount lost to overflow (if any)
    /// - "BoostDuration": Duration of temporary boost (if applicable)
    /// </remarks>
    public ResourceStorageEventArgs(
        ResourceType type,
        float currentAmount,
        float baseCapacity,
        float overflowCapacity,
        float overflowAmount,
        IResourceStorage storage,
        bool isTemporaryBoost = false,
        TimeSpan? boostDuration = null)
        : base(type, currentAmount)
    {
        CurrentAmount = currentAmount;
        BaseCapacity = baseCapacity;
        OverflowCapacity = overflowCapacity;
        OverflowAmount = overflowAmount;
        Storage = storage;
        IsTemporaryBoost = isTemporaryBoost;
        BoostDuration = boostDuration;

        // Add additional context to base GameEventArgs
        Context["StorageUnit"] = storage?.GetType().Name;
        Context["CapacityUtilization"] = $"{(currentAmount / baseCapacity * 100):F1}%";
        if (overflowAmount > 0)
        {
            Context["ResourcesLost"] = overflowAmount;
        }
        if (isTemporaryBoost)
        {
            Context["BoostDuration"] = boostDuration?.ToString();
        }
    }
}