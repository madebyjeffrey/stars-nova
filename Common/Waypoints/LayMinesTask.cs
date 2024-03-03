using System.Linq;
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
    
    /// <summary>
    /// Performs Space Mine Laying.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class LayMinesTask : IWaypointTask
    {
        private List<Message> messages = new List<Message>();
        
        public List<Message> Messages
        {
            get{ return messages;}
        }

        int mineType = 0; // 0 = standard   1= heavy  2 = smart

        public string Name
        {
            get{return "Lay Mines";}
        }

        public int MineType
        {
            get
            {
                return mineType;
            }
            set
            {
                mineType = MineType;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>// 0 = standard   1= heavy  2 = smart
        public LayMinesTask(int type = 0)
        {
            mineType = type;
        }

        /// <summary>
        /// Load: Read in a ColoniseTask from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of a ProductionUnit</param>
        public LayMinesTask(XmlNode node)
        {
            if (node == null)
            {
                return;
            }
            mineType = 0; // default value is not saved
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "minetype":
                            mineType = int.Parse(mainNode.FirstChild.Value);
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


            public bool IsValid(Fleet fleet, Item target, EmpireData sender, EmpireData receiver,out Message messageOut)
        {
            Message message = new Message();
            Messages.Add(message);            
            message.Audience = fleet.Owner;
            message.FleetKey = fleet.Key;
            message.Event = fleet;
            message.Type = "Fleet";

            if ((fleet.NumberOfMines == 0) && (fleet.NumberOfHeavyMines == 0) && (fleet.NumberOfSpeedBumpMines == 0))
            {
                message.Text = fleet.Name + " attempted to lay mines. The order has been canceled because no ship in the fleet has a mine laying pod.";
                messageOut = message;
                return false;
            }
            
            Messages.Clear();
            messageOut = null;
            return true;           
        }
        
        public bool Perform(Fleet fleet, Item target, EmpireData sender, EmpireData receiver,out Message message)
        {
            // See if a Minefield is already here (owned by us). We allow a
            // certain tolerance in distance because it is unlikely that the
            // waypoint has been set exactly right.

            //TODO: Implement per empire minefields.
            /*foreach (Minefield minefield in serverState.AllMinefields.Values)
            {
                if (PointUtilities.IsNear(fleet.Position, minefield.Position))
                {
                    if (minefield.Owner == fleet.Owner)
                    {
                        minefield.NumberOfMines += fleet.NumberOfMines;
                        return true;
                    }
                }
            }
    
            // No Minefield found. Start a new one.
    
            Minefield newField = new Minefield();
    
            newField.Position = fleet.Position;
            newField.Owner = fleet.Owner;
            newField.NumberOfMines = fleet.NumberOfMines;
    
            serverState.AllMinefields[newField.Key] = newField;*/
            message = null;
            return true;
        }
        
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelTask = xmldoc.CreateElement("LayMinesTask");
            if (mineType != 0) Global.SaveData(xmldoc, xmlelTask, "MineType", mineType.ToString());

            return xmlelTask;
        }
    }
}
