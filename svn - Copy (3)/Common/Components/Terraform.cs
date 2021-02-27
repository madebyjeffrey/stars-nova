#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010 stars-nova
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

namespace Nova.Common.Components
{
    using System;
    using System.Xml;
    using Nova.Common;

    /// <summary>
    /// This class defines a terraforming property.
    /// </summary>
    [Serializable]
    public class Terraform : ComponentProperty
    {
        public int MaxModifiedGravity     = 0;
        public int MaxModifiedTemperature = 0;
        public int MaxModifiedRadiation   = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Terraform()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="existing">The object to copy.</param>
        public Terraform(Terraform existing)
        {
            this.MaxModifiedGravity = existing.MaxModifiedGravity;
            this.MaxModifiedTemperature = existing.MaxModifiedTemperature;
            this.MaxModifiedRadiation = existing.MaxModifiedRadiation;
        }

        /// <summary>
        /// Implement the ICloneable interface so properties can be cloned.
        /// </summary>
        /// <returns>A clone of this object.</returns>
        public override object Clone()
        {
            return new Terraform(this);
        }

        /// <summary>
        /// Polymorphic addition of properties.
        /// </summary>
        /// <param name="op2"></param>
        public override void Add(ComponentProperty op2)
        {
            MaxModifiedGravity = Math.Max(MaxModifiedGravity, ((Terraform)op2).MaxModifiedGravity);
            MaxModifiedRadiation = Math.Max(MaxModifiedRadiation, ((Terraform)op2).MaxModifiedRadiation);
            MaxModifiedTemperature = Math.Max(MaxModifiedTemperature, ((Terraform)op2).MaxModifiedTemperature);
        }

        /// <summary>
        /// Polymorphic multiplication of properties.
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public override void Scale(int scalar)
        {
            return;
        }

        /// <summary>
        /// Provide a way to add properties in the ship design.
        /// </summary>
        /// <param name="op1">LHS operator.</param>
        /// <param name="op2">RHS operator.</param>
        /// <returns>A single terraform property that represents the stack.</returns>
        public static Terraform operator +(Terraform op1, Terraform op2)
        {
            Terraform sum = new Terraform(op1);
            sum.MaxModifiedGravity = Math.Max(op1.MaxModifiedGravity, op2.MaxModifiedGravity);
            sum.MaxModifiedRadiation = Math.Max(op1.MaxModifiedRadiation, op2.MaxModifiedRadiation);
            sum.MaxModifiedTemperature = Math.Max(op1.MaxModifiedTemperature, op2.MaxModifiedTemperature);
            return sum;
        }

        /// <summary>
        /// Operator* to scale (multiply) properties in the ship design.
        /// Terraformers don't scale, as the modifications represent maximums.
        /// Note this represents the terraforming capability of a component,
        /// not multiple terraforming units produced by a planet, which work differently (1% each).
        /// </summary>
        /// <param name="op1">Property to be scaled.</param>
        /// <param name="scalar">Number of components in the stack.</param>
        /// <returns>A single property that represents the stack.</returns>
        public static Terraform operator *(Terraform op1, int scalar)
        {
            return op1.Clone() as Terraform;
        }

        public bool isRadiationMaxed(Star star, Race race, int radiationModCapability)
        {
            bool result = true;
            result = (Math.Abs(star.OriginalRadiation - star.Radiation) >= radiationModCapability);  //we have terraformed this attribute to the Max
            int RadAbove = star.Radiation - race.RadiationTolerance.MaximumValue;
            int RadBelow = race.RadiationTolerance.MinimumValue - star.Radiation;
            if (Math.Abs(Math.Abs(RadAbove) - Math.Abs(RadBelow)) <= 1) result = false; // we could terraform by 1/2 point but if we terraform 1 point we will have gone too far
            return result;
        }

        public bool isTemperatureMaxed(Star star, Race race, int TemperatureModCapability)
        {
            bool result = true;
            result = (Math.Abs(star.OriginalTemperature - star.Temperature) >= TemperatureModCapability); //we have terraformed this attribute to the Max
            int TempAbove = star.Temperature - race.TemperatureTolerance.MaximumValue;
            int TempBelow = race.TemperatureTolerance.MinimumValue - star.Temperature;
            if (Math.Abs(Math.Abs(TempAbove) - Math.Abs(TempBelow)) <= 1) result = false; // we could terraform by 1/2 point but if we terraform 1 point we will have gone too far
            return result;
        }

