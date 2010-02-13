﻿// ============================================================================
// Nova. (c) 2009 Daniel Vale
//
// The environmental range a race can tollerate.
//
// TODO - What are the full environment ranges? Min&Max for each variable.

/* From http://www.starsfaq.com/advfaq/guts2.htm#4.11
 * 4.11) Guts of Planet Values

(I haven't included the explanation of how the formula was derived; if you're interested, go to deja news and look up "re: Race wizard - Hab studies" by Bill Butler, 1998/04/10

The full equation is:

Hab%=SQRT[(1-g)^2+(1-t)^2+(1-r)^2]*(1-x)*(1-y)*(1-z)/SQRT[3]

Where g,t,and r (standing for gravity, temperature, and radiation)are given by
Clicks_from_center/Total_clicks_from_center_to_edge

and where x,y, and z  are
x=g-1/2 for g>1/2       x=0 for g<1/2
y=t-1/2 for t>1/2         y=0 for t<1/2
z=r-1/2 for r>1/2         z=0 for r<1/2

The farther habs are from center, the less accurate the result of this equation will be.  However, the errors are small, so the predicted answer will always be within a percentage or two of the actual value.

Thanks to Bill Butler for the mathematical wizardry. 
 * */
//
// This is free software. You can redistribute it and/or modify it under the
// terms of the GNU General Public License version 2 as published by the Free
// Software Foundation.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NovaCommon
{
    // ===========================================================================
    // Class to hold environmental tolerance details
    // ===========================================================================

    [Serializable]
    public sealed class EnvironmentTolerance
    {
        public double Minimum = 0;
        public double Maximum = 0;

        public EnvironmentTolerance() { } // required for serialization
        public EnvironmentTolerance(double minv, double maxv)
        {
            Minimum = minv;
            Maximum = maxv;
        }

        // ============================================================================
        // Initialising Constructor from an xml node.
        // Precondition: node is a "EnvironmentTolerance" node in a Nova compenent definition file (xml document).
        // ============================================================================
        public EnvironmentTolerance(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;
            while (subnode != null)
            {
                try
                {
                    switch (subnode.Name.ToLower())
                    {
                        case "min":
                            Minimum = double.Parse(((XmlText)subnode.FirstChild).Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "max":
                            Maximum = double.Parse(((XmlText)subnode.FirstChild).Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                    }
                }
                catch
                {
                    // ignore incomplete or unset values
                }

                subnode = subnode.NextSibling;
            }

        }

        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelEnvironmentTolerance = xmldoc.CreateElement("EnvironmentTolerance");

            Global.SaveData(xmldoc, xmlelEnvironmentTolerance, "Min", Minimum.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelEnvironmentTolerance, "Max", Maximum.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return xmlelEnvironmentTolerance;
        }
    }


}