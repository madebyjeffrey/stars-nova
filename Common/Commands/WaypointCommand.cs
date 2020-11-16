 #region Copyright Notice
 // ============================================================================
 // Copyright (C) 2011-2012 The Stars-Nova Project
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
    
    using Nova.Common.Waypoints;
    
    /// <summary>
    /// Description of WaypointCommand.
    /// </summary>
    public class WaypointCommand : ICommand
    {        
        public Waypoint Waypoint
        {
            get;
            set;
        }
        
        public int Index
        {
            get;
            set;
        }
        
        public CommandMode Mode
        {
            get;
            set;
        }
        
        public long FleetKey
        {
            get;
            set;
        }
        private List<Message> messages = new List<Message>();

        public List<Message> Messages
        {
            get { return messages; }
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public WaypointCommand()
        {
            Waypoint = new Waypoint();
            Mode = CommandMode.Add;
            FleetKey = Global.None;
            Index = 0;
        }
        
        /// <summary>
        /// Deletion Constructor. Waypoint is null as it is not used.
        /// </summary>
        /// <param name="mode">CommandMode. Should be Delete.</param>
        /// <param name="fleetKey">Fleet key whose waypoints will be affected.</param>
        /// <param name="index">Waypoint Index to delete.</param>
        public WaypointCommand(CommandMode mode, long fleetKey = Global.None, int index = 0)
        {
            Waypoint = null;
            Mode = mode;
            FleetKey = fleetKey;
            Index = index;
        }
        
        /// <summary>
        /// Add/Edit Constructor.
        /// </summary>
        /// <param name="mode">CommandMode.</param>
        /// <param name="waypoint">New Waypoint to create or that will replace an existing one.</param>
        /// <param name="fleetKey">Fleet key whose waypoints will be affected.</param>
        /// <param name="index">Waypoint Index to edit, or where to insert.</param>
        public WaypointCommand(CommandMode mode, Waypoint waypoint, long fleetKey = Global.None, int index = 0)
        {
            Waypoint = waypoint;
            Mode = mode;
            FleetKey = fleetKey;
            Index = index;
        }
                
        
        /// <summary>
        /// Load from XML: Initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> within
        /// a Nova component definition file (xml document).
        /// </param>
        public WaypointCommand(XmlNode node)
        {
            XmlNode subnode = node.FirstChild;
            
            while (subnode != null)
            {
                switch (subnode.Name.ToLower())
                {
                    case "fleetkey":
                      FleetKey = long.Parse(subnode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
                    break;
                    case "mode":
                        Mode = (CommandMode)Enum.Parse(typeof(CommandMode), subnode.FirstChild.Value);
                    break;
                    case "index":
                        Index = int.Parse(subnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                    case "waypoint":
                        Waypoint = new Waypoint(subnode);
                    break;
                }
            
                subnode = subnode.NextSibling;
            }   
        }
        
        
        /// <inheritdoc />
        public bool IsValid(EmpireData empire, out Message message)
        {
            if (!empire.OwnedFleets.ContainsKey(FleetKey))
            {
                message = new Message(empire.Id, " trying to add a Waypoint for a Fleet that you do not own", "Invalid Command", null);
                return false;
            }
            message = null;
            return true;
        }



        /// <summary>
        /// There could be dozens of splits and merges of fleets at an individual waypoint during one turn and dozens of new fleets moving in different directions during that turn.
        /// Process the splits and merges in Chronological Order or the process will be nonsensical!
        /// The design is clear - reproduce Stars! not some new game where merges are programmed to occur at some point in the future.
        /// Practical example of Split/Merge using Stars! existing logic:
        /// The players primary invasion fleet arrives at an opponents Home Planet, after destroying the Station and defense fleet/s the player:
        /// 1/ Moves the escort ships from the Mine Layer group to the invasion fleet (Mine layer group must have been given the "Lay Mines" order 1 turn before arrival in order to lay mines during the first year at the opponents Home Planet.
        /// 2/ Player Splits off 4 SpaceMine clearing fleets of 50 (beam weapon) escort ships and one empty freighter in each new fleet from the invasion fleets to move at warp 4 towards the nearest 4 Mine fields 
        /// 3/ Player Splits off a colonisation Fleet containing a coloniser and any obsolete transports and sets it to "Colonise" so it claims the planets minerals (if invasion Bombers kill off all population during the first turn) before they start to dissipate 
        /// 4/ Player must increase the Fleet ID so it is higher than the invasion fleets ID (so it tries to colonise AFTER the bombers kill off existing population) so the player creates (say) 8 new fleets that use up the fleet ID's that are smaller than the invasion fleets ID
        /// 5/ Player merges the Coloniser Fleets vessels into the highest Fleet ID vessel (so it tries to colonise AFTER the bombers kill off existing population)
        /// 6/ Player spots 2 fat enemy convoys with over 1M tons of capacity and creates 2 new fleets each containing 1 empty transport from the invasion fleet plus 8 of the (beam weapon) escort ships from each of the 4 Mine Clearing Fleets and sends the 2 new fleets after the 2 (hopefully) mineral convoys (needs to be beam weapon ships because the risk of entering min fields is high).
        /// This is a player move that is made to use the existing Stars! programs behaviour, it could be simplified if we executed "Colonise" orders after (implied) bombing orders, it also implies that Fleet ID's are reused like in Stars!(which we do not do yet)
        /// To process the splits and merges in chronological order we needs an increasing key on the SplitMerge orders
        /// if we just iterate through serverState.IterateAllFleets() how do we do the Splits and Merges of the fleets that are created this turn?
        /// We will implement this: 
        /// 1/  Execute the Waypoint zero commands (which may include SplitMergeTask and Load/unloadTask or merge (or InvadeTask?) commands for fleets that do not exist yet or that will not exist at the start or WayPoint One processing), 
        /// 2/  but do not remove them until all Waypoint.Edit and Waypoint.Insert commands are loaded (or their indexes will be nonsensical)
        /// 3/  Delete the waypoint Zero commands that have already been executed

        /// Stars! only supports splits or merges on turn 0
        /// Cargo unload or load from the Cargo dialog is also performed on turn 0 and may be done on fleets that don't exist at the start of turn 0 or may be performed on fleets that dont exist at the end of turn 0
        /// so do load/unload and split/merge in chronological order so the action can be performed on the fleet  while it still exists!

        /// </summary>

        public Message ApplyToState(EmpireData empire)
        {
            switch (Mode)

            {
                case CommandMode.Add:
                    empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint);
                    return null;
                case CommandMode.Insert:
                    empire.OwnedFleets[FleetKey].Waypoints.Insert(Index, Waypoint);
                    return null;
                case CommandMode.Delete:
                    empire.OwnedFleets[FleetKey].Waypoints.RemoveAt(Index);
                    return null;
                case CommandMode.Edit:
                    empire.OwnedFleets[FleetKey].Waypoints.RemoveAt(Index);
                    if (empire.OwnedFleets[FleetKey].Waypoints.Count > Index) empire.OwnedFleets[FleetKey].Waypoints.Insert(Index, Waypoint);
                    else empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint); //Waypoint.Insert[Insert] past the end of the list is an Add
                    return null;
            }
            return null;

        }
        public Message PreApplyToState(EmpireData empire, Item Target)
        {
             Message message = null;
             switch (Mode)

            {
                case CommandMode.Add:
                    {
                        {
                            if  (Waypoint.Task is SplitMergeTask)// the next waypoint command to be processed might be for the fleet created by this waypoint command
                        // so we need to create the fleet here so the next command has a fleet to attach to
                            {
                                empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint);  // Add the Waypoint 
                                if (isWaypointZeroCommand(Waypoint, empire.OwnedFleets[FleetKey]))
                                {
                                    if ((Waypoint.Task as SplitMergeTask).IsValid(empire.OwnedFleets[FleetKey], Target, empire, empire,out message))
                                    {
                                        Waypoint.Task.Perform(empire.OwnedFleets[FleetKey], Target, empire, null, out message);  //PrePerform it so the fleets IDs match the commands that follow
                                        return message;
                                    }
                                    else
                                    {
                                        return message;
                                    }
                                }
                            }
                            else
                            {// the next waypoint command to be processed might be a Split command for this Fleet
                             // so we need to Load the Cargo here or the "Other" fleet may end up empty and this fleet may be asked 
                             // to carry it's full payload of cargo PLUS the payload belonging to the "Other" fleet   :(
                                if (Waypoint.Task is CargoTask)
                                {
                                    empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint);  // Add the Waypoint 
                                    if (isWaypointZeroCommand(Waypoint, empire.OwnedFleets[FleetKey]))
                                    {
                                        if ((Waypoint.Task as CargoTask).IsValid(empire.OwnedFleets[FleetKey], Target, empire, null, out message))
                                        {
                                            Waypoint.Task.Perform(empire.OwnedFleets[FleetKey], Target, empire, null, out message); //PrePerform it so the cargo levels are correct when the next Split or Merge happens 
                                            return message;
                                        }
                                        else return message;
                                    }
                                } // we don't remove the waypoint until all waypoints are inserted as a Waypoint.Edit(7,waypoint) will not work too well
                                  // if we have removed 6 of the waypoints and Waypoint.Count = 1
                                  // Look in SpliFleetStep.cs for the WaypointZero removals
                            }
                            foreach (Fleet newFleet in empire.TemporaryFleets) empire.AddOrUpdateFleet(newFleet);
                            empire.TemporaryFleets.Clear();
                            return null;
                        }
                    }
                case CommandMode.Insert:
                    {// the next waypoint command to be processed might be for the fleet created by this waypoint command
                        // so we need to create the fleet here so the next command has a fleet to attach to
                        empire.OwnedFleets[FleetKey].Waypoints.Insert(Index,Waypoint);
                        {
                            if(Waypoint.Task is SplitMergeTask)
                            {
                                if (isWaypointZeroCommand(Waypoint, empire.OwnedFleets[FleetKey]))
                                {
                                    if ((Waypoint.Task as SplitMergeTask).IsValid(empire.OwnedFleets[FleetKey], Target, empire,empire,out message))
                                    {
                                        Waypoint.Task.Perform(empire.OwnedFleets[FleetKey], Target, empire, null, out message);
                                    }
                                    return message;
                                }
                            }
                            else
                            {// the next waypoint command to be processed might a Split command for this Fleet
                             // so we need to Load the Cargo here or the "Other" fleet may end up empty and this fleet may be asked 
                             // to carry it's full payload of cargo PLUS the payload belonging to the "Other" fleet   :(
                                if (Waypoint.Task is CargoTask)
                                    if (isWaypointZeroCommand(Waypoint, empire.OwnedFleets[FleetKey]))
                                    {
                                        if ((Waypoint.Task as CargoTask).IsValid(empire.OwnedFleets[FleetKey], Target, empire, null, out message))
                                        {
                                            Waypoint.Task.Perform(empire.OwnedFleets[FleetKey], Target, empire, null, out message);
                                            return message;
                                        }
                                        else return message;
                                    }
                            } // we don't remove the waypoint until all waypoints are inserted as a Waypoint.Edit(7,waypoint) will not work too well
                              // if we have removed 6 of the waypoints and Waypoint.Count = 1
                              // Look in SplitFleetStep.cs for the WaypointZero removals
                            foreach (Fleet newFleet in empire.TemporaryFleets) empire.AddOrUpdateFleet(newFleet);
                            empire.TemporaryFleets.Clear();

                            return null;
                        }
                    }
                case CommandMode.Delete:
                    empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint);  // Add the Waypoint 
                    // we prevent Deletes in the Waypoint zero list so no need to pre-process it
                    return null;
                case CommandMode.Edit:
                    empire.OwnedFleets[FleetKey].Waypoints.Add(Waypoint);  // Add the Waypoint 
                    //We prevent edits of Waypoint Zeros so no need to pre-process it
                    //you can edit a waypoint zero action by adding another action that undoes the first action
                    return null;
            }
            return null;
        }

        private bool isWaypointZeroCommand(Waypoint waypoint,Fleet fleet)
        {
            int theIndex = fleet.Waypoints.IndexOf(waypoint);
            int index = 0;
            String destination = fleet.Waypoints[0].Destination;
            bool found = false;
            while ((!found) && (index <= theIndex))
            {
                found = (fleet.Waypoints[index].Destination != destination);
                index++;
            }
            return !found;

        }

        /// <summary>
        /// Save: Serialize this property to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Command.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelCom = xmldoc.CreateElement("Command");
            xmlelCom.SetAttribute("Type", "Waypoint");
            Global.SaveData(xmldoc, xmlelCom, "Mode", Mode.ToString());
            Global.SaveData(xmldoc, xmlelCom, "FleetKey", FleetKey.ToString("X"));
            Global.SaveData(xmldoc, xmlelCom, "Index", Index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (Waypoint != null)
            {
                xmlelCom.AppendChild(Waypoint.ToXml(xmldoc));
            }
            
            return xmlelCom;    
        }
    }
}
