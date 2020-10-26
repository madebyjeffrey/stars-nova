#region Copyright Notice
// ============================================================================
// Copyright (C) 2011-2012 The Stars-Nova Project
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

    using Nova.Common.DataStructures;

    using Nova.Common.Components;

    /// <summary>
    /// This module describes the basic things we can
    /// know about a Star system.
    /// </summary>
    [Serializable]
    public class StarIntel : Mappable
    {
        public int          Year                    { get; set; }
        public Resources    MineralConcentration    { get; set; }
        public int          Gravity                 { get; set; }
        public int          Radiation               { get; set; }
        public int          Temperature             { get; set; }
        public int          Colonists               { get; set; }
        public bool         HasFleetsInOrbit        { get; set; }
        public bool         HasRefuelerInOrbit      { get; set; }
        public bool         HasFreeTransportInOrbit { get; set; }
        public Fleet        Starbase                { get; set; }
        public Resources    ResourcesOnHand         { get; set; }
        public int          baseRadiation           { get;  set; }
        public int          baseGravity             { get;  set; }
        public int          baseTemperature         { get;  set; }

        /// <summary>
        /// Stars are Keyed by name, so overload.
        /// </summary>
        public new string Key
        {
            get 
            { 
                return Name; 
            }
        }
       
        /// <summary>
        /// Default constructor. Sets sensible but meaningless default values for this report.
        /// </summary>
        public StarIntel() :
            base()
        {
            Clear();
        }
        
        /// <summary>
        /// Creates a star report from a star.
        /// </summary>
        /// <param name="star">Star to report.</param>
        /// <param name="scan">Amount of Knowledge to set.</param>
        /// <param name="year">Year of the data.</param>
        public StarIntel(Star star, ScanLevel scan, int year) :
            base()
        {
            Clear();
            Update(star, scan, year);
        }
        
        /// <summary>
        /// Load: initializing constructor to read in a Star report from an XmlNode (from a saved file).
        /// </summary>
        /// <param name="xmlnode">An XmlNode representing a Star report.</param>
        public StarIntel(XmlNode node) :
            base(node)
        {
            XmlNode mainNode = node.FirstChild;
            
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "year":
                            Year = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "mineralconcentration":
                            MineralConcentration = new Resources(mainNode);
                            break;
                        case "gravity":
                            Gravity = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "radiation":
                            Radiation = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "temperature":
                            Temperature = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "resourcesonhand":
                            ResourcesOnHand = new Resources(mainNode);
                            break;
                        case "colonists":
                            Colonists = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "hasfleetsinorbit":
                            HasFleetsInOrbit = bool.Parse(mainNode.FirstChild.Value);
                            break;
                        case "hasrefuelerinorbit":
                            HasRefuelerInOrbit = bool.Parse(mainNode.FirstChild.Value);
                            break;
                        case "hasfreetransportinorbit":
                            HasFreeTransportInOrbit = bool.Parse(mainNode.FirstChild.Value);
                            break;
                        case "starbase":
                            Starbase = new Fleet(long.Parse(mainNode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber));
                            break;
                        case "baseradiation":
                            baseRadiation = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "basegravity":
                            baseGravity = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "basetemperature":
                            baseTemperature = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Report.FatalError(e.Message + "\n Details: \n" + e.ToString());
                }
                
                mainNode = mainNode.NextSibling;
            }           
        }
        
        /// <summary>
        /// Resets all values to default.
        /// </summary>
        public void Clear()
        {
            Year                    = Global.Unset;
            Name                    = string.Empty;
            Position                = new NovaPoint();
            Owner                   = Global.Nobody;
            Type                    = ItemType.StarIntel;
            MineralConcentration    = new Resources();
            ResourcesOnHand         = new Resources();
            Gravity                 = Global.Unset;
            Radiation               = Global.Unset;
            Temperature             = Global.Unset;
            Colonists               = Global.Unset;
            baseRadiation           = Global.Unset;
            baseTemperature         = Global.Unset;
            baseGravity             = Global.Unset;
            HasFleetsInOrbit        = false;
            HasRefuelerInOrbit      = false;
            HasFreeTransportInOrbit = false;
            Starbase = null;            
        }
 
        


        public bool mineralRich()
        {
            return (MineralConcentration.Ironium > 40) && (MineralConcentration.Boranium > 30) && (MineralConcentration.Germanium > 40);
        }
        /// <summary>
        /// Returns the name of the Star.
        /// </summary>
        /// <returns>A string with the format "Star: [name]".</returns>
        public override string ToString()
        {
            return "Star: " + Name;
        }

        /// <summary>
        /// Updates the report with data from a star.
        /// </summary>
        /// <param name="star">Star to report.</param>
        /// <param name="scan">Amount of Knowledge to set.</param>
        /// <param name="year">Year of the updated data.</param>
        public void Update(Star star, ScanLevel scan, int year,bool set = false)
        {
            if (set)
            {
                star.HasFleetsInOrbit = HasFleetsInOrbit;
                star.HasRefuelerInOrbit = HasRefuelerInOrbit;
                star.HasFreeTransportInOrbit = HasFreeTransportInOrbit;
            }
            Clear();
            if (!set)
            {
                HasFleetsInOrbit = star.HasFleetsInOrbit;
                HasRefuelerInOrbit = star.HasRefuelerInOrbit;
                HasFreeTransportInOrbit = star.HasFreeTransportInOrbit;
            }

            if (star == null)
            {
                return;
            }

            if (year < this.Year)
            {
                return;
            }

            // Information that is always available and doesn't
            // depend on scanning level.
            Name = star.Name;
            Position = star.Position; // Can this change? Random Events?
            baseGravity = star.OriginalGravity; // we don't use this until the star is discovered
            baseRadiation = star.OriginalRadiation;
            baseTemperature = star.OriginalTemperature;

            if (scan >= ScanLevel.None)
            {
                // We can't see this star.
            }

            // If we are at least scanning with non-penetrating
            if (scan >= ScanLevel.InScan)
            {
                // Non-pen scanners are useless for stars.
            }

            // If we are at least currently in orbit of the star
            // with no scanners.
            if (scan >= ScanLevel.InPlace)
            {

                Year = year;

                Owner = star.Owner;
                MineralConcentration = star.MineralConcentration;
                Gravity = star.Gravity;
                Radiation = star.Radiation;
                Temperature = star.Temperature;
                Starbase = star.Starbase;
                ResourcesOnHand = star.ResourcesOnHand;

            }

            // If we are have Pen-Scanners, or we are
            // in orbit with scanners.
            if (scan >= ScanLevel.InDeepScan)
            {
                Colonists = star.Colonists;
            }

            // If the star is ours.
            if (scan >= ScanLevel.Owned)
            {
                ResourcesOnHand = star.ResourcesOnHand;
                // We do nothing, as owned Stars are handled
                // elsewhere.
            }
        }

        public int MinValue(Race race)
        {
            double habitableValue = race.HabValue(this);
            double growthRate = race.GrowthRate;

            if (race.HasTrait("HyperExpansion"))
            {
                growthRate *= Global.GrowthFactorHyperExpansion;
            }




            double minValue = growthRate  * habitableValue * 100;
            return (int)minValue;
        }
        public int MaxValue(Race race,int gravityModCapability,int temperatureModCapability,int radiationModCapability)
        {
            //Terraform terraformProperty = new Terraform();
            //Terraform maxTerraform = new Terraform(terraformProperty);
            
            double habitableValue = race.HabValue(this,true,gravityModCapability,temperatureModCapability,radiationModCapability);
            double growthRate = race.GrowthRate;

            if (race.HasTrait("HyperExpansion"))
            {
                growthRate *= Global.GrowthFactorHyperExpansion;
            }




            double maxValue = growthRate  * habitableValue * 100;
            return (int)maxValue;
        }

        /// <summary>
        /// Create an XmlElement representation of the star report for saving.
        /// </summary>
        /// <param name="xmldoc">The parent XmlDocument.</param>
        /// <returns>An XmlElement representation of the report.</returns>
        public new XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelStarIntel = xmldoc.CreateElement("StarIntel");
            
            // include inherited Item properties
            xmlelStarIntel.AppendChild(base.ToXml(xmldoc));
            
            Global.SaveData(xmldoc, xmlelStarIntel, "Year", Year.ToString(System.Globalization.CultureInfo.InvariantCulture));
            
            xmlelStarIntel.AppendChild(MineralConcentration.ToXml(xmldoc, "MineralConcentration"));
            xmlelStarIntel.AppendChild(ResourcesOnHand.ToXml(xmldoc, "ResourcesOnHand"));

            Global.SaveData(xmldoc, xmlelStarIntel, "Gravity", Gravity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelStarIntel, "Radiation", Radiation.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelStarIntel, "Temperature", Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelStarIntel, "baseGravity", baseGravity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelStarIntel, "baseRadiation", baseRadiation.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelStarIntel, "baseTemperature", baseTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture));

            Global.SaveData(xmldoc, xmlelStarIntel, "Colonists", Colonists.ToString(System.Globalization.CultureInfo.InvariantCulture));

            Global.SaveData(xmldoc, xmlelStarIntel, "HasFleetsInOrbit", HasFleetsInOrbit.ToString());
            Global.SaveData(xmldoc, xmlelStarIntel, "HasRefuelerInOrbit", HasRefuelerInOrbit.ToString());
            Global.SaveData(xmldoc, xmlelStarIntel, "HasFreeTransportInOrbit", HasFreeTransportInOrbit.ToString());

            if (Starbase != null)
            {
                Global.SaveData(xmldoc, xmlelStarIntel, "Starbase", Starbase.Key.ToString("X"));
            }

            return xmlelStarIntel;   
        }
    }
}
