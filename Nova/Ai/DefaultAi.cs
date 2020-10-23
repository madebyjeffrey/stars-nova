#region Copyright Notice
// ============================================================================
// Copyright (C) 2009 - 2017 stars-nova
//
// This file is part of Stars-Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

namespace Nova.Ai
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using Nova.Common.Waypoints;

    public class DefaultAi : AbstractAI
    {
        private Intel turnData;
        private FleetList fuelStations = null;
        private DefaultAIPlanner aiPlan = null;
        
        // Sub AIs to manage planets, fleets and stuff
        private Dictionary<string, DefaultPlanetAI> planetAIs = new Dictionary<string, DefaultPlanetAI>();
        private Dictionary<long, DefaultFleetAI> fleetAIs = new Dictionary<long, DefaultFleetAI>();

        /// <summary>
        /// This is the entry point to the AI proper. 
        /// Currently this does not use anything recognized by Computer Science as AI,
        /// just functional programming to complete a list of tasks.
        /// </summary>
        public override void DoMove()
        {
            aiPlan = new DefaultAIPlanner(clientState);

            // create the helper AIs
            foreach (Star star in clientState.EmpireState.OwnedStars.Values)
            {
                if (star.Owner == clientState.EmpireState.Id)
                {
                    DefaultPlanetAI planetAI = new DefaultPlanetAI(star, clientState, this.aiPlan);
                    planetAIs.Add(star.Key, planetAI);
                }
            }

            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.Owner == clientState.EmpireState.Id)
                {
                    aiPlan.CountFleet(fleet);
                    if (fleet.Name.Contains("Scout") || (fleet.Name.Contains("Long Range Scout")))
                    {
                        DefaultFleetAI fleetAI = new DefaultFleetAI(fleet, clientState, fuelStations);
                        fleetAIs.Add(fleet.Id, fleetAI);

                        // reset all waypoint orders
                        for (int wpIndex = 1; wpIndex < fleet.Waypoints.Count; wpIndex++)
                        {
                            WaypointCommand command = new WaypointCommand(CommandMode.Delete, fleet.Key, wpIndex);
                            command.ApplyToState(clientState.EmpireState);
                            clientState.Commands.Push(command);
                        }
                    }
                }
            }

            turnData = clientState.InputTurn;

            HandleProduction();
            HandleResearch();
            HandleScouting();
            HandleColonizing();
            HandleShipDesign();
            HandlePopulationSurplus();
            HandleFuelImpoverishedFleets();
        }

        /// <summary>
        /// Setup the production queue for the AI.
        /// </summary>
        private void HandleProduction()
        {
            foreach (DefaultPlanetAI ai in planetAIs.Values)
            {
                ai.HandleProduction();
            }
        }

        private void HandleScouting()
        {
            List<Fleet> scoutFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.Name.Contains("Scout") == true)
                {
                    scoutFleets.Add(fleet);
                }
            }

            // Find the stars we do not need to scout (eg home world)
            List<StarIntel> excludedStars = new List<StarIntel>();
            foreach (StarIntel report in turnData.EmpireState.StarReports.Values)
            {
                if (report.Year != Global.Unset)
                {
                    excludedStars.Add(report);
                }
            }

            if (scoutFleets.Count > 0)
            {
                foreach (Fleet fleet in scoutFleets)
                {
                    StarIntel starToScout = fleetAIs[fleet.Id].Scout(excludedStars);
                    if (starToScout != null)
                    {
                        excludedStars.Add(starToScout);
                    }
                }
            }
        }

        private void HandleArmedScouting()
        {
            List<Fleet> armedScoutFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if ((fleet.Name.Contains("Pointy Stick") == true) && (fleet.Waypoints.Count == 1))
                {
                    armedScoutFleets.Add(fleet);
                }
            }

            // Find the stars we do not need to scout with an armed scout (eg home world)
            List<StarIntel> excludedStars = new List<StarIntel>();
            foreach (StarIntel report in turnData.EmpireState.StarReports.Values)
            {
                if ((report.Year != Global.Unset) && (report.MinValue(clientState.EmpireState.Race) < 50)
                    && ((report.Owner == clientState.EmpireState.Id)||(report.Owner == Global.Nobody))
                    )
                {
                    excludedStars.Add(report);
                }
            }

            if (armedScoutFleets.Count > 0)
            {
                foreach (Fleet fleet in armedScoutFleets)
                {
                    StarIntel starToScout = fleetAIs[fleet.Id].ArmedScout(excludedStars);
                    if (starToScout != null)
                    {
                        excludedStars.Add(starToScout);
                    }
                }
            }
        }
        private void HandleColonizing()
        {
            List<Fleet> colonyShipsFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.CanColonize == true && fleet.Waypoints.Count == 1)
                {
                    colonyShipsFleets.Add(fleet);
                }
            }

            if (colonyShipsFleets.Count > 0)
            {
                // check if there is any good star to colonize
                foreach (StarIntel report in turnData.EmpireState.StarReports.Values) //Let's cherry pick nice stars first!
                {
                    if (report.Year != Global.Unset && clientState.EmpireState.Race.HabitalValue(report) > 0.5 && report.Owner == Global.Nobody && report.mineralRich())
                    {
                        Fleet found = null;
                        foreach (Fleet fleet in colonyShipsFleets)
                        {
                            if (fleet.canReach(report,clientState.EmpireState.Race))
                            {
                                found = fleet;
                                break;
                            }
                        }
                        if (found != null)
                        {
                            // send fleet to colonise
                            fleetAIs[colonyShipsFleets.IndexOf(found)].Colonise(report);
                            colonyShipsFleets.RemoveAt(colonyShipsFleets.IndexOf(found));
                        }
                        if (colonyShipsFleets.Count == 0)
                        {
                            break;
                        }

                    }
                }
                foreach (StarIntel report in turnData.EmpireState.StarReports.Values)
                {
                    if (report.Year != Global.Unset && clientState.EmpireState.Race.HabitalValue(report) > 0 && report.Owner == Global.Nobody)
                    {
                        if (colonyShipsFleets.Count > 0)
                        {
                            Fleet colonyFleet = colonyShipsFleets[0];
                            // send fleet to colonise
                            fleetAIs[colonyFleet.Id].Colonise(report);
                            colonyShipsFleets.RemoveAt(0);
                            if (colonyShipsFleets.Count == 0)
                            {
                                break;
                            }
                            colonyFleet = colonyShipsFleets[0];
                        }
                    }
                }
            }
        }

        /// <Summary>
        /// Move surplus population to somewhere where it is usefull.
        /// Maintaining a stars capacity around 55% gives the empire the best growth rate
        /// </Summary>
        private void HandlePopulationSurplus()
        {
            List<Fleet> idleTransportFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.CanColonize == false && fleet.Waypoints.Count == 1 && fleet.Cargo.Mass == 0 && fleet.TotalCargoCapacity != 0)
                {
                    idleTransportFleets.Add(fleet);
                }
            }
            List<Star> underPopulated = new List<Star>();
            foreach (Star star in clientState.EmpireState.OwnedStars.Values)
            {
                if (star.Capacity(clientState.EmpireState.Race) < 25)   //if less than 50% growth is reduced
                    underPopulated.Add(star);
            }

            foreach (Star source in clientState.EmpireState.OwnedStars.Values)
            {
                if (source.Capacity(clientState.EmpireState.Race) > 50)   //if more than 50% growth is reduced
                {
                    int surplusPopulationKT = (int)((source.Colonists - source.MaxPopulation(clientState.EmpireState.Race) / 2) / Global.ColonistsPerKiloton); // maintain population at 50% - best growth rate
                    while (surplusPopulationKT > 0)
                    {
                        bool found = false;
                        List<Fleet> occupiedFleets = new List<Fleet>();
                        Fleet nextTransport = null;
                        while (!found)
                        {
                            foreach (Fleet transport in idleTransportFleets)
                                if (transport.Position == source.Position)
                                {
                                    found = true;
                                    nextTransport = transport;
                                    break;
                                }
                        }
                        if (found) //there is a fleet in orbit so use it
                        {
                            foreach (Star target in underPopulated)
                                if (nextTransport.canCurrentlyReach(target, clientState.EmpireState.Race))
                                {
                                    WaypointCommand loadCargo = null;
                                    if (surplusPopulationKT > nextTransport.TotalCargoCapacity)
                                        loadCargo = nextTransport.LoadWaypoint(source, nextTransport.TotalCargoCapacity);
                                    else loadCargo = nextTransport.LoadWaypoint(source, surplusPopulationKT);
                                    loadCargo.ApplyToState(clientState.EmpireState);
                                    clientState.Commands.Push(loadCargo);

                                    SendFleet(target, nextTransport, new CargoTask());
                                    surplusPopulationKT = surplusPopulationKT - nextTransport.TotalCargoCapacity;
                                    occupiedFleets.Add(nextTransport);
                                    if (surplusPopulationKT <= 0) break;
                                }
                        }
                        else // there are no fleets in orbit so send one there
                        {
                            foreach (Fleet transport in idleTransportFleets)
                                if (nextTransport.canCurrentlyReach(source, clientState.EmpireState.Race))
                                {
                                    found = true;
                                    nextTransport = transport;
                                    break;
                                }
                            SendFleet(source, nextTransport, new CargoTask());
                            surplusPopulationKT = surplusPopulationKT - nextTransport.Cargo.Mass;
                            occupiedFleets.Add(nextTransport);
                            if (surplusPopulationKT <= 0) break;
                        }
                        foreach (Fleet occupied in occupiedFleets) idleTransportFleets.Remove(occupied);
                    }
                }
            }
        }
        private void HandleFuelImpoverishedFleets()
        {
            foreach (Message msg in clientState.Messages)
            {
                List<Fleet> idleRefuelFleets = new List<Fleet>();
                foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
                {
                    if (fleet.CanRefuel == false && fleet.Waypoints.Count == 0 && fleet.Cargo.Mass == 0 && fleet.TotalCargoCapacity != 0)
                    {
                        idleRefuelFleets.Add(fleet);
                    }
                }
                if (!string.IsNullOrEmpty(msg.Type) && msg.Type == "Fuel")
                {
                    if (clientState.EmpireState.OwnedFleets.ContainsKey(msg.FleetID))
                    {
                        Fleet crippled = clientState.EmpireState.OwnedFleets[msg.FleetID];
                        int distanceToCrippled = int.MaxValue;
                        uint refuelerID = 0;
                        Fleet chosenRefueler = null;
                        foreach (Fleet refueler in idleRefuelFleets)
                        {
                            if ((refueler.distanceTo(crippled) < distanceToCrippled) && (refueler.Waypoints.Count == 0))
                            {
                                distanceToCrippled = (int)refueler.distanceTo(crippled);
                                refuelerID = refueler.Id;
                                chosenRefueler = refueler;
                            }
                        }
                        if (chosenRefueler != null)
                        {
                            Waypoint waypoint = new Waypoint();
                            waypoint.Destination = crippled.Name;
                            waypoint.WarpFactor = chosenRefueler.SlowestEngine;
                            FuelTransferTask fuelTransferTask = new FuelTransferTask();
                            fuelTransferTask.Target = crippled;
                            (fuelTransferTask.Target as Fleet).Id = crippled.Id;
                            fuelTransferTask.Amount.Value = int.MaxValue;
                            waypoint.Task = fuelTransferTask;
                            chosenRefueler.Waypoints.Add(waypoint);
                        }
                    }
                }
            }
        }


        private void HandleWarpSpeedRecalcRequest() //Something made the fleet ask for it's warp speed to be recalculated
        {                                           //Maybe it got refueled or it's target moved or it's RamScoop engines have overfilled its fuel tanks
            foreach (Message msg in clientState.Messages)
            {
                if (!string.IsNullOrEmpty(msg.Type) && msg.Type == "WarpToChange")
                {
                    if (clientState.EmpireState.OwnedFleets.ContainsKey(msg.FleetID))
                    {
                        Fleet fleet = clientState.EmpireState.OwnedFleets[msg.FleetID];
                        // for the AI the first Waypoint is the destination not the current location?
                        fleet.Waypoints[0].WarpFactor = fleet.SlowestEngine; //TODO if the destination is a hostile planet don't increase speed
                    }
                }
            }
        }
        private void HandleOrphaned() //Something caused the fleet to get "Lost In Space"
        {                           // It has no waypoints and wants a waypoint
            foreach (Message msg in clientState.Messages)
            {
                if (!string.IsNullOrEmpty(msg.Type) && msg.Type == "DestToChange")
                {
                    if (clientState.EmpireState.OwnedFleets.ContainsKey(msg.FleetID))
                    {
                        Fleet fleet = clientState.EmpireState.OwnedFleets[msg.FleetID];
                        if (fleet.Waypoints.Count > 0) return;
                        // for the AI the first Waypoint is the destination not the current location?

                        int distanceToStar = int.MaxValue;
                        Star chosenStar = null;
                        foreach (Star nextStar in clientState.EmpireState.OwnedStars.Values)
                        {
                            if (fleet.distanceTo(nextStar) < distanceToStar) 
                            {
                                distanceToStar = (int)fleet.distanceTo(nextStar);
                                chosenStar = nextStar;
                            }
                        }
                        if (chosenStar != null)
                        {
                            Waypoint waypoint = new Waypoint();
                            waypoint.Task = new NoTask();
                            waypoint.Destination = chosenStar.ToString();
                            if ((fleet.FreeWarpSpeed > 1) && (!chosenStar.Starbase.CanRefuel)) waypoint.WarpFactor = fleet.FreeWarpSpeed - 1; //fastest rate of fuel generation (based on available evidence)
                            else waypoint.WarpFactor = fleet.SlowestEngine;
                        }

                    }
                }
            }
        }

        /// <Summary>
        /// Manage research.
        /// Only changes research field after completing the previous research level.
        /// </Summary>
        private void HandleResearch()
        {
            // Generate a research command to describe the changes.
            ResearchCommand command = new ResearchCommand();
            command.Topics.Zero();
            // Set the percentage of production to dedicate to research
            command.Budget = 0;

            // check if messages contains info about tech advence. Could be more than one, so use a flag to prevent setting the research level multiple times.
            bool hasAdvanced = false;
            foreach (Message msg in clientState.Messages)
            {
                if (!string.IsNullOrEmpty(msg.Type) && msg.Type == "TechAdvance")
////                if (!string.IsNullOrEmpty(msg.Type) && msg.Text.Contains("Your race has advanced to Tech Level") == true)  // can be removed if the previous line works
                {
                    hasAdvanced = true;
                }
            }

            if (hasAdvanced)
            {
                // pick next topic
                int minLevel = int.MaxValue;
                Nova.Common.TechLevel.ResearchField targetResearchField = TechLevel.ResearchField.Weapons; // default to researching weapons

                if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] < 3)
                {
                    // Prop 3 - Long Hump 6 - Warp 6 engine (or fuel mizer at Prop 2)
                    targetResearchField = TechLevel.ResearchField.Propulsion;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Electronics] < 1)
                {
                    // Elec 1 - Rhino Scanner - 50 ly scan
                    targetResearchField = TechLevel.ResearchField.Electronics;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] < 3)
                {
                    // Cons 3 - Destroyer & Medium Freighter
                    targetResearchField = TechLevel.ResearchField.Construction;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] < 5)
                {
                    // Long Hump 7
                    targetResearchField = TechLevel.ResearchField.Propulsion;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Electronics] < 5)
                {
                    // Elec 5 - Scanners
                    targetResearchField = TechLevel.ResearchField.Electronics;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Weapons] < 6)
                {
                    // Wep 6 - Beta Torp (@5) and Yakimora Light Phaser
                    targetResearchField = TechLevel.ResearchField.Weapons;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] < 7)
                {
                    // Prop 7 - Warp 8 engine
                    targetResearchField = TechLevel.ResearchField.Propulsion;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] < 6)
                {
                    // Cons 6 - Frigate
                    targetResearchField = TechLevel.ResearchField.Construction;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Biotechnology] < 4)
                {
                    // Bio 4 - Unlock terraform and prep for mines
                    targetResearchField = TechLevel.ResearchField.Biotechnology;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Energy] < 3)
                {
                    // Energy 3 - Mines and shields
                    targetResearchField = TechLevel.ResearchField.Energy;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] < 9)
                {
                    // Cons 9 - Cruiser
                    targetResearchField = TechLevel.ResearchField.Construction;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Energy] < 6)
                {
                    // Energy 6 - Shields
                    targetResearchField = TechLevel.ResearchField.Energy;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Weapons] < 12)
                {
                    // Weapons 12 - Jihad Missile
                    targetResearchField = TechLevel.ResearchField.Weapons;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] < 13)
                {
                    // Cons 13 - Battleships
                    targetResearchField = TechLevel.ResearchField.Construction;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Energy] < 11)
                {
                    // Energy 11 - Bear Neutrino at 10, and unlocks Syncro Sapper (need weapons 21)
                    targetResearchField = TechLevel.ResearchField.Energy;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Electronics] < 11)
                {
                    // Elect 11 - Jammer 20 and Super Computer
                    targetResearchField = TechLevel.ResearchField.Electronics;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] < 12)
                {
                    // Prop 12 - Warp 10 and Overthruster
                    targetResearchField = TechLevel.ResearchField.Propulsion;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Biotechnology] < 7)
                {
                    // Bio 7 maybe - scanners, Anti-matter generator, smart bombs
                    targetResearchField = TechLevel.ResearchField.Biotechnology;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Weapons] < 24)
                {
                    // Weapons 24 - research all remaining weapons technologies
                    targetResearchField = TechLevel.ResearchField.Weapons;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] < 26)
                {
                    // Cons 26 - Nubian
                    targetResearchField = TechLevel.ResearchField.Construction;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Electronics] < 19)
                {
                    // Elect 19 - Battle nexus
                    targetResearchField = TechLevel.ResearchField.Electronics;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Energy] < 22)
                {
                    // Energy 22 - Complete Phase Shield
                    targetResearchField = TechLevel.ResearchField.Energy;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] < 23)
                {
                    // Prop 23 - Trans-Star 10
                    targetResearchField = TechLevel.ResearchField.Propulsion;
                }
                else if (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Biotechnology] < 10)
                {
                    // Bio 10 - RNA Scanner
                    targetResearchField = TechLevel.ResearchField.Biotechnology;
                }
                else
                {
                    // research lowest tech field
                    for (TechLevel.ResearchField field = TechLevel.FirstField; field <= TechLevel.LastField; field++)
                    {
                        if (clientState.EmpireState.ResearchLevels[field] < minLevel)
                        {
                            minLevel = clientState.EmpireState.ResearchLevels[field];
                            targetResearchField = field;
                        }
                    }
                }
                command.Topics[targetResearchField] = 1;
            }

            if (command.IsValid(clientState.EmpireState))
            {
                clientState.Commands.Push(command);
                command.ApplyToState(clientState.EmpireState);
            }
        }


        private void HandleShipDesign()
        {
            AllComponents allComponents = new AllComponents(true, "A.I. shipdesign");
            designScouts(allComponents.Fetch("Scout"), "Long Range Scout");
            if ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 3) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 5)) designColonizers(allComponents.Fetch("Medium Freighter"), "Medium Santa Maria", allComponents.Fetch("Colonization Module"));
            if ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 8) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 7)) designColonizers(allComponents.Fetch("Large Freighter"), "Large Santa Maria", allComponents.Fetch("Colonization Module"));
            if ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 3) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 5)) designColonizers(allComponents.Fetch("Medium Freighter"), "Medium Freighter", clientState.EmpireState.AvailableComponents.GetBestFuelTank());
            if ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 8) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 7)) designColonizers(allComponents.Fetch("Large Freighter"), "Large Freighter", clientState.EmpireState.AvailableComponents.GetBestScanner());
            Component BattleCruiserHull = null;
            Component cruiserHull = null;
            Component frigateHull = null;
            Component destroyerHull = null;

            foreach (Component component in clientState.EmpireState.AvailableComponents.Values)
            {
                if ((component.Properties.ContainsKey("Hull")) && (component.Name == "Battle Cruiser")) BattleCruiserHull = component;
                if ((component.Properties.ContainsKey("Hull")) && (component.Name == "Cruiser")) cruiserHull = component;
                if ((component.Properties.ContainsKey("Hull")) && (component.Name == "Destroyer")) destroyerHull = component;
                if ((component.Properties.ContainsKey("Hull")) && (component.Name == "Frigate")) frigateHull = component;
            }

            if (destroyerHull != null) designDestroyers(destroyerHull, " Pointy Stick");
            if (cruiserHull != null) designDestroyers(cruiserHull, " Spear");
            if (BattleCruiserHull != null) designDestroyers(BattleCruiserHull, " Dr Death");
            if (frigateHull != null) designFrigate(frigateHull, " Mosquito");
            if (cruiserHull != null) designFrigate(cruiserHull, " Bee");
            if (BattleCruiserHull != null) designFrigate(BattleCruiserHull, " Light Saber");
            if (clientState.EmpireState.AvailableComponents.GetBestRefuelerHull() != null)
                aiPlan.currentRefuelerDesign = designRefuelers(clientState.EmpireState.AvailableComponents.GetBestRefuelerHull(), " Mobile Mobil");
            else aiPlan.currentRefuelerDesign = designRefuelers(allComponents.Fetch("Scout"), " Mobile Mobil ");
            if (clientState.EmpireState.AvailableComponents.GetBestRepairerHull() != null) designRefuelers(clientState.EmpireState.AvailableComponents.GetBestRefuelerHull(), " Grease Monkey");

        }

        private void designDestroyers(Component Hull, String designPrefix)
        {
            Component engine = clientState.EmpireState.AvailableComponents.GetBestEngine(Hull, true);
            Component torpedo = clientState.EmpireState.AvailableComponents.GetBestTorpedo();
            Engine engineSpecs = engine.Properties["Engine"] as Engine;
            bool found = false;
            String designName = torpedo.Name + designPrefix + "(" + engineSpecs.OptimalSpeed.ToString() + ")";
            foreach (ShipDesign ship in clientState.EmpireState.Designs.Values) if (ship.Name == designName) found = true;
            if (!found)
            {
                ShipDesign destroyer = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
                destroyer.Blueprint = Hull;
                foreach (HullModule module in destroyer.Hull.Modules)
                {
                    if (module.ComponentType == "Engine")
                    {
                        module.AllocatedComponent = engine as Component;
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if (module.ComponentType == "Scanner")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestScanner(true);
                        module.ComponentCount = 1;
                    }
                    else if ((module.ComponentType == "General Purpose") || (module.ComponentType == "Weapon"))
                    {
                        module.AllocatedComponent = torpedo;
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if ((module.ComponentType == "Mechanical") || (module.ComponentType == "Scanner Electrical Mechanical") )
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestManeuveringJet();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if  ((module.ComponentType == "Electrical") || (module.ComponentType == "Shield Electrical Mechanical"))
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestBattleComputer();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if ((module.ComponentType == "Armor") || (module.ComponentType == "Shield or Armor"))
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestMobileArmour();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if (module.ComponentType == "Shield")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestShield();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                }
                destroyer.Icon = new ShipIcon(Hull.ImageFile, (System.Drawing.Bitmap)Hull.ComponentImage);

                destroyer.Type = ItemType.Ship;
                destroyer.Name = designName;
                destroyer.Update();
                DesignCommand command = new DesignCommand(CommandMode.Add, destroyer);
                if (command.IsValid(clientState.EmpireState))
                {
                    clientState.Commands.Push(command);
                    command.ApplyToState(clientState.EmpireState);
                }

            }

        }
        private void designFrigate(Component Hull, String designPrefix)
        {
            Component engine = clientState.EmpireState.AvailableComponents.GetBestEngine(Hull, false);
            Component beam = clientState.EmpireState.AvailableComponents.GetBestBeamWeapon();
            Engine engineSpecs = engine.Properties["Engine"] as Engine;
            bool found = false;
            String designName = beam.Name + designPrefix + "(" + engineSpecs.OptimalSpeed.ToString() + ")";
            foreach (ShipDesign ship in clientState.EmpireState.Designs.Values) if (ship.Name == designName) found = true;
            if (!found)
            {
                ShipDesign destroyer = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
                destroyer.Blueprint = Hull;
                foreach (HullModule module in destroyer.Hull.Modules)
                {
                    if (module.ComponentType == "Engine")
                    {
                        module.AllocatedComponent = engine as Component;
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if (module.ComponentType == "Scanner")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestScanner(true);
                        module.ComponentCount = 1;
                    }
                    else if ((module.ComponentType == "General Purpose") || (module.ComponentType == "Weapon"))
                    {
                        module.AllocatedComponent = beam;
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if ((module.ComponentType == "Mechanical") || (module.ComponentType == "Scanner Electrical Mechanical") || (module.ComponentType == "Shield Electrical Mechanical"))
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestManeuveringJet();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if ((module.ComponentType == "Electrical") )
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestCapacitor();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if (module.ComponentType == "Armor") 
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestMobileArmour();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if ((module.ComponentType == "Shield") || (module.ComponentType == "Shield or Armor"))
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestShield();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                }
                destroyer.Icon = new ShipIcon(Hull.ImageFile, (System.Drawing.Bitmap)Hull.ComponentImage);

                destroyer.Type = ItemType.Ship;
                destroyer.Name = designName;
                destroyer.Update();
                DesignCommand command = new DesignCommand(CommandMode.Add, destroyer);
                if (command.IsValid(clientState.EmpireState))
                {
                    clientState.Commands.Push(command);
                    command.ApplyToState(clientState.EmpireState);
                }

            }

        }

        private void designScouts(Component Hull, String designPrefix)
        {
            Component engine = clientState.EmpireState.AvailableComponents.GetBestEngine(Hull, true);
            Engine engineSpecs = engine.Properties["Engine"] as Engine;
            bool found = false;
            String designName = designPrefix + "(" + engineSpecs.OptimalSpeed.ToString() + ")";
            foreach (ShipDesign ship in clientState.EmpireState.Designs.Values) if (ship.Name == designName) found = true;
            if (!found)
            {
                ShipDesign scout = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
                scout.Blueprint = Hull;
                foreach (HullModule module in scout.Hull.Modules)
                {
                    if (module.ComponentType == "Engine")
                    {
                        module.AllocatedComponent = engine as Component;
                        module.ComponentCount = 1;
                    }
                    else if (module.ComponentType == "Scanner")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestScanner(true);
                        module.ComponentCount = 1;
                    }
                    else if (module.ComponentType == "General Purpose")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestFuelTank();
                        module.ComponentCount = 1;
                    }
                }
                scout.Icon = new ShipIcon(Hull.ImageFile, (System.Drawing.Bitmap)Hull.ComponentImage);

                scout.Type = ItemType.Ship;
                scout.Name = designName;
                scout.Update();
                DesignCommand command = new DesignCommand(CommandMode.Add, scout);
                if (command.IsValid(clientState.EmpireState))
                {
                    clientState.Commands.Push(command);
                    command.ApplyToState(clientState.EmpireState);
                }

            }

        }

        private ShipDesign designRefuelers(Component Hull, String designPrefix)
        {
            // in the early game the A.I. will no doubt make mistakes and get fleets stuck somewhere without fuel
            // a fleet that manufactures fuel can be sent to rescue those fleets
            // in the later game the Refueller hull serves multiple purposes when mingled into a fleet (mostly repair but fuel generation is handy)
            // don't build anything that can't make fuel or it will run out of fuel somewhere
            bool makesFuel = false;
            if (Hull.Properties.ContainsKey("Fuel"))
            {
                Fuel fuelProperty = Hull.Properties["Fuel"] as Fuel;
                makesFuel = (fuelProperty.Generation > 0);
            }

            Component engine = clientState.EmpireState.AvailableComponents.GetBestEngine(Hull, !makesFuel); //If the hull makes fuel then get a fast engine otherwise get a Ramscoop
            Engine engineSpecs = engine.Properties["Engine"] as Engine;
            if ((!engineSpecs.RamScoop) &&  (!makesFuel)) return null;
            bool found = false;
            String designName = designPrefix + "(" + engineSpecs.OptimalSpeed.ToString() + ")";
            ShipDesign refueler = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
            foreach (ShipDesign ship in clientState.EmpireState.Designs.Values) if (ship.Name == designName)
                {
                    found = true;
                    refueler = ship;
                }
            if (!found)
            {
                refueler.Blueprint = Hull;
                foreach (HullModule module in refueler.Hull.Modules)
                {
                    if (module.ComponentType == "Engine")
                    {
                        module.AllocatedComponent = engine as Component;
                        module.ComponentCount = module.ComponentMaximum;
                    }
                    else if (module.ComponentType == "Scanner")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestScanner(true);
                        module.ComponentCount = 1;
                    }
                    else if (module.ComponentType == "General Purpose")
                    {
                        module.AllocatedComponent = clientState.EmpireState.AvailableComponents.GetBestFuelTank();
                        module.ComponentCount = module.ComponentMaximum;
                    }
                }
                refueler.Icon = new ShipIcon(Hull.ImageFile, (System.Drawing.Bitmap)Hull.ComponentImage);

                refueler.Type = ItemType.Ship;
                refueler.Name = designName;
                refueler.Update();
                DesignCommand command = new DesignCommand(CommandMode.Add, refueler);
                if (command.IsValid(clientState.EmpireState))
                {
                    clientState.Commands.Push(command);
                    command.ApplyToState(clientState.EmpireState);
                }

            }
            return refueler;

        }
        private void designColonizers(Component Hull, String designPrefix, Component colonyModule)
        {
            Component engine = clientState.EmpireState.AvailableComponents.GetBestEngine(Hull, true);
            Engine engineSpecs = engine.Properties["Engine"] as Engine;
            bool found = false;
            String designName = designPrefix + "(" + engineSpecs.OptimalSpeed.ToString() + ")";
            foreach (ShipDesign ship in clientState.EmpireState.Designs.Values) if (ship.Name == designName) found = true;
            if (!found)
            {
                ShipDesign coloniser = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
                coloniser.Blueprint = Hull;
                foreach (HullModule module in coloniser.Hull.Modules)
                {
                    if (module.ComponentType == "Engine")
                    {
                        module.AllocatedComponent = engine as Component;
                        module.ComponentCount = 1;
                    }
                    else if ((module.ComponentType == "Mechanical")|| (module.ComponentType == "Scanner Electrical Mechanical")) 
                    {
                        module.AllocatedComponent = colonyModule;
                        module.ComponentCount = 1;
                    }
                }
                coloniser.Icon = new ShipIcon(Hull.ImageFile, (System.Drawing.Bitmap)Hull.ComponentImage);

                coloniser.Type = ItemType.Ship;
                coloniser.Name = designName;
                coloniser.Update();
                DesignCommand command = new DesignCommand(CommandMode.Add, coloniser);
                if (command.IsValid(clientState.EmpireState))
                {
                    clientState.Commands.Push(command);
                    command.ApplyToState(clientState.EmpireState);
                }
            }

        }

        private void SendFleet(Star star, Fleet fleet, IWaypointTask task)
        {
            Waypoint w = new Waypoint();
            w.Position = star.Position;
            w.WarpFactor = fleet.SlowestEngine;
            w.Destination = star.Name;
            w.Task = task;

            WaypointCommand command = new WaypointCommand(CommandMode.Add, w, fleet.Key);
            command.ApplyToState(clientState.EmpireState);
            clientState.Commands.Push(command);
        }
    }
}
