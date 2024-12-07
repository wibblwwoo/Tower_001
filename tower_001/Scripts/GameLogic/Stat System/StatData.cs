using Godot;
using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem
{
    public partial class StatData
    {
        private readonly string _characterId;
        public float BaseValue { get; private set; }
        public float Multiplier { get; private set; }
        public float Experience { get; private set; }
        public int Level { get; private set; }

        public StatData(string characterId, float baseValue, float multiplier = 1.0f, float experience = 0, int level = 1)
        {
            _characterId = characterId;
            BaseValue = baseValue;
            Multiplier = multiplier;
            Experience = experience;
            Level = level;
        }

        public float CalculateCurrentValue() => BaseValue * Multiplier;

        public void SetBaseValue(float value) => BaseValue = value;
        public void SetMultiplier(float value) => Multiplier = value;
        public void SetExperience(float value) => Experience = value;
        public void SetLevel(int value) => Level = value;
    }
}
