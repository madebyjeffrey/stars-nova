#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010, 2011 stars-nova
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

namespace Nova.ControlLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Waypoints;

    /// <summary>
    /// A dialog for transferring cargo between a fleet and a Mappable(fleet,planet or salvage).
    /// </summary>
    public partial class CargoDialog : Form
    {
        private Fleet fleet;
        private Mappable target;
        private Cargo fleetCargo;
        private Cargo targetCargo;
        private ClientData clientData;
        
        public Dictionary<CargoMode, CargoTask> Tasks {get; private set;}

        /// <summary>
        /// Initializes a new instance of the CargoDialog class.
        /// </summary>
        public CargoDialog(Fleet fleet, Mappable target, ClientData clientData)
        {
            InitializeComponent();
            cargoIron.ValueChanged += CargoIron_ValueChanged;
            cargoBoran.ValueChanged += CargoBoran_ValueChanged;
            cargoGerman.ValueChanged += CargoGermanium_ValueChanged;
            cargoColonistsInKilotons.ValueChanged += CargoColonists_ValueChanged;

            Tasks = new Dictionary<CargoMode, CargoTask>();

            SetTarget(fleet,target);

            this.clientData = clientData;
        }

        public void CargoIron_ValueChanged(int newValue)
        {
            if (fleetCargo.Mass - fleetCargo.Ironium + newValue > meterCargo.Maximum)
            {
                newValue = meterCargo.Maximum - fleetCargo.Mass + fleetCargo.Ironium;
            }

            int total = fleetCargo.Ironium + targetCargo.Ironium;

            if (newValue > total)
            {
                newValue = total;
            }

            fleetCargo.Ironium = newValue;
            targetCargo.Ironium = total - newValue;

            UpdateMeters();
        }

        public void CargoBoran_ValueChanged(int newValue)
        {
            if (fleetCargo.Mass - fleetCargo.Boranium + newValue > meterCargo.Maximum)
            {
                newValue = meterCargo.Maximum - fleetCargo.Mass + fleetCargo.Boranium;
            }

            int total = fleetCargo.Boranium + targetCargo.Boranium;

            if (newValue > total)
            {
                newValue = total;
            }

            fleetCargo.Boranium = newValue;
            targetCargo.Boranium = total - newValue;
            UpdateMeters();
        }

        public void CargoGermanium_ValueChanged(int newValue)
        {
            if (fleetCargo.Mass - fleetCargo.Germanium + newValue > meterCargo.Maximum)
            {
                newValue = meterCargo.Maximum - fleetCargo.Mass + fleetCargo.Germanium;
            }

            int total = fleetCargo.Germanium + targetCargo.Germanium;

            if (newValue > total)
            {
                newValue = total;
            }

            fleetCargo.Germanium = newValue;
            targetCargo.Germanium = total - newValue;
            UpdateMeters();
        }

        public void CargoColonists_ValueChanged(int newValue)
        {
            if (fleetCargo.Mass - fleetCargo.ColonistsInKilotons + newValue > meterCargo.Maximum)
            {
                newValue = meterCargo.Maximum - fleetCargo.Mass + fleetCargo.ColonistsInKilotons;
            }

            int total = fleetCargo.ColonistsInKilotons + targetCargo.ColonistsInKilotons;

            if (newValue > total)
            {
                newValue = total;
            }

            fleetCargo.ColonistsInKilotons = newValue;
            targetCargo.ColonistsInKilotons = total - newValue;
            UpdateMeters();
        }

        
        private void UpdateMeters()
        {
            cargoIron.Value = fleetCargo.Ironium;
            cargoBoran.Value = fleetCargo.Boranium;
            cargoGerman.Value = fleetCargo.Germanium;
            cargoColonistsInKilotons.Value = fleetCargo.ColonistsInKilotons;

            labelIron.Text = targetCargo.Ironium + " kT";
            labelBoran.Text = targetCargo.Boranium + " kT";
            labelGerman.Text = targetCargo.Germanium + " kT";
            labelColonistsInKilotons.Text = targetCargo.ColonistsInKilotons + " kT";

            meterCargo.CargoLevels = fleetCargo;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Tasks.Add(CargoMode.Load, new CargoTask());
            Tasks.Add(CargoMode.Unload, new CargoTask());
            Tasks[CargoMode.Load].Mode = CargoMode.Load;
            Tasks[CargoMode.Load].Target = target;
            Tasks[CargoMode.Unload].Mode = CargoMode.Unload;
            Tasks[CargoMode.Unload].Target = target;

            // See if this is a Load, Unload or Mixed operation.            
            // If original fleet >= dialog fleet, then Unload. Else, Load.
            CargoMode mode;
            
            foreach(KeyValuePair<ResourceType, int> commodity in fleetCargo.Commodities)
            {
                if (fleet.Cargo[commodity.Key] >= fleetCargo[commodity.Key])
                {
                    mode = CargoMode.Unload;        
                }
                else
                {
                    mode = CargoMode.Load;
                }
                
                Tasks[mode].Amount[commodity.Key] = Math.Abs(fleetCargo[commodity.Key] - fleet.Cargo[commodity.Key]);
                Tasks[mode].Target = target;
            }

            WaypointCommand command;
            foreach (CargoTask task in Tasks.Values)
            {
                if (task.Amount.Mass != 0)
                {
                    Waypoint waypoint = new Waypoint(fleet.Waypoints[0]); // copy first Waypoint
                    waypoint.Task = task;
                    int index = 0;
                    String destination = fleet.Waypoints[0].Destination;
                    bool found = false;
                    while ((!found) && (index < fleet.Waypoints.Count))
                    {
                        found = (fleet.Waypoints[index].Destination != destination);
                        index++;
                    }

                    if (found) index--;
                    command = new WaypointCommand(CommandMode.Insert, waypoint, fleet.Key, index); // add task always to the end of waypoint zero commands.

                    clientData.Commands.Push(command);

                    if (command.IsValid(clientData.EmpireState))
                    {
                        command.ApplyToState(clientData.EmpireState);

                        if ((waypoint.Task as CargoTask).Target.Type == ItemType.Star)
                        {
                            if (clientData.EmpireState.StarReports.ContainsKey((waypoint.Task as CargoTask).Target.Name.ToString()))
                            {
                                StarIntel star = clientData.EmpireState.StarReports[(waypoint.Task as CargoTask).Target.Name.ToString()];
                                // Also perform it here, to update client state for manual xfer.
                                if (command.Waypoint.Task.IsValid(fleet, star, clientData.EmpireState, null))
                                {
                                    command.Waypoint.Task.Perform(fleet, star, clientData.EmpireState, null); // Load, Unload
                                }
                            }
                            //fleet.Waypoints.Remove(waypoint); // immediate commands shouldn't add a visible waypoint to the ship in the client - we have told the server what to do
                        }
                        else  if (clientData.EmpireState.OwnedFleets.ContainsKey((waypoint.Task as CargoTask).Target.Key))
                        {
                            Fleet other = clientData.EmpireState.OwnedFleets[(waypoint.Task as CargoTask).Target.Key];
                            if (waypoint.Task.IsValid(fleet, other, clientData.EmpireState,null))
                            {

                                // Also perform it here, to update client state for manual xfer.
                                if (command.Waypoint.Task.IsValid(fleet, other, clientData.EmpireState, null))
                                {
                                    command.Waypoint.Task.Perform(fleet, other, clientData.EmpireState, null); // Load, Unload
                                }
                            }
                           //fleet.Waypoints.Remove(waypoint); // immediate commands shouldn't add a visible waypoint to the ship in the client - we have told the server what to do

                        }
                    }
                    
                }
            }
            
        }

        /// <summary>
        /// initialize the various fields in the dialog.
        /// </summary>
        /// <param name="targetFleet">The <see cref="Fleet"/> transferring cargo.</param>
        public void SetTarget(Fleet targetFleet,Mappable sourceMappable)
        {
            fleet = targetFleet;
            target = sourceMappable;
            fleetCargo = new Cargo(targetFleet.Cargo); // clone this so it can hold values in case we cancel
            targetCargo = new Cargo();

            if (sourceMappable.Type == ItemType.Star)
            {
                targetCargo.Ironium = (sourceMappable as Star).ResourcesOnHand.Ironium;
                targetCargo.Boranium = (sourceMappable as Star).ResourcesOnHand.Boranium;
                targetCargo.Germanium = (sourceMappable as Star).ResourcesOnHand.Germanium;
                targetCargo.ColonistsInKilotons = (sourceMappable as Star).Colonists / Global.ColonistsPerKiloton;
            }
            else if (sourceMappable.Type == ItemType.Fleet)
            {
                targetCargo.Ironium = (sourceMappable as Fleet).Cargo.Ironium;
                targetCargo.Boranium = (sourceMappable as Fleet).Cargo.Boranium;
                targetCargo.Germanium = (sourceMappable as Fleet).Cargo.Germanium;
                targetCargo.ColonistsInKilotons = (sourceMappable as Fleet).Cargo.ColonistsInKilotons;
            }
            else
            {
                targetCargo.Ironium = 0;
                targetCargo.Boranium =  0;
                targetCargo.Germanium =  0;
                targetCargo.ColonistsInKilotons = 0;    
            }

            cargoIron.Maximum = targetFleet.TotalCargoCapacity;
            cargoBoran.Maximum = targetFleet.TotalCargoCapacity;
            cargoGerman.Maximum = targetFleet.TotalCargoCapacity;
            cargoColonistsInKilotons.Maximum = targetFleet.TotalCargoCapacity;

            meterCargo.Maximum = targetFleet.TotalCargoCapacity;

            UpdateMeters();
        }
    }
}
