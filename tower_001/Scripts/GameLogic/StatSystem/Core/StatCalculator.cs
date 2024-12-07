using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Core
{
    /// <summary>
    /// Handles all stat calculations and value processing
    /// </summary>
    public static class StatCalculator
    {
        public static float Calculate(StatData stat)
        {
            // Start with base value
            float calculatedValue = stat.BaseValue;
            
            // TODO: Phase 2 - Add modifier calculations here
            // This will be expanded when we implement the modifier system
            
            // Apply any game-specific rules based on stat type
            calculatedValue = ApplyStatTypeRules(calculatedValue, stat.StatType);
            
            return calculatedValue;
        }

        private static float ApplyStatTypeRules(float value, StatType statType)
        {
            // Apply any specific rules based on stat type
            switch (statType)
            {
                case StatType.Health:
                case StatType.Shield:
                case StatType.Energy:
                    // These stats cannot be negative
                    return Math.Max(0, value);
                    
                default:
                    return value;
            }
        }
    }
}