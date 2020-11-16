#region Copyright Notice
// ============================================================================
// Copyright (C) 2010 stars-nova
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

namespace Nova.Common
{
    using System;
    using System.Xml;
    
    using Nova.Common.Components;

    /// <summary>
    /// ShipProductionUnit class. This class is used for constructing 1 ship.
    /// </summary>
    public class ShipProductionUnit : IProductionUnit
    {
        private long designKey;
        private string name;
        private Resources cost;
        private Resources remainingCost;
        private string star = "";
        public Resources Cost
        {
            get {return cost;}
        }
                
        public Resources RemainingCost
        {
            get {return remainingCost;}
        }
        
        public string Name
        {
            get {return name;}
        }
        
        public long DesignKey
        {
            get {return designKey;}   
        }

        public String Star
        {
            get { return star; }
        }

        /// <summary>
        /// initializing constructor.
        /// </summary>
        /// <param name="star">Star with this production queue.</param>
        /// <param name="shipDesign"><see cref="ShipDesign"/> to produce.</param>
        public ShipProductionUnit(ShipDesign shipDesign, String starName ,EmpireData sender)
        {
            shipDesign.Update();
            designKey = shipDesign.Key;
            name = shipDesign.Name;
            cost = shipDesign.Cost;
            if ((sender.Designs[shipDesign.Key].IsStarbase) && (sender.StarReports[starName].Starbase !=null))
                cost = shipDesign.Cost
                 - sender.OwnedStars[starName].Starbase.TotalCost; // A 3rd party client could send the name of a planet with a big starbase in "star"
                                                                               // so ensure the server ignores the star name and uses the production queues owner.Name
            cost.Ironium = Math.Max(0, cost.Ironium);                           // TODO (priority 1) find the real formula for starbase upgrades
            cost.Boranium = Math.Max(0, cost.Boranium);                         // Stars! behaviour is close to this: compare each hull.module.allocated in the new design with the old design 
            cost.Germanium = Math.Max(0, cost.Germanium);                       //and just add the cost of modules that are not the same
                                                                                // TODO when upgrading to a better Starbase Hull Stars! calculates the cost by mapping modules from the old hull to modules on the new hull - copy that mapping to Nova
            cost.Energy = Math.Max(0, cost.Energy);                             // both Client and Server and AI need the formula
            remainingCost = cost;                                               //Server shipProductionCommand.IsValid needs to verify the cost that the Client calculated
            star = starName;
        }


        /// <summary>
        /// Load: Read in a ProductionUnit from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of a ProductionUnit</param>
        public ShipProductionUnit(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "cost":
                            cost = new Resources(mainNode);
                            break;
                            
                        case "remainingcost":
                            remainingCost = new Resources(mainNode);
                            break;
                            
                        case "name":
                            name = mainNode.FirstChild.Value;
                            break;

                        case "designkey":
                            designKey = long.Parse(mainNode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
                            break;

                        case "star":
                            star = mainNode.FirstChild.Value;
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
        
        
        /// <summary>
        /// Return true if production of this item will be skipped.
        /// </summary>
        public bool IsSkipped(Star star)
        {
            // Skip if unit needs a resource and there is no amount of that resource available
            // Note that a zero resource cost unit does not exist in Stars!, but check just incase a mod adds one.
            if (
                star.ResourcesOnHand.Energy <= 0 && cost.Energy > 0||
                (star.ResourcesOnHand.Germanium <= 0 && cost.Germanium > 0) ||
                (star.ResourcesOnHand.Boranium <= 0 && cost.Boranium > 0) ||
                (star.ResourcesOnHand.Ironium <= 0 && cost.Ironium > 0)
                )
            {
                return true;
            }

            return false;
        }

        
        /// <summary>
        /// Construct the ship.
        /// </summary>
        public bool Construct(Star star)
        {
            // Partial Build
            if (!(star.ResourcesOnHand >= remainingCost))
            {
                // used to temporarily store the amount of resources we are short
                Resources lacking = remainingCost - star.ResourcesOnHand;               
                
                // Normalized; 1.0 = 100%
                double percentBuildable = 1.0;
                
                // determine which resource limits production (i.e. is able to complete the smallest percentage of production)
                if (percentBuildable > (1 - ((double)lacking.Ironium / remainingCost.Ironium)) && lacking.Ironium > 0)
                {
                    percentBuildable = 1 - ((double)lacking.Ironium / remainingCost.Ironium);
                }
    
                if (percentBuildable > (1 - ((double)lacking.Boranium / remainingCost.Boranium)) && lacking.Boranium > 0)
                {
                    percentBuildable = 1 - ((double)lacking.Boranium / remainingCost.Boranium);
                }
    
                if (percentBuildable > (1 - ((double)lacking.Germanium / remainingCost.Germanium)) && lacking.Germanium > 0)
                {
                    percentBuildable = 1 - ((double)lacking.Germanium / remainingCost.Germanium);
                }
    
                if (percentBuildable > (1 - ((double)lacking.Energy / remainingCost.Energy)) && lacking.Energy > 0)
                {
                    percentBuildable = 1 - ((double)lacking.Energy / remainingCost.Energy);
                }
                
                // What we spend on the partial build.
                star.ResourcesOnHand -= remainingCost * percentBuildable;
                if (star.ResourcesOnHand.Ironium < 0) star.ResourcesOnHand.Ironium = 0;   // correct rounding errors that reduce resources below zero
                if (star.ResourcesOnHand.Boranium < 0) star.ResourcesOnHand.Boranium = 0;
                if (star.ResourcesOnHand.Germanium < 0) star.ResourcesOnHand.Germanium = 0;
                if (star.ResourcesOnHand.Energy < 0) star.ResourcesOnHand.Energy = 0;
                remainingCost -= remainingCost * percentBuildable;
                
                return false;
            }
            else
            {
                star.ResourcesOnHand -= remainingCost;
                if (star.ResourcesOnHand.Ironium < 0) star.ResourcesOnHand.Ironium = 0;   // correct rounding errors that reduce resources below zero
                if (star.ResourcesOnHand.Boranium < 0) star.ResourcesOnHand.Boranium = 0;
                if (star.ResourcesOnHand.Germanium < 0) star.ResourcesOnHand.Germanium = 0;
                if (star.ResourcesOnHand.Energy < 0) star.ResourcesOnHand.Energy = 0;
                return true;
            }
        }
           
        
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelUnit = xmldoc.CreateElement("ShipUnit");
            
            xmlelUnit.AppendChild(cost.ToXml(xmldoc, "Cost"));
            
            xmlelUnit.AppendChild(remainingCost.ToXml(xmldoc, "RemainingCost"));

            Global.SaveData(xmldoc, xmlelUnit, "Name", Name);
            Global.SaveData(xmldoc, xmlelUnit, "DesignKey", designKey.ToString("X"));
            if (star!="") Global.SaveData(xmldoc, xmlelUnit, "Star", star);

            return xmlelUnit;
        }
    }
}
