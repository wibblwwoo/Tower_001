using Godot;
using System;
using Tower_001.Scripts.Events;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.StatSystem.Core
{
    /// <summary>
    /// Core stat data container with integration into the game's event system
    /// </summary>
    public partial class StatData
    {
        private readonly string _characterId;
        private readonly StatType _statType;
        private float _baseValue;
        private bool _isDirty = true;
        private float _cachedValue;
        private float _lastValue;
		private float _experience;
		private int _level;
		private float _ascensionBonus;
		private float _multiplier;

        public float Multiplier => _multiplier;
        
        public float Experience => _experience;
		public int Level => _level;


		public string CharacterId => _characterId;
        public StatType StatType => _statType;
        public float BaseValue => _baseValue;
        public float CurrentValue
        {
            get
            {
                if (!_isDirty) 
                    return _cachedValue;

                _lastValue = _cachedValue;
                _cachedValue = StatCalculator.Calculate(this);
                _isDirty = false;

                if (Math.Abs(_lastValue - _cachedValue) > 0.001f)
                {
                    RaiseStatChangedEvent(_lastValue, _cachedValue);
                }

                return _cachedValue;
            }
        }

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
            this._statType = statType;
        }
        public StatData(string characterId, StatType statType, float baseValue = 0)
        {
            _characterId = characterId;
            _statType = statType;
            _baseValue = baseValue;
            _cachedValue = baseValue;
            _lastValue = baseValue;
        }

        public void SetBaseValue(float value)
        {
            if (Math.Abs(_baseValue - value) < 0.001f) 
                return;

            float oldValue = CurrentValue;
            _baseValue = value;
            MarkDirty();

            float newValue = CurrentValue;
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                RaiseStatChangedEvent(oldValue, newValue);
            }
        }

        public void MarkDirty()
        {
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

		private void RaiseStatChangedEvent(float oldValue, float newValue)
        {
            Globals.Instance?.gameMangers?.Events?.RaiseEvent(
                EventType.CharacterStatChanged,
                new CharacterStatEventArgs(_characterId, _statType, oldValue, newValue)
            );
        }
    }
}