using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

public class AscensionSystemTests : BaseTestSuite
{


	private readonly PlayerManager _playerManager;
	private readonly ProgressionConfig _config;

	// Test state tracking
	private bool _ascensionEventReceived;
	private long _lastAscensionLevel;
	private Dictionary<string, float> _lastUnlockedBonuses;
	private float _lastPowerMultiplier;

	public AscensionSystemTests(EventManager eventManager, PlayerManager playerManager,
						  TestConfiguration testConfig) : base(testConfig)
	{
		_playerManager = playerManager;
		_config = new ProgressionConfig();
		RegisterEventHandlers();
	}

	private void RegisterEventHandlers()
	{
		Globals.Instance.gameMangers.Events.AddHandler<AscensionEventArgs>(
			EventType.AscensionLevelGained,
			OnAscensionGained
		);
	}

	public void RunAllTests()
	{
		// Core Ascension Tests
		if (TestConfig.IsCategoryEnabled(TestCategory.Ascension))
		{
			TestAscensionRequirements();
			TestAscensionBonusCalculation();
			TestAscensionStatModification();
			TestAscensionPersistence();
			TestAscensionReset();
			TestMultipleAscensions();
			TestAscensionEvents();
			TestAscensionFailureCases();

			//// Interaction Tests
			//TestAscensionWithPrestige();
			//TestAscensionPowerScaling();
			//TestAscensionBonusProgression();

			OutputTestResults();

		}
	}

	private void TestAscensionRequirements()
	{
		string testName = "Ascension Requirements";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);

			// Test ascension requirements not met
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension - 1;
			if (_playerManager.progressionManager.TryAscend(characterId))
			{
				testPassed = false;
				LogError(testName, "Ascension succeeded without meeting requirements");
			}

			// Test ascension requirements met
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
			ResetTestFlags();
			if (!_playerManager.progressionManager.TryAscend(characterId))
			{
				testPassed = false;
				LogError(testName, "Ascension failed despite meeting requirements");
			}

