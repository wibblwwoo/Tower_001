using System;
using System.Collections.Generic;
using System.Linq;
using static GlobalEnums;

public class AscensionManager : IManager
{
	private readonly Dictionary<string, ProgressionData> _characterProgress;
	private readonly ProgressionConfig _config;
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;

	public AscensionManager(ProgressionConfig config, PlayerManager playerManager,
						  Dictionary<string, ProgressionData> characterProgress)
	{
		_config = config;
		_eventManager = Globals.Instance.gameMangers.Events;
		_playerManager = playerManager;
		_characterProgress = characterProgress; // Share the same progress dictionary
	}

	public void Setup()
	{
		RegisterEventHandlers();
		DebugLogger.Log("Ascension Manager initialized", DebugLogger.LogCategory.Progress);
	}

	private void RegisterEventHandlers()
	{
		// Register any necessary event handlers
	}

	private void ApplyAscensionBonuses(string characterId, Dictionary<string, float> bonuses)
	{
		var character = _playerManager.GetCharacter(characterId);
		if (character == null) return;

		foreach (var bonus in bonuses)
		{
			// Convert string stat names to StatType and apply bonus
			if (Enum.TryParse<StatType>(bonus.Key, out StatType statType))
			{
				var stat = character.GetStat(statType);
				if (stat != null)
				{
					stat.UpdateAscensionBonus(statType.ToString(), _characterProgress[characterId].AscensionLevel, bonus.Value);

					DebugLogger.Log(
						$"Applied ascension bonus to {statType}: +{bonus.Value:P2}",
						DebugLogger.LogCategory.Progress
					);
				}
			}
		}
	}
	private Dictionary<string, float> CalculateAscensionBonuses(ProgressionData progress)
	{
		var bonuses = new Dictionary<string, float>();

		// Base bonus percentage per ascension level (2% per level)
		float baseBonus = progress.AscensionLevel * 0.02f;

		// Calculate bonus for each stat type
		foreach (StatType statType in Enum.GetValues(typeof(StatType)))
		{
			bonuses[statType.ToString()] = baseBonus;
		}

		DebugLogger.Log($"Calculated Ascension Bonuses for Level {progress.AscensionLevel}:",
					   DebugLogger.LogCategory.Progress);
		foreach (var bonus in bonuses)
		{
			DebugLogger.Log($"{bonus.Key}: +{bonus.Value:P2}", DebugLogger.LogCategory.Progress);
		}

		return bonuses;
	}


	public bool TryAscend(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
			return false;

		if (!CanAscend(characterId))
			return false;

		long oldAscensionLevel = progress.AscensionLevel;
		progress.AscensionLevel++;

		// Handle ascension reset
		float powerMultiplier = progress.TotalPowerMultiplier;
		HandleAscensionReset(characterId, progress);

		// Calculate and apply permanent bonuses
		var unlockedBonuses = CalculateAscensionBonuses(progress);
		ApplyAscensionBonuses(characterId, unlockedBonuses);

		// Raise ascension event
		RaiseAscensionEvent(characterId, progress, oldAscensionLevel, powerMultiplier, unlockedBonuses);

		DebugLogger.Log(
			$"Character {characterId} ascended to level {progress.AscensionLevel}. Applied permanent bonuses: " +
			string.Join(", ", unlockedBonuses.Select(b => $"{b.Key}: +{b.Value:P0}")),
			DebugLogger.LogCategory.Progress
		);

		return true;
	}

	private bool CanAscend(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
			return false;

		return progress.PrestigeLevel >= _config.PrestigeForAscension;
	}

	private void HandleAscensionReset(string characterId, ProgressionData progress)
	{
		progress.Level = 1;
		progress.CurrentExp = 0;
		progress.PrestigeLevel = 0;
		CalculateAscensionBonuses(progress);
	}

	private void RaiseAscensionEvent(
		string characterId,
		ProgressionData progress,
		long oldAscensionLevel,
		float oldPowerMultiplier,
		Dictionary<string, float> unlockedBonuses)
	{
		_eventManager.RaiseEvent(
			EventType.AscensionLevelGained,
			new AscensionEventArgs(
				characterId,
				progress.Level,
				progress.CurrentExp,
				progress.TotalPowerMultiplier,
				oldAscensionLevel,
				progress.AscensionLevel,
				progress.TotalPowerMultiplier - oldPowerMultiplier,
				progress.TotalPowerMultiplier,
				unlockedBonuses
			)
		);
	}
}