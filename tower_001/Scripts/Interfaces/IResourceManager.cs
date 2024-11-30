using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public interface IResourceManager
{
	// Resource Management
	bool AddResource(ResourceType type, float amount);
	bool RemoveResource(ResourceType type, float amount);
	float GetResourceAmount(ResourceType type);
	bool CanAfford(Dictionary<ResourceType, float> costs);

	// Unlocking
	bool UnlockResource(ResourceType type);
	bool IsResourceUnlocked(ResourceType type);
	List<ResourceType> GetUnlockedResources();

	// Collectors
	void AddCollector(IResourceCollector collector);
	void RemoveCollector(string collectorId);
	List<IResourceCollector> GetActiveCollectors();

	// Storage
	void AddStorage(IResourceStorage storage);
	void RemoveStorage(IResourceStorage storage);
	float GetTotalCapacity(ResourceType type);

	// Events
	event Action<ResourceEventArgs> OnResourceChanged;
	event Action<ResourceUnlockEventArgs> OnResourceUnlocked;
	event Action<ResourceStorageEventArgs> OnStorageOverflow;
}
