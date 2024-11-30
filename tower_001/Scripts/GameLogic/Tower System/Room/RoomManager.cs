using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
/// <summary>
/// Manages room instances and their lifecycle within a floor.
/// Handles room generation, state transitions, and difficulty scaling.
/// Integrates with the event system for notifying about room events.
/// </summary>
public partial class RoomManager : BaseManager
{
    // Fields
    private readonly RoomConfiguration _config; // Configuration settings for room generation and difficulty
    private readonly Random _random; // Random number generator for selecting room types and room count

    /// <summary>
    /// Constructor for the RoomManager class, initializing the event manager, configuration, and random generator.
    /// </summary>
    /// <param name="eventManager">The event manager to handle room events.</param>
    public RoomManager()
    {
        _config = new RoomConfiguration(); // Initializes the configuration for room generation
        _random = new Random(); // Initializes the random number generator
    }
    /// <summary>
    /// Registers event handlers for room-related events.
    /// </summary>
    protected override void RegisterEventHandlers()
    {

        // Registers handlers for different room events
        EventManager.AddHandler<RoomEventArgs>(EventType.RoomEntered, OnRoomEntered);
        EventManager.AddHandler<RoomEventArgs>(EventType.RoomExited, OnRoomExited);
        EventManager.AddHandler<RoomStateChangedArgs>(EventType.RoomStateChanged, OnRoomStateChanged);
        EventManager.AddHandler<RoomCompletionEventArgs>(EventType.RoomEventCompleted, OnRoomCompleted);
        EventManager.AddHandler<RoomCompletionEventArgs>(EventType.RoomEventFailed, OnRoomFailed);
    }


    /// <summary>
    /// Generates a list of rooms for a given floor based on floor properties and configuration.
    /// Handles the generation of either a normal floor or a boss-only floor.
    /// </summary>
    /// <param name="floor">The floor data for which rooms are being generated.</param>
    /// <returns>A list of generated room data.</returns>
    public List<RoomData> GenerateRooms(FloorData floor)
    {
        // If the floor number is divisible by the BossFloorInterval, generate a boss-only floor
        if (floor.FloorNumber % _config.BossFloorInterval == 0)
        {
            return GenerateBossOnlyFloor(floor); // Generate a boss-only floor
        }

        return GenerateNormalFloor(floor); // Generate a normal floor with random rooms
    }

    /// <summary>
    /// Generates a floor with a boss-only room. This is called for floors that require a boss.
    /// </summary>
    /// <param name="floor">The floor data to generate a boss room for.</param>
    /// <returns>A list containing the generated boss room data.</returns>
    private List<RoomData> GenerateBossOnlyFloor(FloorData floor)
    {
        // Create a boss room with custom difficulty
        var room = CreateRoom(
            RoomType.Boss,
            0, // First room in the sequence
            floor.FloorNumber,
            floor.Difficulty.BaseValue * _config.BossRoomDifficultyMultiplier
        );
        room.BossId = Guid.NewGuid().ToString(); // Assign a new BossId

        // Raise an event to notify that a room has been discovered
        RaiseRoomDiscoveredEvent(
            floor.Id, // towerId from floor data
            floor.Name, // towerName from floor data
            floor.FloorNumber,
            floor.Id,
            room.Id,
            room.Type,
            room.State
        );

        return new List<RoomData> { room }; // Return the generated boss room
    }

