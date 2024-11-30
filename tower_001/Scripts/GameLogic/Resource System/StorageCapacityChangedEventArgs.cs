using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Event arguments specifically for storage capacity changes
/// </summary>
public partial class StorageCapacityChangedEventArgs : ResourceStorageEventArgs
{
	/// <summary>
	/// The previous capacity before the change
	/// </summary>
	public float PreviousCapacity { get; }

	/// <summary>
	/// The reason for the capacity change
	/// </summary>
	public StorageCapacityChangeReason ChangeReason { get; }

	public StorageCapacityChangedEventArgs(
		ResourceType type,
		float currentAmount,
		float baseCapacity,
		float overflowCapacity,
		float previousCapacity,
		IResourceStorage storage,
		StorageCapacityChangeReason reason)
		: base(type, currentAmount, baseCapacity, overflowCapacity, 0, storage)
	{
		PreviousCapacity = previousCapacity;
		ChangeReason = reason;

		Context["CapacityChange"] = baseCapacity - previousCapacity;
		Context["ChangeReason"] = reason.ToString();
	}
}