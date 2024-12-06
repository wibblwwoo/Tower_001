/// <summary>
/// Core resource manager class that integrates with the existing event and game manager systems
/// </summary>
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using static GlobalEnums;

/// <summary>
/// Core manager class for handling all resource-related functionality.
/// Manages resource amounts, collectors, storage, and related events.
/// </summary>
public partial class ResourceManager : BaseManager
{
	// Resource tracking
	private readonly Dictionary<ResourceType, float> _resourceAmounts = new();
	private readonly Dictionary<ResourceType, ResourceDefinition> _resourceDefinitions = new();
	private readonly Dictionary<ResourceType, bool> _unlockedResources = new();

	// Collectors and Storage
	private readonly Dictionary<string, IResourceCollector> _collectors = new();
	private readonly List<IResourceStorage> _storageUnits = new();

	// Configuration
	private readonly float DEFAULT_OVERFLOW_PERCENTAGE = 0.1f; // 10% overflow

	// Temporary boosts tracking
	private readonly Dictionary<ResourceType, List<TemporaryBoost>> _activeBoosts = new();

	/// <summary>
	/// Initializes the resource manager and registers event handlers
	/// </summary>
	public override void Setup()
	{
		RegisterEventHandlers();
		InitializeResources();
		StartCollectors();
	}

	protected override void RegisterEventHandlers()
	{
		// Register for character-related events to handle unlocks
		EventManager.AddHandler<CharacterLevelEventArgs>(
			EventType.CharacterLevelUp,
			OnCharacterLevelUp);

		EventManager.AddHandler<PrestigeEventArgs>(
			EventType.PrestigeLevelGained,
			OnPrestigeLevelGained);

		EventManager.AddHandler<AscensionEventArgs>(
			EventType.AscensionLevelGained,
			OnAscensionLevelGained);
	}

	/// <summary>
	/// Initializes resource definitions and sets up initial state
	/// </summary>
	private void InitializeResources()
	{
		foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
		{
			var definition = CreateResourceDefinition(type);
			_resourceDefinitions[type] = definition;
			_resourceAmounts[type] = 0;
			_unlockedResources[type] = false;
			_activeBoosts[type] = new List<TemporaryBoost>();
		}
	}

	/// <summary>
	/// Creates the initial configuration for a resource type
	/// </summary>
	private ResourceDefinition CreateResourceDefinition(ResourceType type)
	{
		var definition = new ResourceDefinition
		{
			Type = type,
			Name = type.ToString(),
			Tier = DetermineResourceTier(type),
			BaseCollectionRate = 1.0f,
			BaseStorageCapacity = 1000f,
			OverflowPercentage = DEFAULT_OVERFLOW_PERCENTAGE,
			UnlockRequirements = new Dictionary<UnlockCondition, float>(),
			Dependencies = new Dictionary<ResourceType, float>()
		};

		// Set up unlock requirements based on tier
		switch (definition.Tier)
		{
			case ResourceTier.Basic:
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 1;
				break;
			case ResourceTier.Advanced:
				definition.UnlockRequirements[UnlockCondition.Prestige] = 1;
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 10;
				break;
			case ResourceTier.Premium:
				definition.UnlockRequirements[UnlockCondition.Ascension] = 1;
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 20;
				break;
		}

		return definition;
	}

	/// <summary>
	/// Determines the tier of a resource type
	/// </summary>
	private ResourceTier DetermineResourceTier(ResourceType type)
	{
		// This would be configured based on your resource types
		// For now, using a simple example
		return type switch
		{
			ResourceType.Gold => ResourceTier.Basic,
			ResourceType.Pages => ResourceTier.Advanced,
			_ => ResourceTier.Basic
		};
	}

