using Godot;
using System;
using static GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using Tower_001.Scripts.GameLogic.Balance;

/// <summary>
/// Manages tower instances and their states, including floor generation and tower progression.
/// Handles player interactions with towers and raises events for significant changes.
/// </summary>
public partial class TowerManager : BaseManager
{
    public override IEnumerable<Type> Dependencies => new[]
    {
        typeof(EventManager)
    };

    private Dictionary<string, TowerData> _towers = new();
    private TowerProgressionManager _progressionManager;
    private TowerEventManager _eventManager;
    private TowerDifficultyManager _difficultyManager;
    private TowerDataManager _dataManager;

    public override void Setup()
    {
        base.Setup();
        RegisterEventHandlers();
        InitializeSubManagers();
        LoadTowerData();
    }

    private void InitializeSubManagers()
    {
        _progressionManager = new TowerProgressionManager(EventManager, _towers);
        _eventManager = new TowerEventManager(EventManager, _towers);
        _difficultyManager = new TowerDifficultyManager(_towers);
        _dataManager = new TowerDataManager(EventManager, _towers);
        _eventManager.RegisterEventHandlers();
    }

    private void LoadTowerData()
    {
        _dataManager.LoadTowerData();
    }

    /// <summary>
    /// Generates a specified number of floors for a given tower using the provided configuration.
    /// </summary>
    public bool GenerateFloorsForTower(string towerId, int floorCount, FloorGenerationConfig config = null)
    {
        if (!_towers.TryGetValue(towerId, out var tower))
        {
            GD.PrintErr($"Tower {towerId} not found");
            return false;
        }

        try
        {
            // Generate floors using FloorManager
            bool success = Globals.Instance.gameMangers.World.Floors.GenerateFloorsForTower(tower, floorCount, config);

            if (success)
            {
                // Get all floors and assign them to the tower in order
                tower.Floors = Globals.Instance.gameMangers.World.Floors
                    .GetFloorsForTower(towerId)
                    .OrderBy(f => f.FloorNumber)
                    .ToList();

                if (tower.Floors == null || !tower.Floors.Any())
                {
                    GD.PrintErr("No floors were assigned to tower after generation");
                    return false;
                }

                DebugLogger.Log($"Generated and assigned {tower.Floors.Count} floors to tower {towerId}", DebugLogger.LogCategory.Tower);

                EventManager.RaiseEvent(EventType.FloorsGenerated,
                    FloorEventArgs.CreateGenerationComplete(towerId, floorCount, "Floor generation complete"));
            }

            return success;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error generating floors: {ex.Message}");
            return false;
        }
    }

    public FloorState GetFloorState(string towerId, string floorId)
    {
        if (_towers.TryGetValue(towerId, out var tower))
        {
            var floor = tower.Floors?.FirstOrDefault(f => f.Id == floorId);
            return floor?.CurrentState ?? FloorState.Locked;
        }
        return FloorState.Locked;
    }

    public bool TryEnterFloor(string towerId, string floorId, Character character)
    {
        return _towers.TryGetValue(towerId, out var tower) &&
               Globals.Instance.gameMangers.World.Floors.TryEnterFloor(tower, floorId, character);
    }

    public bool TryEnterTower(string towerId)
    {
        return _progressionManager.TryEnterTower(towerId);
    }

    public float GetTowerDifficulty(string towerId)
    {
        return _difficultyManager.GetTowerDifficulty(towerId);
    }

    public IEnumerable<TowerData> GetAvailableTowers()
    {
        return GetTowersByState(TowerState.Available);
    }

    public IEnumerable<TowerData> GetLockedTowers()
    {
        return GetTowersByState(TowerState.Locked);
    }

    public IEnumerable<TowerData> GetTowersByState(TowerState? state = null)
    {
        return state.HasValue
            ? _towers.Values.Where(t => t.CurrentState == state.Value)
            : GetAllTowers();
    }

    public IEnumerable<TowerData> GetAllTowers()
    {
        return _towers.Values;
    }

    public bool LockTower(string towerId)
    {
        return _progressionManager.LockTower(towerId);
    }

    public bool CompleteFloor(string towerId, string floorId)
    {
        return _progressionManager.CompleteFloor(towerId, floorId);
    }

    public bool FailTower(string towerId)
    {
        return _progressionManager.FailTower(towerId);
    }

    public bool ResetFloorState(string towerId, string floorId)
    {
        if (_towers.TryGetValue(towerId, out var tower))
        {
            return Globals.Instance.gameMangers.World.Floors.ResetFloorState(tower, floorId);
        }
        return false;
    }

    public bool UnlockTower(string towerId)
    {
        return _progressionManager.UnlockTower(towerId);
    }

    public bool FailFloor(string towerId, string floorId)
    {
        return _progressionManager.FailFloor(towerId, floorId);
    }

    public bool ExitTower(string towerId)
    {
        return _progressionManager.ExitTower(towerId);
    }

    protected override void RegisterEventHandlers()
    {
    }
}