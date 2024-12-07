using Godot;
using System;
using Tower_001.Scripts.GameLogic.StatSystem;
using static GlobalEnums;

namespace Tower_001.Scripts.Events
{
    /// <summary>
    /// Base event arguments class for all character-related events in the game.
    /// This class serves as the foundation for the character event system, enabling
    /// tracking and handling of various character state changes and interactions.
    /// </summary>
    /// <remarks>
    /// The CharacterEventArgs class is used as a base for more specific character events,
    /// providing a consistent way to identify the character involved through their unique ID.
    /// This is particularly useful for:
    /// - Character state tracking
    /// - Event logging and analytics
    /// - UI updates
    /// - Achievement tracking
    /// - Quest progression
    /// </remarks>
    public class CharacterEventArgs : GameEventArgs
    {
        /// <summary>
        /// Gets or sets the unique identifier of the character involved in the event.
        /// This ID is used to locate and update the correct character instance in the game.
        /// </summary>
        public string CharacterId { get; set; }

        /// <summary>
        /// Initializes a new instance of CharacterEventArgs with the specified character ID.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character involved in the event.</param>
        public CharacterEventArgs(string characterId)
        {
            CharacterId = characterId;
        }
    }

    /// <summary>
    /// Event arguments for tracking changes to character statistics.
    /// This class handles events when any character stat (Attack, Health, Defense, etc.) is modified.
    /// </summary>
    /// <remarks>
    /// CharacterStatEventArgs is crucial for:
    /// - UI updates showing stat changes
    /// - Combat calculations
    /// - Buff/Debuff system integration
    /// - Character progression tracking
    /// - Achievement triggers based on stat thresholds
    /// 
    /// The event captures both the old and new values to enable:
    /// - Stat change animations
    /// - Reverting changes if needed
    /// - Calculating the magnitude of the change
    /// - Logging significant stat modifications
    /// </remarks>
    public class CharacterStatEventArgs : CharacterEventArgs
    {
        /// <summary>Gets the type of stat that was changed (Attack, Health, Defense, etc.).</summary>
        public StatType StatType { get; set; }
        
        /// <summary>Gets the value of the stat before the change.</summary>
        public float OldValue { get; set; }
        
        /// <summary>Gets the new value of the stat after the change.</summary>
        public float NewValue { get; set; }

