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
    using System.Runtime.Versioning;
    using System.Xml;
    using Nova.Common.Components;
    /// <summary>
    /// This class is used for "constructing" terraform 1%.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class TerraformProductionUnit : IProductionUnit
    {
        private Resources cost;
        private Resources remainingCost;        
        
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
            get {return "Terraform";}
        }
        
        /// <summary>
        /// initializing constructor.
        /// </summary>
        /// <param name="star">The star that is producing this item.</param>
        public TerraformProductionUnit(Race race)
        {
            if (race.HasTrait("CA")) cost = new Resources(0, 0, 0, 0);
            else cost = new Resources(0, 0, 0, 70);
            remainingCost = cost;
        }

        /// <summary>
        /// Returns true if this production item is to be skipped this year.
        /// </summary>
        public bool IsSkipped(Star star, Race race = null, int gravityModCapability = 0, int radiationModCapability = 0, int temperatureModCapability = 0)
        {
            Terraform terraform = new Terraform();
            return !terraform.canTerraformOnePoint(star, race, gravityModCapability, radiationModCapability, temperatureModCapability);
        }

        /// <summary>
        /// Construct a 1% terraform.
        /// </summary>
        public bool  Construct(Star star,out Message messageOut,Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {
            Message message = null;
            Terraform terraform = new Terraform();
            star = terraform.terraformOnePoint(star, race,out message, gravityModCapability, radiationModCapability, temperatureModCapability);
            messageOut = message;
            return true;
        }
        public bool Construct(Star star)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the Resources needed for this production item.
        /// </summary>
        public Resources NeededResources()
        {
            throw new NotImplementedException();
        }


        public TerraformProductionUnit(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                switch (mainNode.Name.ToLower())
                {
                    case "cost":
                        cost = new Resources(mainNode);
                        break;

                    case "remainingcost":
                        remainingCost = new Resources(mainNode);
                        break;
                }

                mainNode = mainNode.NextSibling;
            }
        }



        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelUnit = xmldoc.CreateElement("TerraformUnit");

            xmlelUnit.AppendChild(cost.ToXml(xmldoc, "Cost"));

            xmlelUnit.AppendChild(remainingCost.ToXml(xmldoc, "RemainingCost"));

            return xmlelUnit;
        }
    }
}
