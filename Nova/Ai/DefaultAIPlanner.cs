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
        public int TransportCount = 0; //driven by cargo waiting for transport
        public int RefuelerCount = 0;  
        public int RepairerCount = 0;  //At a guess we want 1 per 10 Bombercover
        public int BomberCount = 0;
        public int DefenderCount = 0; // Hard to calculate without History. 
        public int BomberCoverCount = 0;
        public int MineSweeperCount = 0;
        public int MineLayerCount = 0;
        public int VisibleMineFields = 0;
        public int EuthanasiaTargetRating = 0;  //Rating of the Starbase at the enemy planet that we are envading, Total BomberCover rating must be 300% of this because we take damage on the way there then every turn we spend bombing 
        public int EuthanasiaTargetPopulation = 0;  //Population of the enemy planet that we are envading, the less turns it takes to euthanise them the higher the probability of success

        // EmpireWide weighting to be placed on production of this item 
        //  For example if a planet has a lot of boronium and can produce 10 bombers in 5 years or 3 destroyers in 5 years
        // whilst its neighbour has a lot of ironium and can produce 10 destroyers in 5 years or 1 bomber in 5 years
        // the first planet multiplies the 10 bombers by the "worth to the empire" of each bomber (2) to get 20 points if 
        // it produces bombers or 3 x 5 points if it produces destroyers so so it makes bombers.
        // similarly the second planets scores are 2x1 points if it makes bombers or 10x5 if it makes destroyers so it makes destroyers
        // if the empire needs more destroyers and changes the weighting of destroyers to 10 then both planets will choose to make destroyers

        // default weightings  - as the AI gets into the end game it will adjust these as needed
        public int interceptorProductionPriority = 5; //5 where max = 100 - if the AI feels more threatened it will increase this
        public int starbaseUpgradePriority = 30; 
        public int coloniserProductionPriority = 10 ;
        public int mineLayerProductionPriority = 5;
        public int mineSweeperProductionPriority = 5;
        public int bomberProductionPriority = 3;
        public int bomberCoverProductionPriority = 5;
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
        public ShipDesign currentTransportDesign = null;
        public ShipDesign currentColoniserDesign = null;
        public ShipDesign currentScoutDesign = null;
        public ShipDesign currentDefenderDesign = null; //general purpose defense / interceptor
        public ShipDesign currentBomberDesign = null;
        public ShipDesign currentBomberCoverDesign = null; //sole purpose is to protect bombers and kill Starbases
        public ShipDesign currentRefuelerDesign = null;
        public ShipDesign currentRepairerDesign = null;// sole purpose is to repair the currentBomberCoverDesign 
        public ShipDesign currentMineLayerDesign = null;
        public ShipDesign currentMineSweeperDesign = null;
        public ShipDesign currentStarbaseDesign = null;
        public ShipDesign currentMinimalStarbaseDesign = null;

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
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values) if (design.Name.Contains("Scout")) scoutDesign = design;  //designs from StarMapInitializer.PrepareDesigns which don't use global race ship names
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values) if (design.Name.Contains("Long Range Scout")) scoutDesign = design; //designs from StarMapInitializer.PrepareDesigns which don't use global race ship names
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values) if (design.Name.Contains(Global.AiScout)) scoutDesign = design; //only one design with the prefix - when scouts get destroyed the victor gains part of the tech difference so use low tech scouts with lots of fuel
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
                    List<ShipDesign> colonizers = new List<ShipDesign>();
                    foreach (ShipDesign design in clientState.EmpireState.Designs.Values)
                    {
                        if (design.CanColonize)
                        {
                            colonizers.Add(design);
                        }
                    }
                    if (colonizers.Count > 0) colonizerDesign = colonizers[0];
                    foreach (ShipDesign design in colonizers)
                    {
                        if ((design.CargoCapacity > ColonizerDesign.CargoCapacity) ||
                            ((design.CargoCapacity == ColonizerDesign.CargoCapacity) &&(design.BattleSpeed > ColonizerDesign.BattleSpeed)))
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
        public ShipDesign defenseDesign 
        {
            get
            {
                if (currentDefenderDesign == null)
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
                    currentDefenderDesign = armedDesign;
                    return armedDesign;
                }
                else return currentDefenderDesign;
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
                        } 
                    }
                    transportDesign.Icon = new ShipIcon(freighterHull.ImageFile, (Bitmap)freighterHull.ComponentImage);

                    transportDesign.Type = ItemType.Ship;
                    transportDesign.Name = "Large Freighter";
                    transportDesign.Update();

                    // add the design 
                    // clientState.EmpireState.Designs[transportDesign.Key] = transportDesign;

                    DesignCommand command = new DesignCommand(CommandMode.Add, transportDesign);

                    Message message;
                    if (command.IsValid(clientState.EmpireState,out message))
                    {
                        clientState.Commands.Push(command);               //queue the design command so it is done on the Server
                        command.ApplyToState(clientState.EmpireState);    // also do it on the client so the client can build the design immediately
                    }

                    return transportDesign;
                }

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
            else if (fleet.Name.Contains("Scout") || (fleet.Name.Contains("Long Range Scout") || (fleet.Name.Contains(Global.AiScout))))
            {
                this.ScoutCount++;
            }
            else if ((fleet.Name.Contains(Global.AiRefueler)) && (fleet.Waypoints.Count > 1))
            {
                this.RefuelerCount++;
            }
            else if ((fleet.Name.Contains(Global.AiRepairer)) && (fleet.Waypoints.Count > 1))
            {
                this.RepairerCount++;
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
            else if ((fleet.Name.Contains(Global.AiDefensiveBattleCruiser))|| (fleet.Name.Contains(Global.AiDefensiveCruiser)) || (fleet.Name.Contains(Global.AiDefensiveDestroyer)))
            {
                this.DefenderCount++;
            }
        }
    }
}
