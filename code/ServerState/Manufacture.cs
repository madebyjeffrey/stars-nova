#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project
//
// This file is part of Stars! Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

namespace Nova.Server
{
    using System.Collections.Generic;
    
    using Nova.Common;
    using Nova.Common.Components;
    using System;
    using Nova.Common.Commands;
    using Nova.Common.Waypoints;

    /// <summary>
    /// Class to manufacture the items in a star's queue.
    /// </summary>
    public class Manufacture
    {
        private ServerData serverState;

        public Manufacture(ServerData serverState)
        {
            this.serverState = serverState;
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Manufacture the items in a production queue (resources permitting).
        /// </summary>
        /// <param name="star">The star doing production.</param>
        /// <remarks>
        /// Don't preserve resource count as resource depletion is needed to
        /// contribute with leftover resources for research.
        /// </remarks>
        public void Items(Star star,out List <Message> messageOut, Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {
            List<Message> messages = new List<Message>();
            List<Message> messageList = new List<Message>();

            List<ProductionOrder> completed = new List<ProductionOrder>();
            
            foreach (ProductionOrder productionOrder in star.ManufacturingQueue.Queue)
            {
                if ((star.Starbase == null) && (productionOrder.Unit is ShipProductionUnit))
                {                               //if the planet has no StarBase then there should be no fleets in the queue (other than Starbases)
                    ShipDesign design = null;   // so if there are don't let them block the queue
                    serverState.AllEmpires[star.Owner].Designs.TryGetValue((productionOrder.Unit as ShipProductionUnit).DesignKey, out design);
                    if (!design.IsStarbase) continue;
                }

                if (productionOrder.IsBlocking(star, race, gravityModCapability, radiationModCapability, temperatureModCapability))
                    if (serverState.AllPlayers[star.Owner-1].AiProgram == "Human") // the AI WILL make mistakes that a human would recognise so nothing blocks the queue for AI
                    {
                        // Items block the queue when they can't be processed (i.e. not enough resources)
                        // AND they are not autobuild orders (autobuild never blocks the Queue).
                        break;
                    }
                    else continue;


                // Deal with the production Order.

                int done = productionOrder.Process(star,out messages, race, gravityModCapability, radiationModCapability, temperatureModCapability);
                messageList.AddRange(messages);
                messages.Clear();
                if (done > 0 && productionOrder.Unit is ShipProductionUnit)
                {
                    long designKey = (productionOrder.Unit as ShipProductionUnit).DesignKey;
                    ushort owner = (productionOrder.Unit as ShipProductionUnit).DesignKey.Owner();
                    
                    CreateShips(serverState.AllEmpires[owner].Designs[designKey], star, done);
                }
                    
                if (productionOrder.Quantity == 0)
                {
                    completed.Add(productionOrder);
                }
            }
            
            foreach (ProductionOrder done in completed)
            {
                star.ManufacturingQueue.Queue.Remove(done);
            }
            messageOut = messageList;
        }
        


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Create a new ship or starbase at the specified location. Starbases are
        /// handled just like ships except that they cannot move.
        /// </summary>
        /// <param name="design">A ShipDesign to be constructed.</param>
        /// <param name="star">The star system producing the ship.</param>
        private void CreateShips(ShipDesign design, Star star, int countToBuild)
        {
            design.Update();
            EmpireData empire = serverState.AllEmpires[star.Owner];

            if ((design.IsStarbase) && (star.Starbase != null)) empire.RemoveFleet(star.Starbase);
            ShipToken token = new ShipToken(design, countToBuild);
            
            Fleet fleet = new Fleet(token, star, empire.GetNextFleetKey());
            
            fleet.Name = design.Name + " #" + fleet.Id;
            fleet.FuelAvailable = fleet.TotalFuelCapacity;

            Message message = new Message();
            message.Audience = star.Owner;
            message.Event = star;
            message.Type = "Star";
            message.FleetKey = fleet.Key;
            message.Text = star.Name + " has produced " + countToBuild + " new " + design.Name;
            serverState.AllMessages.Add(message);
            
            // Add the fleet to the state data so it can be tracked.
            serverState.AllEmpires[fleet.Owner].AddOrUpdateFleet(fleet);

            //"Mineral Packet"
            if ((design.Type == ItemType.Salvage) && (design.Name == "Mineral Packet"))
            {
                Cargo minerals = new Cargo();
                minerals.Silicoxium = fleet.TotalCargoCapacity;
                fleet.Cargo.Add(minerals);

                //TODO mineral packet has random destination at the moment
                Random random = new Random();
                int dest = random.Next(1, serverState.AllEmpires.Count);
                if (dest == fleet.Owner) dest = serverState.AllEmpires.Count - 1;
                Star destination = null;
                foreach (Star lamb in serverState.AllEmpires[dest].OwnedStars.Values)
                {
                    destination = lamb;
                    break;
                }

                Waypoint waypoint = new Waypoint();
                waypoint.Destination = star.ToString();
                waypoint.WarpFactor = 0;
                waypoint.Task = new NoTask();
                waypoint.Position = star.Position;
                WaypointCommand command = new WaypointCommand(CommandMode.Add, waypoint, fleet.Key, 0);


                if (command.IsValid(empire, out message))
                {
                    message = command.ApplyToState(empire);
                    if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                }
                else if (Global.Debug) Report.Information(message.Text);

                waypoint = new Waypoint();
                waypoint.Destination = destination.ToString();
                waypoint.WarpFactor = 10;
                waypoint.Task = new NoTask();
                waypoint.Position = destination.Position;
                WaypointCommand command2 = new WaypointCommand(CommandMode.Add, waypoint, fleet.Key, 0);

                if (command2.IsValid(empire, out message))
                {
                    message = command2.ApplyToState(empire);
                    if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                }
                else if (Global.Debug) Report.Information(message.Text);
            }

            if (design.Type == ItemType.Starbase)
            {
                if (star.Starbase != null)
                {
                    // Old starbases are not scrapped. Instead, the reduced
                    // upgrade cost should have already been factored when first
                    // queuing the "upgrade", so the old SB is just
                    // discarded and replaced at this point. -Aeglos 2 Aug 11
                    star.Starbase = null;
                    // waypointTasks.Scrap(star.Starbase, star, false);
                }
                star.Starbase = fleet;
                fleet.Type = ItemType.Starbase;
                fleet.Name = star.Name + " " + fleet.Type;
                fleet.InOrbit = star;

                if (empire.Race.HasTrait("ISB"))
                {
                    fleet.Cloaked = 20;
                }

            }
            else
            {
                fleet.InOrbit = star;
            }
        }
    }
}