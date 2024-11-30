using Godot;
using System;

/// <summary>
/// Manager for world-related systems including towers, resources, etc.
/// </summary>
public partial class WorldManager : BaseManager
{
	public TowerManager Towers { get; private set; }
	//public ResourceManager Resources { get; private set; }
	//public RewardManager Rewards { get; private set; }
	//	public AchievementManager Achievements { get; private set; }
	//private Dictionary<string, FloorManager> _floorManagers;
	public FloorManager Floors{ get; private set; }
	//private Dictionary<string, FloorManager> _floorManagers;
	public RoomManager Rooms { get; private set; }

	public override void Setup()
	{
		RegisterEventHandlers();
		InitializeSubManagers();
	}

	protected override void RegisterEventHandlers()
	{
		// Register world-level event handlers
	}

	private void InitializeSubManagers()
	{
		Towers = new TowerManager();
		Floors = new FloorManager();
		Rooms = new RoomManager();	
		//Resources = new ResourceManager();
		//Rewards = new RewardManager();
		//Achievements = new AchievementManager();

		// Setup in correct order
		Towers.Setup();
		Floors.Setup();
		Rooms.Setup();
		//Resources.Setup();
		//Rewards.Setup();
		//Achievements.Setup();
	}
}