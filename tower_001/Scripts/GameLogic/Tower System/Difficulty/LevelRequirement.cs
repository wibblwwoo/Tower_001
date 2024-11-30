using Godot;
using System;
using System.Collections.Generic;
public class LevelRequirement
{
	public int MinLevel { get; set; }
	public int MaxLevel { get; set; }
	public Dictionary<string, int> StatRequirements { get; set; } = new();
	public List<string> AchievementRequirements { get; set; } = new();
	public List<string> PrerequisiteTowers { get; set; } = new();
	public int RecommendedLevel { get; set; }
	public Dictionary<string, float> DifficultyModifiers { get; set; } = new();
	public bool IsScaling { get; set; }
	public float ScalingFactor { get; set; } = 1.0f;

	public bool MeetsRequirements(Character character)
	{
		if (character.Level < MinLevel)
			return false;

		//foreach (var statReq in StatRequirements)
		//{
		//	var stat = character.GetStat(statReq.Key);
		//	if (stat == null || stat.CurrentValue < statReq.Value)
		//		return false;
		//}

		//foreach (var achievement in AchievementRequirements)
		//{
		//	if (!character.HasAchievement(achievement))
		//		return false;
		//}

		//foreach (var tower in PrerequisiteTowers)
		//{
		//	if (!character.HasCompletedTower(tower))
		//		return false;
		//}

		return true;
	}

	public float GetDifficultyForLevel(int characterLevel)
	{
		if (!IsScaling)
			return 1.0f;

		float levelDifference = characterLevel - RecommendedLevel;
		return 1.0f + (levelDifference * ScalingFactor);
	}

	//public Dictionary<string, object> GetRequirementState(Character character)
	//{
	//	return new Dictionary<string, object>
	//	{
	//		{ "MeetsLevelRequirement", character.Level >= MinLevel && character.Level <= MaxLevel },
	//		{ "MeetsStatRequirements", StatRequirements.All(sr => character.GetStat(sr.Key)?.CurrentValue >= sr.Value) },
	//		{ "MeetsAchievements", AchievementRequirements.All(a => character.HasAchievement(a)) },
	//		{ "MeetsTowerRequirements", PrerequisiteTowers.All(t => character.HasCompletedTower(t)) }
	//	};
	//}
}