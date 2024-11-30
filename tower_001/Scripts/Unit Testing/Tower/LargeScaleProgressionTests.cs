using Godot;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

public partial class LargeScaleProgressionTestsv2 : BaseTestSuite
{
	private readonly TowerManager _towerManager;
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private readonly Random _random;

	// Test tracking
	private int _floorsCompleted;
	private int _totalRoomsCompleted;
	private float _totalTimeSpent;
	private Dictionary<RewardTier, int> _totalRewards;
	private float _totalExperience;

	// Configuration for large-scale testing
	private const int TARGET_FLOOR_COUNT = 10000;
	private const int PERFORMANCE_LOG_INTERVAL = 1000;

	// Performance tracking
	private Dictionary<string, List<double>> _performanceMetrics;

	public LargeScaleProgressionTestsv2(EventManager eventManager, PlayerManager playerManager,
									TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_random = new Random();
		_totalRewards = new Dictionary<RewardTier, int>();
		InitializePerformanceMetrics();
	}

	private void InitializePerformanceMetrics()
	{
		_performanceMetrics = new Dictionary<string, List<double>>
		{
			{ "FloorGeneration", new List<double>() },
			{ "RoomGeneration", new List<double>() },
			{ "FloorCompletion", new List<double>() },
			{ "StateUpdates", new List<double>() }
		};
	}

	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Performance))
		{
			TestLargeScaleTowerProgression();
			TestMemoryUsageUnderLoad();
			OutputTestResults();
		}
	}

	private void TestLargeScaleTowerProgression()
	{
		string testName = "Large Scale Tower Progression";
		bool testPassed = true;
		Stopwatch totalTimer = new Stopwatch();
		totalTimer.Start();

		try
		{
			// Reset all progression tracking
			ResetProgression();

			// Create and setup test character
			var characterId = CreateTestCharacter(1000);
			_playerManager.SetCurrentCharacter(characterId);

			var tower = _towerManager.GetAllTowers().First();
			if (tower == null)
			{
				testPassed = false;
				DebugLogger.Log($"[{testName}] ERROR: Failed to get test tower", DebugLogger.LogCategory.Error);
				return;
			}

			DebugLogger.Log($"\nStarting large-scale progression test", DebugLogger.LogCategory.Progress);

			// Track initial memory state
			var initialMemory = GC.GetTotalMemory(true);

			// Process floors
			for (int currentFloor = 1; currentFloor <= TARGET_FLOOR_COUNT; currentFloor++)
			{
				if (!ProcessFloor(tower, currentFloor, initialMemory, testName))
				{
					testPassed = false;
					break;
				}
			}

			// Verify final state
			if (!VerifyFinalTowerState(tower))
			{
				testPassed = false;
				DebugLogger.Log($"[{testName}] ERROR: Final tower state verification failed", DebugLogger.LogCategory.Error);
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			DebugLogger.Log($"[{testName}] ERROR: {ex.Message}", DebugLogger.LogCategory.Error);
		}

		totalTimer.Stop();
		LogTestResult(testName, testPassed);
		LogProgressionMetrics();
	}

	private bool ProcessFloor(TowerData tower, int floorNumber, long initialMemory, string testName)
	{
		Stopwatch floorTimer = new Stopwatch();
		floorTimer.Start();

		try
		{
			var floor = tower.Floors[floorNumber - 1];
			if (floor == null)
			{
				DebugLogger.Log($"[{testName}] ERROR: Failed to get floor {floorNumber}", DebugLogger.LogCategory.Error);
				return false;
			}

			// Enter floor
			if (!_towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter()))
			{
				DebugLogger.Log($"[{testName}] ERROR: Failed to enter floor {floorNumber}", DebugLogger.LogCategory.Error);
				return false;
			}

			// Complete rooms
			if (!CompleteFloorWithProgressionTracking(tower.Id, floor, _playerManager.GetCurrentCharacter().Id))
			{
				DebugLogger.Log($"[{testName}] ERROR: Failed to complete floor {floorNumber}", DebugLogger.LogCategory.Error);
				return false;
			}

			// Log progress at intervals
			if (floorNumber % PERFORMANCE_LOG_INTERVAL == 0)
			{
				LogProgressInterval(floorNumber, initialMemory);
			}

			floorTimer.Stop();
			_performanceMetrics["FloorCompletion"].Add(floorTimer.Elapsed.TotalMilliseconds);
			return true;
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"[{testName}] ERROR: Error processing floor {floorNumber}: {ex.Message}", DebugLogger.LogCategory.Error);
			return false;
		}
	}

	private void TestMemoryUsageUnderLoad()
	{
		string testName = "Memory Usage Under Load";
		bool testPassed = true;

		try
		{
			ResetProgression();
			var characterId = CreateTestCharacter(1000);
			_playerManager.SetCurrentCharacter(characterId);

			var tower = _towerManager.GetAllTowers().First();
			if (tower == null)
			{
				testPassed = false;
				DebugLogger.Log($"[{testName}] ERROR: Failed to get test tower", DebugLogger.LogCategory.Error);
				return;
			}

			var initialMemory = GC.GetTotalMemory(true);
			var memoryCheckpoints = new List<double>();

			// Process floors in batches and check memory
			for (int i = 0; i < 5; i++)
			{
				ProcessFloorBatch(tower, i * 2000, (i + 1) * 2000);
				GC.Collect();
				var currentMemory = GC.GetTotalMemory(true);
				memoryCheckpoints.Add((currentMemory - initialMemory) / 1024.0 / 1024.0);
			}

			// Verify memory usage
			if (!VerifyMemoryUsage(memoryCheckpoints))
			{
				testPassed = false;
				DebugLogger.Log($"[{testName}] ERROR: Memory usage exceeded acceptable threshold", DebugLogger.LogCategory.Error);
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			DebugLogger.Log($"[{testName}] ERROR: {ex.Message}", DebugLogger.LogCategory.Error);
		}

		LogTestResult(testName, testPassed);
	}

	private bool CompleteFloorWithProgressionTracking(string towerId, FloorData floor, string characterId)
	{
		if (floor.Rooms == null || !floor.Rooms.Any())
			return false;

		foreach (var room in floor.Rooms)
		{
			float completionTime = CalculateRoomCompletionTime(room, _playerManager.GetCurrentCharacter());
			_totalTimeSpent += completionTime;
			_totalRoomsCompleted++;

			if (_towerManager.CompleteFloor(towerId, floor.Id) == false)
			{
				return false;
			}
		}

		_floorsCompleted++;
		return true;
	}

	private float CalculateRoomCompletionTime(RoomData room, Character character)
	{
		float baseTime = 10.0f;
		float powerRatio = character.Power / room.BaseDifficulty;
		float timeMultiplier = Math.Max(0.01f, 1.0f / (powerRatio * powerRatio));

		return Math.Max(0.1f, baseTime * timeMultiplier);
	}

	private void AdjustCharacterForFloor(string characterId, FloorData floor)
	{
		var character = _playerManager.GetCurrentCharacter();
		float floorDifficulty = floor.Difficulty.CalculateFinalDifficulty(
			character.Level,
			character.Element
		);

		if (character.Power < floorDifficulty)
		{
			int levelIncrease = (int)Math.Ceiling((floorDifficulty - character.Power) / 2);
			int newLevel = character.Level + levelIncrease;
			_playerManager.SetCharacterLevel(characterId, newLevel);
		}
	}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
	}

	private bool VerifyFinalTowerState(TowerData tower)
	{
		DebugLogger.Log("\nVerifying final tower state:", DebugLogger.LogCategory.Progress);

		foreach (var floor in tower.Floors)
		{
			var floorState = _towerManager.GetFloorState(tower.Id, floor.Id);
			if (floorState != FloorState.Completed)
			{
				DebugLogger.Log($"Floor {floor.FloorNumber} state incorrect. Expected: Completed, Got: {floorState}", DebugLogger.LogCategory.Error);
				return false;
			}
		}

		DebugLogger.Log("Tower state verification passed", DebugLogger.LogCategory.Progress);
		return true;
	}

	private void ResetProgression()
	{
		_floorsCompleted = 0;
		_totalRoomsCompleted = 0;
		_totalTimeSpent = 0;
		_totalRewards.Clear();
		_totalExperience = 0;
		InitializePerformanceMetrics();
	}

	private void LogProgressInterval(int currentFloor, long initialMemory)
	{
		var currentMemory = GC.GetTotalMemory(false);
		var memoryDelta = (currentMemory - initialMemory) / 1024.0 / 1024.0;
		DebugLogger.Log($"\nProgress - Floor {currentFloor}/{TARGET_FLOOR_COUNT}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Memory Delta: {memoryDelta:F2}MB", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Rooms Completed: {_totalRoomsCompleted}", DebugLogger.LogCategory.Progress);
	}

	private void LogProgressionMetrics()
	{
		DebugLogger.Log("\nFinal Progression Metrics:", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Floors Completed: {_floorsCompleted}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Rooms: {_totalRoomsCompleted}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Time: {_totalTimeSpent:F2} seconds", DebugLogger.LogCategory.Progress);

		DebugLogger.Log("\nPerformance Metrics:", DebugLogger.LogCategory.Progress);
		foreach (var metric in _performanceMetrics)
		{
			if (metric.Value.Any())
			{
				DebugLogger.Log($"{metric.Key} - Avg: {metric.Value.Average():F2}ms", DebugLogger.LogCategory.Progress);
			}
		}
	}

	private void LogTestResult(string testName, bool passed)
	{
		string result = passed ? "PASSED" : "FAILED";
		DebugLogger.Log($"\nTest Result: {testName} - {result}", DebugLogger.LogCategory.Progress);
	}

	private bool VerifyMemoryUsage(List<double> memoryCheckpoints)
	{
		double maxAllowedGrowth = 100.0; // MB
		return memoryCheckpoints.All(checkpoint => checkpoint < maxAllowedGrowth);
	}

	private void ProcessFloorBatch(TowerData tower, int startFloor, int endFloor)
	{
		for (int i = startFloor; i < endFloor && i < tower.Floors.Count; i++)
		{
			var floor = tower.Floors[i];
			if (_towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter()))
			{
				CompleteFloorWithProgressionTracking(tower.Id, floor, _playerManager.GetCurrentCharacter().Id);
			}
		}
	}
	private void ResetAllRoomsInFloor(string towerId, string floorId)
	{
		var tower = _towerManager.GetAvailableTowers().FirstOrDefault(t => t.Id == towerId);
		if (tower == null) return;

		var floor = tower.Floors.FirstOrDefault(f => f.Id == floorId);
		if (floor == null) return;

		ResetRoomStates(floor, tower);
	}

	private void ResetRoomStates(FloorData floor, TowerData tower)
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
					tower.Id,
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
	private void ResetFloorState(TowerData tower, FloorData floor)
	{
		if (floor != null)
		{
			_towerManager.ResetFloorState(tower.Id, floor.Id);
		}
	}
	private void ResetAllFloors(TowerData tower)
	{
		foreach (var floor in tower.Floors)
		{
			ResetFloorState(tower, floor);
			ResetRoomStates(floor, tower);
		}
	}
	/// <summary>
	/// Helper method to reset a tower's state to Available
	/// </summary>
	private void ResetTowerState(TowerData tower)
	{
		if (tower != null)
		{
			var previousState = tower.CurrentState;
			tower.CurrentState = TowerState.Available;
			if (_eventManager != null)
			{
				_eventManager.RaiseEvent(EventType.TowerStateChanged,
					new TowerEventArgs(tower.Id, previousState, TowerState.Available));
			}
		}
	}

	private void OutputTestResults()
	{
		DebugLogger.Log("\n=== Tower Progression Chain Test Results ===", DebugLogger.LogCategory.Progress);
		foreach (var result in TestResults)
		{
			DebugLogger.Log(result, DebugLogger.LogCategory.Progress);
		}
		DebugLogger.Log("===============================\n", DebugLogger.LogCategory.Progress);
	}

}

