/// <summary>
/// Core resource manager class that integrates with the existing event and game manager systems.
/// This class serves as the central hub for all resource-related operations in the game,
/// including resource collection, storage, unlocking, and boost management.
/// </summary>
/// <remarks>
/// The ResourceManager handles:
/// - Resource amount tracking and modification
/// - Resource definitions and configurations
/// - Resource unlocking based on game progression
/// - Resource collection through collectors
/// - Resource storage management
/// - Temporary boost management
/// - Event raising for resource-related changes
/// 
/// Key features:
/// - Supports multiple resource types with different tiers
/// - Handles resource overflow with configurable percentages
/// - Manages temporary boosts with expiration times
/// - Integrates with the event system for notifications
/// - Supports multiple collectors and storage units
/// </remarks>
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

public partial class ResourceManager : BaseManager
{
	// Resource tracking dictionaries
	// These dictionaries form the core state of the resource system
	// _resourceAmounts: Tracks current quantities of each resource type
	// _resourceDefinitions: Stores configuration and properties for each resource
	// _unlockedResources: Keeps track of which resources are available to the player
	private readonly Dictionary<ResourceType, float> _resourceAmounts = new();
	private readonly Dictionary<ResourceType, ResourceDefinition> _resourceDefinitions = new();
	private readonly Dictionary<ResourceType, bool> _unlockedResources = new();

	// Resource generation and storage systems
	// _collectors: Maps collector IDs to their instances for resource generation
	// _storageUnits: List of all available storage units that contribute to total capacity
	private readonly Dictionary<string, IResourceCollector> _collectors = new();
	private readonly List<IResourceStorage> _storageUnits = new();

	// Configuration value from GameBalanceConfig
	// This determines how much a resource can exceed its normal storage capacity
	// Example: 0.1 means storage can exceed by 10% before complete overflow
	private readonly float DEFAULT_OVERFLOW_PERCENTAGE = GameBalanceConfig.ResourceSystem.DefaultOverflowPercentage;

	// Boost system tracking
	// Maps resource types to their active temporary boosts
	// Boosts are time-limited multipliers that affect resource attributes
	private readonly Dictionary<ResourceType, List<TemporaryBoost>> _activeBoosts = new();

	/// <summary>
	/// Initializes the resource manager and registers event handlers
	/// </summary>
	public override void Setup()
	{
		// Order is important:
		// 1. Register event handlers first to catch any events during initialization
		// 2. Initialize resource definitions and states
		// 3. Start collectors last after everything is set up
		RegisterEventHandlers();
		InitializeResources();
		StartCollectors();
	}

