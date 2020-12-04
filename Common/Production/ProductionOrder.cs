#region Copyright Notice
// ============================================================================
// Copyright (C) 2010, 2011 stars-nova
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    using Nova.Common.Components;

    /// <summary>
    /// Details of a design in the queue.
    /// </summary>
    [Serializable]
    public class ProductionOrder 
    {        
        // Number to build
        public int Quantity
        {
            set;
            get;
        }
        
        public bool IsAutoBuild
        {
            private set;
            get;
        }
        
        public IProductionUnit Unit
        {
            private set;
            get;
        }
        
        public string Name
        {
            get {return Unit.Name;}
        }

        
        /// <summary>
        /// initializing constructor.
        /// </summary>
        /// <param name="quantity">The number of items to produce.</param>
        /// <param name="design">The <see cref="Design"/> to build.</param>
        public ProductionOrder(int quantity, IProductionUnit productionUnit, bool isAutoBuild)
        {
            Quantity = quantity;
            Unit = productionUnit;
            IsAutoBuild = isAutoBuild;            
        }

        
        /// <summary>
        /// Return the resources needed to complete this whole order.
        /// </summary>
        /// <returns></returns>
        public Resources NeededResources()
        {
            Resources neededResources = new Resources();
            
            if (Unit.RemainingCost != Unit.Cost)
            {
                neededResources = Unit.RemainingCost + (Unit.Cost * (Quantity - 1));
            }
            else
            {
                neededResources = Unit.Cost * Quantity;
            }
            
            return neededResources;
        }
        
        
        public bool IsBlocking(Star star, Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {
            return (Unit.IsSkipped(star, race, gravityModCapability, radiationModCapability, temperatureModCapability) && !IsAutoBuild);
        }
        
        
        /// <summary>
        /// Processes this order.
        /// </summary>
        /// <param name="star">The star where this unit is processed</param>
        /// <returns>The number of units completed</returns>
        public int Process(Star star,out List <Message> messageOut, Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {
            Message message = null;
            List<Message> messages = new List<Message>();
            int done = 0;
            int oldQuantity = Quantity;
            while (Quantity > 0)
            {
                if (Unit.IsSkipped(star, race, gravityModCapability, radiationModCapability, temperatureModCapability))
                {
                    break;
                }
   
                if (Unit.Construct(star,out message, race, gravityModCapability, radiationModCapability, temperatureModCapability))
                {
                    if (message != null)
                    {
                        //messages.Add(message);
                        message = null;
                    }
                    Quantity--;
                    done++;
                }                
            }
            if (oldQuantity != Quantity)
            {
                message = new Message();
                message.Audience = star.Owner;
                message.Type = Unit.ToString();
                if (oldQuantity - Quantity == 1) message.Text = star.Name.ToString() + " has built a " + Unit.Name.ToString();
                else message.Text = star.Name.ToString() + " has built " + (oldQuantity - Quantity).ToString() + Unit.Name.ToString();
                messages.Add(message);
            }
            messageOut = messages;
            return done;
        }
        
        /// <summary>
        /// Load: Read in a ProductionQueue.Item from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of a ProductionQueue.Item.</param>
        public ProductionOrder(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;
            while (subnode != null)
            {
                try
                {
                    switch (subnode.Name.ToLower())
                    {
                        case "quantity":
                            Quantity = int.Parse(subnode.FirstChild.Value, CultureInfo.InvariantCulture);
                        break;
                            
                        case "isautobuild":
                            IsAutoBuild = bool.Parse(subnode.FirstChild.Value);
                        break;

                        case "factoryunit":
                            Unit = new FactoryProductionUnit(subnode);
                            break;

                        case "terraformunit":
                            Unit = new TerraformProductionUnit(subnode);
                            break;

                        case "mineunit":
                        Unit = new MineProductionUnit(subnode);
                        break;

                        case "defenseunit":
                        Unit = new DefenseProductionUnit(subnode);
                        break;

                        case "shipunit":
                            Unit = new ShipProductionUnit(subnode);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Report.Error(e.Message);
                }
                subnode = subnode.NextSibling;
            }
        }

        
        /// <summary>
        /// Save: Generate an XmlElement representation of the ProductionQueue.Item for saving.
        /// </summary>
        /// <param name="xmldoc">The parent XmlDocument.</param>
        /// <returns>An XmlElement representation of the ProductionQueue.Item.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelProductionOrder = xmldoc.CreateElement("ProductionOrder");
            
            Global.SaveData(xmldoc, xmlelProductionOrder, "Quantity", Quantity.ToString(CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelProductionOrder, "IsAutoBuild", IsAutoBuild.ToString(CultureInfo.InvariantCulture));
            xmlelProductionOrder.AppendChild(Unit.ToXml(xmldoc));

            return xmlelProductionOrder;
        }
    }
}