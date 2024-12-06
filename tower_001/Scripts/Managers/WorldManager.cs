using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manager for world-related systems including towers, resources, etc.
/// </summary>
public partial class WorldManager : BaseManager
{

	private ManagerDependencyResolver _subManagerResolver;
	public TowerManager Towers { get; private set; }
	public FloorManager Floors { get; private set; }
	public RoomManager Rooms { get; private set; }

	public override IEnumerable<Type> Dependencies => new[]
	{
		typeof(EventManager),
		typeof(UIManager)  // For world state visualization
    };

	public override void Setup()
	{
		// Call base.Setup() first to ensure EventManager is ready
		base.Setup();
		RegisterEventHandlers();
		_subManagerResolver = new ManagerDependencyResolver();
		InitializeSubManagers();
	}

	protected override void RegisterEventHandlers()
	{
		// Register world-level event handlers
	}

	private void InitializeSubManagers()
	{
		Towers = new TowerManager(EventManager);
		Floors = new FloorManager();
		Rooms = new RoomManager();

		// Register with resolver
		_subManagerResolver.AddManager(EventManager);
		_subManagerResolver.AddManager(Towers);
		_subManagerResolver.AddManager(Floors);
		_subManagerResolver.AddManager(Rooms);

		// Initialize in dependency order
		var initOrder = _subManagerResolver.GetInitializationOrder();
		foreach (var manager in initOrder)
		{
			manager.Setup();
		}
	}
}