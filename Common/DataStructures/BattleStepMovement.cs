﻿#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010, 2011 The Stars-Nova Project
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

using System;
using System.Xml;

namespace Nova.Common.DataStructures
{
    /// ----------------------------------------------------------------------------
    /// <summary>
    /// A class to record a new stack position.
    /// </summary>
    /// ----------------------------------------------------------------------------
    [Serializable]
    public class BattleStepMovement : BattleStep
    {
        
        public string StackName = null;
        public NovaPoint Position = new NovaPoint();

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public BattleStepMovement()
        {
            Type = "Movement";
        }

        #region Xml


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Load: Initialising Constructor from an xml node.
        /// </summary>
        /// <param name="node">A <see cref="BattleStepTarget"/> XmlNode from a Nova save file (xml document). </param>
        /// ----------------------------------------------------------------------------
        public BattleStepMovement(XmlNode node)
            : base(node)
        {
            XmlNode subnode = node.FirstChild;
            while (subnode != null)
            {
                try
                {
                    switch (subnode.Name.ToLower())
                    {

                        case "stackname":
                            StackName = subnode.FirstChild.Value;
                            break;

                        case "point":
                            Position = new NovaPoint(subnode);
                            break;

                    }
                }
                catch (Exception e)
                {
                    Report.Error("Error loading Battle Step - Movement : " + e.Message);
                }
                subnode = subnode.NextSibling;
            }         
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Generate an XmlElement representation of the xmlelBattleStepMovement for saving to file.
        /// </summary>
        /// <param name="xmldoc">The parent XmlDocument</param>
        /// <returns>An XmlElement representing the xmlelBattleStepMovement</returns>
        /// ----------------------------------------------------------------------------
        public new XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelBattleStepMovement = xmldoc.CreateElement("BattleStepMovement");

            xmlelBattleStepMovement.AppendChild(base.ToXml(xmldoc));
            Global.SaveData(xmldoc, xmlelBattleStepMovement, "StackName", StackName);
            xmlelBattleStepMovement.AppendChild(Position.ToXml(xmldoc));
            return xmlelBattleStepMovement;
        }

        #endregion
    }
}