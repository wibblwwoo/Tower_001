using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public partial class ResourceStorageTests : BaseTestSuite
{
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;
	private bool _overflowEventReceived;
	private bool _capacityChangeEventReceived;
	private float _lastOverflowAmount;

	public ResourceStorageTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
		RegisterEventHandlers();
	}

	private void RegisterEventHandlers()
	{
		_eventManager.AddHandler<ResourceStorageEventArgs>(
			EventType.ResourceAmountChanged,
			OnStorageOverflow);

		_eventManager.AddHandler<StorageCapacityChangedEventArgs>(
			EventType.ResourceAmountChanged,
			OnStorageCapacityChanged);
	}

	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Resources))
		{
			TestBasicStorageCapacity();
			TestResourceOverflow();
			TestTemporaryStorageBoosts();
			TestMultiResourceStorage();
			TestStorageRatios();
			TestStorageEvents();

			OutputTestResults();
		}
	}

	private void TestBasicStorageCapacity()
	{
		string testName = "Basic Storage Capacity";
		bool testPassed = true;

		try
		{
			var storage = ResourceStorageFactory.CreateBasicStorage(1000f);

			// Test adding resources within capacity
			if (!storage.AddResource(ResourceType.Gold, 500f))
			{
				testPassed = false;
				LogError(testName, "Failed to add resources within capacity");
			}

			DebugLogger.Log($"Added 500 Gold to storage with capacity 1000", DebugLogger.LogCategory.Resources);

			// Verify stored amount
			if (Math.Abs(storage.StoredAmounts[ResourceType.Gold] - 500f) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Incorrect stored amount. Expected 500, got {storage.StoredAmounts[ResourceType.Gold]}");
			}

			// Test removing resources
			if (!storage.RemoveResource(ResourceType.Gold, 200f))
			{
				testPassed = false;
				LogError(testName, "Failed to remove resources");
			}

			DebugLogger.Log($"Removed 200 Gold, remaining: {storage.StoredAmounts[ResourceType.Gold]}",
						   DebugLogger.LogCategory.Resources);
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestResourceOverflow()
	{
		string testName = "Resource Overflow";
		bool testPassed = true;

		try
		{
			var storage = ResourceStorageFactory.CreateBasicStorage(1000f);
			ResetEventFlags();

			// Test adding resources beyond capacity but within overflow
			float overflowAmount = 1050f; // 5% over capacity
			storage.AddResource(ResourceType.Gold, overflowAmount);

			if (!_overflowEventReceived)
			{
				testPassed = false;
				LogError(testName, "Overflow event not received");
			}

			DebugLogger.Log($"Added {overflowAmount} Gold to storage with capacity 1000",
						   DebugLogger.LogCategory.Resources);
			DebugLogger.Log($"Overflow amount: {_lastOverflowAmount}",
						   DebugLogger.LogCategory.Resources);

			// Test adding resources beyond overflow
			storage.AddResource(ResourceType.Gold, 2000f);
			if (storage.StoredAmounts[ResourceType.Gold] > storage.OverflowCapacity)
			{
				testPassed = false;
				LogError(testName, "Storage exceeded overflow capacity");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestTemporaryStorageBoosts()
	{
		string testName = "Temporary Storage Boosts";
		bool testPassed = true;

		try
		{
			var storage = ResourceStorageFactory.CreateBasicStorage(1000f);
			ResetEventFlags();

			// Apply temporary boost
			storage.ApplyTemporaryCapacityBoost(0.5f, TimeSpan.FromSeconds(1));

			if (!_capacityChangeEventReceived)
			{
				testPassed = false;
				LogError(testName, "Capacity change event not received");
			}

			float expectedCapacity = 1500f;
			if (Math.Abs(storage.CurrentCapacity - expectedCapacity) > 0.01f)
			{
				testPassed = false;
				LogError(testName, $"Incorrect boosted capacity. Expected {expectedCapacity}, got {storage.CurrentCapacity}");
			}

			DebugLogger.Log($"Applied 50% capacity boost. New capacity: {storage.CurrentCapacity}",
						   DebugLogger.LogCategory.Resources);

			// Wait for boost to expire
			System.Threading.Thread.Sleep(1100);
			storage.Update();

			if (Math.Abs(storage.CurrentCapacity - 1000f) > 0.01f)
			{
				testPassed = false;
				LogError(testName, "Boost did not expire properly");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestMultiResourceStorage()
	{
		string testName = "Multi-Resource Storage";
		bool testPassed = true;

		try
		{
			var ratios = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 0.6f },
				{ ResourceType.Pages, 0.4f }
			};

			var storage = ResourceStorageFactory.CreateCustomStorage(1000f, ratios);

			// Test adding different resources
			storage.AddResource(ResourceType.Gold, 500f);
			storage.AddResource(ResourceType.Pages, 350f);

			DebugLogger.Log($"Added resources to split storage - Gold: {storage.StoredAmounts[ResourceType.Gold]}, " +
						   $"Pages: {storage.StoredAmounts[ResourceType.Pages]}",
						   DebugLogger.LogCategory.Resources);

			// Verify capacity splits
			if (storage.StoredAmounts[ResourceType.Gold] > 600f)
			{
				testPassed = false;
				LogError(testName, "Gold storage exceeded its ratio capacity");
			}

			if (storage.StoredAmounts[ResourceType.Pages] > 400f)
			{
				testPassed = false;
				LogError(testName, "Pages storage exceeded its ratio capacity");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestStorageRatios()
	{
		string testName = "Storage Ratios";
		bool testPassed = true;

		try
		{
			var ratios = new Dictionary<ResourceType, float>
			{
				{ ResourceType.Gold, 0.7f },
				{ ResourceType.Pages, 0.3f }
			};

			var storage = ResourceStorageFactory.CreateCustomStorage(1000f, ratios);

			// Test ratio enforcement
			float goldCapacity = 700f;
			float pagesCapacity = 300f;

			storage.AddResource(ResourceType.Gold, 800f);
			storage.AddResource(ResourceType.Pages, 400f);

			DebugLogger.Log($"Testing storage ratios - Gold: {storage.StoredAmounts[ResourceType.Gold]}/{goldCapacity}, " +
						   $"Pages: {storage.StoredAmounts[ResourceType.Pages]}/{pagesCapacity}",
						   DebugLogger.LogCategory.Resources);

			if (storage.StoredAmounts[ResourceType.Gold] > goldCapacity * 1.1f || // Including overflow
				storage.StoredAmounts[ResourceType.Pages] > pagesCapacity * 1.1f)
			{
				testPassed = false;
				LogError(testName, "Storage ratios not properly enforced");
			}
		}
		catch (Exception ex)
		{
			testPassed = false;
			LogError(testName, $"Exception: {ex.Message}");
		}

		LogTestResult(testName, testPassed);
	}

	private void TestStorageEvents()
	{
		string testName = "Storage Events";
		bool testPassed = true;

		try
		{
			var storage = ResourceStorageFactory.CreateBasicStorage(1000f);
			ResetEventFlags();

			// Test overflow event
			storage.AddResource(ResourceType.Gold, 1100f);
			if (!_overflowEventReceived)
			{
				testPassed = false;
				LogError(testName, "Overflow event not received");
			}

			// Test capacity change event
			ResetEventFlags();
			storage.ApplyTemporaryCapacityBoost(0.2f, TimeSpan.FromSeconds(1));
			if (!_capacityChangeEventReceived)
			{
				testPassed = false;
				LogError(testName, "Capacity change event not received");
			}

			DebugLogger.Log("Storage events test completed", DebugLogger.LogCategory.Resources);
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
		_overflowEventReceived = false;
		_capacityChangeEventReceived = false;
		_lastOverflowAmount = 0;
	}

	private void OnStorageOverflow(ResourceStorageEventArgs args)
	{
		_overflowEventReceived = true;
		_lastOverflowAmount = args.OverflowAmount;
	}

	private void OnStorageCapacityChanged(StorageCapacityChangedEventArgs args)
	{
		_capacityChangeEventReceived = true;
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
		GD.Print("\n=== Resource Storage Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}
	#endregion
}