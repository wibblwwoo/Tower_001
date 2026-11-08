using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Container for all game managers
/// </summary>
/// 
public partial class ManagerContainer : IManagerContainer
{
	private ManagerDependencyResolver _resolver;
	private Dictionary<Type, IManager> _managers;

	// Core Managers - keep the properties for easy access
	public EventManager Events { get; private set; }
	public PlayerManager Player { get; private set; }
	public WorldManager World { get; private set; }
	public UIManager UI { get; private set; }
	public HeartbeatManager Heartbeat { get; private set; }

	public UIResourceManager ResourceManager { get; private set; }

	public ManagerContainer()
	{
		_resolver = new ManagerDependencyResolver();
		_managers = new Dictionary<Type, IManager>();
		RegisterManagers();
	}

	private void RegisterManagers()
	{
		// Create all managers first
		Events = new EventManager();
		UI = new UIManager();
		Player = new PlayerManager();
		World = new WorldManager();
		Heartbeat = new HeartbeatManager();
		ResourceManager = new UIResourceManager();

		// Register them with the resolver
		RegisterManager(Events);
		RegisterManager(UI);
		RegisterManager(Player);
		RegisterManager(World);
		RegisterManager(Heartbeat);
		RegisterManager(ResourceManager);
	}

	private void RegisterManager(IManager manager)
	{
		_managers[manager.GetType()] = manager;
		_resolver.AddManager(manager);
	}

	public void Setup()
	{
		try
		{
			// Get the correct initialization order
			var initOrder = _resolver.GetInitializationOrder();

			// Initialize each manager in the correct order
			foreach (var manager in initOrder)
			{
				GD.Print($"Initializing {manager.GetType().Name}");
				manager.Setup();
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Manager initialization failed: {ex.Message}");
			throw;
		}
	}

	// Helper method to get manager by type
	public T GetManager<T>() where T : class, IManager
	{
		if (_managers.TryGetValue(typeof(T), out var manager))
		{
			return manager as T;
		}
		return null;
	}
}


//Refactored
/*
public partial class ManagerContainer : IManagerContainer
{
	// Core Managers
	public EventManager Events { get; private set; }
	public PlayerManager Player { get; private set; }
	public WorldManager World { get; private set; }
	public UIManager UI { get; private set; }
	public HeartbeatManager Heartbeat { get; set; }

	public UIResourceManager ResourceManager { get; set; }

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
		ResourceManager = new UIResourceManager();
		UI = new UIManager();
		Player = new PlayerManager();
		World = new WorldManager();

		Heartbeat = new HeartbeatManager();
		// EventManager already setup in InitializeManagers
		ResourceManager.Setup();
		UI.Setup();
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

*/
