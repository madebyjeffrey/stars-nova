#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project
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
    using System.IO;
    using System.Reflection;
    
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using Nova.Common.Waypoints;
    
    using Nova.Server.TurnSteps;

    /// <summary>
    /// Class to process a new turn.
    /// </summary>
    public class TurnGenerator
    {
        private ServerData serverState;        
        private SortedList<int, ITurnStep> turnSteps;
        private Random rand;
        
        // Used to order turn steps.
        private const int FIRSTSTEP = 00;
        private const int STARSTEP = 12;
        private const int BOMBINGSTEP = 19;
        private const int COLONISESTEP = 92;
        private const int SCANSTEP = 99;
        
        // TODO: (priority 5) refactor all these into ITurnStep(s).
        private OrderReader orderReader;
        private IntelWriter intelWriter;
        private BattleEngine battleEngine;
        private RonBattleEngine ronBattleEngine;
        private CheckForMinefields checkForMinefields;
        private Manufacture manufacture;
        private Scores scores;
        private VictoryCheck victoryCheck;
        
        /// <summary>
        /// Construct a turn processor. 
        /// </summary>
        public TurnGenerator(ServerData serverState)
        {
            this.serverState = serverState;            
            turnSteps = new SortedList<int, ITurnStep>();
            rand = new Random();
            
            // Now that there is a state, compose the turn processor.
            // TODO ??? (priority 4): Use dependency injection for this? It would
            // generate a HUGE constructor call... a factory to
            // abstract it perhaps? -Aeglos
            orderReader = new OrderReader(this.serverState);
            if (GameSettings.Data.UseRonBattleEngine) ronBattleEngine = new RonBattleEngine(this.serverState, new List <BattleReport>());
            else battleEngine = new BattleEngine(this.serverState, new List<BattleReport>());

//            bombing = new Bombing(ref this.serverState);
            checkForMinefields = new CheckForMinefields(this.serverState);
            manufacture = new Manufacture(this.serverState);
            scores = new Scores(this.serverState);
            intelWriter = new IntelWriter(this.serverState, this.scores);
            victoryCheck = new VictoryCheck(this.serverState, this.scores);
            
            turnSteps.Add(SCANSTEP, new ScanStep());
            turnSteps.Add(BOMBINGSTEP, new BombingStep());  //Do the ColoniseStep after the BombingStep !
            turnSteps.Add(COLONISESTEP, new PostBombingStep()); //Do the ColoniseStep after the BombingStep !
            turnSteps.Add(STARSTEP, new StarUpdateStep());  // In Stars! i often use (say) ten SplitCommands to get the Colony Fleets ID larger than the bombing fleet ID so the 
                                                            // Colony Fleets Colonise Commands are processed after the bombing fleet bombs the planet
                                                            // I can't change the bomber fleets ID in Stars! or it does not do bombing that turn
                                                            // it seems that in Stars! the bombing fleet needs to be a bombing fleet at the target Position for an entire turn
                                                            // before it will carry out any bombing, so moving bombers to a new fleet forfeits their turn
        }
        
        /// <summary>
        /// Generate a new turn by reading in the player turn files to update the master
        /// copy of stars, ships, etc. Then do the processing required to take in the
        /// passage of one year of time and, finally, write out the new turn file.
        /// </summary>
        public void Generate()
        {
            ProgressDialog ServerProgress = new ProgressDialog();
            ServerProgress.Text = "Backup Turn";
            ServerProgress.Show();
            ServerProgress.Name = "Server";
            BackupTurn();
            ServerProgress.Text = "Read Orders";





            // For now, just copy the command stacks right away.
            // TODO (priority 6): Integrity check the new turn before
            // updating the state (cheats, errors).
            ReadOrders();

            // for all commands of all empires: command.ApplyToState(empire);
            // for WaypointCommand: Add Waypoints to Fleets.
            ServerProgress.Text = "Waypoint Zero Commands";
            ParseCommands();  //PreProcess CargoTask and Splitmerge tasks because the Fleet ID's may be deleted before the waypoint0 tasks are completed or may not exist until after waypoint0 tasks are performed

            // Do all fleet movement and actions 
            // TODO (priority 4) - split this up into waypoint zero and waypoint 1 actions

            //Stars! Split functionality splits the fleets at the current position before moving!!
            //the Waypoint Zero commands were applied to the EmpireState during the ParseCommands()
            //but their waypoints have not been removed yet so do that now:

            ServerProgress.Text = "Cleanup Waypoint Zero Commands";
            serverState.AllMessages.AddRange(new SplitFleetStep().Process(serverState)); // Remove spent cargo and splitmerge waypoints

            ServerProgress.Text = "Lay Mines";
            serverState.AllMessages.AddRange( new FirstStep().Process(serverState));
            // ToDo: Step 1 --> Scrap Fleet if waypoint 0 order; here, and only here.
            // ToDo: ScrapFleetStep / foreach ITurnStep for waypoint 0. Own TurnStep-List for Waypoint 0?

            ServerProgress.Text = "Scrap Fleets";
            serverState.AllMessages.AddRange(new ScrapFleetStep().Process(serverState));

            ServerProgress.Text = "Move Fleets";
            List<Fleet> destroyed = new List<Fleet>();
            foreach (Fleet fleet in serverState.IterateAllFleets())
            {
                if ((fleet.Name != "Mineral Packet") && (!fleet.IsStarbase)) if (ProcessFleet(fleet)) destroyed.Add(fleet); // ToDo: don't scrap fleets here at waypoint 1
            }

            List<long> AllFleetIds = new List<long>();
            foreach (long fleetId in serverState.IterateAllFleetKeys()) AllFleetIds.Add(fleetId);
            foreach (long fleetId in AllFleetIds)
            {
                int empire = (int) ((long)fleetId >> 32);
                Fleet fleet = serverState.AllEmpires[empire].OwnedFleets[fleetId];
                if (fleet.Name != "Mineral Packet")   checkForMinefields.Check(fleet); 
            }
            serverState.CleanupFleets();

            // remove battle from old turns
            foreach (EmpireData empire in serverState.AllEmpires.Values)
            {
                empire.BattleReports.Clear();
            }

            ServerProgress.Text = "Run Battle Engine";
            if (GameSettings.Data.UseRonBattleEngine ) ronBattleEngine.Run();
            else battleEngine.Run();

            serverState.CleanupFleets();

            victoryCheck.Victor();

            serverState.TurnYear++;
            
            foreach (EmpireData empire in serverState.AllEmpires.Values)
            {
                empire.TurnYear = serverState.TurnYear;
                empire.TurnSubmitted = false;
            }
                       
            foreach (ITurnStep turnStep in turnSteps.Values)
            {
                List<Message> messages = turnStep.Process(serverState);
                if (messages != null) foreach (Message message in messages) serverState.AllMessages.Add(message);
            }

            ServerProgress.Text = "Move Mineral Packets";
            foreach (Fleet fleet in serverState.IterateAllFleets())  // Move Mineral Packets after they are created
            {
                if (fleet.Name.Contains("Mineral Packet"))
                {
                    ProcessFleet(fleet);
                    serverState.SetFleetOrbit(fleet);
                    if (fleet.InOrbit != null)
                    {
                        serverState.AllMessages.Add(new Message(fleet.Owner, "Your Mineral Packet Destroyed 3/4 of the population of " + fleet.InOrbit.Name.ToString(), "Star", ((Star)fleet.InOrbit), 0));
                        serverState.AllMessages.Add(new Message(((Star)fleet.InOrbit).Owner, "A Mineral Packet Destroyed 3/4 of your population on " + fleet.InOrbit.Name.ToString(), "Star", ((Star)fleet.InOrbit), 0));
                        ((Star)fleet.InOrbit).Colonists = ((Star)fleet.InOrbit).Colonists / 4;
                    }
                    serverState.AllEmpires[fleet.Owner].FleetReports[fleet.Key].Update(fleet, ScanLevel.Owned, serverState.TurnYear);
                }
            }
            serverState.CleanupFleets();

            ServerProgress.Text = "Record entities Visible to Radar";

            foreach (EmpireData empire in serverState.AllEmpires.Values)
            {
                foreach (StarIntel star in empire.StarReports.Values) star.HasFleetsInOrbit = star.HasFreeTransportInOrbit = star.HasRefuelerInOrbit = false;
               
                foreach (Fleet fleet in empire.OwnedFleets.Values)

                    if ((fleet.InOrbit != null) && (!fleet.IsStarbase))
                    {
                        this.serverState.AllEmpires[empire.Id].StarReports[fleet.InOrbit.Name].HasFleetsInOrbit = true;
                        if (fleet.Name.Contains(Global.AiRefueler)) this.serverState.AllEmpires[empire.Id].StarReports[fleet.InOrbit.Name].HasRefuelerInOrbit = true;
                        if (fleet.Name.Contains(Global.AiFreighter) && (fleet.Waypoints.Count < 2)) this.serverState.AllEmpires[empire.Id].StarReports[fleet.InOrbit.Name].HasFreeTransportInOrbit = true;
                    }
            }



            foreach (EmpireData empire in serverState.AllEmpires.Values)
            {


                // -------------------------------------------------------------------
                // (1) First the easy one. Minefields owned by the player.
                // -------------------------------------------------------------------

                foreach (Minefield minefield in serverState.AllMinefields.Values)
                {
                    if (minefield.Empire == empire.Id)
                    {
                        empire.VisibleMinefields[minefield.Key] = minefield;
                    }
                }

                // -------------------------------------------------------------------
                // (2) Not so easy. Minefields within the scanning range of the
                // player's ships.
                // -------------------------------------------------------------------

                foreach (Fleet fleet in empire.OwnedFleets.Values)
                {
                    foreach (Minefield minefield in serverState.AllMinefields.Values)
                    {
                        bool isIn = PointUtilities.CirclesOverlap(
                            fleet.Position,
                            minefield.Position,
                            fleet.ScanRange(empire.Race,empire),
                            minefield.Radius);

                        if (isIn == true)
                        {
                            empire.VisibleMinefields[minefield.Key] = minefield;
                        }
                    }
                }

                // -------------------------------------------------------------------
                // (3) Now that we know how to deal with ship scanners planet scanners
                // are just the same.
                // -------------------------------------------------------------------

                foreach (Minefield minefield in serverState.AllMinefields.Values)
                {
                    foreach (Star report in empire.OwnedStars.Values)
                    {
                        if (report.Owner == empire.Id)
                        {
                            bool isIn = PointUtilities.CirclesOverlap(
                                report.Position,
                                minefield.Position,
                                report.ScanRange,
                                minefield.Radius);

                            if (isIn == true)
                            {
                                empire.VisibleMinefields[minefield.Key] = minefield;
                            }
                        }
                    }
                }
            }



            ServerProgress.Text = "Write Stuff Out";

            WriteIntel();

            // remove old messages, do this last so that the 1st turn intro message is not removed before it is delivered.
            serverState.AllMessages = new List<Message>();
            
            CleanupOrders();
            ServerProgress.Close();
        }


        protected virtual void WriteIntel()
        {
            intelWriter.WriteIntel();
        }

        protected virtual void ReadOrders()
        {
            orderReader.ReadOrders();
        }

        /// <summary>
        /// Validates and applies all commands sent by the clients and read for this turn.
        /// </summary>
        protected virtual void ParseCommands()
        {
            foreach (EmpireData empire in serverState.AllEmpires.Values)
            {
                if (serverState.AllCommands.ContainsKey(empire.Id))
                {                
                    while (serverState.AllCommands[empire.Id].Count > 0) 
                    {
                        ICommand command = serverState.AllCommands[empire.Id].Pop();
                        Message message = null; 
                        if (command.IsValid(empire, out message))
                        {
                            if (null != message) serverState.AllMessages.Add(message);
                            if (command is WaypointCommand)
                            {
                                if (((command as WaypointCommand).Mode == CommandMode.Add) || ((command as WaypointCommand).Mode == CommandMode.Edit) || ((command as WaypointCommand).Mode == CommandMode.Insert))
                                {
                                    if ((command as WaypointCommand).Waypoint.Task is CargoTask) (command as WaypointCommand).PreApplyToState(empire, ((command as WaypointCommand).Waypoint.Task as CargoTask).Target);
                                    else if ((command as WaypointCommand).Waypoint.Task is SplitMergeTask) (command as WaypointCommand).PreApplyToState(empire, null);
                                    else command.ApplyToState(empire);
                                    serverState.CleanupFleets();  
                                }
                                else (command as WaypointCommand).ApplyToState(empire); // might be a waypoint delete or edit so keep the Waypoint indexes alligned between server's and the client's Waypoint Lists
                            }
                            else
                            {
                                message = command.ApplyToState(empire);
                                if (null != message) serverState.AllMessages.Add(message);
                                message = null;
                            }
                        }
                        else
                        {
                            if (null != message)
                            {
                                serverState.AllMessages.Add(message);
                                if (Global.Debug) Report.Information(message.Text);
                            }
                            message = new Message(empire.Id, "Invalid " + command.GetType().Name + "command for " + empire.Race.Name, "Invalid Command", null);
                            serverState.AllMessages.Add(message);
                            if (null != message) serverState.AllMessages.Add(message);
                        }

                    }
                }
                serverState.CleanupFleets();

                foreach (Star star in empire.OwnedStars.Values)
                {
                    serverState.AllStars[star.Key] = star;
                }
            }
        }
        
        /// <summary>
        /// Delete order files, done after turn generation.
        /// </summary>
        protected virtual void CleanupOrders()
        {
            // Delete orders on turn generation.
            // Copy each file into it’s new directory.
            DirectoryInfo source = new DirectoryInfo(serverState.GameFolder);
            foreach (FileInfo fi in source.GetFiles())
            {
                if (fi.Name.ToLower().EndsWith(Global.OrdersExtension))
                {
                    File.Delete(fi.FullName);
                }
            }
        }

        /// <summary>
        /// Copy all turn files to a sub-directory prior to generating the new turn.
        /// </summary>
        protected virtual void BackupTurn()
        {
            // TODO (priority 3) - Add a setting to control the number of backups.
            int currentTurn = serverState.TurnYear;
            string gameFolder = serverState.GameFolder;


            try
            {
                string backupFolder = Path.Combine(gameFolder, currentTurn.ToString());
                DirectoryInfo source = new DirectoryInfo(gameFolder);
                DirectoryInfo target = new DirectoryInfo(backupFolder);

                // Check if the target directory exists, if not, create it.
                if (Directory.Exists(target.FullName) == false)
                {
                    Directory.CreateDirectory(target.FullName);
                }

                // Copy each file into it’s new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                }
            }
            catch (Exception e)
            {
                Report.Error("There was a problem backing up the game files: " + Environment.NewLine + e.Message);
            }
        }


        /// <summary>
        /// Process the elapse of one year (turn) for a fleet.
        /// </summary>
        /// <param name="fleet">The fleet to process a turn for.</param>
        /// <returns>True if the fleet was destroyed.</returns>
        private bool ProcessFleet(Fleet fleet)
        {
            if (fleet == null)
            {
                return true;
            }
                            /////////////////////
            bool destroyed = UpdateFleet(fleet);
                            /////////////////////
                            
            if (destroyed == true)
            {
                return true;
            }

            // refuel/repair
            RegenerateFleet(fleet);

            // Check for no fuel.

            //if (fleet.FuelAvailable == 0 && !fleet.IsStarbase)
            //{
            //    Message message = new Message();
            //    message.Audience = fleet.Owner;
            //    message.Type = "Fuel";
            //    message.Text = fleet.Name + " has run out of fuel.";
            //    message.FleetID = fleet.Id;
            //    serverState.AllMessages.Add(message);
            //}

            return false;
        }

        /// <summary>
        /// Refuel and Repair.
        /// </summary>
        /// <param name="fleet"></param>
        /// <remarks>
        /// To refuel a ship must be in orbit of a planet with a starbase with a dock capacity > 0.
        /// Repair is:
        /// 0% while bombing (or orbiting an enemy planet with attack orders).
        /// 1% moving through space
        /// 2% stopped in space
        /// 3% orbiting, but not bombing an enemy planet
        /// 5% orbiting own planet without a starbase.
        /// 8% orbiting own planet with starbase but 0 dock.
        /// 20 orbiting own planet with dock.
        /// +repair% if stopped or orbiting.
        /// Stopped or orbiting, with at least one Fuel Transport hull in the fleet additional 5%
        /// Stopped or orbiting, with at least one Super Fuel Xport hull in the fleet additional 10%

        /// TODO (priority 3) - A starbase is not counted towards repairs if it is under attack. 
        /// TODO (priority 3) - reference where these rules are from.
        /// </remarks>
        private void RegenerateFleet(Fleet fleet)
        {
            if (fleet == null)
            {
                return;
            }
            
            Star star = null;
            
            if (fleet.InOrbit != null)
            {
                star = serverState.AllStars[fleet.InOrbit.Name];
            }

            // refuel
            if (star != null && star.Owner == fleet.Owner /* TODO (priority 6) or friendly*/ && star.Starbase != null && star.Starbase.CanRefuel)
            {
                fleet.FuelAvailable = fleet.TotalFuelCapacity;
            }

            // repair, TODO (priority 3) skip if fleet has no damage, if that is more efficient 

            int repairRate = 0;
            if (star != null)
            {
                if (star.Owner == fleet.Owner /* TODO (priority 6) or friend */)
                {
                    if (star.Starbase != null /* TODO (priority 6) and not under attack */)
                    {
                        if (star.Starbase.CanRefuel)
                        {
                            // orbiting own planet with dock.
                            repairRate = 20;
                            repairRate = repairRate + fleet.HealsOthersPercent;
                        }
                        else
                        {
                            // orbiting own planet with starbase but 0 dock.
                            repairRate = 8;
                            repairRate = repairRate + fleet.HealsOthersPercent;
                        }
                    }
                    else
                    {
                        // friendly planet, no base
                        repairRate = 5;
                        repairRate = repairRate + fleet.HealsOthersPercent;
                    }
                }
                else
                {
                    // TODO (priority 6) 0% if bombing
                    // orbiting, but not bombing an enemy planet
                    repairRate = 3;
                }
            }
            else
            {
                // TODO (priority 4) - check if a stopped fleet has 1 or 0 waypoints
                if (fleet.Waypoints.Count == 0)
                {
                    // stopped in space
                    repairRate = 2;
                    repairRate = repairRate + fleet.HealsOthersPercent;
                }
                else
                {
                    // moving through space
                    repairRate = 1;
                }
            }
            // repair ships/tokens
            foreach (ShipToken token in fleet.Composition.Values)
            {
                token.Shields = token.Design.Shield * token.Quantity; // note: token.Sheild is for all ships in the token
                if (repairRate > 0)
                {
                    // note: token.Armor is for all ships in the token
                    int repairAmount = Math.Max(token.Design.Armor * token.Quantity * repairRate / 100, 1);
                    token.Armor += repairAmount;
                    token.Armor = Math.Min(token.Armor, token.Design.Armor * token.Quantity);
                }
            }
        }

        /// <summary>
        /// Update the status of a fleet moving through waypoints and performing any
        /// specified waypoint tasks.
        /// </summary>
        /// <param name="fleet"></param>
        /// <returns>Always returns false.</returns>
        private bool UpdateFleet(Fleet fleet)
        {
            Race race = serverState.AllEmpires[fleet.Owner].Race;
            if (fleet.Waypoints.Count > 0)
            {
                Waypoint firstWaypoint = fleet.Waypoints[0];
                Waypoint currentPosition = null;
                currentPosition = fleet.Waypoints[0];

                double availableTime = 1.0;
                while ((fleet.Waypoints.Count > 0) && (fleet.Waypoints[0].Task is NoTask) && (currentPosition.Position == fleet.Waypoints[0].Position)) fleet.Waypoints.RemoveAt(0);// Remove any useless waypoints at the start of the waypoint list (should only be one useless one)
                if ((fleet.Waypoints.Count > 0) && (currentPosition != fleet.Waypoints[0]))  //Don't throw away colonise or scrap tasks or invade

                {
                    Waypoint waypointZero = fleet.Waypoints[0];
                    Fleet.TravelStatus fleetMoveResult;

                    // -------------------
                    // Move
                    // -------------------

                    // Check for Cheap Engines failing to start
                    if (waypointZero.WarpFactor > 6 && race.Traits.Contains("CE") && rand.Next(10) == 1 && !fleet.Name.Contains("Mineral Packet"))
                    {
                        // Engines fail
                        Message message = new Message();
                        message.Audience = fleet.Owner;
                        message.Text = "Fleet " + fleet.Name + "'s engines failed to start. Fleet has not moved this turn.";
                        message.Type = "Cheap Engines";
                        message.Event = fleet;
                        serverState.AllMessages.Add(message);
                        fleetMoveResult = Fleet.TravelStatus.InTransit;
                    }
                    else
                    {
                        Fleet dest = null;
                        int targetVelocity = 0;
                        NovaPoint targetVelocityVector = new NovaPoint(0, 0);
                        foreach (Fleet target in serverState.IterateAllFleets())
                            if (target.Name == waypointZero.Destination)
                            {
                                dest = target;
                                targetVelocity = target.Waypoints[0].WarpFactor * target.Waypoints[0].WarpFactor;
                                targetVelocityVector = target.Waypoints[0].Position - target.Position;
                                continue;
                            }
                        List < Message > messages = new List<Message>();
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                        fleetMoveResult = fleet.Move(ref availableTime, race, out messages, targetVelocity, targetVelocityVector);
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////
                        serverState.AllMessages.AddRange(messages);
                    }


                    if (fleetMoveResult == Fleet.TravelStatus.InTransit)
                    {
                        Waypoint newCurrentPosition = new Waypoint();
                        newCurrentPosition.Position = fleet.Position;
                        newCurrentPosition.Destination = "Space at " + fleet.Position.ToString();
                        newCurrentPosition.Task = new NoTask();
                        fleet.Waypoints.Insert(0, newCurrentPosition);
                        fleet.InOrbit = null;
                    }
                    else
                    {
                        // Arrived
                        EmpireData sender = serverState.AllEmpires[fleet.Owner];
                        EmpireData reciever = null;
                        Star target = null;

                        serverState.AllStars.TryGetValue(waypointZero.Destination, out target);

                        if (target == null) // the long search - we might have followed a fleet here and we had to calculate their expected position at the end of turn so some rounding error is possible
                        {
                            foreach (Star star in serverState.AllStars.Values)
                            {
                                if (star.Position.distanceToSquared(waypointZero.Position) < 1.4143)
                                {
                                    target = star;
                                }
                            }
                        }
                        if (target != null)
                        {
                            fleet.InOrbit = target;
                            fleet.Waypoints[0].Position = target.Position;
                            fleet.Waypoints[0].Destination = target.Name;
                            serverState.AllEmpires.TryGetValue(target.Owner, out reciever);
                            if (availableTime != 1.0) availableTime = 0; //In Stars! the fleet looses the rest of the turn as it enters orbit and scans the Star (And the crew check out the local pub)
                                                                         // We could add a scan-and-go to use up available time but we would need to scan each star visited this year by this Fleet! (and not allow the crew off the ship)
                                                                         // That would require creating a list of each star visited and passing that list to the ScanStep, for now it is easier to just stop the fleet at the first star visited.
                                                                         // There could also need to be a list of battles at every planet visited during this turn, the battles would need to be resolved during the move step ?
                        }
                        else
                        {
                            fleet.Waypoints[0].Position = fleet.Position;
                            fleet.InOrbit = null;
                        }

                        // -------------------------
                        // Waypoint 1 Tasks
                        // -------------------------

                        if ((!(waypointZero.Task is ColoniseTask)) && (!(waypointZero.Task is ScrapTask)))//ScrapTask is after the battle - not sure why - that's just how Stars! did it

                        {//Don't try to colonise before the BombStep or the user might do dozens of extra SplitMergeTasks to get the coloniseTask to be performed after the bombing task
                            Message message;
                            if (waypointZero.Task.IsValid(fleet, target, sender, reciever, out message))
                            {
                                if (null != message) serverState.AllMessages.Add(message);
                                currentPosition = fleet.Waypoints[0];
                                ///////////////////////////////////////////////////////////////////////
                                waypointZero.Task.Perform(fleet, target, sender, reciever, out message);
                                ///////////////////////////////////////////////////////////////////////
                                if (null != message) serverState.AllMessages.Add(message); 
                                currentPosition.Task = new NoTask();
                                currentPosition.Position = fleet.Position;
                                fleet.Waypoints.Insert(0, currentPosition);
                            }
                            else if (null != message) serverState.AllMessages.Add(message);
                        }
                        try
                        {
                            serverState.AllMessages.AddRange(waypointZero.Task.Messages);
                        }
                        catch
                        {
                            if (Global.Debug) Report.Information("Bad waypoint for " + fleet.Name + " Empire " + fleet.Owner.ToString());
                        }



                    }

                }

                if (fleet.Waypoints.Count == 0)
                {
                    fleet.Waypoints.Add(firstWaypoint);
                }
                if (fleet.Waypoints.Count > 1)
                {
                    Waypoint nextWaypoint = fleet.Waypoints[1];

                    double dx = fleet.Position.X - nextWaypoint.Position.X;
                    double dy = fleet.Position.Y - nextWaypoint.Position.Y;
                    fleet.Bearing = ((Math.Atan2(dy, dx) * 180) / Math.PI) + 90;
                }
            }
            // ??? (priority 4) - why does this always return false. - if warp speed too high then the fleet may vanish
            return false; //TODO destroy fleets that exceed warp speed limits 
        }
        
        /// <summary>
        /// This is a utility function. Sets intel for the first turn.
        /// </summary>
        public void AssembleEmpireData()
        {
            // Generates initial reports.
            ITurnStep firstStep = new FirstStep();
            serverState.AllMessages.AddRange( firstStep.Process(serverState));
            ITurnStep scanStep = new ScanStep();
            serverState.AllMessages.AddRange(scanStep.Process(serverState));
        }
    }
}
