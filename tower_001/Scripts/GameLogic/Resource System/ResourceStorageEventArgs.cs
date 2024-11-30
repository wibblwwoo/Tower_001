using Godot;
using System;
using static GlobalEnums;

/// <summary>
/// Event arguments for resource storage-related events, such as overflow or capacity changes
/// </summary>
public class ResourceStorageEventArgs : ResourceEventArgs
{
	/// <summary>
	/// The current amount stored for this resource
	/// </summary>
	public float CurrentAmount { get; }

	/// <summary>
	/// The base storage capacity without overflow
	/// </summary>
	public float BaseCapacity { get; }

	/// <summary>
	/// The overflow capacity (typically 10% above base)
	/// </summary>
	public float OverflowCapacity { get; }

	/// <summary>
	/// Amount of resources lost due to overflow
	/// </summary>
	public float OverflowAmount { get; }

	/// <summary>
	/// The storage unit that triggered this event
	/// </summary>
	public IResourceStorage Storage { get; }

	/// <summary>
	/// Indicates if this event was triggered by a temporary storage boost
	/// </summary>
	public bool IsTemporaryBoost { get; }

	/// <summary>
	/// Duration of temporary boost if applicable
	/// </summary>
	public TimeSpan? BoostDuration { get; }

	public ResourceStorageEventArgs(
		ResourceType type,
		float currentAmount,
		float baseCapacity,
		float overflowCapacity,
		float overflowAmount,
		IResourceStorage storage,
		bool isTemporaryBoost = false,
		TimeSpan? boostDuration = null)
		: base(type, currentAmount)
	{
		CurrentAmount = currentAmount;
		BaseCapacity = baseCapacity;
		OverflowCapacity = overflowCapacity;
		OverflowAmount = overflowAmount;
		Storage = storage;
		IsTemporaryBoost = isTemporaryBoost;
		BoostDuration = boostDuration;

		// Add additional context to base GameEventArgs
		Context["StorageUnit"] = storage?.GetType().Name;
		Context["CapacityUtilization"] = $"{(currentAmount / baseCapacity * 100):F1}%";
		if (overflowAmount > 0)
		{
			Context["ResourcesLost"] = overflowAmount;
		}
		if (isTemporaryBoost)
		{
			Context["BoostDuration"] = boostDuration?.ToString();
		}
	}
}