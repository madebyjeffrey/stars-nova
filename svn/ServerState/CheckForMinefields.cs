#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010 The Stars-Nova Project
//
// This file is part of Stars! Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
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
// along with this program. If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

namespace Nova.Server
{
    using System;    
    using System.Collections.Generic;
    
    using Nova.Common;
    using Nova.Common.DataStructures;
    using Nova.Common.Waypoints;
    
    /// <summary>
    /// Check to see if a fleet is in an enemy Minefield and inflict appropriate damage.
    /// </summary>
    public static class CheckForMinefields
    {
        private static readonly Random random = new Random();
        private ServerData serverState;
        
        public CheckForMinefields(ServerData serverState)
        {
            this.serverState = serverState;
        }

        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Do a check for minefields.
        /// </summary>
        /// <param name="fleet">A moving fleet.</param>
        /// <returns>false</returns>
        /// ----------------------------------------------------------------------------
        public static bool Check(Fleet fleet)
        {
            foreach (Minefield minefield in serverState.AllMinefields.Values)
            {
                if (IsInField(fleet, minefield))
                {
                    DestroyMines(fleet, minefield);    // if you can destroy mines fast enough you are safe
                    if (IsInField(fleet, minefield))
                    {
                        bool hit = CheckForHit(fleet, minefield);
                        if (hit)
                        {
                            InflictDamage(fleet, minefield);
                        }
                    }
                }
            }
            return false;
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Determine if a fleet is within a Minefield. The fleet is inside the
        /// circle if the distance between the field and the center of the field is
        /// less than the radius of the field.
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="minefield"></param>
        /// <returns></returns>
        /// ----------------------------------------------------------------------------
        private static bool IsInField(Fleet fleet, Minefield minefield)
        {
            // If we are travelling at a "safe" speed we can just pretend we are
            // not in a Minefield.

            if (fleet.Speed <= minefield.SafeSpeed)
            {
                return false;
            }

            if (fleet.Owner == minefield.Empire) return false;

            double distance = PointUtilities.Distance(fleet.Position, minefield.Position);

            if (distance < minefield.Radius)
            {
                return true;
            }

            return false;
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Check if the fleet hits the minefield.
        /// </summary>
        /// <remarks>
        /// The probability of hitting a mine is 0.3% per light year traveled for each
        /// warp over the safe speed.
        /// TODO (priority 3) - reference required.
        ///
        /// Example: A fleet traveling at Warp 9 has a 1.5% chance per light year
        /// traveled in a turn.  Traveling 10 light years through the Minefield that
        /// turn, the fleet has a 10.5% chance of triggering a mine.
        /// </remarks>
        /// <param name="fleet">The moving fleet.</param>
        /// <param name="minefield">The minefield being traversed.</param>
        /// <returns>true if the minefield is hit.</returns>
        /// ----------------------------------------------------------------------------
        private static bool CheckForHit(Fleet fleet, Minefield minefield)
        {
            // Calculate how long we are going to be in the Minefield. This is the
            // lesser of the distance to the next waypoint and the radius of the
            // field.

            NovaPoint currentPosition = fleet.Position;
            Waypoint targetWaypoint = fleet.Waypoints[0];
            NovaPoint targetPosition = targetWaypoint.Position;

            double travelDistance = PointUtilities.Distance(currentPosition, targetPosition);
            if (minefield.Radius > (int)travelDistance)
            {
                travelDistance = minefield.Radius;
            }

            double speeding = fleet.Speed - minefield.SafeSpeed;
            double probability = (0.03 * travelDistance * speeding) * 100;
            double dice = random.Next(0, 100);

            if (dice < probability)
            {
                return true;
            }

            return false;
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// We've hit a mine. Inflict appropriate damage to the fleet and bring it to a
        /// stop. If all ships are gone destroy the fleet.
        ///
        /// Let's start with the simplest algoritm:
        ///
        /// 5 destroyers take 500dp damage = 100dp each = 50dp to armor, 50dp to shields
        /// (absorbed).
        /// </summary>
        /// <param name="fleet">The fleet that hit the minefield.</param>
        /// <param name="minefield">The minefield being impacted.</param>
        /// ----------------------------------------------------------------------------
        private static void InflictDamage(Fleet fleet, Minefield minefield)
        {
            bool destroyed = false;
            int shipDamage = 100 / 2;
            int shipsLost = 0;
            fleet.Speed = 0;
            Resources resourcesDestroyed = new Resources();
            Cargo cargoLost = new Cargo();
            List<ShipToken> tokensToRemove = new List<ShipToken>();

            foreach (ShipToken token in fleet.Composition.Values)
            {
                token.Armor -= shipDamage;

                if (token.Damage >= 100)
                {
                    tokensToRemove.Add(token);
                    shipsLost++;
                }
            }

            foreach (ShipToken removeToken in tokensToRemove)
            {
                resourcesDestroyed += removeToken.Design.Cost * removeToken.Quantity;
                Cargo cargoBefore = fleet.Cargo;
                fleet.Composition.Remove(removeToken.Key);
                Cargo cargoAfter = fleet.Cargo;
                cargoLost.Add(cargoBefore);
                cargoLost.Remove(cargoAfter);
            }

            Message message = new Message();
            message.Audience = fleet.Owner;
            message.Type = "Minefield";
            message.Event = minefield.Key;
            message.FleetKey = fleet.Key;
            message.Text = "Fleet " + fleet.Name
               + " has hit a Minefield." + "\n\n";

            if (shipsLost == 0)
            {
                message.Text += "None of your ships were destroyed.";
                fleet.Speed = 0;
            }
            else if (fleet.Composition.Count != 0)
            {
                message.Text += shipsLost.ToString(System.Globalization.CultureInfo.InvariantCulture)
                   + " of your ships were destroyed.\n";
            }
            else
            {
                message.Text += "All of your ships were destroyed.\n";
                message.Text += "You lost this fleet.";
                //serverState.AllEmpires[fleet.Owner].OwnedFleets.Remove(fleet.Key);  //we are in a Foreach (long fleet.id in serverState.IterateAllFleetKeys) so don't destroy fleets 
                //serverState.AllEmpires[fleet.Owner].FleetReports.Remove(fleet.Key); //inside the loop
                destroyed = true;
                CreateSalvage(fleet.Position, resourcesDestroyed, cargoLost, fleet.Owner);
            }

            serverState.AllMessages.Add(message);
            return destroyed;
        }

        private void CreateSalvage(NovaPoint position, Resources salvage, Cargo cargo, int empireID)
        {
            EmpireData empire = serverState.AllEmpires[empireID];
            Common.Components.ShipDesign salvageDesign = null;
            foreach (Common.Components.ShipDesign design in empire.Designs.Values) if (design.Name.Contains("S A L V A G E")) salvageDesign = design;
            ShipToken token = new ShipToken(salvageDesign, 1);
            Fleet fleet = new Fleet(token, position, empire.GetNextFleetKey());
            fleet.Position = position;
            fleet.Name = "S A L V A G E";
            fleet.TurnYear = empire.TurnYear;

            // Add the fleet to the state data so it can be tracked.
            serverState.AllEmpires[fleet.Owner].AddOrUpdateFleet(fleet);
            fleet.Cargo = cargo.Scale(0.75); //TODO priority 1 check if we want to allow survivors to be rescued or do we have to murder them all?
            fleet.Cargo.Ironium += (int)(salvage.Ironium * 0.75);
            fleet.Cargo.Boranium += (int)(salvage.Boranium * 0.75);
            fleet.Cargo.Germanium += (int)(salvage.Germanium * 0.75); //TODO priority 1 check salvage conversion ratios

        }

        private void DestroyMines(Fleet fleet, Minefield minefield)
        {

            int minesToDestroy = 0;

            foreach (ShipToken token in fleet.Composition.Values)
            {
                foreach (Common.Components.Weapon weapon in token.Design.Weapons)
                {
                    if (weapon.IsBeam) minesToDestroy += token.Quantity * weapon.Power * weapon.Power;
                    //TODO (priority 4) find correct formula for quantity of mines to destroy for gatling and beam weapons
                }
            }

            if (minefield.NumberOfMines < 10 + minesToDestroy) minefield.NumberOfMines = 10;
            else minefield.NumberOfMines -= minesToDestroy;
            if (minesToDestroy > 10)   
            {
                Message message = new Message();
                message.Type = "Minefield";
                message.Audience = fleet.Owner;
                message.Event = minefield.Key;
                message.FleetKey = fleet.Key;
                message.Text = "Fleet " + fleet.Name
                   + " has destroyed " + minesToDestroy.ToString() + " mines" + "\n\n";
                serverState.AllMessages.Add(message);
                Message message2 = new Message();
                message2.Type = "Minefield";
                message2.Event = minefield.Key;
                message2.Audience = minefield.Empire;
                message2.Text = "Someone has destroyed " + minesToDestroy.ToString() + " mines in your minefield at " + minefield.Position.ToString() + "\n\n";
                serverState.AllMessages.Add(message);
            }
        }

    }
}
