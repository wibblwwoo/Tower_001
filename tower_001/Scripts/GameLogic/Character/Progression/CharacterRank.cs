using System;
using Tower_001.Scripts.GameLogic.Balance;
using static GlobalEnums;
using static Tower_001.Scripts.GameLogic.Balance.GameBalanceConfig.CharacterProgression;

/// <summary>
/// Handles character rank/star progression system
/// </summary>
public class CharacterRank
{
    private int _currentRank = 1;
    private readonly Character _character;
    private readonly EventManager _eventManager;

    public int CurrentRank => _currentRank;
    public bool CanRankUp => CanAdvanceRank();
    
    public CharacterRank(Character character)
    {
        _character = character;
        _eventManager = Globals.Instance.gameMangers.Events;
    }

    public bool TryRankUp()
    {
        if (!CanRankUp || _currentRank >= PowerCalculation.MaxRank)
            return false;

        var oldRank = _currentRank;
        _currentRank++;
        
        // Raise rank up event
        _eventManager.RaiseEvent(
            EventType.CharacterRankUp,
            new CharacterRankEventArgs(
                _character.Id,
                oldRank,
                _currentRank,
                GetRankMultiplier()
            )
        );

        UpdateCharacterStats();
        return true;
    }

    private bool CanAdvanceRank()
    {
        if (_currentRank >= PowerCalculation.MaxRank)
            return false;

        int requiredLevel = CalculateRequiredLevel();
        return _character.Level >= requiredLevel;
    }

    private int CalculateRequiredLevel()
    {
        return (int)(CharacterUnlock.BaseRankUpLevel * 
            Math.Pow(CharacterUnlock.RankUpLevelMultiplier, _currentRank - 1));
    }

    private void UpdateCharacterStats()
    {
        // Force power recalculation
        _character.UpdatePower();
    }

    public float GetRankMultiplier()
    {
        return 1 + ((_currentRank - 1) * PowerCalculation.BaseRankMultiplier);
    }

    public int GetNextRankRequiredLevel()
    {
        if (_currentRank >= PowerCalculation.MaxRank)
            return int.MaxValue;
            
        return CalculateRequiredLevel();
    }
}