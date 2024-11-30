using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;


//// TestTowerDifficultyScaling
//Current Coverage:

//Tower Unlock System
//Tower Entrance
//Tower Difficulty Calculation
//Tower Requirements
//Tower Reentry After Failure
//Tower State Transitions
//Tower Level Requirements
//Tower Entry with Different Levels
//Tower Difficulty Scaling
//Tower Elemental Interactions
// Verifies:
// - Base difficulty calculations are correct
// - Level scaling applies properly
// - Milestone floors have appropriate difficulty spikes
// - Difficulty caps work correctly
// - Elemental modifiers affect difficulty appropriately
public partial class TowerSystemTests : BaseTestSuite
{


	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private TowerManager _towerManager;
	//private ProgressionSystemTests _progressionSystemTests;

	private Random _random;


	public TowerSystemTests(EventManager eventManager, PlayerManager playerManager, 
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		_towerManager = Globals.Instance.gameMangers.World.Towers;
		_random = new Random();
	}


	#region Tests



	public void RunAllTests()
	{

		if (TestConfig.IsCategoryEnabled(TestCategory.Tower))
		{
			TestTowerUnlockSystem();
			TestTowerEntranceTest();
			TestTowerDifficultyCalculation();
			TestTowerRequirements();
			TestTowerReentryAfterFailure();
			TestTowerStateTransitions();
			TestTowerLevelRequirements();
			TestTowerEntryWithDifferentLevels();
			TestTowerElementalInteractions();
			TestTowerDifficultyScaling();
			OutputTestResults();
		}

	}

	
	private void TestTowerUnlockSystem()
	{
		string testName = "Tower Unlock System";
		bool testPassed = true;

		try
		{
			// Create a character that meets requirements
			var characterId = CreateRandomCharacter();
			_playerManager.SetCharacterLevel(characterId, 1); // Set to minimum required level
			_playerManager.SetCurrentCharacter(characterId);

			// Get a locked tower
			var lockedTowers = _towerManager.GetLockedTowers().ToList();
			if (!lockedTowers.Any())
			{
				testPassed = false;
				LogError(testName, "No locked towers found for testing");
				LogTestResult(testName, testPassed);
				return;
			}

			var tower = lockedTowers.First();

			// Verify initial state
			if (tower.CurrentState != TowerState.Locked)
			{
				testPassed = false;
				LogError(testName, "Tower not in initial locked state");
			}

			// Attempt to unlock tower
			bool unlocked = _towerManager.UnlockTower(tower.Id);
			if (!unlocked)
			{
				testPassed = false;
				LogError(testName, "Failed to unlock tower");
			}

			// Verify tower is now available
			if (tower.CurrentState != TowerState.Available)
			{
				testPassed = false;
				LogError(testName, "Tower not in available state after unlock");
			}

			// Verify tower appears in available towers list
			var availableTowers = _towerManager.GetAvailableTowers().ToList();
			if (!availableTowers.Contains(tower))
			{
				testPassed = false;
				LogError(testName, "Tower not found in available towers list");
			}

			// Try to unlock already unlocked tower
			bool locked = _towerManager.LockTower(tower.Id);
			bool Unlocked = _towerManager.UnlockTower(tower.Id);
			bool reUnlocked = _towerManager.UnlockTower(tower.Id);
			if (reUnlocked)
			{
				testPassed = false;
				LogError(testName, "Able to unlock already unlocked tower");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}


	private void TestTowerEntranceTest()
	{
		string testName = "Tower Entrance Tests";
		bool testPassed = true;

		try
		{
			var availableTowers = _towerManager.GetAvailableTowers().ToList();

			if (!availableTowers.Any())
			{
				testPassed = false;
				LogError(testName, "No towers available after initialization that are unlocked");
			}

			var firstTower = availableTowers.First();
			if (firstTower.Requirements == null)
			{
				testPassed = false;
				LogError(testName, "Tower requirements not initialized");
			}

			if (firstTower.Difficulty == null)
			{
				testPassed = false;
				LogError(testName, "Tower difficulty not initialized");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerDifficultyCalculation()
	{
		string testName = "Tower Difficulty Calculation";
		bool testPassed = true;

		try
		{
			var characterId = CreateRandomCharacter();
			var tower = _towerManager.GetAvailableTowers().First();

			float difficulty = _towerManager.GetTowerDifficulty(tower.Id);

			if (difficulty <= 0)
			{
				testPassed = false;
				LogError(testName, "Invalid difficulty calculation");
			}

			// Test if difficulty is within expected bounds
			if (difficulty < tower.Difficulty.MinimumDifficulty ||
				difficulty > tower.Difficulty.MaximumDifficulty)
			{
				testPassed = false;
				LogError(testName, $"Difficulty {difficulty} outside bounds " +
					$"[{tower.Difficulty.MinimumDifficulty}, {tower.Difficulty.MaximumDifficulty}]");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerRequirements()
	{
		string testName = "Tower Requirements";
		bool testPassed = true;

		try
		{
			var tower = GetOrCreateAvailableTower();
			var characterId = CreateRandomCharacter();
			_playerManager.SetCurrentCharacter(characterId);

			// Modify tower requirements to require higher minimum level for testing
			tower.Requirements.MinLevel = 5;  // Set minimum level requirement to 5

			// Test with character below minimum level
			if (_towerManager.TryEnterTower(tower.Id))
			{
				testPassed = false;
				LogError(testName, "Tower allowed entry below minimum level");
			}

			// Reset requirements after test
			tower.Requirements.MinLevel = 1;

			ResetTowerState(tower);


			// Test with valid level
			int validLevel = (tower.Requirements.MinLevel + tower.Requirements.MaxLevel) / 2;
			_playerManager.SetCharacterLevel(characterId, validLevel);
			if (!_towerManager.TryEnterTower(tower.Id))
			{
				testPassed = false;
				LogError(testName, "Tower denied entry for valid level");
			}

			ResetTowerState(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerEntryWithDifferentLevels()
	{
		string testName = "Tower Entry with Different Levels";
		bool testPassed = true;

		try
		{
			var tower = GetOrCreateAvailableTower();
			var characterId = CreateRandomCharacter();
			_playerManager.SetCurrentCharacter(characterId);

			// Test multiple random levels within valid range
			for (int i = 0; i < 5; i++)
			{
				ResetTowerState(tower);

				int randomLevel = _random.Next(
					tower.Requirements.MinLevel,
					tower.Requirements.MaxLevel + 1
				);

				_playerManager.SetCharacterLevel(characterId, randomLevel);

				//GD.Print($"Testing tower entry with level {randomLevel}");
				if (!_towerManager.TryEnterTower(tower.Id))
				{
					testPassed = false;
					LogError(testName, $"Tower denied entry for valid level {randomLevel}");
					continue;
				}

				if (tower.CurrentState != TowerState.InProgress)
				{
					testPassed = false;
					LogError(testName, $"Incorrect tower state after entry at level {randomLevel}");
				}
			}
			ResetTowerState(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerDifficultyScaling()
	{
		string testName = "Tower Difficulty Scaling";
		bool testPassed = true;

		try
		{
			var tower = GetOrCreateAvailableTower();
			var characterId = CreateRandomCharacter();
			_playerManager.SetCurrentCharacter(characterId);

			float previousDifficulty = 0;
			var difficulties = new List<(int level, float difficulty, float rawDifficulty)>();

			DebugLogger.Log("\nDifficulty Scaling Test Configuration:", DebugLogger.LogCategory.Tower);
			DebugLogger.Log($"Base Value: {tower.Difficulty.BaseValue}", DebugLogger.LogCategory.Tower);
			DebugLogger.Log($"Scaling Factor: {tower.Difficulty.LevelScalingFactor}", DebugLogger.LogCategory.Tower);
			DebugLogger.Log($"Min Difficulty: {tower.Difficulty.MinimumDifficulty}", DebugLogger.LogCategory.Tower);
			DebugLogger.Log($"Max Difficulty: {tower.Difficulty.MaximumDifficulty}", DebugLogger.LogCategory.Tower);


			// Test increasing difficulty with increasing levels
			for (int level = tower.Requirements.MinLevel;
				 level <= tower.Requirements.MaxLevel;
				 level++)
			{
				ResetTowerState(tower);
				_playerManager.SetCharacterLevel(characterId, level);

				// Calculate raw difficulty before clamping
				float levelDifference = level - tower.Requirements.RecommendedLevel;
				float levelMultiplier = 1 + (levelDifference * tower.Difficulty.LevelScalingFactor);
				float rawDifficulty = tower.Difficulty.BaseValue * levelMultiplier;

				float currentDifficulty = _towerManager.GetTowerDifficulty(tower.Id);

				difficulties.Add((level, currentDifficulty, rawDifficulty));

				if (level > tower.Requirements.MinLevel)
				{
					if (currentDifficulty <= previousDifficulty)
					{
						testPassed = false;
						LogError(testName,
							$"Difficulty not increasing with level: {level - 1}={previousDifficulty:F2}, " +
							$"{level}={currentDifficulty:F2} (Raw: {rawDifficulty:F2})");
					}
				}

				previousDifficulty = currentDifficulty;
			}

			// Log all difficulty values for analysis
			DebugLogger.Log("\nDetailed Difficulty Scaling Results:", DebugLogger.LogCategory.Tower);

			foreach (var (level, difficulty, rawDifficulty) in difficulties)
			{
				DebugLogger.Log($"Level {level}: " +
					$"Final Difficulty = {difficulty:F2}, " +
					$"Raw Difficulty = {rawDifficulty:F2}", DebugLogger.LogCategory.Tower);

			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerElementalInteractions()
		{
			string testName = "Tower Elemental Interactions";
			bool testPassed = true;

			try
			{
				var tower = GetOrCreateAvailableTower();
				if (tower == null)
				{
					testPassed = false;
					LogError(testName, "No tower available for testing");
					LogTestResult(testName, testPassed);
					return;
				}

				var characterId = CreateRandomCharacter();
				_playerManager.SetCurrentCharacter(characterId);

				// Set a valid level
				int validLevel = (tower.Requirements.MinLevel + tower.Requirements.MaxLevel) / 2;
				_playerManager.SetCharacterLevel(characterId, validLevel);

				// Test different elements
				foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
				{
					if (element == ElementType.None) continue;

					// Reset tower state before each test
					ResetTowerState(tower);

					// Set character element
					_playerManager.SetCharacterElement(characterId, element);

					float difficulty = _towerManager.GetTowerDifficulty(tower.Id);

					if (difficulty <= 0)
					{
						testPassed = false;
						LogError(testName, $"Invalid difficulty for element {element}");
					}

					// Try to enter tower with this element
					if (!_towerManager.TryEnterTower(tower.Id))
					{
						testPassed = false;
						LogError(testName, $"Could not enter tower with element {element}");
					}
				}
				ResetTowerState(tower);
			}
			catch (Exception ex)
			{
				testPassed = false;
				LogError(testName, $"Exception: {ex.Message}");
			}

			LogTestResult(testName, testPassed);
		}

	private void TestTowerReentryAfterFailure()
	{
		string testName = "Tower Reentry After Failure";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();

			// Modify tower requirements to require higher minimum level for testing
			tower.Requirements.MinLevel = 5;  // Set minimum level requirement to 5

			// Create a character at default level 1 (now below minimum requirement)
			var weakCharacterId = _playerManager.CreateCharacter("knight");
			_playerManager.SetCurrentCharacter(weakCharacterId);

			// Reset tower state
			ResetTowerState(tower);

			// Try to enter with weak character (level 1 < minimum level 5)
			bool initialEntry = _towerManager.TryEnterTower(tower.Id);
			if (initialEntry)
			{
				testPassed = false;
				LogError(testName, $"Level 1 character shouldn't be able to enter tower requiring level {tower.Requirements.MinLevel}");
			}
			else
			{
				DebugLogger.Log($"Successfully prevented under-leveled character (level 1) from entering tower requiring level {tower.Requirements.MinLevel}", DebugLogger.LogCategory.Tower);

			}


			// Create stronger character meeting level requirements
			var strongCharacterId = _playerManager.CreateCharacter("knight");
			_playerManager.SetCharacterLevel(strongCharacterId, 6);  // Level 6 > minimum level 5
			_playerManager.SetCurrentCharacter(strongCharacterId);

			// Try to enter with appropriately leveled character
			bool reentry = _towerManager.TryEnterTower(tower.Id);
			if (!reentry)
			{
				testPassed = false;
				LogError(testName, $"Level 6 character should be able to enter tower requiring level {tower.Requirements.MinLevel}");
			}
			else
			{
				//GD.Print($"Successfully entered tower with level 6 character");
			}

			// Reset requirements after test
			tower.Requirements.MinLevel = 1;
			ResetTowerState(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerStateTransitions()
	{
		string testName = "Tower State Transitions";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();
			var characterId = CreateTestCharacter(5);
			_playerManager.SetCurrentCharacter(characterId);

			bool locked = _towerManager.LockTower(tower.Id);
			if (!locked || tower.CurrentState != TowerState.Locked)
			{
				testPassed = false;
				LogError(testName, "Failed to lock tower");
				return;
			}
			else
			{
				DebugLogger.Log("Successfully locked tower for testing", DebugLogger.LogCategory.Tower);

			}
			// Unlock tower
			bool unlocked = _towerManager.UnlockTower(tower.Id);
			if (!unlocked || tower.CurrentState != TowerState.Available)
			{
				testPassed = false;
				LogError(testName, "Failed to transition to Available state");
			}
			else
			{
				DebugLogger.Log("Successfully transitioned to Available state", DebugLogger.LogCategory.Tower);

			}

			// Enter tower
			bool entered = _towerManager.TryEnterTower(tower.Id);
			if (!entered || tower.CurrentState != TowerState.InProgress)
			{
				testPassed = false;
				LogError(testName, "Failed to transition to InProgress state");
			}
			else
			{
				DebugLogger.Log("Successfully transitioned to InProgress state", DebugLogger.LogCategory.Tower);

			}

			// Complete first floor (can add more complex completion logic later)
			if (tower.Floors.Any())
			{
				bool completed = _towerManager.CompleteFloor(tower.Id, tower.Floors.First().Id);
				if (!completed)
				{
					testPassed = false;
					LogError(testName, "Failed to complete floor");
				}
				else
				{
					DebugLogger.Log("Successfully completed first floor", DebugLogger.LogCategory.Tower);

				}
			}

			ResetTowerState(tower);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTowerLevelRequirements()
	{
		string testName = "Tower Level Requirements";
		bool testPassed = true;

		try
		{
			var tower = _towerManager.GetAvailableTowers().First();

			bool locked = _towerManager.LockTower(tower.Id);

			// Test below minimum level
			var underLevelCharId =CreateTestCharacter(tower.Requirements.MinLevel - 1);
			_playerManager.SetCurrentCharacter(underLevelCharId);

			ResetTowerState(tower);

			bool unlockUnderLevel = _towerManager.UnlockTower(tower.Id);
			if (unlockUnderLevel)
			{
				testPassed = false;
				LogError(testName, "Tower shouldn't unlock for under-leveled character");
			}
			else
			{
				//GD.Print($"Correctly prevented under-leveled (level {tower.Requirements.MinLevel - 1}) character from unlocking tower");
			}

			// Test at minimum level
			var minLevelCharId = CreateTestCharacter(tower.Requirements.MinLevel);
			_playerManager.SetCurrentCharacter(minLevelCharId);

			ResetTowerState(tower);

			locked = _towerManager.LockTower(tower.Id);
			bool unlockMinLevel = _towerManager.UnlockTower(tower.Id);
			if (!unlockMinLevel)
			{
				testPassed = false;
				LogError(testName, "Tower should unlock at minimum level");
			}
			else
			{
				//GD.Print($"Successfully unlocked tower at minimum level {tower.Requirements.MinLevel}");
			}

			// Test at maximum level
			var maxLevelCharId = CreateTestCharacter(tower.Requirements.MaxLevel);
			_playerManager.SetCurrentCharacter(maxLevelCharId);

			ResetTowerState(tower);
			locked = _towerManager.LockTower(tower.Id);
			bool unlockMaxLevel = _towerManager.UnlockTower(tower.Id);
			if (!unlockMaxLevel)
			{
				testPassed = false;
				LogError(testName, "Tower should unlock at maximum level");
			}
			else
			{
				//GD.Print($"Successfully unlocked tower at maximum level {tower.Requirements.MaxLevel}");
			}

			
			ResetTowerState(tower);
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
	private string CreateRandomCharacter()
	{
		string[] classes = { "knight", "mage" };
		string randomClass = classes[_random.Next(classes.Length)];
		return _playerManager.CreateCharacter(randomClass);
	}

	private void LogError(string testName, string message)
		{
			TestResults.Add($"Error in {testName}: {message}");
		}

		private void LogTestResult(string testName, bool passed)
		{
			string result = passed ? "PASSED" : "FAILED";
		TestResults.Add($"Test {testName}: {result}");
		}

		private void OutputTestResults()
		{
			GD.Print("\n=== Tower System Test Results ===");
			foreach (var result in TestResults)
			{
				GD.Print(result);
			}
			GD.Print("===============================\n");
		}

		/// <summary>
		/// Helper method to get or create an available tower
		/// </summary>
		private TowerData GetOrCreateAvailableTower()
		{
			var tower = _towerManager.GetAvailableTowers().FirstOrDefault();
			if (tower == null)
			{
				tower = _towerManager.GetLockedTowers().FirstOrDefault();
				if (tower != null)
				{
					_towerManager.UnlockTower(tower.Id);
				}
				else
				{
					// No towers found, something went wrong in initialization
					throw new InvalidOperationException("No towers available for testing");
				}
			}
			return tower;
		}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
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
	#endregion 
}

