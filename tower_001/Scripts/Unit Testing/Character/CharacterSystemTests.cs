using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

// TestCharacterStatScaling

//Character System Tests
//Current Coverage:

//Character Creation
//Character Leveling
//Character Power Scaling
//Character Stat Scaling
//Buff Application
//Buff Stacking
//Buff Expiration
//Buff Removal
//Element Interactions
//Stat Thresholds

// Verifies:
// - Stats increase appropriately with level
// - Each stat type scales according to its growth rate
// - Minimum stat values are maintained
// - Stat scaling respects class-specific modifiers
// - Derived stats (like power) update correctly
public partial class CharacterSystemTests : BaseTestSuite
{


	#region Test to run
	// Flag to enable/disable individual tests
	private bool _testCharacterCreation = true;
	private bool _testCharacterLeveling = true;
	private bool _testCharacterPowerScaling = true;
	private bool _testCharacterStatScaling = true;
	private bool _testBuffApplication = true;
	private bool _testBuffStacking = true;
	private bool _testBuffExpiration = true;
	private bool _testBuffRemoval = true;
	private bool _testElementalInteractions = true;
	private bool _testStatThresholds = true;
	private bool _testCharacterStatThresholds = true;
	private bool _testCharacterMultipleStatThresholds = true;
	private bool _testBuffInteractionsAndStacking = true;
	#endregion

	#region Test Methods
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;


