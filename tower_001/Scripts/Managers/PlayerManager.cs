using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Globalization;
using Tower_001.Scripts.Events;
using Tower_001.Scripts.GameLogic.StatSystem;
using Tower_001.Scripts.GameLogic.Character;

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

    public float GetStatValue(string characterId, StatType statType)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return character.GetStatValue(statType);
        }
        return 0f;
    }

    public Dictionary<StatType, float> GetCharacterStats(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var stats = new Dictionary<StatType, float>();
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                stats[statType] = character.GetStatValue(statType);
            }
            return stats;
        }
        return new Dictionary<StatType, float>();
    }

    private void ApplyBuffInternal(string characterId, StatModifier modifier)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            character.AddModifier(modifier.StatType, modifier.Id, modifier);
            
            // Raise buff applied event
            EventManager.RaiseEvent(
                EventType.CharacterStatBuffApplied,
                new CharacterStatBuffEventArgs(characterId, modifier.StatType, modifier)
            );
        }
    }

    public void ApplyBuff(string characterId, StatType statType, string source, BuffType buffType, float value, TimeSpan? duration = null)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var modifier = new StatModifier(
                Guid.NewGuid().ToString(),
                source,
                statType,
                buffType,
                value,
                duration
            );

            ApplyBuffInternal(characterId, modifier);
        }
    }

    public void ApplyBuff(string characterId, StatType statType, string source, BuffType buffType, float value, string buffId, TimeSpan? duration = null)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var modifier = new StatModifier(
                buffId,
                source,
                statType,
                buffType,
                value,
                duration
            );

            ApplyBuffInternal(characterId, modifier);
        }
    }

    public void RemoveBuff(string characterId, StatType statType, string buffId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            character.RemoveModifier(statType, buffId);
            
            // Raise buff removed event
            EventManager.RaiseEvent(
                EventType.CharacterStatBuffRemoved,
                new CharacterStatBuffEventArgs(characterId, statType, null)
            );
        }
    }

    // Overload that removes all modifiers with the given buffId regardless of stat type
    public void RemoveBuff(string characterId, string buffId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            var modifiers = character.GetStatModifiers(StatType.Attack)
                .Concat(character.GetStatModifiers(StatType.Defense))
                .Concat(character.GetStatModifiers(StatType.Health))
                .Concat(character.GetStatModifiers(StatType.Speed))
                .Where(m => m.Id == buffId)
                .ToList();

            foreach (var modifier in modifiers)
            {
                character.RemoveModifier(modifier.AffectedStat, modifier.Id);
                
                // Convert ModifierSummary to StatModifier for the event
                var statModifier = new StatModifier(
                    modifier.Id,
                    modifier.Source,
                    modifier.AffectedStat,
                    (BuffType)modifier.Type,  // Convert ModifierType to BuffType
                    modifier.Value,
                    modifier.Duration > 0 ? TimeSpan.FromSeconds(modifier.Duration) : null
                );

                // Raise buff removed event
                EventManager.RaiseEvent(
                    EventType.CharacterStatBuffRemoved,
                    new CharacterStatBuffEventArgs(characterId, modifier.AffectedStat, statModifier)
                );
            }
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

    private void OnCharacterStatBuffApplied(CharacterStatBuffEventArgs args)
    {
        if (_characters.TryGetValue(args.CharacterId, out var character))
        {
            DebugLogger.Log(
                $"Character {args.CharacterId} received {args.Modifier.Value} {args.StatType} buff from {args.Modifier.Source}",
                DebugLogger.LogCategory.Stats
            );
        }
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

    public IReadOnlyList<StatModifier> GetStatModifiers(string characterId, StatType statType)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return (IReadOnlyList<StatModifier>)character.GetStatModifiers(statType);
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

    public float GetCharacterPower(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return character.GetPowerLevel();
        }
        throw new ArgumentException($"Character not found: {characterId}");
    }

    public float GetBaseStatValue(string characterId, StatType statType)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return character.GetBaseStatValue(statType);
        }
        throw new ArgumentException($"Character not found: {characterId}");
    }

    /// <summary>
    /// Resets all stats for a character to their initial values from GameBalanceConfig
    /// </summary>
    public void ResetCharacterStats(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            character.ResetStats();
        }
        else
        {
            throw new ArgumentException($"Character not found: {characterId}");
        }
    }

    public void Update()
    {
        foreach (var character in _characters.Values)
        {
            character.Update();
        }
    }

    /// <summary>
    /// Process idle progression for a character
    /// </summary>
    /// <param name="characterId">ID of the character to process</param>
    /// <param name="timeInMinutes">Time elapsed in minutes</param>
    /// <param name="isOffline">Whether this is offline progression</param>
    public void ProcessCharacterIdleProgression(string characterId, float timeInMinutes, bool isOffline = false)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            character.ProcessIdleProgression(timeInMinutes, isOffline);
        }
        else
        {
            throw new ArgumentException($"Character not found: {characterId}");
        }
    }

    /// <summary>
    /// Process idle progression for all characters
    /// </summary>
    /// <param name="timeInMinutes">Time elapsed in minutes</param>
    /// <param name="isOffline">Whether this is offline progression</param>
    public void ProcessAllCharactersIdleProgression(float timeInMinutes, bool isOffline = false)
    {
        foreach (var character in _characters.Values)
        {
            character.ProcessIdleProgression(timeInMinutes, isOffline);
        }
    }

    /// <summary>
    /// Gets a summary of all characters' states
    /// </summary>
    public IEnumerable<CharacterStateSummary> GetAllCharacterSummaries()
    {
        return _characters.Values.Select(c => c.GetStateSummary());
    }

    /// <summary>
    /// Gets a summary for a specific character
    /// </summary>
    public CharacterStateSummary GetCharacterSummary(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return character.GetStateSummary();
        }
        return null;
    }

    /// <summary>
    /// Gets characters sorted by their power level
    /// </summary>
    public IEnumerable<Character> GetCharactersByPower()
    {
        return _characters.Values.OrderByDescending(c => c.GetPowerLevel());
    }

    /// <summary>
    /// Gets characters sorted by their average level
    /// </summary>
    public IEnumerable<Character> GetCharactersByLevel()
    {
        return _characters.Values.OrderByDescending(c => c.GetAverageLevel());
    }
}