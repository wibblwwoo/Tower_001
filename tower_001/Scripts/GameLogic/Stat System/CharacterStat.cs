using System;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.Events;
using static GlobalEnums;

/// <summary>
/// Represents a single character statistic with support for modifiers, buffs, and debuffs.
/// </summary>
public class CharacterStat
{
    private readonly string _characterId;
    private readonly StatType _statType;
    private float _baseValue;
    private float _growthRate;
    private readonly Dictionary<string, StatModifier> _modifiers = new();
    private readonly List<float> _thresholds;

    private bool _isDirty = true;
    private float _cachedValue;
    private float _lastValue;
    private float _ascensionBonus = 0f;

    public float BaseValue => _baseValue;

    public float CurrentValue
    {
        get
        {
            if (!_isDirty) return _cachedValue;
            
            _lastValue = _cachedValue;
            float flatBonus = 0f;
            float percentageBonus = _ascensionBonus;

            foreach (var mod in _modifiers.Values.Where(m => !m.IsExpired))
            {
                switch (mod.Type)
                {
                    case BuffType.Flat:
                        flatBonus += mod.Value;
                        break;
                    case BuffType.Percentage:
                        percentageBonus += mod.Value;
                        break;
                }
            }

            _cachedValue = (_baseValue + flatBonus) * (1 + percentageBonus);
            _cachedValue = Math.Max(0, _cachedValue);
            _isDirty = false;

            if (Math.Abs(_lastValue - _cachedValue) > 0.001f)
            {
                CheckThresholds(_lastValue, _cachedValue);
            }

            return _cachedValue;
        }
    }

    public CharacterStat(string characterId, StatType statType, float baseValue, float growthRate, List<float> thresholds = null)
    {
        _characterId = characterId;
        _statType = statType;
        _baseValue = baseValue;
        _growthRate = growthRate;
        _thresholds = thresholds ?? new List<float>();
        _lastValue = baseValue;
        _cachedValue = baseValue;
    }

    public void UpdateAscensionBonus(string statName, long ascensionLevel, float bonus)
    {
        float oldValue = CurrentValue;
        _ascensionBonus = bonus;
        _isDirty = true;

        float newValue = CurrentValue;
        if (Math.Abs(oldValue - newValue) > 0.001f)
        {
            RaiseStatChangedEvent(oldValue, newValue);
        }
    }

    public void OnLevelUp(int newLevel, int levelDifference)
    {
        if (levelDifference <= 0) return;
        
        float oldValue = _baseValue;
        _baseValue += _baseValue * (_growthRate * levelDifference);
        _isDirty = true;

        float newValue = CurrentValue;
        RaiseStatChangedEvent(oldValue, newValue);
    }

    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null) return;

        float oldValue = CurrentValue;
        _modifiers[modifier.Id] = modifier;
        _isDirty = true;

        float newValue = CurrentValue;
        if (Math.Abs(oldValue - newValue) > 0.001f)
        {
            RaiseStatChangedEvent(oldValue, newValue);
        }
    }

    public void RemoveModifier(string modifierId)
    {
        if (string.IsNullOrEmpty(modifierId)) return;

        float oldValue = CurrentValue;
        if (_modifiers.Remove(modifierId))
        {
            _isDirty = true;
            float newValue = CurrentValue;
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                RaiseStatChangedEvent(oldValue, newValue);
            }
        }
    }

    public IReadOnlyList<StatModifier> GetActiveModifiers()
    {
        return _modifiers.Values.Where(m => !m.IsExpired).ToList();
    }

    public void Update()
    {
        var expiredMods = _modifiers.Values
            .Where(m => m.IsExpired)
            .Select(m => m.Id)
            .ToList();

        if (expiredMods.Any())
        {
            float oldValue = CurrentValue;
            foreach (var id in expiredMods)
            {
                _modifiers.Remove(id);
            }
            _isDirty = true;
            float newValue = CurrentValue;
            if (Math.Abs(oldValue - newValue) > 0.001f)
            {
                RaiseStatChangedEvent(oldValue, newValue);
            }
        }
    }

    private void CheckThresholds(float oldValue, float newValue)
    {
        if (_thresholds == null || !_thresholds.Any()) return;
        
        float baseForPercentage = _baseValue;
        
        foreach (float thresholdPercent in _thresholds)
        {
            float thresholdValue = baseForPercentage * (thresholdPercent / 100f);
            
            if (oldValue >= thresholdValue && newValue < thresholdValue)
            {
                RaiseThresholdEvent(thresholdPercent, newValue, false);
            }
            else if (oldValue < thresholdValue && newValue >= thresholdValue)
            {
                RaiseThresholdEvent(thresholdPercent, newValue, true);
            }
        }
    }

    private void RaiseStatChangedEvent(float oldValue, float newValue)
    {
        if (Math.Abs(oldValue - newValue) < 0.001f) return;
        
        Globals.Instance?.gameMangers?.Events?.RaiseEvent(
            EventType.CharacterStatChanged,
            new CharacterStatEventArgs(_characterId, _statType, oldValue, newValue)
        );
    }

    private void RaiseThresholdEvent(float thresholdPercent, float newValue, bool crossingUp)
    {
        Globals.Instance?.gameMangers?.Events?.RaiseEvent(
            EventType.CharacterStatThresholdReached,
            new CharacterStatThresholdEventArgs(
                _characterId,
                _statType,
                thresholdPercent,
                newValue,
                crossingUp
            )
        );
    }
}