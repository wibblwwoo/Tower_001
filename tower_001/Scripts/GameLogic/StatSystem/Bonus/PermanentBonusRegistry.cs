using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Bonus
{
    /// <summary>
    /// Registry that manages permanent bonuses for a character.
    /// Handles storing, retrieving, and calculating total bonus values.
    /// </summary>
    public class PermanentBonusRegistry
    {
        private readonly string _characterId;
        private readonly Dictionary<StatType, List<PermanentBonus>> _bonuses;

        public PermanentBonusRegistry(string characterId)
        {
            _characterId = characterId;
            _bonuses = new Dictionary<StatType, List<PermanentBonus>>();
        }

        /// <summary>
        /// Adds a permanent bonus to the registry
        /// </summary>
        public void AddBonus(PermanentBonus bonus)
        {
            if (!_bonuses.ContainsKey(bonus.StatType))
            {
                _bonuses[bonus.StatType] = new List<PermanentBonus>();
            }

            // Remove any existing bonus with the same name
            _bonuses[bonus.StatType].RemoveAll(b => b.Name == bonus.Name);
            
            // Add the new bonus
            _bonuses[bonus.StatType].Add(bonus);
            
            // Log the addition
            DebugLogger.Log($"Added permanent bonus to {_characterId}: {bonus}", DebugLogger.LogCategory.Stats);
        }

        /// <summary>
        /// Removes a bonus by its name and stat type
        /// </summary>
        public void RemoveBonus(string name, StatType statType)
        {
            if (_bonuses.TryGetValue(statType, out var bonusList))
            {
                int count = bonusList.RemoveAll(b => b.Name == name);
                if (count > 0)
                {
                    DebugLogger.Log($"Removed permanent bonus '{name}' from {statType} for {_characterId}", 
                        DebugLogger.LogCategory.Stats);
                }
            }
        }

		public void RemoveBonus(PermanentBonus bonus)
		{

            if (bonus != null)
            {
                RemoveBonus(bonus.Name, bonus.StatType);
            }
			else
            {
				DebugLogger.Log($"trying to remove a permanent bonus that does not exist bonus '{bonus.Name}' from {bonus.StatType}",DebugLogger.LogCategory.Stats);

			}
		}

		/// <summary>
		/// Gets all bonuses for a specific stat type
		/// </summary>
		public IEnumerable<PermanentBonus> GetBonusesForStat(StatType statType)
        {
            return _bonuses.TryGetValue(statType, out var bonusList) ? bonusList : Enumerable.Empty<PermanentBonus>();
        }

        /// <summary>
        /// Gets all bonuses from a specific source
        /// </summary>
        public IEnumerable<PermanentBonus> GetBonusesFromSource(BonusSource source)
        {
            return _bonuses.Values
                .SelectMany(list => list)
                .Where(bonus => bonus.Source == source);
        }

        /// <summary>
        /// Calculates the total bonus value for a stat type
        /// </summary>
        public float GetTotalBonusForStat(StatType statType)
        {
            if (!_bonuses.TryGetValue(statType, out var bonusList) || !bonusList.Any())
                return 0f;

            float total = bonusList.Sum(b => b.Value);
            DebugLogger.Log($"Total permanent bonus for {statType}: {total:P2}", DebugLogger.LogCategory.Stats);
            return total;
        }

        /// <summary>
        /// Clears all bonuses from a specific source
        /// </summary>
        public void ClearBonusesFromSource(BonusSource source)
        {
            foreach (var bonusList in _bonuses.Values)
            {
                bonusList.RemoveAll(b => b.Source == source);
            }
            DebugLogger.Log($"Cleared all bonuses from source {source} for {_characterId}", 
                DebugLogger.LogCategory.Stats);
        }

	}
}
