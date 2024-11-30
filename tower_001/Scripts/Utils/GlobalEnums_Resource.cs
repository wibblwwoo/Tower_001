using Godot;
using System;

public static partial class GlobalEnums
{

	public enum UpgradeType
	{
		CollectionRate,
		CollectionEfficiency,
		MultiResourceBonus,
		StorageCapacity
	}

	public enum ResourceTier
	{
		Basic,
		Advanced,
		Premium
	}

	public enum UnlockCondition
	{
		CharacterLevel,
		Prestige,
		Ascension,
		TowerProgress,
		FloorProgress,
		ResourceAmount
	}

	public enum StorageCapacityChangeReason
	{
		Upgrade,
		TemporaryBoost,
		PrestigeBonus,
		AscensionBonus,
		EventBonus,
		Achievement
	}
}