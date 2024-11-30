using static GlobalEnums;


/// <summary>
/// Event arguments for resource-related events
/// </summary>
public class ResourceEventArgs : GameEventArgs
{
	public ResourceType Type { get; }
	public float Amount { get; }

	public ResourceEventArgs(ResourceType type, float amount)
	{
		Type = type;
		Amount = amount;
	}
}