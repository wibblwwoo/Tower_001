using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Represents a resource collector that can gather multiple types of resources over time
/// </summary>
public class ResourceCollector : IResourceCollector
{
	public string Id { get; }
	public bool IsUnlocked { get; private set; }
	public Dictionary<ResourceType, float> CollectionRates { get; private set; }
	public Dictionary<ResourceType, float> CollectionEfficiencyBonuses { get; private set; }
	public Dictionary<ResourceType, long> LifetimeCollected { get; private set; }

	private readonly CollectorDefinition _definition;
	private readonly Dictionary<ResourceType, List<TemporaryBonus>> _temporaryBonuses;
	private readonly Dictionary<string, float> _upgradeMultipliers;
	private bool _isCollecting;
	private DateTime _lastCollectionTime;

	public ResourceCollector(CollectorDefinition definition)
	{
		Id = definition.Id;
		_definition = definition;
		IsUnlocked = false; // Start locked

		CollectionRates = new Dictionary<ResourceType, float>();
		CollectionEfficiencyBonuses = new Dictionary<ResourceType, float>();
		LifetimeCollected = new Dictionary<ResourceType, long>();
		_temporaryBonuses = new Dictionary<ResourceType, List<TemporaryBonus>>();
		_upgradeMultipliers = new Dictionary<string, float>();

		InitializeCollectionRates();
	}


	private void InitializeCollectionRates()
	{
		foreach (var kvp in _definition.BaseCollectionRates)
		{
			CollectionRates[kvp.Key] = kvp.Value;
			CollectionEfficiencyBonuses[kvp.Key] = 1.0f;
			LifetimeCollected[kvp.Key] = 0;
			_temporaryBonuses[kvp.Key] = new List<TemporaryBonus>();
		}
		UpdateCollectionRates();
	}

	/// <summary>
	/// Attempts to unlock the collector if requirements are met
	/// </summary>

	/// <summary>
	/// Attempts to unlock the collector if requirements are met
	/// </summary>
	public bool TryUnlock(Dictionary<UnlockCondition, float> currentValues)
	{
		if (IsUnlocked) return true;

		foreach (var requirement in _definition.UnlockRequirements)
		{
			if (!currentValues.TryGetValue(requirement.Key, out float value) ||
				value < requirement.Value)
			{
				return false;
			}
		}

		IsUnlocked = true;
		return true;
	}
	public void StartCollecting()
	{
		// Can't start if not unlocked
		if (!IsUnlocked || _isCollecting) return;

		_isCollecting = true;
		_lastCollectionTime = DateTime.UtcNow;
	}

	public void StopCollecting()
	{
		_isCollecting = false;
	}

	/// <summary>
	/// Updates collection rates based on modifiers from various sources
	/// </summary>
	private void UpdateCollectionRates()
	{
		foreach (var kvp in _definition.BaseCollectionRates)
		{
			var resourceType = kvp.Key;
			float baseRate = kvp.Value;

			// Calculate multi-resource efficiency first
			float multiResourceBonus = CalculateMultiResourceEfficiency();
			CollectionEfficiencyBonuses[resourceType] = 1f + multiResourceBonus;

			// Apply base rate with efficiency
			CollectionRates[resourceType] = baseRate * CollectionEfficiencyBonuses[resourceType];

			// Apply temporary bonuses
			float temporaryBonus = CalculateTemporaryBonusMultiplier(resourceType);
			CollectionRates[resourceType] *= (1f + temporaryBonus);
		}
	}

	/// <summary>
	/// Applies a temporary bonus to collection rate for a specific resource
	/// </summary>
	public void ApplyTemporaryBonus(ResourceType type, float multiplier, TimeSpan duration)
	{
		if (!_temporaryBonuses.ContainsKey(type))
		{
			_temporaryBonuses[type] = new List<TemporaryBonus>();
		}

		_temporaryBonuses[type].Add(new TemporaryBonus
		{
			Multiplier = multiplier,
			ExpirationTime = DateTime.UtcNow + duration
		});

		UpdateCollectionRates();
	}
	public void UpdateCollectionRates(Dictionary<ResourceType, float> modifiers)
	{
		// Start with base rates
		foreach (var kvp in _definition.BaseCollectionRates)
		{
			var resourceType = kvp.Key;
			float baseRate = kvp.Value;
			CollectionRates[resourceType] = baseRate;

			DebugLogger.Log($"Base rate for {resourceType}: {baseRate}",
						   DebugLogger.LogCategory.Resources);
		}

		// Calculate efficiency bonus once
		float multiResourceBonus = CalculateMultiResourceEfficiency();
		DebugLogger.Log($"Multi-resource bonus: {multiResourceBonus:P}",
					   DebugLogger.LogCategory.Resources);

		// Apply all multipliers in one pass
		foreach (var resourceType in CollectionRates.Keys.ToList())
		{
			float rate = CollectionRates[resourceType];

			// Apply efficiency bonus
			float efficiencyMultiplier = 1f + multiResourceBonus;
			rate *= efficiencyMultiplier;

			// Apply external modifier if exists
			if (modifiers?.TryGetValue(resourceType, out float modifier) == true)
			{
				rate *= modifier;
			}

			// Apply temporary bonus if exists
			float temporaryBonus = CalculateTemporaryBonusMultiplier(resourceType);
			if (temporaryBonus > 0)
			{
				rate *= (1f + temporaryBonus);
			}

			// Store final rate
			CollectionRates[resourceType] = rate;
			CollectionEfficiencyBonuses[resourceType] = efficiencyMultiplier;

			DebugLogger.Log($"Final rate for {resourceType}: {rate}/s",
						   DebugLogger.LogCategory.Resources);
		}
	}
	


