using Godot;
using System;
using System.Linq;
using Tower_001.Scripts.GameLogic.Tower_System.Room;
using static GlobalEnums;

public class RoomGenerator
{
    private readonly FloorData _floor;
    private readonly Random _random;
    private readonly RoomManager _roomManager;

    public RoomGenerator(FloorData floor)
    {
        _floor = floor ?? throw new ArgumentNullException(nameof(floor));
        _random = new Random();
        _roomManager = Globals.Instance.gameMangers.World.Rooms;
    }

    public bool GenerateRoomsForFloor(int roomCount)
    {
        try
        {
            _floor.Rooms = _roomManager.GenerateRooms(_floor);
            
            if (_floor.Rooms == null || !_floor.Rooms.Any())
            {
                GD.PrintErr($"No rooms generated for floor {_floor.FloorNumber}");
                return false;
            }

            // Make first room available if this is first floor
            if (_floor.FloorNumber == 1)
            {
                _floor.Rooms.First().State = RoomState.Available;
            }

            DebugLogger.Log($"Generated {_floor.Rooms.Count} rooms for floor {_floor.FloorNumber}", DebugLogger.LogCategory.Room);
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error generating rooms: {ex.Message}");
            return false;
        }
    }
}