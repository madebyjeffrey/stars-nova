#region Copyright Notice
// ============================================================================
// Copyright (C) 2011, 2012 The Stars-Nova Project
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
    using System.Runtime.Versioning;

    using Nova.Common;
    using Nova.Common.Components;
    using Nova.Common.Waypoints;

    /// <summary>
    /// Manages any pre-turn generation data setup.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class FirstStep : ITurnStep
    {
        private ServerData serverState;

        public FirstStep()
        {
        }

        public List<Message> Process(ServerData serverState)
        {
            List<Message> messages = new List<Message>();
            this.serverState = serverState;

            foreach (Fleet fleet in serverState.IterateAllFleets())
                if (fleet.Waypoints.Count > 0)
                    if (fleet.Waypoints[0].Task is LayMinesTask)
                    {
                        Waypoint waypointZero = fleet.Waypoints[0];
                        Message message;

                        for (int mineType = 0; mineType < 3; mineType++) //an indexor on the minetype would be nice
                            if (((mineType == 0) && (fleet.NumberOfMines > 0))
                                || ((mineType == 1) && (fleet.NumberOfHeavyMines > 0))
                                || ((mineType == 2) && (fleet.NumberOfSpeedBumpMines > 0)))
                            {
                                message = null;
                                if (waypointZero.Task.IsValid(fleet, null, serverState.AllEmpires[fleet.Owner], null, out message))
                                {
                                    if (null != message) serverState.AllMessages.Add(message);
                                    long key = ((fleet.Position.X / Global.MineFieldSnapToGridSize) * 0x10000000 + (fleet.Position.Y / Global.MineFieldSnapToGridSize)) + fleet.Owner * 0x40000000000000 + mineType * 0x4000000;
                                    Minefield minefield;
                                    serverState.AllMinefields.TryGetValue(key, out minefield);
                                    int increase = 0;
                                    if (mineType == 0) increase = fleet.NumberOfMines;
                                    if (mineType == 1) increase = fleet.NumberOfHeavyMines;
                                    if (mineType == 2) increase = fleet.NumberOfSpeedBumpMines;
                                    if (minefield != null)
                                    {
                                        minefield.NumberOfMines += increase;
                                        messages.Add(new Message(fleet.Owner, fleet.Name + " has increased a " + minefield.MineDescriptor + " minefield by " + increase.ToString() + " mines.", "Increase Minefield", key, fleet.Key));
                                    }
                                    else
                                    {                                       // No Minefield found. Start a new one.
                                        Minefield newField = new Minefield();

                                        newField.Position = fleet.Position;
                                        newField.Owner = fleet.Owner;
                                        newField.NumberOfMines = increase;
                                        newField.Key = key;

                                        serverState.AllMinefields[key] = newField;
                                        messages.Add(new Message(fleet.Owner, fleet.Name + " has created a " + newField.MineDescriptor + " minefield with " + increase.ToString() + " mines.", "New Minefield", key, fleet.Key));

                                    }
                                }
                                if (message != null) messages.Add(message);
                            }
                    }
            List<long> deleted = new List<long>(); 
            foreach (long minefieldKey in serverState.AllMinefields.Keys)
            {
                Minefield minefield = serverState.AllMinefields[minefieldKey];
                // Minefields decay 1% each year. Fields of less than 10 mines are
                // just not worth bothering about.
                minefield.NumberOfMines -= minefield.NumberOfMines / 100;
                if (minefield.NumberOfMines <= 10)
                {
                    deleted.Add(minefieldKey);
                }
            }
            foreach (long key in deleted)  serverState.AllMinefields.Remove(key);
            return messages;
        }
    }
}
