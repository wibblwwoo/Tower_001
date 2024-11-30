using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

public partial class CollectorUpgrade
{
	public string Id { get; set; }
	public string Name { get; set; }
	public UpgradeType Type { get; set; }
	public float BaseValue { get; set; }
	public float IncrementPerLevel { get; set; } // Linear progression
	public int MaxLevel { get; set; }
	public Dictionary<ResourceType, float> CostPerLevel { get; set; }
}