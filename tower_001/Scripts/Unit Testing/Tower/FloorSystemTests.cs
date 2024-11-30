using Godot;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using static Godot.WebSocketPeer;


// TestFloorProgressionSystem
//Current Coverage:

//Floor Unlock System
//Floor Difficulty Scaling
//Floor Access with Difficulty
//Floor Failure System
//Floor Progression System
//Complete Floor Sequence
//Floor Skip Prevention
//Floor State Persistence
//Floor Reset on Tower Failure
//Milestone Floor Handling
//Floor Difficulty Inheritance
// Verifies:
//-Floor state transitions happen in correct order
//- Unlocking mechanisms work properly
//- Floor completion triggers next floor availability
//-Floor difficulty increases appropriately
//-Floor state persists correctly
public partial class FloorSystemTests : BaseTestSuite
{



	private TowerManager _towerManager;
	
	private List<string> _testResults;
	private Random _random;
	private string _currentTowerId;

	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;


	public FloorSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_random = new Random();

		// Get the first available tower with generated floors
		var tower = _towerManager.GetLockedTowers().FirstOrDefault();
		if (tower != null)
		{
			_currentTowerId = tower.Id;
		}
		else
		{
			LogError("Setup", "No available tower found for testing");
		}

	}

	#region Tests
	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Floor))
		{
			TestFloorUnlockSystem();
			TestFloorDifficultyScaling();
			TestFloorAccessWithDifficulty();
			TestFloorFailureSystem();
			TestFloorProgressionSystem();
			TestCompleteFloorSequence();
			TestFloorSkipAttempt();
			TestFloorStatePersistence();
			TestFloorStatePersistence();
			TestFloorResetOnTowerFailure();
			TestMilestoneFloorHandling();
			TestFloorDifficultyInheritance();
			OutputTestResults();

		}
	}
	

	private void TestFloorUnlockSystem()
	{
		string testName = "Floor Unlock System";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			_playerManager.SetCurrentCharacter(characterId);

			var tower = _towerManager.GetAllTowers().FirstOrDefault();

			if (tower.CurrentState == TowerState.Locked)
			{
				_towerManager.UnlockTower(tower.Id);
			}
			ResetAllFloors(tower);

			// First floor should be available
			var firstFloor = tower.Floors.First();
			if (firstFloor.CurrentState != FloorState.Available)
			{
				testPassed = false;
				LogError(testName, "First floor not available at start");
			}
			else
			{
				DebugLogger.Log("First floor correctly available at start", DebugLogger.LogCategory.Floor);

			}

			// Other floors should be locked (expected state)
			var lockedFloors = tower.Floors.Skip(1).Where(f => f.CurrentState != FloorState.Locked).ToList();
			if (lockedFloors.Any())
			{
				testPassed = false;
				LogError(testName, $"Found {lockedFloors.Count} floors incorrectly unlocked at start");
			}
			else
			{
				DebugLogger.Log("All subsequent floors correctly locked at start", DebugLogger.LogCategory.Floor);

			}

			// Complete first floor
			bool completed = _towerManager.CompleteFloor(tower.Id, firstFloor.Id);
			if (!completed)
			{
				testPassed = false;
				LogError(testName, "Failed to complete first floor");
			}
			else
			{
				DebugLogger.Log("Successfully completed first floor", DebugLogger.LogCategory.Floor);

			}

			// Second floor should now be available
			var secondFloor = tower.Floors[1];
			if (secondFloor.CurrentState != FloorState.Available)
			{
				testPassed = false;
				LogError(testName, "Second floor not unlocked after completing first floor");
			}
			else
			{
				DebugLogger.Log("Second floor correctly unlocked after first floor completion", DebugLogger.LogCategory.Floor);

			}

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorProgressionSystem()
	{
		string testName = "Floor Progression System";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			ResetAllFloors(tower);

			for (int i = 0; i < 5; i++)
			{
				var currentFloor = tower.Floors[i];
				DebugLogger.Log($"Testing progression for floor {i + 1}", DebugLogger.LogCategory.Floor);


				if (currentFloor.CurrentState != FloorState.Available)
				{
					testPassed = false;
					LogError(testName, $"Floor {i + 1} not available when expected to be");
					break;
				}

				bool entered = _towerManager.TryEnterFloor(tower.Id, currentFloor.Id, _playerManager.GetCurrentCharacter());
				if (!entered)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {i + 1} with sufficient power");
					break;
				}
				else
				{
					DebugLogger.Log($"Successfully entered floor {i + 1}", DebugLogger.LogCategory.Floor);

				}

				bool completed = _towerManager.CompleteFloor(tower.Id, currentFloor.Id);
				if (!completed)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {i + 1}");
					break;
				}
				else
				{
					DebugLogger.Log($"Successfully completed floor {i + 1}", DebugLogger.LogCategory.Floor);

				}

				// Verify next floor availability (except for last floor)
				if (i < 4)
				{
					var nextFloor = tower.Floors[i + 1];
					if (nextFloor.CurrentState != FloorState.Available)
					{
						testPassed = false;
						LogError(testName, $"Floor {i + 2} not available after completing floor {i + 1}");
						break;
					}
					else
					{
						DebugLogger.Log($"Floor {i + 2} correctly available after completing floor {i + 1}", DebugLogger.LogCategory.Floor);
					}
				}
			}

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorFailureSystem()
	{
		string testName = "Floor Failure System";
		bool testPassed = true;

		try
		{
			// Create a weak character
			var characterId = CreateTestCharacter(1); // Low level character
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			// Reset floor states
			ResetAllFloors(tower);

			// Try to enter a higher difficulty floor (should fail)
			var hardFloor = tower.Floors[5]; // 6th floor
			bool entered = _towerManager.TryEnterFloor(tower.Id, hardFloor.Id, _playerManager.GetCurrentCharacter());

			// This should fail - it's what we want!
			if (entered)
			{
				testPassed = false;
				LogError(testName, "Character should not be able to enter floor beyond their capability");
			}
			else
			{
				DebugLogger.Log($"Successfully prevented weak character from entering floor 6 (Expected behavior)", DebugLogger.LogCategory.Floor);
			}

			// Test explicit floor failure
			var firstFloor = tower.Floors[0];
			bool failedFloor = _towerManager.FailFloor(tower.Id, firstFloor.Id);
			if (!failedFloor)
			{
				testPassed = false;
				LogError(testName, "Failed to mark floor as failed");
			}

			if (firstFloor.CurrentState != FloorState.Failed)
			{
				testPassed = false;
				LogError(testName, "Floor state not properly updated after failure");
			}

			// Reset after test
			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorDifficultyScaling()
	{
		string testName = "Floor Difficulty Scaling";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();
			float previousDifficulty = 0;

			// Create a high-level character just for difficulty calculations
			var characterId = CreateTestCharacter(10);
			var character = _playerManager.GetCurrentCharacter();

			// Only test difficulty calculations - no floor entry
			for (int i = 0; i < 10; i++)
			{
				var floor = tower.Floors[i];
				float currentDifficulty = floor.Difficulty.CalculateFinalDifficulty(character.Level, character.Element);

				DebugLogger.Log($"Floor {i + 1} Difficulty: {currentDifficulty}", DebugLogger.LogCategory.Floor);


				if (i > 0)
				{
					// Check difficulty increases
					if (currentDifficulty <= previousDifficulty)
					{
						testPassed = false;
						LogError(testName, $"Difficulty not increasing: Floor {i + 1} ({currentDifficulty}) <= Floor {i} ({previousDifficulty})");
					}

					// Check milestone spike at floor 10
					if (i == 9)
					{
						float expectedSpike = previousDifficulty * 1.5f;
						if (currentDifficulty < expectedSpike)
						{
							testPassed = false;
							LogError(testName, $"Missing difficulty spike at floor 10. Expected >= {expectedSpike}, got {currentDifficulty}");
						}
					}
				}

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


	// Add a new test specifically for floor access
	private void TestFloorAccessWithDifficulty()
	{
		string testName = "Floor Access With Difficulty";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();

			// Reset all floors
			ResetAllFloors(tower);

			var characterId = CreateTestCharacter(10);
			var character = _playerManager.GetCurrentCharacter();

			// Try to progress through floors properly
			for (int i = 0; i < 5; i++) // Test first 5 floors
			{
				var floor = tower.Floors[i];
				_playerManager.SetCharacterLevel(characterId,i);
				// First floor or previous floor must be completed
				if (i > 0)
				{
					bool completed = _towerManager.CompleteFloor(tower.Id, tower.Floors[i - 1].Id);
					if (!completed)
					{
						testPassed = false;
						LogError(testName, $"Failed to complete floor {i}");
						break;
					}
				}

				// Try to enter current floor
				bool entered = _towerManager.TryEnterFloor(tower.Id, floor.Id, character);
				if (!entered)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {i + 1} with sufficient power level");
					break;
				}
			}

			// Reset after test
			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCompleteFloorSequence()
	{
		string testName = "Complete Floor Sequence";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(10);
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			ResetAllFloors(tower);

			int floorsCompleted = 0;
			foreach (var floor in tower.Floors.Take(10))
			{
				DebugLogger.Log($"Attempting floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);


				if (floor.CurrentState != FloorState.Available)
				{
					DebugLogger.Log($"Floor {floor.FloorNumber} not available - skipping (Expected for higher floors)", DebugLogger.LogCategory.Floor);
					continue;
				}

				bool entered = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
				if (!entered)
				{
					testPassed = false;
					LogError(testName, $"Failed to enter floor {floor.FloorNumber} with sufficient power");
					break;
				}
				else
				{
					DebugLogger.Log($"Successfully entered floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);
				}

				bool completed = _towerManager.CompleteFloor(tower.Id, floor.Id);
				if (!completed)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {floor.FloorNumber}");
					break;
				}
				else
				{
					DebugLogger.Log($"Successfully completed floor {floor.FloorNumber}", DebugLogger.LogCategory.Floor);
				}

				floorsCompleted++;
			}

			
			DebugLogger.Log($"Successfully completed {floorsCompleted} floors in sequence", DebugLogger.LogCategory.Floor);

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorSkipAttempt()
	{
		string testName = "Floor Skip Prevention";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(10); // Strong character to avoid power issues
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			// Reset all floors
			ResetAllFloors(tower);

			// Try to enter floor 3 directly
			var thirdFloor = tower.Floors[2];
			bool skippedEntry = _towerManager.TryEnterFloor(tower.Id, thirdFloor.Id, _playerManager.GetCurrentCharacter());

			if (skippedEntry)
			{
				testPassed = false;
				LogError(testName, "Shouldn't be able to enter floor 3 without completing previous floors");
			}
			else
			{
				DebugLogger.Log("Successfully prevented skipping to floor 3 (Expected)", DebugLogger.LogCategory.Floor);
			}

			// Verify proper sequence works
			var firstFloor = tower.Floors[0];
			bool enteredFirst = _towerManager.TryEnterFloor(tower.Id, firstFloor.Id, _playerManager.GetCurrentCharacter());
			if (!enteredFirst)
			{
				testPassed = false;
				LogError(testName, "Failed to enter first floor properly");
			}
			else
			{
				DebugLogger.Log("Successfully entered first floor in proper sequence", DebugLogger.LogCategory.Floor);
			}

			// Reset after test
			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorStatePersistence()
	{
		string testName = "Floor State Persistence";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			ResetAllFloors(tower);

			// Complete first floor
			var firstFloor = tower.Floors[0];
			bool entered = _towerManager.TryEnterFloor(tower.Id, firstFloor.Id, _playerManager.GetCurrentCharacter());
			bool completed = _towerManager.CompleteFloor(tower.Id, firstFloor.Id);

			if (!completed)
			{
				testPassed = false;
				LogError(testName, "Failed to complete first floor");
				return;
			}

			// Exit tower
			_towerManager.ExitTower(tower.Id);

			// Verify floor states persisted
			if (firstFloor.CurrentState != FloorState.Completed)
			{
				testPassed = false;
				LogError(testName, "First floor state not persisted after tower exit");
			}
			else
			{
				DebugLogger.Log("Floor state correctly persisted after tower exit", DebugLogger.LogCategory.Floor);

			}

			var secondFloor = tower.Floors[1];
			if (secondFloor.CurrentState != FloorState.Available)
			{
				testPassed = false;
				LogError(testName, "Second floor state not persisted after tower exit");
			}
			else
			{
				DebugLogger.Log("Next floor state correctly persisted after tower exit", DebugLogger.LogCategory.Floor);
			}

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorResetOnTowerFailure()
	{
		string testName = "Floor Reset On Tower Failure";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(characterId);
			var tower = _towerManager.GetAvailableTowers().First();

			ResetAllFloors(tower);

			// Complete first few floors
			for (int i = 0; i < 3; i++)
			{
				var floor = tower.Floors[i];
				bool entered = _towerManager.TryEnterFloor(tower.Id, floor.Id, _playerManager.GetCurrentCharacter());
				bool completed = _towerManager.CompleteFloor(tower.Id, floor.Id);

				if (!completed)
				{
					testPassed = false;
					LogError(testName, $"Failed to complete floor {i + 1} during setup");
					return;
				}
				DebugLogger.Log($"Completed floor {i + 1} for test setup", DebugLogger.LogCategory.Floor);
			}

			// Fail the tower
			_towerManager.FailTower(tower.Id);
			DebugLogger.Log("Tower failed for testing floor reset", DebugLogger.LogCategory.Floor );

			// Verify all floors reset properly
			foreach (var floor in tower.Floors)
			{
				var expectedState = floor.FloorNumber == 1 ? FloorState.Available : FloorState.Locked;
				if (floor.CurrentState != expectedState)
				{
					testPassed = false;
					LogError(testName, $"Floor {floor.FloorNumber} not properly reset. Expected {expectedState}, got {floor.CurrentState}");
				}
				else
				{
					DebugLogger.Log($"Floor {floor.FloorNumber} correctly reset to {expectedState}", DebugLogger.LogCategory.Floor);
				}
			}

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMilestoneFloorHandling()
	{
		string testName = "Milestone Floor Handling";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();
			var character = CreateTestCharacter(10); // High level to ensure we can reach milestone floors
			_playerManager.SetCurrentCharacter(character);

			ResetAllFloors(tower);

			// Test regular floor difficulty
			var regularFloor = tower.Floors[7]; // Floor 8
			float regularDifficulty = regularFloor.Difficulty.CalculateFinalDifficulty(
				_playerManager.GetCurrentCharacter().Level,
				_playerManager.GetCurrentCharacter().Element
			);

			// Test milestone floor difficulty
			var milestoneFloor = tower.Floors[9]; // Floor 10
			float milestoneDifficulty = milestoneFloor.Difficulty.CalculateFinalDifficulty(
				_playerManager.GetCurrentCharacter().Level,
				_playerManager.GetCurrentCharacter().Element
			);

			// Milestone floor should have significantly higher difficulty
			float expectedIncrease = 1.5f; // Based on MilestoneDifficultyMultiplier
			float actualIncrease = milestoneDifficulty / regularDifficulty;

			DebugLogger.Log($"Regular Floor (8) Difficulty: {regularDifficulty}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Milestone Floor (10) Difficulty: {milestoneDifficulty}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Difficulty Increase Factor: {actualIncrease}", DebugLogger.LogCategory.Floor);

			if (actualIncrease < expectedIncrease)
			{
				testPassed = false;
				LogError(testName, $"Milestone floor difficulty increase insufficient. Expected >{expectedIncrease}x, got {actualIncrease}x");
			}
			else
			{
				DebugLogger.Log($"Milestone floor difficulty correctly increased by {actualIncrease}x", DebugLogger.LogCategory.Floor);
			}

			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestFloorDifficultyInheritance()
	{
		string testName = "Floor Difficulty Inheritance";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();
			var character = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(character);

			ResetAllFloors(tower);

			// Get tower base difficulty settings
			float towerBaseValue = tower.Difficulty.BaseValue;
			float towerScalingFactor = tower.Difficulty.LevelScalingFactor;

			// Check first floor
			var firstFloor = tower.Floors[0];
			float firstFloorDifficulty = firstFloor.Difficulty.CalculateFinalDifficulty(
				_playerManager.GetCurrentCharacter().Level,
				_playerManager.GetCurrentCharacter().Element
			);

			// Verify base difficulty inheritance
			if (Math.Abs(firstFloor.Difficulty.BaseValue - towerBaseValue) > 0.001f)
			{
				testPassed = false;
				LogError(testName, $"First floor base difficulty ({firstFloor.Difficulty.BaseValue}) doesn't match tower base difficulty ({towerBaseValue})");
			}
			else
			{
				DebugLogger.Log("Floor correctly inherited tower base difficulty", DebugLogger.LogCategory.Floor);
			}

			// Verify scaling factor inheritance
			if (Math.Abs(firstFloor.Difficulty.LevelScalingFactor - towerScalingFactor) > 0.001f)
			{
				testPassed = false;
				LogError(testName, $"Floor scaling factor ({firstFloor.Difficulty.LevelScalingFactor}) doesn't match tower scaling factor ({towerScalingFactor})");
			}
			else
			{
				DebugLogger.Log("Floor correctly inherited tower scaling factor", DebugLogger.LogCategory.Floor);
			}

			// Test difficulty modifiers inheritance
			bool modifiersMatch = true;
			string modifierMismatch = "";

			foreach (var towerModifier in tower.Difficulty.Modifiers)
			{
				var matchingFloorModifier = firstFloor.Difficulty.Modifiers
					.FirstOrDefault(m => m.Type == towerModifier.Type && Math.Abs(m.Value - towerModifier.Value) < 0.001f);

				if (matchingFloorModifier == null)
				{
					modifiersMatch = false;
					modifierMismatch = $"Missing or mismatched modifier: Type={towerModifier.Type}, Value={towerModifier.Value}";
					break;
				}
			}

			if (!modifiersMatch)
			{
				testPassed = false;
				LogError(testName, $"Floor modifiers don't match tower modifiers: {modifierMismatch}");
			}
			else
			{
				DebugLogger.Log("Floor correctly inherited tower difficulty modifiers", DebugLogger.LogCategory.Floor);

			}
			DebugLogger.Log($"Tower Base Difficulty: {towerBaseValue}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Floor Base Difficulty: {firstFloor.Difficulty.BaseValue}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Tower Scaling Factor: {towerScalingFactor}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Floor Scaling Factor: {firstFloor.Difficulty.LevelScalingFactor}", DebugLogger.LogCategory.Floor);
			DebugLogger.Log($"Calculated First Floor Difficulty: {firstFloorDifficulty}", DebugLogger.LogCategory.Floor);


			ResetAllFloors(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}
	#endregion 
	#region Support Methods
	private void ResetAllFloors(TowerData tower)
	{
		foreach (var floor in tower.Floors)
		{
			ResetFloorState(tower, floor);
		}
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

	private void ResetFloorState(TowerData tower, FloorData floor)
	{
		if (floor != null)
		{
			_towerManager.ResetFloorState(tower.Id, floor.Id);
		}
	}
	private void OutputTestResults()
	{
		GD.Print("\n=== Floor System Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}
	#endregion 
}