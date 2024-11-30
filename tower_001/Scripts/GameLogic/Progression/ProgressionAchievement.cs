using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Represents a progression-related achievement
/// </summary>
public class ProgressionAchievement
{
	public string Id { get; set; }
	public ProgressionAchievementType Type { get; set; }
	public float TargetValue { get; set; }
	public Dictionary<string, float> Rewards { get; set; }
}