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
    using System.Drawing;
    using System.Linq;

    using Nova.Common;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;
    using Nova.Server;

    /// <summary>
    /// Deal with combat between races.
    /// </summary>
    public class RonBattleEngine
    {
        private readonly Random random = new Random();
        private readonly int maxBattleRounds = 80;
        private readonly int gridSize = 1000;
        private readonly int gridScale = 100; //MUST BE gridSize /10
        private readonly int gridScaleSquared = 10000; // 

        private ServerData serverState;
        private List <BattleReport> battles;

        /// <summary>
        /// Used to generate fleet id numbers for battle stacks.
        /// </summary>
        private uint stackId;

        private int battleRound = 0;

        /// <summary>
        /// Creates a new battle engine.
        /// </summary>
        /// <param name="serverState">
        /// A <see cref="ServerState"/> which holds the state of the game.
        /// </param>
        /// <param name="battleReport">
        /// A <see cref="BattleReport"/> onto which to write the battle results.
        /// </param>
        public RonBattleEngine(ServerData serverState,List  <BattleReport> BattleReports)
        {
            this.serverState = serverState;
            this.battles = BattleReports;
        }

        /// <summary>
        /// Deal with any fleet battles. How the battle engine in Stars! works is
        /// documented in the Stars! FAQ (a copy is included in the documentation)
        /// however fleet 1 has a tremendous advantage and the range of weapons
        /// is largely ignored as the fleets move too far in one battlestep
        /// A nimble ship with long range weapons has almost no advantage in stars 
        /// because fleets usually move from outside weapon range of the best ships
        /// to well within weapon range of the worst ship instantaneously    :(
        /// Why bother with ship design at all ?
        /// </summary>
        public void Run()
        {
            // Determine the positions of any potential battles. For a battle to
            // take place 2 or more fleets must be within weapon range.

            List<List<Fleet>> potentialBattles = DetermineWeaponRangeFleets();

            // If there are no fleets within weapon range then there are no fleets at all
            // so there is nothing more to do so we can give up here.

            if (potentialBattles.Count == 0)
            {
                return;
            }

            // Eliminate potential battle locations where there is only one race
            // present.

            List<List<Fleet>> engagements = EliminateSingleRaces(potentialBattles);

            // Again this could result in an empty array. If so, give up here.

            if (engagements.Count == 0)
            {
                return;
            }
            // We now have a list of every collection of fleets of more than one
            // race at the same location. Run through each possible combat zone,
            // build the fleet stacks and invoke the battle at each location
            // between any enemies.

            foreach (List<Fleet> battlingFleets in engagements)
            {
                List<NovaPoint> previousBattleLocations = new List<NovaPoint>();
                foreach (Fleet fleet in battlingFleets)
                {
                    if ((fleet != null) && (fleet.Composition.Count > 0))
                    {
                        NovaPoint battleLocation = fleet.Position;
                        if (!previousBattleLocations.Contains(battleLocation))
                        {
                            BattleReport battle = new BattleReport();
                            previousBattleLocations.Add(battleLocation);
                            List<Stack> battlingStacks = GenerateStacks(battlingFleets, battleLocation);

                            // If no targets get selected (for whatever reason) then there is
                            // no battle so we can give up here.

                            if (SelectTargets(battlingStacks) == 0)
                            {
                                return;
                            }



                            stackId = 0;
                            Fleet sample = battlingFleets.First() as Fleet;

                            if (sample.InOrbit != null)
                            {
                                battle.Location = sample.InOrbit.Name;
                            }
                            else
                            {
                                battle.Location = "coordinates " + sample.Position.Scale((Double)(1.0 / gridScale)).ToString();
                            }

                            PositionStacks(battlingStacks, battle);

                            // Copy the full list of stacks into the battle report. We need a
                            // full list to start with as the list in the battle engine will
                            // get depleted during the battle and may not (and most likely will
                            // not) be fully populated by the time we Serialize the
                            // report. Ensure we take a copy at this point as the "real" stack
                            // may mutate as processing proceeds and even ships may vanish.
                            battle.Stacks.Clear();
                            foreach (Stack stack in battlingStacks)
                            {
                                //stack.Token.Design.Update();
                                battle.Stacks[stack.Key] = new Stack(stack);

                            }

                            DoBattle(battlingStacks, battle);

                            ReportBattle(battle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine the positions of any potential battles where the number of fleets
        /// is more than one (this scan could be more efficient but this is easier to
        /// read). 
        /// </summary>
        /// <returns>A list of all lists of weapon range fleets.</returns>
        public List<List<Fleet>> DetermineWeaponRangeFleets()
        {
            List<List<Fleet>> allWeaponRangeFleets = new List<List<Fleet>>();
            Dictionary<long, bool> fleetDone = new Dictionary<long, bool>();

            foreach (Fleet fleetA in serverState.IterateAllFleets())
            {
                if (fleetA.Name == "S A L V A G E")  fleetDone[fleetA.Key] = true;
                if (fleetDone.ContainsKey(fleetA.Key))
                {
                    continue;
                }

                List<Fleet> weaponRangeFleets = new List<Fleet>();

                foreach (Fleet fleetB in serverState.IterateAllFleets())
                {
                    if (fleetB.Name == "S A L V A G E") continue;
                    if (fleetB.Position != fleetA.Position)
                    {
                        continue;
                    }

                    weaponRangeFleets.Add(fleetB);
                    fleetDone[fleetB.Key] = true;
                }

                if (weaponRangeFleets.Count > 1)
                {
                    allWeaponRangeFleets.Add(weaponRangeFleets);
                }
            }

            return allWeaponRangeFleets;
        }

        /// <summary>
        /// Eliminate single race groupings. Note that we know there must be at least
        /// two fleets when we determined co-located fleets earlier.
        /// </summary>
        /// <param name="fleetPositions">A list of all lists of co-located fleets.</param>
        /// <returns>The positions of all potential battles.</returns>
        public List<List<Fleet>> EliminateSingleRaces(List<List<Fleet>> allColocatedFleets)
        {
            List<List<Fleet>> allEngagements = new List<List<Fleet>>();

            foreach (List<Fleet> coLocatedFleets in allColocatedFleets)
            {
                Dictionary<int, bool> empires = new Dictionary<int, bool>();

                foreach (Fleet fleet in coLocatedFleets)
                {
                    empires[fleet.Owner] = true;
                }

                if (empires.Count > 1)
                {
                    allEngagements.Add(coLocatedFleets);
                }
            }
            return allEngagements;
        }

        /// <summary>
        /// Extract a list of stacks from a fleet; each ship design present on
        /// the fleet will form a distinct stack. If multiple fleets are present there may be multiple
        /// stacks of the same design present at the battle. (Or if stack ship limits are exceeded
        /// by a single fleet).
        /// </summary>
        /// <param name="fleet">The <see cref="Fleet"/> to be converted to stacks.</param>
        /// <returns>A list of stacks extracted from the fleet.</returns>
        public List<Stack> BuildFleetStacks(Fleet fleet)
        {
            List<Stack> stackList = new List<Stack>();

            Stack newStack = null;

            foreach (ShipToken token in fleet.Composition.Values)
            {
                newStack = new Stack(fleet, stackId, token);

                stackList.Add(newStack);

                stackId++;
            }

            // Note that each of this Stacks has it's Key UNIQUE within this battle (separate from the
            // Fleet's key),
            // consisting of Owner + stackId. The token inside is still Keyed by design.Key.
            return stackList;
        }

        /// <summary>
        /// Run through all of the fleets in an engagement and convert them to stacks of the
        /// same ship design and battle plan. We will return a complete list of all stacks at
        /// this engagement location.
        /// </summary>
        /// <param name="coLocatedFleets">A list of fleets at the given location.</param>
        /// <returns>A list of Fleets representing all stack in the engagement (1 fleet per unique stack).</returns>
        public List<Stack> GenerateStacks(List<Fleet> coLocatedFleets,NovaPoint battleLocation)
        {
            List<Stack> battlingStacks = new List<Stack>();

            foreach (Fleet fleet in coLocatedFleets)
            {
                if ((battleLocation.distanceToSquared(fleet.Position) <= 2) && (fleet.Composition.Count > 0))
                {
                    List<Stack> fleetStacks = BuildFleetStacks(fleet);

                    foreach (Stack stack in fleetStacks)
                    {
                        battlingStacks.Add(stack);
                    }
                }
            }

            return battlingStacks;
        }

        /// <summary>
        /// Set the initial position of all of the stacks.
        /// </summary>
        /// <param name="battlingStacks">All stacks in this battle.</param>
        public void PositionStacks(List<Stack> battlingStacks,BattleReport battle)
        {
            Dictionary<int, int> empires = new Dictionary<int, int>();
            Dictionary<int, Point> racePositions = new Dictionary<int, Point>();

            foreach (Stack stack in battlingStacks)
            {
                empires[stack.Owner] = stack.Owner;
                stack.Token.Design.Update();
            }

            SpaceAllocator spaceAllocator = new SpaceAllocator(empires.Count);

            // Ensure that we allocate enough space so that all race stacks are
            // out of weapons range (scaled).

            int spaceSize = spaceAllocator.GridAxisCount * Global.MaxWeaponRange;

            // spaceAllocator.AllocateSpace(spaceSize);
            spaceAllocator.AllocateSpace(gridSize); // Set to the standard Stars!Nova battle board size 
            battle.SpaceSize = spaceSize;
            battle.GridSize = gridScale;

            // Now allocate a position for each race in the centre of one of the
            // allocated spacial chunks.
            int index = 0;
            foreach (int empireId in empires.Values)
            {
                Rectangle newPosition = spaceAllocator.GetBox(index,empires.Values.Count);
                Point position = new Point();

                position.X = newPosition.X + (newPosition.Width / 2);
                position.Y = newPosition.Y + (newPosition.Height / 2);

                racePositions[empireId] = position;
                battle.Losses[empireId] = 0;
                index++;
            }

            // Place all stacks belonging to the same race at the same position.

            foreach (Stack stack in battlingStacks)
            {
                stack.Position = racePositions[stack.Owner];
            }

            // Update the known designs of enemy ships.
            foreach (int empireId in empires.Values)
            {
                foreach (Stack stack in battlingStacks)
                {
                    if (stack.Owner != empireId)
                    {
                        foreach (ShipToken token in stack.Composition.Values)
                        {
                            if (serverState.AllEmpires[empireId].EmpireReports[stack.Owner].Designs.ContainsKey(token.Design.Key))
                            {
                                serverState.AllEmpires[empireId].EmpireReports[stack.Owner].Designs[token.Design.Key] = token.Design;
                            }
                            else
                            {
                                serverState.AllEmpires[empireId].EmpireReports[stack.Owner].Designs.Add(token.Design.Key, token.Design);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deal with a battle. This function will execute until all target fleets are
        /// destroyed or a pre-set maximum time has elapsed.   
        /// </summary>
        /// <param name="battlingStacks">All stacks in this battle.</param>
        public void DoBattle(List<Stack> battlingStacks,BattleReport battle)
        {
            battleRound = 1;
            for (battleRound = 1; battleRound <= maxBattleRounds; ++battleRound)
            {
                if (SelectTargets(battlingStacks) == 0)
                {
                    // no more targets
                    break;
                }

                MoveStacks(battlingStacks,battleRound,battle);

                List<WeaponDetails> allAttacks = GenerateAttacks(battlingStacks);

                foreach (WeaponDetails attack in allAttacks)
                {
                    ProcessAttack(attack,battle);
                }
            }
        }

        public struct TargetRow
        {
            public TargetRow(Stack fleet,int priority, double attractiveness)
            {
                Fleet = fleet;
                Priority = priority;
                Attractiveness = attractiveness;
            }
            
            public Stack Fleet { get; }
            public int Priority { get; }
            public double Attractiveness { get; }
        }
        /// <summary>
        /// Select targets (if any). Targets are set on a stack-by-stack basis.
        /// </summary>
        /// <param name="battlingStacks">All stacks in this battle.</param>
        /// <returns>The number of targeted stacks.</returns>
        public int SelectTargets(List<Stack> battlingStacks)
        {
            List<TargetRow> selectedTargets = new List<TargetRow>();

            int numberOfTargets = 0;

            foreach (Stack wolf in battlingStacks)
            {
                if (wolf.Composition.Count > 0) // if not destroyed
                {
                    wolf.Target = null;

                    //if (wolf.IsArmed == false)  // Unarmed ships need to look for nearby enemy armed ships and run away from them so the need to target them
                    //{
                    //    continue;
                    //}
                    bool haveIcremented = false;
                    foreach (Stack lamb in battlingStacks)
                    {
                        if (lamb.Composition.Count > 0) //if lamb not destroyed
                        {
                            if ((AreEnemies(wolf, lamb)) && (lamb.Token.Armor > 0))
                            {
                                double attractiveness;
                                int priority = GetPriority(lamb, wolf);
                                if (wolf.IsArmed) attractiveness = GetAttractiveness(lamb);
                                else attractiveness = Math.Abs(1000.0 / (wolf.Position.distanceToSquared(lamb.Position) + 1)); // move away from closest armed
                                selectedTargets.Add(new TargetRow(lamb, priority, attractiveness));
                                if (!haveIcremented)
                                {
                                    haveIcremented = true;
                                    numberOfTargets++;
                                }
                            }
                        }
                    }
                    wolf.Target = null;
                    wolf.TargetList = new List<Stack>();
                    if (selectedTargets.Count > 0)
                    {
                        System.Collections.Generic.IComparer<TargetRow> targetComparer = new TargetComparer();
                        selectedTargets.Sort(targetComparer);
                        wolf.Target = selectedTargets[selectedTargets.Count - 1].Fleet; // Why is the last one the best target - seems awkward
                        foreach (TargetRow row in selectedTargets) wolf.TargetList.Add(row.Fleet);
                        selectedTargets.Clear();
                    }
                }
            }
            return numberOfTargets;
        }
        public class TargetComparer : System.Collections.Generic.IComparer<TargetRow>
        {
            // Compares the priority and attractivness of two targets.
            public int Compare(TargetRow A, TargetRow B)
            {

                if (A.Priority > B.Priority) return 1;
                if (A.Priority < B.Priority) return -1;
                if ((A.Priority == B.Priority) && (A.Attractiveness > B.Attractiveness)) return 1;
                if ((A.Priority == B.Priority) && (A.Attractiveness < B.Attractiveness)) return -1;
                if ((A.Priority == B.Priority) && (A.Attractiveness == B.Attractiveness)) return 0;
                else return 0; //I don't know how it could get here
            }
        }
        /// <summary>
        /// Determine how well fleet matches tactic
        /// </summary>
        /// <param name="target">A stack.</param>
        /// <returns>A measure of attractiveness.</returns>
        /// 
        public int GetPriority(Stack target, Stack source)
        {
            if (target == null || target.IsDestroyed)
            {
                return 0;
            }

            
            
            BattlePlan planA = serverState.AllEmpires[source.Owner].BattlePlans[source.BattlePlan];
            if (source.IsArmed)
            {
                if (planA == null) return 0;

                if (targetMathchesTargetPriority(planA.PrimaryTarget, target)) return 7;
                if (targetMathchesTargetPriority(planA.SecondaryTarget, target)) return 6;
                if (targetMathchesTargetPriority(planA.TertiaryTarget, target)) return 5;
                if (targetMathchesTargetPriority(planA.QuaternaryTarget, target)) return 4;
                if (targetMathchesTargetPriority(planA.QuinaryTarget, target)) return 3;
                return 0;
            }
            else if (target.IsArmed && !target.IsStarbase) return 7;
            else return 0;
        }

        public bool targetMathchesTargetPriority(int targetPriority,Stack target)
        {
            if (target.IsStarbase && targetPriority == (int)Global.Victims.Starbase) return true;
            if (target.HasBombers && targetPriority == (int)Global.Victims.Bomber) return true;
            if ((target.Token.Design.PowerRating > 2000) && target.IsArmed && targetPriority == (int)Global.Victims.CapitalShip) return true;
            if ((target.Token.Design.PowerRating < 2000) && target.IsArmed && targetPriority == (int)Global.Victims.Escort) return true;
            if (target.IsArmed && targetPriority == (int)Global.Victims.ArmedShip) return true;
            if (targetPriority == (int)Global.Victims.AnyShip) return true;
            if (!target.IsArmed && targetPriority == (int)Global.Victims.SupportShip) return true;

            else return false;
        }

        /// <summary>
        /// Determine how attractive a fleet is to attack.
        /// </summary>
        /// <param name="target">A stack.</param>
        /// <returns>A measure of attractiveness.</returns>
        /// FIXME (priority 3) - Implement the Stars! attractiveness model (and possibly others as options). Provide a reference to the source of the algorithm.
        public double GetAttractiveness(Stack target)
        {
            if (target == null || target.IsDestroyed)
            {
                return 0;
            }

            double cost = target.Mass + target.TotalCost.Energy;
            double dp = target.Defenses;

            return cost / dp;
        }

        /// <summary>
        /// Determine if one stack is a potential target of the other. This depends not
        /// just on the relation (friend, enemy, etc.) but also on the battle plan of
        /// the "wolf" stack (e.g. attack everyone, attack enemies, etc.).
        /// </summary>
        /// <param name="wolf">Potential attacker.</param>
        /// <param name="lamb">Potential target.</param>
        /// <returns>True if lamb is a valid target for wolf.</returns>
        public bool AreEnemies(Fleet wolf, Fleet lamb)
        {
            if (wolf.Owner == lamb.Owner)
            {
                return false;
            }

            EmpireData wolfData = serverState.AllEmpires[wolf.Owner];
            PlayerRelation lambRelation = wolfData.EmpireReports[lamb.Owner].Relation;

            BattlePlan battlePlan = wolfData.BattlePlans[wolf.BattlePlan];

            if (battlePlan.Attack == "Everyone")
            {
                return true;
            }
            else if (battlePlan.TargetId == lamb.Owner)
            {
                return true;
            }
            else if (battlePlan.Attack == "Enemies" && lambRelation == PlayerRelation.Enemy)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move stacks towards their targets (if any). Record each movement in the
        /// battle report.
        /// </summary>
        /// <param name="battlingStacks">All stacks in the battle.</param>
        public void MoveStacks(List<Stack> battlingStacks,int battleRound, BattleReport battle)
        {
            // Fleets initially advance until their opponents are within weapon range  
            // then (depending on BattlePlan) withdraw to keep out of their opponents weapon range.
            // once your opponent is within weapon range you do not move a further 
            // 2 squares towards them and let them move a further 3 squares towards
            // you before you fire.




            foreach (Stack stack in battlingStacks)
            {
                if ((stack != null) && (stack.Composition.Count > 0))
                {
                    if ((stack.Target != null) && (!stack.IsStarbase))
                    {
                        NovaPoint vectorToTarget = stack.Target.Position - stack.Position;
                        NovaPoint newHeading;
                        newHeading = vectorToTarget.BattleSpeedVector(stack.BattleSpeed * gridScale);
                        if ((!stack.Token.Design.HasWeapons) || (battleRound < 5)) // unarmed ships are curious for first 5 rounds and move around
                            if (battleRound < 5)
                            {
                                Random random = new Random();
                                int interestingHeading = random.Next(1, 3);
                                switch (interestingHeading)
                                {
                                    case 1:
                                        newHeading = newHeading.Scale(1.0);
                                        break;
                                    case 2:
                                        newHeading.turnAsFastAsPossible(newHeading.Scale(-1), newHeading);
                                        break;
                                    case 3: break;
                                }
                            }
                            else if (stack.distanceTo(stack.Target) / gridScale < Global.MaxWeaponRange) newHeading = newHeading.Scale(-1.0);  // armed enemy is getting close - run away
                            else newHeading = new NovaPoint(0, 0);  //we are unarmed so don't do anything more unless an enemy approaches
                        if (stack.VelocityVector == null) stack.VelocityVector = newHeading;
                        if (stack.Target.VelocityVector == null) stack.Target.VelocityVector = new NovaPoint(0, 0); //when we calculate the move for the target this will be calculated - first time is unknown
                        if ((stack.Target.IsStarbase) && (vectorToTarget.lengthSquared() < stack.VelocityVector.lengthSquared()))
                        {
                            stack.VelocityVector = vectorToTarget; // This will put us on top of the target so ignore other logic
                            newHeading = vectorToTarget; // travelling at less than BattleSpeed !!
                        }
                        else
                        {
                            bool collisionDetected = ((vectorToTarget + stack.Target.VelocityVector - newHeading).lengthSquared() <
                                (vectorToTarget + stack.Target.VelocityVector - newHeading.Scale(2)).lengthSquared());
                            if (collisionDetected) stack.VelocityVector = stack.VelocityVector.prepareForFlyby(stack.VelocityVector, newHeading);
                            else
                            {
                                double turnAngle = newHeading.angleBetween(stack.VelocityVector, newHeading);
                                if (turnAngle > 90) // target just passed us do a "turnAsFastAsPossible"
                                {
                                    stack.VelocityVector.turnAsFastAsPossible(stack.VelocityVector, newHeading);
                                }
                                else stack.VelocityVector = newHeading;
                            }
                        }
                        NovaPoint oldPosition = stack.Position;
                        stack.Position = stack.Position + newHeading;


                        // Update the battle report with these movements.
                        BattleStepMovement report = new BattleStepMovement();
                        report.StackKey = stack.Key;
                        report.Position = stack.Position;
                        if (oldPosition != stack.Position) battle.Steps.Add(report);

                    }
                }
            }
                
           
        }


        /// <summary>
        /// Quote from Stars! P L A Y E R S    G U I D E 
        //If you are using beam weapons and the damage your token can inflict on an
        //enemy’s token is more than enough to destroy the enemy token, the
        //remainder is used on additional enemy tokens in the same location, limited
        //only by the number of ships in the attacking token.

        /// </summary>
        /// <param name="battlingStacks">All stacks in the battle.</param>
    private List<WeaponDetails> GenerateAttacks(List<Stack> battlingStacks)
        {
            // First, identify all of the weapons and their characteristics for
            // every ship stack present at the battle and who they are pointed at.

            List<WeaponDetails> allAttacks = new List<WeaponDetails>();

            foreach (Stack stack in battlingStacks)
            {
                if ( ! stack.IsDestroyed) 
                {
                    /// Quote from Stars! P L A Y E R S    G U I D E 
                    //Damage is applied as follows: If the damage applied to a token’s armor
                    //exceeds the remaining armor of one or more of the ships in the token, then
                    //those ships are destroyed. Any remaining damage is spread over the ENTIRE
                    //token, with the damage being divided up equally among the remaining ships.
                    foreach (Weapon weaponSystem in stack.Token.Design.Weapons)
                    {

                        int percentToFire = 100;
                        int percentFired = 0;
                        int targetIndex = 0;
                        Double distanceSquared;

                        WeaponDetails weapon = new WeaponDetails();
                        weapon.SourceStack = stack;
                        //stack.Target = stack.TargetList[0];
                        bool doOverkill = false; // First pass do just enough damage to probably kill if spare damage left over hit everything some more

                        while ((percentFired < 100) && (targetIndex < stack.TargetList.Count)) //falls through if not enough targets
                        {
                            Stack stackTarget = stack.TargetList[targetIndex];
                            weapon.Weapon = weaponSystem;
                            distanceSquared = stack.Position.distanceToSquared(stackTarget.Position);
                            if ((weapon.Weapon.Range * weapon.Weapon.Range * gridScaleSquared> distanceSquared) && (stackTarget.TotalArmorStrength > 0))
                                //maybe we have 300 ships in this stack and the opponent has 300 stacks of 1 ship
                            {//TODO calculate 3 standard deviations if we have 2700 missiles firing with 85% chance of success
                                //TODO remember probability math I learned 40 years ago
                                if (weapon.Weapon.IsMissile)
                                {// budget for 115% damage to target to allow for rounding errors and 3 standard deviations of the probability distribution
                                    percentToFire = (int)(115.0 * ((double)(stackTarget.TotalShieldStrength + stackTarget.TotalArmorStrength)) / (weapon.missileAccuracy(stack.Token.Design, stackTarget.Token.Design,weapon.Weapon.Accuracy/100.0) * weapon.Weapon.Power * stack.Token.Quantity / 10.0));// /10.0 because we only fire 10% max per turn with RonBattleEngine
                                    if (doOverkill)
                                    {
                                        //enough firepower to kill everything and we had some leftover so send rest of attack against primary target 
                                        percentToFire = 100; 
                                    }
                                    if (percentToFire > 100) percentToFire = 100;
                                    if (percentToFire + percentFired > 100) percentToFire = 100 - percentFired;
                                    weapon.TargetStack = new WeaponDetails.TargetPercent(stackTarget, percentToFire);
                                    allAttacks.Add(weapon);
                                }
                                else
                                {
                                    if (weapon.Weapon.IsBeam) // budget for 101% damage to target to allow for rounding errors 
                                        percentToFire = (int)(101.00 * ((double)(stackTarget.TotalShieldStrength + stackTarget.TotalArmorStrength)) / ((CalculateWeaponPowerRon(stack.Position, weapon.Weapon, stackTarget.Position, 100, stack.Token.Design, stack.Token.Quantity) / 10.0)));
                                    if (percentToFire > 100) percentToFire = 100;
                                    if (percentToFire + percentFired > 100) percentToFire = 100 - percentFired;
                                    weapon.TargetStack = new WeaponDetails.TargetPercent(stackTarget, percentToFire);
                                    allAttacks.Add(weapon);
                                }
                                percentFired = percentFired + percentToFire;
                            }
                            targetIndex++;
                            if ((targetIndex > stack.TargetList.Count) && (!doOverkill) && (percentToFire < 100))
                            {
                                targetIndex = 0;
                                doOverkill = true;
                            }
                        }
                    }
                }
            }
            // Sort the weapon list according to weapon system initiative.
            allAttacks.Sort();
            return allAttacks;
        }

        /// <summary>
        /// Attempt an attack.
        /// </summary>
        /// <param name="allAttacks">A list of WeaponDetails representing a round of attacks.</param>
        private bool ProcessAttack(WeaponDetails attack,BattleReport battle)
        {     
            // First, check that the target stack we originally identified has not
            // been destroyed (actually, the stack still exists at this point but
            // it may have no ship tokens left). In which case, don't bother trying to
            // fire this weapon system (we'll wait until the next battle clock
            // "tick" and re-target then).    
            if (attack.TargetStack.Target == null || attack.TargetStack.Target.IsDestroyed) 
            {
                return false;
            }

            if (attack.SourceStack == null || attack.SourceStack.IsDestroyed) 
            {
                // Report.Error("attacking stack no longer exists");
                return false;
            }

            // We just calculated this    
            //if (PointUtilities.Distance(attack.SourceStack.Position, attack.TargetStack.Target.Position) > attack.Weapon.Range)
            //{
            //    return false;
            //}

            // Target is valid; execute attack. 
            ExecuteAttack(attack,battle);
            
            return true;
        }

        /// <summary>
        /// DischargeWeapon. We know the weapon and we know the target stack so attack.
        /// </summary>
        /// <param name="ship">The firing stack.</param>
        /// <param name="details">The weapon being fired.</param>
        /// <param name="target">The target stack.</param>
        private void ExecuteAttack(WeaponDetails attack,BattleReport battle)
        {
            // the two stacks involved in the attack          
            Stack attacker = attack.SourceStack;
            Stack target = attack.TargetStack.Target;
            
            // Report on the targeting.
            BattleStepTarget report = new BattleStepTarget();
            report.StackKey = attack.SourceStack.Key;
            report.TargetKey = attack.TargetStack.Target.Key;
            report.percentToFire = attack.TargetStack.PercentToFire;
            battle.Steps.Add(report);

            // Identify the attack parameters that have to take into account
            // factors other than the base values (e.g. jammers, capacitors, etc.)
            double hitPower = CalculateWeaponPowerRon(attacker.Position, attack, target.Position, report.percentToFire, attacker.Token.Design);
            double accuracy = CalculateWeaponAccuracy(attacker.Token.Design, attack, target.Token.Design);

            if (attack.Weapon.IsMissile)
            {
                FireMissile(attacker, target, hitPower, attack.missileAccuracy(attacker.Token.Design, attacker.Target.Token.Design,attack.Weapon.Accuracy/100.0), battle);
            }
            else
            {
                FireBeam(attacker, target, hitPower, battle);
            }

            // If we still have some Armor then the stack hasn't been destroyed
            // yet so this is the end of this shot.

            // FIXME (Priority 7) What about losses of a single ship within the token???
            if (target.Token.Armor <= 0) 
            {
                if (target.IsStarbase) serverState.AllEmpires[target.Owner].OwnedStars[target.InOrbit.Name].Starbase = null;
                DestroyStack(attacker, target, battle);
            }
        }

        /// <summary>
        /// All Defenses are gone. Remove the stack from the battle (which
        /// exists only during the battle) and, more importantly, remove the
        /// token from its "real" fleet. Also, generate a "destroy" event to
        /// update the battle visualization display.
        /// </summary>
        /// <param name="target"></param>
        private void DestroyStack(Stack attacker, Stack target,BattleReport battle)
        {
            // report the losses
            battle.Losses[target.Owner] = battle.Losses[target.Owner] + target.Token.Quantity;

            // for the battle viewer / report
            BattleStepDestroy destroy = new BattleStepDestroy();
            destroy.StackKey = target.Key;
            battle.Steps.Add(destroy);
            //TODO missile velocity must be taken into account - if source initiative is 4001 and target initiative is 4000 does the target have time to fire before the missiles destroy it.
            // My guess is that as soon as the missiles have "tone" on the target the target would fire at something so fleets with almost identical initiative should both get a shot away
            // remove the Token from the Fleet, if it exists
            if (serverState.AllEmpires[target.Owner].OwnedFleets.ContainsKey(target.ParentKey))
            {
                serverState.AllEmpires[target.Owner].OwnedFleets[target.ParentKey].Composition.Remove(target.Token.Key); // remove the token from the fleet

                serverState.AllEmpires[target.Owner].OwnedFleets[target.ParentKey].Composition.Remove(target.Token.Key); // remove the token from the fleet
                Star inOrbit = null;
                foreach (Star star in serverState.AllStars.Values)
                {
                    if (star.Position.distanceToSquared(target.Position) < 1.4143)
                    {
                        inOrbit = star;
                    }
                }
                if (inOrbit != null)
                {
                    inOrbit.ResourcesOnHand.Ironium += (int)0.9 * target.TotalCost.Ironium;
                    inOrbit.ResourcesOnHand.Boranium += (int)0.9 * target.TotalCost.Boranium;
                    inOrbit.ResourcesOnHand.Germanium += (int)0.9 * target.TotalCost.Germanium; //TODO priority 0 adjust scrap quantity from fleets destroyed in orbit
                }
                else CreateSalvage(serverState.AllEmpires[target.Owner].OwnedFleets[target.ParentKey].Position, target.TotalCost, target.Cargo, target.Owner);

                // remove the fleet if no more tokens
                if (serverState.AllEmpires[target.Owner].OwnedFleets[target.ParentKey].Composition.Count == 0)
                {
                    serverState.AllEmpires[target.Owner].OwnedFleets.Remove(target.ParentKey);
                    serverState.AllEmpires[target.Owner].FleetReports.Remove(target.ParentKey);
                    serverState.AllEmpires[attacker.Owner].FleetReports.Remove(target.ParentKey);  // added in Rev# 872
                }
            }

            // remove the token from the Stack (do this last so target.Token remains valid above)
            long fleetKey = target.ParentKey;
            target.Composition.Remove(target.Key);
            if (serverState.AllEmpires[target.Owner].OwnedFleets.ContainsKey(fleetKey))  serverState.AllEmpires[target.Owner].OwnedFleets.Remove(fleetKey);
            if (serverState.AllEmpires[target.Owner].FleetReports.ContainsKey(fleetKey)) serverState.AllEmpires[target.Owner].FleetReports.Remove(fleetKey);
            if (serverState.AllEmpires[attacker.Owner].FleetReports.ContainsKey(fleetKey)) serverState.AllEmpires[attacker.Owner].FleetReports.Remove(target.ParentKey);  // added in Rev# 872

        }

        private void CreateSalvage(NovaPoint position, Resources salvage,Cargo cargo, int empireID)
        {
            EmpireData empire = serverState.AllEmpires[empireID];
            ShipDesign salvageDesign = null;
            foreach (ShipDesign design in empire.Designs.Values) if (design.Name.Contains("S A L V A G E")) salvageDesign = design;  
            ShipToken token = new ShipToken(salvageDesign, 1);
            Fleet fleet = new Fleet(token, position, empire.GetNextFleetKey());
            fleet.Position = position;
            fleet.Name = "S A L V A G E";
            fleet.TurnYear = empire.TurnYear;

            // Add the fleet to the state data so it can be tracked.
            serverState.AllEmpires[fleet.Owner].AddOrUpdateFleet(fleet);
            fleet.Cargo = cargo.Scale(0.75); //TODO priority 1 check if we want to allow survivors to be rescued or do we have to Laser all Escape Pods.
            fleet.Cargo.Ironium += (int)(salvage.Ironium * 0.75); 
            fleet.Cargo.Boranium += (int)(salvage.Boranium * 0.75);
            fleet.Cargo.Germanium += (int)(salvage.Germanium * 0.75); //TODO priority 1 check salvage conversion ratios

        }

        /// <summary>
        /// Do beam weapon damage.
        /// </summary>
        /// <param name="attacker">Token firing the beam.</param>
        /// <param name="target">Weapon target.</param>
        /// <param name="hitPower">Damage done by the weapon.</param>
        private void FireBeam(Stack attacker, Stack target, double hitPower, BattleReport battle)
        {
            // First we have to take down the shields of the target ship. If
            // there is any power left over from firing this weapon system at the
            // shields then it will carry forward to attack Armor. If all we have
            // done is weaken the shields then that is the end of this shot.

            hitPower = DamageShields(attacker, target, hitPower, battle);

            if (target.Token.Shields > 0 || hitPower <= 0)
            {
                return;
            }

            DamageArmor(attacker, target, hitPower, battle);

            // TODO (Priority 6) - beam weapon overkill can hit other stacks (up to one stack per ship in the attacking stack)
        }

        /// <summary>
        /// Fire a missile weapon system.
        /// </summary>
        /// <param name="attacker">Token firing the missile.</param>
        /// <param name="target">Missile weapon target.</param>
        /// <param name="hitPower">Damage the weapon can do.</param>
        /// <param name="accuracy">Missile accuracy.</param>
        /// <remarks>
        /// </remarks>
        private void FireMissile(Stack attacker, Stack target, double hitPower, double accuracy, BattleReport battle)
        {
            // We perform 10 times more movement steps (at 1/10 of the speed) with a battle step between each movement step
            // so we can only allow 1/10 of the damage at each step
            hitPower = hitPower / 10;
            // We might have a stack of 300 ships with 9 weapons of the same type on each ship
            // the chance of every weapon missing the target would be (normalised chance of one weapon to miss) to the power of 2700
            // so if (normalised chance of one weapon to miss) = 5% the chance of every weapon missing = 1.6558145985645346574136982345412 e-3513
            // TODO find someone with good membering of probability Math
            // The formula (that is a poor approximation) that i will use is as follows % hit = P + random(-P/2,+P/2)
            // Google says The standard deviation (ax) is sqrt[ n * P * ( 1 - P ) ] if that helps :)
             
            //Double ax = Math.Sqrt(attacker.Token.Quantity * accuracy  * (1.0 - accuracy));
            int probability = random.Next(-50, 50);
            Double percentHit = (accuracy ) + (probability / 100.0) * accuracy /2.0; 
            if (percentHit > 1) percentHit = 1;
            if (percentHit < 0.0) percentHit = 0.0;  //put some bounds on the dodgy math - just in case

            //  for missiles that miss
            double minDamage = hitPower * (1-percentHit) / 8; //Perhaps 50% of misses should occur before the hits and 50% should occur after
            DamageShields(attacker, target, minDamage, battle);

            // and for missiles that hit
            double shieldsHit = hitPower * percentHit / 2;
            double armorHit = (hitPower * percentHit / 2) + DamageShields(attacker, target, shieldsHit, battle); // FIXME (Priority 5) - do double damage if it is a capital ship missile and all shields have been depleted.
            DamageArmor(attacker, target, armorHit, battle);


        }

        /// <summary>
        /// Attack the shields.
        /// </summary>
        /// <param name="attacker">Token firing a weapon.</param>
        /// <param name="target">Ship being fired on.</param>
        /// <param name="hitPower">Damage output of the weapon.</param>
        /// <returns>Residual damage after shields or zero.</returns>
        private double DamageShields(Stack attacker, Stack target, double hitPower,BattleReport battle)
        {
            if (target.Token.Shields <= 0)
            {
                return hitPower;
            }

            double initialShields = target.Token.Shields;
            target.Token.Shields -= hitPower;

            if (target.Token.Shields < 0)
            {
                target.Token.Shields = 0;
            }

            // Calculate remianing weapon power, after damaging shields (if any)
            double damageDone = initialShields - target.Token.Shields;
            double remainingPower = hitPower - damageDone;
            
            BattleStepWeapons battleStepReport = new BattleStepWeapons();
            battleStepReport.Damage = damageDone;
            battleStepReport.Targeting = BattleStepWeapons.TokenDefence.Shields;
            battleStepReport.WeaponTarget.StackKey = attacker.Key; 
            battleStepReport.WeaponTarget.TargetKey = target.Key;

            battle.Steps.Add(battleStepReport);

            return remainingPower;
        }

        /// <summary>
        /// Attack the Armor.
        /// </summary>
        /// <param name="attacker">Token making the attack.</param>
        /// <param name="target">Target being fired on.</param>
        /// <param name="hitPower">Weapon damage.</param>
        private void DamageArmor(Stack attacker, Stack target, double hitPower, BattleReport battle)
        {
            // FIXME (Priority 6) - damage is being spread over all ships in the stack. Should destroy whole ships first, then spread remaining damage.
            target.Token.Armor -= hitPower;

            BattleStepWeapons battleStepReport = new BattleStepWeapons();
            battleStepReport.Damage = hitPower;
            battleStepReport.Targeting = BattleStepWeapons.TokenDefence.Armor;
            battleStepReport.WeaponTarget.StackKey = attacker.Key;
            battleStepReport.WeaponTarget.TargetKey = target.Key;
            battle.Steps.Add(battleStepReport);
        }

        /// <summary>
        /// Calculate weapon power. For beam weapons, this damage will dissipate over
        /// the range of the beam (no dissipation at range 0,
        /// 10% dissipation at max range. Also capacitors will modify the weapon power.
        ///
        /// For missiles, the power is simply the base power.
        /// </summary>
        /// <param name="ship">Firing ship.</param>
        /// <param name="weapon">Firing weapon.</param>
        /// <param name="target">Ship being fired on.</param>
        /// <returns>Damage weapon is able to do.</returns>
        private double CalculateWeaponPowerRon(NovaPoint source, WeaponDetails weapon, NovaPoint target, int percentToFire, ShipDesign design)
        {
            if (weapon.Weapon.IsBeam)
            {
                if (design.Summary.Properties.ContainsKey("Capacitor"))
                    return (100.0 + (design.Summary.Properties["Capacitor"] as CapacitorProperty).Value) / 100.0 * weapon.Weapon.Power * weapon.beamDispersalRon(source.distanceToSquared(target), gridScaleSquared) * (double)percentToFire * (double)weapon.SourceStack.Composition.Count / 100.0;
                else return 100;
            }
            else return weapon.Weapon.Power * (double)percentToFire * (double)weapon.SourceStack.Composition.Count / 100.0;
        }
        private double CalculateWeaponPower(NovaPoint source, WeaponDetails weapon, NovaPoint target, int percentToFire, ShipDesign design)
        {
            if (weapon.Weapon.IsBeam)
            {
                if (design.Summary.Properties.ContainsKey("Capacitor"))
                    return (100.0 + (design.Summary.Properties["Capacitor"] as CapacitorProperty).Value) / 100.0 * weapon.Weapon.Power * weapon.beamDispersal(source.distanceToSquared(target)) * (double)percentToFire * (double)weapon.SourceStack.Composition.Count / 100.0;
                else return 100;
            }
            else return weapon.Weapon.Power * (double)percentToFire * (double)weapon.SourceStack.Composition.Count / 100.0;
        }
        private double CalculateWeaponPower(NovaPoint source, Weapon weapon, NovaPoint target, int percentToFire, ShipDesign design,int count)
        {
            if (weapon.IsBeam)
            {
                if (design.Summary.Properties.ContainsKey("Capacitor"))
                    return (100.0 + (design.Summary.Properties["Capacitor"] as CapacitorProperty).Value) / 100.0 * weapon.Power * weapon.beamDispersal(source.distanceToSquared(target)) * (double)percentToFire * (double)count / 100.0;
                else return 100;
            }
            else return weapon.Power * (double)percentToFire * (double)count / 100.0;
        }
        private double CalculateWeaponPowerRon(NovaPoint source, Weapon weapon, NovaPoint target, int percentToFire, ShipDesign design, int count)
        {//In the Ron battleGrid the distances are multiplied by 10 to get a grid with a 0.1 step
            if (weapon.IsBeam)
            {
                if (design.Summary.Properties.ContainsKey("Capacitor"))
                    return (100.0 + (design.Summary.Properties["Capacitor"] as CapacitorProperty).Value) / 100.0 * weapon.Power * weapon.beamDispersalRon(source.distanceToSquared(target), gridScaleSquared) * (double)percentToFire * (double)count / 100.0;
                else return 100;
            }
            else return weapon.Power * (double)percentToFire * (double)count / 100.0;
        }

        /// <summary>
        /// Calculate weapon accuracy. For beam weapons, this is 100%.
        ///
        /// For missiles, the chance to hit is based on the base accuracy, the computers
        /// on the ship and the enemy jammers.
        /// </summary>
        /// <param name="ship">Attacking ship.</param>
        /// <param name="weapon">Firing weapon.</param>
        /// <param name="target">Ship being fired on.</param>
        /// <returns>Chance that weapon will hit.</returns>
        private double CalculateWeaponAccuracy(ShipDesign ship, WeaponDetails weapon, ShipDesign target)
        {
            Double weaponAccuracy;
            if (weapon.Weapon.IsMissile)
            {
                weaponAccuracy = weapon.missileAccuracy(ship, target,weapon.Weapon.Accuracy/100.0);
            }
            else weaponAccuracy = 100;

            return weaponAccuracy;
        }

        /// <summary>
        /// Report the battle and losses to each player.
        /// </summary>
        private void ReportBattle(BattleReport battle)
        {
            foreach (int empire in battle.Losses.Keys)
            {
                Message message = new Message(
                    empire,
                    "There was a battle at " + battle.Location + "\r\n",
                    "BattleReport",
                    battle);

                if (battle.Losses[empire] == 0)
                {
                    message.Text += "None of your ships were destroyed";
                }
                else
                {
                    message.Text += battle.Losses[empire].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                       " of your ships were destroyed";
                }

                serverState.AllMessages.Add(message);
                
                serverState.AllEmpires[empire].BattleReports.Add(battle);

                battles.Add(battle);
            }
        }
    }
}

