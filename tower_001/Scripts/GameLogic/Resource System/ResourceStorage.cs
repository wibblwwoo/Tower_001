using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Implements a flexible storage system capable of managing multiple resource types with
/// configurable capacity ratios, overflow handling, and temporary capacity boosts.
/// </summary>
/// <remarks>
/// Key features:
/// - Multi-resource storage with configurable capacity ratios
/// - Overflow buffer with customizable percentage
/// - Temporary capacity boost system
/// - Event-based notifications for storage changes
/// - Automatic overflow handling and cleanup
/// 
/// Common usage scenarios:
/// - Resource stockpiles in buildings
/// - Player inventory systems
/// - Resource collectors and generators
/// - Temporary storage buffers
/// </remarks>
/// <seealso cref="IResourceStorage"/>
/// <seealso cref="ResourceStorageEventArgs"/>
/// <seealso cref="StorageCapacityChangedEventArgs"/>
public class ResourceStorage : IResourceStorage
{
    /// <summary>
    /// Gets the base storage capacity before any modifiers or boosts.
    /// This represents the permanent, unmodified storage limit.
    /// </summary>
    public float BaseCapacity { get; private set; }

    /// <summary>
    /// Gets the current total storage capacity including all active boosts.
    /// This value changes when temporary boosts are applied or expire.
    /// </summary>
    public float CurrentCapacity { get; private set; }

    /// <summary>
    /// Gets the maximum capacity including overflow buffer.
    /// Resources beyond this amount will be lost.
    /// </summary>
    public float OverflowCapacity { get; private set; }

    /// <summary>
    /// Gets the current amount of each resource type stored.
    /// Keys are resource types, values are the stored amounts.
    /// </summary>
    public Dictionary<ResourceType, float> StoredAmounts { get; private set; }

    /// <summary>
    /// Defines how the total capacity is divided between different resource types.
    /// Values represent the percentage of total capacity allocated to each type.
    /// </summary>
    private readonly Dictionary<ResourceType, float> _resourceCapacityRatios;

    /// <summary>
    /// Tracks all active temporary capacity boosts and their expiration times.
    /// </summary>
    private readonly List<TemporaryBoost> _activeBoosts;

    /// <summary>
    /// The percentage of additional capacity allowed for overflow before resources are lost.
    /// For example, 0.1f means 10% extra capacity as overflow buffer.
    /// </summary>
    private readonly float _overflowPercentage;

    /// <summary>
    /// Reference to the event manager for raising storage-related events.
    /// </summary>
    private readonly EventManager _eventManager;

    /// <summary>
    /// Initializes a new instance of the ResourceStorage class with specified capacity and resource allocation.
    /// </summary>
    /// <param name="baseCapacity">The base storage capacity before any modifiers</param>
    /// <param name="resourceRatios">Dictionary defining how capacity is divided between resource types.
    /// Values should be between 0 and 1, and should sum to 1 for 100% capacity utilization.</param>
    /// <param name="overflowPercentage">Additional capacity percentage allowed before resources are lost.
    /// Default is 0.1 (10% overflow buffer).</param>
    /// <remarks>
    /// Example usage:
    /// ```csharp
    /// var ratios = new Dictionary<ResourceType, float>
    /// {
    ///     { ResourceType.Gold, 0.5f },    // 50% for gold
    ///     { ResourceType.Wood, 0.3f },    // 30% for wood
    ///     { ResourceType.Stone, 0.2f }    // 20% for stone
    /// };
    /// var storage = new ResourceStorage(1000f, ratios);
    /// ```
    /// </remarks>
    public ResourceStorage(
        float baseCapacity,
        Dictionary<ResourceType, float> resourceRatios,
        float overflowPercentage = 0.1f)
    {
        BaseCapacity = baseCapacity;
        CurrentCapacity = baseCapacity;
        _resourceCapacityRatios = new Dictionary<ResourceType, float>(resourceRatios);
        _overflowPercentage = overflowPercentage;
        _activeBoosts = new List<TemporaryBoost>();
        StoredAmounts = new Dictionary<ResourceType, float>();
        _eventManager = Globals.Instance.gameMangers.Events;

        InitializeStorage();
        UpdateCapacities();
    }

    /// <summary>
    /// Initializes storage amounts for each configured resource type to zero.
    /// </summary>
    private void InitializeStorage()
    {
        // Initialize storage for each resource type
        foreach (var ratio in _resourceCapacityRatios)
        {
            StoredAmounts[ratio.Key] = 0;
        }
    }

