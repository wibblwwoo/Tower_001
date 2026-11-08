using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Tower_System.Room
{
    /// <summary>
    /// Handles raising all room-related events
    /// </summary>
    public class RoomEventDispatcher
    {
        private readonly EventManager _eventManager;

        public RoomEventDispatcher(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void RaiseRoomDiscoveredEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state)
        {
            var args = new RoomEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state);
            _eventManager.RaiseEvent(EventType.RoomDiscovered, args);
        }

        public void RaiseRoomStateChangedEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomState oldState, RoomState newState)
        {
            var args = new RoomStateChangedArgs(towerId, towerName, floorNumber, floorId, roomId, oldState, newState);
            _eventManager.RaiseEvent(EventType.RoomStateChanged, args);
        }

        public void RaiseRoomCompletionEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, bool success)
        {
            var completionTime = TimeSpan.Zero;
            var args = new RoomCompletionEventArgs(towerId, towerName, floorNumber, floorId, roomId, success, completionTime);
            _eventManager.RaiseEvent(EventType.RoomEventCompleted, args);
        }

        public void RaiseRoomCompletionEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, bool success, TimeSpan completionTime)
        {
            var args = new RoomCompletionEventArgs(towerId, towerName, floorNumber, floorId, roomId, success, completionTime);
            _eventManager.RaiseEvent(EventType.RoomEventCompleted, args);
        }

        public void RaiseRoomEnteredEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state)
        {
            var args = new RoomEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state);
            _eventManager.RaiseEvent(EventType.RoomEntered, args);
        }

        public void RaiseRoomPathEvent(string towerId, string towerName, int floorNumber, string floorId, string roomId, RoomType type, RoomState state, System.Collections.Generic.List<string> paths)
        {
            var args = new RoomPathEventArgs(towerId, towerName, floorNumber, floorId, roomId, type, state, paths);
            _eventManager.RaiseEvent(EventType.RoomPathDiscovered, args);
        }
    }
}