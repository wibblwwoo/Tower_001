using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static GlobalEnums;
// TestRoomGeneration
//Room System Tests
//Current Coverage:

//Room Generation
//Room Type Distribution
//Room Spacing Rules
//Room Difficulty Scaling
//Room State Transitions
//Room Progression System
//Room Failure System
//Complete Room Sequence
// Verifies:
// - Rooms are generated with valid properties
// - Room connections are valid
// - Room types are distributed according to rules
// - Room difficulty follows floor scaling
// - Special rooms (boss, rest) are placed correctly
public partial class RoomSystemTests : BaseTestSuite 
{


	#region Test Methods
	private readonly Random _random;
	private string _currentTowerId;
	
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private readonly TowerManager _towerManager;


	public RoomSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_random = new Random();
	}

	public void RunAllTests()
	{

		if (TestConfig.IsCategoryEnabled(TestCategory.Room))
		{
			TestRoomGeneration();
			TestRoomTypeDistribution();
			TestRoomSpacingRules();
			TestRoomDifficultyScaling();

			// Room state and progression tests
			TestRoomStateTransitions();
			TestRoomProgressionSystem();
			TestRoomFailureSystem();

			TestCompleteEntireTower();

			TestCompleteEntireTowerWithProgression();

			OutputTestResults();
		}

	}


	private void TestRoomGeneration()
	{
		string testName = "Room Generation";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();

			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}


			//var tower = _towerManager.GetAvailableTowers().First();


			var firstFloor = tower.Floors.First();

			// Reset room states before testing
			ResetRoomStates(firstFloor);

			// Test first floor room generation
			if (firstFloor.Rooms == null || !firstFloor.Rooms.Any())
			{
				testPassed = false;
				LogError(testName, "Initial floor has no rooms generated");
			}
			else
			{
				DebugLogger.Log($"First floor has {firstFloor.Rooms.Count} rooms generated", DebugLogger.LogCategory.Room);
			}

			// Verify first room is available and others are locked
			var rooms = firstFloor.Rooms;
			if (rooms.First().State != RoomState.Available)
			{
				testPassed = false;
				LogError(testName, "First room not in Available state");
			}

			if (rooms.Skip(1).Any(r => r.State != RoomState.Locked))
			{
				testPassed = false;
				LogError(testName, "Found subsequent rooms not in Locked state");
			}

			// Verify room properties
			foreach (var room in rooms)
			{
				if (string.IsNullOrEmpty(room.Id) || room.Type == RoomType.None)
				{
					testPassed = false;
					LogError(testName, $"Room {room.Id} has invalid properties");
				}
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestRoomTypeDistribution()
	{
		string testName = "Room Type Distribution";
		bool testPassed = true;

		try
		{

			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var floor = tower.Floors.First();

			ResetRoomStates(floor);

			var typeDistribution = floor.Rooms
				.GroupBy(r => r.Type)
				.ToDictionary(g => g.Key, g => (float)g.Count() / floor.Rooms.Count * 100);

			DebugLogger.Log("\nRoom Type Distribution:", DebugLogger.LogCategory.Room);
			foreach (var kvp in typeDistribution)
			{
				DebugLogger.Log($"{kvp.Key}: {kvp.Value:F1}%", DebugLogger.LogCategory.Room);
			}

			// Verify combat rooms are most common
			if (!typeDistribution.ContainsKey(RoomType.Combat) ||
				typeDistribution[RoomType.Combat] < 50)  // Expected ~70%
			{
				testPassed = false;
				LogError(testName, "Combat room distribution outside expected range");
			}

			// Verify boss/miniboss rooms are rare
			if (typeDistribution.GetValueOrDefault(RoomType.Boss) > 5 ||  // Expected ~2%
				typeDistribution.GetValueOrDefault(RoomType.MiniBoss) > 7)  // Expected ~4%
			{
				testPassed = false;
				LogError(testName, "Boss/MiniBoss room distribution outside expected range");
			}

			// Additional check to ensure the last room is always a FloorBoss
			if (floor.Rooms.Last().Type != RoomType.FloorBoss)
			{
				testPassed = false;
				LogError(testName, "Last room is not a FloorBoss");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCompleteEntireTower()
	{
		string testName = "Complete Tower Test";
		bool testPassed = true;

		try
		{

			//var tower = _towerManager.GetLockedTowers().First();
			var tower = _towerManager.GetAllTowers().First();

			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}



			var characterId = CreateTestCharacter(1);  // Start at level 1
			_playerManager.SetCurrentCharacter(characterId);

			DebugLogger.Log($"\nStarting tower completion test for tower {tower.Id}", DebugLogger.LogCategory.Room);

			// Enter tower
			bool enteredTower = _towerManager.TryEnterTower(tower.Id);
			if (!enteredTower)
			{
				testPassed = false;
				LogError(testName, "Failed to enter tower");
				LogTestResult(testName, testPassed);
				return;
			}

			// Track progress
			int floorsCompleted = 0;
			int totalRoomsCompleted = 0;

			foreach (var floor in tower.Floors)
			{
				DebugLogger.Log($"\nAttempting Floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);

				// Ensure character is strong enough for the floor
				AdjustCharacterForFloor(characterId, floor);

				bool enteredFloor = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
				if (!enteredFloor)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {floor.FloorNumber}");
					break;
				}

				// Complete all rooms in the floor
				bool floorCompleted = CompleteAllRoomsInFloor(tower.Id, floor, characterId);
				if (!floorCompleted)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {floor.FloorNumber}");
					break;
				}

				// Complete the floor
				bool completed = _towerManager.CompleteFloor(tower.Id, floor.Id);
				if (!completed)
				{
					testPassed = false;
					LogError(testName, $"Failed to mark floor {floor.FloorNumber} as completed");
					break;
				}

				floorsCompleted++;
				totalRoomsCompleted += floor.Rooms?.Count ?? 0;

				// Level up character every few floors
				_playerManager.SetCharacterLevel(characterId,
					_playerManager.GetCurrentCharacter().Level + 2);

				DebugLogger.Log($"Completed Floor {floor.FloorNumber}", DebugLogger.LogCategory.Room);
				DebugLogger.Log($"Character Level: {_playerManager.GetCurrentCharacter().Level}", DebugLogger.LogCategory.Room);
			}
			DebugLogger.Log($"\nTower Completion Results:", DebugLogger.LogCategory.Room);
			DebugLogger.Log($"Floors Completed: {floorsCompleted}", DebugLogger.LogCategory.Room);
			DebugLogger.Log($"Total Rooms Completed: {totalRoomsCompleted}", DebugLogger.LogCategory.Room);

			_towerManager.FailTower(tower.Id);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private bool CompleteAllRoomsInFloor(string towerId, FloorData floor, string characterId)
	{
		if (floor.Rooms == null || !floor.Rooms.Any())
		{
			DebugLogger.Log($"No rooms found in floor {floor.FloorNumber}", DebugLogger.LogCategory.Room);

			return false;
		}

		foreach (var room in floor.Rooms)
		{
			// Adjust character power based on room type
			AdjustCharacterForRoom(characterId, room);

			// Handle room completion based on type
			switch (room.Type)
			{
				case RoomType.Boss:
					DebugLogger.Log($"Attempting Boss Room on floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);
					break;
				case RoomType.MiniBoss:
					DebugLogger.Log($"Attempting MiniBoss Room on floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);
					break;
				case RoomType.Combat:
					// Regular combat rooms just need adequate power
					break;
			}

			if (room.State == RoomState.Locked)
			{
				room.State = RoomState.Available;
			}

			// Simulate room completion
			room.State = RoomState.Cleared;

			// Raise room completion event
			_eventManager?.RaiseEvent(
				EventType.RoomEventCompleted,
				new RoomCompletionEventArgs(
					towerId,
					"Test Tower",
					floor.FloorNumber,
					floor.Id,
					room.Id,
					true,
					TimeSpan.FromSeconds(1)
				)
			);
		}

		return true;
	}

	private void TestRoomSpacingRules()
	{
		string testName = "Room Spacing Rules";
		bool testPassed = true;

		try
		{

			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}

			var floor = tower.Floors.First();
			ResetRoomStates(floor);

			// Check Rest Room spacing
			var restRooms = floor.Rooms.Where(r => r.Type == RoomType.Rest).ToList();
			for (int i = 0; i < restRooms.Count - 1; i++)
			{
				int currentIndex = floor.Rooms.IndexOf(restRooms[i]);
				int nextIndex = floor.Rooms.IndexOf(restRooms[i + 1]);

				if (nextIndex - currentIndex < 3) // Minimum 3 rooms between rest rooms
				{
					testPassed = false;
					LogError(testName, $"Rest rooms too close: positions {currentIndex} and {nextIndex}");
				}
			}

			// Check MiniBoss/Boss spacing
			var bossRooms = floor.Rooms.Where(r => r.Type == RoomType.Boss || r.Type == RoomType.MiniBoss).ToList();
			for (int i = 0; i < bossRooms.Count - 1; i++)
			{
				int currentIndex = floor.Rooms.IndexOf(bossRooms[i]);
				int nextIndex = floor.Rooms.IndexOf(bossRooms[i + 1]);

				if (nextIndex - currentIndex < 5) // Minimum 5 rooms between boss rooms
				{
					testPassed = false;
					LogError(testName, $"Boss rooms too close: positions {currentIndex} and {nextIndex}");
				}
			}

			// Verify reward room placement
			var rewardRooms = floor.Rooms.Where(r => r.Type == RoomType.Reward).ToList();
			foreach (var rewardRoom in rewardRooms)
			{
				int roomIndex = floor.Rooms.IndexOf(rewardRoom);
				if (roomIndex == 0) // Reward rooms shouldn't be first
				{
					testPassed = false;
					LogError(testName, "Reward room placed at start of floor");
				}
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestRoomDifficultyScaling()
	{
		string testName = "Room Difficulty Scaling";
		bool testPassed = true;

		try
		{

			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var floor = tower.Floors.First();
			ResetRoomStates(floor);

			float previousDifficulty = 0;
			RoomType previousRoomType = RoomType.None;
			DebugLogger.Log("\nRoom Difficulty Scaling Analysis:", DebugLogger.LogCategory.Room);

			for (int i = 0; i < floor.Rooms.Count; i++)
			{
				var room = floor.Rooms[i];
				float currentDifficulty = room.BaseDifficulty;

				DebugLogger.Log($"Room {i + 1} ({room.Type}): Difficulty = {currentDifficulty:F2}", DebugLogger.LogCategory.Room);

				if (i > 0)
				{
					//boss and miniboss have a difficulty spike. so we need to cater for that here
					// Check progressive difficulty increase
					if (currentDifficulty < previousDifficulty && room.Type != RoomType.Rest && previousRoomType != RoomType.MiniBoss && previousRoomType != RoomType.Boss)
					{
						testPassed = false;
						LogError(testName, $"Difficulty decreased from {previousDifficulty:F2} to {currentDifficulty:F2}");
					}

					// Verify boss room difficulty multiplier
					if (room.Type == RoomType.Boss &&
						currentDifficulty < previousDifficulty * 1.5f)
					{
						testPassed = false;
						LogError(testName, "Boss room difficulty multiplier not applied correctly");
					}

					// Verify miniboss room difficulty multiplier
					if (room.Type == RoomType.MiniBoss &&
						currentDifficulty < previousDifficulty * 1.2f)
					{
						testPassed = false;
						LogError(testName, "MiniBoss room difficulty multiplier not applied correctly");
					}
				}
				previousRoomType = room.Type;
				previousDifficulty = currentDifficulty;
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestRoomStateTransitions()
	{
		string testName = "Room State Transitions";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var floor = tower.Floors.First();
			ResetRoomStates(floor);

			var firstRoom = floor.Rooms.First();
			var secondRoom = floor.Rooms.Skip(1).First();

			// Test initial states
			if (firstRoom.State != RoomState.Available)
			{
				testPassed = false;
				LogError(testName, "First room not in Available state");
			}

			if (secondRoom.State != RoomState.Locked)
			{
				testPassed = false;
				LogError(testName, "Second room not in Locked state");
			}

			// Test state transitions
			DebugLogger.Log("\nTesting room state transitions:", DebugLogger.LogCategory.Room);

			// Available -> Entered
			firstRoom.State = RoomState.Entered;
			DebugLogger.Log("First room: Available -> Entered", DebugLogger.LogCategory.Room);

			// Entered -> EventStarted
			firstRoom.State = RoomState.EventStarted;
			DebugLogger.Log("First room: Entered -> EventStarted", DebugLogger.LogCategory.Room);

			// EventStarted -> EventCompleted
			firstRoom.State = RoomState.EventCompleted;
			DebugLogger.Log("First room: EventStarted -> EventCompleted", DebugLogger.LogCategory.Room);

			// EventCompleted -> Cleared
			firstRoom.State = RoomState.Cleared;
			DebugLogger.Log("First room: EventCompleted->Cleared", DebugLogger.LogCategory.Room);


			// Verify second room becomes available
			secondRoom.State = RoomState.Available;
			if (secondRoom.State != RoomState.Available)
			{
				testPassed = false;
				LogError(testName, "Second room did not transition to Available state");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestRoomProgressionSystem()
	{
		string testName = "Room Progression System";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var floor = tower.Floors.First();
			ResetRoomStates(floor);

			var characterId = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(characterId);

			DebugLogger.Log("\nTesting room progression:", DebugLogger.LogCategory.Room);

			// Test progression through multiple rooms
			for (int i = 0; i < Math.Min(5, floor.Rooms.Count); i++)
			{
				var currentRoom = floor.Rooms[i];

				// Adjust character power for room
				AdjustCharacterForRoom(characterId, currentRoom);

				// Verify room is accessible
				if (currentRoom.State != RoomState.Available && i == 0)
				{
					testPassed = false;
					LogError(testName, $"Room {i} not available when it should be");
					break;
				}

				// Simulate room completion
				currentRoom.State = RoomState.Cleared;
				DebugLogger.Log($"Completed room {i + 1} ({currentRoom.Type})", DebugLogger.LogCategory.Room);
				// Verify next room becomes available
				if (i < floor.Rooms.Count - 1)
				{
					var nextRoom = floor.Rooms[i + 1];
					nextRoom.State = RoomState.Available;

					if (nextRoom.State != RoomState.Available)
					{
						testPassed = false;
						LogError(testName, $"Next room {i + 1} did not become available");
						break;
					}
				}
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestRoomFailureSystem()
	{
		string testName = "Room Failure System";
		bool testPassed = true;

		try
		{

			var tower = _towerManager.GetAllTowers().First();
			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var floor = tower.Floors.First();
			ResetRoomStates(floor);

			var characterId = CreateTestCharacter(1); // Start with weak character
			_playerManager.SetCurrentCharacter(characterId);

			DebugLogger.Log("\nTesting room failure scenarios:", DebugLogger.LogCategory.Room);

			// Test failure on normal room
			var firstRoom = floor.Rooms.First();
			firstRoom.State = RoomState.Failed;

			// Verify room state after failure
			if (firstRoom.State != RoomState.Failed)
			{
				testPassed = false;
				LogError(testName, "Room state not properly set to Failed");
			}

			// Test failure on boss room
			var bossRoom = floor.Rooms.FirstOrDefault(r => r.Type == RoomType.Boss);
			if (bossRoom != null)
			{
				bossRoom.State = RoomState.Failed;

				if (bossRoom.State != RoomState.Failed)
				{
					testPassed = false;
					LogError(testName, "Boss room state not properly set to Failed");
				}
			}

			// Test room retry after failure
			firstRoom.State = RoomState.Available;
			if (firstRoom.State != RoomState.Available)
			{
				testPassed = false;
				LogError(testName, "Room not properly reset after failure");
			}

			DebugLogger.Log("Completed room failure tests", DebugLogger.LogCategory.Room);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}


	private void TestCompleteEntireTowerWithProgression()
	{
		string testName = "Complete Tower Test With Progression";
		bool testPassed = true;
		//DebugLogger.SetLogging(true);
		//DebugLogger.SetLogCategories(DebugLogger.LogCategory.Progress | DebugLogger.LogCategory.Rewards | DebugLogger.LogCategory.Heartbeat);
		try
		{
			var tower = _towerManager.GetAllTowers().First();

			if (tower.CurrentState == TowerState.Locked)
			{
				var characterunlockId = CreateTestCharacter(1);  // Start at level 1
				_playerManager.SetCurrentCharacter(characterunlockId);
				_towerManager.UnlockTower(tower.Id);
			}
			var characterId = CreateTestCharacter(1);  // Start at level 1
			_playerManager.SetCurrentCharacter(characterId);

			DebugLogger.Log($"\nStarting tower progression test for tower {tower.Id}", DebugLogger.LogCategory.Progress);

			// Enter tower
			bool enteredTower = _towerManager.TryEnterTower(tower.Id);
			if (!enteredTower)
			{
				testPassed = false;
				LogError(testName, "Failed to enter tower");
				LogTestResult(testName, testPassed);
				return;
			}

			// Start tower heartbeat
			Globals.Instance.gameMangers.Heartbeat.Resume();

			// Track progress
			int floorsCompleted = 0;
			int totalRoomsCompleted = 0;
			float totalTimeSpent = 0f;

			foreach (var floor in tower.Floors)
			{
				DebugLogger.Log($"\nAttempting Floor {floor.FloorNumber}", DebugLogger.LogCategory.Progress);

				// Ensure character is strong enough for the floor
				AdjustCharacterForFloorWithProgression(characterId, floor);

				bool enteredFloor = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
				if (!enteredFloor)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {floor.FloorNumber}");
					break;
				}

				// Complete all rooms in the floor with progression
				bool floorCompleted = CompleteAllRoomsInFloorWithProgression(tower.Id, floor, characterId, ref totalTimeSpent);
				if (!floorCompleted)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {floor.FloorNumber}");
					break;
				}

				// Complete the floor
				bool completed = _towerManager.CompleteFloor(tower.Id, floor.Id);
				if (!completed)
				{
					testPassed = false;
					LogError(testName, $"Failed to mark floor {floor.FloorNumber} as completed");
					break;
				}

				floorsCompleted++;
				totalRoomsCompleted += floor.Rooms?.Count ?? 0;

				// Level up character every few floors
				_playerManager.SetCharacterLevel(characterId, _playerManager.GetCurrentCharacter().Level + 2);

				DebugLogger.Log($"Completed Floor {floor.FloorNumber}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Character Level: {_playerManager.GetCurrentCharacter().Level}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Time spent: {totalTimeSpent:F2} seconds", DebugLogger.LogCategory.Progress);
			}

			// Stop tower heartbeat
			Globals.Instance.gameMangers.Heartbeat.Pause();

			DebugLogger.Log($"\nTower Progression Results:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Floors Completed: {floorsCompleted}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Total Rooms Completed: {totalRoomsCompleted}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Total Time Spent: {totalTimeSpent:F2} seconds", DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}
	private bool CompleteAllRoomsInFloorWithProgression(string towerId, FloorData floor, string characterId, ref float totalTimeSpent)
	{
		if (floor.Rooms == null || !floor.Rooms.Any())
		{
			DebugLogger.Log($"No rooms found in floor {floor.FloorNumber}", DebugLogger.LogCategory.Progress);
			return false;
		}

		var rewardConfig = new RewardConfig();
		float totalFloorExperience = 0f;
		Dictionary<RewardTier, int> floorRewards = new();

		foreach (var room in floor.Rooms)
		{
			// Adjust character power based on room type
			AdjustCharacterForRoomWithProgression(characterId, room);

			// Calculate completion time based on power difference
			float completionTime = CalculateRoomCompletionTime(room, _playerManager.GetCurrentCharacter());

			// Simulate time passing via heartbeat
			SimulateTimeProgress(completionTime);
			totalTimeSpent += completionTime;

			// Calculate rewards for the room
			var roomRewards = RewardCalculator.CalculateRewards(room, floor, _playerManager.GetCurrentCharacter(), rewardConfig);
			totalFloorExperience += roomRewards.Experience;

			// Accumulate rewards by tier
			foreach (var (tier, quantity) in roomRewards.Rewards)
			{
				if (!floorRewards.ContainsKey(tier))
				{
					floorRewards[tier] = 0;
				}
				floorRewards[tier] += quantity;
			}

			// Handle room completion and logging
			switch (room.Type)
			{
				case RoomType.Boss:
					DebugLogger.Log($"Completed Boss Room on floor {floor.FloorNumber} in {completionTime:F2} seconds", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"Boss Rewards - Experience: {roomRewards.Experience:F0}, Items: {FormatRewards(roomRewards.Rewards)}", DebugLogger.LogCategory.Progress);
					break;
				case RoomType.MiniBoss:
					DebugLogger.Log($"Completed MiniBoss Room on floor {floor.FloorNumber} in {completionTime:F2} seconds", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"MiniBoss Rewards - Experience: {roomRewards.Experience:F0}, Items: {FormatRewards(roomRewards.Rewards)}", DebugLogger.LogCategory.Progress);
					break;
				default:
					DebugLogger.Log($"Completed {room.Type} Room in {completionTime:F2} seconds", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"Room Rewards - Experience: {roomRewards.Experience:F0}, Items: {FormatRewards(roomRewards.Rewards)}", DebugLogger.LogCategory.Progress);
					break;
			}

			if (room.State == RoomState.Locked)
			{
				room.State = RoomState.Available;
			}

			room.State = RoomState.Cleared;

			// Raise room completion event
			_eventManager?.RaiseEvent(
				EventType.RoomEventCompleted,
				new RoomCompletionEventArgs(
					towerId,
					"Test Tower",
					floor.FloorNumber,
					floor.Id,
					room.Id,
					true,
					TimeSpan.FromSeconds(completionTime)
				)
			);
		}

		// Log floor summary
		DebugLogger.Log($"\nFloor {floor.FloorNumber} Summary:", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Experience Gained: {totalFloorExperience:F0}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Rewards: {FormatRewards(floorRewards.Select(kv => (kv.Key, kv.Value)).ToList())}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Time Spent: {totalTimeSpent:F2} seconds\n", DebugLogger.LogCategory.Progress);

		return true;
	}
	#endregion

	#region Support Methods

	private void AdjustCharacterForFloorWithProgression(string characterId, FloorData floor)
	{
		var character = _playerManager.GetCurrentCharacter();
		float floorDifficulty = floor.Difficulty.CalculateFinalDifficulty(character.Level, character.Element);

		// Ensure character is strong enough for the floor
		if (character.Power < floorDifficulty)
		{
			// Calculate needed level increase
			int levelIncrease = (int)Math.Ceiling((floorDifficulty - character.Power) / 2);
			int newLevel = character.Level + levelIncrease;

			_playerManager.SetCharacterLevel(characterId, newLevel);
			DebugLogger.Log($"Adjusted character to level {newLevel} for floor {floor.FloorNumber} (Difficulty: {floorDifficulty:F2})", DebugLogger.LogCategory.Progress);
		}
		else
		{
			DebugLogger.Log($"Character power ({character.Power:F2}) sufficient for floor {floor.FloorNumber} (Difficulty: {floorDifficulty:F2})", DebugLogger.LogCategory.Progress);
		}
	}

	
	

	private string FormatRewards(List<(RewardTier tier, int quantity)> rewards)
	{
		return string.Join(", ", rewards.Select(r => $"{r.quantity}x {r.tier}"));
	}

	private string FormatRewards(Dictionary<RewardTier, int> rewards)
	{
		return string.Join(", ", rewards.Select(r => $"{r.Value}x {r.Key}"));
	}
	private void AdjustCharacterForRoomWithProgression(string characterId, RoomData room)
	{
		var character = _playerManager.GetCurrentCharacter();
		float requiredPower = CalculateRequiredPower(room);

		if (character.Power < requiredPower)
		{
			int levelIncrease = (int)Math.Ceiling((requiredPower - character.Power) / 2);
			int newLevel = character.Level + levelIncrease;

			_playerManager.SetCharacterLevel(characterId, newLevel);
			DebugLogger.Log($"Adjusted character to level {newLevel} for {room.Type} room", DebugLogger.LogCategory.Progress);
		}
	}

	private float CalculateRoomCompletionTime(RoomData room, Character character)
	{
		float baseTime = 10.0f;
		float powerRatio = character.Power / room.BaseDifficulty;

		// More aggressive time reduction based on power ratio
		// If character is 5x more powerful than needed, complete in 0.1 seconds
		// If character is equally powerful, takes full base time
		float timeMultiplier = Math.Max(0.01f, 1.0f / (powerRatio * powerRatio));

		// Room type adjustments
		float roomTypeMultiplier = room.Type switch
		{
			RoomType.Boss => 5.0f,
			RoomType.MiniBoss => 3.0f,
			RoomType.Combat => 1.0f,
			RoomType.Event => 0.5f,
			RoomType.Rest => 0.3f,
			RoomType.Reward => 0.2f,
			_ => 1.0f
		};

		float finalTime = baseTime * timeMultiplier * roomTypeMultiplier;

		// Cap minimum time based on room type
		float minimumTime = room.Type switch
		{
			RoomType.Boss => 1.0f,
			RoomType.MiniBoss => 0.5f,
			_ => 0.1f
		};

		return Math.Max(minimumTime, finalTime);
	}

	private void SimulateTimeProgress(float seconds)
	{
		const float simulationStep = 1.0f / 60.0f; // Simulate at 60fps
		float timeLeft = seconds;

		while (timeLeft > 0)
		{
			float delta = Math.Min(simulationStep, timeLeft);
			Globals.Instance.gameMangers.Heartbeat.Update(delta);
			timeLeft -= delta;
		}
	}

	private float CalculateRequiredPower(RoomData room)
	{
		float basePower = room.BaseDifficulty;

		switch (room.Type)
		{
			case RoomType.Boss:
				basePower *= 2.0f;
				break;
			case RoomType.MiniBoss:
				basePower *= 1.5f;
				break;
		}

		return basePower;
	}

	private void AdjustCharacterForFloor(string characterId, FloorData floor)
	{
		var character = _playerManager.GetCurrentCharacter();
		float floorDifficulty = floor.Difficulty.CalculateFinalDifficulty(
			character.Level,
			character.Element
		);

		// Ensure character is strong enough for the floor
		if (character.Power < floorDifficulty)
		{
			// Calculate needed level increase
			int levelIncrease = (int)Math.Ceiling((floorDifficulty - character.Power) / 2);
			int newLevel = character.Level + levelIncrease;

			_playerManager.SetCharacterLevel(characterId, newLevel);
			DebugLogger.Log($"Adjusted character to level {newLevel} for floor {floor.FloorNumber}", DebugLogger.LogCategory.Room);

		}
	}

	private void AdjustCharacterForRoom(string characterId, RoomData room)
	{
		var character = _playerManager.GetCurrentCharacter();
		float requiredPower = room.BaseDifficulty;

		// Add additional power requirements for special rooms
		switch (room.Type)
		{
			case RoomType.Boss:
				requiredPower *= 2.0f;  // Boss rooms need double power
				break;
			case RoomType.MiniBoss:
				requiredPower *= 1.5f;  // MiniBoss rooms need 1.5x power
				break;
		}

		if (character.Power < requiredPower)
		{
			// Calculate needed power increase
			int levelIncrease = (int)Math.Ceiling((requiredPower - character.Power) / 2);
			int newLevel = character.Level + levelIncrease;

			_playerManager.SetCharacterLevel(characterId, newLevel);
			DebugLogger.Log($"Adjusted character to level {newLevel} for {room.Type} room", DebugLogger.LogCategory.Room);

		}
	}

	private void ResetRoomStates(FloorData floor)
	{
		if (floor?.Rooms == null) return;

		foreach (var room in floor.Rooms)
		{
			// First room available, rest locked
			room.State = room == floor.Rooms.First() ?
				RoomState.Available : RoomState.Locked;

			// Reset any other room-specific states
			room.Tags = GenerateDefaultRoomTags(room.Type);
			room.ConnectedRoomIds.Clear();

			// Raise event for room state change
			_eventManager?.RaiseEvent(
				EventType.RoomStateChanged,
				new RoomStateChangedArgs(
					_currentTowerId,
					"Test Tower",
					floor.FloorNumber,
					floor.Id,
					room.Id,
					RoomState.Locked,
					room.State
				)
			);
		}
	}

	private HashSet<RoomTag> GenerateDefaultRoomTags(RoomType type)
	{
		var tags = new HashSet<RoomTag>();
		switch (type)
		{
			case RoomType.Combat:
				tags.Add(RoomTag.Combat);
				break;
			case RoomType.Rest:
				tags.Add(RoomTag.Rest);
				break;
			case RoomType.Reward:
				tags.Add(RoomTag.Treasure);
				break;
			case RoomType.Boss:
				tags.Add(RoomTag.Boss);
				tags.Add(RoomTag.Combat);
				break;
			case RoomType.MiniBoss:
				tags.Add(RoomTag.Elite);
				tags.Add(RoomTag.Combat);
				break;
		}
		tags.Add(RoomTag.Required);
		return tags;
	}

	private void ResetAllRoomsInFloor(string towerId, string floorId)
	{
		var tower = _towerManager.GetAvailableTowers().FirstOrDefault(t => t.Id == towerId);
		if (tower == null) return;

		var floor = tower.Floors.FirstOrDefault(f => f.Id == floorId);
		if (floor == null) return;

		ResetRoomStates(floor);
	}
	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
	}

	private void LogError(string testName, string message)
	{
		TestResults.Add($"Error in {testName}: {message}");
		GD.PrintErr($"Error in {testName}: {message}");
	}

	private void LogTestResult(string testName, bool passed)
	{
		string result = passed ? "PASSED" : "FAILED";
		TestResults.Add($"Test {testName}: {result}");
		GD.Print($"Test {testName}: {result}");
	}

	private void OutputTestResults()
	{
		GD.Print("\n=== Room System Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}
	#endregion 
}