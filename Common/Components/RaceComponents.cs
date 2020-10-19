#region Copyright Notice
// ============================================================================
// Copyright (C) 2010, 2011 The Stars-Nova Project
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

namespace Nova.Common.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    
    using Nova.Common;

    /// <summary>
    /// Defines a collection of Component objects using Dictionary to represent
    /// the components which are available to a given race.
    /// Allows these to be accessed as either Strings or Component objects, with 
    /// Component.Name acting as the dictionary key. A Component can be added or
    /// removed from RaceComponents using its Name or by using the
    /// AllComponents reference.
    /// </summary>
    [Serializable]
    public class RaceComponents : Dictionary<string, Component>
    {
        private String DialogHint = "";
        private Race race = null;
        private TechLevel tech = null;
        private AllComponents allComponents = null;
        /// <summary>
        /// Default Constructor. Use this when loading from XML and adding components
        /// from there one by one.
        /// </summary>
        public RaceComponents(String dialogHint)
        {
            DialogHint = dialogHint;
            allComponents = new AllComponents(true,DialogHint);
        }
        
        /// <summary>
        /// Constructor. Generates the list of available components at construction for the 
        /// given race from AllComponents.
        /// </summary>
        /// <param name="newRace">The race these RaceComponents are available too.</param>
        /// <param name="newTech">The current tech level of the race.</param>
        public RaceComponents(Race newRace, TechLevel newTech, String dialogHint)
        {
            DialogHint = dialogHint;
            allComponents = new AllComponents(true,DialogHint);
            DetermineRaceComponents(newRace, newTech);
        }

        
        public Component GetBestBeamWeapon()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if ((component.Type == ItemType.BeamWeapons) && ((component.Properties["Weapon"] as Weapon).Group != WeaponType.shieldSapper))
                {
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Weapon"] as Weapon).Range > (candidate.Properties["Weapon"] as Weapon).Range) candidate = component;
                    if (((component.Properties["Weapon"] as Weapon).Range == (candidate.Properties["Weapon"] as Weapon).Range) &&
                        ((component.Properties["Weapon"] as Weapon).Power > (candidate.Properties["Weapon"] as Weapon).Power)) candidate = component;
                }
            return candidate ;

        }
        public Component GetBestTorpedo()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Torpedoes)
                {
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Weapon"] as Weapon).Range > (candidate.Properties["Weapon"] as Weapon).Range) candidate = component;
                    if (((component.Properties["Weapon"] as Weapon).Range == (candidate.Properties["Weapon"] as Weapon).Range) &&
                        ((component.Properties["Weapon"] as Weapon).Power > (candidate.Properties["Weapon"] as Weapon).Power)) candidate = component;
                }
            return candidate;

        }
        public Component GetBestBattleComputer()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if ((component.Type == ItemType.Electrical) && (component.Name.Contains("Computer")))
                {
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Computer"] as Computer).Accuracy > (candidate.Properties["Computer"] as Computer).Accuracy) candidate = component;
                }
            return candidate;

        }
        public Component GetBestManeuveringJet()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
            { // no properties of overthrusters in component !!
                if ((component.Type == ItemType.Mechanical) && (component.Name.Contains("Maneuvering Jet")) && (candidate == null)) candidate = component; //increases speed by 1/4 square
                if ((component.Type == ItemType.Mechanical) && (component.Name.Contains("Overthruster"))) candidate = component; // increases speed by 1/2 square
            }
            return candidate;

        }
        public Component GetBestMobileArmour()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Armor)
                { // Best Armour per KG of mass
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Armor"] as IntegerProperty).Value  / (double)( component.Mass) >
                        (candidate.Properties["Armor"] as IntegerProperty).Value  / (double)( candidate.Mass)) candidate = component;
                }
            return candidate;

        }
        public Component GetBestCapacitor()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Armor)
                { // Best Armour per KG of mass
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Electrical"] as IntegerProperty).Value >
                        (candidate.Properties["Electrical"] as IntegerProperty).Value ) candidate = component;
                }
            return candidate;

        }
        public Component GetBestShield()
        {
            Component candidate = null;
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Armor)
                { // Best shields - shield mass is almost negligible
                    if (candidate == null) candidate = component;
                    if ((component.Properties["Shields"] as IntegerProperty).Value  >
                        (candidate.Properties["Shields"] as IntegerProperty).Value ) candidate = component;
                }
            return candidate;

        }

        public Component GetBestEngine(Component hullType,bool preferWarp = true)
        {
            List<Component> suitableEngines = new List<Component>();
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Engine)
                {
                    if (component.Properties.ContainsKey("Hull Affinity"))
                    {
                        HullAffinity hullAffinity = component.Properties["Hull Affinity"] as HullAffinity;
                        if (hullAffinity.Value == hullType.Name) suitableEngines.Add(component);
                    }
                    else suitableEngines.Add(component);
                }
            Component best = null;
            if (suitableEngines.Count > 0) best = suitableEngines[0];
            foreach (Component engine in suitableEngines)
            {
                if (preferWarp && ((engine.Properties["Engine"] as Engine).FreeWarpSpeed > (best.Properties["Engine"] as Engine).FreeWarpSpeed)) best = engine;
                if (preferWarp && ((engine.Properties["Engine"] as Engine).FreeWarpSpeed == (best.Properties["Engine"] as Engine).FreeWarpSpeed) && ((engine.Properties["Engine"] as Engine).OptimalSpeed > (best.Properties["Engine"] as Engine).OptimalSpeed)) best = engine;
                if (!preferWarp && ((engine.Properties["Engine"] as Engine).OptimalSpeed > (best.Properties["Engine"] as Engine).OptimalSpeed)) best = engine;  
            }
            return best;

        }
        public Component GetBestScanner(bool preferPenetrating = true)
        {
            List<Component> possibleScanner = new List<Component>();
            foreach (Component component in this.Values)
                if (component.Type == ItemType.Scanner)
                {
                    possibleScanner.Add(component);
                }
            Component best = null;
            if (possibleScanner.Count > 0) best = possibleScanner[0];
            foreach (Component scanner in possibleScanner)
            {
                if (preferPenetrating && ((scanner.Properties["Scanner"] as Scanner).PenetratingScan > (best.Properties["Scanner"] as Scanner).PenetratingScan)) best = scanner;
                if (preferPenetrating && ((scanner.Properties["Scanner"] as Scanner).PenetratingScan == (best.Properties["Scanner"] as Scanner).PenetratingScan) && ((scanner.Properties["Scanner"] as Scanner).NormalScan > (best.Properties["Scanner"] as Scanner).NormalScan)) best = scanner;
                if (!preferPenetrating && ((scanner.Properties["Scanner"] as Scanner).NormalScan > (best.Properties["Scanner"] as Scanner).NormalScan)) best = scanner;
            }
            return best;

        }
        public Component GetBestFuelTank()
        {
            List<Component> possibleTank = new List<Component>();
            foreach (Component component in this.Values)
                if ((component.Type == ItemType.Mechanical) && component.Name.Contains("Fuel Tank"))
                {
                    possibleTank.Add(component);
                }
            Component best = null;
            if (possibleTank.Count > 0) best = possibleTank[0];
            foreach (Component tank in possibleTank)
            {
                if ((tank.Properties["Fuel"] as Fuel).Capacity > (best.Properties["Fuel"] as Fuel).Capacity) best = tank;
            }
            return best;

        }




        /// <summary>
        /// Updates the collection for the given race and tech level.
        /// Note this does not remove any existing components from the collection.
        /// </summary>
        /// <param name="newRace"></param>
        /// <param name="newTech"></param>
        public void DetermineRaceComponents(Race newRace, TechLevel newTech)
        {
            race = newRace;
            tech = newTech.Clone();
            if (race == null)
            {
                throw new System.NullReferenceException();
            }
            if (tech == null)
            {
                throw new System.NullReferenceException();
            }
            
            // go through the AllCompoents list
            foreach (Component component in allComponents.GetAll.Values)
            {
                // first check the required tech level
                if (tech < component.RequiredTech)
                {
                    continue;
                }
                if (Contains(component.Name))
                {
                    continue;
                }

                // check if the component is restricted by this race's Primary or Secondary traits.
                bool restricted = false;
                foreach (string trait in AllTraits.TraitKeys)
                {
                    bool hasTrait = race.HasTrait(trait);
                    RaceAvailability availability = component.Restrictions.Availability(trait);
                    if (availability == RaceAvailability.not_available && hasTrait)
                    {
                        restricted = true;
                        break;
                    }
                    if (availability == RaceAvailability.required && !hasTrait)
                    {
                        restricted = true;
                        break;
                    }
                }

                if (!restricted)
                {
                    Add(component.Name, component);
                }
            }
        }

        /// <summary>
        /// Add a component to the list of available components.
        /// </summary>
        /// <param name="newComponent">The Component to add.</param>
        public void Add(Component newComponent)
        {
            if (!Contains(newComponent.Name))
            {
                Add(newComponent.Name, newComponent);
            }
            else
            {
                Component current = this[newComponent.Name] as Component;
                if (current != newComponent)
                {
                    Report.Error("RaceComponents.cs : Add() - attempted to add a new component with the same name as an existing component.");
                }
                // else, they are the same component and can be safely ignored
            }
        }

        /// <summary>
        /// Add a Component to the list of available components.
        /// </summary>
        /// <param name="componentName">The Name of the Component to add.</param>
        public void Add(string componentName)
        {
            if (allComponents.Contains(componentName))
            {
                Component c = allComponents.Fetch(componentName);

                Add(c.Name, c);
            }
            else
            {
                string s = "Error: The " + componentName + " component does not exist!";
                Report.Error(s);
            }
        }

        /// <summary>
        /// Remove a Component from the race's RaceComponents list.
        /// </summary>
        /// <param name="componentToRemove">The Component to remove.</param>
        public void Remove(TraitEntry componentToRemove)
        {
            Remove(componentToRemove.Name);
        }

        /// <summary>
        /// Check if the race's RaceComponents contains a particular Component.
        /// </summary>
        /// <param name="componentName">The Name of the Component to look for.</param>
        /// <returns></returns>
        public bool Contains(string componentName)
        {
            return ContainsKey(componentName);
        }
    }
}
