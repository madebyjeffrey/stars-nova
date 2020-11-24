#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project.
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
    using System.Windows.Forms;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Waypoints;
    using Nova.ControlLibrary;
    using Nova.WinForms.Gui.Dialogs;
    using Common.DataStructures;

    /// <Summary>
    /// Ship Detail display panel.
    /// </Summary>
    public partial class FleetDetail : System.Windows.Forms.UserControl
    {
        private EmpireData empireState;
        private ClientData clientData;
        private Stack<ICommand> commands;

        private Fleet topFleet, lastFleet;
        private Dictionary<long, Fleet> fleetsAtLocation = new Dictionary<long, Fleet>();

        /// <summary>
        /// Signals that the Fleet being displayed has changed.
        /// </summary>
        public event EventHandler<SelectionArgs> FleetSelectionChanged;

        /// <Summary>
        /// This event should be fired when a waypoint is deleted,
        /// so the StarMap updates right away.
        /// </Summary>
        public event EventHandler StarmapChanged;
        public event EventHandler<SelectionArgs> WaypointIndexChanged;


        /// <Summary>
        /// Property to set or get the fleet currently being displayed.
        /// </Summary>
        public Fleet Value
        {
            set
            {
                if (value != null)
                {
                    SetFleetDetails(value);
                }
            }
            get
            {
                return topFleet;
            }
        }


        /// <Summary>
        /// Initializes a new instance of the FleetDetail class.
        /// </Summary>
        public FleetDetail(ClientData clientState)
        {
            this.clientData = clientState;
            this.empireState = clientState.EmpireState;
            this.commands = clientState.Commands;

            InitializeComponent();
        }
        public void ReInitialise(ClientData clientState)
        {
            this.clientData = clientState;
            this.empireState = clientState.EmpireState;
            this.commands = clientState.Commands;

            //InitializeComponent();
        }
        private void wayPoints_DrawItem(object sender,
            System.Windows.Forms.DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Define the default color of the brush as black.
            System.Drawing.Brush wpBrush = System.Drawing.Brushes.Black;

            // Determine the color of the brush to draw each item based 
            // on the index of the item to draw.
            bool isWaypoint0 = true;
            for (int i = 0; i <= e.Index; i++) if ((wayPoints.Items[0] as Waypoint).Destination != (wayPoints.Items[e.Index] as Waypoint).Destination) isWaypoint0 = false;
            if (e.Index == 0) wpBrush = System.Drawing.Brushes.BlueViolet;
            else if (isWaypoint0) wpBrush = System.Drawing.Brushes.LightGray;
            else wpBrush = System.Drawing.Brushes.Black;


            // Draw the current item text based on the current Font 
            // and the custom brush settings.
            e.Graphics.DrawString((wayPoints.Items[e.Index] as Waypoint).Destination,
                e.Font, wpBrush, e.Bounds, System.Drawing.StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
        /// <Summary>
        /// Called when the warp factor slider is moved.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void WaypointSpeedChanged(object sender, System.EventArgs e)
        {
            warpText.Text = "Warp " + warpFactor.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (wayPoints.SelectedItems.Count > 0)
            {
                int index = wayPoints.SelectedIndices[0];

                Waypoint editedWaypoint = new Waypoint();

                editedWaypoint.Destination = topFleet.Waypoints[index].Destination;
                editedWaypoint.Position = topFleet.Waypoints[index].Position;
                editedWaypoint.Task = topFleet.Waypoints[index].Task;

                editedWaypoint.WarpFactor = warpFactor.Value;

                WaypointCommand command = new WaypointCommand(CommandMode.Edit, editedWaypoint, topFleet.Key, index);

                // Minimizing clutter. If the last command (at all) was a speed/task change for this same waypoint,
                // then just use that instead of adding a potentialy huge pile of speed edits.

                if (commands.Count > 0)
                {
                    ICommand lastCommand = commands.Peek();

                    // Make sure it's the same waypoint except for speed/task, and that it's not a freshly added
                    // waypoint.
                    if (lastCommand is WaypointCommand && (lastCommand as WaypointCommand).Waypoint != null)
                    {
                        if ((lastCommand as WaypointCommand).Waypoint.Destination == editedWaypoint.Destination &&
                            (lastCommand as WaypointCommand).Waypoint.Position == editedWaypoint.Position &&
                            (lastCommand as WaypointCommand).Mode != CommandMode.Add)
                        {
                            // Discard it.
                            commands.Pop();
                        }
                    }
                }

                commands.Push(command);

                Nova.Common.Message message;
                if (command.IsValid(empireState, out message))
                {
                    command.ApplyToState(empireState);
                    //TODO separate the commandList from the WaypointListbox and only display waypoints that the user 
                    // can edit while maintaining links from the WaypointListbox.index to the commandList.index (obviously not
                    // one to one)
                }
                else if (Global.Debug) Report.Information(message.Text);

                DisplayLegDetails(index);
            }
        }

        /// <Summary>
        /// On a waypoint being selected update the speed and tasks controls to
        /// reflect the values of the selected waypoint.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void WaypointSelection(object sender, System.EventArgs e)
        {
            if (wayPoints.SelectedItems.Count <= 0)
            {
                return;
            }

            int index = wayPoints.SelectedIndices[0];
            DisplayLegDetails(index);
            Mappable destination = new Mappable((wayPoints.Items[wayPoints.SelectedIndices[0]] as Waypoint).Position);
            //destination.Position = (wayPoints.Items[wayPoints.SelectedIndices[0]] as Waypoint).Position;
            SelectionArgs positionArg = new SelectionArgs(destination);
            WaypointIndexChanged(this, positionArg);

        }

        /// <Summary>
        /// Cargo button pressed. Pop up the cargo transfer dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void CargoButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                Star target = null;
                foreach (Star star in empireState.OwnedStars.Values)
                    if (topFleet.InOrbit == star) target = star;
                if (target == null)
                {
                    Report.Information("Cargo Transfer to enemy Stars not implemeneted - try creating an invasion task"); //  ;)
                }
                else
                {
                    using (CargoDialog cargoDialog = new CargoDialog(topFleet, topFleet.InOrbit, clientData))
                    {
                        cargoDialog.ShowDialog();
                        UpdateCargoMeters();
                        Invalidate();
                    }
                    //OnFleetSelectionChanged(new SelectionArgs(topFleet));
                    wayPoints.DataSource = null;
                    wayPoints.DataSource = topFleet.Waypoints;
                    meterCargo.CargoLevels = topFleet.Cargo;
                }
            }
            catch
            {
                Report.Debug("FleetDetail.cs : CargoButton_Click() - Failed to open cargo dialog.");
            }
        
        }

        /// <Summary>
        /// Catch the backspace key to delete a fleet waypoint.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (wayPoints.SelectedItems.Count <= 0)
            {
                return;
            }

            int index = wayPoints.SelectedIndices[0];

            // backspace
            if (index == 0 || !(e.KeyChar == (char)8))
            {
                return;
            }
            if ((topFleet.Waypoints[index].Task is SplitMergeTask) || (topFleet.Waypoints[index].Task is CargoTask))
            {
                if (Global.Debug) Report.Information("That is a waypoint zero task, removing it may result in a loss of Fleet split, merge, load and unload actions for this fleet and/or other fleets");
                return;
            }
            else
            {
                WaypointCommand command = new WaypointCommand(CommandMode.Delete, topFleet.Key, index);

                commands.Push(command);

                Nova.Common.Message message;
                if (command.IsValid(empireState, out message))
                {
                    command.ApplyToState(empireState);
                }
                else if (Global.Debug) Report.Information(message.Text);
            }
            // Refresh the waypoint list on the GUI.
            UpdateWaypointList(this, new EventArgs());
            if (StarmapChanged != null)
            {
                OnStarmapChanged(EventArgs.Empty);
            }
        }

        /// <Summary>
        /// Process the delete key to delete a fleet waypoint.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                int index = wayPoints.SelectedIndices[0];
                if (index > 0)
                {

                    if ((topFleet.Waypoints[index].Task is SplitMergeTask) || (topFleet.Waypoints[index].Task is CargoTask))
                    {
                        if (Global.Debug)  Report.Information("That is a waypoint zero task, removing it may result in a loss of Fleet split, merge, load and unload actions for this fleet and/or other fleets");
                        return;
                    }
                    else
                    {
                        WaypointCommand command = new WaypointCommand(CommandMode.Delete, topFleet.Key, index);

                        commands.Push(command);

                        Nova.Common.Message message;
                        if (command.IsValid(empireState,out message))
                        {
                            message = command.ApplyToState(empireState);
                            if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                        }
                        else if (Global.Debug) Report.Information(message.Text);
                    }
                    // Refresh the waypoint list on the GUI.
                    UpdateWaypointList(this, new EventArgs());

                    if (StarmapChanged != null)
                    {
                        OnStarmapChanged(EventArgs.Empty);
                    }
                }
                e.Handled = true;
            }
        }

        /// <Summary>
        /// If a waypoint task changes, and a waypoint is selected, change the task at
        /// that waypoint.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void WaypointTaskChanged(object sender, EventArgs e)
        {
            if (topFleet != lastFleet)
            {
                lastFleet = topFleet;
                return;     //The selected Fleet changed so this event was fired by mistake
            }
            if (wayPoints.SelectedItems.Count <= 0)
            {
                return;
            }

            int index = wayPoints.SelectedIndices[0];
            if ((topFleet.Waypoints[index].Task is SplitMergeTask) || (topFleet.Waypoints[index].Task is CargoTask))
            {
                if (Global.Debug) Report.Information("That is a waypoint zero task, editing it may result in a loss of Fleet split, merge, load and unload actions for this fleet and/or other fleets");
                return;
            }
            else
            {

                Waypoint waypoint = topFleet.Waypoints[index];

                Waypoint editedWaypoint = new Waypoint();

                editedWaypoint.Destination = topFleet.Waypoints[index].Destination;
                editedWaypoint.Position = topFleet.Waypoints[index].Position;
                editedWaypoint.WarpFactor = topFleet.Waypoints[index].WarpFactor;
                Mappable target = null;
                foreach (Star star in empireState.OwnedStars.Values) if (star.Name == topFleet.Waypoints[index].Destination) target = star;
                if (target == null) foreach (Fleet fleet in empireState.OwnedFleets.Values) if (fleet.Name == topFleet.Waypoints[index].Destination) target = fleet;
                editedWaypoint.LoadTask(WaypointTasks.Text, null, target);


                WaypointCommand command = new WaypointCommand(CommandMode.Edit, editedWaypoint, topFleet.Key, index);

                // Minimizing clutter. If the last command was a speed/task change for this same waypoint,
                // then just use that instead of adding a potentialy huge pile of task edits.
                if (commands.Count > 0)
                {
                    ICommand lastCommand = commands.Peek();

                    // Make sure it's the same waypoint except for speed/task, and that it's not a freshly added
                    // waypoint.
                    if (lastCommand is WaypointCommand && (lastCommand as WaypointCommand).Waypoint != null)
                    {
                        if ((lastCommand as WaypointCommand).Waypoint.Destination == editedWaypoint.Destination &&
                            (lastCommand as WaypointCommand).Waypoint.Position == editedWaypoint.Position &&
                            (lastCommand as WaypointCommand).Mode != CommandMode.Add)
                        {
                            // Discard it.
                            commands.Pop();
                        }
                    }
                }
                commands.Push(command);

                Nova.Common.Message message;
                if (command.IsValid(empireState, out message))
                {
                    message = command.ApplyToState(empireState);
                    if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                }
                else if (Global.Debug) Report.Information(message.Text);
            }
        }

        /// <Summary>
        /// The manage fleet button has been pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void SplitFleetClick(object sender, EventArgs e)
        {
            DoSplitMerge(null);
        }

        /// <Summary>
        /// Process the Next button being pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void NextFleet_Click(object sender, System.EventArgs e)
        {
            if (empireState.OwnedFleets.Count == 1)
            {
                previousFleet.Enabled = false;
                nextFleet.Enabled = false;
                return;
            }

            previousFleet.Enabled = true;
            nextFleet.Enabled = true;

            topFleet = empireState.OwnedFleets.GetNext(empireState.OwnedFleets[topFleet.Key]);

            // Signal the change.
            OnFleetSelectionChanged(new SelectionArgs(topFleet));
        }


        /// <Summary>
        /// Process the previous button being pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void PreviousFleet_Click(object sender, EventArgs e)
        {
            if (empireState.OwnedFleets.Count == 1)
            {
                previousFleet.Enabled = false;
                nextFleet.Enabled = false;
                return;
            }

            previousFleet.Enabled = true;
            nextFleet.Enabled = true;

            topFleet = empireState.OwnedFleets.GetPrevious(empireState.OwnedFleets[topFleet.Key]);

            // Signal the change.
            OnFleetSelectionChanged(new SelectionArgs(topFleet));
        }

        /// <Summary>
        /// If there is another waypoint before the selected one, display the fuel,
        /// time, etc. required for this leg.
        /// </Summary>
        /// <param name="index">Index of the waypoint to display.</param>
        private void DisplayLegDetails(int index)
        {
            if (topFleet.Waypoints.Count > 0)
            {
                Waypoint thisWaypoint = topFleet.Waypoints[index];

                WaypointTasks.Text = thisWaypoint.Task.Name;

                if (topFleet.Waypoints.Count == 1)
                {
                    thisWaypoint.WarpFactor = 0;
                }

                topFleet.Waypoints[index] = thisWaypoint;
                warpFactor.Value = thisWaypoint.WarpFactor;
                warpText.Text = "Warp " + thisWaypoint.WarpFactor;

                if (index > 0 && thisWaypoint.WarpFactor > 0)
                {
                    Waypoint from = topFleet.Waypoints[index - 1];
                    Waypoint to = topFleet.Waypoints[index];
                    double distance = PointUtilities.Distance(from.Position, to.Position);

                    double time = distance / (to.WarpFactor * to.WarpFactor);

                    double fuelUsed = topFleet.FuelConsumption(to.WarpFactor, empireState.Race)

                                    * time;

                    legDistance.Text = string.Format("{0}", distance.ToString("f1"));
                    legFuel.Text = string.Format("{0}", fuelUsed.ToString("f1"));
                    legTime.Text = string.Format("{0}", time.ToString("f1"));
                }
                else
                {
                    legDistance.Text = "0";
                    legFuel.Text = "0";
                    legTime.Text = "0";
                }

                Waypoint previous = null;
                double fuelRequired = 0;

                // Sum up the total fuel required for all waypoints in the current
                // route (as long as there is more than one waypoint).

                foreach (Waypoint waypoint in topFleet.Waypoints)
                {
                    if (previous != null && waypoint.WarpFactor > 0)
                    {
                        double distance = PointUtilities.Distance(waypoint.Position, previous.Position);
                        int warp = waypoint.WarpFactor;
                        double speed = warp * warp;
                        double travelTime = distance / speed;

                        fuelRequired += topFleet.FuelConsumption(warp, empireState.Race) * travelTime;
                    }
                    previous = waypoint;
                }

                System.Drawing.Color color = fuelRequired > topFleet.FuelAvailable ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                routeFuelUse.ForeColor = color;
                label3.ForeColor = color;
                label5.ForeColor = color;

                routeFuelUse.Text = fuelRequired.ToString("f1");
            }
        }

        /// <Summary>
        /// Set up all the display controls to reflect the selected fleet
        /// </Summary>
        /// <param name="fleet">The selected fleet.</param>
        private void SetFleetDetails(Fleet topFleet)
        {
            wayPoints.DrawMode = DrawMode.OwnerDrawFixed;
            wayPoints.DrawItem += new DrawItemEventHandler(wayPoints_DrawItem);
            if (topFleet == null)
            {
                return;
            }

            this.topFleet = topFleet;

            if (empireState.OwnedFleets.Count > 1)
            {
                previousFleet.Enabled = true;
                nextFleet.Enabled = true;
            }
            else
            {
                previousFleet.Enabled = false;
                previousFleet.Enabled = false;
            }

            groupFleetSelection.Text = "Fleet " + topFleet.Name;

            fleetComposition.Items.Clear();
            fleetComposition.ShowItemToolTips = true;

            foreach (ShipToken token in topFleet.Composition.Values)
            {
                ListViewItem listItem = new ListViewItem(token.Design.Name);

                // Show % damage & remaining armor in a tool tip.
                listItem.ToolTipText = token.Damage.ToString() + "% damage (" + token.Armor.ToString() + " armor remaining.)";

                listItem.SubItems.Add(token.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture));
                fleetComposition.Items.Add(listItem);
            }

            wayPoints.DataSource = topFleet.Waypoints;
            wayPoints.DisplayMember = "Destination";
            wayPoints.SelectedIndex = wayPoints.Items.Count - 1;

            DisplayLegDetails(wayPoints.Items.Count - 1);

            // If we are in orbit around a planet and we have a cargo carrying
            // capacity, enable the Cargo Dialog Button.
            bool inOrbit = topFleet.InOrbit != null;
            groupOrbitPlanet.Text = inOrbit ? "Orbiting " + topFleet.InOrbit.Name : "In deep space";
            buttonCargo.Enabled = inOrbit && topFleet.TotalCargoCapacity > 0;
            buttonGotoPlanet.Enabled = inOrbit;

            List<ComboBoxItem<Fleet>> fleets = new List<ComboBoxItem<Fleet>>();
            fleetsAtLocation = new Dictionary<long, Fleet>();
            foreach (Fleet other in empireState.OwnedFleets.Values)
            {
                if (topFleet.Position == other.Position && !other.IsStarbase && topFleet.Key != other.Key)
                {
                    fleets.Add(new ComboBoxItem<Fleet>(other.Name, other));
                    fleetsAtLocation[other.Key] = other;
                }
            }

            fleets.Sort(delegate (ComboBoxItem<Fleet> x, ComboBoxItem<Fleet> y)
            {
                return x.DisplayName.CompareTo(y.DisplayName);
            });

            comboOtherFleets.Items.Clear();
            bool haveFleets = fleets.Count > 0;

            if (haveFleets)
            {
                comboOtherFleets.Items.AddRange(fleets.ToArray());
                comboOtherFleets.SelectedIndex = 0;
            }

            buttonMerge.Enabled = haveFleets;
            buttonCargoXfer.Enabled = haveFleets;
            buttonGotoFleet.Enabled = haveFleets;

            UpdateCargoMeters();
            Invalidate();
        }

        private void UpdateCargoMeters()
        {
            meterFuel.Maximum = topFleet.TotalFuelCapacity;
            meterFuel.Value = (int)topFleet.FuelAvailable;
            meterCargo.Maximum = topFleet.TotalCargoCapacity;
            meterCargo.CargoLevels = topFleet.Cargo;
            ComboOtherFleets_SelectedIndexChanged(null, null); // Updates the other meters to current selection          
        }



        private Fleet GettopFleetAtLocation()
        {
            if (comboOtherFleets.SelectedItem == null)
            {
                return null;
            }

            ComboBoxItem<Fleet> selected = comboOtherFleets.SelectedItem as ComboBoxItem<Fleet>;
            Fleet fleet;
            if (!fleetsAtLocation.TryGetValue(selected.Tag.Key, out fleet))
            {
                return null;
            }
            return fleet;
        }

        private void ComboOtherFleets_SelectedIndexChanged(object sender, EventArgs e)
        {
            Fleet fleet = GettopFleetAtLocation();
            if (fleet == null)
            {
                meterFuelOther.Value = 0;
                meterFuelOther.Maximum = 0;
                meterCargoOther.CargoLevels = new Cargo();
                meterCargoOther.Maximum = 0;
            }
            else
            {
                meterFuelOther.Maximum = fleet.TotalFuelCapacity;
                meterFuelOther.Value = (int)fleet.FuelAvailable;
                meterCargoOther.Maximum = fleet.TotalCargoCapacity;
                meterCargoOther.CargoLevels = fleet.Cargo;
            }
            Invalidate();
        }

        private void ButtonGotoPlanet_Click(object sender, EventArgs e)
        {
            if (topFleet != null && topFleet.InOrbit != null)
            {
                OnFleetSelectionChanged(new SelectionArgs(topFleet.InOrbit as Star));
            }
        }

        private void ButtonGotoFleet_Click(object sender, EventArgs e)
        {
            Fleet newFleet = GettopFleetAtLocation();

            // Inform of the selection change to all listening objects.
            OnFleetSelectionChanged(new SelectionArgs(newFleet));
        }

        private void ButtonMerge_Click(object sender, EventArgs e)
        {
            Fleet newFleet = GettopFleetAtLocation();
            DoSplitMerge(newFleet);
        }

        /// <Summary>
        /// Populate context menu after right clicking the diamond button
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void ShowWaypointContext(object sender, EventArgs e)
        {
            NovaPoint position = new NovaPoint();

            int index = wayPoints.SelectedIndices[0];
            Waypoint waypoint = new Waypoint(topFleet.Waypoints[index]);

            position = waypoint.Position;

            List<Mappable> nearObjects = FindNearObjects(position);
            if (nearObjects.Count == 0)
            {
                return;
            }

            contextMenuWaypointTargets.Items.Clear();
            bool needSep = false;
            bool doneSep = false;
            foreach (Item sortableItem in nearObjects)
            {
                ToolStripItem menuItem = contextMenuWaypointTargets.Items.Add(sortableItem.Name);
                if (sortableItem.Type == ItemType.StarIntel)
                {
                    // Put stars at the top o' da list
                    contextMenuWaypointTargets.Items.Insert(0, menuItem);
                    contextMenuWaypointTargets.Items.Insert(1, new ToolStripSeparator());
                }
                menuItem.Tag = sortableItem;
                if (sortableItem.Type == ItemType.Salvage)
                {
                    menuItem.Image = Properties.Resources.salvage0000;
                    needSep = true;
                }
                else if (sortableItem.Type == ItemType.StarIntel)
                {
                    menuItem.Image = Properties.Resources.planeticon;
                    needSep = true;
                }
                else if (sortableItem.Type == ItemType.FleetIntel)
                {
                    menuItem.Image = Properties.Resources.fleet;
                    if (needSep && !doneSep)
                    {
                        contextMenuWaypointTargets.Items.Insert(1, new ToolStripSeparator());
                        doneSep = true;
                    }
                }
            }
        }

        /// <Summary>
        /// Provides a list of objects within a certain distance from a position,
        /// ordered by distance.
        /// 
        /// Copied from StarMap.cs (should probably make this a utility)
        /// </Summary>
        /// <param name="position">Starting Point for the search.</param>
        /// <returns>A list of Fleet and Star objects.</returns>
        private List<Mappable> FindNearObjects(NovaPoint position)
        {
            List<Mappable> nearObjects = new List<Mappable>();

            foreach (FleetIntel report in clientData.EmpireState.FleetReports.Values)
            {
                if (!report.IsStarbase)
                {
                    if (PointUtilities.IsNear(report.Position, position))
                    {
                        nearObjects.Add(report);
                    }
                }
            }

            foreach (StarIntel report in clientData.EmpireState.StarReports.Values)
            {
                if (PointUtilities.IsNear(report.Position, position))
                {
                    nearObjects.Add(report);
                }
            }

            // nearObjects.Sort(ItemSorter);
            return nearObjects;
        }


        /// <summary>
        /// Raise the Split/Merge fleet dialog.
        /// </summary>
        /// <param name="otherFleet">The second fleet to merge with or split into (may be null).</param>
        private void DoSplitMerge(Fleet otherFleet = null)
        {
            using (SplitFleetDialog splitFleet = new SplitFleetDialog())
            {
                splitFleet.SetFleet(topFleet, otherFleet);
                splitFleet.nextFleetID = empireState.PeekNextFleetKey();
                if (splitFleet.ShowDialog() == DialogResult.OK)
                {
                    //find the last waypoint with the current destination - it is the last Waypoint zero command so insert after it
                    int index = 0;
                    String destination = topFleet.Waypoints[0].Destination;
                    bool found = false;
                    while ((!found) && (index < topFleet.Waypoints.Count))
                    {
                        found = (topFleet.Waypoints[index].Destination != destination);
                        index++;
                    }
                    if (found) index--;
                    Waypoint waypoint = new Waypoint(topFleet.Waypoints[0]);

                    waypoint.Task = new SplitMergeTask(
                        splitFleet.SourceComposition,
                        splitFleet.OtherComposition,
                        (otherFleet == null) ? 0 : otherFleet.Key);

                    WaypointCommand command = new WaypointCommand(CommandMode.Insert, topFleet.Key, index);


                    command.Waypoint = waypoint;
 
                    Nova.Common.Message message;
                    if (command.IsValid(empireState,out message))
                    {
                        commands.Push(command);

                        command.ApplyToState(empireState);
                        // Also perform it here, to update client state for manual split/merge.
                        if (command.Waypoint.Task.IsValid(topFleet, otherFleet, empireState, empireState, out message))
                        {
                            command.Waypoint.Task.Perform(topFleet, otherFleet, empireState, empireState, out message);
                            if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                            // topFleet.Waypoints.Remove(waypoint);
                            // Now clean and remove empty fleets and update remaining ones
                            // This is done to update the Client State visuals only, the server
                            // will handle this "for real".

                            // Check if original fleet was wiped.
                            if (topFleet.Composition.Count == 0)
                            {
                                empireState.RemoveFleet(topFleet);
                            }
                            else
                            {
                                empireState.AddOrUpdateFleet(topFleet);
                            }

                            // If this was a merge, update old fleet.
                            if (otherFleet != null)
                            {
                                if (otherFleet.Composition.Count == 0)
                                {
                                    empireState.RemoveFleet(otherFleet);
                                }
                                else
                                {
                                    empireState.AddOrUpdateFleet(otherFleet);
                                }
                            }

                            // otherFleet won't come out modified in the case
                            // of a split, so check the fleet Limbo.
                            foreach (Fleet newFleet in empireState.TemporaryFleets)
                            {
                                if (newFleet.Composition.Count == 0)
                                {
                                    empireState.RemoveFleet(newFleet);
                                }
                                else
                                {
                                    empireState.AddOrUpdateFleet(newFleet);
                                }
                            }
                        }
                        else if (Global.Debug) Report.Information(message.Text);
                    }
                    else if (Global.Debug) Report.Information(message.Text);
                    topFleet = (otherFleet == null) ? topFleet : otherFleet;

                }

                // Signal the change.
                OnFleetSelectionChanged(new SelectionArgs(topFleet));
                wayPoints.DataSource = null;
                wayPoints.DataSource = topFleet.Waypoints;
            }
        }

        private void ButtonCargoXfer_Click(object sender, EventArgs e)
        {
            CargoTransferDialog cargoTransferDialog = new CargoTransferDialog();
            {
                cargoTransferDialog.SetFleets(topFleet, GettopFleetAtLocation(), clientData);
                cargoTransferDialog.ShowDialog();
                UpdateCargoMeters();
                Invalidate();
            }


        }

        private void RenameClick(object sender, EventArgs e)
        {
            using (RenameFleetDialog dia = new RenameFleetDialog())
            {
                dia.FleetName = topFleet.Name;
                if (dia.ShowDialog() == DialogResult.OK)
                {
                    // rename the fleet withing the GUI imediately
                    topFleet.Name = dia.FleetName;

                    // create a command to rename the fleet permanantly
                    RenameFleetCommand renameCommand = new RenameFleetCommand(topFleet, dia.FleetName);
                    commands.Push(renameCommand);

                    // Refresh the fleet detail.
                    Invalidate();
                    // Signal the change - rename the fleet in the top left selection panel. 
                    // FIXME (priority 2) - renaming the fleet should also imediately rename the fleet in the right click context menu on the map.
                    OnFleetSelectionChanged(new SelectionArgs(topFleet));
                }
            }
        }


        protected virtual void OnFleetSelectionChanged(SelectionArgs e)
        {
            if (e != null)
                if (e.Selection is Fleet) SetFleetDetails(e.Selection as Fleet);

            if (FleetSelectionChanged != null)
            {
                FleetSelectionChanged(this, e);
            }
        }

        // Updates WaypointList when waypoints are changed from the StarMap (or other).
        public void UpdateWaypointList(object sender, EventArgs e)
        {
            ((CurrencyManager)wayPoints.BindingContext[wayPoints.DataSource]).Refresh();

            wayPoints.SelectedIndex = wayPoints.Items.Count - 1;
        }

        protected virtual void OnStarmapChanged(EventArgs e)
        {
            if (StarmapChanged != null)
            {
                StarmapChanged(this, e);
            }
        }

        /// <Summary>
        /// Process event thrown by a ToolStripMenuItem from the waypoint context menu.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ToolStripItemClickedEventArgs"/> that contains the event data.</param>
        private void ContextMenuWaypointTargets_SelectedIndexChanged(Object sender, ToolStripItemClickedEventArgs e)
        {
            String name = e.ClickedItem.AccessibilityObject.Name;

            int index = wayPoints.SelectedIndices[0];
            Waypoint waypoint = new Waypoint(topFleet.Waypoints[index]);

            NovaPoint position = waypoint.Position;

            List<Mappable> nearObjects = FindNearObjects(position);
            if (nearObjects.Count == 0)
            {
                return;
            }

            Mappable target = null;

            foreach (Mappable report in nearObjects)
            {
                if (report.Name == name)
                {
                    target = report;
                    break;
                }
            }
            if (target == null)
            {
                return;
            }

            if (wayPoints.SelectedItems.Count > 0)
            {
                index = wayPoints.SelectedIndices[0];
                topFleet.Waypoints[index].Destination = target.Name;
                topFleet.Waypoints[index].Position = target.Position;
            }

            (e.ClickedItem as ToolStripMenuItem).Checked = true;
            UpdateWaypointList(this, new EventArgs());
            Invalidate();
        }

        private void buttonSplitAll_Click(object sender, EventArgs e)
        {
            Fleet nextFleet = keepOneSplitRemainder(topFleet, clientData.EmpireState);
            while (nextFleet != null) nextFleet = keepOneSplitRemainder(nextFleet, clientData.EmpireState);
            OnFleetSelectionChanged(new SelectionArgs(topFleet));
        }
    

    private Fleet keepOneSplitRemainder(Fleet fleet, EmpireData empire)
        {                                                                   //  take all but one vessel to the new fleet
                                                                            // and we can split that fleet recursively

            Dictionary<long, ShipToken> LeftComposition = new Dictionary<long, ShipToken>();
            Dictionary<long, ShipToken> RightComposition = new Dictionary<long, ShipToken>();


            bool first = true;
            int rightQuantity = 0;
            foreach (long key in fleet.Composition.Keys)
            {
                long newFleetKey = clientData.EmpireState.PeekNextFleetKey();
                if (first)
                {
                    LeftComposition[key] = new ShipToken(fleet.Composition[key].Design, 1);
                    RightComposition[key] = new ShipToken(fleet.Composition[key].Design, fleet.Composition[key].Quantity - 1);
                    rightQuantity += RightComposition[key].Quantity;
                }
                else
                {
                    RightComposition[key] = new ShipToken(fleet.Composition[key].Design, fleet.Composition[key].Quantity);
                    rightQuantity += RightComposition[key].Quantity;
                }
                first = false;
            }

            if (rightQuantity == 0) return null;

            //find the last waypoint with the current destination - it is the last Waypoint zero command so insert after it
            int wpindex = 0;
            String here = fleet.Waypoints[0].Destination;
            bool found = false;
            while ((!found) && (wpindex < fleet.Waypoints.Count))
            {
                found = (fleet.Waypoints[wpindex].Destination != here);
                wpindex++;
            }
            if (found) wpindex--;

            Waypoint allFleetsDest = null;
            if (fleet.Waypoints.Count > wpindex) allFleetsDest = fleet.Waypoints[wpindex];

            Waypoint waypoint = new Waypoint(fleet.Waypoints[0]);
            waypoint.Task = new SplitMergeTask(
                LeftComposition,
                RightComposition,
                0);
            WaypointCommand command = new WaypointCommand(CommandMode.Insert, fleet.Key, wpindex);



            Fleet newFleet = null;
            command.Waypoint = waypoint;

            Nova.Common.Message message;
            if (command.IsValid(empireState, out message))
            {
                commands.Push(command);

                command.ApplyToState(empireState);
                // Also perform it here, to update client state for manual split/merge.
                if (command.Waypoint.Task.IsValid(fleet, null, empireState, empireState, out message))
                {
                    command.Waypoint.Task.Perform(fleet, null, empireState, empireState, out message);
                    if ((message != null) && (Global.Debug)) Report.Information(message.Text);
                    newFleet = empireState.TemporaryFleets[empireState.TemporaryFleets.Count - 1];
                    empireState.AddOrUpdateFleet(newFleet);

                }
                else if (Global.Debug) Report.Information(message.Text);
            }
            else if (Global.Debug) Report.Information(message.Text);
            if (allFleetsDest != allFleetsDest) //this creates 2 waypoints for some reason so it is commented out for now 
            {
                WaypointCommand otherFleet = new WaypointCommand(CommandMode.Add, newFleet.Key, 2);
                newFleet.Waypoints.Add(allFleetsDest);

                WaypointCommand destCommand = new WaypointCommand(CommandMode.Insert, fleet.Key, wpindex);
                destCommand.Waypoint = allFleetsDest;

                if (destCommand.IsValid(empireState,out message))
                {
                    commands.Push(destCommand);

                    destCommand.ApplyToState(empireState);

                }
                else if (Global.Debug) Report.Information(message.Text);

            }
            return newFleet;

        }

    }

}
    


