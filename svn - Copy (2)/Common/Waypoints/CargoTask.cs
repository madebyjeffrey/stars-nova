#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project
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

namespace Nova.Common.Waypoints
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    
    using Nova.Common;

    public enum CargoMode
    {
        Load = 0,
        Unload
    }

    /// <summary>
    /// Performs Cargo transfer tasks.
    /// </summary>
    public class CargoTask : IWaypointTask
    {
        private List<Message> messages = new List<Message>();

        /// <inheritdoc />
        public List<Message> Messages
        {
            get { return messages; }
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (Mode == CargoMode.Load)
                {
                    return "Load Cargo";
                }
                else
                {
                    return "Unload Cargo";
                }
            }
        }

        /// <summary>
        /// Cargo object representing the amount to Load or Unload.
        /// </summary>
        public Cargo Amount { get; set; }
        public int mgFuel = 0;

        /// <summary>
        /// Load or Unload cargo. Mixed operations are represented by more than one Task.
        /// </summary>
        public CargoMode Mode { get; set; }
        public Item Target { get; set; }


        /// <summary>
        /// Default Constructor.
        /// </summary>
        public CargoTask()
        {
            Amount = new Cargo();
            Mode = CargoMode.Unload;
            Target = new Mappable();
        }


        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="other">CargoTask to copy.</param>
        public CargoTask(CargoTask copy)
        {
            Amount = new Cargo(copy.Amount);
            Mode = copy.Mode;
            Target = copy.Target;
        }


        /// <summary>
        /// Load: Read an object of this class from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of this object.</param>
        public CargoTask(XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "cargo":
                            Amount = new Cargo(mainNode);
                            break;
                        case "item":
                            Target = new Item(node);
                            break;
                        case "mode":
                            Mode = (CargoMode)Enum.Parse(typeof(CargoMode), mainNode.FirstChild.Value);
                            break;
                        case "mgfuel":
                            mgFuel = int.Parse( mainNode.FirstChild.Value);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Report.Error(e.Message);
                }
                mainNode = mainNode.NextSibling;
            }
        }


        /// <inheritdoc />
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelTask = xmldoc.CreateElement("CargoTask");
            Global.SaveData(xmldoc, xmlelTask, "Mode", Mode.ToString());
            if (Target != null) xmlelTask.AppendChild(Target.ToXml(xmldoc));
            xmlelTask.AppendChild(Amount.ToXml(xmldoc));
            if (mgFuel != 0) Global.SaveData(xmldoc, xmlelTask, "mgFuel", mgFuel.ToString());
            return xmlelTask;
        }


        /// <inheritdoc />
        public bool IsValid(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message messageOut)
        {
            if (target == null)
            {

                Message message = new Message();
                message.Audience = fleet.Owner;
                message.FleetID = fleet.Id;
                message.Type = "Load/Unload";
                message.Text = "Fleet " + fleet.Name + " attempted to load/unload cargo to empty space: ";
                Messages.Add(message);
                messageOut = message;
                return false;
            }
            if ((target.Type == ItemType.Star) || (target.Type == ItemType.StarIntel))
            {
                if (sender.StarReports.ContainsKey(target.Name.ToString()))
                {
                    StarIntel star = sender.StarReports[target.Name.ToString()];

                    if ((Math.Abs(star.Position.X - fleet.Position.X) > 1) || (Math.Abs(star.Position.Y - fleet.Position.Y) > 1))
                    {

                        Message message = new Message();
                        message.Audience = fleet.Owner;
                        message.FleetID = fleet.Id;
                        message.Type = "Load/Unload";
                        message.Text = "Fleet " + fleet.Name + " attempted to load/unload cargo when too far from target: " + target.Name;
                        Messages.Add(message);
                        messageOut = message;
                        return false;
                    }
                }


                // Check ownership.
                if (!sender.OwnedStars.ContainsKey(target.Name))
                {
                    if (receiver == null)
                    {
                        if (Global.Debug) Report.Information("Cargo Transfer to enemy Stars not implemeneted - try creating an invasion task"); //  ;)
                        Message message = new Message(sender.Id, "Cargo Transfer to enemy Stars not implemeneted - try creating an invasion task", "Invalid Command", null);
                        messageOut = message;
                        return false;
                    }
                    else
                    {
                        bool toReturn = false;

                        InvadeTask invade = new InvadeTask();
                        Message message = null;
                        if (invade.IsValid(fleet, target, sender, receiver, out message))
                        {
                            toReturn = invade.Perform(fleet, target, sender, receiver, out message);
                        }

                        Messages.AddRange(invade.Messages);
                        if (invade.Messages.Count > 0) messageOut = invade.Messages[0];
                        else messageOut = null;
                        return toReturn;
                    }
                }
            }

            if (target.Type == ItemType.Fleet)
            {
                if ((sender.OwnedFleets.ContainsKey(target.Key)) || (target.Name == "S A L V A G E"))
                {
                    Fleet other = null;
                    if ((sender.OwnedFleets.ContainsKey(target.Key)))   other = sender.OwnedFleets[target.Key];
                    else other = receiver.OwnedFleets[target.Key];
                    if ((Math.Abs(other.Position.X - fleet.Position.X) > 1) || (Math.Abs(other.Position.Y - fleet.Position.Y) > 1))
                    {
                        Message message = new Message();
                        message.Audience = fleet.Owner;
                        message.FleetID = fleet.Id;
                        message.Type = "Load/Unload";
                        message.Text = "Fleet " + fleet.Name + " attempted to load/unload cargo when too far from target: " + target.Name;
                        Messages.Add(message);
                        messageOut = message;
                        return false;
                    }
                }
            }
            messageOut = null;
            return true;
        }


        /// <inheritdoc />
        public bool Perform(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message message)
        {
            switch (Mode)
            {
                case CargoMode.Load:
                    message = null;
                    return Load(fleet, target, sender, receiver, out message);

                case CargoMode.Unload:
                    message = null;
                    return Unload(fleet, target, sender, receiver, out message);
            }

            message = null;
            return false;
        }


        /// <summary>
        /// Performs concrete unloading.
        /// </summary>
        private bool Unload(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message message)
        {
            if ((target.Type == ItemType.Star) || (target.Type == ItemType.StarIntel))
            {
                if (sender.OwnedStars.ContainsKey(target.Name))
                {
                    Star star = sender.OwnedStars[target.Name];
                    {
                        message = new Message();
                        message.Audience = fleet.Owner;
                        message.Type = "Load/Unload";
                        message.FleetID = fleet.Id;
                        message.Text = "Fleet " + fleet.Name + " has unloaded its cargo at " + star.Name + ".";
                        Messages.Add(message);
                        Amount = Amount.Min(Amount, fleet.Cargo);
                        star.Add(Amount);
                        fleet.Cargo.Remove(Amount);
                        return true;
                    }
                }
                else
                {
                    message = new Message();
                    message.Audience = fleet.Owner;
                    message.FleetID = fleet.Id;
                    message.Type = "Load/Unload";
                    message.Text = "Fleet " + fleet.Name + " couldn't find " + target + " to unload cargo to.";
                    return false;
                }
            }
            else
            {
                if (target.Type == ItemType.Fleet)
                {
                    if (sender.OwnedFleets.ContainsKey(target.Key))
                    {
                        Fleet other = sender.OwnedFleets[target.Key];
                        message = new Message();
                        message.FleetID = fleet.Id;
                        message.Audience = fleet.Owner;
                        message.Type = "Load/Unload";
                        message.Text = "Fleet " + fleet.Name + " has transferred cargo to " + other.Name + ".";
                        Messages.Add(message);

                        other.Cargo.Add(Amount);
                        other.FuelAvailable += mgFuel;
                        fleet.Cargo.Remove(Amount);
                        fleet.FuelAvailable -= mgFuel;

                        return true;
                    }
                }
                {
                    message = new Message();
                    message.Audience = fleet.Owner;
                    message.FleetID = fleet.Id;
                    message.Type = "Load/Unload";
                    message.Text = "Fleet " + fleet.Name + " couldn't find " + target + " to unload cargo to.";
                    return false;
                }
            }
        }


        /// <summary>
        /// Performs concrete loading.
        /// </summary>
        private bool Load(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message message)
        {
            if ((target.Type == ItemType.Star) || (target.Type == ItemType.StarIntel))
            {
                if (sender.OwnedStars.ContainsKey(target.Name))
                {
                    Star star = sender.OwnedStars[target.Name];
                    {
                        message = new Message();
                        message.Audience = fleet.Owner;
                        message.FleetID = fleet.Id;
                        message.Type = "Load/Unload";
                        message.Text = "Fleet " + fleet.Name + " has loaded cargo from " + star.Name + ".";
                        Messages.Add(message);
                        fleet.Cargo.Add(Amount);
                        star.Remove(Amount);
                        return true;
                    }
                }
                else
                {
                    message = new Message();
                    message.Audience = fleet.Owner;
                    message.FleetID = fleet.Id;
                    message.Type = "Load/Unload";
                    message.Text = "Fleet " + fleet.Name + " couldn't find " + target + " to load cargo to.";
                    return false;
                }
            }
            else if (target.Type == ItemType.Fleet)
            {
                if ((sender.OwnedFleets.ContainsKey(target.Key)) || (target.Name == "S A L V A G E"))
                {
                    Fleet other = null;
                    if (sender.OwnedFleets.ContainsKey(target.Key)) other = sender.OwnedFleets[target.Key];
                    else other = receiver.OwnedFleets[target.Key];
                    message = new Message();
                    message.Audience = fleet.Owner;
                    message.FleetID = fleet.Id;
                    message.Type = "Load/Unload";
                    message.Text = "Fleet " + fleet.Name + " has transferred cargo from " + other.Name + ".";
                    Messages.Add(message);
                    fleet.Cargo.Add(Amount);
                    fleet.FuelAvailable += mgFuel;
                    other.Cargo.Remove(Amount);
                    other.FuelAvailable -= mgFuel;
                    return true;
                }
                else
                {
                    message = new Message();
                    message.Audience = fleet.Owner;
                    message.FleetID = fleet.Id;
                    message.Type = "Load/Unload";
                    message.Text = "Fleet " + fleet.Name + " couldn't find " + target + " to load cargo to.";
                    return false;
                }
            }
            {
                message = new Message();
                message.Audience = fleet.Owner;
                message.FleetID = fleet.Id;
                message.Type = "Load/Unload";
                message.Text = "Fleet " + fleet.Name + " couldn't find " + target + " to load cargo to.";
                return false;
            }
        }
    }
}
