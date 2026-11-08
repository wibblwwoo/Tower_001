using Godot;
using System;

/// <summary>
/// Event arguments for character rank changes
/// </summary>
public class CharacterRankEventArgs : GameEventArgs
{
    public string CharacterId { get; }
    public int OldRank { get; }
    public int NewRank { get; }
    public float NewMultiplier { get; }

    public CharacterRankEventArgs(string characterId, int oldRank, int newRank, float newMultiplier) : base()
    {
        CharacterId = characterId;
        OldRank = oldRank;
        NewRank = newRank;
        NewMultiplier = newMultiplier;
    }
}