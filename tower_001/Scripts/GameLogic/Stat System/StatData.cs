using Godot;
using System;
using Tower_001.Scripts.Events;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem
{
    public partial class StatData
    {
        private readonly string _characterId;
        private float _baseValue;
        private float _multiplier;
        private float _experience;
        private int _level;
        private float _ascensionBonus;
        private bool _isDirty = true;
        private float _cachedValue;
        private float _lastValue;

        public float BaseValue => _baseValue;
        public float Multiplier => _multiplier;
        public float Experience => _experience;
        public int Level => _level;

        public StatType Type;

		public StatData(string characterId, float baseValue, float multiplier = 1.0f, float experience = 0, int level = 1)
        {
            _characterId = characterId;
            _baseValue = baseValue;
            _multiplier = multiplier;
            _experience = experience;
            _level = level;
            _ascensionBonus = 0f;
            _cachedValue = baseValue * multiplier;
            _lastValue = _cachedValue;
        }
		public StatData(string characterId, float baseValue, float multiplier = 1.0f, float experience = 0, int level = 1, StatType statType = default, Bonus.PermanentBonusRegistry bonusRegistry = null)
		{
			_characterId = characterId;
			_baseValue = baseValue;
			_multiplier = multiplier;
			_experience = experience;
			_level = level;
			_ascensionBonus = 0f;
			_cachedValue = baseValue * multiplier;
			_lastValue = _cachedValue;
			Type = statType;
		}

		public float CalculateCurrentValue()
        {
            if (!_isDirty) return _cachedValue;

            _lastValue = _cachedValue;
            _cachedValue = _baseValue * (_multiplier + _ascensionBonus);
            _isDirty = false;

            if (Math.Abs(_lastValue - _cachedValue) > 0.001f)
            {
                RaiseStatChangedEvent(_lastValue, _cachedValue);
            }

            return _cachedValue;
        }

        public void SetBaseValue(float value)
        {
            if (Math.Abs(_baseValue - value) < 0.001f) return;
            _baseValue = value;
            _isDirty = true;
        }

        public void SetMultiplier(float value)
        {
            if (Math.Abs(_multiplier - value) < 0.001f) return;
            _multiplier = value;
            _isDirty = true;
        }

        public void SetExperience(float value) => _experience = value;
        public void SetLevel(int value) => _level = value;

        public void UpdateAscensionBonus(float bonus)
        {
            if (Math.Abs(_ascensionBonus - bonus) < 0.001f) return;
            _ascensionBonus = bonus;
            _isDirty = true;
        }

        private void RaiseStatChangedEvent(float oldValue, float newValue)
        {
            Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                EventType.CharacterStatChanged,
                new CharacterStatEventArgs(_characterId, Type, oldValue, newValue)
            );
        }
        public void MarkDirty()
        {
            _isDirty = true;
        }

	}
}