	/// <summary>
	/// Adds resources, respecting storage limits and handling overflow
	/// </summary>
	public bool AddResource(ResourceType type, float amount)
	{
		if (!_unlockedResources[type] || amount <= 0)
			return false;

		float currentAmount = _resourceAmounts[type];
		float maxCapacity = GetTotalCapacity(type);
		float overflowCapacity = maxCapacity * (1 + _resourceDefinitions[type].OverflowPercentage);

		// Calculate how much we can actually add
		float spaceAvailable = overflowCapacity - currentAmount;
		float amountToAdd = Math.Min(amount, spaceAvailable);
		float overflow = Math.Max(0, amount - spaceAvailable);

		if (amountToAdd > 0)
		{
			_resourceAmounts[type] += amountToAdd;
			RaiseResourceChangedEvent(type, amountToAdd);

			// If we hit overflow, raise the overflow event
			if (overflow > 0 || currentAmount + amountToAdd >= maxCapacity)
			{
				RaiseStorageOverflowEvent(type, currentAmount + amountToAdd, overflow);
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Removes resources if available
	/// </summary>
	public bool RemoveResource(ResourceType type, float amount)
	{
		if (!_unlockedResources[type] || amount <= 0)
			return false;

		if (_resourceAmounts[type] >= amount)
		{
			_resourceAmounts[type] -= amount;
			RaiseResourceChangedEvent(type, -amount);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets the current amount of a resource
	/// </summary>
	public float GetResourceAmount(ResourceType type)
	{
		return _resourceAmounts.GetValueOrDefault(type, 0);
	}

	/// <summary>
	/// Checks if the required amounts of resources are available
	/// </summary>
	public bool CanAfford(Dictionary<ResourceType, float> costs)
	{
		return costs.All(kvp => GetResourceAmount(kvp.Key) >= kvp.Value);
	}

	/// <summary>
	/// Adds a collector to the resource system
	/// </summary>
	public void AddCollector(IResourceCollector collector)
	{
		if (!_collectors.ContainsKey(collector.Id))
		{
			_collectors[collector.Id] = collector;
			collector.StartCollecting();
		}
	}

	/// <summary>
	/// Calculates total storage capacity including all storage units and active boosts
	/// </summary>
	public float GetTotalCapacity(ResourceType type)
	{
		float baseCapacity = _resourceDefinitions[type].BaseStorageCapacity;
		float storageUnitsCapacity = _storageUnits.Sum(s => s.BaseCapacity);
		float boostMultiplier = CalculateStorageBoostMultiplier(type);

		return (baseCapacity + storageUnitsCapacity) * boostMultiplier;
	}

	/// <summary>
	/// Updates and removes expired boosts
	/// </summary>
	public void Update()
	{
		var now = DateTime.UtcNow;
		foreach (var type in _activeBoosts.Keys)
		{
			_activeBoosts[type].RemoveAll(boost => boost.ExpirationTime <= now);
		}

		// Update collectors
		foreach (var collector in _collectors.Values)
		{
			// Add collected resources
			foreach (var kvp in collector.CollectionRates)
			{
				AddResource(kvp.Key, kvp.Value);
			}
		}
	}

	private float CalculateStorageBoostMultiplier(ResourceType type)
	{
		if (!_activeBoosts.ContainsKey(type))
			return 1.0f;

		var now = DateTime.UtcNow;
		return 1.0f + _activeBoosts[type]
			.Where(b => b.ExpirationTime > now)
			.Sum(b => b.Multiplier);
	}

	private void RaiseResourceChangedEvent(ResourceType type, float amount)
	{
		EventManager?.RaiseEvent(
			EventType.ResourceAmountChanged,
			new ResourceEventArgs(type, amount));
	}

	private void RaiseStorageOverflowEvent(ResourceType type, float currentAmount, float overflow)
	{
		var storage = _storageUnits.FirstOrDefault();
		if (storage != null)
		{
			EventManager?.RaiseEvent(
				EventType.ResourceAmountChanged,
				new ResourceStorageEventArgs(
					type,
					currentAmount,
					GetTotalCapacity(type),
					GetTotalCapacity(type) * (1 + DEFAULT_OVERFLOW_PERCENTAGE),
					overflow,
					storage));
		}
	}

	private void OnCharacterLevelUp(CharacterLevelEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.CharacterLevel, args.NewLevel);
	}

	private void OnPrestigeLevelGained(PrestigeEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.Prestige, args.NewPrestigeLevel);
	}

	private void OnAscensionLevelGained(AscensionEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.Ascension, args.NewAscensionLevel);
	}

	private void CheckResourceUnlocks(UnlockCondition condition, float value)
	{
		foreach (var definition in _resourceDefinitions.Values)
		{
			if (!_unlockedResources[definition.Type] &&
				definition.UnlockRequirements.TryGetValue(condition, out float requirement) &&
				value >= requirement)
			{
				UnlockResource(definition.Type);
			}
		}
	}

	private void UnlockResource(ResourceType type)
	{
		if (!_unlockedResources[type])
		{
			_unlockedResources[type] = true;
			EventManager?.RaiseEvent(
				EventType.ResourceUnlocked,
				new ResourceUnlockEventArgs(type));
		}
	}

	private void StartCollectors()
	{
		foreach (var collector in _collectors.Values)
		{
			collector.StartCollecting();
		}
	}

	private class TemporaryBoost
	{
		public float Multiplier { get; set; }
		public DateTime ExpirationTime { get; set; }
	}
}