using NUnit.Framework;
using System;
using System.Linq;
using static GlobalEnums;

namespace Tower_001.Tests.GameLogic
{
    [TestFixture]
    public class TowerGenerationTests
    {
        private TowerManager _towerManager;
        private FloorManager _floorManager;
        private EventManager _eventManager;
        private WorldManager _worldManager;

        [SetUp]
        public void Setup()
        {
            _eventManager = new EventManager();
            _worldManager = new WorldManager();
            _worldManager.Setup();
            _towerManager = _worldManager.Towers;
            _floorManager = _worldManager.Floors;
        }

        [Test]
        public void GenerateTower_CreatesValidTowerWithFloors()
        {
            // Act
            var tower = CreateTestTower();

            // Assert
            Assert.That(tower, Is.Not.Null);
            Assert.That(tower.Id, Is.Not.Empty);
            Assert.That(tower.CurrentState, Is.EqualTo(TowerState.Available));
            Assert.That(tower.Floors, Is.Not.Null);
            Assert.That(tower.Floors.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GenerateFloors_AssignsFloorsToTower()
        {
            // Arrange
            var tower = CreateTestTower();
            const int expectedFloorCount = 10;

            // Act
            bool success = _towerManager.GenerateFloorsForTower(tower.Id, expectedFloorCount);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(tower.Floors, Is.Not.Null);
            Assert.That(tower.Floors.Count, Is.EqualTo(expectedFloorCount));
            
            // Check that floors are properly linked to tower
            Assert.That(_floorManager.GetFloorsForTower(tower.Id).Count, Is.EqualTo(expectedFloorCount));
        }

        [Test]
        public void GenerateRooms_CreatesValidRoomsForFloor()
        {
            // Arrange
            var tower = CreateTestTower();
            _towerManager.GenerateFloorsForTower(tower.Id, 1);
            var floor = tower.Floors.First();

            // Assert
            Assert.That(floor.Rooms, Is.Not.Null);
            Assert.That(floor.Rooms.Count, Is.GreaterThan(0));
            
            foreach (var room in floor.Rooms)
            {
                Assert.That(room.Id, Is.Not.Empty);
                Assert.That(room.Position.x, Is.GreaterThanOrEqualTo(0));
                Assert.That(room.Type, Is.Not.EqualTo(RoomType.None));
                Assert.That(room.State, Is.EqualTo(RoomState.Locked));
            }
        }

        [Test]
        public void GenerateBossFloor_HasCorrectRoomConfiguration()
        {
            // Arrange
            var tower = CreateTestTower();
            _towerManager.GenerateFloorsForTower(tower.Id, 10); // Generate 10 floors to include a boss floor

            // Act
            var bossFloor = tower.Floors.FirstOrDefault(f => f.IsBossFloor);

            // Assert
            Assert.That(bossFloor, Is.Not.Null);
            Assert.That(bossFloor.Rooms.Count, Is.EqualTo(1)); // Boss floors should have exactly one room