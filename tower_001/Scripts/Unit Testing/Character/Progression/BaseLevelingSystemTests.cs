using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Tower_001.Scripts.Events;
using static GlobalEnums;

public class BaseLevelingSystemTests : BaseTestSuite
{



	private readonly ProgressionManager _progressionManager;
	private readonly PlayerManager _playerManager;

	private bool _experienceEventReceived;
	private bool _levelUpEventReceived;
	private float _lastExperienceGained;
	private int _lastLevelGained;

	public BaseLevelingSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_playerManager = playerManager;
		_progressionManager = playerManager.progressionManager;
		RegisterEventHandlers();
	}


	private void RegisterEventHandlers()
	{
		Globals.Instance.gameMangers.Events.AddHandler<ExperienceGainEventArgs>(
			EventType.ExperienceGained,
			OnExperienceGained
		);

		Globals.Instance.gameMangers.Events.AddHandler<CharacterLevelEventArgs>(
			EventType.CharacterLevelUp,
			OnLevelUp
		);
	}

	public void RunAllTests()
	{

		if (TestConfig.IsCategoryEnabled(TestCategory.BasicLeveling))
		{
			TestBasicExperienceGain();
			TestExperienceScaling();
			TestExperienceMultipliers();
			TestMultiLevelGain();
			TestExperienceSourceTracking();
			TestExperienceEvents();
			TestNegativeExperience();
			TestExperienceOverflow();
			TestConcurrentExperienceGains();
			TestLevelCapBehavior();

			OutputTestResults();
		}

	}

	private void TestBasicExperienceGain()
	{
		string testName = "Basic Experience Gain";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			ResetTestFlags();

			// Test basic experience gain
			float expAmount = 100f;
			_progressionManager.AddExperience(characterId, expAmount, ExperienceSource.Combat);

			// Verify experience was added
			if (!_experienceEventReceived)
			{
				testPassed = false;
				LogError(testName, "Experience gain event not received");
			}

			if (Math.Abs(_lastExperienceGained - expAmount) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Expected {expAmount} experience, got {_lastExperienceGained}");
			}

			DebugLogger.Log($"Basic Experience Test - Gained: {_lastExperienceGained}", DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestExperienceScaling()
	{
		string testName = "Experience Scaling";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var baseExpNeeded = _progressionManager.GetExpForNextLevel(characterId);
			var previousExpNeeded = baseExpNeeded;

			DebugLogger.Log("\nExperience Scaling Analysis:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Base Experience Required: {baseExpNeeded}", DebugLogger.LogCategory.Progress);

			// Test scaling over 5 levels
			for (int i = 2; i <= 5; i++)
			{
				// Level up character
				while (_playerManager.GetCharacter(characterId).Level < i)
				{
					_progressionManager.AddExperience(characterId, previousExpNeeded, ExperienceSource.Combat);
				}

				var currentExpNeeded = _progressionManager.GetExpForNextLevel(characterId);
				var scalingFactor = currentExpNeeded / previousExpNeeded;

				DebugLogger.Log($"Level {i} - Required Exp: {currentExpNeeded}, Scaling Factor: {scalingFactor:F2}x",
					DebugLogger.LogCategory.Progress);

				if (currentExpNeeded <= previousExpNeeded)
				{
					testPassed = false;
					LogError(testName, $"Experience requirement not increasing at level {i}");
				}

				previousExpNeeded = currentExpNeeded;
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestExperienceMultipliers()
	{
		string testName = "Experience Multipliers";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			float baseAmount = 100f;

			// Test different source multipliers
			Dictionary<ExperienceSource, float> expectedMultipliers = new()
			{
				{ ExperienceSource.Combat, 1.0f },
				{ ExperienceSource.RoomCompletion, 1.2f },
				{ ExperienceSource.FloorCompletion, 1.5f },
				{ ExperienceSource.TowerCompletion, 2.0f }
			};

			foreach (var kvp in expectedMultipliers)
			{
				ResetTestFlags();
				_progressionManager.AddExperience(characterId, baseAmount, kvp.Key);

				float expectedExp = baseAmount * kvp.Value;
				if (Math.Abs(_lastExperienceGained - expectedExp) > 0.01f)
				{
					testPassed = false;
					LogError(testName, $"Wrong multiplier for {kvp.Key}. Expected {expectedExp}, got {_lastExperienceGained}");
				}

				DebugLogger.Log($"{kvp.Key} multiplier test - Expected: {expectedExp}, Actual: {_lastExperienceGained}",
					DebugLogger.LogCategory.Progress);
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultiLevelGain()
	{
		string testName = "Multi-Level Gain";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			int startLevel = _playerManager.GetCharacter(characterId).Level;
			float expForMultipleLevels = _progressionManager.GetExpForNextLevel(characterId) * 3;

			ResetTestFlags();
			_progressionManager.AddExperience(characterId, expForMultipleLevels, ExperienceSource.Combat);

			int endLevel = _playerManager.GetCharacter(characterId).Level;
			int levelsGained = endLevel - startLevel;

			if (levelsGained < 2)
			{
				testPassed = false;
				LogError(testName, $"Expected multiple levels, only gained {levelsGained}");
			}

			DebugLogger.Log($"Multi-level gain test - Levels gained: {levelsGained}", DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestExperienceSourceTracking()
	{
		string testName = "Experience Source Tracking";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var experienceBySource = new Dictionary<ExperienceSource, float>();
			var sourceMultipliers = new Dictionary<ExperienceSource, float>
			{
				{ ExperienceSource.Combat, 1.0f },
				{ ExperienceSource.RoomCompletion, 1.2f },
				{ ExperienceSource.FloorCompletion, 1.5f },
				{ ExperienceSource.TowerCompletion, 2.0f },
				{ ExperienceSource.Achievement, 1.3f },
				{ ExperienceSource.Quest, 1.4f },
				{ ExperienceSource.Bonus, 1.1f },
				{ ExperienceSource.Event, 1.25f }
			};

			// Add experience from different sources, considering multipliers
			foreach (ExperienceSource source in Enum.GetValues(typeof(ExperienceSource)))
			{
				float baseAmount = 100f * ((int)source + 1);
				float multiplier = sourceMultipliers.GetValueOrDefault(source, 1.0f);
				float expectedAmount = baseAmount * multiplier;

				_progressionManager.AddExperience(characterId, baseAmount, source);
				experienceBySource[source] = expectedAmount;

				DebugLogger.Log($"Adding experience - Source: {source}, Base: {baseAmount}, " +
					$"Multiplier: {multiplier}, Expected: {expectedAmount}",
					DebugLogger.LogCategory.Progress);
			}

			// Verify tracking for each source
			foreach (var kvp in experienceBySource)
			{
				var trackedExp = _progressionManager.GetLifetimeExperience(characterId, kvp.Key);

				if (Math.Abs(trackedExp - kvp.Value) > 0.01f)
				{
					testPassed = false;
					LogError(testName,
						$"Source {kvp.Key} tracking mismatch. Expected {kvp.Value}, got {trackedExp}");
				}

				DebugLogger.Log(
					$"Source {kvp.Key} - Base Expected: {kvp.Value}, Tracked: {trackedExp}",
					DebugLogger.LogCategory.Progress
				);
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestExperienceEvents()
	{
		string testName = "Experience Events";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			ResetTestFlags();

			// Test experience gain event
			_progressionManager.AddExperience(characterId, 100f, ExperienceSource.Combat);
			if (!_experienceEventReceived)
			{
				testPassed = false;
				LogError(testName, "Experience gain event not received");
			}

			// Test level up event
			float expForLevelUp = _progressionManager.GetExpForNextLevel(characterId);
			ResetTestFlags();
			_progressionManager.AddExperience(characterId, expForLevelUp, ExperienceSource.Combat);

			if (!_levelUpEventReceived)
			{
				testPassed = false;
				LogError(testName, "Level up event not received");
			}

			DebugLogger.Log($"Experience events test - Exp event received: {_experienceEventReceived}, Level event received: {_levelUpEventReceived}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestNegativeExperience()
	{
		string testName = "Negative Experience";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			float initialExp = _progressionManager.GetCurrentExperience(characterId);

			// Attempt to add negative experience
			_progressionManager.AddExperience(characterId, -100f, ExperienceSource.Combat);

			float finalExp = _progressionManager.GetCurrentExperience(characterId);
			if (finalExp < initialExp)
			{
				testPassed = false;
				LogError(testName, "Negative experience was incorrectly applied");
			}

			DebugLogger.Log($"Negative experience test - Initial: {initialExp}, Final: {finalExp}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestExperienceOverflow()
	{
		string testName = "Experience Overflow";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			float maxPossibleExp = float.MaxValue;

			// Attempt to add massive experience amount
			_progressionManager.AddExperience(characterId, maxPossibleExp, ExperienceSource.Combat);

			float currentExp = _progressionManager.GetCurrentExperience(characterId);
			if (float.IsInfinity(currentExp) || float.IsNaN(currentExp))
			{
				testPassed = false;
				LogError(testName, "Experience overflow not properly handled");
			}

			DebugLogger.Log($"Experience overflow test - Final exp: {currentExp}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestConcurrentExperienceGains()
	{
		string testName = "Concurrent Experience Gains";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			float initialExp = _progressionManager.GetCurrentExperience(characterId);
			float[] expAmounts = { 100f, 200f, 300f };
			float expectedTotal = expAmounts.Sum();

			// Add experience from multiple sources simultaneously
			foreach (var amount in expAmounts)
			{
				_progressionManager.AddExperience(characterId, amount, ExperienceSource.Combat);
			}

			float actualGain = _progressionManager.GetCurrentExperience(characterId) - initialExp;
			if (Math.Abs(actualGain - expectedTotal) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Expected total gain {expectedTotal}, got {actualGain}");
			}

			DebugLogger.Log($"Concurrent gains test - Expected: {expectedTotal}, Actual: {actualGain}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestLevelCapBehavior()
	{
		string testName = "Level Cap Behavior";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			int maxLevel = _progressionManager.GetMaxLevel();

			// Level up to max level
			while (_playerManager.GetCharacter(characterId).Level < maxLevel)
			{
				float expNeeded = _progressionManager.GetExpForNextLevel(characterId);
				_progressionManager.AddExperience(characterId, expNeeded, ExperienceSource.Combat);
			}

			// Try to gain more experience at max level
			float initialExp = _progressionManager.GetCurrentExperience(characterId);
			_progressionManager.AddExperience(characterId, 1000f, ExperienceSource.Combat);
			float finalExp = _progressionManager.GetCurrentExperience(characterId);

			if (_playerManager.GetCharacter(characterId).Level > maxLevel)
			{
				testPassed = false;
				LogError(testName, "Level exceeded maximum cap");
			}

			DebugLogger.Log($"Level cap test - Max level: {maxLevel}, Final level: {_playerManager.GetCharacter(characterId).Level}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}


	#region Support Methods

	private void ResetTestFlags()
	{
		_experienceEventReceived = false;
		_levelUpEventReceived = false;
		_lastExperienceGained = 0f;
		_lastLevelGained = 0;
	}

	private void OnExperienceGained(ExperienceGainEventArgs args)
	{
		_experienceEventReceived = true;
		_lastExperienceGained = args.ExperienceGained;

		DebugLogger.Log(
			$"Experience gained: {args.ExperienceGained} from {args.Source} " +
			$"(Base: {args.BaseExperience}, Multiplier: {args.ExperienceMultiplier}x)",
			DebugLogger.LogCategory.Progress
		);
	}

	private void OnLevelUp(CharacterLevelEventArgs args)
	{
		_levelUpEventReceived = true;
		_lastLevelGained = args.NewLevel;

		DebugLogger.Log(
			$"Level up: {args.OldLevel} -> {args.NewLevel}",
			DebugLogger.LogCategory.Progress
		);
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

	private void ValidateExperienceGain(
		string characterId,
		float expectedExp,
		ExperienceSource source,
		string context = "")
	{
		float initialExp = _progressionManager.GetCurrentExperience(characterId);
		ResetTestFlags();

		_progressionManager.AddExperience(characterId, expectedExp, source);

		float actualGain = _progressionManager.GetCurrentExperience(characterId) - initialExp;

		if (Math.Abs(actualGain - expectedExp) > 0.01f)
		{
			throw new Exception(
				$"{context} Expected experience gain: {expectedExp}, Actual: {actualGain}"
			);
		}
	}

	private void ValidateLevelUp(
		string characterId,
		int expectedFinalLevel,
		string context = "")
	{
		int actualFinalLevel = _playerManager.GetCharacter(characterId).Level;

		if (actualFinalLevel != expectedFinalLevel)
		{
			throw new Exception(
				$"{context} Expected final level: {expectedFinalLevel}, Actual: {actualFinalLevel}"
			);
		}
	}

	private void WaitForLevelUp(string characterId, int targetLevel)
	{
		int maxAttempts = 100; // Prevent infinite loops
		int attempts = 0;

		while (_playerManager.GetCharacter(characterId).Level < targetLevel && attempts < maxAttempts)
		{
			float expNeeded = _progressionManager.GetExpForNextLevel(characterId);
			_progressionManager.AddExperience(characterId, expNeeded, ExperienceSource.Combat);
			attempts++;
		}

		if (attempts >= maxAttempts)
		{
			throw new Exception($"Failed to reach target level {targetLevel} after {maxAttempts} attempts");
		}
	}

	private Dictionary<ExperienceSource, float> GetExperienceSnapshot(string characterId)
	{
		var snapshot = new Dictionary<ExperienceSource, float>();

		foreach (ExperienceSource source in Enum.GetValues(typeof(ExperienceSource)))
		{
			snapshot[source] = _progressionManager.GetLifetimeExperience(characterId, source);
		}

		return snapshot;
	}

	private void ValidateExperienceSnapshot(
		string characterId,
		Dictionary<ExperienceSource, float> beforeSnapshot,
		Dictionary<ExperienceSource, float> expectedChanges)
	{
		var afterSnapshot = GetExperienceSnapshot(characterId);

		foreach (var kvp in expectedChanges)
		{
			float initialValue = beforeSnapshot.GetValueOrDefault(kvp.Key, 0f);
			float expectedFinal = initialValue + kvp.Value;
			float actualFinal = afterSnapshot.GetValueOrDefault(kvp.Key, 0f);

			if (Math.Abs(actualFinal - expectedFinal) > 0.01f)
			{
				throw new Exception(
					$"Experience tracking mismatch for {kvp.Key}. " +
					$"Expected: {expectedFinal}, Actual: {actualFinal}"
				);
			}
		}
	}

	private void EnsureCleanTestState(string characterId)
	{
		ResetTestFlags();
		_playerManager.SetCharacterLevel(characterId, 1);
		// Additional cleanup as needed
	}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
	}


	#endregion
}
