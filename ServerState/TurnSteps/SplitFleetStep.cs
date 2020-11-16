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
    public class WaypointComparer : System.Collections.IComparer
    {
        // Compares the timestamps of two waypoints.
        public int Compare(Object A, Object B)
        {
            long timestampA;
            long timestampB;
            timestampA = (A as Waypoint).guiTimestamp.Ticks;
            timestampB = (B as Waypoint).guiTimestamp.Ticks;
            if (timestampA > timestampB) return 1;
            if (timestampA < timestampB) return -1;
            if (timestampA == timestampB) return 0;
            else return 0;
        }
    }
    public class SplitFleetStep : ITurnStep
    {
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
        /// 6/ Player spots 2 fat enemy convoys with over 1M tons of capacity and creates 2 new fleets each containing 1 empty transport from the invasion fleet plus 8 of the (beam weapon) escort ships from each of the 4 Mine Clearing Fleets and sends the 2 new fleets after the 2 (hopefully) mineral convoys (needs to be beam weapon ships because the risk of entering mine fields is high).
        /// This is a player move that is made to use the existing Stars! programs behaviour, it could be simplified if we executed "Colonise" orders after (implied) bombing orders, it also implies that Fleet ID's are reused like in Stars!(which we do not do yet)
        /// To process the splits and merges in chronological order we needs an increasing key on the SplitMerge orders
        /// if we just iterate through serverState.IterateAllFleets() how do we do the Splits and Merges of the fleets that are created this turn?
        /// Some possibilities:
        /// 1/ A constriction that might work in a Beta program might be to prevent splits and merges on newly created fleets (tough sell to the Stars! crowd)
        /// 2/ We could keep iterating through the Waypoints (serverState.IterateAllFleets()) doing only the next chronological SplitMerge waypoint task on each pass through until no more SplitMerge tasks exist
        /// 3/ We could do one iteration of the serverState.IterateAllFleets() and get a list of ONLY the SplitMerge tasks (which may be very much smaller than the list of every waypoint), then sort and execute that list in sequence, waypoint tasks for the intermediate (new) fleets must be added in as the fleets are created. How do we transmit the waypoints for the fleets that are not in (serverState.IterateAllFleets()) yet?
        /// How do we transmit the waypoints for the fleets that are not in (serverState.IterateAllFleets()) yet?

        /// We will implement this: 
        /// 1/  Execute the Waypoint zero commands (which may include SplitMergeTask and Load/unloadTask or merge (or InvadeTask?) commands for fleets that do not exist yet or that will not exist at the start or WayPoint One processing), 
        /// 2/  but do not remove them until all Waypoint.Edit and Waypoint.Insert commands are loaded (or their indexes will be nonsensical)
        /// 3/  Delete the waypoint Zero commands that have already been executed

        /// Stars! only supports splits or merges on turn 0
        /// Cargo unload or load from the Cargo dialog is also performed on turn 0 and may be done on fleets that don't exist at the start of turn 0 or may be performed on fleets that dont exist at the end of turn 0
        /// so do load/unload and split/merge in chronological order so the action can be performed on the fleet  while it still exists!

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
        public Message Process(ServerData serverState)
        {
            WaypointComparer wCompare = new WaypointComparer();
            string WaypointZeroDestination = "";
            int Index = 0;
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                if (fleet.Waypoints.Count > 0)
                {
                    WaypointZeroDestination = fleet.Waypoints[0].Destination;
                    Index = 1;
                    while ((Index < fleet.Waypoints.Count) && (fleet.Waypoints[Index].Destination == WaypointZeroDestination))
                    {
                        if ((fleet.Waypoints[Index].Task is SplitMergeTask) || (fleet.Waypoints[Index].Task is CargoTask) || (fleet.Waypoints[Index].Task is NoTask))
                        {
                            fleet.Waypoints.RemoveAt(Index); //Remove waypoints that have already been processed
                        }
                        else Index++;
                    }
                }
            }
            serverState.CleanupFleets();
            return null;
        }
    }
}

