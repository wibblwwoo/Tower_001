using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Core manager class for handling all progression-related functionality.
/// Manages experience, leveling, prestige, ascension, and related events.
/// </summary>
public partial class ProgressionManager : IManager
{
	private readonly Dictionary<string, ProgressionData> _characterProgress;
	private AscensionManager ascensionManager;

	private ProgressionConfig _config;
	private readonly EventManager _eventManager;
	private readonly PlayerManager _playerManager;

	public AscensionManager AscensionManager { get => ascensionManager; set => ascensionManager = value; }
	public ProgressionConfig Config { get => _config; set => _config = value; }

	/// <summary>
	/// Initializes a new instance of the ProgressionManager
	/// </summary>
	/// <param name="eventManager">Reference to the game's event manager</param>
	/// <param name="playerManager">Reference to the player manager</param>
	public ProgressionManager(PlayerManager playerManager)
	{
		_characterProgress = new Dictionary<string, ProgressionData>();
		_config = new ProgressionConfig
		{
			BaseExpForLevel = 1000f,
			ExpScalingFactor = 1.15f,
			LevelsForPrestige = 100000,
			PrestigePowerMultiplier = 2.0f
		};

		_eventManager = Globals.Instance.gameMangers.Events;
		_playerManager = playerManager;
		AscensionManager = new AscensionManager(_config, _playerManager, _characterProgress);
	}


	/// <summary>
	/// Sets up the progression manager and registers event handlers
	/// </summary>
	public void Setup()
	{
		RegisterEventHandlers();
		DebugLogger.Log("Progression Manager initialized", DebugLogger.LogCategory.Progress);
		AscensionManager.Setup();
	}

	/// <summary>
	/// Registers all progression-related event handlers
	/// </summary>
	private void RegisterEventHandlers()
	{
		// Character events
		_eventManager.AddHandler<CharacterEventArgs>(
			EventType.CharacterCreated,
			OnCharacterCreated);

		_eventManager.AddHandler<CharacterLevelEventArgs>(
			EventType.CharacterLevelUp,
			OnCharacterLevelUp);

		// Room completion events
		_eventManager.AddHandler<RoomCompletionEventArgs>(
			EventType.RoomEventCompleted,
			OnRoomCompleted);

		// Floor completion events
		_eventManager.AddHandler<FloorEventArgs>(
			EventType.FloorCompleted,
			OnFloorCompleted);

		// Tower completion events 
		_eventManager.AddHandler<TowerEventArgs>(
			EventType.TowerCompleted,
			OnTowerCompleted);
	}

	/// <summary>
	/// Adds experience to a character from a specific source
	/// </summary>
	public void AddExperience(string characterId, float amount, ExperienceSource source)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
		{
			DebugLogger.LogWarning($"No progression data found for character {characterId}");
			return;
		}

		if (amount < 0)
		{
			DebugLogger.LogWarning($"Negative Experence Supplied - Ignoring {characterId}");
			return;

		}

		// Calculate final experience with multipliers
		float multiplier = progress.GetEffectiveMultiplier(source);
		float finalExperience = amount * multiplier;

		// Update progress
		float oldExp = progress.CurrentExp;
		int oldLevel = progress.Level;
		progress.CurrentExp += finalExperience;
		progress.LifetimeExperience[source] += finalExperience;

		// Raise experience gain event
		RaiseExperienceGainedEvent(characterId, progress, finalExperience, source, multiplier, amount);

		// Check for level up
		CheckLevelUp(characterId, progress, oldLevel);

