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

namespace Nova.Common.Waypoints
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    
    using Nova.Common;
    using Nova.Common.Components;
    
    /// <summary>
    /// Performs split or merge of fleets.
    /// </summary>
    public class SplitMergeTask : IWaypointTask
    {           
        private List<Message> messages = new List<Message>();
        
        /// <inheritdoc />
        public List<Message> Messages
        {
            get { return messages; }
        }
        
        /// <inheritdoc />
        public string Name
        {
            get 
            {
                if (OtherFleetKey == 0)
                {
                    return "Split Fleet";
                }
                else
                {
                    return "Merge Fleet";
                }
            }
        }
        
        /// <summary>
        /// Composition of the fleet on the left side of the SplitFleetsDialog dialog.
        /// </summary>
        public Dictionary<long, ShipToken> LeftComposition { get; set; }
        
        /// <summary>
        /// Composition of the fleet on the right side of the SplitFleetsDialog dialog.
        /// </summary>
        public Dictionary<long, ShipToken> RightComposition { get; set; }
        
        /// <summary>
        /// The Fleet.Key of the target fleet to merge with, if any.
        /// </summary>
        /// <remarks>
        /// The "this" fleet is the fleet this waypoint task belongs to.
        /// </remarks>
        public long OtherFleetKey { get; set; }
        public bool SimpleMerge { get; set; }
        public bool EqualCargoDistribution { get; set; }
        public bool RemoveLeftFleet { get; set; }

        /// <summary>
        ///  Default Constructor
        /// </summary>
        /// <param name="leftComposition"></param>  // for complex splitmerge this will be the new source Fleet composition, for split this will be the source composition after the spli
        /// <param name="rightComposition"></param> // for complex splitmerge this will be the new "other" Fleet composition
        /// <param name="otherFleetKey"></param>    // required (may be peekNewFleetKey())
        /// <param name="simpleMerge"></param>      // true if just putting every ship into one fleet
        /// <param name="equalCargoDist"></param>   // true if every ship gets an equal amount of the cargo
        /// <param name="removeLeft"></param>       // true for simple merge where every ship ends up in the "other" fleet
        public SplitMergeTask(Dictionary<long, ShipToken> leftComposition, Dictionary<long, ShipToken> rightComposition, long otherFleetKey = 0, bool simpleMerge = false, bool equalCargoDist = false, bool removeLeft = false)
        {
            LeftComposition = leftComposition;            
            RightComposition = rightComposition;
            OtherFleetKey = otherFleetKey;
            SimpleMerge = simpleMerge;
            EqualCargoDistribution = equalCargoDist;
        }
        

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="other">SplitTask to copy.</param>
        public SplitMergeTask(SplitMergeTask copy)
        {
            LeftComposition = new Dictionary<long, ShipToken>(copy.LeftComposition);            
            RightComposition = new Dictionary<long, ShipToken>(copy.RightComposition);
            OtherFleetKey = copy.OtherFleetKey;
            SimpleMerge = copy.SimpleMerge;
            EqualCargoDistribution = copy.EqualCargoDistribution;
        }


        /// <summary>
        /// Load: Read an object of this class from and XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a representation of this object.</param>
        public SplitMergeTask(XmlNode node)
        {
            if (node == null)
            {
                return;
            }
            
            LeftComposition = new Dictionary<long, ShipToken>();
            RightComposition = new Dictionary<long, ShipToken>();
            
            XmlNode mainNode = node.FirstChild;
            XmlNode subNode;
            ShipToken token;
            SimpleMerge = false;
            EqualCargoDistribution = false;
            RemoveLeftFleet = false;
            while (mainNode != null)
            {
                try
                {
                    subNode = mainNode.FirstChild;
                    
                    switch (mainNode.Name.ToLower())
                    {
                        case "simplemerge":
                            SimpleMerge = bool.Parse(subNode.Value);
                            break;
                        case "removeleft":
                            RemoveLeftFleet = bool.Parse(subNode.Value);
                            break;
                        case "equaldist":
                            EqualCargoDistribution = bool.Parse(subNode.Value);
                            break;
                        case "rightkey":
                            OtherFleetKey = long.Parse(subNode.Value, System.Globalization.NumberStyles.HexNumber);
                            break;

                        case "leftcomposition":
                            while (subNode != null)
                            {
                                token = new ShipToken(subNode);
                                LeftComposition.Add(token.Key, token);
                                subNode = subNode.NextSibling;
                            }
                            break;
                            
                        case "rightcomposition":
                            while (subNode != null)
                            {
                                token = new ShipToken(subNode);
                                RightComposition.Add(token.Key, token);
                                subNode = subNode.NextSibling;
                            }
                            break;                             
                    }
                }
                catch (Exception e)
                {
                    Report.Error(e.Message);
                }
                mainNode = mainNode.NextSibling;
            }
        }
        
        
        /// <inheritdoc />
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelTask = xmldoc.CreateElement("SplitMergeTask");

            // We are either merging or splitting or combining two fleets into 2 fleets of different compositions.
            // For a merge we need two keys (one comes from this waypoint's owner)
            // and for a split, one key and 2 compositions. 
            // and for a splitmerge we need everything 
            if (SimpleMerge) Global.SaveData(xmldoc, xmlelTask, "SimpleMerge", SimpleMerge.ToString());
            if (EqualCargoDistribution) Global.SaveData(xmldoc, xmlelTask, "EqualDist", EqualCargoDistribution.ToString());
            if (RemoveLeftFleet) Global.SaveData(xmldoc, xmlelTask, "RemoveLeft", RemoveLeftFleet.ToString());
            Global.SaveData(xmldoc, xmlelTask, "RightKey", OtherFleetKey.ToString("X"));
            if (!SimpleMerge)
            {            
                XmlElement xmlelLeft = xmldoc.CreateElement("LeftComposition");
                foreach (ShipToken token in LeftComposition.Values)
                {
                    xmlelLeft.AppendChild(token.ToXml(xmldoc));
                }            
                xmlelTask.AppendChild(xmlelLeft);
                
                XmlElement xmlelRight = xmldoc.CreateElement("RightComposition");
                foreach (ShipToken token in RightComposition.Values)
                {
                    xmlelRight.AppendChild(token.ToXml(xmldoc));
                }            
                xmlelTask.AppendChild(xmlelRight);
            }
            
            return xmlelTask;
        }

        
        /// <inheritdoc />
        public bool IsValid(Fleet fleet, Item target, EmpireData sender, EmpireData receiver, out Message message)
        {
            // fleet is the original fleet to split, or the recipient of a merge.
            // target (as Fleet) is the new fleet in a split, or the fleet to be merged into FirstFleet.           

            // TODO (priority 5) - Validate SplitMergeTask:
            // fleet & target valid vs sender versions
            // FirstFleet cargo + SecondFleet cargo = original cargo from either fleet + target or senders's fleets.
            message = null;
            return true;
        }
        
        
        /// <summary>
        /// Performs three functions 
        /// 1/ Move one or more tokens from Fleet A to Fleet B
        /// 2/ Merge two fleets together
        /// 3/ split one fleet into two
        /// </summary>
        /// <param name="fleet"></param> When splitting a fleet this is the composition of this Fleet after the split 
        /// <param name="target"></param> When splitting this is ignored 
        /// <param name="sender"></param> Sender empire
        /// <param name="receiver"></param> receiver empire (to allow expansion where fleet exchange with another empire is allowed)
        /// <param name="message"></param> 
        /// <returns></returns>
        public bool Perform(Fleet fleet, Item target, EmpireData sender, EmpireData receiver,out Message message)
        { 
            Fleet secondFleet = null;
            
            // Look for an appropriate fleet for a merge.
            if ((OtherFleetKey != 0) && (OtherFleetKey != Global.Unset))
            {
                // This allows to merge with other empires if desired at some point.
                if (receiver != null && receiver.OwnedFleets.ContainsKey(OtherFleetKey))
                {
                    secondFleet = receiver.OwnedFleets[OtherFleetKey];
                }
                else if (sender.OwnedFleets.ContainsKey(OtherFleetKey)) 
                {
                    // The other fleet is also ours: OtherFleetKey belongs to the same Race/Player as the fleet with the SplitMergeTask waypoint order.
                    secondFleet = sender.OwnedFleets[OtherFleetKey];    
                }
            }
            
                   
            if (SimpleMerge)  //Simple merge where all fleets end up in the same Fleet
            {
                if (!RemoveLeftFleet) MergeFleets(fleet, secondFleet);
                else MergeFleets(secondFleet, fleet);
            }
            else if (OtherFleetKey != Global.Unset)          // It's a complex swap where tokens come from 2 fleets and we end up with 2 Fleets
            {
                ReassignShips(fleet,secondFleet , sender.PeekFleetKey());
            }
            else  //(OtherFleetKey == Global.Unset)         // Else it's a split. Need a new fleet so clone original and change stuff.
            {
                secondFleet = sender.MakeNewFleet(fleet);
                ReassignShips(fleet, secondFleet,sender.PeekFleetKey());
                // Now send new Fleets to limbo pending inclusion.
                sender.TemporaryFleets.Add(secondFleet);                
            }
            message = null;
            return true;            
        }


        /// <summary>
        /// Used to merge fleets. Much cruder than ReassignShips, but requires
        /// less information.
        /// </summary>
        /// <remarks>
        /// This is a simple merge where all the ships end up in one fleet.
        /// All ships end up in the left fleet.
        /// </remarks>
        /// <param name="left">The composition of ships on the left side of the SplitFleetsDialog dialog.</param>
        /// <param name="right">The composition of ships on the right side of the SplitFleetsDialog dialog.</param>
        private void MergeFleets(Fleet left, Fleet right)
        {
            foreach (ShipToken token in right.Composition.Values)
            {
                if (!left.Composition.ContainsKey(token.Key))
                {
                    left.Composition.Add(token.Key, token);
                }
                else
                {
                    left.Composition[token.Key].Quantity += token.Quantity;
                    left.Composition[token.Key].Armor += token.Armor;
                }
            }

            left.FuelAvailable += right.FuelAvailable;
            left.Cargo.Add(right.Cargo);
            right.Composition.Clear();
        }


        /// <summary>
        /// Reassign Fleet compositions and Cargo. 
        /// </summary>
        /// <param name="left">Original Fleet.</param>
        /// <param name="right">New Fleet.</param>
        private void ReassignShips(Fleet left, Fleet right,long fleetNumber)
        {
            long mostVessels = 0;
            String designName = "";
            List<long> allKeys = new List<long>();
            allKeys.AddRange(LeftComposition.Keys);
            foreach (long newKey in RightComposition.Keys) if (!allKeys.Contains(newKey)) allKeys.Add(newKey);

            foreach (long key in allKeys)
            {
                int leftNewCount = LeftComposition.ContainsKey(key) ? LeftComposition[key].Quantity : 0;
                int leftOldCount = left.Composition.ContainsKey(key) ? left.Composition[key].Quantity : 0;

                if (leftNewCount == leftOldCount)
                {
                    continue; // no moves of this design
                }

                Fleet from;
                Fleet to;
                int moveCount;
                
                if (leftNewCount > leftOldCount)
                {
                    from = right;
                    to = left;
                    moveCount = leftNewCount - leftOldCount;
                }
                else
                {
                    from = left;
                    to = right;
                    moveCount = leftOldCount - leftNewCount;
                    if (moveCount > mostVessels)
                    { 
                        mostVessels = moveCount;
                        designName = left.Composition[key].Design.Name;
                        right.Name = designName + " #" + ((int) fleetNumber).ToString();  //Name of new fleet is taken from the design of the most populous vessel in the fleet

                    }
                }
                
                if (!to.Composition.ContainsKey(key))
                {
                    to.Composition.Add(key, new ShipToken(from.Composition[key].Design, 0));
                }
                
                to.Composition[key].Quantity += moveCount;
                from.Composition[key].Quantity -= moveCount;
                
                if (from.Composition[key].Quantity == 0)
                {
                    from.Composition.Remove(key);
                }
                
                ReassignCargo(left, right);
            }            
        }
      
        
        /// <summary>
        /// Used to redistribute cargo (including colonists) and fuel once fleets are split.
        /// </summary>
        private void ReassignCargo(Fleet left, Fleet right)
        {
            // Ships are moved. Now to reassign fuel/cargo
            int ktToMove = 0;
            Cargo fromCargo = left.Cargo;
            Cargo toCargo = right.Cargo;
            if (left.Cargo.Mass > left.TotalCargoCapacity)
            {
                fromCargo = left.Cargo;
                toCargo = right.Cargo;
                ktToMove = left.Cargo.Mass - left.TotalCargoCapacity;
            }
            else if (right.Cargo.Mass > right.TotalCargoCapacity)
            {
                fromCargo = right.Cargo;
                toCargo = left.Cargo;
                ktToMove = right.Cargo.Mass - right.TotalCargoCapacity;
            }

            double proportion = (double)ktToMove / fromCargo.Mass;
            if (ktToMove > 0)
            {
                // Try and move cargo
                int ironToMove = (int)Math.Ceiling(fromCargo.Ironium * proportion);
                if (ironToMove > ktToMove)
                {
                    ironToMove = ktToMove;
                }
                toCargo.Ironium += ironToMove;
                fromCargo.Ironium -= ironToMove;
                ktToMove -= ironToMove;
            }
            if (ktToMove > 0)
            {
                // Try and move cargo
                int borToMove = (int)Math.Ceiling(fromCargo.Boranium * proportion);
                if (borToMove > ktToMove)
                {
                    borToMove = ktToMove;
                }
                toCargo.Boranium += borToMove;
                fromCargo.Boranium -= borToMove;
                ktToMove -= borToMove;
            }
            if (ktToMove > 0)
            {
                // Try and move cargo
                int germToMove = (int)Math.Ceiling(fromCargo.Germanium * proportion);
                if (germToMove > ktToMove)
                {
                    germToMove = ktToMove;
                }
                toCargo.Germanium += germToMove;
                fromCargo.Germanium -= germToMove;
                ktToMove -= germToMove;
            }
            if (ktToMove > 0)
            {
                // Try and move cargo
                int colonistsToMoveInKilotons = (int)Math.Ceiling(fromCargo.ColonistsInKilotons * proportion);
                if (colonistsToMoveInKilotons > ktToMove)
                {
                    colonistsToMoveInKilotons = ktToMove;
                }
                toCargo.ColonistsInKilotons += colonistsToMoveInKilotons;
                fromCargo.ColonistsInKilotons -= colonistsToMoveInKilotons;
                ktToMove -= colonistsToMoveInKilotons;
            }
            Debug.Assert(ktToMove == 0, "Must not be negative.");

            // fuel is split in proportion to fuel tank capacity in Stars!
            if (left.FuelAvailable > left.TotalFuelCapacity)
            {
                // Move excess to right and set left to max
                right.FuelAvailable += left.FuelAvailable - left.TotalFuelCapacity;
                left.FuelAvailable = left.TotalFuelCapacity;
            }
            else if (right.FuelAvailable > right.TotalFuelCapacity)
            {
                // Move excess to left and set right to max
                left.FuelAvailable += right.FuelAvailable - right.TotalFuelCapacity;
                right.FuelAvailable = right.TotalFuelCapacity;
            }
        }
    }
}
