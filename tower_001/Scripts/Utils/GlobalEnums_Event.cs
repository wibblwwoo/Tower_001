using Godot;
using System;

public static partial class GlobalEnums
{
	public enum EventType
	{
		None = 0,

		// Tower Events (200-299)
		TowerInitialized = 200,
		TowerStateChanged = 201,
		TowerEntered = 202,
		TowerExited = 203,
		TowerFailed = 204,
		TowerCompleted = 205,

		// Floor Events (300-399)
		FloorDiscovered = 300,
		FloorStateChanged = 301,
		FloorEntered = 302,
		FloorCompleted = 303,
		FloorPathDiscovered = 304,
		FloorPathSelected = 305,
		FloorInitialized = 306,
		FloorsGenerated = 307,

		// Room Events (400-499)
		RoomDiscovered = 400,
		RoomStateChanged = 401,
		RoomEntered = 402,
		RoomExited = 403,
		RoomEventStarted = 404,
		RoomEventCompleted = 405,
		RoomEventFailed = 406,
		RoomPathDiscovered = 407,
		RoomChoicesAvailable = 408,
		RoomChoiceSelected = 409,
		RoomRewardGranted = 410,

		// Character Events (500-599)
		CharacterCreated = 500,
		CharacterStatChanged = 501,
		CharacterStatBuffApplied = 502,
		CharacterStatBuffRemoved = 503,
		CharacterStatBuffExpired = 504,
		CharacterStatThresholdCrossed = 505,
		CharacterStatThresholdReached = 506,
		CharacterLevelUp = 507,
		CharacterPrestige = 508,
		CharacterAscension = 509,

		// Combat and Reward Events (600-699)
		CombatStarted = 600,
		CombatUpdated = 601,
		CombatCompleted = 602,
		RewardGranted = 603,
		RewardCollected = 604,

		// Progress Events (700-799)
		ProgressUpdated = 700,
		StatisticsUpdated = 701,
		AchievementUnlocked = 702,
		Combat = 703,
		Reward = 704,
		Rest = 705,

		// Progression Events (800-899)
		ExperienceGained = 800,
		LevelMilestoneReached = 801,
		PrestigeLevelGained = 802,
		AscensionLevelGained = 803,
		ProgressionAchievementUpdated = 804,
		ProgressionMultiplierChanged = 805,
		ProgressionBonusUnlocked = 806,
		ProgressionMilestoneReached = 807,
		ProgressionRewardGranted = 808,
		ProgressionStatUpdated = 809,

		// System Events (1000+)
		GameTick = 1001,
		ResourceAmountChanged = 1002,
		ResourceUnlocked = 1003,
		MenuAction = 1004,
		ProgressionUpdated = 1005,

		GameAction = 2000,

		UIControlRegister = 4000,
		UIControlAssignToContainer = 4003,

		UIResourcePanelRegistration = 4000,


		System_ManagerError = 5000,
	}
}