		DebugLogger.Log(
			$"Experience added - Character: {characterId}, Amount: {finalExperience:F0}, Source: {source}",
			DebugLogger.LogCategory.Progress);
	}

	/// <summary>
	/// Attempts to prestige a character if requirements are met
	/// </summary>
	public bool TryPrestige(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
			return false;

		if (!progress.CanPrestige)
			return false;

		int oldPrestige = progress.PrestigeLevel;
		progress.PrestigeLevel++;

		// Handle prestige reset
		float powerMultiplier = progress.TotalPowerMultiplier;
		HandlePrestigeReset(characterId);

		// Raise prestige event
		RaisePrestigeEvent(characterId, progress, oldPrestige, powerMultiplier);

		DebugLogger.Log(
			$"Character {characterId} prestiged to level {progress.PrestigeLevel}",
			DebugLogger.LogCategory.Progress);

		return true;
	}

	
	/// <summary>
	/// Updates progression-related achievements for a character
	/// </summary>
	public void UpdateAchievements(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
			return;

		foreach (var achievement in GetTrackedAchievements(progress))
		{
			float currentProgress = CalculateAchievementProgress(achievement, progress);
			bool wasCompleted = progress.CompletedAchievements.Contains(achievement.Id);

			// Update progress
			progress.AchievementProgress[achievement.Id] = currentProgress;

			// Check for completion
			if (currentProgress >= achievement.TargetValue && !wasCompleted)
			{
				progress.CompletedAchievements.Add(achievement.Id);
				RaiseAchievementEvent(characterId, progress, achievement, currentProgress);
			}
		}
	}

	#region Event Raising Methods

	private void RaiseExperienceGainedEvent(
		string characterId,
		ProgressionData progress,
		float experienceGained,
		ExperienceSource source,
		float multiplier,
		float baseAmount)
	{
		_eventManager.RaiseEvent(
			EventType.ExperienceGained,
			new ExperienceGainEventArgs(
				characterId,
				progress.Level,
				progress.CurrentExp,
				progress.TotalPowerMultiplier,
				experienceGained,
				source,
				multiplier,
				baseAmount
			)
		);
	}

	private void RaisePrestigeEvent(
		string characterId,
		ProgressionData progress,
		int oldPrestigeLevel,
		float oldPowerMultiplier)
	{
		_eventManager.RaiseEvent(
			EventType.PrestigeLevelGained,
			new PrestigeEventArgs(
				characterId,
				progress.Level,
				progress.CurrentExp,
				progress.TotalPowerMultiplier,
				oldPrestigeLevel,
				progress.PrestigeLevel,
				progress.TotalPowerMultiplier - oldPowerMultiplier,
				progress.TotalPowerMultiplier
			)
		);
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

	private void RaiseAchievementEvent(
		string characterId,
		ProgressionData progress,
		ProgressionAchievement achievement,
		float currentProgress)
	{
		_eventManager.RaiseEvent(
			EventType.ProgressionAchievementUpdated,
			new ProgressionAchievementEventArgs(
				characterId,
				progress.Level,
				progress.CurrentExp,
				progress.TotalPowerMultiplier,
				achievement.Id,
				achievement.Type.ToString(),
				currentProgress,
				achievement.TargetValue,
				true,
				achievement.Rewards
			)
		);
	}

	#endregion

	#region Event Handlers

	private void OnCharacterCreated(CharacterEventArgs args)
	{
		// Initialize progression data for new character
		_characterProgress[args.CharacterId] = new ProgressionData(_config);
		DebugLogger.Log($"Initialized progression for character {args.CharacterId}",
			DebugLogger.LogCategory.Progress);
	}

	private void OnCharacterLevelUp(CharacterLevelEventArgs args)
	{
		if (_characterProgress.TryGetValue(args.CharacterId, out var progress))
		{
			UpdateAchievements(args.CharacterId);
			CheckProgressionMilestones(args.CharacterId, progress);
		}
	}

	private void OnRoomCompleted(RoomCompletionEventArgs args)
	{
		// Get current character from player manager
		var character = _playerManager.GetCurrentCharacter();
		if (character == null) return;

		// Calculate and award room completion experience
		float baseExp = CalculateRoomExperience(args);
		AddExperience(character.Id, baseExp, ExperienceSource.RoomCompletion);
	}

	private void OnFloorCompleted(FloorEventArgs args)
	{
		// Get current character from player manager
		var character = _playerManager.GetCurrentCharacter();
		if (character == null) return;

		// Calculate and award floor completion experience
		float baseExp = CalculateFloorExperience(args);
		AddExperience(character.Id, baseExp, ExperienceSource.FloorCompletion);
	}

	private void OnTowerCompleted(TowerEventArgs args)
	{
		// Get current character from player manager
		var character = _playerManager.GetCurrentCharacter();
		if (character == null) return;

		// Calculate and award tower completion experience
		float baseExp = CalculateTowerExperience(args);
		AddExperience(character.Id, baseExp, ExperienceSource.TowerCompletion);
	}

	#endregion

	#region Helper Methods

	private void CheckLevelUp(string characterId, ProgressionData progress, int oldLevel)
	{
		while (progress.CurrentExp >= progress.GetExpForNextLevel())
		{
			progress.CurrentExp -= progress.GetExpForNextLevel();
			progress.Level++;

			// Let the character manager handle the actual level-up
			_playerManager.SetCharacterLevel(characterId, progress.Level);
		}
	}

	public void HandlePrestigeReset(string characterId)
	{
		if (GetProgressData(characterId) is ProgressionData progress)
		{
			progress.Level = 1;
			progress.CurrentExp = 0;
			// Reset other appropriate values while maintaining prestige bonuses
		}
	}

	private void HandleAscensionReset(string characterId, ProgressionData progress)
	{
		progress.Level = 1;
		progress.CurrentExp = 0;
		progress.PrestigeLevel = 0;
		// Reset other appropriate values while maintaining ascension bonuses
	}

	private Dictionary<string, float> CalculateAscensionBonuses(ProgressionData progress)
	{
		// Calculate and return the bonuses unlocked at this ascension level
		return AscensionBonusCalculator.CalculateAscensionBonuses(progress);
	}

	private IEnumerable<ProgressionAchievement> GetTrackedAchievements(ProgressionData progress)
	{
		// Return list of achievements to track
		return new List<ProgressionAchievement>();
	}

	private float CalculateAchievementProgress(ProgressionAchievement achievement, ProgressionData progress)
	{
		// Calculate current progress for the achievement
		return 0f;
	}

	private float CalculateRoomExperience(RoomCompletionEventArgs args)
	{
		// Calculate base experience for room completion
		return 100f;
	}

	private float CalculateFloorExperience(FloorEventArgs args)
	{
		// Calculate base experience for floor completion
		return 500f;
	}

	private float CalculateTowerExperience(TowerEventArgs args)
	{
		// Calculate base experience for tower completion
		return 2000f;
	}

	private void CheckProgressionMilestones(string characterId, ProgressionData progress)
	{
		// Check and handle any progression milestones reached
	}

	public float GetCurrentExperience(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
		{
			return 0f;
		}
		return progress.CurrentExp;
	}

	public float GetExpForNextLevel(string characterId)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
		{
			return _config.BaseExpForLevel;
		}
		return _config.BaseExpForLevel * (float)Math.Pow(_config.ExpScalingFactor, progress.Level - 1);
	}

	public float GetLifetimeExperience(string characterId, ExperienceSource source)
	{
		if (!_characterProgress.TryGetValue(characterId, out var progress))
		{
			return 0f;
		}
		return progress.LifetimeExperience.GetValueOrDefault(source, 0f);
	}

	public int GetMaxLevel()
	{
		// This could also be configurable in ProgressionConfig
		return 100; // Default max level
	}


	#endregion

	public ProgressionData GetProgressData(string characterId)
	{
		if (_characterProgress.TryGetValue(characterId, out var progress))
		{
			return progress;
		}
		return null;
	}

	public bool CanPrestige(string characterId, int prestigeLevel)
	{
		if (_characterProgress.TryGetValue(characterId, out var progress))
		{
			long prestigeCost = GetPrestigeCost(characterId, prestigeLevel);
			// && progress.ResourceManager.CanAfford(ResourceType.Currency, prestigeCost)
			return progress.Level >= _config.LevelsForPrestige;
		}
		return false;
	}

	public long GetPrestigeCost(string characterId, int prestigeLevel)
	{
		if (_config.PrestigeCosts.TryGetValue(prestigeLevel, out long cost))
		{
			return cost;
		}
		return 0;
	}


	private void HandlePrestigeReset(string characterId, ProgressionData progress)
	{
		progress.Level = 1;
		progress.CurrentExp = 0;
		//progress.PrestigeMultiplierGained = CalculatePrestigeMultiplier(progress);
		// Reset other appropriate values while maintaining prestige bonuses
	}

	
}