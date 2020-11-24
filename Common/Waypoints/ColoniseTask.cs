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
    using System.Linq;
    using System.Xml;
    
    using Nova.Common;
    
    /// <summary>
    /// Performs Star Colonisation.
    /// </summary>
    public class ColoniseTask : IWaypointTask
    {
        private List<Message> messages = new List<Message>();
        
        public List<Message> Messages
        {
            get
            { 
                return messages;
            }
        }
        
        public string Name
        {
            get
            {
                return "Colonise";
            }
        }
        
        public ColoniseTask()
        {
        }
        
        /// <summary>
        /// Load: Read in a ColoniseTask from and XmlNode representation.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> containing a representation of a <see cref="ProductionUnit"/>.</param>
        public ColoniseTask(XmlNode node)
        {
            if (node == null)
            {
                return;
            }    
        }
        
        public bool IsValid(Fleet fleet, Item target, EmpireData sender, EmpireData reciever, out Message messageOut)
        {
            Message message = new Message();
            Messages.Add(message);
            
            message.Audience = fleet.Owner;
            message.FleetID = fleet.Id;
            message.Text = fleet.Name + " attempted to colonise ";
            
            if (fleet.InOrbit == null || target == null || !(target is Star))
            {
                message.Text += "something that is not a star.";
                messageOut = message;
                return false;
            }
            
            Star star = (Star)target;
            message.Text += target.Name;
            
            if (star.Colonists != 0)
            {
                message.Text += " but it is already occupied, ";
                messageOut = message;
                return true;
            }
            
            if (fleet.Cargo.ColonistsInKilotons == 0)
            {
                message.Text += " but no colonists were on board.";
                messageOut = message;
                return false;
            }
            
            if (fleet.CanColonize == false)
            {
                message.Text += " but no ships with colonization module were present.";
                messageOut = message;
                return false;
            }
            
            Messages.Clear();
            messageOut = null;
            return true;           
        }
        
        public bool Perform(Fleet fleet, Item target, EmpireData sender, EmpireData reciever, out Message messageOut)
        {
            Message message = null;
            Star star = target as Star;
            if (fleet.Owner == star.Owner)            {
                star.ResourcesOnHand += fleet.Cargo.ToResource();
                star.Colonists += fleet.Cargo.ColonistNumbers;
                message = new Message();
                message.Audience = fleet.Owner;
                message.FleetID = fleet.Id;
                message.Type = "Load/Unload";
                message.Text = fleet.Cargo.ColonistNumbers.ToString() + " colonists beamed to surface of " + star.Name + "."; // This helps the AI by emptying the Colonizer
                fleet.Cargo.Clear();
                message.Audience = fleet.Owner;
                message.Type = "DestToChange";
                messageOut = message;
                Messages.Add(message);
            }
            else if (0 == star.Owner)
            {

                message = new Message();
                message.Audience = fleet.Owner;
                message.Text = " You have colonised " + star.Name + ".";
                message.Type = "DestToChange";
                Messages.Add(message);

                star.ResourcesOnHand = fleet.Cargo.ToResource();
                star.Colonists = fleet.Cargo.ColonistNumbers;
                fleet.Cargo.Clear();
                star.Owner = fleet.Owner;
                star.ThisRace = sender.Race;

                fleet.TotalCost.Energy = 0;
                star.ResourcesOnHand += fleet.TotalCost * 0.75;

                fleet.Composition.Clear();

                sender.OwnedStars.Add(star);
                sender.StarReports[star.Name].Update(star, ScanLevel.Owned, sender.TurnYear);
            }
            messageOut = message;
            return true;
        }
        
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelTask = xmldoc.CreateElement("ColoniseTask");
            
            return xmlelTask;
        }
    }
}
