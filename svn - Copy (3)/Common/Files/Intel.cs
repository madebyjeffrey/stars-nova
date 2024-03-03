#region Copyright Notice
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

#region Module Description
// ===========================================================================
// This module contains the data that is generated by the Nova Console to
// generate a turn (including the very first one). This is the Intel sent to 
// the player.
// ===========================================================================
#endregion
// ============================================================================
// Manipulation of the turn data that is created by the Nova Console and read
// by the Nova GUI.
// ============================================================================

namespace Nova.Common
{
    #region Using Statements

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Xml;
    using System.Xml.Serialization;

    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using NUnit.Framework;

    #endregion

    /// <summary>
    /// The data items created by the Nova Console and read by the Nova GUI.
    /// </summary>
    [Serializable]
    public sealed class Intel
    {
        public EmpireData EmpireState = new EmpireData(true,"Intel");
        
        public List<Message> Messages = new List<Message>();
        
        public List<ScoreRecord> AllScores = new List<ScoreRecord>();
        
        public Dictionary<long, Minefield> AllMinefields = new Dictionary<long, Minefield>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Intel()
        {
        }

        /// <summary>
        /// Reset all data structures.
        /// </summary>
        public void Clear()
        {
            AllMinefields.Clear();
            Messages.Clear();
            EmpireState.Clear();
        }

        #region To From Xml

        /// <summary>
        /// Load <see cref="Intel">Intel</see> from an xml document.
        /// </summary>
        /// <param name="xmldoc">Produced using XmlDocument.Load(filename).</param>
        public Intel(XmlDocument xmldoc,String RaceHint)
        {
            XmlNode xmlnode = xmldoc.DocumentElement;
            LoadFromXmlNode(xmlnode, RaceHint);
        }
        
        /// <summary>
        /// Load <see cref="Intel">Intel</see> from an xml node.
        /// </summary>
        public void LoadFromXmlNode(XmlNode xmlnode, String RaceName)
        {
            while (xmlnode != null)
            {
                try
                {
                    switch (xmlnode.Name.ToLower())
                    {
                        case "root":
                            xmlnode = xmlnode.FirstChild;
                            continue;
                        case "intel":
                            xmlnode = xmlnode.FirstChild;
                            continue;

                        case "empiredata":
                            EmpireState = new EmpireData(xmlnode, RaceName);
                            break;

                        case "message":
                            Message message = new Message(xmlnode);
                            Messages.Add(message);
                            break;

                        case "scorerecord":
                            ScoreRecord newScore = new ScoreRecord(xmlnode);
                            if (EmpireState.EmpireReports.ContainsKey((ushort)newScore.EmpireId)) newScore.EmpireIcon = EmpireState.EmpireReports[(ushort)(newScore.EmpireId)].Icon.Image;
                            else newScore.EmpireIcon = EmpireState.Race.Icon.Image;
                            AllScores.Add(newScore);

                            break;

                        case "minefield":
                            Minefield minefield = new Minefield(xmlnode);
                            AllMinefields.Add(minefield.Key, minefield);
                            break;

                        default: break;
                    }
                }
                catch (Exception e)
                {
                    Report.FatalError(e.Message + "\n Details: \n" + e);
                }

                xmlnode = xmlnode.NextSibling;
            }    
        }

        /// <summary>
        /// Save: Serialize this object to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Intel.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            // create the outer element
            XmlElement xmlelIntel = xmldoc.CreateElement("Intel");
            
            xmlelIntel.AppendChild(EmpireState.ToXml(xmldoc));

            // Messages 
            if (Messages.Count > 0)
            {
                foreach (Message message in Messages)
                {
                    xmlelIntel.AppendChild(message.ToXml(xmldoc));
                }
            }

            // AllScores 
            foreach (ScoreRecord score in AllScores)
            {
                xmlelIntel.AppendChild(score.ToXml(xmldoc));
            }

            // AllMinefields
            foreach (Minefield mine in AllMinefields.Values)
            {
                xmlelIntel.AppendChild(mine.ToXml(xmldoc));
            }

            // return the outer element
            return xmlelIntel;
        }

        #endregion
    }
}