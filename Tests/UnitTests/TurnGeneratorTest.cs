using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Nova.Server
{
    public class SimpleServerData : ServerData
    {
        public SimpleServerData()
        {
        }
    }

    public class SimpleTurnGenerator : TurnGenerator
    {
        public SimpleTurnGenerator(ServerData serverState) : base(serverState)
        {
        }

        protected override void BackupTurn()
        {
            // base.BackupTurn();
        }

        protected override void ReadOrders()
        {
            // base.ReadOrders();
        }

        protected override void ParseCommands()
        {
            // base.ParseCommands();
        }

        protected override void WriteIntel()
        {
            // base.WriteIntel();
        }

        protected override void CleanupOrders()
        {
            // base.CleanupOrders();
        }
    }
}

namespace Nova.Common
{
    public class SimpleEmpireData : EmpireData
    {
        public SimpleEmpireData()
        {
        }

        protected override void Initialize(bool complete,String hint)
        {
            base.Initialize(complete, hint);
        }
    }
}

namespace Nova.Tests.UnitTests
{
    using NUnit.Framework;

    using Nova.Common;
    using Nova.Server;
    using Nova.Common.Waypoints;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;

    [TestFixture]
    public class TurnGeneratorTest
    {
        private ServerData serverData;
        private List<Fleet> fleets;
        private EmpireData empireData;

        [SetUp]
        public void Init()
        {
            fleets = new List<Fleet>();
            Fleet fleet = new Fleet(1);
            fleet.Owner = 1;
            ShipDesign shipDesign = new ShipDesign(1);
            ShipToken shipToken = new ShipToken(shipDesign, 1);
            fleet.Composition.Add(shipToken.Key, shipToken);
            fleets.Add(fleet);
            Waypoint waypoint = new Waypoint();
            IWaypointTask task = new ScrapTask();
            waypoint.Task = task;
            waypoint.Destination = "Star1";
            fleet.Waypoints.Add(waypoint);
            serverData = new SimpleServerData();
            Star star = new Star();
            star.Name = "Star1";
            serverData.AllStars.Add(star.Key, star);
            empireData = new SimpleEmpireData();
            empireData.Id = 1;
            empireData.OwnedFleets.Add(fleet);
            serverData.AllEmpires.Add(empireData.Id, empireData);
            Console.WriteLine(fleets.First().Composition.Count());
            Assert.That(fleets.First().Composition.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Generate_ScrapFleets()
        {
            SimpleTurnGenerator turnGenerator = new SimpleTurnGenerator(serverData);
            Assert.That(serverData.IterateAllFleets().ToList(), Is.Not.Empty);
            turnGenerator.Generate();
            Console.WriteLine(fleets.First().Composition.Count());
            Assert.That(fleets.First().Composition.Count(), Is.EqualTo(0));
            Assert.That(serverData.IterateAllFleets().ToList(), Is.Empty);
        }

        [Test]
        public void Generate_Dont_ScrapFleets()
        {
            // ToDo: Fleet-Generation and other things to seperate class/es and/or methods
            Fleet fleet = new Fleet(2);
            fleet.Owner = 1;
            NovaPoint point = new NovaPoint(0, 0);
            fleet.Position = point;
            ShipDesign shipDesign = new ShipDesign(2);
            shipDesign.Blueprint = new Component();
            Hull hull = new Hull();
            hull.Modules = new List<HullModule>();
            hull.Modules.Add(new HullModule());
            shipDesign.Blueprint.Properties.Add("Hull", hull);
            ShipToken shipToken = new ShipToken(shipDesign, 1);
            fleet.Composition.Add(shipToken.Key, shipToken);
            fleets.Add(fleet);
            Waypoint waypoint = new Waypoint();
            NovaPoint waypointpoint = new NovaPoint(1,1);
            waypoint.Position = waypointpoint;
            IWaypointTask task = new NoTask();
            waypoint.Task = task;
            waypoint.Destination = "Star1";
            fleet.Waypoints.Add(waypoint);

            empireData.AddOrUpdateFleet(fleet);
            // empireData.OwnedFleets.Add(fleet); // ToDo: this should not be allowed I think

            Console.WriteLine("2 Fleets: " + fleets.Count());
            Assert.That(serverData.IterateAllFleets().ToList(), Is.Not.Empty);
            Console.WriteLine("all Fleets count: " + serverData.IterateAllFleets().ToList().Count());

            SimpleTurnGenerator turnGenerator = new SimpleTurnGenerator(serverData);
            turnGenerator.Generate();
            // Assert.AreEqual(fleets.First().Composition.Count(), 1);
            Assert.That(serverData.IterateAllFleets().ToList(), Is.Not.Empty);
        }

        [Test]
        public void SetFleetOrbit()
        {
            Fleet fleet = new Fleet(1);
            fleet.InOrbit = null;

            Star star = new Star();
            star.Name = "Star1";
            NovaPoint starPoint = new NovaPoint(0, 0);

            serverData = new SimpleServerData();
            serverData.AllStars.Add(star.Key, star);

            NovaPoint fleetPoint = new NovaPoint(0, 0);
            fleet.Position = fleetPoint;
            serverData.SetFleetOrbit(fleet);
            Assert.That(star.Name, Is.EqualTo(fleet.InOrbit.Name));

            fleet.Position.X = 1;
            serverData.SetFleetOrbit(fleet);
            Assert.That(null, Is.EqualTo(fleet.InOrbit));
        }
    }
}
