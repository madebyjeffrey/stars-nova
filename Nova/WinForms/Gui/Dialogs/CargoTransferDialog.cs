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
    using Nova.WinForms.Gui.Controls;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Waypoints;


    public partial class CargoTransferDialog : Form
    {
        private Fleet fleet;
        private Mappable target;
        private ClientData clientData;

        public Dictionary<CargoMode, CargoTask> Tasks { get; private set; }
        private Cargo leftCargo;
        private Cargo rightCargo;
        private int leftFuel;
        private int rightFuel;

        public CargoTransferDialog()
        {
            InitializeComponent();

            cargoIronLeft.ValueChanged += CargoIronLeft_ValueChanged;
            cargoIronRight.ValueChanged += CargoIronRight_ValueChanged;
            cargoBoraniumLeft.ValueChanged += CargoBoraniumLeft_ValueChanged;
            cargoBoraniumRight.ValueChanged += CargoBoraniumRight_ValueChanged;
            cargoGermaniumLeft.ValueChanged += CargoGermaniumLeft_ValueChanged;
            cargoGermaniumRight.ValueChanged += CargoGermaniumRight_ValueChanged;
            cargoColonistsLeft.ValueChanged += CargoColonistsLeft_ValueChanged;
            cargoColonistsRight.ValueChanged += CargoColonistsRight_ValueChanged;
            fuelLeft.ValueChanged += FuelLeft_ValueChanged;
            fuelRight.ValueChanged += FuelRight_ValueChanged;
        }

        private int ReJigValues(int newValue, out int newRightValue, int leftLevel, int rightLevel, int leftMass, int rightMass, int leftMax, int rightMax)
        {
            // First if we request more than there is then change to request the max available
            int totalLevel = leftLevel + rightLevel;
            if (newValue > totalLevel)
            {
                newValue = totalLevel;
            }

            leftMass -= leftLevel;
            rightMass -= rightLevel;
            if (leftMass + newValue > leftMax)
            {
                // Have blown the left cargo limit - work out how far we can go
                newValue = leftMax - leftMass;
            }

            // so now we work out the right side
            newRightValue = leftLevel + rightLevel - newValue;
            if (rightMass + newRightValue > rightMax)
            {
                // Have blown the left cargo limit - work out how far we can go
                newRightValue = rightMax - rightMass;
                newValue = leftLevel + rightLevel - newRightValue;
                if (leftMass + newValue > leftMax)
                {
                    // We can't do it - reset
                    newValue = leftLevel;
                    newRightValue = rightLevel;
                }
            }
            return newValue;
        }

        private void CargoIronLeft_ValueChanged(int newValue)
        {
            int newRightValue;
            newValue = ReJigValues(newValue, out newRightValue, leftCargo.Ironium, rightCargo.Ironium, leftCargo.Mass, rightCargo.Mass, cargoMeterLeft.Maximum, cargoMeterRight.Maximum);
            leftCargo.Ironium = newValue;
            rightCargo.Ironium = newRightValue;
            UpdateMeters();
        }


        public void CargoIronRight_ValueChanged(int newValue)
        {
            int newLeftLevel;
            newValue = ReJigValues(newValue, out newLeftLevel, rightCargo.Ironium, leftCargo.Ironium, rightCargo.Mass, leftCargo.Mass, cargoMeterRight.Maximum, cargoMeterLeft.Maximum);
            leftCargo.Ironium = newLeftLevel;
            rightCargo.Ironium = newValue;
            UpdateMeters();
        }

        private void CargoBoraniumLeft_ValueChanged(int newValue)
        {
            int newRightValue;
            newValue = ReJigValues(newValue, out newRightValue, leftCargo.Boranium, rightCargo.Boranium, leftCargo.Mass, rightCargo.Mass, cargoMeterLeft.Maximum, cargoMeterRight.Maximum);
            leftCargo.Boranium = newValue;
            rightCargo.Boranium = newRightValue;
            UpdateMeters();
        }

        private void CargoBoraniumRight_ValueChanged(int newValue)
        {
            int newLeftLevel;
            newValue = ReJigValues(newValue, out newLeftLevel, rightCargo.Boranium, leftCargo.Boranium, rightCargo.Mass, leftCargo.Mass, cargoMeterRight.Maximum, cargoMeterLeft.Maximum);
            leftCargo.Boranium = newLeftLevel;
            rightCargo.Boranium = newValue;
            UpdateMeters();
        }

        private void CargoGermaniumLeft_ValueChanged(int newValue)
        {
            int newRightValue;
            newValue = ReJigValues(newValue, out newRightValue, leftCargo.Germanium, rightCargo.Germanium, leftCargo.Mass, rightCargo.Mass, cargoMeterLeft.Maximum, cargoMeterRight.Maximum);
            leftCargo.Germanium = newValue;
            rightCargo.Germanium = newRightValue;
            UpdateMeters();
        }

        private void CargoGermaniumRight_ValueChanged(int newValue)
        {
            int newLeftLevel;
            newValue = ReJigValues(newValue, out newLeftLevel, rightCargo.Germanium, leftCargo.Germanium, rightCargo.Mass, leftCargo.Mass, cargoMeterRight.Maximum, cargoMeterLeft.Maximum);
            leftCargo.Germanium = newLeftLevel;
            rightCargo.Germanium = newValue;
            UpdateMeters();
        }

        private void CargoColonistsLeft_ValueChanged(int newValue)
        {
            int newRightValue;
            newValue = ReJigValues(newValue, out newRightValue, leftCargo.ColonistsInKilotons, rightCargo.ColonistsInKilotons, leftCargo.Mass, rightCargo.Mass, cargoMeterLeft.Maximum, cargoMeterRight.Maximum);
            leftCargo.ColonistsInKilotons = newValue;
            rightCargo.ColonistsInKilotons = newRightValue;
            UpdateMeters();
        }

        private void CargoColonistsRight_ValueChanged(int newValue)
        {
            int newLeftLevel;
            newValue = ReJigValues(newValue, out newLeftLevel, rightCargo.ColonistsInKilotons, leftCargo.ColonistsInKilotons, rightCargo.Mass, leftCargo.Mass, cargoMeterRight.Maximum, cargoMeterLeft.Maximum);
            leftCargo.ColonistsInKilotons = newLeftLevel;
            rightCargo.ColonistsInKilotons = newValue;
            UpdateMeters();
        }

        private void FuelLeft_ValueChanged(int newValue)
        {
            int newRightValue;
            newValue = ReJigValues(newValue, out newRightValue, leftFuel, rightFuel, leftFuel, rightFuel, fuelLeft.Maximum, fuelRight.Maximum);
            leftFuel = newValue;
            rightFuel = newRightValue;
            UpdateMeters();
        }

        private void FuelRight_ValueChanged(int newValue)
        {
            int newLeftLevel;
            newValue = ReJigValues(newValue, out newLeftLevel, rightFuel, leftFuel, rightFuel, leftFuel, fuelRight.Maximum, fuelLeft.Maximum);
            leftFuel = newLeftLevel;
            rightFuel = newValue;
            UpdateMeters();
        }



        public void SetFleets(Fleet left, Fleet right, ClientData clientData)
        {
            Tasks = new Dictionary<CargoMode, CargoTask>();
            target = right;
            fleet = left;
            this.clientData = clientData;
            leftCargo = new Cargo(left.Cargo);
            rightCargo = new Cargo(right.Cargo);
            leftFuel = (int) left.FuelAvailable;
            rightFuel = (int) right.FuelAvailable;

            cargoIronLeft.Maximum = left.TotalCargoCapacity;
            cargoBoraniumLeft.Maximum = left.TotalCargoCapacity;
            cargoGermaniumLeft.Maximum = left.TotalCargoCapacity;
            cargoColonistsLeft.Maximum = left.TotalCargoCapacity;

            cargoIronRight.Maximum = right.TotalCargoCapacity;
            cargoBoraniumRight.Maximum = right.TotalCargoCapacity;
            cargoGermaniumRight.Maximum = right.TotalCargoCapacity;
            cargoColonistsRight.Maximum = right.TotalCargoCapacity;

            fuelLeft.Maximum = left.TotalFuelCapacity;
            fuelRight.Maximum = right.TotalFuelCapacity;

            cargoMeterLeft.Maximum = left.TotalCargoCapacity;
            cargoMeterRight.Maximum = right.TotalCargoCapacity;

            labelFleet1.Text = left.Name;
            labelFleet2.Text = right.Name;

            UpdateMeters();
        }

        private void UpdateMeters()
        {
            cargoIronLeft.Value = leftCargo.Ironium;
            cargoBoraniumLeft.Value = leftCargo.Boranium;
            cargoGermaniumLeft.Value = leftCargo.Germanium;
            cargoColonistsLeft.Value = leftCargo.ColonistsInKilotons;

            cargoIronRight.Value = rightCargo.Ironium;
            cargoBoraniumRight.Value = rightCargo.Boranium;
            cargoGermaniumRight.Value = rightCargo.Germanium;
            cargoColonistsRight.Value = rightCargo.ColonistsInKilotons;

            fuelLeft.Value = leftFuel;
            fuelRight.Value = rightFuel;

            cargoMeterLeft.CargoLevels = leftCargo;
            cargoMeterRight.CargoLevels = rightCargo;
        }

        public Cargo LeftCargo
        {
            get { return leftCargo; }
        }

        public Cargo RightCargo
        {
            get { return rightCargo; }
        }

        public int LeftFuel
        {
            get { return leftFuel; }
        }

        public int RightFuel
        {
            get { return rightFuel; }
        }

        private void okButton_Click(object sender, EventArgs e)
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

            foreach (KeyValuePair<ResourceType, int> commodity in LeftCargo.Commodities)
            {
                if (fleet.Cargo[commodity.Key] >= leftCargo[commodity.Key])
                {
                    mode = CargoMode.Unload;
                }
                else
                {
                    mode = CargoMode.Load;
                }

                Tasks[mode].Amount[commodity.Key] = Math.Abs(leftCargo[commodity.Key] - fleet.Cargo[commodity.Key]);
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
                    command = new WaypointCommand(CommandMode.Insert, waypoint, fleet.Key, index ); //  add task always to the end of waypoint zero commands.

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
                        else if (clientData.EmpireState.OwnedFleets.ContainsKey((waypoint.Task as CargoTask).Target.Key))
                        {
                            Fleet other = clientData.EmpireState.OwnedFleets[(waypoint.Task as CargoTask).Target.Key];
                            if (waypoint.Task.IsValid(fleet, other, clientData.EmpireState, null))
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

    }
}

