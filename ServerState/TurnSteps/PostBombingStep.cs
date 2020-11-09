﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.Server.TurnSteps
{
    using Nova.Common;
    using Nova.Common.Waypoints;
    /// <summary>
    /// Colonise Tasks are performed after bombing steps. In Stars it was necessary to perform (sometimes) dozens of SplitMerge steps to
    /// ensure the ID of the colonizer was higher than the ID of the bomber Fleet and therefore performed it's colonise order after the
    /// Bombing was carried out. It is easier to just perform the colonise steps last rather than have the Orders file filled up with
    /// SplitMerge tasks.
    /// </summary>
    class PostBombingStep : ITurnStep
    {
        public void Process(ServerData serverState)
        {
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                int index = 1;
                int maxIndex = fleet.Waypoints.Count - 1;
                if (fleet.Waypoints.Count > 0)
                {
                    string dest0 = fleet.Waypoints[0].Destination;
                    while (index <= maxIndex)
                    {
                        if (fleet.Waypoints[index].Destination == dest0)
                        {
                            Star target = null;
                            foreach (Star star in serverState.AllStars.Values) if (star.Name == dest0) target = star;
                            if ((fleet.Waypoints[index].Task.IsValid(fleet, target, null, null)) && (fleet.Waypoints[index].Task is ColoniseTask))
                            {
                                fleet.Waypoints[index].Task.Perform(fleet, target, null, null);
                            }
                            try
                            {
                                serverState.AllMessages.AddRange(fleet.Waypoints[index].Task.Messages);
                            }
                            catch
                            {
                                Report.Information("Bad waypoint for " + fleet.Name);
                            }
                            // Task is done, clear it.
                            fleet.Waypoints[index].Task = new NoTask();

                        }
                        index++;
                    }
                    serverState.CleanupFleets();
                }
            }
        }
    }
}