	/// <summary>
	/// Processes resource collection for the current tick
	/// </summary>
	public Dictionary<ResourceType, float> ProcessCollection(float deltaTime)
	{

		CleanupExpiredBonuses();
		if (!_isCollecting || !IsUnlocked)
		{
			DebugLogger.Log("Collection skipped - Collector not collecting or not unlocked",
						   DebugLogger.LogCategory.Resources);
			return new Dictionary<ResourceType, float>();
		}

		var collected = new Dictionary<ResourceType, float>();
		foreach (var kvp in CollectionRates)
		{
			float amount = kvp.Value * deltaTime;
			collected[kvp.Key] = amount;
			LifetimeCollected[kvp.Key] += (long)amount;

			DebugLogger.Log($"Collected {amount} {kvp.Key} (Rate: {kvp.Value}/s, Time: {deltaTime}s)",
						   DebugLogger.LogCategory.Resources);
		}

		return collected;
	}


	/// <summary>
	/// Calculates efficiency bonus for collecting multiple resources
	/// </summary>
	private float CalculateMultiResourceEfficiency()
	{
		int resourceCount = _definition.BaseCollectionRates.Count;

		DebugLogger.Log($"Calculating multi-resource efficiency for {resourceCount} resources",
					   DebugLogger.LogCategory.Resources);

		if (resourceCount <= 1) return 0f;

		float bonus = 0f;
		if (resourceCount >= 2 && _definition.MultiResourceBonuses.TryGetValue("dual_collection", out float dualBonus))
		{
			bonus += dualBonus;
			DebugLogger.Log($"Applied dual collection bonus: {dualBonus:P}",
						   DebugLogger.LogCategory.Resources);
		}
		if (resourceCount >= 3 && _definition.MultiResourceBonuses.TryGetValue("triple_collection", out float tripleBonus))
		{
			bonus += tripleBonus;
			DebugLogger.Log($"Applied triple collection bonus: {tripleBonus:P}",
						   DebugLogger.LogCategory.Resources);
		}

		DebugLogger.Log($"Total multi-resource bonus: {bonus:P}",
					   DebugLogger.LogCategory.Resources);
		return bonus;
	}

	private float CalculateBaseMultiplier(ResourceType type)
	{
		return CollectionEfficiencyBonuses.GetValueOrDefault(type, 1.0f);
	}

	private float CalculateTemporaryBonusMultiplier(ResourceType type)
	{
		if (!_temporaryBonuses.ContainsKey(type)) return 0.0f;

		var now = DateTime.UtcNow;
		return _temporaryBonuses[type]
			.Where(b => b.ExpirationTime > now)
			.Sum(b => b.Multiplier);
	}

	private float CalculateUpgradeMultiplier()
	{
		return 1.0f + _upgradeMultipliers.Values.Sum();
	}

	public void RemoveModifiers()
	{
		UpdateCollectionRates();

	}

	public void RemoveTemporaryBonuse()
	{
		_temporaryBonuses.Clear();
		UpdateCollectionRates();
		
	}

	private void CleanupExpiredBonuses()
	{
		var now = DateTime.UtcNow;
		bool hasExpired = false;

		// Actively remove expired bonuses
		foreach (var type in _temporaryBonuses.Keys.ToList())
		{
			hasExpired |= _temporaryBonuses[type].RemoveAll(b => b.ExpirationTime <= now) > 0;

			// If all bonuses for this type are removed, remove the empty list
			if (_temporaryBonuses[type].Count == 0)
			{
				_temporaryBonuses.Remove(type);
			}
		}

		// Update rates if any bonus expired
		if (hasExpired)
		{
			UpdateCollectionRates();
		}
	}


	/// <summary>
	/// Checks if the collector can be unlocked based on given conditions
	/// </summary>
	public bool CheckUnlockRequirements(Dictionary<UnlockCondition, float> currentValues)
	{
		if (IsUnlocked) return true;

		foreach (var requirement in _definition.UnlockRequirements)
		{
			if (!currentValues.TryGetValue(requirement.Key, out float value) ||
				value < requirement.Value)
			{
				return false;
			}
		}

		IsUnlocked = true;
		return true;
	}

	private class TemporaryBonus
	{
		public float Multiplier { get; set; }
		public DateTime ExpirationTime { get; set; }
	}
}

