using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;

public partial class CollectorDefinition
{
	public string Id { get; set; }
	public string Name { get; set; }
	public Dictionary<UnlockCondition, float> UnlockRequirements { get; set; }
	public Dictionary<ResourceType, float> BaseCollectionRates { get; set; }
	public Dictionary<string, float> MultiResourceBonuses { get; set; }
	public List<CollectorUpgrade> AvailableUpgrades { get; set; }
}