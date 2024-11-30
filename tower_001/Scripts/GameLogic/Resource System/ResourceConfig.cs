using System.Collections.Generic;
using static GlobalEnums;


/// <summary>
/// Configuration class for resources
/// </summary>
public class ResourceConfig
{
	public ResourceType Type { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string IconPath { get; set; }
	public float StorageLimit { get; set; }
	public Dictionary<ResourceType, float> UnlockRequirements { get; set; } = new();
	public HashSet<CollectorType> ValidCollectors { get; set; } = new();

}
