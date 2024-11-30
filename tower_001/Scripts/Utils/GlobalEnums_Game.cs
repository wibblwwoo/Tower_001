using Godot;
using System;

public static partial class GlobalEnums
{
	public enum StatType
	{
		Health,
		Mana,
		Attack,
		Defense,
		Speed
	}

	public enum BuffType
	{
		Flat,
		Percentage,
		Growth
	}
	public enum DisplayFormat
	{
		WholeNumber,
		Decimal,
		Percentage
	}

	public enum ResourceType
	{
		Gold,
		Pages
	}
	public enum ModifierType
	{
		Additive,
		Multiplicative,
		Override,
		Percentage,
		Exponential,
		Logarithmic,
		Min,
		Max,
		Complex
	}

	public enum ModifierCategory
	{
		General,
		Combat,
		Environment,
		Status,
		Equipment,
		Temporary,
		Permanent,
		Event,
		Achievement,
		Challenge
	}

	public enum ModifierTag
	{
		None = 0,
		Positive = 1,
		Negative = 2,
		Neutral = 3,
		Combat = 4,
		NonCombat = 5,
		Environmental = 6,
		Timed = 7,
		Conditional = 8,
		Stackable = 9,
		Unique = 10,
		Hidden = 11,
		Elemental = 12,
		Status = 13,
		Equipment = 14,
		Achievement = 15,
		Event = 16,
		Challenge = 17,
		Special = 18
	}
	public enum RewardTier
	{
		Common,
		Uncommon,
		Rare,
		Epic,
		Legendary
	}
}
