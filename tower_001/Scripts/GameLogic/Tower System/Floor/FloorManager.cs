using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tower.GameLogic.Core;
using static GlobalEnums;

/// <summary>
/// Manages floors within a tower, including their generation, state transitions, and interactions.
/// Handles floor-specific events and integrates with room management.
/// </summary>
/// <summary>
/// Dependencies and Usage:
/// - Used by: TowerManager, GameManager
/// - Uses: RoomManager, EventManager
/// - Events: FloorEventArgs, ManagerErrorEventArgs
/// - Related Systems: TowerProgressionManager
/// - Scene Files: Floor.tscn
/// 
/// Manages floors within a tower, including their generation, state transitions, and interactions.
/// Handles floor-specific events and integrates with room management.
/// </summary>
public partial class FloorManager : BaseManager
{

    public override IEnumerable<Type> Dependencies => new[]
    {
        typeof(EventManager),
        typeof(TowerManager)  // Floors need Tower info
    };

    private readonly Dictionary<string, Dictionary<string, FloorData>> _floorsByTower = new();

    /// <summary>
    /// The ID of the current floor being interacted with.
    /// </summary>
    private string _currentFloorId;

    /// <summary>
    /// Number of future floors for which rooms should be pre-generated.
    /// </summary>
    private const int ROOM_GENERATION_LOOKAHEAD = 10;

    /// <summary>
    /// Number of past floors for which rooms should be kept in memory.
    /// </summary>
    private const int ROOM_GENERATION_CLEANUP = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="FloorManager"/> class.
    /// Sets up the floor manager with a specific tower ID and event manager.
    /// </summary>
    /// <param name="towerId">The unique identifier of the tower this manager is responsible for.</param>
    /// <param name="eventManager">The event manager to handle floor-related events.</param>
    public FloorManager()
    {
        base.Setup();
        _floorsByTower = new Dictionary<string, Dictionary<string, FloorData>>();
        //_roomManager = new RoomManager(_eventManager); // Initialize the room manager with the event manager
    }

    /// <summary>
    /// Registers event handlers for floor-related events.
    /// Handles state changes, floor entries, and floor completions.
    /// </summary>
    protected override void RegisterEventHandlers()
    {
        // Register a handler for when a floor's state changes
        EventManager.AddHandler<FloorEventArgs>(EventType.FloorStateChanged, HandleFloorStateChanged);

        // Register a handler for when a floor is entered
        EventManager.AddHandler<FloorEventArgs>(EventType.FloorEntered, HandleFloorEntered);

        // Register a handler for when a floor is completed
        EventManager.AddHandler<FloorEventArgs>(EventType.FloorCompleted, HandleFloorCompleted);
    }

    /// <summary>
    /// Creates a new floor for the tower and initializes its properties, difficulty, and rooms.
    /// </summary>
    /// <param name="towerId">The unique identifier of the tower the floor belongs to.</param>
    /// <param name="floorNumber">The number of the floor within the tower.</param>
    /// <param name="type">The type of the floor (e.g., normal, boss).</param>
    /// <param name="baseDifficulty">The base difficulty value for the floor.</param>
    /// <param name="towerDifficulty">The difficulty settings inherited from the tower.</param>
    /// <param name="isBossFloor">Indicates whether the floor is a boss floor.</param>
    /// <returns>True if the floor was successfully created; otherwise, false.</returns>
    public bool CreateFloor(
        string towerId,
        int floorNumber,
        FloorType type,
        float baseDifficulty,
        FloorDifficulty towerDifficulty,
        bool isBossFloor = false)
    {
        // Generate a unique ID for the floor based on the tower ID and floor number
        string floorId = $"{towerId}_floor_{floorNumber}";

        // Check if the floor already exists; if so, abort creation

        if (!_floorsByTower.ContainsKey(towerId))
        {
            _floorsByTower[towerId] = new Dictionary<string, FloorData>();
        }

        if (_floorsByTower[towerId].ContainsKey(floorId))
            return false;

        // Initialize a new FloorData object with the specified properties
        var floor = new FloorData
        {
            Id = floorId, // Assign the unique ID
            Name = $"Floor {floorNumber}", // Set a descriptive name
            Type = type, // Define the floor type (e.g., normal, boss)
            FloorNumber = floorNumber, // Assign the floor number
            CurrentState = floorNumber == 1 ? FloorState.Available : FloorState.Locked, // Unlock the first floor; lock others
            IsBossFloor = isBossFloor // Set whether the floor is a boss floor
        };

        // Inherit difficulty settings from the tower and adjust for the specific floor
        floor.Difficulty.FloorNumber = floorNumber;
        floor.Difficulty.InheritSettings(towerDifficulty);

        // Create rooms for this floor using a room generator
        var roomGenerator = new RoomGenerator(floor);
        int roomCount = CalculateRoomCount(floor); // Calculate the number of rooms for the floor

        // Generate the rooms; if generation fails, log an error and abort floor creation
        bool roomsGenerated = roomGenerator.GenerateRoomsForFloor(roomCount);
        if (!roomsGenerated)
        {
            GD.PrintErr($"Failed to generate rooms for floor {floorNumber}");
            return false;
        }

        // Add the floor to the dictionary of managed floors
        _floorsByTower[towerId][floorId] = floor;
        // Raise an event to notify that the floor has been initialized
        RaiseFloorInitializedEvent(towerId, floor);

        return true; // Indicate successful floor creation
    }

