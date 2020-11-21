﻿ #region Copyright Notice
 // ============================================================================
 // Copyright (C) 2012 The Stars-Nova Project
 //
 // This file is part of Stars-Nova.
 // See <http://sourceforge.net/projects/stars-nova/>;.
 //
 // This program is free software; you can redistribute it and/or modify
 // it under the terms of the GNU General Public License version 2 as
 // published by the Free Software Foundation.
 //
 // This program is distributed in the hope that it will be useful,
 // but WITHOUT ANY WARRANTY; without even the implied warranty of
 // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 // GNU General Public License for more details.
 //
 // You should have received a copy of the GNU General Public License
 // along with this program. If not, see <http://www.gnu.org/licenses/>;
 // ===========================================================================
 #endregion
 
namespace Nova.Common.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    
    using Nova.Common.Components;
    
    /// <summary>
    /// Description of WaypointCommand.
    /// </summary>
    public class DesignCommand : ICommand
    {        
        public ShipDesign Design
        {
            private set;
            get;
        }
        
        public CommandMode Mode
        {
            private set;
            get;
        }
        
        
        // Create a blank design command.
        public DesignCommand()
        {
            Design = new ShipDesign(Global.None);
            Mode = CommandMode.Add;
        }
        
        /// <summary>
        /// Creates a design command with a design key. Useful to delete designs without
        /// bloating the orders file when all that is needed is the numeric Key instead of
        /// the full design.
        /// </summary>
        public DesignCommand(CommandMode mode, long designKey)
        {
            Design = new ShipDesign(designKey);
            Mode = mode;
        }
        

        /// <summary>
        /// Creates a design command by providing a full design object. Use when adding or
        /// modifying designs.
        /// </summary>
        public DesignCommand(CommandMode mode, ShipDesign design)
        {
            Design = design;
            Mode = mode;
        }
                
        
        /// <summary>
        /// Load from XML: Initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> within
        /// a Nova component definition file (xml document).
        /// </param>
        public DesignCommand(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            
            while (mainNode != null)
            {
                switch (mainNode.Name.ToLower())
                {
                    case "mode":
                        Mode = (CommandMode)Enum.Parse(typeof(CommandMode), mainNode.FirstChild.Value);
                        break;                   
                    
                    case "design":
                        Design = new ShipDesign(mainNode);
                        Design.Update();
                        break;
                    
                    case "shipdesign":
                        Design = new ShipDesign(mainNode);
                        Design.Update();
                        break;

                    case "key": // occurs if CommandMode is Delete
                        Design = new ShipDesign(long.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture));
                        break;
                }
            
                mainNode = mainNode.NextSibling;
            }   
        }
        
        
        
        public bool IsValid(EmpireData empire, out Message message)
        {           
            switch (Mode)
            {
                case CommandMode.Add:
                    if (empire.Designs.ContainsKey(Design.Key))
                    {
                        message = new Message(empire.Id, "Cant re-add same design:" + Design.Name, "Invalid Command", null);
                        return false;
                    }
                break;
                case CommandMode.Delete: // Both cases check for existing design before editing/deleting.
                    if (!empire.Designs.ContainsKey(Design.Key))
                    {
                        message = new Message(empire.Id, "Cant re-add same design:" + Design.Name, "Invalid Command", null);
                        return false;
                    }
                break;
                case CommandMode.Edit:
                    {
                        if (Global.Debug) message = new Message(empire.Id,  Design.Name + " obsolete status set to " + (!Design.Obsolete).ToString(), (!Design.Obsolete).ToString(),null);
                        return true;
                    }
            }

            message = new Message(empire.Id, "Design:" + Design.Name + " added", "Invalid Command", null);
            return true;
        }
        
        
        
        public Message ApplyToState(EmpireData empire)
        {
            switch (Mode)
            {
                case CommandMode.Add:
                    Design.Update();
                    empire.Designs.Add(Design.Key, Design);
                    return null;
                case CommandMode.Delete:
                    empire.Designs.Remove(Design.Key);                
                    UpdateFleetCompositions(empire);
                    return null;
                case CommandMode.Edit:
                    ShipDesign oldDesign = new ShipDesign(Design.Key);
                    empire.Designs.TryGetValue(Design.Key,out oldDesign);
                    oldDesign.Update();                    
                    empire.Designs.Remove(Design.Key);
                    oldDesign.Obsolete = !oldDesign.Obsolete;
                    empire.Designs.Add(Design.Key, oldDesign);
                    return null;

            }
                    return null;
        }
        
        
        /// <summary>
        /// Handle destroying ships of the deleted/edited design.
        /// </summary>
        private void UpdateFleetCompositions(EmpireData empire)
        {
            // Note that we are not allowed to delete the ships or fleets on the
            // iteration as that is not allowed (it
            // destroys the validity of the iterator). Consequently we identify
            // anything that needs deleting and remove them separately from their
            // identification.
            List<Fleet> fleetsToRemove = new List<Fleet>();
            
            foreach (Fleet fleet in empire.OwnedFleets.Values)
            {
                List<ShipToken> tokensToRemove = new List<ShipToken>();
    
                foreach (ShipToken token in fleet.Composition.Values)
                {
                    if (token.Design.Key == Design.Key)
                    {
                        tokensToRemove.Add(token);
                    }
                }
    
                foreach (ShipToken token in tokensToRemove)
                {
                    fleet.Composition.Remove(token.Design.Key);
                }
    
                if (fleet.Composition.Count == 0)
                {
                    fleetsToRemove.Add(fleet);
                }
            }
    
            foreach (Fleet fleet in fleetsToRemove)
            {
                empire.OwnedFleets.Remove(fleet.Key);
                empire.FleetReports.Remove(fleet.Key);
            }
        }
        
        
        /// <summary>
        /// Save: Serialize this property to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Property.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelCom = xmldoc.CreateElement("Command");
            xmlelCom.SetAttribute("Type", "Design");
            Global.SaveData(xmldoc, xmlelCom, "Mode", Mode.ToString());
            if ((Mode != CommandMode.Delete) && (Mode != CommandMode.Edit))
            {
                // serialise a normal design
                xmlelCom.AppendChild(Design.ToXml(xmldoc));
            }
            else
            {
                // For CommandMode.Delete and edit the design only contains a valid Tag
                Global.SaveData(xmldoc, xmlelCom, "Key", Design.Key);
            }
            
            return xmlelCom;    
        }
    }
}
