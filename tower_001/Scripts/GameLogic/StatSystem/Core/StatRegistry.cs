using System;
using System.Collections.Generic;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Core
{
    /// <summary>
    /// Manages all stats for a character, providing centralized access and management
    /// </summary>
    public class StatRegistry
    {
        private readonly string _characterId;
        private readonly Dictionary<StatType, StatData> _stats = new();

        public StatRegistry(string characterId)
        {
            _characterId = characterId;
        }

        public StatData CreateStat(StatType statType, float baseValue = 0)
        {
            if (_stats.ContainsKey(statType))
                throw new ArgumentException($"Stat of type '{statType}' already exists for character {_characterId}.");

            var stat = new StatData(_characterId, statType, baseValue);
            _stats[statType] = stat;
            return stat;
        }

        public StatData GetStat(StatType statType)
        {
            if (!_stats.TryGetValue(statType, out var stat))
                throw new KeyNotFoundException($"Stat of type '{statType}' not found for character {_characterId}.");
            
            return stat;
        }

        public bool TryGetStat(StatType statType, out StatData stat)
        {
            return _stats.TryGetValue(statType, out stat);
        }

        public void RemoveStat(StatType statType)
        {
            _stats.Remove(statType);
        }

        public void Clear()
        {
            _stats.Clear();
        }

        // Optional: Method to initialize common stats for a character
        public void InitializeDefaultStats()
        {
            // Initialize common stats with default values
            // Values should come from game balance configuration
            CreateStat(StatType.Health, 100);
            CreateStat(StatType.Attack, 10);
            CreateStat(StatType.Defense, 5);
            CreateStat(StatType.Energy, 100);
            // Add other default stats as needed
        }
    }
}