    public List<FloorData> GetFloorsForTower(string towerId)
    {
        return _floorsByTower.TryGetValue(towerId, out var floors)
            ? floors.Values.OrderBy(f => f.FloorNumber).ToList()
            : new List<FloorData>();
    }

    public bool GenerateFloorsForTower(TowerData tower, int floorCount, FloorGenerationConfig config)
    {
        if (!_floorsByTower.ContainsKey(tower.Id))
        {
            _floorsByTower[tower.Id] = new Dictionary<string, FloorData>();
        }

        var generator = new FloorGenerator(this, tower);
        bool success = generator.GenerateFloorsForTower(floorCount, config);

        if (success)
        {
            DebugLogger.Log($"Generated {floorCount} floors for tower {tower.Id}", DebugLogger.LogCategory.Room);

        }
        else
        {
            GD.PrintErr($"Failed to generate floors for tower {tower.Id}");
        }

        return success;
    }

    private bool GenerateRoomsForFloor(FloorData floor)
    {
        if (floor.Rooms?.Any() == true)
        {
            return true; // Rooms already exist
        }

        var roomGenerator = new RoomGenerator(floor);
        int roomCount = CalculateRoomCount(floor);

        bool success = roomGenerator.GenerateRoomsForFloor(roomCount);

        if (success)
        {
            DebugLogger.Log($"Generated {roomCount} rooms for floor {floor.FloorNumber}", DebugLogger.LogCategory.Room);

        }
        else
        {
            GD.PrintErr($"Failed to generate rooms for floor {floor.FloorNumber}");
        }

        return success;
    }

    /// <summary>
    /// Calculates the total number of rooms for a given floor based on its characteristics.
    /// Accounts for variability and scaling as floors progress.
    /// </summary>
    /// <param name="floor">The floor data for which the room count is calculated.</param>
    /// <returns>The calculated number of rooms for the floor.</returns>
    private int CalculateRoomCount(FloorData floor)
    {
        // Boss floors always have exactly one room, the boss room
        if (floor.IsBossFloor)
        {
            return 1;
        }

        // Base number of rooms for normal floors
        int baseRoomCount = 20;

        // Add more rooms as floor numbers increase, scaling every 10 floors
        int additionalRooms = (floor.FloorNumber / 10);

        // Add a small random variability to make room counts less predictable
        int variability = RandomManager.Instance.Next(-2, 3); // Range: -2 to +2

        // Calculate the total room count
        int totalRooms = baseRoomCount + additionalRooms + variability;

        // Ensure the total room count stays within the allowed range (1 to 100)
        return Math.Clamp(totalRooms, 1, 100);
    }

