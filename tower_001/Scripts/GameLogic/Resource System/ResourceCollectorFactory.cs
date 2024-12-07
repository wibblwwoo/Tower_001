using static GlobalEnums;
using System.Collections.Generic;
using Tower_001.Scripts.GameLogic.Balance;

/// <summary>
/// Factory class responsible for creating and configuring resource collectors in the game.
/// This static factory implements the Factory Method pattern to centralize collector creation
/// and ensure consistent initialization of collector instances. It handles the setup of
/// collection rates, unlock requirements, and bonus multipliers for resource collectors.
/// </summary>
public static class ResourceCollectorFactory
{
    /// <summary>
    /// Creates a new resource collector with the specified configuration.
    /// </summary>
    /// <param name="id">Unique identifier for the collector. Used to generate the collector's name
    /// and track it in the resource management system.</param>
    /// <param name="baseRates">Dictionary mapping resource types to their base collection rates.
    /// Key: The type of resource that can be collected
    /// Value: The base rate at which the resource is collected per time unit</param>
    /// <returns>A fully configured ResourceCollector instance ready for use in the game.</returns>
    /// <remarks>
    /// The created collector includes:
    /// - Unique identification (ID and name)
    /// - Base collection rates for different resources
    /// - Unlock requirements from GameBalanceConfig
    /// - Multi-resource collection bonuses from GameBalanceConfig
    /// - An empty list of available upgrades (to be populated later)
    /// </remarks>
    public static ResourceCollector CreateCollector(string id, Dictionary<ResourceType, float> baseRates)
    {
        var definition = new CollectorDefinition
        {
            Id = id,
            Name = $"Collector_{id}",
            BaseCollectionRates = baseRates,
            UnlockRequirements = new Dictionary<UnlockCondition, float>
            {
                { UnlockCondition.CharacterLevel, GameBalanceConfig.ResourceSystem.Collectors.BaseUnlockLevel }
            },
            MultiResourceBonuses = new Dictionary<string, float>
            {
                { "dual_collection", GameBalanceConfig.ResourceSystem.Collectors.DualCollectionBonus },
                { "triple_collection", GameBalanceConfig.ResourceSystem.Collectors.TripleCollectionBonus }
            },
            AvailableUpgrades = new List<CollectorUpgrade>()
        };

        return new ResourceCollector(definition);
    }
}
