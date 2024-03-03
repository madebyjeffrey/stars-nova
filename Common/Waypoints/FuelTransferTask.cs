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
    using System.Runtime.Versioning;
    using System.Xml;

    using Nova.Common;
    using Nova.Common.Components;



    /// <summary>
    /// Performs transfer of fuel between fleets
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class FuelTransferTask : IWaypointTask
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
                return "Load Fuel";
            }
        }


        /// <summary>
        /// Int object representing the amount of Fuel to Load.
        /// </summary>
        public IntegerProperty Amount { get; set; }


        /// <summary>
        /// Load fuel to item (may be negative)
        /// </summary>

        public Item Target { get; set; }


        /// <summary>
        /// Default Constructor.
        /// </summary>
        public FuelTransferTask()
        {
            Amount = new IntegerProperty();
            Target = new Mappable();
        }


        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="other">CargoTask to copy.</param>
        public FuelTransferTask(FuelTransferTask copy)
        {
            Amount = new IntegerProperty(copy.Amount);
            Target = copy.Target;
        }


        /// <summary>
        /// Load: Read an object of this class from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of this object.</param>
        public FuelTransferTask(XmlNode node)
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
                        case "fuel":
                            Amount = new IntegerProperty(mainNode);
                            break;
                        case "item":
                            Target = new Item(node);
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
            if (Target != null) xmlelTask.AppendChild(Target.ToXml(xmldoc));
            xmlelTask.AppendChild(Amount.ToXml(xmldoc));

            return xmlelTask;
        }


        /// <inheritdoc />
        public bool IsValid(Fleet fleet, Item target, EmpireData sender, EmpireData receiver ,out Message messageOut)
        {
            if (target == null)
            {

                Message message = new Message();
                message.Type = "Fleet";
                message.Event = fleet;
                message.Audience = fleet.Owner;
                message.Text = "Fleet " + fleet.Name + " attempted to load/unload fuel to empty space: ";
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
                        message.Type = "Fleet";
                        message.Audience = fleet.Owner;
                        message.Event = fleet;
                        message.FleetKey = fleet.Key;
                        message.Text = "Fleet " + fleet.Name + " attempted to load/unload fuel when too far from target: " + target.Name;
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
                        Message message = new Message(sender.Id, "Fleet " + fleet.Name + " - Invalid constructor call to Waypoint.IsValid - Fuel dumped in Space", "Invalid Command", null);
                        messageOut = message;
                        return false;
                    }
                    else
                    {
                        Message message = new Message(sender.Id, "Fleet " + fleet.Name + " Receiving Race had the wrong Fuel nozzles - Fuel dumped in Space", "Invalid Command", null);
                        messageOut = message;
                        return true;
                    }
                }
            }

            if (target.Type == ItemType.Fleet)
            {
                if (sender.OwnedFleets.ContainsKey(target.Key))
                {
                    Fleet other = sender.OwnedFleets[target.Key];

                    if ((Math.Abs(other.Position.X - fleet.Position.X) > 1) || (Math.Abs(other.Position.Y - fleet.Position.Y) > 1))
                    {

                        Message message = new Message();
                        message.Type = "Fleet";
                        message.Audience = fleet.Owner;
                        message.FleetKey = fleet.Key;
                        message.Text = "Fleet " + fleet.Name + " attempted to load/unload fuel when too far from target: " + target.Name;
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
        public bool Perform(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message messageOut)
        {// The existing Client has no controls to add a fuel transfer commands at any place other than waypoint zero 
            // The existing AI only has no methods (23 Oct 2020) that add fuel transfer commands at waypoint zero (usually Waypoint 1) or at some distance from the fleets current location
            if ((target.Type == ItemType.Star) || (target.Type == ItemType.StarIntel))
            {
                if (sender.OwnedStars.ContainsKey(target.Name))
                {
                    Star star = sender.OwnedStars[target.Name];
                    {
                        //fleet.FuelAvailable = fleet.FuelAvailable - (double)Amount.Value;
                        //interactions between starbases and orbiting fuel is handled elsewhere
                        messageOut = null;
                        return true;
                    }
                }
                else
                {
                    Message message = new Message();
                    message.Type = "Fleet";
                    message.Event = fleet;
                    message.Audience = fleet.Owner;
                    message.FleetKey = fleet.Key;
                    message.Text = "Fleet " + fleet.Name + " tried to load fuel to/from " + target.Name + ".";
                    Messages.Add(message);
                    messageOut = null;
                    return false;
                }
            }
            else if (target.Type == ItemType.Fleet)
            {

                if (sender.OwnedFleets.ContainsKey((target as Fleet).Key))
                {
                    Fleet other = sender.OwnedFleets[(target as Fleet).Key];
                    Message message = new Message();
                    message = new Message();
                    message.Text = "Fleet " + other.Name + " has received fuel from " + fleet.Name + ".";
                    message.Type = "WarpToChange";
                    message.Event = fleet;
                    message.FleetKey = other.Key;
                    Messages.Add(message);
                    message = new Message();
                    message.Event = fleet;
                    message.Text = "Fleet " + fleet.Name + " has transferred fuel to " + other.Name + ".";
                    message.Type = "DestToChange";
                    message.FleetKey = fleet.Key;
                    Messages.Add(message);
                    double AmountToTransfer = Amount.Value;
                    if (fleet.FuelAvailable < AmountToTransfer) AmountToTransfer = fleet.FuelAvailable; // existing AI commands just set Amount.value to int.MaxValue
                    if (other.TotalFuelCapacity - other.FuelAvailable < AmountToTransfer) AmountToTransfer = other.TotalFuelCapacity - other.FuelAvailable;
                    fleet.FuelAvailable = fleet.FuelAvailable - AmountToTransfer;
                    other.FuelAvailable = other.FuelAvailable + AmountToTransfer;
                    messageOut = message;
                    return true;
                }
                messageOut = null;
                return false;
            }
            else
            {
                messageOut = null;
                return false;
            }
        }
    
    }
}
