using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;
/// <summary>
/// Represents a modifier applied to difficulty calculations, supporting additive, multiplicative, and complex adjustments.
/// Includes support for conditions and expiration logic.
/// </summary>
public class DifficultyModifier
{
	/// <summary>
	/// The type of modification (e.g., Additive, Multiplicative, Percentage).
	/// </summary>
	public ModifierType Type { get; set; }

	/// <summary>
	/// The value of the modifier (e.g., +10 for additive, x1.2 for multiplicative).
	/// </summary>
	public float Value { get; set; }

	/// <summary>
	/// The source of the modifier (e.g., ability, item, or system).
	/// Useful for tracking the origin of the modifier.
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// Indicates whether this modifier is temporary or permanent.
	/// Temporary modifiers have a defined expiration time.
	/// </summary>
	public bool IsTemporary { get; set; }

	/// <summary>
	/// The expiration time for the modifier, if applicable.
	/// Null if the modifier is permanent.
	/// </summary>
	public DateTime? ExpirationTime { get; set; }

	/// <summary>
	/// The priority of the modifier, used to determine the order in which sub-modifiers are applied.
	/// Lower values indicate higher priority.
	/// </summary>
	public int Priority { get; set; }

	/// <summary>
	/// Indicates whether this modifier is currently active.
	/// Inactive modifiers are ignored during calculations.
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// A set of conditions required for the modifier to be applied.
	/// Conditions are represented as key-value pairs.
	/// </summary>
	public Dictionary<string, object> Conditions { get; set; } = new();

	/// <summary>
	/// A description of the modifier, useful for debugging or displaying information to players.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The category of the modifier (e.g., combat, exploration).
	/// Useful for organizing and filtering modifiers.
	/// </summary>
	public ModifierCategory Category { get; set; }

	/// <summary>
	/// A set of tags associated with the modifier, allowing for additional categorization or filtering.
	/// </summary>
	public HashSet<ModifierTag> Tags { get; set; } = new();

	/// <summary>
	/// Special values used for complex modification logic.
	/// </summary>
	private Dictionary<string, float> _specialValues = new();

	/// <summary>
	/// A list of sub-modifiers that can be applied in addition to this modifier.
	/// Sub-modifiers are applied in order of their priority.
	/// </summary>
	private List<DifficultyModifier> _subModifiers = new();

	public DifficultyModifier()
	{
	}

	/// <summary>
	/// Constructor for creating a new DifficultyModifier with specified type, value, and optional source.
	/// </summary>
	/// <param name="type">The type of the modifier (e.g., Additive, Multiplicative).</param>
	/// <param name="value">The value of the modifier.</param>
	/// <param name="source">The source of the modifier.</param>
	public DifficultyModifier(ModifierType type, float value, string source = null)
	{
		Type = type;
		Value = value;
		Source = source;
	}

	/// <summary>
	/// Applies the modifier to the current difficulty value.
	/// </summary>
	/// <param name="currentDifficulty">The current difficulty value to be modified.</param>
	/// <returns>The modified difficulty value.</returns>
	public float Apply(float currentDifficulty)
	{
		// Skip inactive or expired modifiers
		if (!IsActive || (ExpirationTime.HasValue && DateTime.UtcNow > ExpirationTime.Value))
			return currentDifficulty;

		// Apply the modifier based on its type
		float result = Type switch
		{
			ModifierType.Additive => currentDifficulty + Value,
			ModifierType.Multiplicative => currentDifficulty * Value,
			ModifierType.Override => Value,
			ModifierType.Percentage => currentDifficulty * (1 + Value),
			ModifierType.Exponential => (float)(Math.Pow(currentDifficulty, Value)),
			ModifierType.Logarithmic => (float)(Math.Log(currentDifficulty + 1) * Value),
			ModifierType.Min => Math.Max(currentDifficulty, Value),
			ModifierType.Max => Math.Min(currentDifficulty, Value),
			ModifierType.Complex => ApplyComplexModification(currentDifficulty),
			_ => currentDifficulty
		};

		// Apply any sub-modifiers in order of priority
		foreach (var subModifier in _subModifiers.OrderBy(m => m.Priority))
		{
			result = subModifier.Apply(result);
		}

		return result;
	}


