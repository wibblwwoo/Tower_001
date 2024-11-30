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
	public string Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public RoomType Type { get; set; }
	public Vector2 Position { get; set; }
	public bool IsRequired { get; set; } = true;
	public bool IsHidden { get; set; }
	public ElementType ElementType { get; set; }
	public float BaseDifficulty { get; set; } = 1.0f;
	public List<string> ConnectedRoomIds { get; set; } = new();
	public string BossId { get; set; }  // For boss rooms
	public int EncounterLevel { get; set; } = 1;
	public List<string> AvailableMonsters { get; set; } = new();
	public HashSet<RoomTag> Tags { get; set; } = new();
	public RoomState State { get; internal set; } = RoomState.Locked;
}