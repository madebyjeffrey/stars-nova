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

namespace Nova.Common
{
    using System;
    using System.ComponentModel;
    using System.Xml;

    using Nova.Common.Converters;
    
    public enum ResourceType
    {
        Ironium,
        Boranium,
        Germanium,
        Energy,
        ColonistsInKilotons,
        Silicoxium
    }

    /// <summary>
    /// Resource class which represents the resources needed to construct a game item. 
    /// Individual resource values are either kT (minerals on hand) or percent (mineral concentrations).
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ResourcesConverter))]
    public class Resources
    {
        public int Boranium = 0;
        public int Ironium = 0;
        public int Germanium = 0;
        public int Energy = 0;
        public int Silicoxium = 0;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Resources() 
        { 
        }

        /// <summary>
        /// Initializing Constructor.
        /// </summary>
        public Resources(int i, int b, int g, int e)
        {
            Ironium = i;
            Boranium = b;
            Germanium = g;
            Energy = e;
            Silicoxium = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">Object to copy.</param>
        public Resources(Resources copy)
        {
            this.Ironium = copy.Ironium;
            this.Boranium = copy.Boranium;
            this.Germanium = copy.Germanium;
            this.Energy = copy.Energy;
            this.Silicoxium = 0;
        }

        /// <summary>
        /// See if a resource set is greater than another.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator >=(Resources lhs, Resources rhs)
        {
            if (lhs.Ironium >= rhs.Ironium && lhs.Boranium >= rhs.Boranium &&
                lhs.Germanium >= rhs.Germanium && lhs.Energy >= rhs.Energy)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// See if a resource set is equal to another.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Resources lhs, Resources rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
            {
                return true;
            }
            else if ((object)lhs == null)
            {
                return false;
            }
            else
            {
                return lhs.Equals(rhs);
            }
        }

        /// <summary>
        /// See if a resource set is not equal to another.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(Resources lhs, Resources rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
            {
                return false;
            }
            if (lhs == null)
            {
                return true;
            }
            else
            {
                return !lhs.Equals(rhs);
            }
        }

        /// <summary>
        /// Check if this is equal to the System.Object obj.
        /// </summary>
        /// <param name="obj">Any System.Object to compare.</param>
        /// <returns>true if obj is a Resources and all commodities match.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Resources res = obj as Resources;
            if ((object)res == null)
            {
                return false; // could not be cast
            }
            if (res.Ironium == Ironium && res.Boranium == Boranium && res.Germanium == Germanium && res.Energy == Energy)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if this is equal to the Resources res.
        /// </summary>
        /// <param name="obj">Any Resources to compare.</param>
        /// <returns>Returns true if all commodities match.</returns>
        public bool Equals(Resources res)
        {
            if (res == null)
            {
                return false;
            }

            if (res.Ironium == Ironium && res.Boranium == Boranium && res.Germanium == Germanium && res.Energy == Energy)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generate a hash from the commodities.
        /// </summary>
        /// <returns>Logical exclusive or of the commodities.</returns>
        public override int GetHashCode()
        {
            return Ironium ^ Boranium ^ Germanium ^ Energy;
        }

        /// <summary>
        /// See if a resources set is less than another.
        /// </summary>
        public static bool operator <=(Resources lhs, Resources rhs)
        {
            return rhs >= lhs;
        }

        /// <summary>
        /// Subtract one resource set from another.
        /// </summary>
        public static Resources operator -(Resources lhs, Resources rhs)
        {
            Resources result = new Resources();

            result.Ironium = lhs.Ironium - rhs.Ironium;
            result.Boranium = lhs.Boranium - rhs.Boranium;
            result.Germanium = lhs.Germanium - rhs.Germanium;
            result.Energy = lhs.Energy - rhs.Energy;
            result.Silicoxium = lhs.Silicoxium - rhs.Silicoxium;
            return result;
        }

        /// <summary>
        /// Add a resource set to another.
        /// </summary>
        public static Resources operator +(Resources lhs, Resources rhs)
        {
            Resources result = new Resources();

            result.Ironium = lhs.Ironium + rhs.Ironium;
            result.Boranium = lhs.Boranium + rhs.Boranium;
            result.Germanium = lhs.Germanium + rhs.Germanium;
            result.Energy = lhs.Energy + rhs.Energy;
            result.Silicoxium = lhs.Silicoxium + rhs.Silicoxium;
            return result;
        }

        public static Resources operator *(Resources lhs, int rhs)
        {
            Resources result = new Resources();
            
            result.Ironium = lhs.Ironium * rhs;
            result.Boranium = lhs.Boranium * rhs;
            result.Germanium = lhs.Germanium * rhs;
            result.Energy = lhs.Energy * rhs;
            result.Silicoxium = lhs.Silicoxium * rhs;
            return result;
        }
        public static double operator /(Resources lhs, Resources rhs)
        {
            double result = 0.0;

            double i = lhs.Ironium / Math.Max(0.1,rhs.Ironium);  //prevent negative numbers and divide by zero errors
            double b = lhs.Boranium / Math.Max(0.1,rhs.Boranium);
            double g = lhs.Germanium / Math.Max(0.1,rhs.Germanium);
            double e = lhs.Energy / Math.Max(0.1,rhs.Energy);
            result = Math.Min(Math.Min(Math.Min(i, b), g), e);
            return result;
        }
        public static Resources operator *(int lhs, Resources rhs)
        {
            return rhs * lhs;
        }
        
        // Rounding can cause one more resource to be consumed than we have
        public static Resources operator *(Resources lhs, double rhs)
        {
            Resources result = new Resources();
            
            result.Ironium = (int)Math.Ceiling((double)lhs.Ironium * rhs);
            result.Boranium = (int)Math.Ceiling((double)lhs.Boranium * rhs);
            result.Germanium = (int)Math.Ceiling((double)lhs.Germanium * rhs);
            result.Energy = (int)Math.Ceiling((double)lhs.Energy * rhs);
            result.Silicoxium = (int)Math.Ceiling((double)lhs.Silicoxium * rhs);
            return result;
        }
        
        public static Resources operator *(double lhs, Resources rhs)
        {
            return rhs * lhs;
        }

        /// <summary>
        /// Return the mass of a resource set (Energy does not contribute to the mass).
        /// </summary>
        public int Mass
        {
            get { return Ironium + Boranium + Germanium + Silicoxium; }
        }

        /// <summary>
        /// Load from XML: initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">A node is a "resource" <see cref="XmlNode"/> in a Nova component definition file (xml document).
        /// </param>
        public Resources(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "ironium":
                            Ironium = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "boranium":
                            Boranium = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "germanium":
                            Germanium = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "energy":
                            Energy = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "silicoxium":
                            Silicoxium = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
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
        /// Save: Serialize this Resources to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent.<see cref="XmlDocument"/>.</param>
        /// <param name ="nodeName">The name this resource node will have on the XML file. Default is "Cost".</param>
        /// <returns>Return an <see cref="XmlElement"/> representation of the resource cost.</returns>
        public XmlElement ToXml(XmlDocument xmldoc, string nodeName = "Cost")
        {
            XmlElement xmlelResource = xmldoc.CreateElement(nodeName);

            // Boranium
            if (this.Boranium > 0)
            {
                XmlElement xmlelBoranium = xmldoc.CreateElement("Boranium");
                XmlText xmltxtBoranium = xmldoc.CreateTextNode(this.Boranium.ToString(System.Globalization.CultureInfo.InvariantCulture));
                xmlelBoranium.AppendChild(xmltxtBoranium);
                xmlelResource.AppendChild(xmlelBoranium);
            }
            // Ironium
            if (this.Ironium > 0)
            {
                XmlElement xmlelIronium = xmldoc.CreateElement("Ironium");
                XmlText xmltxtIronium = xmldoc.CreateTextNode(this.Ironium.ToString(System.Globalization.CultureInfo.InvariantCulture));
                xmlelIronium.AppendChild(xmltxtIronium);
                xmlelResource.AppendChild(xmlelIronium);
            }
            // Germanium
            if (this.Germanium > 0)
            {
                XmlElement xmlelGermanium = xmldoc.CreateElement("Germanium");
                XmlText xmltxtGermanium = xmldoc.CreateTextNode(this.Germanium.ToString(System.Globalization.CultureInfo.InvariantCulture));
                xmlelGermanium.AppendChild(xmltxtGermanium);
                xmlelResource.AppendChild(xmlelGermanium);
            }
            // Energy
            if (this.Energy > 0)
            {
                XmlElement xmlelEnergy = xmldoc.CreateElement("Energy");
                XmlText xmltxtEnergy = xmldoc.CreateTextNode(this.Energy.ToString(System.Globalization.CultureInfo.InvariantCulture));
                xmlelEnergy.AppendChild(xmltxtEnergy);
                xmlelResource.AppendChild(xmlelEnergy);
            }
            // Silicoxium
            if (this.Silicoxium > 0)
            {
                XmlElement xmlelSilicoxium = xmldoc.CreateElement("Silicoxium");
                XmlText xmltxtSilicoxium = xmldoc.CreateTextNode(this.Silicoxium.ToString(System.Globalization.CultureInfo.InvariantCulture));
                xmlelSilicoxium.AppendChild(xmltxtSilicoxium);
                xmlelResource.AppendChild(xmlelSilicoxium);
            }
            return xmlelResource;
        }
    }
}