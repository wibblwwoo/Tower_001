using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public partial class ResourceDefinition
{
	public ResourceType Type { get; set; }
	public ResourceTier Tier { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public Dictionary<ResourceType, float> Dependencies { get; set; }
	public Dictionary<UnlockCondition, float> UnlockRequirements { get; set; }
	public float BaseCollectionRate { get; set; }
	public float BaseStorageCapacity { get; set; }
	public float OverflowPercentage { get; set; } // Default 10%
	public bool IsDiscovered { get; set; }
}