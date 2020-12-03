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

    using Nova.Common;
    using Nova.Common.Components;
    using Nova.Common.Waypoints;

    /// <summary>
    /// Manages any pre-turn generation data setup.
    /// </summary>
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
                        if (waypointZero.Task.IsValid(fleet, null, serverState.AllEmpires[fleet.Owner], null, out message))
                        {
                            if (null != message) serverState.AllMessages.Add(message);

                            long key = ((fleet.Position.X / Global.MineFieldSnapToGridSize) * 4294967296 + (fleet.Position.Y / Global.MineFieldSnapToGridSize)) + fleet.Owner * 1152921504606846976;
                                Minefield minefield;
                            serverState.AllMinefields.TryGetValue(key, out minefield);
                            if (minefield != null)
                            {
                                minefield.NumberOfMines += fleet.NumberOfMines;
                                messages.Add(new Message(fleet.Owner, fleet.Name + " has increased a minefield by "+ fleet.NumberOfMines.ToString()+" mines.", "Increase Minefield", null, fleet.Id));
                            }
                            else
                            {                                       // No Minefield found. Start a new one.
                                Minefield newField = new Minefield();

                                newField.Position = fleet.Position;
                                newField.Owner = fleet.Owner;
                                newField.NumberOfMines = fleet.NumberOfMines;
                                newField.Key = key;

                                serverState.AllMinefields[key] = newField;
                                messages.Add(new Message(fleet.Owner, fleet.Name + " has created a minefield with " + fleet.NumberOfMines.ToString() + " mines.", "New Minefield", null, fleet.Id));

                            }
                        }
                        if (message != null) messages.Add(message);
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
