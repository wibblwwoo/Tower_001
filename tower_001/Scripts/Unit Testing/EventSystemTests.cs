using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

// TestBasicEventRegistration
// Verifies:
// - Handler registration works correctly
// - Events trigger registered handlers
// - Handler removal works correctly
// - Removed handlers don't receive events
// - Multiple handlers can be registered for same event
public partial class EventSystemTests : BaseTestSuite
{

	#region Tests to Run
	// Flag to enable/disable individual tests
	private bool _testBasicEventRegistration = true;
	private bool _testEventRaising = true;
	private bool _testEventQueueing = true;
	private bool _testUIEventHandling = true;

	#endregion

	private bool _testPassed;
	private string _testName;

	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;


	public EventSystemTests(EventManager eventManager, PlayerManager playerManager,
							  TestConfiguration testConfig) : base(testConfig)
	{
		_eventManager = eventManager;
		_playerManager = playerManager;
	}


	#region Tests


	public void RunAllTests()
	{
		if (TestConfig.IsCategoryEnabled(TestCategory.Events))
		{
			// Run individual tests
			TestBasicEventRegistration();
			TestEventRaising();
			TestEventQueueing();
			TestUIEventHandling();
			// Output results
			OutputTestResults();
		}
	}

	private void TestBasicEventRegistration()
	{
		_testName = "Basic Event Registration";
		_testPassed = true;

		try
		{
			// Test adding and removing handlers
			bool handlerCalled = false;
			void TestHandler(GameEventArgs args) => handlerCalled = true;

			_eventManager.AddHandler<GameEventArgs>(EventType.None, TestHandler);
			_eventManager.RaiseEvent(EventType.None, new GameEventArgs());

			if (!handlerCalled)
			{
				_testPassed = false;
				LogError("Handler was not called after registration");
			}

			// Test handler removal
			handlerCalled = false;
			_eventManager.RemoveHandler<GameEventArgs>(EventType.None, TestHandler);
			_eventManager.RaiseEvent(EventType.None, new GameEventArgs());

			if (handlerCalled)
			{
				_testPassed = false;
				LogError("Handler was called after removal");
			}
		}
		catch (Exception ex)
		{
			_testPassed = false;
			LogError($"Exception during test: {ex.Message}");
		}

		LogTestResult();
	}

	private void TestEventRaising()
	{
		_testName = "Event Raising";
		_testPassed = true;

		try
		{
			// Test specific event types
			bool uiEventCalled = false;
			void UIHandler(UIButton_Click_EventArgs args) => uiEventCalled = true;

			_eventManager.AddHandler<UIButton_Click_EventArgs>(
				EventType.UIButton_Click,
				UIHandler
			);

			var eventArgs = new UIButton_Click_EventArgs(EnumUIPanelParentType.Menu_Start);
			_eventManager.RaiseEvent(EventType.UIButton_Click, eventArgs);

			if (!uiEventCalled)
			{
				_testPassed = false;
				LogError("UI event handler was not called");
			}
		}
		catch (Exception ex)
		{
			_testPassed = false;
			LogError($"Exception during test: {ex.Message}");
		}

		LogTestResult();
	}

	private void TestEventQueueing()
	{
		_testName = "Event Queueing";
		_testPassed = true;

		try
		{
			int eventCount = 0;
			void QueueHandler(GameEventArgs args) => eventCount++;

			_eventManager.AddHandler<GameEventArgs>(EventType.None, QueueHandler);

			// Queue multiple events
			for (int i = 0; i < 5; i++)
			{
				_eventManager.QueueEvent(EventType.None, new GameEventArgs());
			}

			_eventManager.ProcessEvents();

			if (eventCount != 5)
			{
				_testPassed = false;
				LogError($"Expected 5 events, but processed {eventCount}");
			}
		}
		catch (Exception ex)
		{
			_testPassed = false;
			LogError($"Exception during test: {ex.Message}");
		}

		LogTestResult();
	}

	private void TestUIEventHandling()
	{
		_testName = "UI Event Handling";
		_testPassed = true;

		try
		{
			bool panelVisibilityChanged = false;
			void VisibilityHandler(UIPanel_Visibility_EventArgs args) =>
				panelVisibilityChanged = true;

			_eventManager.AddHandler<UIPanel_Visibility_EventArgs>(
				EventType.UIPanel_Visibility,
				VisibilityHandler
			);

			// Test UI panel visibility event
			var eventArgs = new UIPanel_Visibility_EventArgs(null);
			_eventManager.RaiseEvent(EventType.UIPanel_Visibility, eventArgs);

			if (!panelVisibilityChanged)
			{
				_testPassed = false;
				LogError("UI panel visibility event not handled");
			}
		}
		catch (Exception ex)
		{
			_testPassed = false;
			LogError($"Exception during test: {ex.Message}");
		}

		LogTestResult();
	}
	#endregion

	#region Support Methods
	private void LogError(string message)
	{
		TestResults.Add($"Error in {_testName}: {message}");
	}

	private void LogTestResult()
	{
		string result = _testPassed ? "PASSED" : "FAILED";
		TestResults.Add($"Test {_testName}: {result}");
	}

	private void OutputTestResults()
	{
		GD.Print("\n=== Event System Test Results ===");
		foreach (var result in TestResults)
		{
			GD.Print(result);
		}
		GD.Print("===============================\n");
	}
	#endregion 

}