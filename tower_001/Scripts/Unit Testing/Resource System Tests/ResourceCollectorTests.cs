using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class ResourceCollectorTests : BaseTestSuite
{
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private bool _collectionEventReceived;
	private float _lastCollectedAmount;
	private Dictionary<ResourceType, float> _lastCollectionRates;

	public ResourceCollectorTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		RegisterEventHandlers();
	}

	private void RegisterEventHandlers()
	{
		_eventManager.AddHandler<ResourceEventArgs>(
			EventType.ResourceAmountChanged,
			OnResourceCollected);
	}

	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Resources_Collectors))
		{
			TestBasicCollection();
			TestMultiResourceCollection();
			TestCollectionEfficiency();
			TestTemporaryCollectionBoosts();
			TestLifetimeTracking();
			TestCollectorUnlocking();

			OutputTestResults();
		}
	}


	private void TestBasicCollection()
	{
		string testName = "Basic Collection";
		bool testPassed = true;

		try
		{
			// Create collector with single resource type
			var baseRates = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 10f } // 10 gold per second
            };

			var collector = ResourceCollectorFactory.CreateCollector("test_collector", baseRates);
			ResetEventFlags();

			if (!collector.IsUnlocked)
			{
				var sufficientValues = new Dictionary<UnlockCondition, float>{{ UnlockCondition.CharacterLevel, 5f }};
				bool unlockResult = collector.TryUnlock(sufficientValues);
			}

			// Start collection
			collector.StartCollecting();
			var collected = collector.ProcessCollection(1.0f); // Process one second

			DebugLogger.Log($"Basic collection test - Rate: 10/s, Collected: {collected[ResourceType.Gold]}",
						   DebugLogger.LogCategory.Resources);

			if (Math.Abs(collected[ResourceType.Gold] - 10f) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Incorrect collection amount. Expected 10, got {collected[ResourceType.Gold]}");
			}

			// Test collection stop
			collector.StopCollecting();
			collected = collector.ProcessCollection(1.0f);
			if (collected.Values.Sum() > 0)
			{
				testPassed = false;
				LogError(testName, "Collector continued collecting after stop");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultiResourceCollection()
	{
		string testName = "Multi-Resource Collection";
		bool testPassed = true;

		try
		{
			var baseRates = new Dictionary<ResourceType, float>
		{
			{ ResourceType.Gold, 10f },
			{ ResourceType.Pages, 5f }
		};

			var collector = ResourceCollectorFactory.CreateCollector("multi_collector", baseRates);

			if (!collector.IsUnlocked)
			{
				var sufficientValues = new Dictionary<UnlockCondition, float> { { UnlockCondition.CharacterLevel, 5f } };
				bool unlockResult = collector.TryUnlock(sufficientValues);
			}

			collector.StartCollecting();
			var collected = collector.ProcessCollection(1.0f);

			DebugLogger.Log($"Multi-resource collection test - Gold: {collected[ResourceType.Gold]}/s, " +
						   $"Pages: {collected[ResourceType.Pages]}/s",
						   DebugLogger.LogCategory.Resources);

			// Check collection rates with multi-resource bonus (30%)
			float expectedGold = 10f * 1.3f;  // Base rate * (1 + 0.3)
			float expectedPages = 5f * 1.3f;   // Base rate * (1 + 0.3)

			if (Math.Abs(collected[ResourceType.Gold] - expectedGold) > 0.01f ||
				Math.Abs(collected[ResourceType.Pages] - expectedPages) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Incorrect multi-resource collection rates. Expected Gold: {expectedGold}, Pages: {expectedPages}");
			}

			// Verify collection efficiency bonus is applied
			if (collector.CollectionEfficiencyBonuses.Values.All(v => v <= 1.0f))
			{
				testPassed = false;
				LogError(testName, "Multi-resource efficiency bonus not applied");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCollectionEfficiency()
	{
		string testName = "Collection Efficiency";
		bool testPassed = true;

		try
		{
			DebugLogger.Log("\n=== Starting Collection Efficiency Test ===",
						   DebugLogger.LogCategory.Resources);

			var baseRates = new Dictionary<ResourceType, float>
		{
			{ ResourceType.Gold, 10f },
			{ ResourceType.Pages, 5f }
		};

			var collector = ResourceCollectorFactory.CreateCollector("efficiency_collector", baseRates);

			// Log initial state
			DebugLogger.Log("\nInitial state:", DebugLogger.LogCategory.Resources);
			foreach (var rate in collector.CollectionRates)
			{
				DebugLogger.Log($"Base rate - {rate.Key}: {rate.Value}/s",
							   DebugLogger.LogCategory.Resources);
			}

			collector.StartCollecting();
			var initialCollection = collector.ProcessCollection(1.0f);

			DebugLogger.Log("\nInitial collection (with multi-resource bonus):",
						   DebugLogger.LogCategory.Resources);
			foreach (var amount in initialCollection)
			{
				DebugLogger.Log($"{amount.Key}: {amount.Value}",
							   DebugLogger.LogCategory.Resources);
			}

			// Apply external modifier
			var modifiers = new Dictionary<ResourceType, float>
			{
			{ ResourceType.Gold, 1.5f }
			};
			DebugLogger.Log($"\nApplying external modifier to Gold: 1.5x",DebugLogger.LogCategory.Resources);

			collector.RemoveTemporaryBonuse();
			collector.UpdateCollectionRates(modifiers);
			if (!collector.IsUnlocked)
			{
				var sufficientValues = new Dictionary<UnlockCondition, float> { { UnlockCondition.CharacterLevel, 5f } };
				bool unlockResult = collector.TryUnlock(sufficientValues);
			}
			collector.StartCollecting();
			var modifiedCollection = collector.ProcessCollection(1.0f);

			DebugLogger.Log($"Temporary boost test - Base rate: 10/s, " +
			   $"Boosted collection: {modifiedCollection[ResourceType.Gold]}/s",
			   DebugLogger.LogCategory.Resources);


			DebugLogger.Log("\nCollection after modifier:", DebugLogger.LogCategory.Resources);
			foreach (var amount in modifiedCollection)
			{
				DebugLogger.Log($"{amount.Key}: {amount.Value}",
							   DebugLogger.LogCategory.Resources);
			}

			// Expected: base(10) * multi-resource(1.3) * modifier(1.5) = 19.5
			float expectedGoldRate = 10f * 1.3f * 1.5f;
			if (Math.Abs(modifiedCollection[ResourceType.Gold] - expectedGoldRate) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Incorrect Gold collection rate. " +
								  $"Expected: {expectedGoldRate}, " +
								  $"Got: {modifiedCollection[ResourceType.Gold]}");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTemporaryCollectionBoosts()
	{
		string testName = "Temporary Collection Boosts";
		bool testPassed = true;

		try
		{
			var baseRates = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 10f }
			};

			var collector = ResourceCollectorFactory.CreateCollector("boost_collector", baseRates);

			if (!collector.IsUnlocked)
			{
				var sufficientValues = new Dictionary<UnlockCondition, float> { { UnlockCondition.CharacterLevel, 5f } };
				bool unlockResult = collector.TryUnlock(sufficientValues);
			}
			collector.StartCollecting();

			// Apply temporary boost
			collector.ApplyTemporaryBonus(ResourceType.Gold, 0.5f, TimeSpan.FromSeconds(1));
			var boostedCollection = collector.ProcessCollection(1.0f);

			DebugLogger.Log($"Temporary boost test - Base rate: 10/s, " +
						   $"Boosted collection: {boostedCollection[ResourceType.Gold]}/s",
						   DebugLogger.LogCategory.Resources);

			if (Math.Abs(boostedCollection[ResourceType.Gold] - 15f) > 0.01f) // 10 * (1 + 0.5)
			{
				testPassed = false;
				LogError(testName, "Temporary boost not properly applied");
			}

			// Wait for boost to expire
			System.Threading.Thread.Sleep(1100);
			var normalCollection = collector.ProcessCollection(1.0f);

			if (Math.Abs(normalCollection[ResourceType.Gold] - 10f) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Temporary boost did not expire properly");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestLifetimeTracking()
	{
		string testName = "Lifetime Collection Tracking";
		bool testPassed = true;

		try
		{
			var baseRates = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 10f }
			};

			var collector = ResourceCollectorFactory.CreateCollector("tracking_collector", baseRates);

			if (!collector.IsUnlocked)
			{
				var sufficientValues = new Dictionary<UnlockCondition, float> { { UnlockCondition.CharacterLevel, 5f } };
				bool unlockResult = collector.TryUnlock(sufficientValues);
			}

			collector.StartCollecting();

			// Collect for multiple ticks
			for (int i = 0; i < 3; i++)
			{
				collector.ProcessCollection(1.0f);
			}

			DebugLogger.Log($"Lifetime tracking test - Expected: 30, " +
						   $"Tracked: {collector.LifetimeCollected[ResourceType.Gold]}",
						   DebugLogger.LogCategory.Resources);

			if (collector.LifetimeCollected[ResourceType.Gold] != 30) // 10 * 3
			{
				testPassed = false;
				LogError(testName, "Lifetime collection tracking incorrect");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestCollectorUnlocking()
	{
		string testName = "Collector Unlocking";
		bool testPassed = true;

			try
			{
				var baseRates = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 10f }
			};
			var collector = ResourceCollectorFactory.CreateCollector("unlock_collector", baseRates);

			// Verify collector starts locked
			if (collector.IsUnlocked)
			{
				testPassed = false;
				LogError(testName, "Collector should start in locked state");
			}

			// Verify collection doesn't work while locked
			collector.StartCollecting();
			var collectedWhileLocked = collector.ProcessCollection(1.0f);
			if (collectedWhileLocked.Values.Sum() > 0)
			{
				testPassed = false;
				LogError(testName, "Locked collector should not collect resources");
			}

			DebugLogger.Log("Verified collector starts locked and cannot collect",
						   DebugLogger.LogCategory.Resources);

			// Test insufficient unlock requirements
			var insufficientValues = new Dictionary<UnlockCondition, float>
			{
			{ UnlockCondition.CharacterLevel, 3f }
			};

			bool unlockResult = collector.TryUnlock(insufficientValues);
			if (unlockResult || collector.IsUnlocked)
			{
				testPassed = false;
				LogError(testName, "Collector should not unlock with insufficient requirements");
			}

			// Test sufficient unlock requirements
			var sufficientValues = new Dictionary<UnlockCondition, float>
			{
			{ UnlockCondition.CharacterLevel, 5f }
			};

			unlockResult = collector.TryUnlock(sufficientValues);
			if (!unlockResult || !collector.IsUnlocked)
			{
				testPassed = false;
				LogError(testName, "Collector failed to unlock with sufficient requirements");
			}

			// Verify collection works after unlocking
			collector.StartCollecting();
			var collectedAfterUnlock = collector.ProcessCollection(1.0f);
			if (Math.Abs(collectedAfterUnlock[ResourceType.Gold] - 10f) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Collection not working properly after unlock");
			}

			DebugLogger.Log($"Collector unlocked and collecting at rate: {collectedAfterUnlock[ResourceType.Gold]}/s",
						   DebugLogger.LogCategory.Resources);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	#region Support Methods
	private void ResetEventFlags()
	{
		_collectionEventReceived = false;
		_lastCollectedAmount = 0;
		_lastCollectionRates = new Dictionary<ResourceType, float>();
	}

	private void OnResourceCollected(ResourceEventArgs args)
	{
		_collectionEventReceived = true;
		_lastCollectedAmount = args.Amount;
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
		GD.Print("\n=== Resource Collector Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}
	#endregion
}