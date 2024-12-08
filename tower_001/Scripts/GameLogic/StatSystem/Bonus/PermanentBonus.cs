using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Bonus
{
    /// <summary>
    /// Represents a permanent bonus that can be applied to a character's stats.
    /// These bonuses persist through resets and are typically granted from progression systems.
    /// </summary>
    public class PermanentBonus
    {
        // Core bonus properties
        public string Name { get; }
        public StatType StatType { get; }
        public BonusSource Source { get; }
        public float Value { get; }
        public string Description { get; }
        public DateTime DateAcquired { get; }

        /// <summary>
        /// Creates a new permanent bonus
        /// </summary>
        /// <param name="name">Unique identifier for the bonus</param>
        /// <param name="statType">The stat this bonus affects</param>
        /// <param name="source">Source of the bonus (e.g., Ascension, Prestige)</param>
        /// <param name="value">Bonus value as a decimal (e.g., 0.02 for 2%)</param>
        /// <param name="description">Human-readable description of the bonus</param>
        public PermanentBonus(
            string name,
            StatType statType,
            BonusSource source,
            float value,
            string description)
        {
            Name = name;
            StatType = statType;
            Source = source;
            Value = value / 100f; // Convert percentage to decimal (2% -> 0.02)
            Description = description;
            DateAcquired = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a copy of this bonus with a new value
        /// </summary>
        public PermanentBonus WithValue(float newValue)
        {
            return new PermanentBonus(
                Name,
                StatType,
                Source,
                newValue,
                Description
            );
        }

        /// <summary>
        /// Returns a string representation of this bonus for debugging
        /// </summary>
        public override string ToString()
        {
            return $"{Name}: {Value:P2} to {StatType} from {Source}";
        }
    }
}
