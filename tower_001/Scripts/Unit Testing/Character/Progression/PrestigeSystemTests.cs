using Godot;
using System.Collections.Generic;
using System;
using static GlobalEnums;
using System.Reflection.Metadata.Ecma335;

public class PrestigeSystemTests : BaseTestSuite 
{
	private readonly ProgressionManager _progressionManager;
	private readonly PlayerManager _playerManager;
	private bool _prestigeEventReceived;
	private int _lastPrestigeLevel;
	private ProgressionConfig _config;



	public PrestigeSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_playerManager = playerManager;
		_progressionManager = playerManager.progressionManager;
		_config = new ProgressionConfig
		{
			BaseExpForLevel = 1000f,
			ExpScalingFactor = 1.15f,
			LevelsForPrestige = 100000,
			PrestigePowerMultiplier = 2.0f
		};
		_progressionManager.Config = _config;
		RegisterEventHandlers();
	}
	private void RegisterEventHandlers()
	{
		Globals.Instance.gameMangers.Events.AddHandler<PrestigeEventArgs>(
			EventType.PrestigeLevelGained,
			OnPrestigeGained
		);
	}

	public void RunAllTests()
	{

		if (TestConfig.IsCategoryEnabled(TestCategory.Prestige))
		{
			TestPrestigeRequirements();
			TestPrestigeMultipliers();
			TestPrestigeCosts();
			TestPrestigeReset();
			TestPrestigeBonuses();
			TestPrestigeEvents();
			TestMultiPrestige();

			OutputTestResults();
		}
	}
	public ProgressionData GetProgressData(string characterId)
	{
		return Globals.Instance.gameMangers.Player.progressionManager.GetProgressData(characterId);
    }
    

public bool CanPrestige(string characterId, int prestigeLevel)
{
		var progress = GetProgressData(characterId);
        long prestigeCost = GetPrestigeCost(characterId, prestigeLevel);
		//&& progress.ResourceManager.CanAfford(ResourceType.Currency, prestigeCost
		return progress.Level >= _config.LevelsForPrestige;
}

public long GetPrestigeCost(string characterId, int prestigeLevel)
{
    if (_config.PrestigeCosts.TryGetValue(prestigeLevel, out long cost))
    {
        return cost;
    }
    return 0;
}

