using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public interface IResourceCollector
{
	string Id { get; }
	bool IsUnlocked { get; }
	Dictionary<ResourceType, float> CollectionRates { get; }
	Dictionary<ResourceType, float> CollectionEfficiencyBonuses { get; }
	Dictionary<ResourceType, long> LifetimeCollected { get; }

	void StartCollecting();
	void StopCollecting();
	void UpdateCollectionRates(Dictionary<ResourceType, float> modifiers);
	void ApplyTemporaryBonus(ResourceType type, float multiplier, TimeSpan duration);
}