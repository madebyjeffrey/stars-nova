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
        public Message Process(ServerData serverState)
        {
            Message message = null;
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                int index = 0;
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

                            EmpireData receiver = null;
                            if ((target is Star) && (target.Owner != 0)) receiver = serverState.AllEmpires[target.Owner];
                            if ((fleet.Waypoints[index].Task.IsValid(fleet, target, serverState.AllEmpires[fleet.Owner], receiver,out message)) && ((fleet.Waypoints[index].Task is ColoniseTask) || (fleet.Waypoints[index].Task is InvadeTask)))
                            {
                                if (message != null) serverState.AllMessages.Add(message);
                                message = null;
                                bool invading = false;
                                if ((receiver != null) && (receiver != serverState.AllEmpires[fleet.Owner]))
                                {
                                    fleet.Waypoints[index].Task = new NoTask();
                                    invading = true;
                                    IWaypointTask invade = new InvadeTask();
                                    invade.Perform(fleet, target, serverState.AllEmpires[fleet.Owner],receiver,out message); //Not exactly how Stars! does it but it should make programming the AI easier
                                    if (message != null) serverState.AllMessages.Add(message);
                                    message = null;
                                }
                                else fleet.Waypoints[index].Task.Perform(fleet, target, serverState.AllEmpires[fleet.Owner], receiver,out message);
                                if (message != null) serverState.AllMessages.Add(message);
                                message = null;
                                try
                                {
                                    if (index < fleet.Waypoints.Count)
                                    serverState.AllMessages.AddRange(fleet.Waypoints[index].Task.Messages);
                                }
                                catch
                                {
                                    if (Global.Debug) Report.Information("Bad waypoint for " + fleet.Name + " Empire " + fleet.Owner.ToString());
                                }
                                if (!invading)
                                {
                                    fleet.Waypoints.RemoveAt(index);
                                    maxIndex--;
                                    index--;
                                }
                            }
                            else
                            {
                                if (message != null) serverState.AllMessages.Add(message);
                            }

                        }
                        index++;
                    }
                }
            }
            serverState.CleanupFleets();
            return message;
        }
    }
}