public void HandlePrestigeReset(string characterId, ProgressionData progress)
{
    progress.Level = 1;
    progress.CurrentExp = 0;
    // Reset other appropriate values while maintaining prestige bonuses
}
	private void TestPrestigeRequirements()
	{
		
		string testName = "Prestige Requirements";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);

			var progress = _progressionManager.GetProgressData(characterId);

			// Test prestige requirements not met
			progress.Level = _config.LevelsForPrestige - 1;
			if (_progressionManager.TryPrestige(characterId))
			{
				testPassed = false;
				LogError(testName, "Prestige succeeded without meeting requirements");
			}

			// Test prestige requirements met
			progress.Level = _config.LevelsForPrestige;
			if (!_progressionManager.TryPrestige(characterId))
			{
				testPassed = false;
				LogError(testName, "Prestige failed even though requirements were met");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestPrestigeMultipliers()
	{
		string testName = "Prestige Multipliers";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var progress = _progressionManager.GetProgressData(characterId);

			// Test prestige multiplier application
			int prestigeLevel = 5;
			progress.PrestigeLevel = prestigeLevel;
			float expectedMultiplier = (float)Math.Pow(_config.PrestigePowerMultiplier, prestigeLevel);
			float actualMultiplier = progress.TotalPowerMultiplier;

			if (Math.Abs(actualMultiplier - expectedMultiplier) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Prestige multiplier mismatch. Expected {expectedMultiplier:F2}, got {actualMultiplier:F2}");
			}

			DebugLogger.Log($"Prestige multiplier test - Expected: {expectedMultiplier:F2}, Actual: {actualMultiplier:F2}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestPrestigeCosts()
	{
		string testName = "Prestige Costs";
		bool testPassed = true;

		try
		{
			// Create a character at the minimum required prestige level
			var characterId = CreateTestCharacter(_config.LevelsForPrestige);
			var progress = _progressionManager.GetProgressData(characterId);

			// Test prestige cost requirements at the minimum level
			long expectedCost = _config.PrestigeCosts[1]; // change here
			//_playerManager.SetCharacterLevel(characterId, (int)expectedCost);
			progress.Level = (int)expectedCost; // change here
			if (!_progressionManager.CanPrestige(characterId,1))
			{
				testPassed = false;
				LogError(testName, $"Failed to prestige at level {_config.LevelsForPrestige} even though cost is met");
			}

			if (_progressionManager.GetPrestigeCost(characterId, 1) != expectedCost)
			{
				testPassed = false;
				LogError(testName, $"Prestige cost mismatch at level {_config.LevelsForPrestige}. Expected {expectedCost}, got {_progressionManager.GetPrestigeCost(characterId, 1)}");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestPrestigeReset()
	{
		string testName = "Prestige Reset";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var progress = _progressionManager.GetProgressData(characterId);

			// Prestige the character a few times
			int prestigeLevels = 3;
			for (int i = 0; i < prestigeLevels; i++)
			{
				_progressionManager.TryPrestige(characterId);
			}

			int prevPrestigeLevel = progress.PrestigeLevel;
			int prevLevel = progress.Level;
			float prevPowerMultiplier = progress.TotalPowerMultiplier;

			// Handle prestige reset
			_progressionManager.HandlePrestigeReset(characterId);

			if (progress.PrestigeLevel != prevPrestigeLevel ||
				progress.Level != 1 ||
				progress.CurrentExp != 0 ||
				Math.Abs(progress.TotalPowerMultiplier - prevPowerMultiplier) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Prestige reset did not restore the expected state");
			}

			DebugLogger.Log($"Prestige reset test - " +
				$"Previous Prestige: {prevPrestigeLevel}, New Prestige: {progress.PrestigeLevel}, " +
				$"Previous Level: {prevLevel}, New Level: {progress.Level}, " +
				$"Previous Power: {prevPowerMultiplier:F2}, New Power: {progress.TotalPowerMultiplier:F2}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestPrestigeBonuses()
	{
		string testName = "Prestige Bonuses";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var progress = _progressionManager.GetProgressData(characterId);


		


			// Test prestige bonus application
			int prestigeLevel = 5;
			progress.PrestigeLevel = prestigeLevel;
			float expectedBonus = CalculatePrestigeBonus(prestigeLevel);
			float actualBonus = progress.PrestigeMultiplierGained;

			if (Math.Abs(actualBonus - expectedBonus) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Prestige bonus mismatch. Expected {expectedBonus:F2}, got {actualBonus:F2}");
			}

			DebugLogger.Log($"Prestige bonus test - Expected: {expectedBonus:F2}, Actual: {actualBonus:F2}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestPrestigeEvents()
	{
		string testName = "Prestige Events";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var progress = _progressionManager.GetProgressData(characterId);

			progress.Level = _config.LevelsForPrestige;

			ResetTestFlags();
			_progressionManager.TryPrestige(characterId);

			if (!_prestigeEventReceived)
			{
				testPassed = false;
				LogError(testName, "Prestige event not received");
			}

			if (_lastPrestigeLevel != progress.PrestigeLevel)
			{
				testPassed = false;
				LogError(testName, $"Prestige level mismatch. Expected {progress.PrestigeLevel}, got {_lastPrestigeLevel}");
			}

			DebugLogger.Log($"Prestige event test - Received: {_prestigeEventReceived}, Prestige Level: {_lastPrestigeLevel}",
				DebugLogger.LogCategory.Progress);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultiPrestige()
	{
		string testName = "Multiple Prestige";
		bool testPassed = true;

		try
		{
			var characterId = CreateTestCharacter(1);
			var progress = _progressionManager.GetProgressData(characterId);

			int maxPrestige = 5;
			float initialPowerMultiplier = progress.TotalPowerMultiplier;

			// Prestige the character multiple times
			for (int i = 0; i < maxPrestige; i++)
			{
				ResetTestFlags();
				long expectedCost = _config.PrestigeCosts[i + 1]; // change here
				progress.Level = (int)expectedCost; // change here

				_progressionManager.TryPrestige(characterId);

				if (!_prestigeEventReceived)
				{
					testPassed = false;
					LogError(testName, "Prestige event not received");
				}

				if (_lastPrestigeLevel != progress.PrestigeLevel)
				{
					testPassed = false;
					LogError(testName, $"Prestige level mismatch. Expected {progress.PrestigeLevel}, got {_lastPrestigeLevel}");
				}

				float expectedMultiplier = initialPowerMultiplier * (float)Math.Pow(_config.PrestigePowerMultiplier, i + 1);
				if (Math.Abs(progress.TotalPowerMultiplier - expectedMultiplier) > 0.01f)
				{
					testPassed = false;
					LogError(testName, $"Power multiplier mismatch. Expected {expectedMultiplier:F2}, got {progress.TotalPowerMultiplier:F2}");
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

	#region Support Methods

	private void ResetTestFlags()
	{
		_prestigeEventReceived = false;
		_lastPrestigeLevel = 0;
	}

	private void OnPrestigeGained(PrestigeEventArgs args)
	{
		_prestigeEventReceived = true;
		_lastPrestigeLevel = args.NewPrestigeLevel;

		DebugLogger.Log(
			$"Prestige gained: {args.PreviousPrestigeLevel} -> {args.NewPrestigeLevel}, " +
			$"Multiplier Gained: {args.PrestigeMultiplierGained:F2}, " +
			$"Total Multiplier: {args.TotalPrestigeMultiplier:F2}",
			DebugLogger.LogCategory.Progress
		);
	}


	private float CalculatePrestigeBonus(int prestigeLevel)
	{
		// Implement the logic to calculate the prestige bonus based on the level
		return (float)Math.Pow(_config.PrestigePowerMultiplier, prestigeLevel); 
		//return (prestigeLevel - 1) * 0.5f;
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
		GD.Print("\n=== Prestige System Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}

	private string CreateTestCharacter(int level)
	{
		string characterId = _playerManager.CreateCharacter("knight");
		_playerManager.SetCharacterLevel(characterId, level);
		return characterId;
	}

	#endregion
}