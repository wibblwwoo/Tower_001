using Godot;
using System;

public static partial class GlobalEnums
{
	public enum TowerState
	{
		Locked,
		Available,
		InProgress,
		Completed,
		Failed
	}

	public enum FloorState
	{
		Locked,
		Available,
		InProgress,
		Completed,
		Failed
	}

	public enum RoomState
	{
		Locked,
		Available,
		Entered,
		EventStarted,
		EventCompleted,
		Failed,
		Cleared
	}
	public enum ElementType
	{
		None,
		Fire,
		Water
		// More to be added later
	}

	public enum RoomType
	{
		None,
		Encounter,
		Reward,
		Rest,
		Combat,
		Event,
		MiniBoss,
		Boss,       // Randomly distributed boss rooms
		FloorBoss   // Guaranteed last room	
	}
	public enum FloorType
	{

		None,
		Wind,
		Magic,
		Custom

	}
	public enum RewardType
	{
		None,
		Equipment,
		Resource,
		Currency
		// More to be added later
	}
	public enum TriggerType
	{
		SpawnMonster,
		GiveReward,
		ActivateEffect,
		ChangeRoom,
		ModifyStats,
		StartDialog
	}

	public enum ConditionType
	{
		PlayerLevel,
		PlayerHealth,
		HasItem,
		RoomCompleted,
		EventCompleted,
		StatCheck
	}

	public enum ComparisonOperator
	{
		Equals,
		NotEquals,
		GreaterThan,
		LessThan,
		GreaterThanOrEqual,
		LessThanOrEqual
	}

	public enum RequirementType
	{
		Item,
		Level,
		Quest,
		Achievement,
		Stat,
		Custom
	}

	public enum ConsequenceType
	{
		Death,
		Restart
	}

	[Flags]
	public enum RoomTag
	{
		None = 0,
		Combat = 1 << 0,
		Puzzle = 1 << 1,
		Story = 1 << 2,
		Shop = 1 << 3,
		Rest = 1 << 4,
		Boss = 1 << 5,
		Hidden = 1 << 6,
		Elite = 1 << 7,
		Treasure = 1 << 8,
		Event = 1 << 9,
		Challenge = 1 << 10,
		Required = 1 << 11,
		Optional = 1 << 12,
		Secret = 1 << 13,
		Timed = 1 << 14,
		MultiStage = 1 << 15,
		Special = 1 << 16,
		Tutorial = 1 << 17,
		Repeatable = 1 << 18,
		OneTime = 1 << 19,
		Checkpoint = 1 << 20
	}

	public enum RequirementOperator
	{
		And,
		Or,
		Xor,
		Not
	}

	public enum RequirementSeverity
	{
		Required,
		Recommended,
		Optional,
		Warning
	}



	public enum EventFailureAction
	{
		Retry,
		Skip,
		Alternate,
		Fail,
		ReturnToPrevious
	}

	public enum ChoiceCategory
	{
		Story,
		Combat,
		Exploration,
		Trade,
		Dialog,
		Challenge,
		Special
	}
}