    /// <summary>
    /// Generates a normal floor with multiple rooms. This method creates a set of rooms based on the floor�s properties.
    /// </summary>
    /// <param name="floor">The floor data for generating normal rooms.</param>
    /// <returns>A list of generated room data.</returns>
    private List<RoomData> GenerateNormalFloor(FloorData floor)
    {
        // Randomly decide how many rooms to generate for this floor
        int roomCount = _random.Next(_config.MinRoomsPerFloor, _config.MaxRoomsPerFloor + 1);
        var rooms = new List<RoomData>();
        var lastRoomPositions = new Dictionary<RoomType, int>(); // Keeps track of the last position of each room type

        // Generate all rooms except the last one
        for (int i = 0; i < roomCount - 1; i++)
        {
            var roomType = SelectRoomType(i, lastRoomPositions, roomCount); // Select a room type based on the current position
            var room = CreateRoom(
                roomType,
                i,
                floor.FloorNumber,
                CalculateRoomDifficulty(roomType, i, floor.FloorNumber)
            );

            rooms.Add(room); // Add the generated room to the list
            lastRoomPositions[roomType] = i; // Store the last position of this room type

            // Raise an event to notify that a room has been discovered
            RaiseRoomDiscoveredEvent(
                floor.Id,
                floor.Name,
                floor.FloorNumber,
                floor.Id,
                room.Id,
                room.Type,
                room.State
            );
        }

        // Generate the last room, which is either a Boss or Reward room
        var finalRoomType = _random.Next(2) == 0 ? RoomType.Boss : RoomType.Reward;
        var finalRoom = CreateRoom(
            finalRoomType,
            roomCount - 1,
            floor.FloorNumber,
            CalculateRoomDifficulty(finalRoomType, roomCount - 1, floor.FloorNumber)
        );

        if (finalRoomType == RoomType.FloorBoss)
        {
            finalRoom.BossId = Guid.NewGuid().ToString(); // Assign BossId if it�s a boss room
        }

        rooms.Add(finalRoom); // Add the final room to the list

        // Raise an event for the final room discovered
        RaiseRoomDiscoveredEvent(
            floor.Id,
            floor.Name,
            floor.FloorNumber,
            floor.Id,
            finalRoom.Id,
            finalRoom.Type,
            finalRoom.State
        );

        return rooms; // Return the list of generated rooms
    }
    /// <summary>
    /// Creates a new room with specified parameters.
    /// </summary>
    /// <param name="type">The type of room (e.g., Combat, Boss, Reward, etc.).</param>
    /// <param name="position">The position of the room within the floor.</param>
    /// <param name="floorNumber">The number of the floor where the room is located.</param>
    /// <param name="difficulty">The difficulty of the room based on floor difficulty and room type.</param>
    /// <returns>A new RoomData object representing the room.</returns>
    private RoomData CreateRoom(RoomType type, int position, int floorNumber, float difficulty)
    {
        return new RoomData
        {
            Id = Guid.NewGuid().ToString(), // Generate a unique ID for the room
            Name = $"Room {position + 1}", // Set room name based on position
            Type = type, // Set the room type (Combat, Boss, Reward, etc.)
            Position = new Vector2(position, 0), // Set the position of the room
            BaseDifficulty = difficulty, // Set the room�s difficulty
            IsRequired = true, // Indicate that this room is required in the floor layout
            State = RoomState.Locked, // Initially the room is locked
            ElementType = ElementType.None, // No specific element type for the room
            EncounterLevel = floorNumber // Set the encounter level based on the floor number
        };
    }