        public bool isGravityMaxed(Star star, Race race, int gravityModCapability)
        {
            bool result = true;
            result = (Math.Abs(star.OriginalGravity - star.Gravity) >= gravityModCapability);  //we have terraformed this attribute to the Max
            int GravAbove = star.Gravity - race.GravityTolerance.MaximumValue;
            int GravBelow = race.GravityTolerance.MinimumValue - star.Radiation;
            if (Math.Abs(Math.Abs(GravAbove) - Math.Abs(GravBelow)) <= 1) result = false; // we could terraform by 1/2 point but if we terraform 1 point we will have gone too far
            return result;
        }


        public bool isEnvironmentIdeal(Star star, Race race)
        {
            //if star is already 100% don't terraform it
            return (1.0 == race.HabValue(star,false,0,0,0));
        }

        /// <summary>
        /// Returns true if the tech level is high enough for this race to perform one level of terraforming
        /// </summary>
        /// <param name="star"></param>
        /// <param name="race"></param>
        /// <param name="gravityModCapability"></param>
        /// <param name="radiationModCapability"></param>
        /// <param name="temperatureModCapability"></param>
        /// <returns></returns>
        public bool canTerraformOnePoint(Star star,  Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {

           return !((isEnvironmentIdeal(star, race)) ||
            ((isRadiationMaxed(star, race, radiationModCapability)) &&
            (isTemperatureMaxed(star, race, temperatureModCapability)) &&
            (isGravityMaxed(star, race, gravityModCapability))) 
            );

        }


        /// <summary>
        /// MUST call canTerraformOnePoint(Star star,  Race race, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        /// to test if it is OK to call this code
        /// </summary>
        /// <param name="star"></param>
        /// <param name="race"></param>
        /// <param name="gravityModCapability"></param>
        /// <param name="radiationModCapability"></param>
        /// <param name="temperatureModCapability"></param>
        /// <returns></returns>
        public Star terraformOnePoint(Star star, Race race, out Message messageOut, int gravityModCapability, int radiationModCapability, int temperatureModCapability)
        {
            Message message = null;
            int RadAbove = star.Radiation - race.RadiationTolerance.MaximumValue;
            int RadBelow = race.RadiationTolerance.MinimumValue - star.Radiation;
            int RadiationHostility = Math.Max(RadAbove, RadBelow);
            if (Math.Abs(Math.Abs(RadAbove) + Math.Abs(RadBelow)) <= 1) RadiationHostility = int.MinValue; // within 1 point so no benefit to change it
            if (isRadiationMaxed(star, race, radiationModCapability)) RadiationHostility = int.MinValue;  // we can't choose this one
            int TempAbove = star.Temperature - race.TemperatureTolerance.MaximumValue;
            int TempBelow = race.TemperatureTolerance.MinimumValue - star.Temperature;
            int TemperatureHostility = Math.Max(TempAbove, TempBelow);
            if (Math.Abs(Math.Abs(TempAbove) + Math.Abs(TempBelow)) <= 1) TemperatureHostility = int.MinValue; // within 1 point so no benefit to change it
            if (isTemperatureMaxed(star, race, temperatureModCapability)) TemperatureHostility = int.MinValue;  // we can't choose this one
            int GravAbove = star.Gravity - race.GravityTolerance.MaximumValue;
            int GravBelow = race.GravityTolerance.MinimumValue - star.Radiation;
            int GravityHostility = Math.Max(GravAbove, GravBelow);
            if (Math.Abs(Math.Abs(GravAbove) + Math.Abs(GravBelow)) <= 1) GravityHostility = int.MinValue; // within 1 point so no benefit to change it
            if (isGravityMaxed(star, race, gravityModCapability)) GravityHostility = int.MinValue;  // we can't choose this one

            if ((RadiationHostility >= TemperatureHostility) && (RadiationHostility >= GravityHostility)) star = fixRadiation(star, race, out message);
            else if ((TemperatureHostility >= RadiationHostility) && (TemperatureHostility >= GravityHostility)) star = fixTemperature(star, race, out message);
            else if ((GravityHostility >= RadiationHostility) && (GravityHostility >= TemperatureHostility)) star = fixGravity(star, race, out message);
            else message = null;
            messageOut = message;
            return star;
        }

        public Star fixRadiation(Star star, Race race, out Message message)
        {
            int Above = star.Radiation - race.RadiationTolerance.MaximumValue;
            int Below = race.RadiationTolerance.MinimumValue - star.Radiation;

            if (Above > Below)
            {
                star.Radiation -= 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has decreased its Radiation to " + star.Gravity.ToString() + "mR\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else if (Below > Above)
            {
                star.Radiation += 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has increased its Radiation to " + star.Gravity.ToString() + "mR\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else message = null;
            return star;
        }

        public Star fixTemperature(Star star, Race race, out Message message)
        {
            int Above = star.Temperature - race.TemperatureTolerance.MaximumValue;
            int Below = race.TemperatureTolerance.MinimumValue - star.Temperature;
            if (Above > Below)
            {
                star.Temperature -= 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has decreased its Temperature to " + star.Gravity.ToString() + "°C\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else if (Above < Below)
            {
                star.Temperature += 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has increased its Temperature to " + star.Gravity.ToString() + "°C\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else message = null;
            return star;
        }

        public Star fixGravity(Star star, Race race, out Message message)
        {
            int Above = star.Gravity - race.GravityTolerance.MaximumValue;
            int Below = race.GravityTolerance.MinimumValue - star.Gravity;
            if (Above > Below)
            {
                star.Gravity -= 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has decreased its Gravity to " + star.Gravity.ToString() + "g\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else if (Above < Below)

            {
                star.Gravity += 1;
                message = new Message();
                message.Event = star;
                message.Audience = star.Owner;
                message.Type = "Terraform";
                message.Text = star.Name.ToString() + " has increased its Gravity to " + star.Gravity.ToString() + "g\r\n it's value is now " + Math.Ceiling(race.HabValue(star) * 100) + "%";
            }
            else message = null;
            return star;
        }

        /// <summary>
        /// Load from XML: initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> within
        /// a Nova component definition file (xml document).
        /// </param>
        public Terraform(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;
            while (subnode != null)
            {
                try
                {
                    if (subnode.Name.ToLower() == "maxmodifiedgravity")
                    {
                        MaxModifiedGravity = int.Parse(((XmlText)subnode.FirstChild).Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (subnode.Name.ToLower() == "maxmodifiedtemperature")
                    {
                        MaxModifiedTemperature = int.Parse(((XmlText)subnode.FirstChild).Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (subnode.Name.ToLower() == "maxmodifiedradiation")
                    {
                        MaxModifiedRadiation = int.Parse(((XmlText)subnode.FirstChild).Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception e)
                {
                    Report.Error("Unable to load terraforming property: " + Environment.NewLine + e.Message);
                }
                subnode = subnode.NextSibling;
            }
        }

        /// <summary>
        /// Save: Serialize this property to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Property.</returns>
        public override XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelProperty = xmldoc.CreateElement("Property");

            // MaxModifiedGravity
            XmlElement xmlelMaxModifiedGravity = xmldoc.CreateElement("MaxModifiedGravity");
            XmlText xmltxtMaxModifiedGravity = xmldoc.CreateTextNode(this.MaxModifiedGravity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlelMaxModifiedGravity.AppendChild(xmltxtMaxModifiedGravity);
            xmlelProperty.AppendChild(xmlelMaxModifiedGravity);
            // MaxModifiedTemperature
            XmlElement xmlelMaxModifiedTemperature = xmldoc.CreateElement("MaxModifiedTemperature");
            XmlText xmltxtMaxModifiedTemperature = xmldoc.CreateTextNode(this.MaxModifiedTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlelMaxModifiedTemperature.AppendChild(xmltxtMaxModifiedTemperature);
            xmlelProperty.AppendChild(xmlelMaxModifiedTemperature);
            // MaxModifiedRadiation
            XmlElement xmlelMaxModifiedRadiation = xmldoc.CreateElement("MaxModifiedRadiation");
            XmlText xmltxtMaxModifiedRadiation = xmldoc.CreateTextNode(this.MaxModifiedRadiation.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlelMaxModifiedRadiation.AppendChild(xmltxtMaxModifiedRadiation);
            xmlelProperty.AppendChild(xmlelMaxModifiedRadiation);

            return xmlelProperty;
        }
    }
}

