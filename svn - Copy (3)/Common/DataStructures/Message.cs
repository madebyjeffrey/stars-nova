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

namespace Nova.Common
{
    using System;
    using System.Xml;
    using Nova.Common.DataStructures;

    /// <summary>
    /// This class defines the format of messages sent to one or more players.
    /// </summary>
    [Serializable]
    public class Message
    {
        public string Text;      // The text to display in the message box.
        public int Audience;     // An int representing the destination of the message. 0 means everyone. 
        public string Type;      // Text that indicates the type of event that generated the message.
        public object Event;     // An object used with the Goto button to display more information to the player. See Messages.GotoButton_Click
        public long FleetKey;     // Required for messages of type "Fuel"
        // Ensure when adding new message types to add code to the Xml functions to handle your object type.
        public string EventString;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message() 
        { 
        }

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="audience">A string representing the destination of the message. Either a race name or and asterisk.</param>
        /// <param name="messageEvent">An object used with the Goto button to display more information to the player. See Messages.GotoButton_Click.</param>
        /// <param name="text">The text to display in the message box.</param>
        public Message(int audience, string text, string messageType, object messageEvent,long fleetKey = 0)
        {
            Audience = audience;
            Text     = text;
            Type     = messageType;
            Event    = messageEvent;
            FleetKey = fleetKey;
        }

        /// <summary>
        /// Load: initializing constructor to read in a Star from an XmlNode (from a saved file).
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> representing a Star.
        /// </param>
        public Message(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;

            // Read the node
            while (subnode != null)
            {
                try
                {
                    switch (subnode.Name.ToLower())
                    {
                        case "text":
                            if (subnode.FirstChild != null)
                            {
                                Text = subnode.FirstChild.Value;
                            }
                            break;
                        case "audience":
                            if (subnode.FirstChild != null)
                            {
                                Audience = int.Parse(subnode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
                            }
                            break;
                        case "type":
                            if (subnode.FirstChild != null)
                            {
                                Type = subnode.FirstChild.Value;
                            }
                            break;
                        case "fleetkey":
                            if (subnode.FirstChild != null)
                            {
                                FleetKey = long.Parse(subnode.FirstChild.Value);
                            }
                            break;
                        case "event":
                            EventString = (string)subnode.FirstChild.Value;
                            Event = (object)subnode.FirstChild.Value; // this is only true for BattleReports but we need the BattleReport key put in before LinkIntelReferences()
                            break;

                        default: break;
                    }
                }
                catch (Exception e)
                {
                    Report.FatalError(e.Message + "\n Details: \n" + e.ToString());
                }
                subnode = subnode.NextSibling;
            }
        }

        /// <summary>
        /// Save: Serialize this Message to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Message.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelMessage = xmldoc.CreateElement("Message");
            if (Text != null)
            {
                Global.SaveData(xmldoc, xmlelMessage, "Text", Text);
            }

            Global.SaveData(xmldoc, xmlelMessage, "Audience", Audience.ToString("X"));

            if (Type != null)
            {
                Global.SaveData(xmldoc, xmlelMessage, "Type", Type);
            }

            if (FleetKey != 0) // many messages involve a star and a fleet so allow both keys to be saved
            {
                Global.SaveData(xmldoc, xmlelMessage, "FleetKey", FleetKey);
            }

            if (Event != null)
            {
                switch (Type)
                {
                    case "TechAdvance":
                    case "NewComponent":
                    case "Invalid Command":
                        // No object reference required to be saved.
                        break;
                    case "Load/Unload":
                    case "WarpToChange":
                    case "DestToChange":
                    case "Cheap Engines":
                    case "Fuel":
                    case "Ship":            // Key is (long)fleetCounter | ((long)empireId << 32);
                    case "Fleet":           // "
                        if (Event != null)
                        {
                            if (Event is Fleet) Global.SaveData(xmldoc, xmlelMessage, "Event", (Event as Fleet).Key);   // 0000 0000 0000 0000 0000 00EE EEEE EEEE  FFFF FFFF FFFF FFFF FFFF FFFF FFFF FFFF
                            if (Event is FleetIntel) Global.SaveData(xmldoc, xmlelMessage, "Event", (Event as FleetIntel).Key);   // 0000 0000 0000 0000 0000 00EE EEEE EEEE  FFFF FFFF FFFF FFFF FFFF FFFF FFFF FFFF
                        }
                        break;  
                        // E = Empire (Bits 32-41)    F = fleet key  
                    case "New Minefield":
                    case "Increase Minefield":  // for minefields there is a unique address for each 5 x 5 square and laying mines anywhere in that 5 x 5 square add to the minefield at the address of that square.
                    case "Minefield": //(long)key = ((fleet.Position.X / Global.MineFieldSnapToGridSize) * 268,435,456 + (fleet.Position.Y / Global.MineFieldSnapToGridSize)) + minefield.Owner * 18,014,398,509,481,984;
                        if (Event != null) Global.SaveData(xmldoc, xmlelMessage, "Event", Event.ToString());   // EEEE EEEE EEYY YYYY YYYY YYYY YYYY YYYY YYYY TTXX XXXX XXXX XXXX XXXX XXXX XXXX 
                        break;                                                                      // E = Empire (Bits 54-63)     Y = Minefield.Position.Y (Bits 28-53)   T = Type of mine (Bits 26-27)   X = Minefield.Position.X (bits 0-25)

                    case "New Defense":
                    case "Terraform":
                    case "Factory":
                    case "Mine":
                    case "Star":
                    case "StarIntel": // "Key" is a string in the form "Star: Ssssssssssssssssssssssssssssssssss"
                        if (Event != null) Global.SaveData(xmldoc, xmlelMessage, "Event", Event.ToString() );
                        break;

                    case "BattleReport":
                        Global.SaveData(xmldoc, xmlelMessage, "Event", (Event as BattleReport).Key);
                        break;

                    default:
                        Report.Error("Message.ToXml() - Unable to convert Message.Event of type: (\"" + Type +"\"), Event = "+ Event.ToString()+". The message TEXT is : "+Text);
                        break;
                }
            }

            return xmlelMessage;
        }
    }
}