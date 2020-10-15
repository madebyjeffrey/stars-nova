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
        /// Some possibilities:
        /// 1/ A constriction that might work in a Beta program might be to prevent splits and merges on newly created fleets (tough sell to the Stars! crowd)
        /// 2/ We could keep iterating through the Waypoints (serverState.IterateAllFleets()) doing only the next chronological SplitMerge waypoint task on each pass through until no more SplitMerge tasks exist
        /// 3/ We could do one iteration of the serverState.IterateAllFleets() and get a list of ONLY the SplitMerge tasks (which may be very much smaller than the list of every waypoint), then sort and execute that list in sequence, waypoint tasks for the intermediate (new) fleets must be added in as the fleets are created. How do we transmit the waypoints for the fleets that are not in (serverState.IterateAllFleets()) yet?
        /// How do we transmit the waypoints for the fleets that are not in (serverState.IterateAllFleets()) yet?

        /// We will try to implement this: 
        /// 1/  get a list of ONLY the SplitMerge commands (which may include SplitMerge commands for fleets that do not exist yet), 
        /// 2/  sort that list in chronological sequence
        /// 3/  execute tasks from that list in chronological sequence (the client must be able to predict the ID's of the Fleets correctly so the commands can be matched to the new fleets for this turn) 

        /// Stars! only supports splits or merges on turn 0
        /// Cargo unload or load from the Cargo dialog is also performed on turn 0 and may be done on fleets that don't exist at the start of turn 0 or may be performed on fleets that dont exist at the end of turn 0
        /// so do load/unload and split/merge in chronological order so the action can be performed on the fleet  while it still exists!

        /// </summary>
        public void Process(ServerData serverState)
        {
            List<Waypoint> splits = new List<Waypoint>();
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                splits.Clear();
                foreach  (Waypoint waypoint in fleet.Waypoints)
                {

                    if (waypoint.Task is SplitMergeTask && waypoint.Task.IsValid(fleet, null, null))
                    {


                        EmpireData sender = serverState.AllEmpires[fleet.Owner];

                        waypoint.Task.Perform(fleet, fleet, sender);
                        splits.Add(waypoint);
                    }
                    if (waypoint.Task is CargoTask) // cargo transfers are allowed in space also (how would you collect minerals from space debris otherwise) interfleet cargo transfers in space are rare but sometimes needed
                    {
                        if ((waypoint.Task as CargoTask).Target.Type == ItemType.Star)
                        {
                            if (serverState.AllStars.ContainsKey((waypoint.Task as CargoTask).Target.Name.ToString()))
                            {
                                Star star = serverState.AllStars[(waypoint.Task as CargoTask).Target.Name.ToString()];
                                EmpireData sender = serverState.AllEmpires[fleet.Owner];
                                if (waypoint.Task.IsValid(fleet, star, sender))
                                {
                                    waypoint.Task.Perform(fleet, star, sender);
                                    splits.Add(waypoint);
                                }
                            }
                        }
                        else  if (serverState.AllEmpires[fleet.Owner].OwnedFleets.ContainsKey((waypoint.Task as CargoTask).Target.Key))
                        {
                            EmpireData sender = serverState.AllEmpires[fleet.Owner];
                            Fleet other = serverState.AllEmpires[fleet.Owner].OwnedFleets[(waypoint.Task as CargoTask).Target.Key];
                            if (waypoint.Task.IsValid(fleet, other, sender))
                            {

                                waypoint.Task.Perform(fleet, other, sender);
                                splits.Add(waypoint);
                            }
                        }
                    }
                }                
                foreach (Waypoint waypoint in splits) fleet.Waypoints.Remove(waypoint);
            }

            serverState.CleanupFleets();
        }
    }
}
