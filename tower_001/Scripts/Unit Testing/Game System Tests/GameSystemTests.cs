using Godot;
using System;
using static DebugLogger;
using static GlobalEnums;
using static System.Net.Mime.MediaTypeNames;
using Tower_001.Scripts.GameLogic.StatSystem.Tests;

public partial class GameSystemTests : Node
{
	private readonly TestConfiguration _testConfig;
	private EventManager _eventManager;
	private PlayerManager _playerManager;
	private CharacterSystemTests _characterSystemTests;
	private BaseLevelingSystemTests _baseLevelingSystemTests;
	private PrestigeSystemTests _prestigeSystemTests;
	private AscensionSystemTests _ascensionSystemTests;
	private TowerProgressionChainTests _towerProgressionChainTests;

	private TowerSystemTests _towerSystemTests;
	private FloorSystemTests _floorSystemTests;
	private RoomSystemTests _roomSystemTests;

	private ResourceStorageTests _resourceStorageTests;
	private ResourceCollectorTests _resourceCollectorTests;
	private LargeScaleProgressionTestsv2 _largeScaleProgressionTestsv2;
	private TowerCompletionPersistenceTest _towerCompletionPersistenceTest;

	private EventSystemTests _eventSystemTests;

	[Export]
	public TestCategory Category { get; set; }

	[Export]
	public LogCategory Logging_Category { get; set; }

	public GameSystemTests()
	{
		_testConfig = new TestConfiguration();

		// Initialize test suites with configuration
		//	_characterTests = new CharacterSystemTests(_eventManager, _playerManager, _testConfig);

		//_prestigeTests = new PrestigeSystemTests(_eventManager, _playerManager, _testConfig);
		// ... initialize other test suites
	}

	public void Setup()
	{
		_eventManager = Globals.Instance.gameMangers.Events;
		_playerManager = Globals.Instance.gameMangers.Player;

		// Convert export flags to TestType enum

		_characterSystemTests = new CharacterSystemTests(_eventManager, _playerManager, _testConfig);
		_baseLevelingSystemTests = new BaseLevelingSystemTests(_eventManager, _playerManager, _testConfig);
		_prestigeSystemTests = new PrestigeSystemTests(_eventManager, _playerManager, _testConfig);
		_ascensionSystemTests = new AscensionSystemTests(_eventManager, _playerManager, _testConfig);
		_towerSystemTests = new TowerSystemTests(_eventManager, _playerManager, _testConfig);
		_floorSystemTests = new FloorSystemTests(_eventManager, _playerManager, _testConfig);
		_roomSystemTests = new RoomSystemTests(_eventManager, _playerManager, _testConfig);

		_eventSystemTests = new EventSystemTests(_eventManager, _playerManager, _testConfig);

		_resourceStorageTests = new ResourceStorageTests(_eventManager, _playerManager, _testConfig);
		_resourceCollectorTests = new ResourceCollectorTests(_eventManager, _playerManager, _testConfig);
		_largeScaleProgressionTestsv2 = new LargeScaleProgressionTestsv2(_eventManager, _playerManager, _testConfig);
		
		_towerProgressionChainTests = new TowerProgressionChainTests(_eventManager, _playerManager, _testConfig);
		_towerCompletionPersistenceTest = new TowerCompletionPersistenceTest(_eventManager, _playerManager, _testConfig);

		// Configure which tests to run
		foreach (TestCategory _Category in Enum.GetValues(typeof(TestCategory)))
		{
			if ((Category & _Category) != 0) {
				_testConfig.SetCategory(_Category, true);
			}
		}

		DebugLogger.SetLogging(true);
		DebugLogger.SetLogCategories(Logging_Category);
		
		RunAllTests();
	}

	public void RunAllTests()
	{
		//Example of running a specific category with only certain test types
        GD.Print("\n=== Running Stat System Validation ===");
        try
        {
            var validator = new StatSystemValidator("test_character_001");
            bool passed = validator.RunFullValidation();
            if (!passed)
            {
                GD.PrintErr("Stat System Validation failed! See above for details.");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Stat System Validation failed with error: {e.Message}");
        }

		_characterSystemTests.RunAllTests();
		_baseLevelingSystemTests.RunAllTests();
		_prestigeSystemTests.RunAllTests();
		_ascensionSystemTests.RunAllTests();
		_towerSystemTests.RunAllTests();
		_floorSystemTests.RunAllTests();
		_roomSystemTests.RunAllTests();
		_eventSystemTests.RunAllTests();
		_resourceStorageTests.RunAllTests();
		_resourceCollectorTests.RunAllTests();	
		_towerProgressionChainTests.RunAllTests();
		_largeScaleProgressionTestsv2.RunAllTests();
		_towerCompletionPersistenceTest.RunAllTests();
	}
}