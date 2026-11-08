using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Tower_System.Room
{
    /// <summary>
    /// Handles the creation and initialization of rooms with specific configurations
    /// </summary>
    public class RoomFactory
    {
        private readonly RoomConfiguration _config;
        private readonly Random _random;

        public RoomFactory(RoomConfiguration config)
        {
            _config = config;
            _random = new Random();
        }

        /// <summary>
        /// Creates a new room with specified parameters.
        /// </summary>
        public RoomData CreateRoom(RoomType type, int position, int floorNumber, float difficulty)
        {
            return new RoomData
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Room {position + 1}",
                Type = type,
                Position = new Vector2(position, 0),
                BaseDifficulty = difficulty,
                IsRequired = true,
                State = RoomState.Locked,
                ElementType = ElementType.None,
                EncounterLevel = floorNumber
            };
        }

        /// <summary>
        /// Selects a valid room type based on the current position in the floor and the room placement rules.
        /// </summary>
        public RoomType SelectRoomType(int position, Dictionary<RoomType, int> lastPositions, int totalRooms)
        {
            var availableTypes = _config.RoomTypeDistribution
                .Where(kvp => IsValidRoomPlacement(kvp.Key, position, lastPositions, totalRooms))
                .ToList();

            float totalWeight = availableTypes.Sum(x => x.Value);
            float random = (float)(_random.NextDouble() * totalWeight);
            float currentWeight = 0;

            foreach (var type in availableTypes)
            {
                currentWeight += type.Value;
                if (random <= currentWeight)
                    return type.Key;
            }

            return RoomType.Combat;
        }

        /// <summary>
        /// Checks whether a room type can be placed at the current position based on the minimum spacing rule.
        /// </summary>
        private bool IsValidRoomPlacement(RoomType type, int position, Dictionary<RoomType, int> lastPositions, int totalRooms)
        {
            if (!_config.MinimumSpacing.TryGetValue(type, out int spacing))
                return true;

            if (!lastPositions.TryGetValue(type, out int lastPosition))
                return true;

            return position - lastPosition >= spacing;
        }
    }
}