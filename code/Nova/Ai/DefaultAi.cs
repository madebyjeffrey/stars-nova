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
        /// The "AI" just uses Simple heuristics and the differences between races is accomplished by adjusting
        /// each Races proclivities.
        /// This AI can largely adapt to new environments (new component lists) but still fails if 
        /// the names of hulls are changed. :(
        /// TODO use another method of picking hulls for each task instead of referring to hulls by name!
        /// </summary>
        public override void DoMove()
        {
            aiPlan = new DefaultAIPlanner(clientState);

            // create the helper AIs
            foreach (Star star in clientState.EmpireState.OwnedStars.Values)
            {
                if (star.Owner == clientState.EmpireState.Id)
                {
                    clientState.EmpireState.StarReports[star.Name].Update(star, ScanLevel.Owned, clientState.EmpireState.TurnYear, true);
                    DefaultPlanetAI planetAI = new DefaultPlanetAI(star, clientState, this.aiPlan);
                    planetAIs.Add(star.Key, planetAI);
                }
            }

            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.Owner == clientState.EmpireState.Id)
                {
                    aiPlan.CountFleet(fleet);
                    if (((fleet.Waypoints.Count == 0) || ((fleet.Waypoints.Count == 1) && fleet.Waypoints[0].Task is NoTask && ((fleet.InOrbit != null) && (fleet.InOrbit.Name == fleet.Waypoints[0].Destination)))) || ((fleet.Name.Contains("Scout") || (fleet.Name.Contains("Long Range Scout") || (fleet.Name.Contains(Global.AiScout))))))
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

            HandleShipDesign(); //load designs into AI plan once so other modules can use them
            HandleProduction();
            HandleResearch();
            HandleScouting();
            HandleArmedScouting();
            HandleColonizing();
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
                if ((fleet.Name.Contains("Scout") || (fleet.Name.Contains("Long Range Scout") || (fleet.Name.Contains(Global.AiScout)))))
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

        /// <summary>
        /// Similar to early scouting but send fleets to prime stars to verify that they are still OK and scout any stars that our early scouts 
        /// failed to reach (scouts may have been killed by other races)
        /// </summary>
        private void HandleArmedScouting()
        {
            List<Fleet> armedScoutFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if ((fleet.Name.Contains(Global.AiDefensiveDestroyer) == true) && ((fleet.Waypoints.Count == 0) || ((fleet.Waypoints.Count == 1) && fleet.Waypoints[0].Task is NoTask && ((fleet.InOrbit != null) && (fleet.InOrbit.Name == fleet.Waypoints[0].Destination)))))
                {
                    armedScoutFleets.Add(fleet);
                }
            }

            // Find the stars we do not need to scout with an armed scout (eg home world)
            List<StarIntel> excludedStars = new List<StarIntel>();
            foreach (StarIntel report in turnData.EmpireState.StarReports.Values)
            {
                if ((report.Year != Global.Unset) || ((report.MinValue(clientState.EmpireState.Race) < 50)
                    && (report.Owner != clientState.EmpireState.Id) && (report.Owner != Global.Nobody)))

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

        /// <summary>
        /// Look for good targets to send colonisers to and look for any colonisers that could reach that target.
        /// If we still have colonisers left send them to mediocre targets.
        /// </summary>
        private void HandleColonizing()
        {
            List<Fleet> colonyShipsFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)  //find idle colonisers
            {
                if ((fleet.CanColonize && (fleet.Waypoints.Count == 0) && (fleet.FuelAvailable == fleet.TotalFuelCapacity) && (fleet.InOrbit != null) )
                    || (fleet.CanColonize && (fleet.FuelAvailable >= fleet.TotalFuelCapacity)&&(fleet.Waypoints.Count == 1) && fleet.Waypoints[0].Task is NoTask && (fleet.InOrbit != null) && (fleet.InOrbit.Name == fleet.Waypoints[0].Destination)))
                {
                    colonyShipsFleets.Add(fleet);
                }
            }
            List<string> beingColonised = new List<string>();
            foreach (Fleet fleet in turnData.EmpireState.OwnedFleets.Values)  // ignore Stars that have a coloniser going to them
            {
                String destination = "";
                if (fleet.CanColonize)
                {
                    foreach (Waypoint dest in fleet.Waypoints)
                        if ((dest.Task is ColoniseTask) && (fleet.Cargo.Mass > 0)) destination = dest.Destination;
                }
                if (destination != "")
                {
                    clientState.EmpireState.StarReports.TryGetValue(destination, out StarIntel star);
                    if (star != null) beingColonised.Add(star.Name);
                }
            }

            if (colonyShipsFleets.Count > 0)
            {
                // check if there are any good stars to colonize
                foreach (StarIntel report in turnData.EmpireState.StarReports.Values) //Let's cherry pick nice stars first!
                {
                    if ((report.Year != Global.Unset && clientState.EmpireState.Race.HabitalValue(report) > 0.5 && report.Owner == Global.Nobody && report.mineralRich())
                        && (!beingColonised.Contains(report.Name)))
                    {
                        Fleet found = null;
                        foreach (Fleet fleet in colonyShipsFleets)
                        {
                            if (fleet.canReach(report, clientState.EmpireState.Race))
                            {
                                found = fleet;
                                break;
                            }
                        }
                        if (found != null)
                        {
                            // send fleet to colonise
                            fleetAIs[(colonyShipsFleets[colonyShipsFleets.IndexOf(found)]).Id].Colonise(report);
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
                        foreach (Fleet fleet in colonyShipsFleets) if ((!beingColonised.Contains(report.Name)) && fleet.canReach(report, clientState.EmpireState.Race) && (fleet.Waypoints.Count < 2))
                            {
                                // send fleet to colonise
                                fleetAIs[fleet.Id].Colonise(report);
                                colonyShipsFleets.RemoveAt(colonyShipsFleets.IndexOf(fleet));
                                break;
                            }
                    }
                }
            }
            if (colonyShipsFleets.Count > 0)
            {                                             //the colonizers apparently can't reach the destination so merge a fueler with the colony ships.
                List<Fleet> idleRefuelFleets = new List<Fleet>();
                foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
                    if (fleet.Name.Contains(Global.AiRefueler))
                    {
                        if (!fleet.CanColonize && fleet.CanRefuel == false && ((fleet.Waypoints.Count == 0) || ((fleet.Waypoints.Count == 1) && fleet.Waypoints[0].Task is NoTask && ((fleet.InOrbit != null) && (fleet.InOrbit.Name == fleet.Waypoints[0].Destination)))))
                        {
                            idleRefuelFleets.Add(fleet);
                        }
                    }
                if (idleRefuelFleets.Count > 0)
                    foreach (Fleet coloniser in colonyShipsFleets)
                    {
                        int index = 0;
                        bool found = false;
                        while ((idleRefuelFleets.Count > 0) && (index <= idleRefuelFleets.Count - 1) && !found)
                        {
                            if (coloniser.Position != idleRefuelFleets[index].Position)
                            {
                                index++;
                            }
                            else
                            {
                                Fleet refueler = idleRefuelFleets[index];
                                idleRefuelFleets.Remove(refueler);
                                WaypointCommand command = refueler.merge(coloniser);
                                clientState.Commands.Push(command);
                                found = true;
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
            int maxTransportCapacity = 0;
            List<Fleet> idleTransportFleets = new List<Fleet>();
            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.CanColonize == false &&
                    ((fleet.Waypoints.Count == 0) || ((fleet.Waypoints.Count == 1) && fleet.Waypoints[0].Task is NoTask && ((fleet.InOrbit != null) && (fleet.InOrbit.Name == fleet.Waypoints[0].Destination)) && fleet.Cargo.Mass == 0 && fleet.TotalCargoCapacity != 0)))
                {
                    idleTransportFleets.Add(fleet);
                    if (fleet.TotalCargoCapacity > maxTransportCapacity) maxTransportCapacity = fleet.TotalCargoCapacity;
                }
            }
            List<string> underPopulated = new List<string>();
            foreach (Star star in clientState.EmpireState.OwnedStars.Values)
            {
                if (star.Capacity(clientState.EmpireState.Race) < 25)   //if less than 50% growth is reduced
                    underPopulated.Add(star.Name);
            }
            foreach (StarIntel star in clientState.EmpireState.StarReports.Values)
            {
                if ((star.Owner != 0) && (star.Owner != clientState.EmpireState.Id) && (star.Colonists < (maxTransportCapacity / 2)) && (star.MinValue(clientState.EmpireState.Race) > 50))
                    underPopulated.Add(star.Name);  //throw in some invade tasks 
            }
            if (underPopulated.Count > 0)
            {
                foreach (Star source in clientState.EmpireState.OwnedStars.Values)
                {
                    if (source.Capacity(clientState.EmpireState.Race) > 45)   //if more than 50% growth is reduced
                    {
                        int surplusPopulationKT = (int)((source.Colonists - source.MaxPopulation(clientState.EmpireState.Race) * 0.45) / Global.ColonistsPerKiloton); // maintain population at 50% - best growth rate

                        bool found = false;
                        List<Fleet> occupiedFleets = new List<Fleet>();
                        Fleet nextTransport = null;
                        foreach (Fleet transport in idleTransportFleets) if ((transport.Position == source.Position) && (surplusPopulationKT > 0))
                            {
                                found = true;
                                nextTransport = transport;

                                foreach (string target in underPopulated)
                                    if (nextTransport.canReach(clientState.EmpireState.StarReports[target], clientState.EmpireState.Race))
                                    {
                                        WaypointCommand loadCargo = null;
                                        if (surplusPopulationKT > nextTransport.TotalCargoCapacity)
                                            loadCargo = nextTransport.LoadWaypoint(source, nextTransport.TotalCargoCapacity);
                                        else loadCargo = nextTransport.LoadWaypoint(source, surplusPopulationKT);
                                        loadCargo.ApplyToState(clientState.EmpireState);
                                        clientState.Commands.Push(loadCargo);

                                        CargoTask unload = new CargoTask(int.MaxValue);
                                        unload.Target = clientState.EmpireState.StarReports[target];
                                        SendFleet(clientState.EmpireState.StarReports[target], nextTransport, unload);
                                        surplusPopulationKT = surplusPopulationKT - nextTransport.TotalCargoCapacity;
                                        occupiedFleets.Add(nextTransport);
                                        if (surplusPopulationKT <= 0) break;
                                        underPopulated.RemoveAt(underPopulated.IndexOf(target)); //send next transport to another planet
                                        break; // nextTransport sent so leave this loop 
                                        if (underPopulated.Count == 0) surplusPopulationKT = 0;  // no more targets
                                    }
                            }


                        if (surplusPopulationKT > 0) // there are not enough fleets in orbit so send one there
                        {
                            foreach (Fleet transport in idleTransportFleets)
                                if (transport.canCurrentlyReach(clientState.EmpireState.StarReports[source.Name], clientState.EmpireState.Race))
                                {
                                    found = true;
                                    nextTransport = transport;
                                    break;
                                }
                            if (found)
                            {
                                SendFleet(clientState.EmpireState.StarReports[source.Name], nextTransport, new CargoTask(int.MaxValue));
                                surplusPopulationKT = surplusPopulationKT - nextTransport.Cargo.Mass;
                                occupiedFleets.Add(nextTransport);
                                if (surplusPopulationKT <= 0) break;
                            }
                            else
                            {                   //if not found wait until a better fleet appears
                                surplusPopulationKT = 0;
                            }
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
                commandArguments = new CommandArguments(args);
                Console.WriteLine("Nova AI");
                if (commandArguments.Count < 3)
                {
                    Console.WriteLine("Usage: Nova --ai -r <race_name> -t <turn_number> -i <intel_file>");
                    return;
                }

                raceName = commandArguments[CommandArguments.Option.RaceName];
                turnNumber = int.Parse(commandArguments[CommandArguments.Option.Turn], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                Console.WriteLine("Usage: Nova --ai -r <race_name> -t <turn_number> -i <intel_file>");
                return;
                
            }
            // read in race data
            Console.WriteLine("Playing turn {0} for race \"{1}\".", turnNumber, raceName);
            try
            {
                // TODO (priority 6) - bypass password entry for AI.
                // Note: passwords have currently been disabled completely, awaiting a new more effective implementation - Dan 02 Mar 10
                ClientState.Initialize(commandArguments.ToArray()); 
            }
            catch
            {
                Console.WriteLine("Nova_AI encountered an error reading its intel.");
                return;
            }

            // play turn
            // Currently just builds factories/mines/defenses
            try
            {
                Intel turnData = ClientState.Data.InputTurn;

                // currently does nothing: This is where the AI propper should do its work.
                foreach (Star star in ClientState.Data.PlayerStars.Values)
                {
                    star.ManufacturingQueue.Queue.Clear();
                    ProductionQueue.Item item = new ProductionQueue.Item();
                    Design design;

                    // build factories (limited by Germanium, and don't want to use it all)
                    if (star.ResourcesOnHand.Germanium > 50)
                    {
                        item.Name = "Factory";
                        item.Quantity = (int)((star.ResourcesOnHand.Germanium - 50) / 5);
                        item.Quantity = Math.Max(0, item.Quantity);

                        design = turnData.AllDesigns[ClientState.Data.RaceName + "/" + item.Name] as Design;

                        item.BuildState = design.Cost;

                        star.ManufacturingQueue.Queue.Add(item);

                    }

                    // build mines
                    item = new ProductionQueue.Item();
                    item.Name = "Mine";
                    item.Quantity = 100;
                    design = turnData.AllDesigns[ClientState.Data.RaceName + "/" + item.Name] as Design;
                    item.BuildState = design.Cost;
                    star.ManufacturingQueue.Queue.Add(item);

                    // build defenses
                    int defenceToBuild = Global.MaxDefenses - star.Defenses;
                    item = new ProductionQueue.Item();
                    item.Name = "Defenses";
                    item.Quantity = defenceToBuild;
                    design = turnData.AllDesigns[ClientState.Data.RaceName + "/" + item.Name] as Design;
                    item.BuildState = design.Cost;
                    star.ManufacturingQueue.Queue.Add(item);
                }

            }
            catch (Exception)
            {
                Report.FatalError("AI failed to take proper actions.");
            }

            // save turn)
            try
            {
                OrderWriter.WriteOrders();
            }
            catch
            {
                Console.WriteLine("Nova_AI encountered an error writing its orders.");
            }

            return;
        }
    }
}