        /// <summary>
        /// Initializes a new instance of CharacterStatEventArgs to track a stat change.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <param name="statType">The type of stat that changed.</param>
        /// <param name="oldValue">The original value before the change.</param>
        /// <param name="newValue">The new value after the change.</param>
        public CharacterStatEventArgs(
            string characterId,
            StatType statType,
            float oldValue,
            float newValue) : base(characterId)
        {
            StatType = statType;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event arguments for handling stat modifier (buff/debuff) applications and removals.
    /// This class manages temporary or permanent modifications to character stats.
    /// </summary>
    /// <remarks>
    /// The stat buff system is integral to:
    /// - Combat mechanics (temporary combat buffs)
    /// - Equipment effects (gear-based stat modifications)
    /// - Status effects (poisons, enchantments, etc.)
    /// - Skill effects and abilities
    /// - Team synergy bonuses
    /// 
    /// StatModifier can represent:
    /// - Flat bonuses/penalties
    /// - Percentage-based modifications
    /// - Time-limited effects
    /// - Stacking buffs/debuffs
    /// </remarks>
    public class CharacterStatBuffEventArgs : CharacterEventArgs
    {
        /// <summary>Gets the type of stat being modified by the buff.</summary>
        public StatType StatType { get; }
        
        /// <summary>Gets the modifier containing the buff's effect details.</summary>
        public StatModifier Modifier { get; }

        /// <summary>
        /// Initializes a new instance of CharacterStatBuffEventArgs for buff application/removal.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <param name="statType">The type of stat being modified.</param>
        /// <param name="modifier">The modifier containing the buff's properties and values.</param>
        public CharacterStatBuffEventArgs(
            string characterId,
            StatType statType,
            StatModifier modifier) : base(characterId)
        {
            StatType = statType;
            Modifier = modifier;
        }
    }

    /// <summary>
    /// Event arguments for monitoring when character stats cross specific thresholds.
    /// This class is essential for triggering effects based on stat percentages.
    /// </summary>
    /// <remarks>
    /// Threshold events are used for:
    /// - Low health warnings and effects
    /// - Rage mechanics when health is below certain levels
    /// - Triggering special abilities at stat thresholds
    /// - Achievement tracking
    /// - UI warnings and notifications
    /// - AI behavior changes
    /// 
    /// The CrossingUp property helps determine if the character is:
    /// - Recovering (crossing up through threshold)
    /// - Deteriorating (crossing down through threshold)
    /// This enables different responses based on the direction of change.
    /// </remarks>
    public class CharacterStatThresholdEventArgs : CharacterEventArgs
    {
        /// <summary>Gets the type of stat that crossed the threshold.</summary>
        public StatType StatType { get; }
        
        /// <summary>Gets the threshold percentage that was crossed (0.0 to 1.0).</summary>
        public float ThresholdPercent { get; }
        
        /// <summary>Gets the current value of the stat when crossing the threshold.</summary>
        public float CurrentValue { get; }
        
        /// <summary>Gets whether the stat is increasing (true) or decreasing (false).</summary>
        public bool CrossingUp { get; }

        /// <summary>
        /// Initializes a new instance of CharacterStatThresholdEventArgs.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <param name="statType">The type of stat that crossed the threshold.</param>
        /// <param name="thresholdPercent">The threshold percentage that was crossed.</param>
        /// <param name="currentValue">The current value of the stat.</param>
        /// <param name="crossingUp">True if the stat is increasing, false if decreasing.</param>
        public CharacterStatThresholdEventArgs(
            string characterId,
            StatType statType,
            float thresholdPercent,
            float currentValue,
            bool crossingUp) : base(characterId)
        {
            StatType = statType;
            ThresholdPercent = thresholdPercent;
            CurrentValue = currentValue;
            CrossingUp = crossingUp;
        }
    }

    /// <summary>
    /// Event arguments for character level changes, tracking character progression.
    /// This class handles events triggered when a character gains or loses levels.
    /// </summary>
    /// <remarks>
    /// Level change events are crucial for:
    /// - Unlocking new abilities and features
    /// - Updating character stats
    /// - Triggering level-up effects and animations
    /// - Quest and achievement progress
    /// - UI updates and notifications
    /// - Analytics tracking
    /// 
    /// The event captures both old and new levels to:
    /// - Handle multi-level gains/losses
    /// - Trigger appropriate celebrations/effects
    /// - Update dependent systems (skills, stats, etc.)
    /// - Track progression metrics
    /// </remarks>
    public class CharacterLevelEventArgs : CharacterEventArgs
    {
        /// <summary>Gets the character's level before the change.</summary>
        public int OldLevel { get; }
        
        /// <summary>Gets the character's new level after the change.</summary>
        public int NewLevel { get; }

        /// <summary>
        /// Initializes a new instance of CharacterLevelEventArgs.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <param name="oldLevel">The character's level before the change.</param>
        /// <param name="newLevel">The character's new level after the change.</param>
        public CharacterLevelEventArgs(
            string characterId,
            int oldLevel,
            int newLevel) : base(characterId)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

	public class CharacterStatLevelEventArgs : CharacterEventArgs
	{
		/// <summary>Gets the character's level before the change.</summary>
		public int OldLevel { get; }

		/// <summary>Gets the character's new level after the change.</summary>
		public int NewLevel { get; }

        public StatType statType { get; }

		/// <summary>
		/// Initializes a new instance of CharacterLevelEventArgs.
		/// </summary>
		/// <param name="characterId">The unique identifier of the character.</param>
		/// <param name="oldLevel">The character's level before the change.</param>
		/// <param name="newLevel">The character's new level after the change.</param>
		public CharacterStatLevelEventArgs(
			string characterId,
			StatType stattype,
			int newLevel) : base(characterId)
		{
			NewLevel = newLevel;
			statType = stattype;
		}
	}
}