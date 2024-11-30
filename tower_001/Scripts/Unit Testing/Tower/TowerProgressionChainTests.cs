using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static GlobalEnums;

public partial class TowerProgressionChainTests : BaseTestSuite
{
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private readonly TowerManager _towerManager;
	private int _floorsCompleted;

	private int _totalRoomsCompleted;
	private float _totalTimeSpent;
	private Dictionary<RewardTier, int> _totalRewards;
	private float _totalExperience;

	public TowerProgressionChainTests(EventManager eventManager, PlayerManager playerManager,
								  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_totalRewards = new Dictionary<RewardTier, int>();
	}

	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Tower_Progress))
		{
			TestSingleFloorProgression();
			TestMultiFloorProgression();
			TestProgressionChainRewards();
			TestProgressionScalingCurve();
			OutputTestResults();
		}
	}

	private void TestSingleFloorProgression()
	{
		string testName = "Single Floor Progression Chain";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			var characterId = CreateTestCharacter(1);
			_playerManager.SetCurrentCharacter(characterId);

			ResetProgression();

			DebugLogger.Log("\nStarting Single Floor Progression Test", DebugLogger.LogCategory.Progress);

			if (tower.CurrentState == TowerState.Locked)
			{
				_towerManager.UnlockTower(tower.Id);
			}

			bool enteredTower = _towerManager.TryEnterTower(tower.Id);
			if (!enteredTower)
			{
				testPassed = false;
				LogError(testName, "Failed to enter tower");
				LogTestResult(testName, testPassed);
				return;
			}

			// Test first floor progression
			var firstFloor = tower.Floors.First();

			// Ensure character is strong enough
			AdjustCharacterForFloor(characterId, firstFloor);

			bool enteredFloor = _towerManager.TryEnterFloor(tower.Id, firstFloor.Id, _playerManager.GetCurrentCharacter());
			if (!enteredFloor)
			{
				testPassed = false;
				LogError(testName, "Failed to enter first floor");
				return;
			}

			// Complete floor rooms with tracking
			bool floorCompleted = CompleteFloorWithProgressionTracking(tower.Id, firstFloor, characterId);
			if (!floorCompleted)
			{
				testPassed = false;
				LogError(testName, "Failed to complete floor progression");
				return;
			}

			// Verify progression metrics
			DebugLogger.Log("\nProgression Metrics:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Rooms Completed: {_totalRoomsCompleted}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Experience Gained: {_totalExperience}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Time Spent: {_totalTimeSpent:F2} seconds", DebugLogger.LogCategory.Progress);
			DebugLogger.Log("Rewards Gained:", DebugLogger.LogCategory.Progress);
			foreach (var reward in _totalRewards)
			{
				DebugLogger.Log($"{reward.Key}: {reward.Value}", DebugLogger.LogCategory.Progress);
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultiFloorProgression()
	{
		string testName = "Multi-Floor Progression Chain";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			var characterId = CreateTestCharacter(1);
			_playerManager.SetCurrentCharacter(characterId);

			ResetProgression();
			ResetTowerState(tower);
			ResetAllFloors(tower);

			DebugLogger.Log("\nStarting Multi-Floor Progression Test", DebugLogger.LogCategory.Progress);


			if (tower.CurrentState == TowerState.Locked)
			{
				_towerManager.UnlockTower(tower.Id);
			}

			bool enteredTower = _towerManager.TryEnterTower(tower.Id);
			if (!enteredTower)
			{
				testPassed = false;
				LogError(testName, "Failed to enter tower");
				return;
			}

			// Test progression through first 5 floors
			for (int i = 0; i < 5; i++)
			{
				var floor = tower.Floors[i];

				DebugLogger.Log($"\nAttempting Floor {floor.FloorNumber}", DebugLogger.LogCategory.Progress);

				// Scale character appropriately
				AdjustCharacterForFloor(characterId, floor);

				bool enteredFloor = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
				if (!enteredFloor)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {i + 1}");
					break;
				}

				bool floorCompleted = CompleteFloorWithProgressionTracking(tower.Id, floor, characterId);
				if (!floorCompleted)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {i + 1}");
					break;
				}

				// Verify progression curve
				VerifyProgressionCurve(floor.FloorNumber);
			}

			// Output final progression metrics
			LogProgressionMetrics();
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestProgressionChainRewards()
	{
		string testName = "Progression Chain Rewards";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			var characterId = CreateTestCharacter(1);
			_playerManager.SetCurrentCharacter(characterId);

			ResetProgression();
			ResetProgression();
			ResetTowerState(tower);
			ResetAllFloors(tower);

			// Track rewards through multiple floors
			var rewardsPerFloor = new List<Dictionary<RewardTier, int>>();

			for (int i = 0; i < 3; i++)
			{
				var floor = tower.Floors[i];
				AdjustCharacterForFloor(characterId, floor);

				if (_towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter()))
				{
					bool completed = CompleteFloorWithProgressionTracking(tower.Id, floor, characterId);
					if (completed)
					{
						rewardsPerFloor.Add(new Dictionary<RewardTier, int>(_totalRewards));
					}
				}
			}

			// Verify reward scaling
			VerifyRewardScaling(rewardsPerFloor);
			LogRewardProgression(rewardsPerFloor);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestProgressionScalingCurve()
	{
		string testName = "Progression Scaling Curve";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAllTowers().First();
			var characterId = CreateTestCharacter(1);
			_playerManager.SetCurrentCharacter(characterId);
			ResetProgression();
			ResetTowerState(tower);
			ResetAllFloors(tower);


			var difficultyPoints = new List<(int floor, float difficulty)>();
			var experiencePoints = new List<(int floor, float experience)>();

			// Collect scaling data through floors
			for (int i = 0; i < 5; i++)
			{
				var floor = tower.Floors[i];
				float difficulty = floor.Difficulty.CalculateFinalDifficulty(
					_playerManager.GetCurrentCharacter().Level,
					_playerManager.GetCurrentCharacter().Element
				);
				difficultyPoints.Add((i + 1, difficulty));


				AdjustCharacterForFloor(characterId, floor);

				if (_towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter()))
				{
					CompleteFloorWithProgressionTracking(tower.Id, floor, characterId);
					experiencePoints.Add((i + 1, _totalExperience));
				}
			}

			// Verify scaling curves
			VerifyScalingCurves(difficultyPoints, experiencePoints);
			LogScalingData(difficultyPoints, experiencePoints);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	#region Support Methods
	private void ResetProgression()
	{

		_floorsCompleted = 0;
		_totalRoomsCompleted = 0;
		_totalTimeSpent = 0;
		_totalRewards.Clear();
		_totalExperience = 0;
	}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
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

	private bool CompleteFloorWithProgressionTracking(string towerId, FloorData floor, string characterId)
	{
		if (floor.Rooms == null || !floor.Rooms.Any())
			return false;

		var rewardConfig = new RewardConfig();

		foreach (var room in floor.Rooms)
		{
			float completionTime = CalculateRoomCompletionTime(room, _playerManager.GetCurrentCharacter());
			_totalTimeSpent += completionTime;

			var roomRewards = RewardCalculator.CalculateRewards(room, floor, _playerManager.GetCurrentCharacter(), rewardConfig);
			_totalExperience += roomRewards.Experience;

			foreach (var (tier, quantity) in roomRewards.Rewards)
			{
				if (!_totalRewards.ContainsKey(tier))
					_totalRewards[tier] = 0;
				_totalRewards[tier] += quantity;
			}

			_totalRoomsCompleted++;
			room.State = RoomState.Cleared;
		}

		_floorsCompleted++;
		return _towerManager.CompleteFloor(towerId, floor.Id);
	}

	private float CalculateRoomCompletionTime(RoomData room, Character character)
	{
		float baseTime = 10.0f;
		float powerRatio = character.Power / room.BaseDifficulty;
		float timeMultiplier = Math.Max(0.01f, 1.0f / (powerRatio * powerRatio));

		return Math.Max(0.1f, baseTime * timeMultiplier);
	}

	private void VerifyProgressionCurve(int floorNumber)
	{
		DebugLogger.Log($"\nFloor {floorNumber} Progression:", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Experience Rate: {_totalExperience / _totalTimeSpent:F2} exp/s", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Rooms per Second: {_totalRoomsCompleted / _totalTimeSpent:F2}", DebugLogger.LogCategory.Progress);
	}

	private void VerifyRewardScaling(List<Dictionary<RewardTier, int>> rewardsPerFloor)
	{
		for (int i = 1; i < rewardsPerFloor.Count; i++)
		{
			var prevFloor = rewardsPerFloor[i - 1];
			var currentFloor = rewardsPerFloor[i];

			DebugLogger.Log($"\nReward Scaling Floor {i}:", DebugLogger.LogCategory.Progress);
			foreach (var tier in currentFloor.Keys)
			{
				float scalingFactor = prevFloor.ContainsKey(tier) ?
					(float)currentFloor[tier] / prevFloor[tier] : 0;
				DebugLogger.Log($"{tier}: x{scalingFactor:F2}", DebugLogger.LogCategory.Progress);
			}
		}
	}

	private void VerifyScalingCurves(
		List<(int floor, float difficulty)> difficultyPoints,
		List<(int floor, float experience)> experiencePoints)
	{
		DebugLogger.Log("\nScaling Curves:", DebugLogger.LogCategory.Progress);
		for (int i = 1; i < difficultyPoints.Count; i++)
		{
			float difficultyScale = difficultyPoints[i].difficulty / difficultyPoints[i - 1].difficulty;
			float experienceScale = experiencePoints[i].experience / experiencePoints[i - 1].experience;

			DebugLogger.Log($"Floor {i + 1}:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Difficulty Scaling: x{difficultyScale:F2}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Experience Scaling: x{experienceScale:F2}", DebugLogger.LogCategory.Progress);
		}
	}

	private void LogProgressionMetrics()
	{
		DebugLogger.Log("\nFinal Progression Metrics:", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Floors Completed: {_floorsCompleted}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Rooms: {_totalRoomsCompleted}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Experience: {_totalExperience:F0}", DebugLogger.LogCategory.Progress);
		DebugLogger.Log($"Total Time: {_totalTimeSpent:F2}s", DebugLogger.LogCategory.Progress);
	}
		private void LogRewardProgression(List<Dictionary<RewardTier, int>> rewardsPerFloor)
		{
			DebugLogger.Log("\nReward Progression by Floor:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < rewardsPerFloor.Count; i++)
			{
				DebugLogger.Log($"\nFloor {i + 1} Rewards:", DebugLogger.LogCategory.Progress);
				foreach (var reward in rewardsPerFloor[i])
				{
					DebugLogger.Log($"{reward.Key}: {reward.Value}", DebugLogger.LogCategory.Progress);
				}
			}
		}

		private void LogScalingData(
			List<(int floor, float difficulty)> difficultyPoints,
			List<(int floor, float experience)> experiencePoints)
		{
			DebugLogger.Log("\nDetailed Scaling Data:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < difficultyPoints.Count; i++)
			{
				var (floor, difficulty) = difficultyPoints[i];
				var experience = experiencePoints[i].experience;

				DebugLogger.Log($"\nFloor {floor}:", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Difficulty: {difficulty:F2}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Experience: {experience:F0}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Exp/Difficulty Ratio: {experience / difficulty:F2}", DebugLogger.LogCategory.Progress);
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
		#endregion
	}