    /// <summary>
    /// Attempts to enter a floor with the given character.
    /// Verifies the floor's availability and difficulty requirements before allowing entry.
    /// </summary>
    /// <param name="floorId">The unique identifier of the floor to enter.</param>
    /// <param name="character">The character attempting to enter the floor.</param>
    /// <returns>
    /// True if the character successfully enters the floor; otherwise, false.
    /// </returns>
    public bool TryEnterFloor(TowerData tower, string floorId, Character character)
    {
        // Ensure the specified floor exists in the system
        if (!_floorsByTower.TryGetValue(tower.Id, out var floors) ||
                !floors.TryGetValue(floorId, out var floor))
            return false;

        // Generate rooms for future floors to maintain a smooth gameplay experience
        EnsureUpcomingRoomsGenerated(tower.Id, floor.FloorNumber);

        // Clean up rooms from distant past floors to free up memory
        CleanupDistantFloorRooms(tower.Id, floor.FloorNumber);

        // Check if the floor is available to enter
        if (floor.CurrentState != FloorState.Available)
        {
            GD.PrintErr($"Floor {floorId} is not available (Current State: {floor.CurrentState})");
            return false;
        }

        // Calculate the difficulty of the floor based on the character's level and element
        float difficulty = floor.Difficulty.CalculateFinalDifficulty(
            character.Level,
            character.Element
        );

        // Check if the character's power meets the floor's difficulty requirement
        if (character.Power < difficulty)
        {
            // Notify the system of the failure due to difficulty mismatch
            EventManager?.RaiseEvent(EventType.FloorStateChanged,
                new FloorEventArgs(
                    tower.Id,
                    floorId,
                    floor.FloorNumber,
                    floor.CurrentState,
                    FloorState.Failed,
                    difficulty,
                    "Failed to meet difficulty requirement"
                ));
            return false;
        }

        // Update the floor's state to "In Progress" and set it as the current floor
        var previousState = floor.CurrentState;
        floor.CurrentState = FloorState.InProgress;
        _currentFloorId = floorId;

        // Raise an event to notify the system that the floor has been entered
        EventManager?.RaiseEvent(EventType.FloorEntered,
            new FloorEventArgs(
                tower.Id,
                floorId,
                floor.FloorNumber,
                previousState,
                FloorState.InProgress,
                difficulty,
                "Floor entered"
            ));

        return true;
    }

    /// Ensures upcoming floors have rooms generated
    /// </summary>
    private void EnsureUpcomingRoomsGenerated(string towerId, int currentFloorNumber)
    {
        if (!_floorsByTower.TryGetValue(towerId, out var floors))
            return;

        int maxFloorToGenerate = currentFloorNumber + ROOM_GENERATION_LOOKAHEAD;

        foreach (var floor in floors.Values
            .Where(f => f.FloorNumber > currentFloorNumber &&
                       f.FloorNumber <= maxFloorToGenerate &&
                       (f.Rooms == null || !f.Rooms.Any())))
        {
            GenerateRoomsForFloor(floor);
        }
    }

    /// <summary>
    /// Ensures that rooms are generated for floors ahead of the current floor within the lookahead range.
    /// Helps maintain seamless gameplay by pre-generating rooms for upcoming floors.
    /// </summary>
    /// /// <param name="towerId">The tower to clean up.</param>
    /// <param name="currentFloorNumber">The number of the current floor.</param>
    /// Cleans up rooms from distant past floors
    /// </summary>
    private void CleanupDistantFloorRooms(string towerId, int currentFloorNumber)
    {
        if (!_floorsByTower.TryGetValue(towerId, out var floors))
            return;

        int minFloorToKeep = currentFloorNumber - ROOM_GENERATION_CLEANUP;

        foreach (var floor in floors.Values
            .Where(f => f.FloorNumber < minFloorToKeep &&
                       f.Rooms?.Any() == true))
        {
            //floor.Rooms.Clear();
            DebugLogger.Log($"Cleared rooms from floor {floor.FloorNumber} to save memory",
                DebugLogger.LogCategory.Floor);
        }
    }

    public bool CompleteFloor(TowerData tower, string floorId)
    {
        // Ensure the specified floor exists in the system
        if (!_floorsByTower.TryGetValue(tower.Id, out var floors) ||
                !floors.TryGetValue(floorId, out var floor))
            return false;

        var previousState = floor.CurrentState;
        floor.CurrentState = FloorState.Completed;
        _currentFloorId = null;

        // Unlock next floor if it exists
        var nextFloorId = tower.Id + "_floor_" + (floor.FloorNumber + 1).ToString();
        if (_floorsByTower.TryGetValue(tower.Id, out floors) && floors.TryGetValue(nextFloorId, out var nextFloor))
        {
            var nextPreviousState = nextFloor.CurrentState;
            nextFloor.CurrentState = FloorState.Available;
            EventManager?.RaiseEvent(EventType.FloorStateChanged,
                new FloorEventArgs(
                    tower.Id,
                    nextFloorId,
                    nextFloor.FloorNumber,
                    nextPreviousState,
                    FloorState.Available,
                    null,
                    "Floor unlocked"
                ));
        }

        EventManager?.RaiseEvent(EventType.FloorCompleted,
            new FloorEventArgs(
                tower.Id,
                floorId,
                floor.FloorNumber,
                previousState,
                FloorState.Completed,
                null,
                "Floor completed"
            ));

        return true;
    }