	protected override void RegisterEventHandlers()
	{
		// Register for progression-related events that can trigger resource unlocks
		// These events come from the character progression system
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
		// For each resource type in the game:
		// 1. Create its definition with configuration
		// 2. Initialize its amount to 0
		// 3. Set it as locked initially
		// 4. Create an empty list for future boosts
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
		// Create base definition with default values from GameBalanceConfig
		var definition = new ResourceDefinition
		{
			Type = type,
			Name = type.ToString(),
			Tier = DetermineResourceTier(type),
			BaseCollectionRate = GameBalanceConfig.ResourceSystem.BaseCollectionRate,
			BaseStorageCapacity = GameBalanceConfig.ResourceSystem.BaseStorageCapacity,
			OverflowPercentage = GameBalanceConfig.ResourceSystem.DefaultOverflowPercentage,
			UnlockRequirements = new Dictionary<UnlockCondition, float>(),
			Dependencies = new Dictionary<ResourceType, float>()
		};

		// Set up unlock requirements based on tier
		// Different tiers have progressively harder unlock conditions
		// This creates a natural progression path for resources
		switch (definition.Tier)
		{
			case ResourceTier.Basic:
				// Basic resources only require character level
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 
					GameBalanceConfig.ResourceSystem.UnlockRequirements.Basic.CharacterLevel;
				break;
			case ResourceTier.Advanced:
				// Advanced resources require both character level and prestige
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 
					GameBalanceConfig.ResourceSystem.UnlockRequirements.Advanced.CharacterLevel;
				definition.UnlockRequirements[UnlockCondition.Prestige] = 
					GameBalanceConfig.ResourceSystem.UnlockRequirements.Advanced.Prestige;
				break;
			case ResourceTier.Premium:
				// Premium resources require both character level and ascension
				definition.UnlockRequirements[UnlockCondition.CharacterLevel] = 
					GameBalanceConfig.ResourceSystem.UnlockRequirements.Premium.CharacterLevel;
				definition.UnlockRequirements[UnlockCondition.Ascension] = 
					GameBalanceConfig.ResourceSystem.UnlockRequirements.Premium.Ascension;
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
	/// Gets the current amount of a resource
	/// </summary>
	/// <param name="type">The type of resource to get the amount for</param>
	/// <returns>The current amount of the resource</returns>
	public float GetResourceAmount(ResourceType type)
	{
		return _resourceAmounts.GetValueOrDefault(type, 0);
	}

	/// <summary>
	/// Checks if the required amounts of resources are available
	/// </summary>
	/// <param name="costs">A dictionary of resource types and their required amounts</param>
	/// <returns>True if all required resources are available, false otherwise</returns>
	public bool CanAfford(Dictionary<ResourceType, float> costs)
	{
		return costs.All(kvp => GetResourceAmount(kvp.Key) >= kvp.Value);
	}

	/// <summary>
	/// Adds a collector to the resource system.
	/// </summary>
	/// <param name="collector">The collector to add. Must have a unique ID.</param>
	/// <remarks>
	/// If a collector with the same ID already exists, the new collector will not be added.
	/// The collector will automatically start collecting resources once added.
	/// </remarks>
	public void AddCollector(IResourceCollector collector)
	{
		if (!_collectors.ContainsKey(collector.Id))
		{
			_collectors[collector.Id] = collector;
			collector.StartCollecting();
		}
	}

	/// <summary>
	/// Calculates the total storage capacity for a specific resource type.
	/// </summary>
	/// <param name="type">The resource type to calculate capacity for</param>
	/// <returns>The total storage capacity including base capacity, storage units, and active boosts</returns>
	/// <remarks>
	/// The total capacity is calculated as:
	/// (Base Capacity + Sum of Storage Unit Capacities) * Boost Multiplier
	/// </remarks>
	public float GetTotalCapacity(ResourceType type)
	{
		// Get base capacity from resource definition
		float baseCapacity = _resourceDefinitions[type].BaseStorageCapacity;
		
		// Sum up capacity from all storage units
		float storageUnitsCapacity = _storageUnits.Sum(s => s.BaseCapacity);
		
		// Apply any active boost multipliers
		float boostMultiplier = CalculateStorageBoostMultiplier(type);

		// Calculate and return total capacity
		return (baseCapacity + storageUnitsCapacity) * boostMultiplier;
	}

	/// <summary>
	/// Adds resources, respecting storage limits and handling overflow
	/// </summary>
	public bool AddResource(ResourceType type, float amount)
	{
		// Early exit conditions:
		// - Resource must be unlocked
		// - Amount must be positive
		if (!_unlockedResources[type] || amount <= 0)
			return false;

		// Calculate storage limits:
		// - currentAmount: Current resource quantity
		// - maxCapacity: Normal storage limit
		// - overflowCapacity: Maximum amount including overflow allowance
		float currentAmount = _resourceAmounts[type];
		float maxCapacity = GetTotalCapacity(type);
		float overflowCapacity = maxCapacity * (1 + _resourceDefinitions[type].OverflowPercentage);

		// Calculate how much we can actually add:
		// - spaceAvailable: Room left until overflow capacity
		// - amountToAdd: Lesser of requested amount and available space
		// - overflow: Amount that exceeds even overflow capacity
		float spaceAvailable = overflowCapacity - currentAmount;
		float amountToAdd = Math.Min(amount, spaceAvailable);
		float overflow = Math.Max(0, amount - spaceAvailable);

		if (amountToAdd > 0)
		{
			// Update the resource amount and notify listeners
			_resourceAmounts[type] += amountToAdd;
			RaiseResourceChangedEvent(type, amountToAdd);

			// If we hit normal capacity or have overflow, notify listeners
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
		// Early exit conditions:
		// - Resource must be unlocked
		// - Amount must be positive
		// - Must have enough resources
		if (!_unlockedResources[type] || amount <= 0)
			return false;

		if (_resourceAmounts[type] >= amount)
		{
			// Update amount and notify listeners
			// Note: amount is negated in the event to indicate removal
			_resourceAmounts[type] -= amount;
			RaiseResourceChangedEvent(type, -amount);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Updates and removes expired boosts
	/// </summary>
	public void Update()
	{
		var now = DateTime.UtcNow;

		// Clean up expired boosts for each resource type
		foreach (var type in _activeBoosts.Keys)
		{
			_activeBoosts[type].RemoveAll(boost => boost.ExpirationTime <= now);
		}

		// Update collectors and process their resource generation
		foreach (var collector in _collectors.Values)
		{
			// Each collector can generate multiple resource types
			// Process each resource type and add it to storage
			foreach (var kvp in collector.CollectionRates)
			{
				AddResource(kvp.Key, kvp.Value);
			}
		}
	}

	/// <summary>
	/// Calculates the current storage boost multiplier for a resource type
	/// </summary>
	private float CalculateStorageBoostMultiplier(ResourceType type)
	{
		// If no boosts exist for this type, return base multiplier (1.0)
		if (!_activeBoosts.ContainsKey(type))
			return 1.0f;

		var now = DateTime.UtcNow;

		// Calculate total boost:
		// 1. Filter out expired boosts
		// 2. Sum up all active boost multipliers
		// 3. Add to base multiplier (1.0)
		return 1.0f + _activeBoosts[type]
			.Where(b => b.ExpirationTime > now)
			.Sum(b => b.Multiplier);
	}

	/// <summary>
	/// Raises an event to notify listeners of a change in resource amount
	/// </summary>
	private void RaiseResourceChangedEvent(ResourceType type, float amount)
	{
		// Notify listeners through the event system
		// amount can be positive (addition) or negative (removal)
		EventManager?.RaiseEvent(
			EventType.ResourceAmountChanged,
			new ResourceEventArgs(type, amount));
	}

	/// <summary>
	/// Raises an event to notify listeners that a resource storage has overflowed
	/// </summary>
	private void RaiseStorageOverflowEvent(ResourceType type, float currentAmount, float overflow)
	{
		// Only raise overflow event if we have at least one storage unit
		var storage = _storageUnits.FirstOrDefault();
		if (storage != null)
		{
			// Calculate important capacity values:
			// - Normal capacity
			// - Overflow capacity (includes overflow percentage)
			// - Amount of overflow
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

	/// <summary>
	/// Handles the CharacterLevelUp event to unlock resources
	/// </summary>
	/// <param name="args">The event arguments containing the new character level</param>
	private void OnCharacterLevelUp(CharacterLevelEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.CharacterLevel, args.NewLevel);
	}

	/// <summary>
	/// Handles the PrestigeLevelGained event to unlock resources
	/// </summary>
	/// <param name="args">The event arguments containing the new prestige level</param>
	private void OnPrestigeLevelGained(PrestigeEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.Prestige, args.NewPrestigeLevel);
	}

	/// <summary>
	/// Handles the AscensionLevelGained event to unlock resources
	/// </summary>
	/// <param name="args">The event arguments containing the new ascension level</param>
	private void OnAscensionLevelGained(AscensionEventArgs args)
	{
		CheckResourceUnlocks(UnlockCondition.Ascension, args.NewAscensionLevel);
	}

	/// <summary>
	/// Checks if resources can be unlocked based on the given condition and value
	/// </summary>
	/// <param name="condition">The unlock condition to check</param>
	/// <param name="value">The value to check against the unlock requirements</param>
	private void CheckResourceUnlocks(UnlockCondition condition, float value)
	{
		// Check each resource definition for potential unlocks
		foreach (var definition in _resourceDefinitions.Values)
		{
			// Resource can be unlocked if:
			// 1. It's not already unlocked
			// 2. It has a requirement for this condition
			// 3. The current value meets or exceeds the requirement
			if (!_unlockedResources[definition.Type] &&
				definition.UnlockRequirements.TryGetValue(condition, out float requirement) &&
				value >= requirement)
			{
				UnlockResource(definition.Type);
			}
		}
	}

	/// <summary>
	/// Unlocks a resource type
	/// </summary>
	/// <param name="type">The resource type to unlock</param>
	private void UnlockResource(ResourceType type)
	{
		// Only unlock and raise event if not already unlocked
		if (!_unlockedResources[type])
		{
			_unlockedResources[type] = true;
			EventManager?.RaiseEvent(
				EventType.ResourceUnlocked,
				new ResourceUnlockEventArgs(type));
		}
	}

	/// <summary>
	/// Starts all collectors
	/// </summary>
	private void StartCollectors()
	{
		// Activate all registered collectors
		foreach (var collector in _collectors.Values)
		{
			collector.StartCollecting();
		}
	}

	/// <summary>
	/// Represents a temporary boost to resource-related attributes
	/// </summary>
	private class TemporaryBoost
	{
		// The multiplier value for this boost (e.g., 1.5 = 50% increase)
		public float Multiplier { get; set; }

		// UTC timestamp when this boost expires
		// Using UTC ensures consistent behavior across time zones
		public DateTime ExpirationTime { get; set; }
	}
}