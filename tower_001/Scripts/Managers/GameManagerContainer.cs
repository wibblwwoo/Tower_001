using Godot;
using System;

/// <summary>
/// Container class to handle all game managers and resolve dependencies
/// </summary>
public class GameManagerContainer
{
    public EventManager Events { get; private set; }
    public PlayerManager Player { get; private set; }
    public ProgressionManager Progression { get; private set; }

    public GameManagerContainer()
    {
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        // Create managers in dependency order
        Events = new EventManager();
        Player = new PlayerManager();
        Progression = new ProgressionManager(Events, Player);

        // Setup managers
        Events.Setup();
        Player.Setup();
        Progression.Setup();

        // Set cross-references
        Player.SetProgressionManager(Progression);
    }
}