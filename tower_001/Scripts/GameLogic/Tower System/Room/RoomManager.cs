using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;
using Tower_001.Scripts.GameLogic.Tower_System.Room;

namespace Tower_001.Scripts.GameLogic.Tower_System.Room
{
    /// <summary>
    /// Manages room instances and their lifecycle within a floor.
    /// Handles room generation, state transitions, and difficulty scaling.
    /// Integrates with the event system for notifying about room events.
    /// </summary>
    public partial class RoomManager : BaseManager
    {
        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(EventManager),
            typeof(FloorManager)  // Rooms need Floor info
        };
        
        // Component fields
        private readonly RoomFactory _roomFactory;
        private readonly FloorGenerator _floorGenerator;
        private readonly RoomEventDispatcher _eventDispatcher;
        private readonly RoomDifficultyCalculator _difficultyCalculator;
        private readonly RoomConfiguration _config;

        /// <summary>
        /// Constructor for the RoomManager class, initializing components
        /// </summary>
        public RoomManager()
        {
            _config = new RoomConfiguration();
            _roomFactory = new RoomFactory(_config);
            _difficultyCalculator = new RoomDifficultyCalculator();
            _eventDispatcher = new RoomEventDispatcher(Globals.Instance.gameMangers.Events);
            _floorGenerator = new FloorGenerator(
                _roomFactory,
                _config,
                _eventDispatcher,
                _difficultyCalculator
            );
        }

        /// <summary>
        /// Registers event handlers for room-related events.
        /// </summary>
        protected override void RegisterEventHandlers()
        {
            EventManager.AddHandler<RoomEventArgs>(EventType.RoomEntered, OnRoomEntered);
            EventManager.AddHandler<RoomEventArgs>(EventType.RoomExited, OnRoomExited);
            EventManager.AddHandler<RoomStateChangedArgs>(EventType.RoomStateChanged, OnRoomStateChanged);
            EventManager.AddHandler<RoomCompletionEventArgs>(EventType.RoomEventCompleted, OnRoomCompleted);
            EventManager.AddHandler<RoomCompletionEventArgs>(EventType.RoomEventFailed, OnRoomFailed);
        }

        /// <summary>
        /// Generates a list of rooms for a given floor based on floor properties and configuration.
        /// </summary>
        public List<RoomData> GenerateRooms(FloorData floor)
        {
            return floor.FloorNumber % _config.BossFloorInterval == 0
                ? _floorGenerator.GenerateBossOnlyFloor(floor)
                : _floorGenerator.GenerateNormalFloor(floor);
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
}