			if (!_ascensionEventReceived)
			{
				testPassed = false;
				LogError(testName, "Ascension event not received");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestAscensionBonusCalculation()
	{
		string testName = "Ascension Bonus Calculation";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;

			// Record initial stats
			var character = _playerManager.GetCharacter(characterId);
			float initialAttack = character.GetStatValue(StatType.Strength);
			float initialHealth = character.GetStatValue(StatType.Dexterity);

			// Perform ascension
			ResetTestFlags();
			_playerManager.progressionManager.TryAscend(characterId);

			// Verify first level bonuses (2%)
			float expectedAttackBonus = 0.02f;
			float expectedHealthBonus = 0.02f;

			var newAttack = character.GetStatValue(StatType.Strength);
			var newHealth = character.GetStatValue(StatType.Dexterity);

			float expectedAttackValue = initialAttack * (1 + expectedAttackBonus);
			float expectedHealthValue = initialHealth * (1 + expectedHealthBonus);

			// Verify attack bonus
			if (Math.Abs(newAttack - expectedAttackValue) > 0.001f)
			{
				testPassed = false;
				LogError(testName,
					$"Attack bonus incorrect. Expected {expectedAttackValue:F2} (base {initialAttack:F2} * (1 + {expectedAttackBonus:P})), " +
					$"got {newAttack:F2}");
			}

			// Verify health bonus
			if (Math.Abs(newHealth - expectedHealthValue) > 0.001f)
			{
				testPassed = false;
				LogError(testName,
					$"Health bonus incorrect. Expected {expectedHealthValue:F2} (base {initialHealth:F2} * (1 + {expectedHealthBonus:P})), " +
					$"got {newHealth:F2}");
			}

			// Log final values
			DebugLogger.Log($"\nAscension Bonus Test Results:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Attack: {initialAttack:F2} -> {newAttack:F2} (Expected: {expectedAttackValue:F2})",
						   DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Health: {initialHealth:F2} -> {newHealth:F2} (Expected: {expectedHealthValue:F2})",
						   DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestAscensionStatModification()
	{
		string testName = "Ascension Stat Modification";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Record initial stats
			Dictionary<StatType, float> initialStats = new()
		{
			{ StatType.Strength, character.GetStatValue(StatType.Strength) },
			{ StatType.Dexterity, character.GetStatValue(StatType.Dexterity) }
		};

			// Log initial values
			DebugLogger.Log($"\nInitial Stats:", DebugLogger.LogCategory.Progress);
			foreach (var stat in initialStats)
			{
				DebugLogger.Log($"{stat.Key}: {stat.Value:F2}", DebugLogger.LogCategory.Progress);
			}

			// Perform multiple ascensions and track progression
			int ascensionCount = 3;
			for (int i = 0; i < ascensionCount; i++)
			{
				ResetTestFlags();
				progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
				_playerManager.progressionManager.TryAscend(characterId);

				// Log values after each ascension
				DebugLogger.Log($"\nAfter Ascension {i + 1}:", DebugLogger.LogCategory.Progress);
				foreach (var statType in initialStats.Keys)
				{
					float expectedBonusPercent = 0;
					float expectedValue = 0;
					float currentValue = character.GetStatValue(statType);
					for (int j = 0; j <= i; j++)
					{
						expectedBonusPercent = (GameBalanceConfig.Progression.BaseStatBonusPerLevel * (j + 1));
						expectedValue += initialStats[statType] * expectedBonusPercent;
					}
					//float expectedBonusPercent = (i + 1) * (GameBalanceConfig.Progression.BaseStatBonusPerLevel * (i+1));
					//float expectedValue = initialStats[statType] * (1f + expectedBonusPercent);
					expectedValue += initialStats[statType];
					DebugLogger.Log(
						$"{statType}: {currentValue:F2} (Expected: {expectedValue:F2} with +{expectedBonusPercent:P0})",
						DebugLogger.LogCategory.Progress
					);

					// Verify value at each step
					if (Math.Abs(currentValue - expectedValue) > 1f)
					{
						testPassed = false;
						LogError(testName,
							$"{statType} value incorrect at ascension {i + 1}.\n" +
							$"Expected {expectedValue:F2} (base {initialStats[statType]:F2} * (1 + {expectedBonusPercent:P0})),\n" +
							$"got {currentValue:F2}");
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

	private void TestAscensionPersistence()
	{
		string testName = "Ascension Persistence";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Record initial stats
			Dictionary<StatType, float> initialStats = new()
		{
			{ StatType.Strength, character.GetStatValue(StatType.Strength) },
			{ StatType.Dexterity, character.GetStatValue(StatType.Dexterity) }
		};

			// Log initial values
			DebugLogger.Log($"\nInitial Stats:", DebugLogger.LogCategory.Progress);
			foreach (var stat in initialStats)
			{
				DebugLogger.Log($"{stat.Key}: {stat.Value:F2}", DebugLogger.LogCategory.Progress);
			}

			// Perform multiple ascensions

			for (int i = 0; i < 3; i++)
			{

				progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
				_playerManager.progressionManager.TryAscend(characterId);

			}
			// Record post-ascension stats
			Dictionary<StatType, float> postAscensionStats = new()
		{
			{ StatType.Strength, character.GetStatValue(StatType.Strength)},
			{ StatType.Dexterity, character.GetStatValue(StatType.Dexterity) }
		};

			// Log post-ascension values
			DebugLogger.Log($"\nPost-Ascension Stats:", DebugLogger.LogCategory.Progress);
			foreach (var stat in postAscensionStats)
			{
				DebugLogger.Log($"{stat.Key}: {stat.Value:F2}", DebugLogger.LogCategory.Progress);
			}

			// Simulate game restart by recreating character
			_playerManager.SetCharacterLevel(characterId,1);

			// Verify stats persist
			foreach (var statType in initialStats.Keys)
			{
				float expectedBonusPercent = 0;
				float expectedValue = 0;

				for (int j = 0; j <= 2; j++)
				{
					expectedBonusPercent = (GameBalanceConfig.Progression.BaseStatBonusPerLevel * (j + 1));
					expectedValue += initialStats[statType] * expectedBonusPercent;
				}
				expectedValue += initialStats[statType];
				float persistedValue = character.GetStatValue(statType);

				DebugLogger.Log($"\nVerifying {statType} persistence:", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Initial: {initialStats[statType]:F2}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Expected: {expectedValue:F2}", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Persisted: {persistedValue:F2}", DebugLogger.LogCategory.Progress);

				if (Math.Abs(persistedValue - expectedValue) > 0.05f)
				{
					testPassed = false;
					LogError(testName,
						$"{statType} bonus did not persist correctly.\n" +
						$"Expected {expectedValue:F2} (base {initialStats[statType]:F2} * 1.02),\n" +
						$"got {persistedValue:F2}");
				}
			}

			// Verify ascension level persists
			if (progress.AscensionLevel != 3)
			{
				testPassed = false;
				LogError(testName, $"Ascension level did not persist. Expected 3, got {progress.AscensionLevel}");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestAscensionReset()
	{
		string testName = "Ascension Reset";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);

			// Setup initial state
			progress.Level = 50;
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension + 5;
			progress.CurrentExp = 1000;

			// Store pre-ascension values
			int oldLevel = progress.Level;
			int oldPrestige = progress.PrestigeLevel;
			float oldExp = progress.CurrentExp;

			// Perform ascension
			_playerManager.progressionManager.TryAscend(characterId);

			// Verify resets
			if (progress.Level != 1)
			{
				testPassed = false;
				LogError(testName, $"Level not reset. Expected 1, got {progress.Level}");
			}

			if (progress.PrestigeLevel != 0)
			{
				testPassed = false;
				LogError(testName, $"Prestige level not reset. Expected 0, got {progress.PrestigeLevel}");
			}

			if (progress.CurrentExp != 0)
			{
				testPassed = false;
				LogError(testName, $"Experience not reset. Expected 0, got {progress.CurrentExp}");
			}

			// Verify ascension level increased
			if (progress.AscensionLevel != 1)
			{
				testPassed = false;
				LogError(testName, $"Ascension level not increased. Expected 1, got {progress.AscensionLevel}");
			}

			DebugLogger.Log($"Reset Test - Pre-Ascension: Level {oldLevel}, Prestige {oldPrestige}, Exp {oldExp}",
						  DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Reset Test - Post-Ascension: Level {progress.Level}, Prestige {progress.PrestigeLevel}, Exp {progress.CurrentExp}",
						  DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultipleAscensions()
	{
		string testName = "Multiple Ascensions";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Record initial stats
			Dictionary<StatType, float> initialStats = new()
		{
			{ StatType.Strength, character.GetStatValue(StatType.Strength) },
			{ StatType.Dexterity, character.GetStatValue(StatType.Dexterity)	 }
		};

			// Track progression through multiple ascensions
			int ascensionCount = 3;
			List<Dictionary<StatType, float>> statProgressions = new();

			// Log initial state
			DebugLogger.Log($"\nInitial Stats:", DebugLogger.LogCategory.Progress);
			foreach (var stat in initialStats)
			{
				DebugLogger.Log($"{stat.Key}: {stat.Value:F2}", DebugLogger.LogCategory.Progress);
			}

			// Perform multiple ascensions
			for (int i = 0; i < ascensionCount; i++)
			{
				progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
				bool ascended = _playerManager.progressionManager.TryAscend(characterId);

				if (!ascended)
				{
					testPassed = false;
					LogError(testName, $"Failed to ascend at iteration {i + 1}");
					break;
				}

				// Record stats after each ascension
				var currentStats = new Dictionary<StatType, float>();
				foreach (var statType in initialStats.Keys)
				{
					currentStats[statType] = character.GetStatValue(statType);
				}
				statProgressions.Add(currentStats);

				// Verify ascension level
				if (progress.AscensionLevel != i + 1)
				{
					testPassed = false;
					LogError(testName, $"Incorrect ascension level. Expected {i + 1}, got {progress.AscensionLevel}");
				}

				// Verify stat values
				foreach (var statType in initialStats.Keys)
				{

					float expectedValue = 0;
					float bonusPercent = 0;
					for (int j = 0; j <= i; j++)
					{
						bonusPercent = GameBalanceConfig.Progression.BaseStatBonusPerLevel * (j + 1);
						expectedValue += initialStats[statType] * bonusPercent;
					}
					expectedValue += initialStats[statType];
					float currentValue = character.GetStatValue(statType);

					DebugLogger.Log($"\nAscension {i + 1} - {statType}:", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"Expected bonus: +{bonusPercent:P2}", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"Expected value: {expectedValue:F2}", DebugLogger.LogCategory.Progress);
					DebugLogger.Log($"Actual value: {currentValue:F2}", DebugLogger.LogCategory.Progress);

					if (Math.Abs(currentValue - expectedValue) > 0.05f)
					{
						testPassed = false;
						LogError(testName,
							$"Incorrect stat progression for {statType} at ascension {i + 1}.\n" +
							$"Expected {expectedValue:F2} (base {initialStats[statType]:F2} * (1 + {bonusPercent:P2})),\n" +
							$"got {currentValue:F2}");
					}
				}
			}

			// Log final progression summary
			DebugLogger.Log($"\nProgression Summary:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < statProgressions.Count; i++)
			{
				DebugLogger.Log($"Ascension Level {i + 1}:", DebugLogger.LogCategory.Progress);
				foreach (var stat in statProgressions[i])
				{
					float multiplier = stat.Value / initialStats[stat.Key];
					DebugLogger.Log($"{stat.Key}: {stat.Value:F2} (x{multiplier:F2})",
								  DebugLogger.LogCategory.Progress);
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

	private void TestAscensionEvents()
	{
		string testName = "Ascension Events";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);

			// Prepare for ascension
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
			ResetTestFlags();

			// Perform ascension
			bool ascended = _playerManager.progressionManager.TryAscend(characterId);

			// Verify event was received
			if (!_ascensionEventReceived)
			{
				testPassed = false;
				LogError(testName, "Ascension event not received");
			}

			// Verify event data
			if (_lastAscensionLevel != 1)
			{
				testPassed = false;
				LogError(testName, $"Incorrect ascension level in event. Expected 1, got {_lastAscensionLevel}");
			}

			// Verify bonuses in event
			if (_lastUnlockedBonuses == null || !_lastUnlockedBonuses.Any())
			{
				testPassed = false;
				LogError(testName, "No bonuses reported in ascension event");
			}
			else
			{
				// Verify bonus values (should be 2% for first ascension)
				foreach (var bonus in _lastUnlockedBonuses)
				{
					float expectedBonus = 0.02f; // 2% for first ascension
					if (Math.Abs(bonus.Value - expectedBonus) > 0.001f)
					{
						testPassed = false;
						LogError(testName,
							$"Incorrect bonus value for {bonus.Key}. " +
							$"Expected {expectedBonus:P2}, got {bonus.Value:P2}");
					}

					DebugLogger.Log($"Verified {bonus.Key}: +{bonus.Value:P2}",
								  DebugLogger.LogCategory.Progress);
				}
			}

			// Log event details
			DebugLogger.Log("\nAscension Event Details:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Ascension Level: {_lastAscensionLevel}", DebugLogger.LogCategory.Progress);
			if (_lastUnlockedBonuses != null)
			{
				DebugLogger.Log("Unlocked Bonuses:", DebugLogger.LogCategory.Progress);
				foreach (var bonus in _lastUnlockedBonuses)
				{
					DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", DebugLogger.LogCategory.Progress);
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

	private void TestAscensionFailureCases()
	{
		string testName = "Ascension Failure Cases";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);

			// Test case 1: Insufficient prestige level
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension - 1;
			ResetTestFlags();
			if (_playerManager.progressionManager.TryAscend(characterId))
			{
				testPassed = false;
				LogError(testName, "Ascension succeeded with insufficient prestige level");
			}

			// Test case 2: Max ascension level
			progress.AscensionLevel = _config.MaxAscensionLevel;
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
			ResetTestFlags();
			if (_playerManager.progressionManager.TryAscend(characterId))
			{
				testPassed = false;
				LogError(testName, "Ascension succeeded beyond max level");
			}

			// Test case 3: Invalid character ID
			if (_playerManager.progressionManager.TryAscend("invalid_id"))
			{
				testPassed = false;
				LogError(testName, "Ascension succeeded with invalid character ID");
			}

			// Test case 4: Multiple rapid ascensions
			progress.AscensionLevel = 0;
			bool firstAscension = _playerManager.progressionManager.TryAscend(characterId);
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
			bool secondAscension = _playerManager.progressionManager.TryAscend(characterId);

			if (!firstAscension || !secondAscension)
			{
				testPassed = false;
				LogError(testName, "Incorrect handling of multiple rapid ascensions");
			}

			DebugLogger.Log("\nTested Failure Cases:", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Insufficient Prestige Level: {_config.PrestigeLevelsForAscension - 1}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Max Ascension Level: {_config.MaxAscensionLevel}", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Invalid Character ID Test", DebugLogger.LogCategory.Progress);
			DebugLogger.Log($"Multiple Rapid Ascensions Test", DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestAscensionWithPrestige()
	{
		string testName = "Ascension With Prestige";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Track power progression through prestige and ascension
			List<(string stage, float power, Dictionary<StatType, float> stats)> progressionSteps = new();

			// Record initial state
			progressionSteps.Add((
				"Initial",
				character.Power,
				RecordStats(character)
			));

			// First prestige
			progress.Level = _config.LevelsForPrestige;
			_playerManager.progressionManager.TryPrestige(characterId);
			progressionSteps.Add((
				"After First Prestige",
				character.Power,
				RecordStats(character)
			));

			// Second prestige
			progress.Level = _config.LevelsForPrestige;
			_playerManager.progressionManager.TryPrestige(characterId);
			progressionSteps.Add((
				"After Second Prestige",
				character.Power,
				RecordStats(character)
			));

			// Prepare for and perform ascension
			progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
			bool ascended = _playerManager.progressionManager.TryAscend(characterId);
			progressionSteps.Add((
				"After Ascension",
				character.Power,
				RecordStats(character)
			));

			// Verify ascension succeeded
			if (!ascended)
			{
				testPassed = false;
				LogError(testName, "Failed to ascend after prestiging");
			}

			// Verify prestige level reset
			if (progress.PrestigeLevel != 0)
			{
				testPassed = false;
				LogError(testName, $"Prestige level not reset after ascension. Expected 0, got {progress.PrestigeLevel}");
			}

			// Verify stat increases
			var initialStats = progressionSteps[0].stats;
			var postAscensionStats = progressionSteps[3].stats;

			foreach (var statType in initialStats.Keys)
			{
				float expectedBonus = 0.02f; // 2% for first ascension
				float expectedValue = initialStats[statType] * (1f + expectedBonus);
				float actualValue = postAscensionStats[statType];

				if (Math.Abs(actualValue - expectedValue) > 0.001f)
				{
					testPassed = false;
					LogError(testName,
						$"Incorrect {statType} value after ascension.\n" +
						$"Expected {expectedValue:F2} (base {initialStats[statType]:F2} * (1 + {expectedBonus:P2})),\n" +
						$"got {actualValue:F2}");
				}
			}

			// New prestige after ascension
			progress.Level = _config.LevelsForPrestige;
			_playerManager.progressionManager.TryPrestige(characterId);
			progressionSteps.Add((
				"Post-Ascension Prestige",
				character.Power,
				RecordStats(character)
			));

			// Log progression
			DebugLogger.Log("\nProgression Steps:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < progressionSteps.Count; i++)
			{
				var (stage, power, stats) = progressionSteps[i];
				float powerMultiplier = i > 0 ? power / progressionSteps[0].power : 1;

				DebugLogger.Log($"\n{stage}:", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Power: {power:F2} (x{powerMultiplier:F2})", DebugLogger.LogCategory.Progress);

				foreach (var stat in stats)
				{
					float statMultiplier = i > 0 ? stat.Value / progressionSteps[0].stats[stat.Key] : 1;
					DebugLogger.Log($"{stat.Key}: {stat.Value:F2} (x{statMultiplier:F2})",
								  DebugLogger.LogCategory.Progress);
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

	private void TestAscensionPowerScaling()
	{
		string testName = "Ascension Power Scaling";
		bool testPassed = true;

		try
		{

			DebugLogger.SetLogging(true);
			DebugLogger.SetLogCategories(DebugLogger.LogCategory.Progress | DebugLogger.LogCategory.Events);
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Track power scaling across multiple ascension levels
			List<(long level, float power, Dictionary<StatType, float> stats)> scalingData = new();

			// Record initial state
			scalingData.Add((0, character.Power, RecordStats(character)));

			// Test multiple ascension levels
			int testLevels = 5;
			for (int i = 0; i < testLevels; i++)
			{
				// Prepare and perform ascension
				progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
				_playerManager.progressionManager.TryAscend(characterId);

				// Record data after ascension
				scalingData.Add((
					progress.AscensionLevel,
					character.Power,
					RecordStats(character)
				));

				// Calculate expected bonus for this level
				float expectedBonus = (i + 1) * 0.02f; // 2% per level

				// Verify stats at each level
				foreach (var stat in scalingData[0].stats.Keys)
				{
					float baseValue = scalingData[0].stats[stat];
					float expectedValue = baseValue * (1f + expectedBonus);
					float actualValue = character.GetStatValue(stat)	;

					if (Math.Abs(actualValue - expectedValue) > 0.001f)
					{
						testPassed = false;
						LogError(testName,
							$"Incorrect {stat} scaling at ascension {i + 1}.\n" +
							$"Expected {expectedValue:F2} (base {baseValue:F2} * (1 + {expectedBonus:P2})),\n" +
							$"got {actualValue:F2}");
					}
				}

				// Verify power increase
				float powerRatio = character.Power / scalingData[0].power;
				float minimumExpectedRatio = 1f + expectedBonus;

				//if (powerRatio < minimumExpectedRatio)
				if (powerRatio < minimumExpectedRatio - 0.005f)  // Allow 0.5% tolerance
				{
					testPassed = false;
					LogError(testName,
						$"Insufficient power scaling at ascension {i + 1}.\n" +
						$"Expected minimum ratio: {minimumExpectedRatio:F2},\n" +
						$"got {powerRatio:F2}");
				}
			}

			// Log scaling progression
			DebugLogger.Log("\nAscension Power Scaling:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < scalingData.Count; i++)
			{
				var (level, power, stats) = scalingData[i];
				float powerMultiplier = i > 0 ? power / scalingData[0].power : 1;

				DebugLogger.Log($"\nAscension Level {level}:", DebugLogger.LogCategory.Progress);
				DebugLogger.Log($"Power: {power:F2} (x{powerMultiplier:F2})", DebugLogger.LogCategory.Progress);

				foreach (var stat in stats)
				{
					float statMultiplier = i > 0 ? stat.Value / scalingData[0].stats[stat.Key] : 1;
					DebugLogger.Log($"{stat.Key}: {stat.Value:F2} (x{statMultiplier:F2})",
								  DebugLogger.LogCategory.Progress);
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

	private void TestAscensionBonusProgression()
	{
		string testName = "Ascension Bonus Progression";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter();
			var progress = _playerManager.progressionManager.GetProgressData(characterId);
			var character = _playerManager.GetCharacter(characterId);

			// Structure to track bonus progression
			var bonusProgression = new List<(
				long ascensionLevel,
				Dictionary<string, float> bonuses,
				Dictionary<StatType, float> stats,
				float powerLevel
			)>();

			// Record initial state
			bonusProgression.Add((
				0,
				new Dictionary<string, float>(),
				RecordStats(character),
				character.Power
			));

			// Test multiple ascension levels
			int testLevels = 3;
			for (int i = 0; i < testLevels; i++)
			{
				// Perform ascension
				progress.PrestigeLevel = _config.PrestigeLevelsForAscension;
				_playerManager.progressionManager.TryAscend(characterId);

				// Record progression data
				bonusProgression.Add((
					progress.AscensionLevel,
					new Dictionary<string, float>(_lastUnlockedBonuses),
					RecordStats(character),
					character.Power
				));

				// Verify bonus values at current level
				float expectedBonus = (i + 1) * 0.02f;
				foreach (var bonus in _lastUnlockedBonuses)
				{
					if (Math.Abs(bonus.Value - expectedBonus) > 0.001f)
					{
						testPassed = false;
						LogError(testName,
							$"Incorrect bonus for {bonus.Key} at ascension {i + 1}.\n" +
							$"Expected {expectedBonus:P2}, got {bonus.Value:P2}");
					}
				}

				// Verify stat values
				foreach (var statType in bonusProgression[0].stats.Keys)
				{
					float baseValue = bonusProgression[0].stats[statType];
					float expectedValue = baseValue * (1f + expectedBonus);
					float actualValue = character.GetStatValue(statType);

					if (Math.Abs(actualValue - expectedValue) > 0.001f)
					{
						testPassed = false;
						LogError(testName,
							$"Incorrect {statType} value at ascension {i + 1}.\n" +
							$"Expected {expectedValue:F2} (base {baseValue:F2} * (1 + {expectedBonus:P2})),\n" +
							$"got {actualValue:F2}");
					}
				}
			}

			// Log progression
			DebugLogger.Log("\nBonus Progression:", DebugLogger.LogCategory.Progress);
			for (int i = 0; i < bonusProgression.Count; i++)
			{
				var (level, bonuses, stats, power) = bonusProgression[i];

				DebugLogger.Log($"\nAscension Level {level}:", DebugLogger.LogCategory.Progress);
				if (bonuses.Any())
				{
					DebugLogger.Log("Active Bonuses:", DebugLogger.LogCategory.Progress);
					foreach (var bonus in bonuses)
					{
						DebugLogger.Log($"  {bonus.Key}: +{bonus.Value:P2}",
									  DebugLogger.LogCategory.Progress);
					}
				}

				DebugLogger.Log("Stats:", DebugLogger.LogCategory.Progress);
				foreach (var stat in stats)
				{
					float multiplier = i > 0 ? stat.Value / bonusProgression[0].stats[stat.Key] : 1;
					DebugLogger.Log($"  {stat.Key}: {stat.Value:F2} (x{multiplier:F2})",
								  DebugLogger.LogCategory.Progress);
				}

				float powerMultiplier = i > 0 ? power / bonusProgression[0].powerLevel : 1;
				DebugLogger.Log($"Power: {power:F2} (x{powerMultiplier:F2})",
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

	private Dictionary<StatType, float> RecordStats(Character character)
	{
		return Enum.GetValues(typeof(StatType))
			.Cast<StatType>()
			.ToDictionary(
				statType => statType,
				statType => character.GetStatValue(statType) 
			);
	}
	#region Support Methods
	private string CreateTestCharacter()
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, 1);
		return characterId;
	}

	private void ResetTestFlags()
	{
		_ascensionEventReceived = false;
		_lastAscensionLevel = 0;
		_lastUnlockedBonuses = null;
		_lastPowerMultiplier = 0;
	}

	private void OnAscensionGained(AscensionEventArgs args)
	{
		_ascensionEventReceived = true;
		_lastAscensionLevel = args.NewAscensionLevel;
		_lastUnlockedBonuses = args.UnlockedBonuses;
		_lastPowerMultiplier = args.TotalAscensionMultiplier;

		DebugLogger.Log(
			$"Ascension gained: Level {args.PreviousAscensionLevel} -> {args.NewAscensionLevel}\n" +
			$"Multiplier: {args.AscensionMultiplierGained:P2}\n" +
			$"Unlocked Bonuses: {string.Join(", ", args.UnlockedBonuses.Select(b => $"{b.Key}: {b.Value:P2}"))}",
			DebugLogger.LogCategory.Progress
		);
	}

	private void LogError(string testName, string message)
	{
		TestResults.Add($"Error in {testName}: {message}");
		DebugLogger.Log($"Error in {testName}: {message}", DebugLogger.LogCategory.Progress);
		System.Diagnostics.Debug.WriteLine($"Godot: {message}");
	}

	private void LogTestResult(string testName, bool passed)
	{
		string result = passed ? "PASSED" : "FAILED";
		TestResults.Add($"Test {testName}: {result}");
		DebugLogger.Log($"Test {testName}: {result}", DebugLogger.LogCategory.Progress);
	}

	private void OutputTestResults()
	{
		DebugLogger.Log("\n=== Ascension System Test Results ===", DebugLogger.LogCategory.Progress);
		foreach (var result in TestResults)
		{
			DebugLogger.Log(result, DebugLogger.LogCategory.Progress);
		}
		DebugLogger.Log("===============================\n", DebugLogger.LogCategory.Progress);
	}
	#endregion
}