    /// <summary>
    /// Raises the "RoomDiscovered" event to notify the system about a newly discovered room.
    /// </summary>
    /// <param name="towerId">The ID of the tower where the room is located.</param>
    /// <param name="towerName">The name of the tower.</param>
    /// <param name="floorNumber">The number of the floor containing the room.</param>
    /// <param name="floorId">The ID of the floor containing the room.</param>
    /// <param name="roomId">The ID of the room that has been discovered.</param>
    /// <param name="type">The type of the room (e.g., Boss, Combat, etc.).</param>
    /// <param name="state">The initial state of the room (e.g., Locked, Completed, etc.).</param>
    private void RaiseRoomDiscoveredEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state)
    {
        var args = new RoomEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state);
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomDiscovered, args); // Raise the event to notify the system
    }

    /// <summary>
    /// Raises the "RoomStateChanged" event to notify the system of a change in the room's state.
    /// This event is triggered when a room's state changes (e.g., from "Locked" to "In Progress").
    /// </summary>
    /// <param name="towerId">The ID of the tower containing the room.</param>
    /// <param name="towerName">The name of the tower containing the room.</param>
    /// <param name="floorNumber">The number of the floor containing the room.</param>
    /// <param name="floorId">The ID of the floor containing the room.</param>
    /// <param name="roomId">The ID of the room whose state is changing.</param>
    /// <param name="oldState">The previous state of the room before the change.</param>
    /// <param name="newState">The new state of the room after the change.</param>
    private void RaiseRoomStateChangedEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomState oldState, RoomState newState)
    {
        // Create event arguments to pass to the event handler
        var args = new RoomStateChangedArgs(towerId, towerName, floorNumber, floorId, roomId, oldState, newState);

        // Raise the "RoomStateChanged" event with the provided arguments
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomStateChanged, args);
    }
    /// <summary>
    /// Raises the "RoomEventCompleted" event to notify the system that a room event has completed.
    /// This version does not include the completion time, which defaults to zero.
    /// </summary>
    /// <param name="towerId">The ID of the tower containing the room.</param>
    /// <param name="towerName">The name of the tower containing the room.</param>
    /// <param name="floorNumber">The number of the floor containing the room.</param>
    /// <param name="floorId">The ID of the floor containing the room.</param>
    /// <param name="roomId">The ID of the room being completed.</param>
    /// <param name="success">Indicates whether the room event was completed successfully.</param>
    private void RaiseRoomCompletionEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, bool success)
    {
        // Since the completion time is not provided, set it to zero
        var completionTime = TimeSpan.Zero; // This would normally be calculated based on room start/end time

        // Create event arguments with the required information
        var args = new RoomCompletionEventArgs(towerId, towerName, floorNumber, floorId, roomId, success, completionTime);

        // Raise the "RoomEventCompleted" event with the provided arguments
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomEventCompleted, args);
    }
    /// <summary>
    /// Raises the "RoomEventCompleted" event to notify the system that a room event has completed.
    /// This version includes the completion time of the room event.
    /// </summary>
    /// <param name="towerId">The ID of the tower containing the room.</param>
    /// <param name="towerName">The name of the tower containing the room.</param>
    /// <param name="floorNumber">The number of the floor containing the room.</param>
    /// <param name="floorId">The ID of the floor containing the room.</param>
    /// <param name="roomId">The ID of the room being completed.</param>
    /// <param name="success">Indicates whether the room event was completed successfully.</param>
    /// <param name="completionTime">The time it took to complete the room event.</param>
    private void RaiseRoomCompletionEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, bool success, TimeSpan completionTime)
    {
        var args = new RoomCompletionEventArgs(towerId, towerName, floorNumber, floorId, roomId, success, completionTime);
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomEventCompleted, args);
    }

    private void RaiseRoomEnteredEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state)
    {
        var args = new RoomEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state);
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomEntered, args);
    }

    private void RaiseRoomPathEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state, List<string> paths)
    {
        var args = new RoomPathEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state, paths);
        Globals.Instance.gameMangers.Events.RaiseEvent(EventType.RoomPathDiscovered, args);
    }



    /// <summary>
    /// Selects a valid room type based on the current position in the floor and the room placement rules.
    /// Ensures room types are placed according to predefined spacing and distribution rules.
    /// </summary>
    /// <param name="position">The current position in the floor where the room is being placed.</param>
    /// <param name="lastPositions">A dictionary tracking the last position of each room type.</param>
    /// <param name="totalRooms">The total number of rooms to be placed on the floor.</param>
    /// <returns>The selected room type for the current position.</returns>
    private RoomType SelectRoomType(int position, Dictionary<RoomType, int> lastPositions, int totalRooms)
    {
        // Filter room types based on placement validity, considering room type distribution and spacing constraints
        var availableTypes = _config.RoomTypeDistribution
            .Where(kvp => IsValidRoomPlacement(kvp.Key, position, lastPositions, totalRooms)) // Ensure valid room placement
            .ToList();

        // Calculate the total weight of the available room types based on their distribution
        float totalWeight = availableTypes.Sum(x => x.Value);

        // Generate a random number between 0 and totalWeight to select a room type based on its weight
        float random = (float)(_random.NextDouble() * totalWeight);
        float currentWeight = 0;

        // Iterate over the available room types and select one based on the random value and their weight
        foreach (var type in availableTypes)
        {
            currentWeight += type.Value;
            if (random <= currentWeight) // If the random number is less than or equal to the cumulative weight, select this room type
                return type.Key;
        }

        // Default room type if none is selected (though this shouldn't normally occur)
        return RoomType.Combat;
    }

    /// <summary>
    /// Checks whether a room type can be placed at the current position based on the minimum spacing rule.
    /// Ensures that room types do not violate the required spacing constraints.
    /// </summary>
    /// <param name="type">The room type being checked for placement.</param>
    /// <param name="position">The current position in the floor where the room is being placed.</param>
    /// <param name="lastPositions">A dictionary tracking the last position of each room type.</param>
    /// <param name="totalRooms">The total number of rooms on the floor, used for placement logic.</param>
    /// <returns>True if the room type can be placed at the current position, false otherwise.</returns>
    private bool IsValidRoomPlacement(RoomType type, int position, Dictionary<RoomType, int> lastPositions, int totalRooms)
    {
        // Check if a minimum spacing constraint exists for this room type
        if (!_config.MinimumSpacing.TryGetValue(type, out int spacing))
            return true; // If no minimum spacing is defined, placement is always valid

        // Check if the room type has been placed before and retrieve its last position
        if (!lastPositions.TryGetValue(type, out int lastPosition))
            return true; // If the room type has not been placed yet, placement is valid

        // Ensure that the current position is at least the required spacing distance from the last placed room of the same type
        return position - lastPosition >= spacing;
    }

    /// <summary>
    /// Calculates the difficulty of a room based on its position, floor number, base floor difficulty, and room type.
    /// The difficulty is scaled by the position of the room on the floor, with additional multipliers for specific room types.
    /// </summary>
    /// <param name="roomType">The type of the room (e.g., Boss, MiniBoss, etc.), which influences its difficulty.</param>
    /// <param name="roomPosition">The position of the room within the floor (starting from 0).</param>
    /// <param name="floorNumber">The floor number where the room is located.</param>
    /// <returns>The calculated difficulty for the room, clamped to the maximum floor difficulty.</returns>
    private float CalculateRoomDifficulty(RoomType roomType, int roomPosition, int floorNumber)
    {
        float typeMultiplier = roomType switch
        {
            RoomType.Boss => GameBalanceConfig.RoomDifficulty.BossMultiplier,
            RoomType.MiniBoss => GameBalanceConfig.RoomDifficulty.MiniBossMultiplier,
            RoomType.Combat => GameBalanceConfig.RoomDifficulty.CombatMultiplier,
            RoomType.Event => GameBalanceConfig.RoomDifficulty.EventMultiplier,
            RoomType.Rest => GameBalanceConfig.RoomDifficulty.RestMultiplier,
            _ => GameBalanceConfig.RoomDifficulty.CombatMultiplier
        };

        // Calculate position-based scaling
        float positionScaling = Mathf.Pow(GameBalanceConfig.RoomDifficulty.PositionScalingBase, roomPosition);

        // Add floor number scaling
        float floorScaling = 1 + (floorNumber * GameBalanceConfig.RoomDifficulty.FloorNumberScaling);

        return typeMultiplier * positionScaling * floorScaling;
    }


    private void OnRoomEntered(RoomEventArgs args)
    {
        DebugLogger.Log($"Entered room {args.RoomId} of type {args.Type} on floor {args.FloorId}", DebugLogger.LogCategory.Room);
    }

    private void OnRoomExited(RoomEventArgs args)
    {
        DebugLogger.Log($"Exited room {args.RoomId} on floor {args.FloorId}", DebugLogger.LogCategory.Room);
    }

    private void OnRoomStateChanged(RoomStateChangedArgs args)
    {
        DebugLogger.Log($"Room {args.RoomId} on floor {args.FloorId} state changed from {args.OldState} to {args.NewState}", DebugLogger.LogCategory.Room);
    }

    private void OnRoomCompleted(RoomCompletionEventArgs args)
    {
        DebugLogger.Log($"Room {args.RoomId} on floor {args.FloorId} completed with success: {args.Success}", DebugLogger.LogCategory.Room);
    }

    private void OnRoomFailed(RoomCompletionEventArgs args)
    {
        DebugLogger.Log($"Room {args.RoomId} on floor {args.FloorId} failed", DebugLogger.LogCategory.Room);
    }

}