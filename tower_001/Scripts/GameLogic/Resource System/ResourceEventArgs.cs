using static GlobalEnums;

/// <summary>
/// Event arguments class for resource-related events in the game's resource management system.
/// This class encapsulates information about resource changes, including the type of resource
/// and the amount that was modified. It is used for events such as resource collection,
/// consumption, or any other resource quantity modifications.
/// </summary>
public class ResourceEventArgs : GameEventArgs
{
    /// <summary>
    /// Gets the type of resource involved in the event.
    /// This property identifies which resource (e.g., Gold, Wood, Stone) was affected.
    /// </summary>
    public ResourceType Type { get; }

    /// <summary>
    /// Gets the amount by which the resource was changed.
    /// Positive values indicate resource gains, while negative values indicate resource consumption.
    /// The amount is stored as a float to support fractional resource quantities.
    /// </summary>
    public float Amount { get; }

    /// <summary>
    /// Initializes a new instance of the ResourceEventArgs class.
    /// </summary>
    /// <param name="type">The type of resource that was modified.</param>
    /// <param name="amount">The amount by which the resource was changed. Can be positive (gains) or negative (losses).</param>
    public ResourceEventArgs(ResourceType type, float amount)
    {
        Type = type;
        Amount = amount;
    }
}