    /// <summary>
    /// Attempts to add resources to storage, handling any overflow conditions.
    /// </summary>
    /// <param name="type">The type of resource to add</param>
    /// <param name="amount">The amount to add (must be positive)</param>
    /// <returns>True if any amount was successfully added, false otherwise</returns>
    /// <remarks>
    /// The method will:
    /// 1. Verify the resource type is valid and amount is positive
    /// 2. Calculate available space including overflow buffer
    /// 3. Store as much as possible within limits
    /// 4. Handle any overflow and raise appropriate events
    /// 
    /// If the storage would overflow:
    /// - Resources up to overflow capacity are stored
    /// - Excess resources are lost
    /// - An overflow event is raised
    /// </remarks>
    public bool AddResource(ResourceType type, float amount)
    {
        if (!StoredAmounts.ContainsKey(type) || amount <= 0)
            return false;

        float maxForType = GetMaxCapacityForResource(type);
        float currentAmount = StoredAmounts[type];
        float overflowLimit = maxForType * (1 + _overflowPercentage);
        float spaceAvailable = overflowLimit - currentAmount;

        // Calculate how much we can actually store
        float amountToStore = Math.Min(amount, spaceAvailable);
        float overflow = Math.Max(0, amount - spaceAvailable);

        if (amountToStore > 0)
        {
            StoredAmounts[type] += amountToStore;

            // Check if we've entered overflow territory
            if (StoredAmounts[type] > maxForType)
            {
                RaiseStorageOverflowEvent(type, StoredAmounts[type], overflow);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to remove resources from storage.
    /// </summary>
    /// <param name="type">The type of resource to remove</param>
    /// <param name="amount">The amount to remove (must be positive)</param>
    /// <returns>True if the full amount was successfully removed, false otherwise</returns>
    /// <remarks>
    /// The method will:
    /// 1. Verify the resource type is valid and amount is positive
    /// 2. Check if sufficient resources are available
    /// 3. Remove the resources if possible
    /// 
    /// Note: This method is all-or-nothing. It will only remove resources
    /// if the full requested amount is available.
    /// </remarks>
    public bool RemoveResource(ResourceType type, float amount)
    {
        if (!StoredAmounts.ContainsKey(type) || amount <= 0)
            return false;

        if (StoredAmounts[type] >= amount)
        {
            StoredAmounts[type] -= amount;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies a temporary boost to the storage capacity.
    /// </summary>
    /// <param name="multiplier">The capacity multiplier to apply (e.g., 1.5f for 50% increase)</param>
    /// <param name="duration">How long the boost should last</param>
    /// <remarks>
    /// The boost will:
    /// 1. Increase total capacity by the multiplier
    /// 2. Automatically expire after the specified duration
    /// 3. Stack with other active boosts
    /// 4. Trigger capacity changed events
    /// 
    /// Note: The boost affects all resource types equally
    /// </remarks>
    public void ApplyTemporaryCapacityBoost(float multiplier, TimeSpan duration)
    {
        var boost = new TemporaryBoost
        {
            Multiplier = multiplier,
            ExpirationTime = DateTime.UtcNow + duration
        };

        _activeBoosts.Add(boost);
        UpdateCapacities();

        // Raise event for temporary boost
        RaiseStorageCapacityChangedEvent(
            StorageCapacityChangeReason.TemporaryBoost);
    }

    /// <summary>
    /// Updates the storage state, handling expired boosts and overflow conditions.
    /// Should be called regularly, typically once per frame or tick.
    /// </summary>
    /// <remarks>
    /// This method:
    /// 1. Removes any expired capacity boosts
    /// 2. Recalculates capacities if boosts changed
    /// 3. Checks for and handles resource overflow
    /// 4. Raises appropriate events for any changes
    /// 
    /// Performance note: Consider the update frequency based on your needs.
    /// Very frequent updates may impact performance.
    /// </remarks>
    public void Update()
    {
        bool hadExpiredBoosts = RemoveExpiredBoosts();
        if (hadExpiredBoosts)
        {
            UpdateCapacities();
        }

        // Check for and handle overflow in each resource type
        foreach (var kvp in StoredAmounts.ToList())
        {
            float maxCapacity = GetMaxCapacityForResource(kvp.Key);
            float overflowLimit = maxCapacity * (1 + _overflowPercentage);

            if (kvp.Value > overflowLimit)
            {
                float overflow = kvp.Value - overflowLimit;
                StoredAmounts[kvp.Key] = overflowLimit;
                RaiseStorageOverflowEvent(kvp.Key, overflowLimit, overflow);
            }
        }
    }

    /// <summary>
    /// Calculates the maximum capacity for a specific resource type based on its ratio.
    /// </summary>
    /// <param name="type">The resource type to calculate capacity for</param>
    /// <returns>The maximum capacity allocated for the specified resource type</returns>
    private float GetMaxCapacityForResource(ResourceType type)
    {
        if (!_resourceCapacityRatios.TryGetValue(type, out float ratio))
            return 0;

        return CurrentCapacity * ratio;
    }

    /// <summary>
    /// Updates current and overflow capacities based on active boosts.
    /// </summary>
    /// <remarks>
    /// This method:
    /// 1. Calculates total boost multiplier from all active boosts
    /// 2. Updates current capacity with boosts applied
    /// 3. Recalculates overflow capacity
    /// </remarks>
    private void UpdateCapacities()
    {
        float totalMultiplier = 1.0f + _activeBoosts.Sum(b => b.Multiplier);
        CurrentCapacity = BaseCapacity * totalMultiplier;
        OverflowCapacity = CurrentCapacity * (1 + _overflowPercentage);
    }

    /// <summary>
    /// Removes expired capacity boosts from the active boosts list.
    /// </summary>
    /// <returns>True if any boosts were removed, false otherwise</returns>
    private bool RemoveExpiredBoosts()
    {
        int initialCount = _activeBoosts.Count;
        var now = DateTime.UtcNow;
        _activeBoosts.RemoveAll(b => b.ExpirationTime <= now);
        return _activeBoosts.Count < initialCount;
    }

    /// <summary>
    /// Raises an event to notify listeners about resource overflow.
    /// </summary>
    /// <param name="type">The type of resource that overflowed</param>
    /// <param name="currentAmount">Current amount after overflow handling</param>
    /// <param name="overflow">Amount of resources lost to overflow</param>
    private void RaiseStorageOverflowEvent(ResourceType type, float currentAmount, float overflow)
    {
        _eventManager?.RaiseEvent(
            EventType.ResourceAmountChanged,
            new ResourceStorageEventArgs(
                type,
                currentAmount,
                BaseCapacity,
                OverflowCapacity,
                overflow,
                this
            ));
    }

    /// <summary>
    /// Raises an event to notify listeners about storage capacity changes.
    /// </summary>
    /// <param name="reason">The reason for the capacity change</param>
    private void RaiseStorageCapacityChangedEvent(StorageCapacityChangeReason reason)
    {
        _eventManager?.RaiseEvent(
            EventType.ResourceAmountChanged,
            new StorageCapacityChangedEventArgs(
                ResourceType.Gold, // Example resource type, should be handled better
                StoredAmounts.Values.Sum(),
                BaseCapacity,
                OverflowCapacity,
                BaseCapacity, // Previous capacity
                this,
                reason
            ));
    }

    private class TemporaryBoost
    {
        public float Multiplier { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}

/// <summary>
/// Factory class for creating storage units
/// </summary>
public static class ResourceStorageFactory
{
    /// <summary>
    /// Creates a basic storage unit with equal ratios for all resource types
    /// </summary>
    /// <param name="capacity">The base storage capacity</param>
    /// <returns>A new ResourceStorage instance with equal resource ratios</returns>
    public static ResourceStorage CreateBasicStorage(float capacity)
    {
        // Create a basic storage unit with equal ratios for all resource types
        var resourceTypes = Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .ToDictionary(
                type => type,
                _ => 1.0f / Enum.GetValues(typeof(ResourceType)).Length
            );

        return new ResourceStorage(capacity, resourceTypes);
    }

    /// <summary>
    /// Creates a custom storage unit with specified resource ratios
    /// </summary>
    /// <param name="capacity">The base storage capacity</param>
    /// <param name="ratios">Dictionary defining how capacity is divided between resource types</param>
    /// <param name="overflowPercentage">Additional capacity percentage allowed before resources are lost</param>
    /// <returns>A new ResourceStorage instance with custom resource ratios</returns>
    public static ResourceStorage CreateCustomStorage(
        float capacity,
        Dictionary<ResourceType, float> ratios,
        float overflowPercentage = 0.1f)
    {
        // Normalize ratios to ensure they sum to 1
        float totalRatio = ratios.Values.Sum();
        var normalizedRatios = ratios.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value / totalRatio
        );

        return new ResourceStorage(capacity, normalizedRatios, overflowPercentage);
    }
}