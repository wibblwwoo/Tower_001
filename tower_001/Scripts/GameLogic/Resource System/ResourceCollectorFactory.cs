using static GlobalEnums;
using System.Collections.Generic;


/// <summary>
/// Factory class for creating resource collectors
/// </summary>
public static class ResourceCollectorFactory
{
	public static ResourceCollector CreateCollector(string id, Dictionary<ResourceType, float> baseRates)
	{
		var definition = new CollectorDefinition
		{
			Id = id,
			Name = $"Collector_{id}",
			BaseCollectionRates = baseRates,
			UnlockRequirements = new Dictionary<UnlockCondition, float>
			{
				{ UnlockCondition.CharacterLevel, 5 }  // Changed from 1 to 5
            },
			MultiResourceBonuses = new Dictionary<string, float>
			{
				{ "dual_collection", 0.3f },    // Changed from 0.1f to 0.3f
                { "triple_collection", 0.5f }   // Changed from 0.2f to 0.5f
            },
			AvailableUpgrades = new List<CollectorUpgrade>()
		};

		return new ResourceCollector(definition);
	}
}
