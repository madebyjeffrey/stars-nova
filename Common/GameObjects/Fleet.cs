#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010, 2011 The Stars-Nova Project
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

namespace Nova.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using Nova.Common.Waypoints;
    using Nova.Common.Commands;


    /// <summary>
    /// Fleet class. A fleet is a container for one or more ships (which may be of
    /// different designs). Ship instances do not exist by themselves, they are
    /// always part of a fleet (even if they are the only ship in the fleet).
    /// A fleet may be a starbase.
    /// </summary>
    [Serializable]
    public class Fleet : Mappable
    {   
        /// <summary>
        /// Holds the ship tokens in the format "ShipDesign, Quantity, Damage%".
        /// </summary>
        private Dictionary<long, ShipToken> tokens  = new Dictionary<long, ShipToken>();
        public List<Waypoint> Waypoints   = new List<Waypoint>();

        /// <summary>
        /// The cargo carried by the entire fleet. 
        /// To avoid issues with duplication cargo is tracked at the fleet level only.
        /// </summary>
        public Cargo Cargo = new Cargo(); 
        
        public Mappable InOrbit = null;
        public double Bearing = 0;
        public double Cloaked = 0;
        public double FuelAvailable = 0;
        public double TargetDistance = 100;
        public string BattlePlan = "Default";
        public int maxPopulation = 1000000; //TODO when Race.HasTrait = "AR" starbases have different max populations
        public int TurnYear = -1;  // If Fleet.Name = "Salvage" then decrease the cargo every year for three years then destroy it
        public enum TravelStatus 
        { 
            Arrived, InTransit 
        }

        /// <summary>
        /// Return the total normal bombing capability.
        /// </summary>
        public Bomb BombCapability
        {
            get
            {
                Bomb totalBombs = new Bomb(); // TODO priority 4 verify if this method of combining Smart and Conventional Bombs is the same as Stars!
                foreach (ShipToken token in tokens.Values)
                {
                    Bomb bombC = token.Design.BombCapabilityConventional * token.Quantity;
                    Bomb bombS = token.Design.BombCapabilityConventional * token.Quantity;
                    totalBombs.PopKill += bombC.PopKill;
                    totalBombs.Installations += bombC.Installations;
                    totalBombs.MinimumKill += bombC.MinimumKill;
                    totalBombs.PopKill += bombS.PopKill;
                    totalBombs.Installations += bombS.Installations;
                    totalBombs.MinimumKill += bombS.MinimumKill;
                }
                return totalBombs;
            }
        }

        /// <summary>
        /// Check if any of the ships has colonization module.
        /// </summary>
        public bool CanColonize
        {
            get
            {
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.CanColonize)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Property to determine if a fleet can re-fuel.
        /// </summary>
        public bool CanRefuel
        {
            get
            {
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.CanRefuel)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// This property is true if the fleet has at least one ship with a scanner.
        /// </summary>
        public bool CanScan
        {
            get
            {
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.CanScan)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Return the composition of a fleet (ship design and number of ships of that
        /// design).
        /// </summary>
        public Dictionary<long, ShipToken> Composition
        {
            get
            {
                return tokens;
            }
        }


        /// <summary>
        /// Return Free Warp speed for fleet.
        /// </summary>
        public int FreeWarpSpeed
        {
            get
            {
                int speed = 10;
                foreach (ShipToken token in tokens.Values)
                {
                    speed = Math.Min(speed, token.Design.FreeWarpSpeed);
                }

                return speed;
            }
        }
        public int HealsOthersPercent
        {
            get
            {
                int heals = 0;
                foreach (ShipToken token in tokens.Values)
                {
                    heals = Math.Max(heals, token.Design.HealsOthersPercent);
                }

                return heals;
            }
        }


        public int SlowestEngine
        {
            get
            {
                int speed = 10;
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.Engine == null) return 0;
                    speed = Math.Min(speed, token.Design.Engine.OptimalSpeed);
                }

                return speed;
            }
        }







        /// <summary>
        /// Determine if the fleet has bombers.
        /// </summary>
        public bool HasBombers
        {
            get
            {
                bool bombers = false;
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.IsBomber)
                    {
                        bombers = true;
                    }
                }
                return bombers;
            }
        }

        /// <summary>
        /// Choose an image from one of the ships in the fleet.
        /// </summary>
        public ShipIcon Icon
        {
            get
            {
                try
                {
                    ShipToken token = tokens.Values.First();
                    return token.Design.Icon;
                }
                catch
                {
                    Report.Error("Fleet.cs Fleet.Icon (get): unable to get ship image.");
                }
                return null;
            }
        }

        /// <summary>
        /// Report if a fleet is armed.
        /// </summary>
        public bool IsArmed
        {
            get
            {
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.HasWeapons)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Property to determine if a fleet is a starbase.
        /// </summary>
        public bool IsStarbase
        {
            get
            {
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.IsStarbase)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        /// <summary>
        /// Return the mass of a fleet.
        /// </summary>
        public int Mass
        {
            get
            {
                int totalMass = 0;

                foreach (ShipToken token in tokens.Values)
                {
                    totalMass += token.Design.Mass * token.Quantity;
                }
                totalMass += Cargo.Mass;

                return totalMass;
            }
        }

        /// <summary>
        /// Return the the number of mines this fleet can lay.
        /// </summary>
        public int NumberOfMines
        {
            get
            {
                int mineCount = 0;

                foreach (ShipToken token in tokens.Values)
                {
                    mineCount += token.Design.MineCount * token.Quantity;
                }

                return mineCount;
            }
        }
        
        /// <summary>
        /// Return the penetrating range scan capability of the fleet.
        /// FIXME (priority 4) - scanning capability can be additive (but the formula is non-linear).
        /// </summary>
        public int PenScanRange
        {
            get
            {
                int penRange = 0;
                
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.ScanRangePenetrating > penRange)
                    {
                        penRange = token.Design.ScanRangePenetrating;
                    }
                }
                return penRange;
            }
        }
        
        /// <summary>
        /// Return the non penetrating range scan capability of the fleet.
        /// FIXME (priority 4) - scanning capability can be additive (but the formula is non-linear).
        /// </summary>
        public int ScanRange
        {
            get
            {
                int scanRange = 0;
                
                foreach (ShipToken token in tokens.Values)
                {
                    if (token.Design.ScanRangeNormal > scanRange)
                    {
                        scanRange = token.Design.ScanRangeNormal;
                    }
                }
                return scanRange;
            }
        }

        /// <summary>
        /// Return the current speed of the fleet.
        /// </summary>
        public int Speed
        {
            get
            {
                if (Waypoints.Count == 0) return 0;
                Waypoint target = Waypoints[0];
                return target.WarpFactor;
            }

            set
            {
                if (Waypoints.Count > 0)
                {
                    Waypoint target = Waypoints[0];
                    target.WarpFactor = 0;
                }
            }
        }


        /// <summary>
        /// Return the current total amour strength of the fleet.
        /// </summary>
        public double TotalArmorStrength
        {
            get
            {
                return tokens.Values.Sum(token => token.Armor);  // note: token.Armour is a total so no need to * by quantity
            }
        }

        /// <summary>
        /// Find the total cargo capacity of the fleet.
        /// </summary>
        public int TotalCargoCapacity
        {
            get
            {
                return tokens.Values.Sum(token => token.Design.CargoCapacity * token.Quantity);
            }
        }

        /// <summary>
        /// Return the cost of a fleet. 
        /// </summary>
        public Resources TotalCost
        {
            get
            {
                Resources cost = new Resources();

                foreach (ShipToken token in tokens.Values)
                {
                    cost += token.Design.Cost;
                }

                return cost;
            }
        }

        /// <summary>
        /// Find the total dock capacity of the fleet.
        /// </summary>
        public int TotalDockCapacity
        {
            get
            {
                return tokens.Values.Sum(token => token.Design.DockCapacity);
            }
        }

        /// <summary>
        /// Find the total fuel capacity of all ships in the fleet.
        /// </summary>
        public int TotalFuelCapacity
        {
            get
            {
                return tokens.Values.Sum(token => token.Design.FuelCapacity * token.Quantity);
            }
        }

        /// <summary>
        /// Return the total shield strength of the fleet.
        /// </summary>
        public double TotalShieldStrength
        {
            get
            {
                return tokens.Values.Sum(token => token.Shields);  // note token.Shields is a total so no need to multiply by quantity
            }
        }

        
        /// <summary>
        /// Placeholder constructor - Fleet should be replaced by a reference to the fleet with the same Key.
        /// </summary>
        public Fleet(long newKey) 
        { 
            Key = newKey; 
        }

        
        /// <summary>
        /// Fleet construction for unit testing and stack creation during a battle.
        /// </summary>
        /// <param name="name">The fleet name.</param>
        /// <param name="id">The fleet id.</param>
        /// <param name="position">The fleet position.</param>
        public Fleet(string name, ushort owner, uint id, NovaPoint position)
        {
            Name = name;
            Owner = owner;
            Id = id;
            Position = position;
        }
        
        public Fleet(Fleet copy)
            : base(copy)
        {
        }
        
        /// <summary>
        /// Fleet construction based on a ShipToken and some parameters from a star (this is
        /// the usual case for most fleets when a new ship is manufactured at a star).
        /// </summary>
        /// <param name="ship">The ShipToken being constructed.</param>
        /// <param name="star">The star constructing the ship.</param>
        public Fleet(ShipToken token, Star star, long newKey)
        {
            tokens.Add(token.Key, token);

            FuelAvailable = TotalFuelCapacity;
            Type          = ItemType.Fleet;

            // Have one waypoint to reflect the fleet's current position and the
            // planet it is in orbit around.
         
            Waypoint w    = new Waypoint();      
            w.Position    = star.Position;
            w.Destination = star.Name;
            w.WarpFactor  = 0;

            Waypoints.Add(w);

            // Inititialise the fleet elements that come from the star.

            Position     = star.Position;       
            InOrbit      = star;                
            Key          = newKey;    
        }
        
        
        /// <summary>
        /// Fleet construction based on a ship and some parameters from a star (this is
        /// the usual case for most fleets when a new ship is manufactured at a star).
        /// </summary>
        /// <param name="ship">The ship being constructed.</param>
        /// <param name="star">The star constructing the ship.</param>
        public Fleet(ShipDesign design, int quantity, Star star, long newKey) :
            this(new ShipToken(design, quantity), star, newKey)
        {
        }


        public TravelStatus GetTravelStatus()
        {
            Waypoint target = Waypoints[0];
            if (Position.distanceToSquared(target.Position) < 2)
            {
                return TravelStatus.Arrived;
            }
            else
            {
                return TravelStatus.InTransit;
            }
        }

       ///
/// Calculates the point of interception for one object starting at point
/// <code>a</code> with speed vector <code>v</code> and another object
///  starting at point <code>b</code> with a speed of <code>s</code>.
/// 
/// @see <a
///      href="http://jaran.de/goodbits/2011/07/17/calculating-an-intercept-course-to-a-target-with-constant-direction-and-velocity-in-a-2-dimensional-plane/">Calculating
///      an intercept course to a target with constant direction and velocity
///      (in a 2-dimensional plane)</a>
/// 
/// @param a
//            start vector of the object to be intercepted
/// @param v
///            speed vector of the object to be intercepted
/// @param b
///            start vector of the intercepting object
/// @param s
///            speed of the intercepting object
/// @return vector of interception or <code>null</code> if object cannot be
///         intercepted or calculation fails
/// 
/// @author Jens Seiler
///
        public NovaPoint calculateInterceptionPoint(NovaPoint a, NovaPoint v,  NovaPoint b,  double s)
        {
             double ox = a.X - b.X;
             double oy = a.Y - b.Y;

             double h1 = v.X * v.X + v.Y * v.Y - s * s;
             double h2 = ox * v.X + oy * v.Y;
            double t;
            if (h1 == 0)
            { // problem collapses into a simple linear equation 
                t = -(ox * ox + oy * oy) / (2 * h2);
            }
            else
            { // solve the quadratic equation
                double minusPHalf = -h2 / h1;

                 double discriminant = minusPHalf * minusPHalf - (ox * ox + oy * oy) / h1; // term in brackets is h3
                if (discriminant < 0)
                { // no (real) solution then...
                    return null;
                }

                 double root = Math.Sqrt(discriminant);

                 double t1 = minusPHalf + root;
                 double t2 = minusPHalf - root;

                 double tMin = Math.Min(t1, t2);
                 double tMax = Math.Max(t1, t2);

                t = tMin > 0 ? tMin : tMax; // get the smaller of the two times, unless it's negative
                if (t < 0)
                { // we don't want a solution in the past
                    return null;
                }
            }

            // calculate the point of interception using the found intercept time and return it
            return new NovaPoint((int)(a.X + t * v.X),(int) (a.Y + t * v.Y));
        }

        /// <summary>
        /// Move the fleet towards the waypoint at the top of the list. Fuel is consumed
        /// at the rate of the sum of each of the individual ships (i.e. available fuel
        /// is automatically "pooled" between the ships).
        /// in Stars! a fleet will reach a planet that is 36.99 light years away in one year when travelling at warp 6! 100.99 LY at warp 10
        /// </summary>
        /// <param name="availableTime">The portion of a year left for travel.</param>
        /// <param name="race">The race this fleet belongs to.</param>
        /// <returns>A TravelStatus indicating arrival or in-transit.</returns>
        public TravelStatus Move(ref double availableTime, Race race, ref List<Message> messages,int targetVelocity,NovaPoint targetVelocityVector)
        {
            if (GetTravelStatus() == TravelStatus.Arrived)
            {
                return TravelStatus.Arrived;
            }

            Waypoint target = Waypoints[0];

            InOrbit = null;
            // move the lamb and wolf at the same time, at intercept the wolf's travel time equals the lamb's travel time, assume Wolf Follows an Intercept course
            // Twolf = Tlamb
            // also Twolf * Twolf = Tlamb * Tlamb
            // and Twolf = DistWolf / VelocityWolf
            // and Twolf = Sqrt(deltaXwolf * deltaXwolf + deltaYwolf * deltaYwolf) / (Wolf.WarpFactor * Wolf.WarpFactor)
            // and Tlamb =  Sqrt(deltaXlamb * deltaXlamb + deltaYlamb * deltaYlamb) / (Lamb.WarpFactor * Lamb.WarpFactor)
            // and at intercept Wolf.Xintercept = Lamb.Xintercept and Wolf.Yintercept = Lamb.Yintercept and Wolf.PositionIntercept = Lamb.PositionIntercept
            // and deltaXLamb = Lamb.X0 + Tlamb * VelocityX
            // and deltaYLamb = Lamb.Y0 + Tlamb * VelocityY
            // and deltaXWolf = Wolf.X0 + Twolf * VelocityX
            // and deltaYWolf = Wolf.Y0 + Twolf * VelocityY
            // give me a minute - the last time I intersected two spaceships using math was 44 years ago
            //
            //so Sqrt(deltaXwolf * deltaXwolf + deltaYwolf * deltaYwolf) / (Wolf.WarpFactor * Wolf.WarpFactor) = Sqrt(deltaXlamb * deltaXlamb + deltaYlamb * deltaYlamb) / (Lamb.WarpFactor * Lamb.WarpFactor)
            //
            // Sqrt((Wolf.X0 + Twolf * VelocityX) * (Wolf.X0 + Twolf * VelocityX) + (Wolf.Y0 + Twolf * VelocityY) * (Wolf.Y0 + Twolf * VelocityY)) / (Wolf.WarpFactor * Wolf.WarpFactor) = Sqrt((Lamb.X0 + Tlamb * VelocityX) * (Lamb.X0 + Tlamb * VelocityX) + (Lamb.Y0 + Tlamb * VelocityY) * (Lamb.Y0 + Tlamb * VelocityY)) / (Lamb.WarpFactor * Lamb.WarpFactor)
            // now just add the relationship between VelocityX VelocityY and warpFactor and velocotyVector then solve it
            // Much easier to Google it - see calculateInterceptionPoint
            // then we need to perform the battle at that position!
            // TODO priority 5 stop the Lamb at the intercept point and do the battle there 
            // for now just go to where the lamb will be at the end of it's move and wait for it to arrive  
            NovaPoint targetPosition = new NovaPoint(target.Position);
            if (targetVelocity != 0 ) // if the target is moving
            {
                NovaPoint oneYearsTargetTravel = targetVelocityVector.BattleSpeedVector(targetVelocity); // just normalises the direction vector then multiplies it by the speed to get a speed vector
                targetPosition += oneYearsTargetTravel;
            }
            double legDistance = PointUtilities.Distance(Position, targetPosition); 

            int warpFactor = target.WarpFactor;
            int speed = warpFactor * warpFactor;
            double speedStars270j = warpFactor * warpFactor + 1 -1.0/(Double)int.MaxValue;
            double targetTime = legDistance / speed;
            double targetTimeStars270j = legDistance / speedStars270j;
            double fuelConsumptionRate = FuelConsumption(warpFactor, race);
            if (warpFactor == 1) fuelConsumptionRate = -1; // From observation of millions of Fleets in Stars! 2.70j
            double fuelTime = FuelAvailable / fuelConsumptionRate;
            double travelTime = targetTime;

            // Determine just how long we have available to travel towards the
            // waypoint target. This will be the smaller of target time (the ideal
            // case, we get there) available time (didn't get there but still can
            // move towards there next turn) and fuel time.

            TravelStatus arrived = TravelStatus.Arrived;

            if (targetTimeStars270j > availableTime)  // Corporate HQ will not pay an extra days wages for 2.5% of a days travel so orders are to nudge the speed a bit and fire the retro rockets a little bit closer to the star and as hard as the passengers can handle :)
            {                                           // in Stars! 2.70j a 36.88 LY journey to a star will be completed in 1 turn at warp 6 - i assume it will also travel 36.99 LY, it does not travel 37.0 LY
                travelTime = availableTime;             // this rule only applies if the journey is less than 37LY - on longer journeys 36LY is subtracted per year but the last year of a long journey could be 36.99LY travelled in one year
                arrived = TravelStatus.InTransit;       // the  A.I. (or player) could exploit this by splitting the journey into a lot of small 36.9999999 LY journeys but that is what Stars! does.
            }

            if (travelTime > fuelTime)
            {
                travelTime = fuelTime;
                arrived = TravelStatus.InTransit;
            }
            
            // If we have arrived then the new fleet position is the waypoint
            // target. Otherwise the position is determined by how far we got
            // in the time or fuel available.

            if (arrived == TravelStatus.Arrived)
            {
                Position = target.Position;
                target.WarpFactor = 0;
            }
            else
            {
                double travelled = speed * travelTime;
                Position = PointUtilities.MoveTo(Position, target.Position, travelled);
            }

            // Update the travel time left for this year and the total fuel we
            // now have available.

            availableTime -= travelTime;
            int fuelUsed = (int)(fuelConsumptionRate * travelTime);
            if ((TotalFuelCapacity/FuelAvailable > 2) && (fuelUsed < 0))
            {
                Message message = new Message();
                message.Audience = Owner;
                message.Text = "Fleet " + Name + "has generated " + fuelUsed.ToString() +"mg of fuel.";
                message.Type = "WarpToChange";
                message.Event = this;
                messages.Add(message);
            }
            FuelAvailable -= fuelUsed;

            // Added check if fleet run out of full it's speed will be changed 
            // to free warp speed.
            if (arrived == TravelStatus.InTransit && fuelConsumptionRate > this.FuelAvailable)
            {
                target.WarpFactor = this.FreeWarpSpeed;
                if (target.WarpFactor == 0) target.WarpFactor = 1; // in Stars every fleet can travel at warp 1 and generate 1mg of fuel per turn
                Message message = new Message();
                message.Audience = Owner;
                message.Text = "Fleet " + Name + "has run out of fuel. Its speed has been reduced to Warp " + this.FreeWarpSpeed.ToString() + ".";
                message.Type = "WarpToChange";
                message.FleetID = this.Id;
                message.Event = this;
                messages.Add(message);

            }
            return arrived;
        }


        /// <summary>
        /// Return the fuel consumption (mg per year) of the fleet at the specified
        /// warp factor.
        /// </summary>
        /// <param name="warpFactor">The warp speed of the fleet.</param>
        /// <param name="race">The race this fleet belongs too.</param>
        /// <returns>The rate of fuel consumption in mg / year.</returns>
        public double FuelConsumption(int warpFactor, Race race)
        {
            double fuelConsumption = 0;

            // Work out how full of cargo the fleet is.
            double cargoFullness;
            if (TotalCargoCapacity == 0)
            {
                cargoFullness = 0;
            }
            else
            {
                cargoFullness = ((double)Cargo.Mass) / ((double)TotalCargoCapacity);
            }


            foreach (ShipToken token in tokens.Values)
            {
                fuelConsumption += token.Design.FuelConsumption(warpFactor, race, (int)(token.Design.CargoCapacity * cargoFullness));
            }

            return fuelConsumption;
        }
        /// <summary>
        /// Return the fuel consumption (mg per year) of the fleet at the specified
        /// warp factor if it were full.
        /// </summary>
        /// <param name="warpFactor">The warp speed of the fleet.</param>
        /// <param name="race">The race this fleet belongs too.</param>
        /// <returns>The rate of fuel consumption in mg / year.</returns>
        public double FuelConsumptionWhenFull(int warpFactor, Race race)
        {
            double fuelConsumption = 0;

            // Work out how full of cargo the fleet is.
            double cargoFullness;
            if (TotalCargoCapacity == 0)
            {
                cargoFullness = 0;
            }
            else
            {
                cargoFullness = 1.0;
            }


            foreach (ShipToken token in tokens.Values)
            {
                fuelConsumption += token.Design.FuelConsumption(warpFactor, race, (int)(token.Design.CargoCapacity * cargoFullness));
            }

            return fuelConsumption;
        }

        /// <summary>
        /// returns true if the fleet can reach the destination with the current cargo
        /// 
        /// </summary>
        public bool canCurrentlyReach(Star destination, Race race)
        {
            double destinationDistance = this.distanceTo(destination);
            double yearsOfTravel = destinationDistance / (this.SlowestEngine * this.SlowestEngine);
            double fuelRequired = this.FuelConsumption(this.SlowestEngine, race) * yearsOfTravel;
            return (this.FuelAvailable > fuelRequired);
        }
        /// <summary>
        /// returns true if the fleet can reach the destination when fully loaded
        /// 
        /// </summary>
        public bool canReach(Star destination, Race race)
        {
            double destinationDistance = this.distanceTo(destination);
            double yearsOfTravel = destinationDistance / (this.SlowestEngine * this.SlowestEngine);
            double fuelRequired = this.FuelConsumptionWhenFull(this.SlowestEngine, race) * yearsOfTravel;
            return (this.FuelAvailable > fuelRequired);
        }


        /// <summary>
        /// loads the requested population into the fleet and send it to the target
        /// 
        /// </summary>
        public WaypointCommand LoadWaypoint(Star source, int PopulationKT )
        {
            // load up
            CargoTask wpTask = new CargoTask();
            wpTask.Mode = CargoMode.Load;
            wpTask.Target = this.InOrbit;
            wpTask.Amount.ColonistsInKilotons = PopulationKT;
            wpTask.Amount.Germanium = PopulationKT;

            Waypoint wp = new Waypoint();
            wp.Task = wpTask;
            wp.Position = this.InOrbit.Position;
            wp.WarpFactor = this.SlowestEngine;
            wp.Destination = source.ToString();

            WaypointCommand loadCommand = new WaypointCommand(CommandMode.Add, wp, this.Key);
            return loadCommand;
        }


        /// <summary>
        /// returns true if the fleet can reach the destination when fully loaded
        /// 
        /// </summary>
        public bool canReach(StarIntel destination, Race race)
        {
            double destinationDistance = this.distanceTo(destination);
            double yearsOfTravel = destinationDistance / (this.SlowestEngine * this.SlowestEngine);
            double fuelRequired = this.FuelConsumptionWhenFull(this.SlowestEngine, race) * yearsOfTravel;
            return (this.FuelAvailable > fuelRequired);
        }

        /// <summary>
        /// Calculate the fuel required for this fleet to reach a given destination.
        /// </summary>
        /// <param name="warpFactor">The warp speed to travel at.</param>
        /// <param name="race">The race operating the fleet.</param>
        /// <param name="dest">The destination as a <see cref="NovaPoint"/>.</param>
        /// <returns>The estimated fuel consumption.</returns>
        /// <remarks>
        /// FIXME (priority 4) - probably has rounding errors.
        /// FIXME (priority 3) - should this account for final year slow down?.
        /// </remarks>
        public int GetFuelRequired(int warpFactor, Race race, NovaPoint dest)
        {
            double fuelConsumption = FuelConsumption(warpFactor, race);
            double time = PointUtilities.DistanceSquare(this.Position, dest) / (warpFactor * warpFactor * warpFactor * warpFactor);
            return (int)(time * fuelConsumption);
        }

        /// <summary>
        /// Load: initializing constructor to load a fleet from an XmlNode (save file).
        /// </summary>
        /// <param name="node">An XmlNode representing the fleet.</param>
        public Fleet(XmlNode node)
            : base(node)
        {
            // Read the node
            XmlNode mainNode = node.FirstChild;
            try
            {
                while (mainNode != null)
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "fleetid":
                            Id = uint.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "cargo":
                            Cargo = new Cargo(mainNode);
                            break;
                        case "inorbit":
                            InOrbit = new Star();
                            InOrbit.Name = mainNode.FirstChild.Value;
                            break;
                        case "bearing":
                            Bearing = double.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "cloaked":
                            Cloaked = double.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "fuelavailable":
                            FuelAvailable = double.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "targetdistance":
                            TargetDistance = double.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "battleplan":
                            BattlePlan = mainNode.FirstChild.Value;
                            break;
                        case "tokens":
                            XmlNode subNode = mainNode.FirstChild;
                            ShipToken token;
                            while (subNode != null)
                            {
                                token = new ShipToken(subNode);
                                tokens.Add(token.Key, token);
                                subNode = subNode.NextSibling;
                            }
                            break;
                        case "waypoint":
                            Waypoint waypoint = new Waypoint(mainNode);
                            Waypoints.Add(waypoint);
                            break;
                        case "turnyear":
                            TurnYear = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;

                        default: break;
                    }


                    mainNode = mainNode.NextSibling;
                }
            }
            catch (Exception e)
            {
                Report.Error("Error loading fleet:" + Environment.NewLine + e.Message);
                throw e;
            }
        }


        /// <summary>
        /// Save: Return an XmlElement representation of the Fleet.
        /// </summary>
        /// <param name="xmldoc">The parent xml document.</param>
        /// <returns>An XmlElement representation of the Fleet.</returns>
        public new XmlElement ToXml(XmlDocument xmldoc, string nodeName = "Fleet")
        {
            XmlElement xmlelFleet = xmldoc.CreateElement(nodeName);

            xmlelFleet.AppendChild(base.ToXml(xmldoc));
            
            if (InOrbit != null)
            {
                Global.SaveData(xmldoc, xmlelFleet, "InOrbit", InOrbit.Name);
            }

            Global.SaveData(xmldoc, xmlelFleet, "Bearing", this.Bearing.ToString(System.Globalization.CultureInfo.InvariantCulture));
            
            if (Cloaked != 0)
            {
                Global.SaveData(xmldoc, xmlelFleet, "Cloaked", this.Cloaked.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            
            Global.SaveData(xmldoc, xmlelFleet, "FuelAvailable", this.FuelAvailable.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelFleet, "FuelCapacity", this.TotalFuelCapacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelFleet, "TargetDistance", this.TargetDistance.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (TurnYear > 0)
            {
                Global.SaveData(xmldoc, xmlelFleet, "TurnYear", this.TurnYear.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (Cargo.Mass > 0)
            {
                Global.SaveData(xmldoc, xmlelFleet, "CargoCapacity", this.TotalCargoCapacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            
            Global.SaveData(xmldoc, xmlelFleet, "BattlePlan", this.BattlePlan);

            xmlelFleet.AppendChild(Cargo.ToXml(xmldoc));

            foreach (Waypoint waypoint in Waypoints)
            {
                xmlelFleet.AppendChild(waypoint.ToXml(xmldoc));
            }

            XmlElement xmlelTokens = xmldoc.CreateElement("Tokens");
            foreach (ShipToken token in tokens.Values)
            {
                xmlelTokens.AppendChild(token.ToXml(xmldoc));
            }            
            xmlelFleet.AppendChild(xmlelTokens);

            return xmlelFleet;
        }
        
        public FleetIntel GenerateReport(ScanLevel scan, int year)
        {
            FleetIntel report = new FleetIntel(this, scan, year);
            
            return report;
        }
        public double maxDistance(Race race)
        {
            double distancePerYear = (this.SlowestEngine * this.SlowestEngine);
            double fuelPerYear = FuelConsumptionWhenFull(SlowestEngine, race);
            return (FuelAvailable * fuelPerYear * distancePerYear);
        }

    }
}
