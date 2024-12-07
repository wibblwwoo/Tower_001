using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem
{
    /// <summary>
    /// Represents a modifier that affects a character's stat, such as a buff or debuff.
    /// Includes information about its source, type, value, and duration.
    /// </summary>
    /// Dependencies and Usage:
    /// - Used by: CharacterStats, BuffManager, StatusEffectSystem
    /// - Related Systems: Combat system, Equipment system, Skill system
    /// - Configuration: References GameBalanceConfig for modifier limits and scaling
    /// - Data Flow: Applies modifications to base character stats through buff/debuff pipeline
    /// - Core Features:
    ///   * Flat and percentage-based modifications
    ///   * Temporary and permanent buffs
    ///   * Source tracking for stacking rules
    ///   * Time-based expiration
    ///   * Unique identifier system
    /// </remarks>    
    public partial class StatModifier
    {
        /// <summary>
        /// A unique identifier for the stat modifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The source of the modifier (e.g., an ability, item, or effect).
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The type of modifier (e.g., flat increase, percentage increase).
        /// </summary>
        public BuffType Type { get; }

        /// <summary>
        /// The value of the modifier (e.g., +10, +15%).
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// The duration of the modifier.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// The start time of the modifier.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Indicates whether the modifier is permanent (i.e., has no expiration time).
        /// </summary>
        public bool IsPermanent => Duration == TimeSpan.Zero;

        /// <summary>
        /// Indicates whether the modifier has expired.
        /// </summary>
        public bool IsExpired => !IsPermanent && DateTime.Now - StartTime >= Duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatModifier"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the modifier.</param>
        /// <param name="source">The source of the modifier.</param>
        /// <param name="type">The type of modifier (e.g., flat, percentage).</param>
        /// <param name="value">The value of the modifier.</param>
        /// <param name="duration">
        /// Optional duration for the modifier. If null, the modifier is considered permanent.
        /// </param>
        public StatModifier(string id, string source, BuffType type, float value, TimeSpan? duration = null)
        {
            Id = id;
            Source = source;
            Type = type;
            Value = value;
            Duration = duration ?? TimeSpan.Zero;
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// Creates a permanent stat modifier
        /// </summary>
        public StatModifier(string id, string source, BuffType type, float value)
            : this(id, source, type, value, TimeSpan.Zero)
        {
        }
    }
}