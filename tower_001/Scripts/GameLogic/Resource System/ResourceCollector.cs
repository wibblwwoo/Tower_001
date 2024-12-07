using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

/// <summary>
/// Represents a resource collector that can gather multiple types of resources over time.
/// This class manages resource collection rates, efficiency bonuses, and temporary modifiers
/// for automated resource gathering in the game. It supports multi-resource collection with
/// efficiency bonuses and various types of collection rate modifiers.
/// </summary>
public class ResourceCollector : IResourceCollector
{
    /// <summary>
    /// Gets the unique identifier for this collector instance.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets whether this collector is unlocked and available for use.
    /// Collectors start locked and must meet specific requirements to be unlocked.
    /// </summary>
    public bool IsUnlocked { get; private set; }

    /// <summary>
    /// Gets the current collection rates for each resource type.
    /// These rates are calculated from base rates modified by efficiency bonuses and temporary modifiers.
    /// Key: Resource type
    /// Value: Current collection rate per second
    /// </summary>
    public Dictionary<ResourceType, float> CollectionRates { get; private set; }

    /// <summary>
    /// Gets the efficiency bonuses applied to each resource type.
    /// These bonuses come from multi-resource collection and other permanent modifiers.
    /// Key: Resource type
    /// Value: Efficiency multiplier (1.0 = no bonus)
    /// </summary>
    public Dictionary<ResourceType, float> CollectionEfficiencyBonuses { get; private set; }

    /// <summary>
    /// Gets the total amount of each resource type collected by this collector.
    /// Key: Resource type
    /// Value: Lifetime total amount collected
    /// </summary>
    public Dictionary<ResourceType, long> LifetimeCollected { get; private set; }

    private readonly CollectorDefinition _definition;
    private readonly Dictionary<ResourceType, List<TemporaryBonus>> _temporaryBonuses;
    private readonly Dictionary<string, float> _upgradeMultipliers;
    private bool _isCollecting;
    private DateTime _lastCollectionTime;

    /// <summary>
    /// Initializes a new instance of the ResourceCollector class.
    /// </summary>
    /// <param name="definition">The configuration definition for this collector,
    /// containing base rates, unlock requirements, and bonus settings.</param>
    public ResourceCollector(CollectorDefinition definition)
    {
        // Store the collector's unique identifier for tracking and reference
        Id = definition.Id;
        
        // Keep the original definition for rate calculations and requirement checks
        _definition = definition;
        
        // All collectors start in a locked state and must be unlocked through progression
        IsUnlocked = false;

        // Initialize collection tracking dictionaries
        CollectionRates = new Dictionary<ResourceType, float>();            // Current effective rates
        CollectionEfficiencyBonuses = new Dictionary<ResourceType, float>(); // Permanent bonuses
        LifetimeCollected = new Dictionary<ResourceType, long>();           // Total historical collection

        // Initialize modifier tracking
        _temporaryBonuses = new Dictionary<ResourceType, List<TemporaryBonus>>(); // Time-limited bonuses
        _upgradeMultipliers = new Dictionary<string, float>();                    // Permanent upgrades

        // Set up initial collection rates and bonuses
        InitializeCollectionRates();
    }

    /// <summary>
    /// Initializes the collection rates for all resource types defined in the collector's configuration.
    /// Sets up the base rates, efficiency bonuses, and tracking for lifetime collection amounts.
    /// </summary>
    private void InitializeCollectionRates()
    {
        // Initialize each resource type defined in the collector's configuration
        foreach (var kvp in _definition.BaseCollectionRates)
        {
            // Set up the base collection rate from definition
            CollectionRates[kvp.Key] = kvp.Value;
            
            // Start with no efficiency bonus (multiplier of 1.0)
            CollectionEfficiencyBonuses[kvp.Key] = 1.0f;
            
            // Initialize lifetime collection counter
            LifetimeCollected[kvp.Key] = 0;
            
            // Create empty list for temporary bonuses
            _temporaryBonuses[kvp.Key] = new List<TemporaryBonus>();
        }

        // Calculate initial rates with any applicable bonuses
        UpdateCollectionRates();
    }

