using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;
using Tower.GameLogic.Core;

/// <summary>
/// Responsible for generating rooms based on configuration and floor difficulty.
/// Supports both normal and boss-only floor generation.
/// Ensures room distribution adheres to predefined rules and constraints.
/// </summary>
public partial class RoomGenerator
{
    private readonly FloorData _floorData; // FloorData object to store and manipulate data for the current floor
    private readonly RoomConfiguration _config; // Room configuration settings (e.g., maximum rooms, room type distribution)

    /// <summary>
    /// Constructor to initialize the RoomGenerator with required dependencies and configurations.
    /// </summary>
    /// <param name="roomManager">The RoomManager that handles room management for the floor.</param>
    /// <param name="floorData">The floor data that contains the current floor's information.</param>
    public RoomGenerator(FloorData floorData)
    {
        _floorData = floorData; // Initializes the FloorData (contains floor-specific info)
        _config = new RoomConfiguration(); // Initializes the room configuration settings
    }

    /// <summary>
    /// Generates rooms for the current floor, ensuring that the number of rooms is valid and falls within the allowed range.
    /// </summary>
    /// <param name="roomCount">The number of rooms to generate for the floor.</param>
    /// <returns>True if the rooms were generated successfully, false otherwise.</returns>
    public bool GenerateRoomsForFloor(int roomCount)
    {
        // Validate the room count to ensure it's within a valid range
        if (roomCount <= 0 || roomCount > _config.MaxRoomsPerFloor)
        {
            GD.PrintErr($"Invalid room count: {roomCount}");
            return false;
        }

        try
        {
            List<RoomData> rooms = new(); // Initialize a list to store generated room data
            var lastRoomPositions = new Dictionary<RoomType, int>(); // Track last position of each room type

            // Ensure the last room is always a boss room
            //var bossRoom = CreateRoom(RoomType.Boss, roomCount - 1, CalculateRoomDifficulty(roomCount - 1, RoomType.Boss));
            //rooms.Add(bossRoom);

            var floorBossRoom = CreateRoom(RoomType.FloorBoss, roomCount - 1, CalculateRoomDifficulty(roomCount - 1, RoomType.FloorBoss));
            rooms.Add(floorBossRoom);

            // Ensure at least one room is a reward or miniboss room
            int guaranteedSpecialRoomIndex = RandomManager.Instance.Next(0, roomCount - 1); // Random index excluding the last room
            var specialRoomType = RandomManager.Instance.Next(0, 2) == 0 ? RoomType.Reward : RoomType.MiniBoss; // Randomly select reward or miniboss
            var specialRoom = CreateRoom(specialRoomType, guaranteedSpecialRoomIndex, CalculateRoomDifficulty(guaranteedSpecialRoomIndex, specialRoomType));
            rooms.Add(specialRoom);
            lastRoomPositions[specialRoomType] = guaranteedSpecialRoomIndex;

            // Generate the remaining rooms dynamically
            for (int i = 0; i < roomCount - 1; i++)
            {
                // Skip already assigned special room index
                if (i == guaranteedSpecialRoomIndex)
                    continue;

                // Select a valid room type
                var roomType = SelectRoomType(i, lastRoomPositions, roomCount);
                var room = CreateRoom(roomType, i, CalculateRoomDifficulty(i, roomType));
                rooms.Add(room);
                lastRoomPositions[roomType] = i;
            }
            // Sort rooms by position before assigning them to the floor
            _floorData.Rooms = rooms.OrderBy(r => r.Position.X).ThenBy(r => r.Position.Y).ToList();
            return true; // Room generation was successful
        }
        catch (Exception ex)
        {
            // Catch and log any errors during room generation
            GD.PrintErr($"Error generating rooms: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Selects the type of room to create at a given position on the floor.
    /// The selection is weighted based on the room type distribution and considers placement rules.
    /// </summary>
    /// <param name="position">The position of the room on the floor (0-based index).</param>
    /// <param name="lastPositions">A dictionary tracking the last positions of each room type.</param>
    /// <param name="totalRooms">The total number of rooms on the floor.</param>
    /// <returns>The selected <see cref="RoomType"/> for the room.</returns>
    private RoomType SelectRoomType(int position, Dictionary<RoomType, int> lastPositions, int totalRooms)
    {
        // Get valid room types for this position
        var validRoomTypes = GetValidRoomTypes(position, lastPositions);

        // Return fallback type if no valid types remain
        if (!validRoomTypes.Any())
            return GetFallbackRoomType();

        // Normalize weights for valid room types
        var normalizedWeights = NormalizeWeights(validRoomTypes);

        // Select a room type based on normalized weights
        return SelectFromNormalizedWeights(normalizedWeights);
    }

    /// <summary>
    /// Checks if a room type can be placed at a specific position based on minimum spacing rules.
    /// </summary>
    /// <param name="type">The type of room to validate.</param>
    /// <param name="position">The current position of the room on the floor (0-based index).</param>
    /// <param name="lastPositions">
    /// A dictionary tracking the last positions of each room type.
    /// Used to enforce spacing rules between similar room types.
    /// </param>
    /// <returns>True if the room type can be placed at the specified position; otherwise, false.</returns>
    private bool IsValidRoomPlacement(RoomType type, int position, Dictionary<RoomType, int> lastPositions)
    {
        // Check if there are minimum spacing rules for the specified room type
        if (_config.MinimumSpacing.TryGetValue(type, out int spacing))
        {
            // Check if the last position of this room type is tracked
            if (lastPositions.TryGetValue(type, out int lastPosition))
            {
                // Ensure the spacing rule is satisfied
                if (position - lastPosition < spacing)
                {
                    return false; // Placement is invalid due to insufficient spacing
                }
            }
        }

        return true; // Placement is valid
    }

    /// <summary>
    /// Creates a new instance of <see cref="RoomData"/> with specified parameters.
    /// Initializes the room's properties based on its type, position, and difficulty.
    /// </summary>
    /// <param name="type">The type of the room (e.g., combat, reward, boss).</param>
    /// <param name="position">The position of the room on the floor (0-based index).</param>
    /// <param name="difficulty">The base difficulty level for the room.</param>
    /// <returns>A new <see cref="RoomData"/> object representing the room.</returns>
    private RoomData CreateRoom(RoomType type, int position, float difficulty)
    {
        // Initialize the room data with the provided parameters and default values
        var room = new RoomData
        {
            Id = Guid.NewGuid().ToString(), // Generate a unique identifier for the room
            Name = $"Room {position + 1}", // Assign a default name based on the position
            Description = GenerateRoomDescription(type), // Generate a description based on the room type
            Type = type, // Set the room type
            Position = new Vector2(position, 0), // Set the room's position in a 2D layout
            BaseDifficulty = difficulty, // Assign the base difficulty for the room
            IsRequired = true, // Mark the room as required for progression
            State = position == 0 ? RoomState.Available : RoomState.Locked, // Unlock the first room, lock others by default
            ElementType = _floorData.Type switch // Set the elemental type based on the floor type
            {
                FloorType.Wind => ElementType.None,
                FloorType.Magic => ElementType.None,
                _ => ElementType.None
            },
            Tags = GenerateRoomTags(type) // Generate room tags based on the type
        };

        // Assign a unique boss ID if the room is a boss or mini-boss
        if (type == RoomType.Boss || type == RoomType.MiniBoss)
        {
            room.BossId = Guid.NewGuid().ToString();
        }

        return room; // Return the constructed room
    }

    /// <summary>
    /// Generates a new boss room for the current floor.
    /// Initializes the room with default boss-specific properties, including difficulty and tags.
    /// </summary>
    /// <returns>A <see cref="RoomData"/> object representing the boss room.</returns>
    private RoomData GenerateBossRoom()
    {
        return new RoomData
        {
            Id = Guid.NewGuid().ToString(), // Generate a unique identifier for the room
            Name = $"Boss Room - Floor {_floorData.FloorNumber}", // Assign a name indicating it's a boss room
            Description = "A powerful enemy awaits...", // Set a predefined description for the boss room
            Type = RoomType.Boss, // Mark the room as a boss room
            Position = new Vector2(0, 0), // Place the boss room at the default position (center of the floor)
            BaseDifficulty = _floorData.Difficulty.BaseValue * _config.BossRoomDifficultyMultiplier,
            // Calculate the base difficulty using the floor's base difficulty and a boss-specific multiplier

            IsRequired = true, // Boss rooms are always required for progression
            State = RoomState.Available, // Boss rooms start in an available state
            ElementType = CalculateDynamicElementType(_floorData.Type, _floorData.FloorNumber),
            BossId = Guid.NewGuid().ToString(), // Generate a unique identifier for the boss in the room
            Tags = GenerateRoomTags(RoomType.Boss)
            //ElementType = _floorData.Type switch // Determine the elemental type based on the floor's type
            //{
            //    FloorType.Wind => ElementType.None,
            //    FloorType.Magic => ElementType.None,
            //    _ => ElementType.None
            //},
            //Tags = new HashSet<RoomTag> // Add tags to categorize the room
            //{
            //    RoomTag.Boss, // Tag the room as a boss room
            //    RoomTag.Combat, // Indicate that combat is involved
            //    RoomTag.Required // Mark the room as required for floor completion
            //}
        };
    }

    /// <summary>
    /// Calculates the difficulty of a room based on its position, type, and the floor's base difficulty.
    /// Applies progression scaling and type-specific multipliers.
    /// </summary>
    /// <param name="position">The position of the room on the floor (0-based index).</param>
    /// <param name="roomType">The type of the room (e.g., boss, miniboss, combat).</param>
    /// <returns>The calculated difficulty value for the room.</returns>
    private float CalculateRoomDifficulty(int position, RoomType roomType)
    {
        // Base difficulty from the floor
        float baseDifficulty = _floorData.Difficulty.BaseValue;

        // Scale difficulty based on the room's position in the floor
        float progression = 1 + (_config.BaseRoomDifficultyIncrement * position);

        // Adjust difficulty further based on the room's type
        float typeMultiplier = roomType switch
        {
            RoomType.Boss => _config.BossRoomDifficultyMultiplier, // Higher multiplier for boss rooms
            RoomType.MiniBoss => _config.MiniBossRoomDifficultyMultiplier, // Moderate multiplier for miniboss rooms
            _ => 1.0f // Default multiplier for standard rooms
        };

        // Calculate and cap the difficulty at the maximum allowed for the floor
        return Math.Min(
            baseDifficulty * progression * typeMultiplier, // Final difficulty calculation
            _config.MaxFloorDifficulty // Maximum difficulty limit
        );
    }

    /// <summary>
    /// Generates a description for a room based on its type.
    /// Provides context for what to expect in the room (e.g., combat, rewards, events).
    /// </summary>
    /// <param name="type">The type of the room (e.g., combat, rest, reward).</param>
    /// <returns>A string description of the room.</returns>
    private string GenerateRoomDescription(RoomType type)
    {
        return type switch
        {
            RoomType.Combat => "A hostile area filled with enemies.", // Description for combat rooms
            RoomType.Rest => "A peaceful room to recover your strength.", // Description for rest rooms
            RoomType.Reward => "A room containing valuable treasures.", // Description for reward rooms
            RoomType.Event => "Something interesting might happen here.", // Description for event rooms
            RoomType.MiniBoss => "A powerful enemy guards this room.", // Description for miniboss rooms
            RoomType.Boss => "An extremely dangerous foe awaits.", // Description for boss rooms
            RoomType.Encounter => "A unique encounter awaits.", // Description for encounter rooms
            _ => "A mysterious room." // Default description for unclassified room types
        };
    }

    /// <summary>
    /// Dynamically determines the <see cref="ElementType"/> for a room based on floor data.
    /// </summary>
    /// <param name="floorType">The type of the floor (e.g., Wind, Magic).</param>
    /// <param name="floorNumber">The floor number, used to influence element rotation or scaling.</param>
    /// <returns>The calculated <see cref="ElementType"/> for the room.</returns>
    private ElementType CalculateDynamicElementType(FloorType floorType, int floorNumber)
    {
        return floorType switch
        {
            FloorType.Wind => ElementType.None,
            FloorType.Magic => ElementType.None,
            _ => ElementType.None
            //FloorType.Wind => ElementType.Air, // Specific to wind floors
            //FloorType.Magic => ElementType.Fire, // Example: Magic floors have fire elements
            //FloorType.Fire => floorNumber % 3 == 0 ? ElementType.Earth : ElementType.Water, // Rotates every 3 floors
            //FloorType.Water => ElementType.Water, // Default for water floors
            //_ => ElementType.None // Default fallback
        };
    }

    /// <summary>
    /// Generates a set of tags for a room based on its type.
    /// </summary>
    /// <param name="type">The type of the room (e.g., boss, combat, reward).</param>
    /// <returns>A set of <see cref="RoomTag"/> values associated with the room type.</returns>
    private HashSet<RoomTag> GenerateRoomTags(RoomType type)
    {
        var tags = new HashSet<RoomTag>();

        switch (type)
        {
            case RoomType.Combat:
                tags.Add(RoomTag.Combat);
                break;
            case RoomType.Rest:
                tags.Add(RoomTag.Rest);
                break;
            case RoomType.Reward:
                tags.Add(RoomTag.Treasure);
                break;
            case RoomType.Event:
                tags.Add(RoomTag.Event);
                break;
            case RoomType.MiniBoss:
                tags.Add(RoomTag.Combat);
                tags.Add(RoomTag.Elite);
                break;
            case RoomType.Boss:
                tags.Add(RoomTag.Combat);
                tags.Add(RoomTag.Boss);
                break;
        }

        tags.Add(RoomTag.Required); // All rooms are required for now
        return tags;
    }

    private List<KeyValuePair<RoomType, float>> GetValidRoomTypes(int position, Dictionary<RoomType, int> lastPositions)
    {
        return _config.RoomTypeDistribution
            .Where(kvp => IsValidRoomPlacement(kvp.Key, position, lastPositions))
            .ToList();
    }

    /// <summary>
    /// Returns a fallback room type when no valid options are available.
    /// </summary>
    private RoomType GetFallbackRoomType()
    {
        return RoomType.Combat; // Default fallback type
    }

    /// <summary>
    /// Normalizes the weights of valid room types to sum up to 1.
    /// </summary>
    private List<(RoomType Type, float NormalizedWeight)> NormalizeWeights(List<KeyValuePair<RoomType, float>> validRoomTypes)
    {
        float totalWeight = validRoomTypes.Sum(x => x.Value);
        return validRoomTypes
            .Select(kvp => (Type: kvp.Key, NormalizedWeight: kvp.Value / totalWeight))
            .ToList();
    }

    /// <summary>
    /// Randomly selects a room type based on normalized weights.
    /// </summary>
    private RoomType SelectFromNormalizedWeights(List<(RoomType Type, float NormalizedWeight)> normalizedWeights)
    {
        float randomValue = (float)RandomManager.Instance.NextDouble();
        float cumulativeWeight = 0;

        foreach (var (type, weight) in normalizedWeights)
        {
            cumulativeWeight += weight;
            if (randomValue <= cumulativeWeight)
            {
                return type;
            }
        }

        // Should not reach here due to cumulative weight always summing to 1
        throw new InvalidOperationException("Failed to select a room type from normalized weights.");
    }
}
