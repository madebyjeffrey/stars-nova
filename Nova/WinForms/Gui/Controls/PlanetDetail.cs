#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010, 2011 The Stars-Nova Project
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

namespace Nova.WinForms.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    
    using Nova.Client;
    using Nova.Common;
    using Nova.ControlLibrary;
    
    /// <Summary>
    /// Planet Detail display pane.
    /// </Summary>
    public partial class PlanetDetail : System.Windows.Forms.UserControl
    {
        private EmpireData empireState;

        private Star selectedStar;
        
        // FIXME:(priority 3) this should not be here. It is only needed to pass it
        // down to the ProductionDialog. In any case, ProductionDialog shouldn't need
        // the whole state either. Must refactor this.
        private ClientData clientState;

        private Dictionary<string, Fleet> fleetsInOrbit = new Dictionary<string, Fleet>();

        /// <summary>
        /// Signals that the selected Star has changed.
        /// </summary>
        public event EventHandler<SelectionArgs> PlanetSelectionChanged;

        /// <Summary>
        /// Initializes a new instance of the PlanetDetail class.
        /// </Summary>
        public PlanetDetail(EmpireData empireState, ClientData clientState)
        {
            this.empireState = empireState;
            this.clientState = clientState;

            InitializeComponent();
        }
        public void ReInitialise(EmpireData empireState, ClientData clientState)
        {
            this.empireState = empireState;
            this.clientState = clientState;

            // InitializeComponent();
        }
        public void ClearStar()
        {
            productionQueue.Clear();
            starbaseArmor.Text = "";
            starbaseShields.Text = "";
            starbaseDamage.Text = "";
            starbaseCapacity.Text = "";
            defenseCoverage.Text = "";
            defenses.Text = "";
            defenseType.Text = "";
            factories.Text = "";
            mines.Text = "";
            Resources unknown = new Resources(0, 0, 0, 0);
            resourceDisplay.Value = unknown;
            scannerType.Text = "";
            scannerRange.Text = "";
            population.Text = "unknown";
            resourceDisplay.ResourceRate = 0;
            resourceDisplay.ResearchBudget = 0;
            resourceDisplay.Name = "";
            resourceDisplay.ResetText();

        }
        /// <Summary>
        /// The change queue button has been pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void ChangeProductionQueue_Click(object sender, EventArgs e)
        {
            ProductionDialog productionDialog = new ProductionDialog(selectedStar, clientState);

            productionDialog.ShowDialog();
            productionDialog.Dispose();
            
            productionQueue.Populate(selectedStar);
            UpdateFields();


        }


        /// <Summary>
        /// Next planet button pressed
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void NextPlanet_Click(object sender, EventArgs e)
        {
            if (empireState.OwnedStars.Count == 1)
            {
                previousPlanet.Enabled = false;
                nextPlanet.Enabled = false;
                return;
            }

            previousPlanet.Enabled = true;
            nextPlanet.Enabled = true;

            selectedStar = empireState.OwnedStars.GetNext(empireState.OwnedStars[selectedStar.Name]);

            // Inform of the selection change to all listening objects.
            OnPlanetSelectionChanged(new SelectionArgs(selectedStar));
        }


        /// <Summary>
        /// Previous planet button pressed
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void PreviousPlanet_Click(object sender, EventArgs e)
        {
            if (empireState.OwnedStars.Count == 1)
            {
                previousPlanet.Enabled = false;
                nextPlanet.Enabled = false;
                return;
            }

            previousPlanet.Enabled = true;
            nextPlanet.Enabled = true;

            selectedStar = empireState.OwnedStars.GetPrevious(empireState.OwnedStars[selectedStar.Name]);

            // Inform of the selection change to all listening objects.
            OnPlanetSelectionChanged(new SelectionArgs(selectedStar));
        }

        /// <Summary>
        /// Set the Star which is to be displayed.
        /// </Summary>
        /// <param name="selectedStar">The Star to be displayed.</param>
        private void SetStarDetails(Star selectedStar)
        {
            if (selectedStar == null)
            {
                ClearStar();
                return;
            }

            this.selectedStar = selectedStar;

            UpdateFields();

            groupPlanetSelect.Text = "Planet " + selectedStar.Name;

            if (empireState.OwnedStars.Count > 1)
            {                
                previousPlanet.Enabled = true;
                nextPlanet.Enabled = true;
            }
            else
            {
                previousPlanet.Enabled = false;
                nextPlanet.Enabled = false;
            }
        }


        /// <Summary>
        /// Update all the fields in the planet Detail display.
        /// </Summary>
        private void UpdateFields()
        {
            if (selectedStar == null)
            {
                return;
            }

            productionQueue.Populate(selectedStar);








            Resources wholeQueueCost = new Resources(0, 0, 0, 0);      // resources required to build everything in the Production Queue
            Resources selectedItemCost = new Resources(0, 0, 0, 0);    // resources required to build the selected Item stack in the Production Queue
            double percentComplete = 0.0;
            int minesInQueue = 0;
            int factoriesInQueue = 0;
            int minYearsCurrent, maxYearsCurrent, minYearsSelected, maxYearsSelected, minYearsTotal, maxYearsTotal;
            int yearsSoFar, yearsToCompleteOne;
            minYearsSelected = maxYearsSelected = minYearsTotal = maxYearsTotal = 0;

            Race starOwnerRace = selectedStar.ThisRace;
            Resources potentialResources = new Resources();

            if (starOwnerRace == null)
            {
                // set potentialResources to zero as they are unknown without knowing the Race that owns the Star
                // and set yearsSoFar to -1 to cause the calculations of years to complete to be skipped
                yearsSoFar = -1;
            }
            else
            {
                yearsSoFar = 1;
                // initialize the mineral portion of the potentialResources
                potentialResources.Ironium = selectedStar.ResourcesOnHand.Ironium;
                potentialResources.Boranium = selectedStar.ResourcesOnHand.Boranium;
                potentialResources.Germanium = selectedStar.ResourcesOnHand.Germanium;
            }

            // check if there are any items in the Production Queue before attempting to determine costs
            // requires checking if queueList.Items.Count > than 1 due to Placeholder value at Top of the Queue
            if (selectedStar.ManufacturingQueue.Queue.Count > 1)
            {


                minYearsSelected = maxYearsSelected = -2;

                // initialize / setup variables:
                //  currentStackCost : used to store the cost of the Stack of items being evaluated
                //  allBuilt : used to determine if all items remaining in the stack can be built
                //  quantityYetToBuild : used to track how many items in the current stack still need to have their build time estimated
                Resources currentStackCost = null;
                ProductionOrder productionOrder = null;
                bool allBuilt = false;
                int quantityYetToBuild;

                for (int queueIndex = 0; queueIndex < selectedStar.ManufacturingQueue.Queue.Count; queueIndex++)
                {
                    productionOrder = (selectedStar.ManufacturingQueue.Queue[queueIndex] as ProductionOrder);
                    quantityYetToBuild = productionOrder.Quantity;
                    currentStackCost = productionOrder.NeededResources();
                    wholeQueueCost += currentStackCost;

                    if (yearsSoFar < 0)
                    {   // if yearsSoFar is less than zero than the Item cannot currently be built because
                        // an Item further up the queue cannot be built
                        productionQueue.Items[queueIndex].ForeColor = System.Drawing.Color.Red;
                        minYearsCurrent = maxYearsTotal = -1;
                        if (minYearsTotal == 0)
                        {
                            minYearsTotal = -1;
                        }
                        maxYearsTotal = -1;
                    }
                    else
                    {
                        // loop to determine the number of years to complete the current stack of items
                        allBuilt = false;
                        minYearsCurrent = maxYearsCurrent = yearsToCompleteOne = 0;
                        while (yearsSoFar >= 0 && !allBuilt)
                        {
                            // need to determine / update the resources available at this Point (resources on
                            // planet plus those already handled in the queue - including those from
                            // each year of this loop)

                            // determine the potentialResources based on the population, factories, and mines that actually exist
                            // determine the number of years to build this based on current mines / factories
                            // then update to include the effect of any mines or factories in the queue
                            // UPDATED May 11: to use native Star method of resource rate prediction. -Aeglos
                            potentialResources.Energy += selectedStar.GetFutureResourceRate(factoriesInQueue);

                            // Account for resources destinated for research.
                            // Use the set client percentage, not the planet allocation. This is because the
                            // allocated resources only change during turn generation, but the budget may change
                            // many times while playing a turn. This makes the Star's values out of sync, so,
                            // predict them for now.
                            // Only do this if the Star is respecting research budget.
                            if (selectedStar.OnlyLeftover == false)
                            {
                                potentialResources.Energy -= potentialResources.Energy * clientState.EmpireState.ResearchBudget / 100;
                            }

                            // need to know how much of each mineral is currently available on the Star (queueStar.ResourcesOnHand)
                            // need race information to determine how many minerals are produced by each mine each year
                            // need to make sure that no more than the maximum number of mines operable by colonists are being operated
                            // add one year of mining results to the remaining potentialResources
                            // UPDATED May 11: to use native Star methods of mining rate prediction.
                            potentialResources.Ironium += selectedStar.GetFutureMiningRate(selectedStar.MineralConcentration.Ironium, minesInQueue);
                            potentialResources.Boranium += selectedStar.GetFutureMiningRate(selectedStar.MineralConcentration.Boranium, minesInQueue);
                            potentialResources.Germanium += selectedStar.GetFutureMiningRate(selectedStar.MineralConcentration.Germanium, minesInQueue);

                            // check how much can be done with resources available

                            if (potentialResources >= currentStackCost)
                            {
                                // everything remaining in this stack can be done
                                // therefore set allBuilt to true and reduce potential resources
                                // do not increment the yearsSoFar as other items might be able to be completed
                                //    this year with the remaining resources
                                allBuilt = true;
                                yearsToCompleteOne = 0;
                                potentialResources = potentialResources - currentStackCost;
                                if (minYearsCurrent == 0)
                                {
                                    minYearsCurrent = yearsSoFar;
                                }
                                maxYearsCurrent = yearsSoFar;

                                if (minYearsTotal == 0)
                                {
                                    minYearsTotal = yearsSoFar;
                                }
                                maxYearsTotal = yearsSoFar;
                                if (productionOrder.Unit is MineProductionUnit)
                                {
                                    minesInQueue += quantityYetToBuild;
                                }
                                if (productionOrder.Unit is FactoryProductionUnit)
                                {
                                    factoriesInQueue += quantityYetToBuild;
                                }
                            }
                            else
                            {
                                // not everything in the stack can be built this year
                                maxYearsSelected = -1;

                                // the current build state is the remaining cost of the production unit.
                                Resources currentBuildState = productionOrder.Unit.RemainingCost;

                                // Normialized to 1.0 = 100%
                                double fractionComplete = 1.0;

                                // determine the percentage able to be completed by whichever resource is limiting production
                                if (fractionComplete > (double)potentialResources.Ironium / currentStackCost.Ironium && currentStackCost.Ironium > 0)
                                {
                                    fractionComplete = (double)potentialResources.Ironium / currentStackCost.Ironium;
                                }
                                if (fractionComplete > (double)potentialResources.Boranium / currentStackCost.Boranium && currentStackCost.Boranium > 0)
                                {
                                    fractionComplete = (double)potentialResources.Boranium / currentStackCost.Boranium;
                                }
                                if (fractionComplete > (double)potentialResources.Germanium / currentStackCost.Germanium && currentStackCost.Germanium > 0)
                                {
                                    fractionComplete = (double)potentialResources.Germanium / currentStackCost.Germanium;
                                }
                                if (fractionComplete > (double)potentialResources.Energy / currentStackCost.Energy && currentStackCost.Energy > 0)
                                {
                                    fractionComplete = (double)potentialResources.Energy / currentStackCost.Energy;
                                }
                                // apply this percentage to the currentStackCost to determine how much to remove from
                                // potentialResources and currentStackCost
                                Resources amountUsed = fractionComplete * currentStackCost;
                                potentialResources -= amountUsed;
                                currentStackCost -= amountUsed;

                                // check if at least the top Item in the stack can be built "this" year
                                if (amountUsed >= currentBuildState)
                                {
                                    // at least one Item can be built this year
                                    yearsToCompleteOne = 0;
                                    if (minYearsCurrent == 0)
                                    {
                                        minYearsCurrent = yearsSoFar;
                                    }

                                    if (minYearsTotal == 0)
                                    {
                                        minYearsTotal = yearsSoFar;
                                    }
                                    // determine how many items are able to be built and reduce quantity accordingly
                                    for (int quantityStepper = 1; quantityStepper < quantityYetToBuild; quantityStepper++)
                                    {
                                        if (amountUsed <= (currentBuildState + (quantityStepper * productionOrder.Unit.Cost)))
                                        {
                                            quantityYetToBuild -= quantityStepper;
                                            quantityStepper = quantityYetToBuild + 1;
                                        }
                                    }
                                }
                                else
                                {
                                    // not able to complete even one Item this year
                                    yearsToCompleteOne++;
                                }


                                if (yearsToCompleteOne > 20)
                                {
                                    // an Item is considered to be unbuildable if it will take more than
                                    // 20 years to build it
                                    yearsSoFar = -1;
                                    if (minYearsCurrent == 0)
                                    {
                                        minYearsCurrent = -1;
                                    }
                                    maxYearsCurrent = -1;
                                    if (minYearsTotal == 0)
                                    {
                                        minYearsTotal = -1;
                                    }
                                    maxYearsTotal = -1;
                                }
                                allBuilt = false;
                                if (yearsSoFar >= 0)
                                {
                                    yearsSoFar++;
                                }
                            }
                        }
                        // end of the while loop to determine years to build items in the stack

                        // once *YearsCurrent have been determined set font color appropriately
                        if (minYearsCurrent == 1)
                        {
                            if (maxYearsCurrent == 1)
                            {
                                productionQueue.Items[queueIndex].ForeColor = System.Drawing.Color.Green;
                            }
                            else
                            {
                                productionQueue.Items[queueIndex].ForeColor = System.Drawing.Color.Blue;
                            }
                        }
                        else
                        {
                            if (minYearsCurrent == -1)
                            {
                                productionQueue.Items[queueIndex].ForeColor = System.Drawing.Color.Red;
                            }
                            else
                            {
                                productionQueue.Items[queueIndex].ForeColor = System.Drawing.Color.Black;
                            }
                        }
                    }
                }
            }














            Defenses.ComputeDefenseCoverage(selectedStar);

            defenseType.Text = selectedStar.DefenseType;
            defenses.Text = selectedStar.Defenses.ToString(System.Globalization.CultureInfo.InvariantCulture);
            defenseCoverage.Text = Defenses.SummaryCoverage.ToString(System.Globalization.CultureInfo.InvariantCulture);

            factories.Text = selectedStar.Factories.ToString(System.Globalization.CultureInfo.InvariantCulture)
                             + " of " +
                             selectedStar.GetOperableFactories().ToString(System.Globalization.CultureInfo.InvariantCulture);
            mines.Text = selectedStar.Mines.ToString(System.Globalization.CultureInfo.InvariantCulture)
                         + " of " + selectedStar.GetOperableMines().ToString(System.Globalization.CultureInfo.InvariantCulture);
            population.Text = selectedStar.Colonists.ToString(System.Globalization.CultureInfo.InvariantCulture);

            resourceDisplay.ResourceRate = selectedStar.GetResourceRate();

            if (selectedStar.OnlyLeftover == false)
            {
                resourceDisplay.ResearchBudget = empireState.ResearchBudget;
            }
            else
            {
                // We treat Stars contributing only leftover resources as having
                // a 0% budget allocation.

                resourceDisplay.ResearchBudget = 0;
            }

            resourceDisplay.Value = selectedStar.ResourcesOnHand;

            scannerRange.Text = selectedStar.ScanRange.ToString(System.Globalization.CultureInfo.InvariantCulture);
            scannerType.Text = selectedStar.ScannerType;

            if (selectedStar.Starbase == null)
            {
                starbasePanel.Text = "No Starbase";
                starbasePanel.Enabled = false;
            }
            else
            {
                Fleet starbase = selectedStar.Starbase;
                starbaseArmor.Text = starbase.TotalArmorStrength.ToString(System.Globalization.CultureInfo.InvariantCulture);
                starbaseCapacity.Text =
                    starbase.TotalDockCapacity.ToString(System.Globalization.CultureInfo.InvariantCulture);
                starbaseDamage.Text = "0";
                starbasePanel.Enabled = true;
                starbasePanel.Text = starbase.Name;
                starbaseShields.Text = starbase.TotalShieldStrength.ToString();

                massDriverType.Text = "None";
                massDriverDestination.Text = "None";
                targetButton.Enabled = false;
            }

            List<string> fleetnames = new List<string>();
            fleetsInOrbit = new Dictionary<string, Fleet>();
            foreach (Fleet fleet in empireState.OwnedFleets.Values)
            {
                if (fleet.InOrbit != null && fleet.InOrbit.Name == selectedStar.Name && !fleet.IsStarbase)
                {
                    fleetnames.Add(fleet.Name);
                    fleetsInOrbit[fleet.Name] = fleet;
                }
            }
            fleetnames.Sort();
            comboFleetsInOrbit.Items.Clear();
            bool haveFleets = fleetnames.Count > 0;
            if (haveFleets)
            {
                comboFleetsInOrbit.Items.AddRange(fleetnames.ToArray());
                comboFleetsInOrbit.SelectedIndex = 0;
            }
            buttonGoto.Enabled = haveFleets;
            buttonGoto.Enabled = haveFleets;

            Invalidate();

        }

        /// <Summary>
        /// Access to the Star Report whose details are displayed in the panel.
        /// </Summary>
        public Star Value
        {
            set { SetStarDetails(value); }
            get { return selectedStar; }
        }
        private Fleet GettopFleetInOrbit()
        {
            if (comboFleetsInOrbit.SelectedItem == null)
            {
                return null;
            }

            Fleet fleet;
            if (!fleetsInOrbit.TryGetValue(comboFleetsInOrbit.SelectedItem.ToString(), out fleet))
            {
                return null;
            }

            return fleet;
        }

        private void ComboFleetsInOrbit_SelectedIndexChanged(object sender, EventArgs e)
        {
            Fleet fleet = GettopFleetInOrbit();
            if (fleet == null)
            {
                meterFuel.Value = 0;
                meterFuel.Maximum = 0;
                meterCargo.CargoLevels = new Cargo();
                meterCargo.Maximum = 0;
            }
            else
            {
                meterFuel.Maximum = fleet.TotalFuelCapacity;
                meterFuel.Value = (int)fleet.FuelAvailable;
                meterCargo.Maximum = fleet.TotalCargoCapacity;
                meterCargo.CargoLevels = fleet.Cargo;
            }
            Invalidate();
        }

        private void ButtonGoto_Click(object sender, EventArgs e)
        {
            Fleet fleet = GettopFleetInOrbit();
            if (fleet != null)
            {
                OnPlanetSelectionChanged(new SelectionArgs(fleet));
            }
        }

        private void ButtonCargo_Click(object sender, EventArgs e)
        {
            Fleet fleet = GettopFleetInOrbit();
            if ((fleet != null) && (fleet.InOrbit != null))
            {
                try
                {
                    using (CargoDialog cargoDialog = new CargoDialog(fleet, fleet.InOrbit, clientState))
                    {
                        cargoDialog.ShowDialog(); 
                        UpdateFields();
                    }

                    ComboFleetsInOrbit_SelectedIndexChanged(null, null);
                }
                catch
                {
                    Report.Debug("FleetDetail.cs : CargoButton_Click() - Failed to open cargo dialog.");
                }
            }
        }
        
        
        protected virtual void OnPlanetSelectionChanged(SelectionArgs e)
        {
            SetStarDetails(selectedStar);
                
            if (PlanetSelectionChanged != null) {
                PlanetSelectionChanged(this, e);
            }
        }       
    }
}