    /// <summary>
    /// Attempts to unlock the collector if all requirements are met.
    /// </summary>
    /// <param name="currentValues">Dictionary of current values for each unlock condition.
    /// Key: The type of condition (e.g., character level, prestige level)
    /// Value: The current value for that condition</param>
    /// <returns>True if the collector is now unlocked (or was already unlocked), false otherwise.</returns>
    public bool TryUnlock(Dictionary<UnlockCondition, float> currentValues)
    {
        // Skip check if already unlocked
        if (IsUnlocked) return true;

        // Check each unlock requirement against current values
        foreach (var requirement in _definition.UnlockRequirements)
        {
            // Fail if required value isn't present or is too low
            if (!currentValues.TryGetValue(requirement.Key, out float value) ||
                value < requirement.Value)
            {
                return false;
            }
        }

        // All requirements met, unlock the collector
        IsUnlocked = true;
        return true;
    }

    /// <summary>
    /// Starts the collection process for this collector.
    /// Collection will only start if the collector is unlocked and not already collecting.
    /// </summary>
    public void StartCollecting()
    {
        // Prevent collection if locked or already collecting
        if (!IsUnlocked || _isCollecting) return;

        // Start collection and record the start time
        _isCollecting = true;
        _lastCollectionTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Stops the collection process for this collector.
    /// </summary>
    public void StopCollecting()
    {
        _isCollecting = false;
    }

    /// <summary>
    /// Updates collection rates based on all active modifiers.
    /// This includes base rates, efficiency bonuses, and temporary modifiers.
    /// </summary>
    private void UpdateCollectionRates()
    {
        // Update rates for each resource type individually
        foreach (var kvp in _definition.BaseCollectionRates)
        {
            var resourceType = kvp.Key;
            float baseRate = kvp.Value;

            // Step 1: Calculate multi-resource collection bonus
            float multiResourceBonus = CalculateMultiResourceEfficiency();
            CollectionEfficiencyBonuses[resourceType] = 1f + multiResourceBonus;

            // Step 2: Apply base rate with efficiency bonus
            CollectionRates[resourceType] = baseRate * CollectionEfficiencyBonuses[resourceType];

            // Step 3: Apply any active temporary bonuses
            float temporaryBonus = CalculateTemporaryBonusMultiplier(resourceType);
            CollectionRates[resourceType] *= (1f + temporaryBonus);
        }
    }

    /// <summary>
    /// Updates collection rates with external modifiers.
    /// </summary>
    /// <param name="modifiers">Dictionary of resource types and their modifiers.
    /// Key: Resource type
    /// Value: Collection rate modifier</param>
    public void UpdateCollectionRates(Dictionary<ResourceType, float> modifiers)
    {
        // Step 1: Reset to base rates
        foreach (var kvp in _definition.BaseCollectionRates)
        {
            var resourceType = kvp.Key;
            float baseRate = kvp.Value;
            CollectionRates[resourceType] = baseRate;

            // Log base rate for debugging
            DebugLogger.Log($"Base rate for {resourceType}: {baseRate}",
                           DebugLogger.LogCategory.Resources);
        }

        // Step 2: Calculate multi-resource efficiency bonus once for all resources
        float multiResourceBonus = CalculateMultiResourceEfficiency();
        DebugLogger.Log($"Multi-resource bonus: {multiResourceBonus:P}",
                       DebugLogger.LogCategory.Resources);

        // Step 3: Apply all modifiers in sequence
        foreach (var resourceType in CollectionRates.Keys.ToList())
        {
            float rate = CollectionRates[resourceType];

            // Apply multi-resource efficiency bonus
            float efficiencyMultiplier = 1f + multiResourceBonus;
            rate *= efficiencyMultiplier;

            // Apply external modifiers if present
            if (modifiers?.TryGetValue(resourceType, out float modifier) == true)
            {
                rate *= modifier;
            }

            // Apply temporary bonuses if active
            float temporaryBonus = CalculateTemporaryBonusMultiplier(resourceType);
            if (temporaryBonus > 0)
            {
                rate *= (1f + temporaryBonus);
            }

            // Store final calculated rates
            CollectionRates[resourceType] = rate;
            CollectionEfficiencyBonuses[resourceType] = efficiencyMultiplier;

            // Log final rate for debugging
            DebugLogger.Log($"Final rate for {resourceType}: {rate}/s",
                           DebugLogger.LogCategory.Resources);
        }
    }

    /// <summary>
    /// Processes resource collection for the current tick and returns the amounts collected.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last collection tick</param>
    /// <returns>Dictionary of resource types and amounts collected during this tick</returns>
    public Dictionary<ResourceType, float> ProcessCollection(float deltaTime)
    {
        // Clean up any expired bonuses before processing
        CleanupExpiredBonuses();

        // Skip collection if collector is locked or not active
        if (!_isCollecting || !IsUnlocked)
        {
            DebugLogger.Log("Collection skipped - Collector not collecting or not unlocked",
                           DebugLogger.LogCategory.Resources);
            return new Dictionary<ResourceType, float>();
        }

        // Calculate and track collection for each resource
        var collected = new Dictionary<ResourceType, float>();
        foreach (var kvp in CollectionRates)
        {
            // Calculate amount based on rate and time elapsed
            float amount = kvp.Value * deltaTime;
            
            // Store in results dictionary
            collected[kvp.Key] = amount;
            
            // Update lifetime collection counter
            LifetimeCollected[kvp.Key] += (long)amount;

            // Log collection for debugging
            DebugLogger.Log($"Collected {amount} {kvp.Key} (Rate: {kvp.Value}/s, Time: {deltaTime}s)",
                           DebugLogger.LogCategory.Resources);
        }

        return collected;
    }

    /// <summary>
    /// Calculates the efficiency bonus for collecting multiple resources simultaneously.
    /// Applies bonuses for dual and triple collection if available.
    /// </summary>
    /// <returns>The total efficiency bonus multiplier</returns>
    private float CalculateMultiResourceEfficiency()
    {
        // Get count of resources being collected
        int resourceCount = _definition.BaseCollectionRates.Count;

        DebugLogger.Log($"Calculating multi-resource efficiency for {resourceCount} resources",
                       DebugLogger.LogCategory.Resources);

        // No bonus for single resource collection
        if (resourceCount <= 1) return 0f;

        float bonus = 0f;

        // Apply dual collection bonus if collecting 2 or more resources
        if (resourceCount >= 2 && _definition.MultiResourceBonuses.TryGetValue("dual_collection", out float dualBonus))
        {
            bonus += dualBonus;
            DebugLogger.Log($"Applied dual collection bonus: {dualBonus:P}",
                           DebugLogger.LogCategory.Resources);
        }

        // Apply triple collection bonus if collecting 3 or more resources
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

    /// <summary>
    /// Gets the base multiplier for a specific resource type.
    /// </summary>
    /// <param name="type">The resource type to get the multiplier for</param>
    /// <returns>The base multiplier (defaults to 1.0 if not found)</returns>
    private float CalculateBaseMultiplier(ResourceType type)
    {
        return CollectionEfficiencyBonuses.GetValueOrDefault(type, 1.0f);
    }

    /// <summary>
    /// Calculates the total temporary bonus multiplier for a specific resource type.
    /// </summary>
    /// <param name="type">The resource type to calculate bonuses for</param>
    /// <returns>The sum of all active temporary bonus multipliers</returns>
    private float CalculateTemporaryBonusMultiplier(ResourceType type)
    {
        // Return 0 if no temporary bonuses exist for this resource
        if (!_temporaryBonuses.ContainsKey(type)) return 0.0f;

        var now = DateTime.UtcNow;
        
        // Sum all active bonus multipliers
        return _temporaryBonuses[type]
            .Where(b => b.ExpirationTime > now)  // Only include non-expired bonuses
            .Sum(b => b.Multiplier);             // Add up all active multipliers
    }

    /// <summary>
    /// Calculates the total upgrade multiplier from all active upgrades.
    /// </summary>
    /// <returns>The combined upgrade multiplier (1.0 plus sum of all upgrade bonuses)</returns>
    private float CalculateUpgradeMultiplier()
    {
        return 1.0f + _upgradeMultipliers.Values.Sum();
    }

    /// <summary>
    /// Removes all modifiers and resets collection rates to their base values.
    /// </summary>
    public void RemoveModifiers()
    {
        UpdateCollectionRates();
    }

    /// <summary>
    /// Removes all temporary bonuses and updates collection rates.
    /// </summary>
    public void RemoveTemporaryBonuse()
    {
        _temporaryBonuses.Clear();
        UpdateCollectionRates();
    }

    /// <summary>
    /// Removes expired temporary bonuses and updates collection rates if needed.
    /// </summary>
    private void CleanupExpiredBonuses()
    {
        var now = DateTime.UtcNow;
        bool hasExpired = false;

        // Check each resource type for expired bonuses
        foreach (var type in _temporaryBonuses.Keys.ToList())
        {
            // Remove expired bonuses and track if any were removed
            hasExpired |= _temporaryBonuses[type].RemoveAll(b => b.ExpirationTime <= now) > 0;

            // Clean up empty bonus lists
            if (_temporaryBonuses[type].Count == 0)
            {
                _temporaryBonuses.Remove(type);
            }
        }

        // Only update rates if bonuses were removed
        if (hasExpired)
        {
            UpdateCollectionRates();
        }
    }

    /// <summary>
    /// Checks if the collector can be unlocked based on given conditions.
    /// </summary>
    /// <param name="currentValues">Dictionary of current values for each unlock condition</param>
    /// <returns>True if all requirements are met and the collector is unlocked, false otherwise</returns>
    public bool CheckUnlockRequirements(Dictionary<UnlockCondition, float> currentValues)
    {
        // Skip check if already unlocked
        if (IsUnlocked) return true;

        // Check each unlock requirement against current values
        foreach (var requirement in _definition.UnlockRequirements)
        {
            // Fail if required value isn't present or is too low
            if (!currentValues.TryGetValue(requirement.Key, out float value) ||
                value < requirement.Value)
            {
                return false;
            }
        }

        // All requirements met, unlock the collector
        IsUnlocked = true;
        return true;
    }

    /// <summary>
    /// Applies a temporary bonus multiplier to the collection rate of a specific resource type.
    /// </summary>
    /// <param name="type">The resource type to apply the bonus to</param>
    /// <param name="multiplier">The bonus multiplier value (e.g., 0.5 for 50% increase)</param>
    /// <param name="duration">How long the bonus should last</param>
    public void ApplyTemporaryBonus(ResourceType type, float multiplier, TimeSpan duration)
    {
        // Skip if we don't collect this resource type
        if (!CollectionRates.ContainsKey(type)) return;

        // Create or get the list of temporary bonuses for this resource
        if (!_temporaryBonuses.ContainsKey(type))
        {
            _temporaryBonuses[type] = new List<TemporaryBonus>();
        }

        // Add the new temporary bonus
        _temporaryBonuses[type].Add(new TemporaryBonus
        {
            Multiplier = multiplier,
            ExpirationTime = DateTime.UtcNow + duration
        });

        // Update collection rates to apply the new bonus
        UpdateCollectionRates();
    }

    /// <summary>
    /// Represents a temporary bonus applied to resource collection.
    /// </summary>
    private class TemporaryBonus
    {
        /// <summary>
        /// The bonus multiplier value
        /// </summary>
        public float Multiplier { get; set; }

        /// <summary>
        /// When this bonus expires
        /// </summary>
        public DateTime ExpirationTime { get; set; }
    }
}
