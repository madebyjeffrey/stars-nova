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
    using System.Drawing;
    using System.Linq;
    using System.Text;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using Nova.Common.Waypoints;

 

    /// <summary>
    /// An AI sub-component to manage planning AI moves. 
    /// </summary>
    /// <remarks>
    /// The default AI is stateless - it does not persist any information between turns other than what is in an ordinary player's state.
    /// </remarks>
    public class DefaultAIPlanner
    {
        public const int EarlyScouts = 5;
        public const int LowProduction = 100;

        /// <summary>
        /// Minimum hab value for colonizing a planet.
        /// </summary>
        public const double MinHabValue = 0.05;

        public int ScoutCount = 0;
        public int ColonizerCount = 0;
        public int TransportCount = 0;
        public int BomberCount = 0;
        public int WarfleetCount = 0;
        private int SurplusPopulationKT
        { 
            get
            {
                int surplusPopulation = 0;
                foreach (Star star in clientState.EmpireState.OwnedStars.Values)
                {
                    if (star.Capacity(clientState.EmpireState.Race) > 50) surplusPopulation = surplusPopulation + (star.Colonists - star.MaxPopulation(clientState.EmpireState.Race) / 2); // maintain population at 50% - best growth rate
                }
                return surplusPopulation / Global.ColonistsPerKiloton;
            }
        }
        /// <summary>
        /// Backing store for the TotalTransportKt property.
        /// </summary>
        private int totalTransportKt = 0;

        private ClientData clientState = null;

        /// <summary>
        /// The ShipDesign to use for building scouts.
        /// </summary>
        private ShipDesign scoutDesign = null;

        /// <summary>
        /// The ShipDesign to use for building colonizers.
        /// </summary>
        private ShipDesign colonizerDesign = null;

        /// <summary>
        /// The ShipDesign to use for building transports.
        /// </summary>
        private ShipDesign transportDesign = null;
        private ShipDesign currentTransportDesign = null;
        private ShipDesign currentArmedDesign = null;
        private ShipDesign currentBomberDesign = null;
        private ShipDesign currentRefuelerDesign = null;
        private ShipDesign currentMineLayerDesign = null;
        private ShipDesign currentMineSweeperDesign = null;

        /// <summary>
        /// The number of scouted, unowned planets with > 5% habitability.
        /// </summary>
        /// <remarks>
        /// Initialized to -1 to flag it has not been calculated. 
        /// </remarks>
        private int planetsToColonize = -1;
        
        /// <summary>
        /// The current design to be used for building scouts.
        /// </summary>
        public ShipDesign ScoutDesign
        {
            get
            {
                if (scoutDesign == null)
                {
                    ShipDesign longRangeScout = null;
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Scout"))
                        {
                            scoutDesign = design;
                        }
                        if (design.Name.Contains("Long Range Scout"))
                        {
                            scoutDesign = longRangeScout;
                        }
                    }
                    if (longRangeScout != null) scoutDesign = longRangeScout;
                }
                return scoutDesign;
            }
        }

        /// <summary>
        /// The current design to be used for building colonizers.
        /// </summary>
        public ShipDesign ColonizerDesign
        {
            get
            {
                if (colonizerDesign == null)
                {
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Santa Maria"))
                        {
                            colonizerDesign = design;
                        }
                    }
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Medium Santa Maria"))
                        {
                            colonizerDesign = design;
                        }
                    }
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Large Santa Maria"))
                        {
                            colonizerDesign = design;
                        }
                    }
                }
                return colonizerDesign;
            }
        }

        /// <summary>
        /// Track the number of known planets suitable for sending a colonizer too.
        /// </summary>
        public int PlanetsToColonize
        {
            get
            {
                if (planetsToColonize < 0)
                {
                    planetsToColonize = 0;
                    foreach (StarIntel star in clientState.EmpireState.StarReports.Values)
                    {
                        if (star.Owner == Global.Nobody && clientState.EmpireState.Race.HabitalValue(star) > DefaultAIPlanner.MinHabValue)
                        {
                            planetsToColonize++;
                        }
                    }
                }
                return planetsToColonize;
            }
        }

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        public DefaultAIPlanner(ClientData newClientState)
        {
            clientState = newClientState;
        }

        /// <summary>
        /// Property to track the total capacity of transport fleets.
        /// </summary>
        public int TotalTransportKt
        {
            get
            {
                return totalTransportKt;
            }
        }


        public ShipDesign AnyTransportDesign
        {
            get
            {
                if (currentTransportDesign == null)
                {
                    ShipDesign transportDesign = null;
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Large Freighter"))
                        {
                            if (transportDesign == null) transportDesign = design;
                            if (design.Engine.RamScoop && !transportDesign.Engine.RamScoop) transportDesign = design; //if the new design has ram scoop engines use it
                            if ((design.Engine.RamScoop && transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                            if ((!transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                        }
                    }
                    if (transportDesign == null)
                    {
                        foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                        {
                            if (design.Name.Contains("Medium Freighter"))
                            {
                                if (transportDesign == null) transportDesign = design;
                                if (design.Engine.RamScoop && !transportDesign.Engine.RamScoop) transportDesign = design; //if the new design has ram scoop engines use it
                                if ((design.Engine.RamScoop && transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                                if ((!transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                            }
                        }
                    }
                    currentTransportDesign = transportDesign;
                    return transportDesign;
                }
                else return currentTransportDesign;
            }
        }
        public ShipDesign AnyArmedDesign
        {
            get
            {
                if (currentArmedDesign == null)
                {
                    ShipDesign armedDesign = null;
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Cruiser"))
                        {
                            if (armedDesign == null) armedDesign = design;
                            if (design.Engine.RamScoop && !armedDesign.Engine.RamScoop) armedDesign = design; //if the new design has ram scoop engines use it
                            if ((design.Engine.RamScoop && armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                            if ((!armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                        }
                    }
                    if (armedDesign == null)
                    {
                        foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                        {
                            if (design.Name.Contains("Frigate"))
                            {
                                if (armedDesign == null) armedDesign = design;
                                if (design.Engine.RamScoop && !armedDesign.Engine.RamScoop) armedDesign = design; //if the new design has ram scoop engines use it
                                if ((design.Engine.RamScoop && armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                                if ((!armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                            }
                        }
                    }
                    if (armedDesign == null)
                    {
                        foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                        {
                            if (design.Name.Contains("Destroyer"))
                            {
                                if (armedDesign == null) armedDesign = design;
                                if (design.Engine.RamScoop && !armedDesign.Engine.RamScoop) armedDesign = design; //if the new design has ram scoop engines use it
                                if ((design.Engine.RamScoop && armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                                if ((!armedDesign.Engine.RamScoop) && (armedDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) armedDesign = design;
                            }
                        }
                    } // if still null should we arm a scout?
                    currentArmedDesign = armedDesign;
                    return armedDesign;
                }
                else return currentArmedDesign;
            }
        }


        public ShipDesign TransportDesign
        {


            get
            {
                Component freighterHull = null;
                Component engine = null;
                if (transportDesign == null)
                {
                    ShipDesign transportDesign = null;
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.Name.Contains("Large Freighter"))
                        {
                            if (transportDesign == null) transportDesign = design;
                            if (design.Engine.RamScoop && !transportDesign.Engine.RamScoop) transportDesign = design; //if the new design has ram scoop engines use it
                            if ((design.Engine.RamScoop && transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                            if ((!transportDesign.Engine.RamScoop) && (transportDesign.Engine.OptimalSpeed < design.Engine.OptimalSpeed)) transportDesign = design;
                        }
                    }
                    return transportDesign;

                }



                /* TODO - a better transport?
                else if (clientState.EmpireState.ResearchLevels > new TechLevel(0, 0, 0, 11, 0, 8))
                {
                    // build a really good transport
                    // Super Freighter (Cons 13)? - Not cost effective
                    // Large Freighter (cons 8), Interspace 10 engine (prop 11), 
                }
                 */
                else if (clientState.EmpireState.ResearchLevels > new TechLevel(0, 0, 0, 7, 0, 8))
                {
                    // build a good transport
                    // Large Freighter (cons 8), Alpha Drive 8 (prop 7)

                    if (!clientState.EmpireState.AvailableComponents.TryGetValue("Large Freighter", out freighterHull))
                    {
                        throw new System.NotImplementedException();
                    }
                    if (!clientState.EmpireState.AvailableComponents.TryGetValue("Alpha Drive 8", out engine))
                    {
                        throw new System.NotImplementedException();
                    }

                    transportDesign = new ShipDesign(clientState.EmpireState.GetNextDesignKey());
                    transportDesign.Blueprint = freighterHull;
                    foreach (HullModule module in transportDesign.Hull.Modules)
                    {
                        if (module.ComponentType == "Engine")
                        {
                            module.AllocatedComponent = engine;
                            module.ComponentCount = 2;
                        } /* TODO Cargo Pod?
                        else if (module.ComponentType == "Mechanical")
                        {
                            module.AllocatedComponent = cargoPod;
                            module.ComponentCount = 1;
                        }*/
                    }
                    transportDesign.Icon = new ShipIcon(freighterHull.ImageFile, (Bitmap)freighterHull.ComponentImage);

                    transportDesign.Type = ItemType.Ship;
                    transportDesign.Name = "Large Freighter";
                    transportDesign.Update();

                    // add the design - done by the command???
                    // clientState.EmpireState.Designs[transportDesign.Key] = transportDesign;

                    DesignCommand command = new DesignCommand(CommandMode.Add, transportDesign);

                    if (command.IsValid(clientState.EmpireState))
                    {
                        clientState.Commands.Push(command);
                        command.ApplyToState(clientState.EmpireState);
                    }

                    return transportDesign;
                }
                    /* TODO - a medium transport?
                else if (clientState.EmpireState.ResearchLevels > new TechLevel(0, 0, 0, 3, 0, 3))
                {
                    // build a minimal transport
                    // Medium Freighter (cons 3), Long Hump 6 (prop 3)
                }
                     */
                else
                {
                    // do not build transports - tech too low
                    return null;
                }
            }
        }

        public int TransportKtRequired
        {
            get
            {
                return this.SurplusPopulationKT;
                //return 5000;
            }
        }

        /// <summary>
        /// Count and classify owned fleets.
        /// </summary>
        /// <param name="fleet"></param>
        public void CountFleet(Fleet fleet)
        {
            // Work out what we have
            if (fleet.CanColonize)
            {
                this.ColonizerCount++;
            }
            else if (fleet.Name.Contains("Scout")|| (fleet.Name.Contains("Long Range Scout")))
            {
                this.ScoutCount++;
            }
            else if (fleet.HasBombers)
            {
                this.BomberCount++;
            }
            else if (fleet.TotalCargoCapacity > 0)
            {
                this.TransportCount++;
                this.totalTransportKt += fleet.TotalCargoCapacity;
            }
            else if (fleet.IsArmed)
            {
                this.WarfleetCount++;
            }
        }
    }
}
