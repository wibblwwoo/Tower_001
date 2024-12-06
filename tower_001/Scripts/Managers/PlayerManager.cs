using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Globalization;
using Tower_001.Scripts.Events;

public partial class PlayerManager : BaseManager
{
    private readonly Dictionary<string, Character> _characters = new();
    private Character _currentCharacter;
    public ProgressionManager progressionManager { get; set; }

    public override IEnumerable<Type> Dependencies => new[]
    {
        typeof(EventManager),
        typeof(UIManager)  // For UI updates
    };

    public PlayerManager()
    {
    }

    public override void Setup()
    {
        base.Setup(); 
    }

    protected override void RegisterEventHandlers()
    {
        progressionManager = new ProgressionManager(this);
        progressionManager.Setup();

        EventManager.AddHandler<CharacterStatEventArgs>(EventType.CharacterStatChanged, OnCharacterStatChanged);
        EventManager.AddHandler<CharacterStatBuffEventArgs>(EventType.CharacterStatBuffApplied, OnCharacterStatBuffApplied);

        // Register stat change handler
        EventManager.AddHandler<CharacterStatEventArgs>(
            EventType.CharacterStatChanged,
            OnCharacterStatChanged
        );

        // Register buff handlers
        EventManager.AddHandler<CharacterStatBuffEventArgs>(
            EventType.CharacterStatBuffApplied,
            OnCharacterStatBuffApplied
        );
        EventManager.AddHandler<CharacterStatBuffEventArgs>(
            EventType.CharacterStatBuffRemoved,
            OnCharacterStatBuffRemoved
        );
        EventManager.AddHandler<CharacterStatBuffEventArgs>(
            EventType.CharacterStatBuffExpired,
            OnCharacterStatBuffExpired
        );

        // Register threshold handler
        EventManager.AddHandler<CharacterStatThresholdEventArgs>(
            EventType.CharacterStatThresholdReached,
            OnCharacterStatThresholdCrossed
        );
    }

    public Character GetCharacter(string characterId)
    {
        return _characters.TryGetValue(characterId, out var character) ? character : null;
    }

    public string CreateCharacter(string className)
    {
        Character character = className.ToLower() switch
        {
            "knight" => new Knight(),
            "mage" => new Mage(),
            _ => throw new ArgumentException($"Unknown character class: {className}")
        };

        _characters[character.Id] = character;
        return character.Id;
    }

    private void ApplyBuffInternal(Character character, StatType statType, StatModifier modifier)
    {
        var stat = character.GetStat(statType);
        stat.AddModifier(modifier);
    }

    public void ApplyBuff(string characterId, StatType statType, string source, BuffType buffType, float value, TimeSpan? duration = null)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var modifier = new StatModifier(
                Guid.NewGuid().ToString(),
                source,
                buffType,
                value,
                duration
            );

            ApplyBuffInternal(character, statType, modifier);
        }
    }

    public void ApplyBuff(string characterId, StatType statType, string source, BuffType buffType, float value, string buffId, TimeSpan? duration = null)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var modifier = new StatModifier(
                buffId,
                source,
                buffType,
                value,
                duration
            );

            ApplyBuffInternal(character, statType, modifier);
        }
    }

    public void RemoveBuff(string characterId, StatType statType, string buffId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var stat = character.GetStat(statType);
            stat.RemoveModifier(buffId);
        }
    }

    public void Update()
    {
        foreach (var character in _characters.Values)
        {
            character.Update();
        }
    }

    private void OnCharacterStatChanged(CharacterStatEventArgs args)
    {
        if (_characters.TryGetValue(args.CharacterId, out var character))
        {
            DebugLogger.Log(
                $"{character.Name} ({args.CharacterId}): {args.StatType} changed from {args.OldValue:F1} to {args.NewValue:F1}",
                DebugLogger.LogCategory.Stats
            );
        }
    }

    public Dictionary<StatType, float> GetCharacterStats(string characterId)
    {
        if (!_characters.TryGetValue(characterId, out var character))
        {
            return new Dictionary<StatType, float>();
        }

        return Enum.GetValues<StatType>()
            .ToDictionary(
                statType => statType,
                statType => character.GetStat(statType)?.CurrentValue ?? 0
            );
    }

    private void OnCharacterStatBuffApplied(CharacterStatBuffEventArgs args)
    {
        DebugLogger.Log(
            $"Character {args.CharacterId} received {args.Modifier.Value} {args.StatType} buff from {args.Modifier.Source}",
            DebugLogger.LogCategory.Stats
        );
    }

    public void PrintCharacterStats(string characterId)
    {
        if (!_characters.TryGetValue(characterId, out var character))
            return;

        GD.Print($"=== {character.Name} ({characterId}) Stats ===");

        foreach (var stat in GetCharacterStats(characterId))
        {
            GD.Print($"{stat.Key}: {stat.Value:F1}");
        }
    }

    public float GetStatValue(StatType statType)
    {
        return _currentCharacter?.GetStat(statType)?.CurrentValue ?? 0;
    }

    public IReadOnlyList<StatModifier> GetStatModifiers(string characterId, StatType statType)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var stat = character.GetStat(statType);
            return (IReadOnlyList<StatModifier>)stat.GetActiveModifiers();
        }

        return new List<StatModifier>();
    }

    public bool SetCurrentCharacter(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            _currentCharacter = character;
            return true;
        }
        return false;
    }

    public Character GetCurrentCharacter()
    {
        return _currentCharacter;
    }

    public bool SetCharacterLevel(string characterId, int level)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            character.LevelUp(level);
            return true;
        }
        return false;
    }

    private void RaiseCharacterStatEvent(string characterId, StatType statType, float oldValue, float newValue, EventType eventType)
    {
        if (Globals.Instance?.gameMangers?.Events != null)
        {
            var args = new CharacterStatEventArgs(characterId, statType, oldValue, newValue);
            Globals.Instance.gameMangers.Events.RaiseEvent(eventType, args);
        }
    }

    public bool SetCharacterElement(string characterId, ElementType element)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            ElementType oldElement = character.Element;
            character.Element = element;
            RaiseCharacterStatEvent(characterId, StatType.Health, (float)oldElement, (float)element, EventType.CharacterStatChanged);
            return true;
        }
        return false;
    }

    private void OnCharacterStatBuffRemoved(CharacterStatBuffEventArgs args)
    {
        if (_characters.TryGetValue(args.CharacterId, out var character))
        {
            DebugLogger.Log(
                $"{character.Name} had {args.StatType} buff from {args.Modifier.Source} removed",
                DebugLogger.LogCategory.Stats
            );
        }
    }

    private void OnCharacterStatBuffExpired(CharacterStatBuffEventArgs args)
    {
        if (_characters.TryGetValue(args.CharacterId, out var character))
        {
            DebugLogger.Log(
                $"{character.Name} {args.StatType} buff from {args.Modifier.Source} expired",
                DebugLogger.LogCategory.Stats
            );
        }
    }

    private void OnCharacterStatThresholdCrossed(CharacterStatThresholdEventArgs args)
    {
        if (_characters.TryGetValue(args.CharacterId, out var character))
        {
            string direction = args.CrossingUp ? "above" : "below";
            DebugLogger.Log(
                $"{character.Name} {args.StatType} crossed {args.ThresholdPercent}% threshold ({direction}). Current value: {args.CurrentValue:F1}",
                DebugLogger.LogCategory.Stats
            );
        }
    }
}