    // Fix for GetFloorByRoom method (CS0103)
    public (FloorData floor, int floorNumber) GetFloorByRoom(string towerId, string roomId)
    {
        if (_floorsByTower.TryGetValue(towerId, out var floors))
        {
            foreach (var floor in floors.Values)
            {
                if (floor.Rooms?.Any(r => r.Id == roomId) == true)
                {
                    return (floor, floor.FloorNumber);
                }
            }
        }
        return (null, 0);
    }

    public bool FailFloor(TowerData tower, string floorId)
    {
        // Ensure the specified floor exists in the system
        if (!_floorsByTower.TryGetValue(tower.Id, out var floors) ||
                !floors.TryGetValue(floorId, out var floor))
            return false;

        var previousState = floor.CurrentState;
        floor.CurrentState = FloorState.Failed;
        _currentFloorId = null;

        EventManager?.RaiseEvent(EventType.FloorStateChanged, new FloorEventArgs(tower.Id, floorId, floor.FloorNumber, previousState, FloorState.Failed, null, "Floor failed"));

        return true;
    }

    public float GetFloorDifficulty(TowerData tower, string floorId, Character character)
    {
        // Ensure the specified floor exists in the system
        if (!_floorsByTower.TryGetValue(tower.Id, out var floors) || !floors.TryGetValue(floorId, out var floor))
            return 0f;
        //if (!_floors.TryGetValue(floorId, out var floor))
        //	return 0f;

        return floor.Difficulty.CalculateFinalDifficulty(
            character.Level,
            character.Element
        );
    }

    public FloorData GetFloor(TowerData tower, string floorId)
    {
        if (_floorsByTower.TryGetValue(tower.Id, out var floors) &&
            floors.TryGetValue(floorId, out var floor))
        {
            return floor;
        }
        return null;
    }

    // Fix for GetNextFloorId method (CS0103)
    private string GetNextFloorId(string currentFloorId)
    {
        string towerId = currentFloorId.Split('_')[0];
        if (!_floorsByTower.TryGetValue(towerId, out var floors) ||
            !floors.TryGetValue(currentFloorId, out var currentFloor))
        {
            return null;
        }

        string nextFloorId = $"{towerId}_floor_{currentFloor.FloorNumber + 1}";
        return floors.ContainsKey(nextFloorId) ? nextFloorId : null;
    }

    public bool ResetFloorState(TowerData tower, string floorId)
    {
        if (!_floorsByTower.TryGetValue(tower.Id, out var floors) ||
            !floors.TryGetValue(floorId, out var floor))
            return false;

        var previousState = floor.CurrentState;
        floor.CurrentState = floor.FloorNumber == 1 ? FloorState.Available : FloorState.Locked;
        _currentFloorId = null;

        EventManager?.RaiseEvent(EventType.FloorStateChanged, new FloorEventArgs(tower.Id, floorId, floor.FloorNumber, previousState, floor.CurrentState));

        // Floor reset logic...
        return true;
    }

    private void RaiseFloorInitializedEvent(string towerId, FloorData floor)
    {
        EventManager.RaiseEvent(
            EventType.FloorInitialized,
            new FloorEventArgs(
                towerId,
                floor.Id,
                floor.FloorNumber,
                FloorState.Locked,
                floor.CurrentState,
                floor.Difficulty.BaseValue,
                $"Floor {floor.FloorNumber} initialized with {floor.Rooms?.Count ?? 0} rooms"
            )
        );
    }

    private void HandleFloorStateChanged(FloorEventArgs args)
    {
        if (_floorsByTower.TryGetValue(args.TowerId, out var floors) &&
            floors.TryGetValue(args.FloorId, out var floor))
        {
            floor.CurrentState = args.NewState;
        }
    }

    public IEnumerable<FloorData> GetAllFloors()
    {
        return _floorsByTower.Values
            .SelectMany(floors => floors.Values)
            .OrderBy(f => f.FloorNumber);
    }

    private void HandleFloorEntered(FloorEventArgs args)
    {
        _currentFloorId = args.FloorId;
    }

    private void HandleFloorCompleted(FloorEventArgs args)
    {
        if (_floorsByTower.TryGetValue(args.TowerId, out var floors) &&
            floors.TryGetValue(args.FloorId, out var floor))
        {
            floor.CurrentState = FloorState.Completed;
        }
    }

    public IEnumerable<FloorData> GetFloorsByState(FloorState state)
    {
        return _floorsByTower.Values
            .SelectMany(floors => floors.Values)
            .Where(f => f.CurrentState == state);
    }

    public string GetCurrentFloorId() => _currentFloorId;
}
