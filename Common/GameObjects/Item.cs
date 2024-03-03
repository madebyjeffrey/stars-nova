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
    using System.Drawing;
    using System.Runtime.Versioning;
    using System.Xml;

    using Nova.Common.DataStructures;

    /// <summary>
    /// Base class for most game items. 
    /// </summary>
    [Serializable]
    [SupportedOSPlatform("windows")]
    public class Item
    {
        /// <summary>
        /// Backing store for the game wide unique key. 
        /// First bit is for sign. Negative values are reserved for special flags.
        /// Bits 2-24 are reserved.
        /// Bits 25-32 are for the empire.Id aka Owner.
        /// Bits 33-64 are the Item.Id, which is a number generated by the client and unique for objects owned by that empire.
        /// Bit map is:
        /// S-----------------------OOOOOOOOIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII
        /// IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII
        /// ^                  ^                ^            ^
        /// +-- sign bit       +-- reserved     +-- owner    +-- client generated Id.
        /// </summary>
        private long key = Global.Nobody; // Default to no-id and no owner.
        
        /// <summary>
        /// The name of the derived item, for example the name of a star.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the derived item (e.g. "Ship", "Star", "Starbase", etc).
        /// </summary>
        public ItemType Type = ItemType.None;
        
        /// <summary>
        /// Property for accessing the game wide unique key.
        /// </summary>
        public long Key
        {
            get
            {
                return key;
            }
            set
            {
                if (value < 0) 
                { 
                    throw new ArgumentException("OwnerId out of range"); 
                }
                key = value;
            }
        }

        /// <summary>
        /// Property for accessing the object's owning empire id, stored as bits 25-32 of the key.
        /// Range is 1 to 255, with zero being reserved for no/any empire.
        /// </summary>
        public ushort Owner
        {
            get
            {
                return key.Owner();
            }

            set
            {
                key = key.SetOwner(value);
            }
        }

        /// <summary>
        /// Property for accessing the objects owner specific Id (e.g., for use in generating the default name). 
        /// Range is 1 - 0xFFFFFFFF, with 0 reserved for undefined Id.
        /// </summary>
        public uint Id
        {
            get
            {
                return key.Id();
            }

            set
            {
                key = key.SetId(value);
            }
        }

        /// <summary>
        /// Default Construction.
        /// </summary>
        public Item() 
        { 
        }

        public Item(long key)
        {
            Key = key;
        }

        /// <summary>
        /// Copy (initializing) constructor.
        /// </summary>
        /// <param name="existing">An existing <see cref="Item"/>.</param>
        public Item(Item copy)
        {
            if (copy == null)
            {
                return;
            }

            Name = copy.Name;
            Owner = copy.Owner;
            Type = copy.Type;
        }
        
        /// <summary>
        /// Load: initializing constructor from an XmlNode representing the Item (from a save file).
        /// </summary>
        /// <param name="node">An XmlNode representing the Item.</param>
        public Item(XmlNode node)
        {
            if (node == null)
            {
                Report.FatalError("Item.cs: Item(XmlNode node) - node is null - no Item found.");
                return;
            }

            XmlNode itemNode = null;
            
            // Search for the first Item node in this Xml representation.
            while (node != null)
            {
                if ((itemNode = node.SelectSingleNode("Item")) != null)
                {
                    break;
                }
                
                node = node.FirstChild;
            }
            
            if (itemNode == null)
            {
                Report.FatalError("Item.cs: Item(XmlNode node) - could not find Item node, input file may be corrupt.");
                return;
            }

            XmlNode mainNode = itemNode.FirstChild;

            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "key":
                            key = long.Parse(mainNode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
                            break;
                        case "name":
                            Name = mainNode.FirstChild.Value;
                            break;
                        case "type":
                            Type = (ItemType)Enum.Parse(typeof(ItemType), mainNode.FirstChild.Value);
                            break;
                    }
                }

                catch (Exception e)
                {
                    Report.Error(e.Message + " \n Details: \n " + e.ToString());
                }

                mainNode = mainNode.NextSibling;
            }
        }
                       
        
        /// <summary>
        /// Save: Return an XmlElement representation of the <see cref="Item"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Property.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelItem = xmldoc.CreateElement("Item");

            Global.SaveData(xmldoc, xmlelItem, "Key", Key.ToString("X"));
            
            if (Name != null)
            {
                Global.SaveData(xmldoc, xmlelItem, "Name", Name);
            }
            
            Global.SaveData(xmldoc, xmlelItem, "Type", Type.ToString());

            return xmlelItem;
        }
    }
}