	/// <summary>
	/// Applies complex modification logic using special values.
	/// </summary>
	/// <param name="currentDifficulty">The current difficulty value to be modified.</param>
	/// <returns>The modified difficulty value.</returns>
	private float ApplyComplexModification(float currentDifficulty)
	{
		float result = currentDifficulty;

		// Iterate through special values and apply modification logic
		foreach (var specialValue in _specialValues)
		{
			switch (specialValue.Key)
			{
				case "scalingFactor":
					result *= (1 + (specialValue.Value * currentDifficulty));
					break;
				case "threshold":
					if (currentDifficulty > specialValue.Value)
						result *= Value;
					break;
				case "cap":
					result = Math.Min(result, specialValue.Value);
					break;
				case "floor":
					result = Math.Max(result, specialValue.Value);
					break;
			}
		}

		return result;
	}

	/// <summary>
	/// Adds a special value used for complex modification logic.
	/// </summary>
	/// <param name="key">The name of the special value.</param>
	/// <param name="value">The value to add.</param>
	public void AddSpecialValue(string key, float value)
	{
		_specialValues[key] = value;
	}

	/// <summary>
	/// Adds a sub-modifier to this modifier.
	/// Sub-modifiers are applied after the primary modifier.
	/// </summary>
	/// <param name="modifier">The sub-modifier to add.</param>
	public void AddSubModifier(DifficultyModifier modifier)
	{
		_subModifiers.Add(modifier);
	}

	/// <summary>
	/// Sets the expiration time for the modifier.
	/// </summary>
	/// <param name="duration">The duration until expiration.</param>
	public void SetExpiration(TimeSpan duration)
	{
		ExpirationTime = DateTime.UtcNow + duration;
		IsTemporary = true;
	}

	/// <summary>
	/// Checks if the conditions required for this modifier to apply are satisfied.
	/// </summary>
	/// <param name="context">The context against which the conditions are checked.</param>
	/// <returns>True if all conditions are met, false otherwise.</returns>
	public bool CheckConditions(Dictionary<string, object> context)
	{
		foreach (var condition in Conditions)
		{
			if (!context.TryGetValue(condition.Key, out var contextValue) ||
				!contextValue.Equals(condition.Value))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Creates a deep copy of this modifier, including its sub-modifiers and special values.
	/// </summary>
	/// <returns>A new instance of DifficultyModifier with identical properties.</returns>
	public DifficultyModifier Clone()
	{
		return new DifficultyModifier
		{
			Type = this.Type,
			Value = this.Value,
			Source = this.Source,
			IsTemporary = this.IsTemporary,
			ExpirationTime = this.ExpirationTime,
			Priority = this.Priority,
			IsActive = this.IsActive,
			Conditions = new Dictionary<string, object>(this.Conditions),
			Description = this.Description,
			Category = this.Category,
			Tags = new HashSet<ModifierTag>(this.Tags),
			_specialValues = new Dictionary<string, float>(this._specialValues),
			_subModifiers = this._subModifiers.Select(m => m.Clone()).ToList()
		};
	}

	/// <summary>
	/// Retrieves detailed information about this modifier for debugging or logging purposes.
	/// </summary>
	/// <returns>A dictionary containing key details about the modifier.</returns>
	public Dictionary<string, object> GetModifierInfo()
	{
		return new Dictionary<string, object>
		{
			{ "Type", Type },
			{ "Value", Value },
			{ "Source", Source },
			{ "IsTemporary", IsTemporary },
			{ "ExpirationTime", ExpirationTime },
			{ "Priority", Priority },
			{ "IsActive", IsActive },
			{ "Category", Category },
			{ "Tags", Tags },
			{ "SpecialValues", _specialValues },
			{ "SubModifiers", _subModifiers.Count },
			{ "Description", Description }
		};
	}

	public bool HasTag(ModifierTag tag) => Tags.Contains(tag);
	public void AddTag(ModifierTag tag) => Tags.Add(tag);
	public void RemoveTag(ModifierTag tag) => Tags.Remove(tag);
}