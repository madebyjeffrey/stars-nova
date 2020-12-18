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


    /// <summary>
    /// A helper object for the default AI for managing planets.
    /// </summary>
    public class DefaultPlanetAI
    {
        private const int FactoryProductionPrecedence = 0;
        private const int MineProductionPrecedence = 1;
        private ClientData clientState;
        private DefaultAIPlanner aiPlan = null;

        private Star planet;
        private ShipDesign transportDesign = null;

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="newStar">The planet the ai is to manage.</param>
        public DefaultPlanetAI(Star newStar, ClientData newState, DefaultAIPlanner newAIPlan)
        {
            planet = newStar;
            clientState = newState;
            aiPlan = newAIPlan;
        }

        public void HandleProduction()
        {
            // keep track of the position in the production queue
            int productionIndex = 0;
            Message message = null;
            // Clear the current manufacturing queue (except for partially built ships/starbases).
            Queue<ProductionCommand> clearProductionList = new Queue<ProductionCommand>();
            foreach (ProductionOrder productionOrderToclear in this.planet.ManufacturingQueue.Queue)
            {
                if ((productionOrderToclear.Unit.Cost == productionOrderToclear.Unit.RemainingCost) && !(productionOrderToclear.Unit is ShipProductionUnit))
                {
                    ProductionCommand clearProductionCommand = new ProductionCommand(CommandMode.Delete, productionOrderToclear, this.planet.Key);
                    if (clearProductionCommand.IsValid(clientState.EmpireState,out message))
                    {
                        // Put the items to be cleared in a queue, as the actual cleanup can not be done while iterating the list.
                        clearProductionList.Enqueue(clearProductionCommand);
                        clientState.Commands.Push(clearProductionCommand);
                    }
                }
                else
                {
                    productionIndex++;
                }
            }

            foreach (ProductionCommand clearProductionCommand in clearProductionList)
            {
                clearProductionCommand.ApplyToState(clientState.EmpireState);
            }
            //if population is over 55% capacity and we don't have mediumm freighters yet then just research until we do!
            //if population is over 80% capacity and we don't have large freighters yet then just research until we do!
            if ((planet.Capacity(clientState.EmpireState.Race) < 55) || ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 3) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 5)))
            {
                productionIndex = BuildTransport(productionIndex);
            }
            if (((planet.Capacity(clientState.EmpireState.Race) < 55) || ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 3) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 5)))
                && ((planet.Capacity(clientState.EmpireState.Race) < 80) || ((clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Construction] >= 8) && (clientState.EmpireState.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 7))))
            {
                Double earlyProductionMultiplier = 1.0; // Rush factories for 1st 8 years
                if (clientState.EmpireState.TurnYear > 2106) earlyProductionMultiplier = 0.3; //then Rush some scouts
                if (clientState.EmpireState.TurnYear > 2115) earlyProductionMultiplier = 0.5; //then build a mix of stuff
                if (clientState.EmpireState.TurnYear > 2120) earlyProductionMultiplier = 0.6;
                // build factories (limited by Germanium, and don't want to use it all)
                if (this.planet.ResourcesOnHand.Germanium > 50)
                {
                    int factoryBuildCostGerm = clientState.EmpireState.Race.HasTrait("CF") ? 3 : 4;
                    int factoriesToBuild = (int)((this.planet.ResourcesOnHand.Germanium - 50) / factoryBuildCostGerm);
                    if (factoriesToBuild > (int)(this.planet.GetOperableFactories() * earlyProductionMultiplier - this.planet.Factories))
                    {
                        factoriesToBuild = (int)(this.planet.GetOperableFactories() * earlyProductionMultiplier) - this.planet.Factories;
                    }

                    if (factoriesToBuild > 0)
                    {
                        ProductionOrder factoryOrder = new ProductionOrder(factoriesToBuild, new FactoryProductionUnit(clientState.EmpireState.Race), false);
                        ProductionCommand factoryCommand = new ProductionCommand(CommandMode.Add, factoryOrder, this.planet.Key, FactoryProductionPrecedence);
                        productionIndex++;
                        if (factoryCommand.IsValid(clientState.EmpireState,out message))
                        {
                            factoryCommand.ApplyToState(clientState.EmpireState);
                            this.clientState.Commands.Push(factoryCommand);
                        }
                    }
                }
                // Min Terraform
                if (planet.MinValue(clientState.EmpireState.Race) < 10)
                {
                    ProductionOrder terraformOrder1 = new ProductionOrder(100, new TerraformProductionUnit(clientState.EmpireState.Race), false);
                    ProductionCommand terraformCommand1 = new ProductionCommand(CommandMode.Add, terraformOrder1, this.planet.Key,productionIndex);
                    productionIndex++;
                    if (terraformCommand1.IsValid(clientState.EmpireState, out message))
                    {
                        terraformCommand1.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(terraformCommand1);
                    }
                }

                // build mines
                int maxMines = (int)(this.planet.GetOperableMines() * earlyProductionMultiplier);
                if (this.planet.Mines < maxMines)
                {
                    ProductionOrder mineOrder = new ProductionOrder(maxMines - this.planet.Mines, new MineProductionUnit(clientState.EmpireState.Race), false);
                    ProductionCommand mineCommand = new ProductionCommand(CommandMode.Add, mineOrder, this.planet.Key, Math.Min(MineProductionPrecedence, productionIndex));
                    productionIndex++;
                    if (mineCommand.IsValid(clientState.EmpireState,out message))
                    {
                        mineCommand.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(mineCommand);
                    }
                }
                // Max Terraform

                if ((planet.Capacity(clientState.EmpireState.Race) < 25) || (this.planet.Starbase != null))
                {
                    ProductionOrder terraformOrder = new ProductionOrder(100, new TerraformProductionUnit(clientState.EmpireState.Race), false);
                    ProductionCommand terraformCommand = new ProductionCommand(CommandMode.Add, terraformOrder, this.planet.Key, productionIndex);
                    productionIndex++;

                    if (terraformCommand.IsValid(clientState.EmpireState, out message))
                    {
                        terraformCommand.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(terraformCommand);
                    }
                }
                

                // Build ships
                productionIndex = BuildShips(productionIndex);

                // build defenses
                int defenseToBuild = Global.MaxDefenses - this.planet.Defenses;
                if (defenseToBuild > 0)
                {
                    ProductionOrder defenseOrder = new ProductionOrder(defenseToBuild, new DefenseProductionUnit(), false);
                    ProductionCommand defenseCommand = new ProductionCommand(CommandMode.Add, defenseOrder, this.planet.Key, productionIndex);
                    productionIndex++;
                    if (defenseCommand.IsValid(clientState.EmpireState, out message))
                    {
                        defenseCommand.ApplyToState(clientState.EmpireState);
                        //clientState.Commands.Push(defenseCommand); // build defenses in specific circumstances NOT on EVERY planet?
                    }
                }

            }
            else
            {
                // Report.Information("A.I. planet capacity is high and A.I. is devoting itself to researching better freighters");
            }
        }

        /// <summary>
        /// 
        /// </summary>


        public ShipDesign TransportDesign
        {

            get
            {
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

                }
                return transportDesign;
            }
        }


        private int BuildShips(int productionIndex)
        {
            if (this.planet.Starbase == null) productionIndex = BuidMinimalStarbase(productionIndex);
            else
            {
                productionIndex = BuildScout(productionIndex);
                productionIndex = BuildColonizer(productionIndex);
                productionIndex = BuildTransport(productionIndex);
                productionIndex = BuidSuitableFleet(productionIndex);
                productionIndex = BuildRefueler(productionIndex);
            }
            return productionIndex;
        } // Build ships

        /// <summary>
        /// Add a scout to the production queue, if required and we can afford it.
        /// </summary>
        /// <param name="productionIndex">The current insertion point into the planet's production queue.</param>
        /// <returns>The updated productionIndex.</returns>
        private int BuildScout(int productionIndex)
        {
            int earlyScouts = (int)Math.Max(DefaultAIPlanner.EarlyScouts, clientState.EmpireState.StarReports.Count / 8);
            if (this.planet.GetResourceRate() > DefaultAIPlanner.LowProduction && this.aiPlan.ScoutCount < earlyScouts)
            {
                if (this.aiPlan.ScoutDesign != null)
                {
                    ProductionOrder scoutOrder = new ProductionOrder(1, new ShipProductionUnit(this.aiPlan.ScoutDesign,planet.Name,clientState.EmpireState), false);
                    ProductionCommand scoutCommand = new ProductionCommand(CommandMode.Add, scoutOrder, this.planet.Key, productionIndex);
                    Message message;
                    if (scoutCommand.IsValid(clientState.EmpireState,out message))
                    {
                        scoutCommand.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(scoutCommand);
                        productionIndex++;
                    }
                }
            }
            return productionIndex;
        } // BuildScouts()

        /// <summary>
        /// Add a colonizer to the production queue, if required and we can afford it.
        /// </summary>
        /// <param name="productionIndex">The current insertion point into the planet's production queue.</param>
        /// <returns>The updated productionIndex.</returns>
        /// <remarks>
        /// Always make one spare colonizer.
        /// </remarks>
        private int BuildColonizer(int productionIndex)
        {
            if (this.planet.GetResourceRate() > DefaultAIPlanner.LowProduction && this.aiPlan.ColonizerCount < (this.aiPlan.PlanetsToColonize))
            {
                if ((clientState.EmpireState.TurnYear % 5 ) == 0 && (this.aiPlan.ColonizerDesign != null)) for (int fleet = 1;fleet <10;fleet++) // build colonisers in batches with 5 years between batches so we send only one to each destination and wait 5 years for it to arrive before sending a second one
                {
                    ProductionOrder colonizerOrder = new ProductionOrder(1, new ShipProductionUnit(this.aiPlan.ColonizerDesign,planet.Name,clientState.EmpireState), false);
                    ProductionCommand colonizerCommand = new ProductionCommand(CommandMode.Add, colonizerOrder, this.planet.Key, productionIndex);
                    Message message;
                    if (colonizerCommand.IsValid(clientState.EmpireState, out message))
                    {
                        colonizerCommand.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(colonizerCommand);
                        productionIndex++;
                    }
                    else if (Global.Debug) Report.Information(message.Text);
                }
            }
            return productionIndex;
        } // BuildColonizer()


        /// <summary>
        /// Add a transport to the production queue, if required and we can afford it.
        /// </summary>
        /// <param name="productionIndex">The current insertion point into the planet's production queue.</param>
        /// <returns>The updated productionIndex.</returns>
        /// <remarks>
        /// How many transports do we need? - Let the aiPlan decide.
        /// </remarks>
        private int BuildTransport(int productionIndex)
        {
            if ((this.aiPlan.AnyTransportDesign != null) && (this.planet.Starbase != null))
                if (this.planet.Starbase.TotalDockCapacity > this.aiPlan.AnyTransportDesign.Mass)

                    if ((this.planet.GetResourceRate() > DefaultAIPlanner.LowProduction) && (this.planet.Capacity(clientState.EmpireState.Race) > 25) && !(this.planet.HasFreeTransportInOrbit))
                    {
                        {
                            ProductionOrder transportOrder = new ProductionOrder(1, new ShipProductionUnit(this.aiPlan.AnyTransportDesign,planet.Name,clientState.EmpireState), false);
                            ProductionCommand transportCommand = new ProductionCommand(CommandMode.Add, transportOrder, this.planet.Key, productionIndex);
                            Message message;
                            if (transportCommand.IsValid(clientState.EmpireState, out message))
                            {
                                transportCommand.ApplyToState(clientState.EmpireState);
                                clientState.Commands.Push(transportCommand);
                                productionIndex++;
                            }
                            else if (Global.Debug) Report.Information(message.Text);
                        }
                    }
            return productionIndex;
        } // BuildTransport()

        private int BuildRefueler(int productionIndex)
        {
            if (this.planet.GetResourceRate() > DefaultAIPlanner.LowProduction && !this.planet.HasRefuelerInOrbit)
            {
                if (this.aiPlan.currentRefuelerDesign != null)
                {
                    ProductionOrder refuelerOrder = new ProductionOrder(1, new ShipProductionUnit(this.aiPlan.currentRefuelerDesign,planet.Name,clientState.EmpireState), false);
                    ProductionCommand refuelerCommand = new ProductionCommand(CommandMode.Add, refuelerOrder, this.planet.Key, productionIndex);
                    Message message;
                    if (refuelerCommand.IsValid(clientState.EmpireState, out message))
                    {
                        refuelerCommand.ApplyToState(clientState.EmpireState);
                        clientState.Commands.Push(refuelerCommand);
                        productionIndex++;
                    }
                    else if (Global.Debug) Report.Information(message.Text);
                }
            }
            return productionIndex;
        } // BuildRefueler()

        private double howManyCanIBuild(ShipDesign fleet, Resources currentStarbase = null)
        //how many of this design can this planet build in 8 years
        {
            Resources resourcesOnHand = new Resources(planet.ResourcesOnHand);
            Resources nextOneYears = new Resources(planet.GetMiningRate(planet.MineralConcentration.Ironium), planet.GetMiningRate(planet.MineralConcentration.Boranium), planet.GetMiningRate(planet.MineralConcentration.Germanium), planet.GetFutureResourceRate(0));
            resourcesOnHand = resourcesOnHand + 8 * nextOneYears;  //rough approximation
            double howMany = 0;
            if (fleet.IsStarbase)
            {
                if (currentStarbase == null)
                {  //fleet
                    if (fleet != null) howMany = resourcesOnHand / fleet.Cost;
                }
                else
                {  //Starbase upgrade
                    if (fleet != null)
                    {
                        if (fleet.Cost == currentStarbase) howMany = 0;
                        else howMany = resourcesOnHand / (fleet.Cost - currentStarbase);
                    }
                }
            }
            else howMany = resourcesOnHand / fleet.Cost; // Not Starbase
            return howMany;
        }
        private int BuidSuitableFleet(int productionIndex) //Based on this planets resources and the empire needs
        {
            int queueYears;
            int queueEnergy = 0;
            foreach (ProductionOrder order in this.planet.ManufacturingQueue.Queue)
            {
                queueEnergy += (order.NeededResources() as Resources).Energy;
            }
            queueYears = queueEnergy / Math.Max(1, this.planet.ResourcesOnHand.Energy);
            if (queueYears > 1) return productionIndex;
            Resources currentStarbase = new Resources();
            if (this.planet.Starbase != null) currentStarbase = this.planet.Starbase.TotalCost;

            double defenseFleets = howManyCanIBuild(aiPlan.currentDefenderDesign) * this.aiPlan.interceptorProductionPriority;
            double bomberFleets = howManyCanIBuild(aiPlan.currentBomberDesign) * this.aiPlan.bomberProductionPriority;
            double bomberCoverFleets = howManyCanIBuild(aiPlan.currentBomberCoverDesign) * this.aiPlan.bomberCoverProductionPriority;
            double starStation = howManyCanIBuild(aiPlan.currentStarbaseDesign, currentStarbase ) * this.aiPlan.starbaseUpgradePriority;
            ShipDesign chosenOne = null;
            int chosenQty = 1;
            if ((defenseFleets > bomberFleets) && (defenseFleets > bomberCoverFleets) && (defenseFleets > starStation))
            {
                chosenOne = aiPlan.currentDefenderDesign;
                chosenQty = (int)defenseFleets;
            }
            else if ((bomberFleets > defenseFleets) && (bomberFleets > bomberCoverFleets) && (bomberFleets > starStation))
            {
                chosenOne = aiPlan.currentBomberDesign;
                chosenQty = (int)bomberFleets;
            }
            else if ((bomberCoverFleets > defenseFleets) && (bomberCoverFleets > bomberFleets) && (bomberCoverFleets > starStation))
            {
                chosenOne = aiPlan.currentBomberCoverDesign;
                chosenQty = (int)bomberCoverFleets;
            }
            else if (aiPlan.currentStarbaseDesign != null)
            if ((this.planet.Starbase != null) && (!this.planet.Starbase.Name.Contains(aiPlan.currentStarbaseDesign.Name)))
            {
                chosenOne = aiPlan.currentStarbaseDesign;
                chosenQty = 1; //don't do the Same upgrade multiple times
            }
            if (chosenOne != null)
            {
                ProductionOrder chosenOrder = new ProductionOrder(chosenQty, new ShipProductionUnit(chosenOne,this.planet.Name,clientState.EmpireState), false);
                ProductionCommand suitableCommand = new ProductionCommand(CommandMode.Add, chosenOrder, this.planet.Key, productionIndex);
                foreach (ProductionOrder order in this.planet.ManufacturingQueue.Queue) if (order.Name == chosenOne.Name) return productionIndex;
                Message message;
                if (suitableCommand.IsValid(clientState.EmpireState, out message))
                {
                    suitableCommand.ApplyToState(clientState.EmpireState);
                    clientState.Commands.Push(suitableCommand);
                    productionIndex++;
                }
                else if (Global.Debug) Report.Information(message.Text);
            }
            return productionIndex;
        }
        private int BuidMinimalStarbase(int productionIndex) 
        {
            int queueYears;
            int queueEnergy = 0;
            foreach (ProductionOrder order in this.planet.ManufacturingQueue.Queue)
            {
                if (order is MineProductionUnit) queueEnergy += (order.NeededResources() as Resources).Energy;
            }
            queueYears = queueEnergy / Math.Max(1, this.planet.ResourcesOnHand.Energy);
            if (queueYears > 1) return productionIndex;

            ShipDesign smallStarbase = aiPlan.currentMinimalStarbaseDesign;

            if (smallStarbase != null)
            {
                ProductionOrder chosenOrder = new ProductionOrder(1, new ShipProductionUnit(smallStarbase,planet.Name,clientState.EmpireState), false);
                ProductionCommand suitableCommand = new ProductionCommand(CommandMode.Add, chosenOrder, this.planet.Key, productionIndex);
                foreach (ProductionOrder order in this.planet.ManufacturingQueue.Queue) if (order.Name == smallStarbase.Name) return productionIndex;
                Message message;
                if (suitableCommand.IsValid(clientState.EmpireState, out message))
                {
                    suitableCommand.ApplyToState(clientState.EmpireState);
                    clientState.Commands.Push(suitableCommand);
                    productionIndex++;
                }
                else if (Global.Debug) Report.Information(message.Text);
            }
            return productionIndex;
        }
    }
}
    
    


