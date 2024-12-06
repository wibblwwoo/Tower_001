using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines level and progression requirements for accessing tower content
/// </summary>
/// <remarks>
/// Dependencies and Usage:
/// - Used by: TowerManager, FloorManager, RoomManager
/// - Related Systems: Character progression, Achievement system, Tower progression
/// - Configuration: Uses GameBalanceConfig for level scaling and difficulty calculations
/// - Data Flow: Validates character requirements before allowing access to tower content
/// </remarks>
public class LevelRequirement
{
    /// <summary>
    /// Minimum character level required to attempt this content
    /// </summary>
    public int MinLevel { get; set; }

    /// <summary>
    /// Maximum level cap for this content (0 means no cap)
    /// </summary>
    public int MaxLevel { get; set; }

    /// <summary>
    /// Required character stats (Key: stat name, Value: minimum value needed)
    /// </summary>
    public Dictionary<string, int> StatRequirements { get; set; } = new();

    /// <summary>
    /// List of achievements that must be completed before accessing this content
    /// </summary>
    public List<string> AchievementRequirements { get; set; } = new();

    /// <summary>
    /// List of towers that must be completed before accessing this content
    /// </summary>
    public List<string> PrerequisiteTowers { get; set; } = new();

    /// <summary>
    /// The ideal level for attempting this content
    /// </summary>
    public int RecommendedLevel { get; set; }

    /// <summary>
    /// Modifiers that affect difficulty calculation (Key: modifier type, Value: modifier value)
    /// </summary>
    public Dictionary<string, float> DifficultyModifiers { get; set; } = new();

    /// <summary>
    /// Whether the difficulty scales with character level
    /// </summary>
    public bool IsScaling { get; set; }

    /// <summary>
    /// Rate at which difficulty increases per level difference (default: 1.0 = 100% per level)
    /// </summary>
    public float ScalingFactor { get; set; } = 1.0f;

    /// <summary>
    /// Checks if a character meets all requirements to access this content
    /// </summary>
    /// <param name="character">The character to check requirements against</param>
    /// <returns>True if all requirements are met, false otherwise</returns>
    public bool MeetsRequirements(Character character)
    {
        if (character.Level < MinLevel)
            return false;

        //foreach (var statReq in StatRequirements)
        //{
        //    var stat = character.GetStat(statReq.Key);
        //    if (stat == null || stat.CurrentValue < statReq.Value)
        //        return false;
        //}

        //foreach (var achievement in AchievementRequirements)
        //{
        //    if (!character.HasAchievement(achievement))
        //        return false;
        //}

        //foreach (var tower in PrerequisiteTowers)
        //{
        //    if (!character.HasCompletedTower(tower))
        //        return false;
        //}

        return true;
    }

    /// <summary>
    /// Calculates the difficulty multiplier based on character level
    /// </summary>
    /// <param name="characterLevel">Current level of the character</param>
    /// <returns>Difficulty multiplier (1.0 = normal difficulty)</returns>
    public float GetDifficultyForLevel(int characterLevel)
    {
        if (!IsScaling)
            return 1.0f;

        float levelDifference = characterLevel - RecommendedLevel;
        return 1.0f + (levelDifference * ScalingFactor);
    }

    //public Dictionary<string, object> GetRequirementState(Character character)
    //{
    //    return new Dictionary<string, object>
    //    {
    //        { "MeetsLevelRequirement", character.Level >= MinLevel && character.Level <= MaxLevel },
    //        { "MeetsStatRequirements", StatRequirements.All(sr => character.GetStat(sr.Key)?.CurrentValue >= sr.Value) },
    //        { "MeetsAchievements", AchievementRequirements.All(a => character.HasAchievement(a)) },
    //        { "MeetsTowerRequirements", PrerequisiteTowers.All(t => character.HasCompletedTower(t)) }
    //    };
    //}
}