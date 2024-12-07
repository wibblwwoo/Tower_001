using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
/// <summary>
/// Represents the static properties of a room in the tower.
/// Includes configuration for room type, difficulty, position, and connections.
/// </summary>
public partial class RoomData
{
    /// <summary>
    /// Unique identifier for the room instance
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the room shown in the UI
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of the room's purpose and contents
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The type of room (e.g., Combat, Boss, Treasure, etc.)
    /// </summary>
    public RoomType Type { get; set; }

    /// <summary>
    /// 2D coordinates of the room within the floor layout
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Indicates if the room must be completed to progress
    /// Default: true
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// If true, room is not visible on the map until discovered
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// The elemental affinity of the room affecting encounters and rewards
    /// </summary>
    public ElementType ElementType { get; set; }

    /// <summary>
    /// Base difficulty value before scaling. Default: 1.0
    /// </summary>
    public float BaseDifficulty { get; set; } = 1.0f;

    /// <summary>
    /// IDs of rooms that can be accessed from this room
    /// </summary>
    public List<string> ConnectedRoomIds { get; set; } = new();

    /// <summary>
    /// ID of the boss entity for boss-type rooms
    /// </summary>
    public string BossId { get; set; }

    /// <summary>
    /// Level requirement for the room's encounters. Default: 1
    /// </summary>
    public int EncounterLevel { get; set; } = 1;

    /// <summary>
    /// List of monster IDs that can spawn in this room
    /// </summary>
    public List<string> AvailableMonsters { get; set; } = new();

    /// <summary>
    /// Special attributes or modifiers applied to the room
    /// </summary>
    public HashSet<RoomTag> Tags { get; set; } = new();

    /// <summary>
    /// Current state of the room (Locked, Available, Completed, etc.)
    /// Default: Locked
    /// </summary>
    public RoomState State { get; internal set; } = RoomState.Locked;
}