using System;
using System.Collections.Generic;
using static GlobalEnums;
using Tower_001.Scripts.GameLogic.Balance;

namespace Tower_001.Scripts.GameLogic.Tower_System.Room
{
    /// <summary>
    /// Handles the generation of different floor types and their rooms
    /// </summary>
    public class FloorGenerator
    {
        private readonly RoomFactory _roomFactory;
        private readonly RoomConfiguration _config;
        private readonly RoomEventDispatcher _eventDispatcher;
        private readonly RoomDifficultyCalculator _difficultyCalculator;
        private readonly Random _random;

        public FloorGenerator(
            RoomFactory roomFactory, 
            RoomConfiguration config,
            RoomEventDispatcher eventDispatcher,
            RoomDifficultyCalculator difficultyCalculator)
        {
            _roomFactory = roomFactory ?? throw new ArgumentNullException(nameof(roomFactory));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _difficultyCalculator = difficultyCalculator ?? throw new ArgumentNullException(nameof(difficultyCalculator));
            _random = new Random();
        }

        /// <summary>
        /// Generates a normal floor with multiple rooms
        /// </summary>
        public List<RoomData> GenerateNormalFloor(FloorData floor)
        {
            int roomCount = _random.Next(_config.MinRoomsPerFloor, _config.MaxRoomsPerFloor + 1);
            var rooms = new List<RoomData>();
            var lastRoomPositions = new Dictionary<RoomType, int>();

            for (int i = 0; i < roomCount - 1; i++)
            {
                var roomType = _roomFactory.SelectRoomType(i, lastRoomPositions, roomCount);
                var difficulty = _difficultyCalculator.CalculateRoomDifficulty(roomType, i, floor.FloorNumber);
                var room = _roomFactory.CreateRoom(roomType, i, floor.FloorNumber, difficulty);

                rooms.Add(room);
                lastRoomPositions[roomType] = i;

                _eventDispatcher.RaiseRoomDiscoveredEvent(
                    floor.Id,
                    floor.Name,
                    floor.FloorNumber,
                    floor.Id,
                    room.Id,
                    room.Type,
                    room.State
                );
            }

            var finalRoomType = _random.Next(2) == 0 ? RoomType.Boss : RoomType.Reward;
            var finalDifficulty = _difficultyCalculator.CalculateRoomDifficulty(finalRoomType, roomCount - 1, floor.FloorNumber);
            var finalRoom = _roomFactory.CreateRoom(finalRoomType, roomCount - 1, floor.FloorNumber, finalDifficulty);

            if (finalRoomType == RoomType.FloorBoss)
            {
                finalRoom.BossId = Guid.NewGuid().ToString();
            }

            rooms.Add(finalRoom);

            _eventDispatcher.RaiseRoomDiscoveredEvent(
                floor.Id,
                floor.Name,
                floor.FloorNumber,
                floor.Id,
                finalRoom.Id,
                finalRoom.Type,
                finalRoom.State
            );

            return rooms;
        }

        /// <summary>
        /// Generates a boss-only floor
        /// </summary>
        public List<RoomData> GenerateBossOnlyFloor(FloorData floor)
        {
            var difficulty = floor.Difficulty.BaseValue * _config.BossRoomDifficultyMultiplier;
            var room = _roomFactory.CreateRoom(RoomType.Boss, 0, floor.FloorNumber, difficulty);
            room.BossId = Guid.NewGuid().ToString();

            _eventDispatcher.RaiseRoomDiscoveredEvent(
                floor.Id,
                floor.Name,
                floor.FloorNumber,
                floor.Id,
                room.Id,
                room.Type,
                room.State
            );

            return new List<RoomData> { room };
        }
    }
}