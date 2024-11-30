using Godot;
using System;

/// <summary>
/// Container for all game managers
/// </summary>
public partial class ManagerContainer : IManagerContainer
{
	// Core Managers
	public EventManager Events { get; private set; }
	public PlayerManager Player { get; private set; }
	public WorldManager World { get; private set; }
	public UIManager UI { get; private set; }
	public HeartbeatManager Heartbeat { get; set; }

	public ManagerContainer()
	{
		InitializeManagers();
	}

	private void InitializeManagers()
	{
		// Initialize in dependency order
		Events = new EventManager();
		Events.Setup(); // Setup EventManager first

	}

	public void Setup()
	{
		UI = new UIManager();
		Player = new PlayerManager();
		World = new WorldManager();

		Heartbeat = new HeartbeatManager();
		// EventManager already setup in InitializeManagers
		Player.Setup();
		World.Setup();
	}
}
//public partial class ManagerContainer : IManagerContainer
//{
//	public EventManager Events { get; set; }
//	public UIManager UI { get; set; }

//	public PlayerManager Player { get; set; }

//	public TowerManager Tower { get; set; }

//	public HeartbeatManager Heartbeat { get; set; }



//	public ManagerContainer()
//	{
//	}
//	public void Setup()
//	{
//		Heartbeat = new HeartbeatManager();
//		Events = new EventManager();
//		Events.Setup();
//		UI = new UIManager();
//		Player = new PlayerManager();
//		Tower = new TowerManager();

//		Tower.Setup();
//	}

//}
