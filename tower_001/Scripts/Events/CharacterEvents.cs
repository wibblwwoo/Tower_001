using Godot;
using System;
using static GlobalEnums;

namespace Tower_001.Scripts.Events
{
    /// <summary>
    /// Event arguments for general character-related events.
    /// Provides the unique identifier of the character involved in the event.
    /// </summary>
    public class CharacterEventArgs : GameEventArgs
    {
        public string CharacterId { get; set; }

        public CharacterEventArgs(string characterId)
        {
            CharacterId = characterId;
        }
    }

    /// <summary>
    /// Event arguments for when a character's stat changes.
    /// Provides details about the stat type, old value, and new value.
    /// </summary>
    public class CharacterStatEventArgs : CharacterEventArgs
    {
        public StatType StatType { get; set; }
        public float OldValue { get; set; }
        public float NewValue { get; set; }

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
    /// Event arguments for the application or removal of a stat buff on a character.
    /// </summary>
    public class CharacterStatBuffEventArgs : CharacterEventArgs
    {
        public StatType StatType { get; }
        public StatModifier Modifier { get; }

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
    /// Event arguments for when a character's stat crosses a threshold.
    /// </summary>
    public class CharacterStatThresholdEventArgs : CharacterEventArgs
    {
        public StatType StatType { get; }
        public float ThresholdPercent { get; }
        public float CurrentValue { get; }
        public bool CrossingUp { get; }

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
    /// Event arguments for when a character's level changes.
    /// </summary>
    public class CharacterLevelEventArgs : CharacterEventArgs
    {
        public int OldLevel { get; }
        public int NewLevel { get; }

        public CharacterLevelEventArgs(
            string characterId,
            int oldLevel,
            int newLevel) : base(characterId)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }
}