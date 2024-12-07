using System;
using System.Collections.Generic;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Character
{
    /// <summary>
    /// Represents a summary of a character's current state including stats, levels, and progression
    /// </summary>
    public class CharacterStateSummary
    {
        // Basic Info
        public string CharacterId { get; }
        public float TotalPower { get; set; }
        public float AveragePrimaryLevel { get; set; }
        
        // Primary Stats Summary
        public Dictionary<StatType, StatSummary> PrimaryStats { get; }
        
        // Derived Stats Summary
        public Dictionary<StatType, float> DerivedStats { get; }
        
        // Progression Info
        public float IdleTimeAccumulated { get; set; }
        public Dictionary<StatType, float> IdleGainsPerHour { get; }
        
        public CharacterStateSummary(string characterId)
        {
            CharacterId = characterId;
            PrimaryStats = new Dictionary<StatType, StatSummary>();
            DerivedStats = new Dictionary<StatType, float>();
            IdleGainsPerHour = new Dictionary<StatType, float>();
        }
    }

    /// <summary>
    /// Detailed information about a specific stat
    /// </summary>
    public class StatSummary
    {
        public float BaseValue { get; set; }
        public float CurrentValue { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float ExperienceToNextLevel { get; set; }
        public float ProgressPercentage => Experience / ExperienceToNextLevel * 100f;
        public List<ModifierSummary> ActiveModifiers { get; }

        public StatSummary()
        {
            ActiveModifiers = new List<ModifierSummary>();
        }
    }

    /// <summary>
    /// Summary of a stat modifier
    /// </summary>
    public class ModifierSummary
    {
        public ModifierSummary()
        {
            Id = string.Empty;
            Source = string.Empty;
            Type = ModifierType.Additive; // Default to Additive
            Value = 0;
            Duration = 0;  // Changed from null to 0 for non-nullable float
            TimeRemaining = 0;  // Changed from null to 0 for non-nullable float
            AffectedStat = StatType.None; // Initialize with default value
        }

        public string Id { get; set; }
        public string Source { get; set; }
        public ModifierType Type { get; set; }
        public StatType AffectedStat { get; set; }  // Add the StatType that this modifier affects
        public float Value { get; set; }
        public float Duration { get; set; }  // Changed from float? to float
        public float TimeRemaining { get; set; }  // Changed from float? to float

        public float ProgressPercentage => Duration > 0 
            ? (1 - TimeRemaining / Duration) * 100f 
            : 0f;
    }
}
