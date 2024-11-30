using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Represents a storage unit that can hold multiple types of resources
/// </summary>
public class ResourceStorage : IResourceStorage
{
	public float BaseCapacity { get; private set; }
	public float CurrentCapacity { get; private set; }
	public float OverflowCapacity { get; private set; }
	public Dictionary<ResourceType, float> StoredAmounts { get; private set; }

	private readonly Dictionary<ResourceType, float> _resourceCapacityRatios;
	private readonly List<TemporaryBoost> _activeBoosts;
	private readonly float _overflowPercentage;
	private readonly EventManager _eventManager;

	/// <summary>
	/// Initializes a new storage unit with specified capacity and resource ratios
	/// </summary>
	/// <param name="baseCapacity">Base storage capacity before boosts</param>
	/// <param name="resourceRatios">How capacity is divided between resource types</param>
	/// <param name="overflowPercentage">Percentage of additional overflow capacity (default 10%)</param>
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

	private void InitializeStorage()
	{
		// Initialize storage for each resource type
		foreach (var ratio in _resourceCapacityRatios)
		{
			StoredAmounts[ratio.Key] = 0;
		}
	}

	/// <summary>
	/// Attempts to add resources to storage, handling overflow
	/// </summary>
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
	/// Attempts to remove resources from storage
	/// </summary>
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
	/// Applies a temporary capacity boost to the storage
	/// </summary>
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
	/// Updates the storage to handle expired boosts and recalculate capacities
	/// </summary>
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
	/// Gets the maximum capacity for a specific resource type
	/// </summary>
	private float GetMaxCapacityForResource(ResourceType type)
	{
		if (!_resourceCapacityRatios.TryGetValue(type, out float ratio))
			return 0;

		return CurrentCapacity * ratio;
	}

	private void UpdateCapacities()
	{
		float totalMultiplier = 1.0f + _activeBoosts.Sum(b => b.Multiplier);
		CurrentCapacity = BaseCapacity * totalMultiplier;
		OverflowCapacity = CurrentCapacity * (1 + _overflowPercentage);
	}

	private bool RemoveExpiredBoosts()
	{
		int initialCount = _activeBoosts.Count;
		var now = DateTime.UtcNow;
		_activeBoosts.RemoveAll(b => b.ExpirationTime <= now);
		return _activeBoosts.Count < initialCount;
	}

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