#region Copyright Notice
// ============================================================================
// Copyright (C) 2009 - 2017 stars-nova
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

namespace Nova.Server.TurnSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Nova.Common;
    using Nova.Common.Waypoints;

    public class SplitFleetStep : ITurnStep
    {
        /// <summary>
        /// There could be dozens of splits and merges of fleets at an individual waypoint during one turn and dozens of new fleets moving in different directions during that turn.
        /// Process the splits and merges in Chronological Order or the process will be nonsensical!
        /// The design is clear - reproduce Stars! not some new game where merges are programmed to occur at some point in the future.

        /// We will implement this: 
        /// 1/  Execute the Waypoint zero commands (which may include SplitMergeTask and Load/unloadTask or merge (or InvadeTask?) commands for fleets that do not exist yet or that will not exist at the start or WayPoint One processing), (Waypoint.PreProcess)
        /// 2/  but do not remove them until all Waypoint.Edit and Waypoint.Insert commands are loaded (or their indexes will be nonsensical)
        /// 3/  Delete the waypoint Zero commands that have already been executed (SplitFleetStep)

        /// Stars! only supports splits or merges on turn 0
        /// Cargo unload or load from the Cargo dialog may also performed on turn 0 and may be done on fleets that don't exist at the start of turn 0 or may be performed on fleets that dont exist at the end of turn 0
        /// so do load/unload and split/merge in chronological order so the action can be performed on the fleet  while it still exists!
        /// 
        /// </summary>
        public struct fleetWaypoint
        {
            public fleetWaypoint(Fleet fleet, Waypoint wayPoint)
            {
                Fleet = fleet;
                Waypoint = wayPoint;
            }
            public Fleet Fleet { get; }
            public Waypoint Waypoint { get; }
        }
        /// <summary>
        /// The CargoTasks and SplitMergeTasks were preProcessed in sequence but not removed (during "ParseCommands") so as to keep the indexes alligned between server and client.
        /// All that is left now is to remove the already processed waypoints.
        /// 
        /// </summary>
        /// <param name="serverState"></param>
        /// <returns></returns>
        public List <Message> Process(ServerData serverState)
        {
            List<Message> result = new List<Message>();
            string WaypointZeroDestination = "";
            int Index = 0;
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                if (fleet.Waypoints.Count > 0)
                {
                    Waypoint waypointZero = new Waypoint (fleet.Waypoints[0]);
                    waypointZero.Task = new NoTask();
                    WaypointZeroDestination = fleet.Waypoints[0].Destination;
                    Index = 0;
                    while ((Index < fleet.Waypoints.Count) && (fleet.Waypoints[Index].Destination == WaypointZeroDestination))
                    {
                        if (fleet.Waypoints[Index].Task is SplitMergeTask) 
                        {
                            fleet.Waypoints.RemoveAt(Index); //Remove waypoints that have already been processed
                        }
                        else if  (fleet.Waypoints[Index].Task is CargoTask)
                        {
                            if ((fleet.Waypoints[Index].Task as CargoTask).Amount.Mass == 0)
                            {
                                fleet.Waypoints.RemoveAt(Index); //Only remove "spent" waypoints 
                            }
                            else 
                            {
                                Message messageIsValid = null;
                                Message messagePerform = null;
                                if ((fleet.Waypoints[Index].Task as CargoTask).IsValid(fleet, (fleet.Waypoints[Index].Task as CargoTask).Target, serverState.AllEmpires[fleet.Owner], null, out messageIsValid)) fleet.Waypoints[Index].Task.Perform(fleet, (fleet.Waypoints[Index].Task as CargoTask).Target, serverState.AllEmpires[fleet.Owner], null, out messagePerform);
                                if (messageIsValid != null) result.Add(messageIsValid);
                                if (messagePerform != null) result.Add(messagePerform);
                                fleet.Waypoints.RemoveAt(Index); //Process and remove waypointZero cargoTask waypoints 
                            }
                        }
                        else Index++;
                    }
                    if (fleet.Waypoints.Count == 0)
                    {
                        fleet.Waypoints.Add(waypointZero);
                    }
                }
            }
            serverState.CleanupFleets();
            return result;
        }
    }
}

