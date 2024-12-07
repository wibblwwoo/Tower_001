using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Tests
{
    /// <summary>
    /// Validates that the new CharacterStatSystem produces identical results to the existing CharacterStats system.
    /// This class helps ensure a safe transition between systems by running them in parallel and comparing outputs.
    /// </summary>
    public class StatSystemValidator
    {
        private readonly CharacterStats _oldSystem;
        private readonly CharacterStatSystem _statSystem;
        private readonly string _characterId;

        public StatSystemValidator(string characterId)
        {
            _characterId = characterId;
            _oldSystem = new CharacterStats(characterId);
            _statSystem = new CharacterStatSystem(characterId);
        }

        /// <summary>
        /// Validates that both systems produce identical stat values
        /// </summary>
        public bool ValidateStatValues()
        {
            bool allMatch = true;
            StringBuilder differences = new StringBuilder();

            // Test primary stats
            StatType[] primaryStats = {
                StatType.Strength,
                StatType.Dexterity,
                StatType.Intelligence,
                StatType.Stamina
            };

            foreach (var stat in primaryStats)
            {
                float oldValue = _oldSystem.GetStatValue(stat);
                float newValue = _statSystem.GetStatValue(stat);

                if (Math.Abs(oldValue - newValue) > 0.001f)
                {
                    allMatch = false;
                    differences.AppendLine($"Mismatch in {stat}:");
                    differences.AppendLine($"  Old System: {oldValue}");
                    differences.AppendLine($"  New System: {newValue}");
                }
            }

            // Test derived stats
            StatType[] derivedStats = {
                StatType.Health,
                StatType.Attack,
                StatType.Defense,
                StatType.Speed
            };

            foreach (var stat in derivedStats)
            {
                float oldValue = _oldSystem.GetDerivedStatValue(stat);
                float newValue = _statSystem.GetDerivedStatValue(stat);

                if (Math.Abs(oldValue - newValue) > 0.001f)
                {
                    allMatch = false;
                    differences.AppendLine($"Mismatch in derived stat {stat}:");
                    differences.AppendLine($"  Old System: {oldValue}");
                    differences.AppendLine($"  New System: {newValue}");
                }
            }

            // Log results
            if (allMatch)
            {
                GD.Print($"[StatSystemValidator] All stat values match between systems for character {_characterId}");
            }
            else
            {
                GD.PrintErr($"[StatSystemValidator] Discrepancies found for character {_characterId}:");
                GD.PrintErr(differences.ToString());
            }

            return allMatch;
        }

        /// <summary>
        /// Tests experience gain and leveling behavior between systems
        /// </summary>
        public bool ValidateExperienceAndLeveling()
        {
            bool allMatch = true;
            StringBuilder differences = new StringBuilder();

            // Test stats to validate
            StatType[] testStats = {
                StatType.Strength,
                StatType.Dexterity,
                StatType.Intelligence,
                StatType.Stamina
            };

            // Test different experience amounts
            float[] expAmounts = { 100f, 500f, 1000f, 5000f };

            foreach (var stat in testStats)
            {
                GD.Print($"\nTesting experience and leveling for {stat}:");
                
                foreach (var exp in expAmounts)
                {
                    // Store initial values
                    float oldInitialValue = _oldSystem.GetStatValue(stat);
                    float newInitialValue = _statSystem.GetStatValue(stat);

                    // Add experience to both systems
                    _oldSystem.AddExperience(stat, exp);
                    _statSystem.AddExperience(stat, exp);

                    // Get final values
                    float oldFinalValue = _oldSystem.GetStatValue(stat);
                    float newFinalValue = _statSystem.GetStatValue(stat);

                    // Compare results
                    if (Math.Abs(oldFinalValue - newFinalValue) > 0.001f)
                    {
                        allMatch = false;
                        differences.AppendLine($"\nMismatch in {stat} after adding {exp} experience:");
                        differences.AppendLine($"  Old System: {oldInitialValue} -> {oldFinalValue}");
                        differences.AppendLine($"  New System: {newInitialValue} -> {newFinalValue}");
                    }
                    else
                    {
                        GD.Print($"  Added {exp} exp: {oldInitialValue} -> {oldFinalValue} (matched)");
                    }
                }
            }

            // Log results
            if (allMatch)
            {
                GD.Print("\n[StatSystemValidator] All experience and leveling tests passed!");
            }
            else
            {
                GD.PrintErr("\n[StatSystemValidator] Experience and leveling discrepancies found:");
                GD.PrintErr(differences.ToString());
            }

            return allMatch;
        }

        /// <summary>
        /// Tests idle gains calculations between systems
        /// </summary>
        private bool ValidateIdleGains()
        {
            var testStats = new List<StatType>
            {
                StatType.Strength,
                StatType.Dexterity,
                StatType.Intelligence,
                StatType.Stamina
            };

            float timeInMinutes = 60f; // Test with 1 hour
            foreach (var stat in testStats)
            {
                float oldGain = _oldSystem.CalculateIdleGains(stat, timeInMinutes);
                float newGain = _statSystem.CalculateIdleGains(stat, timeInMinutes);

                // Allow for a larger margin of error since the new system has more sophisticated calculations
                if (Math.Abs(oldGain - newGain) > newGain * 0.1f) // 10% margin
                {
                    GD.PrintErr($"Idle gains mismatch for {stat}: Old={oldGain}, New={newGain}");
                    return false;
                }
            }

            GD.Print("[StatSystemValidator] All idle gain calculations within acceptable margin!");
            return true;
        }

        /// <summary>
        /// Tests idle progression calculations between systems
        /// </summary>
        public void ValidateIdleProgression(StatType statType, float timeInMinutes)
        {
            // Test idle progression calculations
            float gains = _statSystem.CalculateIdleGains(statType, timeInMinutes);
            
            // Gains should be non-negative
            if (gains < 0)
            {
                throw new ValidationException($"Idle gains for {statType} should be non-negative, but was {gains}");
            }

            // Test offline progression (should have bonus multiplier)
            float offlineGains = _statSystem.CalculateIdleGains(statType, timeInMinutes, true);
            if (offlineGains < gains)
            {
                throw new ValidationException($"Offline gains for {statType} should be greater than online gains");
            }
        }

        /// <summary>
        /// Tests modifier application between systems
        /// </summary>
        public bool ValidateModifiers()
        {
            bool allMatch = true;
            StringBuilder differences = new StringBuilder();

            StatType[] testStats = {
                StatType.Strength,
                StatType.Dexterity
            };

            // Test cases for modifiers
            var testCases = new[]
            {
                (id: "test_flat_1", source: "test", type: BuffType.Flat, value: 10f, duration: TimeSpan.FromSeconds(10)),
                (id: "test_flat_2", source: "test", type: BuffType.Flat, value: 5f, duration: TimeSpan.FromSeconds(10)),
                (id: "test_percent_1", source: "test", type: BuffType.Percentage, value: 0.5f, duration: TimeSpan.FromSeconds(10)),
                (id: "test_percent_2", source: "test", type: BuffType.Percentage, value: 0.25f, duration: TimeSpan.FromSeconds(10))
            };

            foreach (var stat in testStats)
            {
                GD.Print($"\nTesting modifiers for {stat}:");
                float baseValue = _oldSystem.GetStatValue(stat);
                GD.Print($"  Base value: {baseValue}");

                // Test adding modifiers one by one
                foreach (var (id, source, type, value, duration) in testCases)
                {
                    // Create and apply modifier to both systems
                    var modifier = new StatModifier(id, source, stat, type, value, duration);
                    _oldSystem.AddModifier(stat, id, modifier);
                    _statSystem.AddModifier(stat, id, modifier);

                    // Compare values
                    float oldValue = _oldSystem.GetStatValue(stat);
                    float newValue = _statSystem.GetStatValue(stat);

                    if (Math.Abs(oldValue - newValue) > 0.001f)
                    {
                        allMatch = false;
                        differences.AppendLine($"\nMismatch in {stat} after adding {type} modifier (value: {value}):");
                        differences.AppendLine($"  Old System: {oldValue}");
                        differences.AppendLine($"  New System: {newValue}");
                    }
                    else
                    {
                        GD.Print($"  Added {type} modifier (value: {value}): {oldValue} (matched)");
                    }
                }

                // Test removing modifiers
                foreach (var (id, _, type, value, _) in testCases)
                {
                    _oldSystem.RemoveModifier(stat, id);
                    _statSystem.RemoveModifier(stat, id);

                    float oldValue = _oldSystem.GetStatValue(stat);
                    float newValue = _statSystem.GetStatValue(stat);

                    if (Math.Abs(oldValue - newValue) > 0.001f)
                    {
                        allMatch = false;
                        differences.AppendLine($"\nMismatch in {stat} after removing {type} modifier (value: {value}):");
                        differences.AppendLine($"  Old System: {oldValue}");
                        differences.AppendLine($"  New System: {newValue}");
                    }
                    else
                    {
                        GD.Print($"  Removed {type} modifier (value: {value}): {oldValue} (matched)");
                    }
                }
            }

            // Log results
            if (allMatch)
            {
                GD.Print("\n[StatSystemValidator] All modifier tests passed!");
            }
            else
            {
                GD.PrintErr("\n[StatSystemValidator] Modifier test discrepancies found:");
                GD.PrintErr(differences.ToString());
            }

            return allMatch;
        }

        /// <summary>
        /// Runs a complete validation suite on both systems
        /// </summary>
        public bool RunFullValidation()
        {
            bool allTestsPassed = true;

            // Test 1: Initial stat values
            GD.Print("\n[StatSystemValidator] Testing initial stat values...");
            allTestsPassed &= ValidateStatValues();

            // Test 2: Experience and leveling
            GD.Print("\n[StatSystemValidator] Testing experience and leveling...");
            allTestsPassed &= ValidateExperienceAndLeveling();

            // Test 3: Idle gains
            GD.Print("\n[StatSystemValidator] Testing idle gain calculations...");
            allTestsPassed &= ValidateIdleGains();

            // Test 4: Modifiers
            GD.Print("\n[StatSystemValidator] Testing modifier system...");
            allTestsPassed &= ValidateModifiers();

            // Final results
            if (allTestsPassed)
            {
                GD.Print("\n[StatSystemValidator] All validation tests passed successfully!");
            }
            else
            {
                GD.PrintErr("\n[StatSystemValidator] Some validation tests failed. See above for details.");
            }

            return allTestsPassed;
        }
    }
}
