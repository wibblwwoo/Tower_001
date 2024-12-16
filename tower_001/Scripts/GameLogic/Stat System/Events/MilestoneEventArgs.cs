using System;
using static GlobalEnums;

namespace Tower_001.Scripts.GameLogic.Stat_System.Events
{
	/// <summary>
	/// Event arguments for tracking bonus changes to character statistics.
	/// This class handles events when any permanent bonus (Ascension, Prestige, Achievement, etc.) is added or removed.
	/// </summary>
	/// <remarks>
	/// CharacterBonusEventArgs is used for:
	/// - UI updates showing bonus changes
	/// - Permanent progression tracking
	/// - Achievement system integration
	/// - Character power calculation
	/// - Milestone tracking
	/// 
	/// The event captures:
	/// - The type of bonus (source)
	/// - The affected stat
	/// - The bonus value
	/// - Whether it was added or removed
	/// </remarks>
	public class MilestoneEventArgs : GameEventArgs
	{
		/// <summary>
		/// Gets the unique identifier of the character receiving the bonus
		/// </summary>
		public string CharacterId { get; }

		public MilestoneType Source { get; }

		/// <summary>
		/// Gets the value of the bonus being applied or removed
		/// </summary>
		public string MilestoneId { get; }

		/// <summary>
		/// Gets whether the bonus is being added (true) or removed (false)
		/// </summary>
		public bool IsAdded { get; }

		/// <summary>
		/// Initializes a new instance of CharacterBonusEventArgs
		/// </summary>
		/// <param name="characterId">The unique identifier of the character</param>
		/// <param name="statType">The stat being modified</param>
		/// <param name="source">The source of the bonus</param>
		/// <param name="value">The value of the bonus</param>
		/// <param name="isAdded">True if bonus is being added, false if removed</param>
		/// 
		//_characterId, milestone.Id, milestone.Type
		public MilestoneEventArgs(
			string characterId,
			string milestoneId,
			MilestoneType source)
		{
			CharacterId = characterId;
			MilestoneId = milestoneId;
			Source = source;
		}
	}
}
