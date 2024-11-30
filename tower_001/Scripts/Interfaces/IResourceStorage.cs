using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

// Storage System

public interface IResourceStorage
{
	float BaseCapacity { get; }
	float CurrentCapacity { get; }
	float OverflowCapacity { get; }
	Dictionary<ResourceType, float> StoredAmounts { get; }

	bool AddResource(ResourceType type, float amount);
	bool RemoveResource(ResourceType type, float amount);
	void ApplyTemporaryCapacityBoost(float multiplier, TimeSpan duration);
}