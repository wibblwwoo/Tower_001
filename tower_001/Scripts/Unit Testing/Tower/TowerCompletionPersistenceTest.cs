using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class TowerCompletionPersistenceTest : BaseTestSuite
{
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private readonly TowerManager _towerManager;

	private int _totalAttempts;
	private float _totalTimeSpent;
	private Dictionary<int, int> _failuresByFloor;
	private Dictionary<int, (int level, float power)> _progressByAttempt;
	private List<(int floor, string reason)> _failureReasons;

	public TowerCompletionPersistenceTest(EventManager eventManager, PlayerManager playerManager,
									  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_failuresByFloor = new Dictionary<int, int>();
		_progressByAttempt = new Dictionary<int, (int level, float power)>();
		_failureReasons = new List<(int floor, string reason)>();
	}

	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Tower_Progress))
		{
			TestTowerCompletionPersistence();
			OutputTestResults();
		}
	}

	private void TestTowerCompletionPersistence()
	{
		string testName = "Tower Completion Persistence";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			var characterId = CreateTestCharacter(1);  // Start at level 1
			_playerManager.SetCurrentCharacter(characterId);

			ResetTrackingMetrics();
			DateTime startTime = DateTime.Now;

			bool towerCompleted = false;
			int maxAttempts = 100; // Safeguard against infinite loops

			DebugLogger.Log("\nStarting Tower Completion Persistence Test", DebugLogger.LogCategory.Progress);

			while (!towerCompleted && _totalAttempts < maxAttempts)
			{
				_totalAttempts++;
				bool thisAttemptFailed = false;

				// Track character stats at start of attempt
				_progressByAttempt[_totalAttempts] = (_playerManager.GetCurrentCharacter().Level,
													 _playerManager.GetCurrentCharacter().Power);

				DebugLogger.Log($"\nAttempt #{_totalAttempts}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Character Level: {_playerManager.GetCurrentCharacter().Level}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Character Power: {_playerManager.GetCurrentCharacter().Power:F2}", DebugLogger.LogCategory.Progress);

				// Enter tower
				if (tower.CurrentState == TowerState.Locked)
				{
					_towerManager.UnlockTower(tower.Id);
				}
				bool enteredTower = _towerManager.TryEnterTower(tower.Id);

				if (!enteredTower)
				{
					LogFailure(0, "Failed to enter tower");
					continue;
				}

				int i = 0;
				// Attempt each floor
				foreach (var floor in tower.Floors)
				{
					if (i == 99)
					{
						int k = 1;

					}
					bool enteredFloor = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
					if (!enteredFloor)
					{
						IncrementFloorFailure(floor.FloorNumber);
						LogFailure(floor.FloorNumber, "Insufficient power for floor");
						thisAttemptFailed = true;
						break;
					}

					// Try to complete the floor
					try
					{
						bool floorCompleted = AttemptFloorCompletion(tower.Id, floor, characterId);
						if (!floorCompleted)
						{
							IncrementFloorFailure(floor.FloorNumber);
							LogFailure(floor.FloorNumber, "Failed to complete floor");
							thisAttemptFailed = true;
							break;
						}
					}
					catch (Exception ex)
					{
						IncrementFloorFailure(floor.FloorNumber);
						LogFailure(floor.FloorNumber, $"Error: {ex.Message}");
						thisAttemptFailed = true;
						break;
					}
					i++;
				}

				if (thisAttemptFailed)
				{
					_towerManager.FailTower(tower.Id);
					// Level up character after failure to simulate progression
					_playerManager.SetCharacterLevel(characterId, _playerManager.GetCurrentCharacter().Level + 2);
				}
				else
				{
					towerCompleted = true;
				}
			}

			_totalTimeSpent = (float)(DateTime.Now - startTime).TotalSeconds;

			if (!towerCompleted)
			{
				testPassed = false;
				LogError(testName, $"Failed to complete tower after {maxAttempts} attempts");
			}

			LogCompletionResults();
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	#region Support Methods
	private void ResetTrackingMetrics()
	{
		_totalAttempts = 0;
		_totalTimeSpent = 0;
		_failuresByFloor.Clear();
		_progressByAttempt.Clear();
		_failureReasons.Clear();
	}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
	}

	private void IncrementFloorFailure(int floorNumber)
	{
		if (!_failuresByFloor.ContainsKey(floorNumber))
		{
			_failuresByFloor[floorNumber] = 0;
		}
		_failuresByFloor[floorNumber]++;
	}

	private void LogFailure(int floor, string reason)
	{
		_failureReasons.Add((floor, reason));
	}

	private bool AttemptFloorCompletion(string towerId, FloorData floor, string characterId)
	{
		if (floor.Rooms == null || !floor.Rooms.Any())
			return false;

		var rewardConfig = new RewardConfig();
		var character = _playerManager.GetCurrentCharacter();

		foreach (var room in floor.Rooms)
		{
			// Check if character is strong enough for the room
			float requiredPower = CalculateRequiredPower(room);
			if (character.Power < requiredPower)
			{
				LogFailure(floor.FloorNumber, $"Room power check failed: Character power {character.Power:F2} < Required {requiredPower:F2}");
				return false;
			}

			try
			{
				// Attempt room completion with tracking
				float completionTime = CalculateRoomCompletionTime(room, character);
				_totalTimeSpent += completionTime;

				var roomRewards = RewardCalculator.CalculateRewards(room, floor, character, rewardConfig);

				// Boss/MiniBoss rooms have additional power checks
				if ((room.Type == RoomType.Boss || room.Type == RoomType.MiniBoss) &&
					character.Power < requiredPower * 1.2f)
				{
					LogFailure(floor.FloorNumber, $"Boss/MiniBoss power check failed: Character power {character.Power:F2} < Required {requiredPower * 1.2f:F2}");
					return false;
				}

				room.State = RoomState.Cleared;
			}
			catch (Exception ex)
			{
				LogFailure(floor.FloorNumber, $"Room completion error: {ex.Message}");
				return false;
			}
		}

		return _towerManager.CompleteFloor(towerId, floor.Id);
	}

	private float CalculateRequiredPower(RoomData room)
	{
		float basePower = room.BaseDifficulty;

		switch (room.Type)
		{
			case RoomType.Boss:
				basePower *= 2.0f;  // Boss rooms need double power
				break;
			case RoomType.MiniBoss:
				basePower *= 1.5f;  // MiniBoss rooms need 1.5x power
				break;
			case RoomType.Combat:
				basePower *= 1.2f;  // Combat rooms need 1.2x power
				break;
		}

		return basePower;
	}

	private void LogCompletionResults()
	{
		DebugLogger.Log("\nTower Completion Results:", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Attempts: {_totalAttempts}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Time: {_totalTimeSpent:F2} seconds", DebugLogger.LogCategory.Progress);

		DebugLogger.Log("\nFailures by Floor:", DebugLogger.LogCategory.Progress);
		foreach (var failure in _failuresByFloor.OrderBy(f => f.Key))
		{
			DebugLogger.Log($"Floor {failure.Key}: {failure.Value} failures", DebugLogger.LogCategory.Progress);
		}

		DebugLogger.Log("\nProgression by Attempt:", DebugLogger.LogCategory.Progress);
		foreach (var progress in _progressByAttempt.OrderBy(p => p.Key))
		{
			DebugLogger.Log($"Attempt {progress.Key}: Level {progress.Value.level}, Power {progress.Value.power:F2}",
						   DebugLogger.LogCategory.Progress);
		}

		DebugLogger.Log("\nFirst 5 Failure Reasons:", DebugLogger.LogCategory.Progress);
		foreach (var failure in _failureReasons.Take(5))
		{
			DebugLogger.Log($"Floor {failure.floor}: {failure.reason}", DebugLogger.LogCategory.Progress);
		}
	}

	private void LogError(string testName, string message)
	{
		TestResults.Add($"Error in {testName}: {message}");
		DebugLogger.Log($"Error in {testName}: {message}", DebugLogger.LogCategory.Progress);
	}

	private void LogTestResult(string testName, bool passed)
	{
		string result = passed ? "PASSED" : "FAILED";
		TestResults.Add($"Test {testName}: {result}");
		DebugLogger.Log($"Test {testName}: {result}", DebugLogger.LogCategory.Progress);
	}

	private void OutputTestResults()
	{
		DebugLogger.Log("\n=== Tower Completion Persistence Test Results ===", DebugLogger.LogCategory.Progress);
		foreach (var result in TestResults)
		{
			DebugLogger.Log(result, DebugLogger.LogCategory.Progress);
		}
		DebugLogger.Log("===============================\n", DebugLogger.LogCategory.Progress);
	}

	private float CalculateRoomCompletionTime(RoomData room, Character character)
	{
		float baseTime = 10.0f;
		float powerRatio = character.Power / room.BaseDifficulty;
		float timeMultiplier = Math.Max(0.01f, 1.0f / (powerRatio * powerRatio));

		return Math.Max(0.1f, baseTime * timeMultiplier);
	}
	#endregion
}