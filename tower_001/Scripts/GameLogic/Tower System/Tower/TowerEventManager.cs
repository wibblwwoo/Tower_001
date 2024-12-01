using Godot;
using System;
using System.Collections.Generic;
using static GlobalEnums;

/// <summary>
/// Manages tower-related events and their handlers.
/// </summary>
public class TowerEventManager
{
    private readonly EventManager _eventManager;
    private readonly Dictionary<string, TowerData> _towers;

    public TowerEventManager(
        EventManager eventManager,
        Dictionary<string, TowerData> towers)
    {
        _eventManager = eventManager;
        _towers = towers;
    }

    /// <summary>
    /// Handles the event when a tower's state changes.
    /// Updates the tower's state and logs the change.
    /// </summary>
    /// <param name="args">The event arguments containing details about the state change.</param>
    private void HandleTowerStateChanged(TowerEventArgs args)
    {
        // Attempt to find the tower in the _towers dictionary based on the tower ID in the event args
        if (_towers.TryGetValue(args.TowerId, out var tower))
        {
            // Update the tower's current state to the new state provided in the event arguments
            tower.CurrentState = args.NewState;

            // Log the state change
            DebugLogger.Log($"Tower {tower.Name} state changed from {args.PreviousState} to {args.NewState}", DebugLogger.LogCategory.Tower);
        }
    }

    /// <summary>
    /// Handles the event when a tower is initialized.
    /// Logs the initialization of the tower to confirm it has been initialized successfully.
    /// </summary>
    /// <param name="args">The event arguments containing information about the initialized tower.</param>
    private void HandleTowerInitialized(TowerEventArgs args)
    {
        // Attempt to retrieve the tower using the provided tower ID from the event arguments
        if (_towers.TryGetValue(args.TowerId, out var tower))
        {
            // Log the tower initialization message
            DebugLogger.Log($"Tower {tower.Name} initialized", DebugLogger.LogCategory.Tower);
        }
    }

    /// <summary>
    /// Handles the event when floors have been generated for a tower.
    /// If the tower is locked, it updates the tower's state to "Available" and raises an event to notify the system.
    /// </summary>
    /// <param name="args">The event arguments containing details about the floor generation.</param>
    private void HandleFloorsGenerated(FloorEventArgs args)
    {
        // Attempt to retrieve the tower using the FloorId (FloorId contains the towerId in this context)
        if (_towers.TryGetValue(args.TowerId, out var tower))
        {
            // REMOVED: Automatic unlocking after floor generation
            // The tower should remain locked until explicitly unlocked
            DebugLogger.Log($"Floors generated for tower {args.TowerId}", DebugLogger.LogCategory.Tower);
        }
    }

    /// <summary>
    /// Handles the event when an individual floor is initialized.
    /// This method can be extended to perform additional actions when a floor is initialized.
    /// </summary>
    /// <param name="args">The event arguments containing details about the floor initialization.</param>
    private void HandleFloorInitialized(FloorEventArgs args)
    {
        //GD.Print($"Floor initialized in tower {args.FloorId} with number {args.FloorNumber}");
    }

    public void RegisterEventHandlers()
    {
        _eventManager.AddHandler<TowerEventArgs>(EventType.TowerStateChanged, HandleTowerStateChanged);
        _eventManager.AddHandler<TowerEventArgs>(EventType.TowerInitialized, HandleTowerInitialized);
        _eventManager.AddHandler<FloorEventArgs>(EventType.FloorsGenerated, HandleFloorsGenerated);
        _eventManager.AddHandler<FloorEventArgs>(EventType.FloorInitialized, HandleFloorInitialized);
    }
}
