using Godot;
using System;

public static partial class GlobalEnums
{
	// Called when the node enters the scene tree for the first time.

	public enum EventType
	{

		None = 0,
		UIButton_Click,
		UIButton_Enable,
		UIButton_Visibility,
		UIButton_SetLabel,


		UIPanel_Enable,
		UIPanel_Visibility,


		// Existing events...
		// Tower Events
		TowerInitialized = 200,
		TowerStateChanged = 201,
		TowerEntered = 202,
		TowerExited = 203,    // Added
		TowerFailed = 204,    // Added
		TowerCompleted = 205,

		// Floor Events (300-399) - Need to Add
		FloorDiscovered = 300,
		FloorStateChanged = 301,
		FloorEntered = 302,
		FloorCompleted = 303,
		FloorPathDiscovered = 304,
		FloorPathSelected = 305,
		FloorInitialized = 306, // In the Floor Events 300-399 range
		FloorsGenerated = 307,  // New event for when all floors are generated

		// Room Events
		// Room Events (400-499) - Need to Add
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

		// Character Events
		CharacterCreated = 500,
		CharacterStatChanged = 501,
		CharacterStatBuffApplied = 502,
		CharacterStatBuffRemoved = 503,
		CharacterStatBuffExpired = 504,
		CharacterStatThresholdCrossed = 505,
		CharacterLevelUp = 506,
		CharacterPrestige = 507,
		CharacterAscension = 508,

	

		// Event Processing (500-599) - Need to Add
		CombatStarted = 500,
		CombatUpdated = 501,
		CombatCompleted = 502,
		RewardGranted = 503,
		RewardCollected = 504,

		// Progress Events (600-699) - Need to Add
		ProgressUpdated = 600,
		StatisticsUpdated = 601,
		AchievementUnlocked = 602,
		Combat = 603,
		Reward = 604,
		Rest = 605,

		// Progression Events (700-799)
		ExperienceGained = 700,
		LevelMilestoneReached = 701,
		PrestigeLevelGained = 702,
		AscensionLevelGained = 703,
		ProgressionAchievementUpdated = 704,
		ProgressionMultiplierChanged = 705,
		ProgressionBonusUnlocked = 706,
		ProgressionMilestoneReached = 707,
		ProgressionRewardGranted = 708,
		ProgressionStatUpdated = 709,

		GameTick = 1001,
		ResourceAmountChanged = 1002,
		ResourceUnlocked = 1003,
	}


}