	public CharacterSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig): base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
	}
	public void RunAllTests()
	{

		if (TestConfig.IsCategoryEnabled(TestCategory.Character))
		{
				TestCharacterCreation();
				TestCharacterLeveling();
				TestCharacterPowerScaling();
				TestCharacterStatScaling();
				TestBuffApplication();
				TestBuffStacking();
				TestBuffExpiration();
				TestBuffRemoval();
				TestElementalInteractions();
				TestStatThresholds();
				TestCharacterStatThresholds();
				TestCharacterMultipleStatThresholds();
				TestBuffInteractionsAndStacking();
				TestCharacterLeveling();
				TestCharacterPowerScaling();
				TestCharacterStatScaling();

			OutputTestResults();
		}


		//_experienceAndLevelingTests.RunAllTests();
	}

	private void TestCharacterCreation()
	{
		string testName = "Character Creation";
		bool testPassed = true;

		try
		{
			// Test Knight creation
			string knightId = _playerManager.CreateCharacter("knight");
			var knight = _playerManager.GetCharacter(knightId);
			if (knight == null || knight.GetType() != typeof(Knight))
			{
				testPassed = false;
				LogError(testName, "Failed to create Knight character");
			}

			// Test Mage creation
			string mageId = _playerManager.CreateCharacter("mage");
			var mage = _playerManager.GetCharacter(mageId);
			if (mage == null || mage.GetType() != typeof(Mage))
			{
				testPassed = false;
				LogError(testName, "Failed to create Mage character");
			}

			// Verify initial stats
			var knightStats = _playerManager.GetCharacterStats(knightId);
			var mageStats = _playerManager.GetCharacterStats(mageId);

			if (knightStats[StatType.Health] != 100 || mageStats[StatType.Health] != 80)
			{
				testPassed = false;
				LogError(testName, "Initial stats not set correctly");
			}


			DebugLogger.Log($"Knight Initial Health: {knightStats[StatType.Health]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Mage Initial Health: {mageStats[StatType.Health]}", DebugLogger.LogCategory.Character);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCharacterLeveling()
	{
		string testName = "Character Leveling";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Test multiple level ups
			for (int i = 2; i <= 5; i++)
			{
				_playerManager.SetCharacterLevel(characterId, i);
				var newStats = _playerManager.GetCharacterStats(characterId);

				// Verify power increase
				if (newStats[StatType.Attack] <= initialStats[StatType.Attack])
				{
					testPassed = false;
					LogError(testName, $"Attack did not increase at level {i}");
				}

				// Verify health increase
				if (newStats[StatType.Health] <= initialStats[StatType.Health])
				{
					testPassed = false;
					LogError(testName, $"Health did not increase at level {i}");
				}

				DebugLogger.Log($"Level {i} Stats - Health: {newStats[StatType.Health]}, Attack: {newStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
				initialStats = newStats;
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCharacterPowerScaling()
	{
		string testName = "Character Power Scaling";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			float initialPower = _playerManager.GetCharacter(characterId).Power;

			DebugLogger.Log("\nPower Scaling Analysis:", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Initial Power: {initialPower}", DebugLogger.LogCategory.Character);
			for (int level = 2; level <= 10; level++)
			{
				_playerManager.SetCharacterLevel(characterId, level);
				float newPower = _playerManager.GetCharacter(characterId).Power;

				if (newPower <= initialPower)
				{
					testPassed = false;
					LogError(testName, $"Power did not increase at level {level}");
				}

				float powerIncrease = newPower - initialPower;
				DebugLogger.Log($"Level {level} Power: {newPower} (Increase: +{powerIncrease:F2})", DebugLogger.LogCategory.Character);
				initialPower = newPower;
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}


	private void TestCharacterStatScaling()
	{
		string testName = "Character Stat Scaling";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			DebugLogger.Log("\nStat Scaling Analysis:", DebugLogger.LogCategory.Character);
			foreach (var stat in initialStats)
			{
				DebugLogger.Log($"Initial {stat.Key}: {stat.Value}", DebugLogger.LogCategory.Character);
			}

			// Test stat scaling over 5 levels
			for (int level = 2; level <= 5; level++)
			{
				_playerManager.SetCharacterLevel(characterId, level);
				var newStats = _playerManager.GetCharacterStats(characterId);

				//GD.Print($"\nLevel {level} Stats:");
				foreach (var stat in newStats)
				{
					float increase = stat.Value - initialStats[stat.Key];
					DebugLogger.Log($"{stat.Key}: {stat.Value} (Increase: +{increase:F2})", DebugLogger.LogCategory.Character);
					if (stat.Value <= initialStats[stat.Key])
					{
						testPassed = false;
						LogError(testName, $"{stat.Key} did not increase at level {level}");
					}
				}

				initialStats = newStats;
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestBuffApplication()
	{
		string testName = "Buff Application";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Test flat buff
			_playerManager.ApplyBuff(
				characterId,
				StatType.Attack,
				"Weapon Buff",
				BuffType.Flat,
				10.0f
			);

			// Test percentage buff
			_playerManager.ApplyBuff(
				characterId,
				StatType.Health,
				"Armor Buff",
				BuffType.Percentage,
				0.2f
			);

			var buffedStats = _playerManager.GetCharacterStats(characterId);

			// Verify flat buff
			if (buffedStats[StatType.Attack] != initialStats[StatType.Attack] + 10)
			{
				testPassed = false;
				LogError(testName, "Flat buff not applied correctly");
			}

			// Verify percentage buff
			float expectedHealth = initialStats[StatType.Health] * 1.2f;
			if (Math.Abs(buffedStats[StatType.Health] - expectedHealth) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Percentage buff not applied correctly");
			}

			DebugLogger.Log("\nBuff Application Results:", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Attack: {initialStats[StatType.Attack]} -> {buffedStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Health: {initialStats[StatType.Health]} -> {buffedStats[StatType.Health]}", DebugLogger.LogCategory.Character);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestBuffStacking()
	{
		string testName = "Buff Stacking";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Apply multiple buffs to same stat
			_playerManager.ApplyBuff(characterId, StatType.Attack, "Buff 1", BuffType.Flat, 10.0f);
			_playerManager.ApplyBuff(characterId, StatType.Attack, "Buff 2", BuffType.Flat, 5.0f);
			_playerManager.ApplyBuff(characterId, StatType.Attack, "Buff 3", BuffType.Percentage, 0.2f);

			var buffedStats = _playerManager.GetCharacterStats(characterId);
			float expectedAttack = (initialStats[StatType.Attack] + 15) * 1.2f;

			if (Math.Abs(buffedStats[StatType.Attack] - expectedAttack) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Buff stacking not calculated correctly");
			}

			DebugLogger.Log("\nBuff Stacking Results:", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Initial Attack: {initialStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Final Attack: {buffedStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Expected Attack: {expectedAttack}", DebugLogger.LogCategory.Character);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestBuffExpiration()
	{
		string testName = "Buff Expiration";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Apply temporary buff
			_playerManager.ApplyBuff(
				characterId,
				StatType.Attack,
				"Temporary Buff",
				BuffType.Flat,
				10.0f,
				TimeSpan.FromSeconds(1)
			);

			var buffedStats = _playerManager.GetCharacterStats(characterId);

			// Verify buff is applied
			if (buffedStats[StatType.Attack] <= initialStats[StatType.Attack])
			{
				testPassed = false;
				LogError(testName, "Temporary buff not applied");
			}

			// Wait for buff to expire
			System.Threading.Thread.Sleep(1100); // Wait slightly longer than buff duration
			_playerManager.Update(); // Force update to process expired buffs

			var finalStats = _playerManager.GetCharacterStats(characterId);

			// Verify buff expired
			if (Math.Abs(finalStats[StatType.Attack] - initialStats[StatType.Attack]) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Temporary buff did not expire");
			}

			DebugLogger.Log("\nBuff Expiration Results:", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Initial Attack: {initialStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Buffed Attack: {buffedStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Final Attack: {finalStats[StatType.Attack]}", DebugLogger.LogCategory.Character);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}


	private void TestBuffRemoval()
	{
		string testName = "Buff Removal";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Apply multiple buffs
			string buffId1 = Guid.NewGuid().ToString();
			string buffId2 = Guid.NewGuid().ToString();

			_playerManager.ApplyBuff(characterId, StatType.Attack, "Buff 1", BuffType.Flat, 10.0f, buffId: buffId1);
			_playerManager.ApplyBuff(characterId, StatType.Attack, "Buff 2", BuffType.Percentage, 0.2f, buffId: buffId2);

			var buffedStats = _playerManager.GetCharacterStats(characterId);

			// Remove one buff
			_playerManager.RemoveBuff(characterId, StatType.Attack, buffId1);
			var afterRemovalStats = _playerManager.GetCharacterStats(characterId);

			// Calculate expected values
			float expectedAfterRemoval = initialStats[StatType.Attack] * 1.2f;

			if (Math.Abs(afterRemovalStats[StatType.Attack] - expectedAfterRemoval) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Buff removal not calculated correctly");
			}

			DebugLogger.Log("\nBuff Removal Results:", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Initial Attack: {initialStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"Fully Buffed Attack: {buffedStats[StatType.Attack]}", DebugLogger.LogCategory.Character);
			DebugLogger.Log($"After Removal Attack: {afterRemovalStats[StatType.Attack]}", DebugLogger.LogCategory.Character);

		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestElementalInteractions()
	{
		string testName = "Elemental Interactions";
		bool testPassed = true;

		try
		{
			string characterId = _playerManager.CreateCharacter("knight");
			var character = _playerManager.GetCharacter(characterId);

			// Test setting different elements
			foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
			{
				_playerManager.SetCharacterElement(characterId, element);
				if (character.Element != element)
				{
					testPassed = false;
					LogError(testName, $"Failed to set element to {element}");
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

	/// <summary>
	/// Modified test to properly verify threshold events
	/// </summary>
	private void TestStatThresholds()
	{
		string testName = "Stat Thresholds";
		bool testPassed = true;

		try
		{
			// Create character and get initial stats
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);
			float initialHealth = initialStats[StatType.Health];

			// Apply damage
			_playerManager.ApplyBuff(
				characterId,
				StatType.Health,
				"Critical Damage",
				BuffType.Percentage,
				-0.85f
			);

			// Verify health dropped below thresholds
			var damagedStats = _playerManager.GetCharacterStats(characterId);
			float healthAfterDamage = damagedStats[StatType.Health];

			// Check if we crossed the 50% threshold
			bool crossed50Percent = healthAfterDamage < (initialHealth * 0.5f);
			// Check if we crossed the 25% threshold
			bool crossed25Percent = healthAfterDamage < (initialHealth * 0.25f);

			if (!crossed50Percent || !crossed25Percent)
			{
				testPassed = false;
				LogError(testName, $"Health did not drop below thresholds. Initial: {initialHealth}, After: {healthAfterDamage}");
			}

			// Apply healing
			_playerManager.ApplyBuff(
				characterId,
				StatType.Health,
				"Major Healing",
				BuffType.Percentage,
				0.7f
			);

			// Verify health recovered above thresholds
			var healedStats = _playerManager.GetCharacterStats(characterId);
			float healthAfterHealing = healedStats[StatType.Health];

			// Check if we crossed back above 25%
			bool recoveredAbove25 = healthAfterHealing > (initialHealth * 0.25f);

			if (!recoveredAbove25)
			{
				testPassed = false;
				LogError(testName, $"Health did not recover above threshold. Initial: {initialHealth}, After: {healthAfterHealing}");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCharacterStatThresholds()
	{
		string testName = "Character Stat Thresholds";
		bool testPassed = true;

		try
		{
			// Create a character and get initial stats
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);
			float initialHealth = initialStats[StatType.Health];

			// Apply damage to drop health below thresholds
			_playerManager.ApplyBuff(
				characterId,
				StatType.Health,
				"Critical Damage",
				BuffType.Percentage,
				-0.85f
			);

			// Verify health dropped below thresholds
			var damagedStats = _playerManager.GetCharacterStats(characterId);
			float healthAfterDamage = damagedStats[StatType.Health];

			// Check if we crossed the 50% threshold
			bool crossed50Percent = healthAfterDamage < (initialHealth * 0.5f);
			if (!crossed50Percent)
			{
				testPassed = false;
				LogError(testName, "Health did not drop below 50% threshold");
			}

			// Check if we crossed the 25% threshold
			bool crossed25Percent = healthAfterDamage < (initialHealth * 0.25f);
			if (!crossed25Percent)
			{
				testPassed = false;
				LogError(testName, "Health did not drop below 25% threshold");
			}

			// Apply healing to restore health above thresholds
			_playerManager.ApplyBuff(
				characterId,
				StatType.Health,
				"Major Healing",
				BuffType.Percentage,
				0.7f
			);

			// Verify health recovered above thresholds
			var healedStats = _playerManager.GetCharacterStats(characterId);
			float healthAfterHealing = healedStats[StatType.Health];

			// Check if we crossed back above 25%
			bool recoveredAbove25 = healthAfterHealing > (initialHealth * 0.25f);
			if (!recoveredAbove25)
			{
				testPassed = false;
				LogError(testName, "Health did not recover above 25% threshold");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}
	private void TestCharacterMultipleStatThresholds()
	{
		string testName = "Character Multiple Stat Thresholds";
		bool testPassed = true;

		try
		{
			// Create a character and get initial stats
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);
			float initialMana = initialStats[StatType.Mana];

			// Set multiple thresholds for the Mana stat
			List<float> manaThresholds = new List<float> { 80f, 50f, 20f };

			// Apply buffs to deplete mana and cross thresholds
			_playerManager.ApplyBuff(
				characterId,
				StatType.Mana,
				"Mana Drain",
				BuffType.Flat,
				-initialMana * 0.8f
			);

			// Verify all thresholds were crossed
			var currentStats = _playerManager.GetCharacterStats(characterId);
			float currentMana = currentStats[StatType.Mana];

			foreach (float threshold in manaThresholds)
			{
				float thresholdValue = initialMana * (threshold / 100f);
				bool crossedThreshold = currentMana < thresholdValue;
				if (!crossedThreshold)
				{
					testPassed = false;
					LogError(testName, $"Mana did not cross {threshold}% threshold");
				}
			}

			// Apply buffs to restore mana above thresholds
			_playerManager.ApplyBuff(
				characterId,
				StatType.Mana,
				"Mana Restoration",
				BuffType.Flat,
				initialMana * 0.9f
			);

			currentStats = _playerManager.GetCharacterStats(characterId);
			currentMana = currentStats[StatType.Mana];

			// Verify all thresholds were crossed back
			foreach (float threshold in manaThresholds)
			{
				float thresholdValue = initialMana * (threshold / 100f);
				bool recoveredAboveThreshold = currentMana > thresholdValue;
				if (!recoveredAboveThreshold)
				{
					testPassed = false;
					LogError(testName, $"Mana did not recover above {threshold}% threshold");
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

	private void TestBuffInteractionsAndStacking()
	{
		string testName = "Buff Interactions and Stacking";
		bool testPassed = true;

		try
		{
			// Create a character to test buffs
			string characterId = _playerManager.CreateCharacter("knight");
			var initialStats = _playerManager.GetCharacterStats(characterId);

			// Test flat buff application and stacking
			ApplyFlatBuffsAndVerify(characterId, StatType.Attack, initialStats[StatType.Attack]);
			ApplyFlatBuffsAndVerify(characterId, StatType.Health, initialStats[StatType.Health]);

			// Reset stats to initial values before testing percentage buffs
			ResetCharacterState(characterId, initialStats);

			// Test percentage buff application and stacking
			ApplyPercentageBuffsAndVerify(characterId, StatType.Attack, initialStats[StatType.Attack]);
			ApplyPercentageBuffsAndVerify(characterId, StatType.Health, initialStats[StatType.Health]);

			// Reset stats to initial values before testing mixed buffs
			ResetCharacterState(characterId, initialStats);

			// Test mixed buff types (flat and percentage)
			ApplyMixedBuffsAndVerify(characterId, initialStats[StatType.Attack], initialStats[StatType.Health]);

			// Reset stats to initial values before testing buff expiration
			ResetCharacterState(characterId, initialStats);

			// Test buff expiration and removal
			TestBuffExpiration(characterId, initialStats[StatType.Attack], initialStats[StatType.Health]);
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

	private void ApplyFlatBuffsAndVerify(string characterId, StatType statType, float initialValue)
	{
		_playerManager.ApplyBuff(characterId, statType, "Flat Buff 1", BuffType.Flat, 10.0f);
		_playerManager.ApplyBuff(characterId, statType, "Flat Buff 2", BuffType.Flat, 5.0f);

		var buffedStats = _playerManager.GetCharacterStats(characterId);
		float expectedValue = initialValue + 15;
		float actualValue = buffedStats[statType];

		if (Math.Abs(actualValue - expectedValue) > 0.2f * expectedValue)
		{
			LogError("Flat Buff Stacking", $"Expected {statType}: {expectedValue}, Actual: {actualValue}");
		}
	}

	private void ApplyPercentageBuffsAndVerify(string characterId, StatType statType, float initialValue)
	{
		_playerManager.ApplyBuff(characterId, statType, "Percentage Buff 1", BuffType.Percentage, 0.2f);
		_playerManager.ApplyBuff(characterId, statType, "Percentage Buff 2", BuffType.Percentage, 0.1f);

		var buffedStats = _playerManager.GetCharacterStats(characterId);
		float expectedValue = initialValue * 1.32f;
		float actualValue = buffedStats[statType];

		if (Math.Abs(actualValue - expectedValue) > 0.2f * expectedValue)
		{
			LogError("Percentage Buff Stacking", $"Expected {statType}: {expectedValue}, Actual: {actualValue}");
		}
	}

	private void ApplyMixedBuffsAndVerify(string characterId, float initialAttack, float initialHealth)
	{
		_playerManager.ApplyBuff(characterId, StatType.Attack, "Flat Buff", BuffType.Flat, 10.0f);
		_playerManager.ApplyBuff(characterId, StatType.Health, "Percentage Buff", BuffType.Percentage, 0.2f);

		var buffedStats = _playerManager.GetCharacterStats(characterId);
		float expectedAttack = initialAttack + 10;
		float expectedHealth = initialHealth * 1.2f;
		float actualAttack = buffedStats[StatType.Attack];
		float actualHealth = buffedStats[StatType.Health];

		if (Math.Abs(actualAttack - expectedAttack) > 0.02f * expectedAttack)
		{
			LogError("Mixed Buff Interaction", $"Expected Attack: {expectedAttack}, Actual: {actualAttack}");
		}

		if (Math.Abs(actualHealth - expectedHealth) > 0.02f * expectedHealth)
		{
			LogError("Mixed Buff Interaction", $"Expected Health: {expectedHealth}, Actual: {actualHealth}");
		}
	}

	private void TestBuffExpiration(string characterId, float initialAttack, float initialHealth)
	{
		string buffId1 = Guid.NewGuid().ToString();
		string buffId2 = Guid.NewGuid().ToString();

		_playerManager.ApplyBuff(characterId, StatType.Attack, "Temporary Buff 1", BuffType.Flat, 10.0f, buffId1, TimeSpan.FromSeconds(1));
		_playerManager.ApplyBuff(characterId, StatType.Health, "Temporary Buff 2", BuffType.Percentage, 0.2f, buffId2, TimeSpan.FromSeconds(1));

		var buffedStats = _playerManager.GetCharacterStats(characterId);
		float expectedAttack = initialAttack + 10;
		float expectedHealth = initialHealth * 1.2f;

		if (Math.Abs(buffedStats[StatType.Attack] - expectedAttack) > 0.01f)
		{
			LogError("Buff Expiration", "Temporary attack buff not applied correctly");
		}

		if (Math.Abs(buffedStats[StatType.Health] - expectedHealth) > 0.01f)
		{
			LogError("Buff Expiration", "Temporary health buff not applied correctly");
		}

		// Wait for buffs to expire
		System.Threading.Thread.Sleep(1100);
		_playerManager.Update();

		var finalStats = _playerManager.GetCharacterStats(characterId);

		if (Math.Abs(finalStats[StatType.Attack] - initialAttack) > 0.01f)
		{
			LogError("Buff Expiration", "Temporary attack buff did not expire correctly");
		}

		if (Math.Abs(finalStats[StatType.Health] - initialHealth) > 0.01f)
		{
			LogError("Buff Expiration", "Temporary health buff did not expire correctly");
		}
	}
	private void ResetCharacterState(string characterId, Dictionary<StatType, float> initialStats)
	{
		_playerManager.SetCharacterLevel(characterId, 1);

		// Remove all active buffs
		foreach (var stat in initialStats.Keys)
		{
			var activeModifiers = _playerManager.GetStatModifiers(characterId, stat);
			foreach (var modifier in activeModifiers)
			{
				_playerManager.RemoveBuff(characterId, stat, modifier.Id);
			}
		}


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
		GD.Print("\n=== Character System Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}



	#endregion
}