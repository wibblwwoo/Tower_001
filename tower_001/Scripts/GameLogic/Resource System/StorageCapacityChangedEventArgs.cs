using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Event arguments raised when a resource storage capacity changes.
/// This class extends ResourceStorageEventArgs to provide specific information about capacity changes,
/// such as the previous capacity and the reason for the change.
/// </summary>
/// <remarks>
/// This event is typically raised in scenarios such as:
/// - Storage upgrades or downgrades
/// - Temporary capacity boosts from buffs or items
/// - Research or technology improvements affecting storage
/// - Building or structure modifications
/// 
/// The event provides context about both the capacity change and its cause,
/// allowing systems to react appropriately to different types of capacity changes.
/// </remarks>
/// <seealso cref="ResourceStorageEventArgs"/>
/// <seealso cref="IResourceStorage"/>
public partial class StorageCapacityChangedEventArgs : ResourceStorageEventArgs
{
    /// <summary>
    /// Gets the storage capacity before the change occurred.
    /// This can be compared with the current BaseCapacity to determine the magnitude of the change.
    /// </summary>
    /// <remarks>
    /// This value is particularly useful for:
    /// - Calculating the net change in capacity
    /// - Validating capacity modifications
    /// - Reverting changes if needed
    /// - UI updates showing capacity changes
    /// </remarks>
    public float PreviousCapacity { get; }

    /// <summary>
    /// Gets the reason why the storage capacity was changed.
    /// This helps systems respond appropriately to different types of capacity changes.
    /// </summary>
    /// <remarks>
    /// Common reasons include:
    /// - Upgrades: Permanent increases from improvements
    /// - Buffs: Temporary increases from effects
    /// - Penalties: Reductions from events or conditions
    /// - Modifications: Changes from building or structure alterations
    /// 
    /// Systems can use this information to:
    /// - Apply different visual effects
    /// - Trigger specific sound effects
    /// - Update UI elements appropriately
    /// - Log changes for debugging
    /// </remarks>
    public StorageCapacityChangeReason ChangeReason { get; }

    /// <summary>
    /// Initializes a new instance of StorageCapacityChangedEventArgs.
    /// </summary>
    /// <param name="type">The type of resource affected by the capacity change</param>
    /// <param name="currentAmount">The current amount of the resource in storage</param>
    /// <param name="baseCapacity">The new base capacity after the change</param>
    /// <param name="overflowCapacity">Additional capacity beyond the base capacity</param>
    /// <param name="previousCapacity">The capacity before the change occurred</param>
    /// <param name="storage">Reference to the storage container that changed</param>
    /// <param name="reason">The reason for the capacity change</param>
    /// <remarks>
    /// This constructor:
    /// 1. Initializes the base event args with resource and capacity information
    /// 2. Sets the previous capacity and change reason
    /// 3. Adds context information about the capacity change and reason
    /// 
    /// The context dictionary includes:
    /// - "CapacityChange": The numerical change in capacity (positive or negative)
    /// - "ChangeReason": String representation of the reason for the change
    /// </remarks>
    public StorageCapacityChangedEventArgs(
        ResourceType type,
        float currentAmount,
        float baseCapacity,
        float overflowCapacity,
        float previousCapacity,
        IResourceStorage storage,
        StorageCapacityChangeReason reason)
        : base(type, currentAmount, baseCapacity, overflowCapacity, 0, storage)
    {
        PreviousCapacity = previousCapacity;
        ChangeReason = reason;

        Context["CapacityChange"] = baseCapacity - previousCapacity;
        Context["ChangeReason"] = reason.ToString();
    }
}