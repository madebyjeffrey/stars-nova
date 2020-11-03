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

#region Module Description
// ===========================================================================
// Definition of a battle plan.
// ===========================================================================
#endregion

namespace Nova.Common
{
    #region Using Statements
    using System;
    using System.Xml;
    #endregion

    [Serializable]
    public class BattlePlan
    {
        // FIXME:(priority 2) This should all be enums!
        public string Name            = "Default";
        public int PrimaryTarget = 0;
        public int SecondaryTarget = 1;
        public int TertiaryTarget = 3;
        public int QuaternaryTarget = 5;
        public int QuinaryTarget = 6;

        public string Tactic          = "Maximise Damage";
        public string Attack          = "Enemies";
        public int TargetId;

        #region Construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BattlePlan() 
        { 
        }

        #endregion

        #region Load Save Xml

        /// <summary>
        /// Load: initializing constructor from an XmlNode.
        /// </summary>
        /// <param name="node">An XmlNode representing a BattlePlan.</param>
        public BattlePlan(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;
            while (subnode != null)
            {
                try
                {
                    switch (subnode.Name.ToLower())
                    {
                        case "name":
                            Name = subnode.FirstChild.Value;
                            break;
                        case "primarytarget":
                            PrimaryTarget = int.Parse(subnode.FirstChild.Value);
                            break;
                        case "secondarytarget":
                            SecondaryTarget = int.Parse(subnode.FirstChild.Value);
                            break;
                        case "tertiarytarget":
                            TertiaryTarget = int.Parse(subnode.FirstChild.Value);
                            break;
                        case "quaternarytarget":
                            QuaternaryTarget = int.Parse(subnode.FirstChild.Value);
                            break;
                        case "quinarytarget":
                            QuinaryTarget = int.Parse(subnode.FirstChild.Value);
                            break;
                        case "tactic":
                            Tactic = subnode.FirstChild.Value;
                            break;
                        case "attack":
                            Attack = subnode.FirstChild.Value;
                            break;
                        case "targetid":
                            TargetId = int.Parse(subnode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
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

        /// <summary>
        /// Save: Generate an XmlElement representation of a battle plan for saving.
        /// </summary>
        /// <param name="xmldoc">The parent XmlDocument.</param>
        /// <returns>An XmlElement representation of the BattlePlan.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelBattlePlan = xmldoc.CreateElement("BattlePlan");

            Global.SaveData(xmldoc, xmlelBattlePlan, "Name", Name);
            Global.SaveData(xmldoc, xmlelBattlePlan, "PrimaryTarget", ((int)PrimaryTarget).ToString());
            Global.SaveData(xmldoc, xmlelBattlePlan, "SecondaryTarget", ((int)SecondaryTarget).ToString());
            Global.SaveData(xmldoc, xmlelBattlePlan, "TertiaryTarget", ((int)TertiaryTarget).ToString());
            Global.SaveData(xmldoc, xmlelBattlePlan, "QuaternaryTarget", ((int)QuaternaryTarget).ToString());
            Global.SaveData(xmldoc, xmlelBattlePlan, "QuinaryTarget", ((int)QuinaryTarget).ToString());
            Global.SaveData(xmldoc, xmlelBattlePlan, "Tactic", Tactic);
            Global.SaveData(xmldoc, xmlelBattlePlan, "Attack", Attack);
            Global.SaveData(xmldoc, xmlelBattlePlan, "TargetId", TargetId.ToString("X"));

            return xmlelBattlePlan;
        }

        #endregion
    }
}
