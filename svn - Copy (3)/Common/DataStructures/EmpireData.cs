#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project
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
// ============================================================================
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

    public enum PlayerRelation
    {
        Enemy,
        Neutral,
        Friend
    }

    /// <summary>
    /// Race specific data that may change from year-to-year that must be passed to
    /// the Nova console/server. 
    /// </summary>
    [Serializable]
    public class EmpireData
    {
        private ushort empireId;

        /// <summary>
        /// The year that corresponds to this data. Normally the current game year.
        /// </summary>
        public int TurnYear = Global.StartingYear;

        /// <summary>
        /// Set to true when submit turn is selected in the client. Indicates when orders are ready for processing by the server.
        /// </summary>
        public bool TurnSubmitted = false;

        /// <summary>
        /// The last game year for which a turn was submitted. Should be the previous game year until the current year is submitted. May be several years previous if turns were skipped. 
        /// </summary>
        public int LastTurnSubmitted = 0;

        private Race race = new Race(); // This empire's race.

        public int ResearchBudget = 10; // % of resources allocated to research

        /// <summary>
        /// Current levels of technology.
        /// </summary>
        public TechLevel ResearchLevels = new TechLevel();
        public TechLevel ResearchResources = new TechLevel(); // current cumulative resources on technologies
        public TechLevel ResearchTopics = new TechLevel(); // order of researching

        public RaceComponents AvailableComponents;
        public Dictionary<long, ShipDesign> Designs = new Dictionary<long, ShipDesign>();

        public StarList OwnedStars = new StarList();
        public Dictionary<string, StarIntel> StarReports = new Dictionary<string, StarIntel>();

        public FleetList OwnedFleets = new FleetList();
        public Dictionary<long, FleetIntel> FleetReports = new Dictionary<long, FleetIntel>();

        // This is Fleet Limbo~
        // ??? What is this for?
        // My guess would be for storing of waypoints for fleets that don't exist at this instant in time
        // either they will exist later this turn or they existed earlier this turn
        // also fleets that have been split/merged seem to come through Temporary Fleets
        public List<Fleet> TemporaryFleets = new List<Fleet>();

        public Dictionary<ushort, EmpireIntel> EmpireReports = new Dictionary<ushort, EmpireIntel>();

        public Dictionary<string, BattlePlan> BattlePlans = new Dictionary<string, BattlePlan>();

        public List<BattleReport> BattleReports = new List<BattleReport>();

        public Dictionary<long, Minefield> VisibleMinefields = new Dictionary<long, Minefield>();

        // See associated properties.
        private long fleetCounter = 0;
        private long designCounter = 0;
        public int gravityModCapability = 0;
        public int radiationModCapability = 0;
        public int temperatureModCapability = 0;

        public Race Race
        {
            get
            {
                return this.race;
            }

            set
            {
                if (value != null)
                {
                    race = value;
                }
            }
        }

        /// <summary>
        /// Sets or gets this empires unique integer Id.
        /// </summary>
        public ushort Id
        {
            get
            {
                return empireId;
            }

            set
            {
                // Empire Id should only be set on game creation, from a simple 0-127 int.
                if (value > 127)
                {
                    throw new ArgumentException("EmpireId out of range");
                }
                empireId = value;
            }
        }

        /// <summary>
        /// Gets the next available Fleet Key from the internal FleetCounter.
        /// </summary>
        public long GetNextFleetKey()
        {
            ++fleetCounter;
            return (long)fleetCounter | ((long)empireId << 32);
        }

        /// <summary>
        /// Peeks the next available Fleet Key from the internal FleetCounter.
        /// </summary>
        public long PeekNextFleetKey()
        {

            return fleetCounter + 1 | ((long)empireId << 32) ;
        }

        public long PeekNextFleetId()
        {

            return fleetCounter + 1  ;
        }


        /// <summary>
        /// Peeks the next available Fleet Key from the internal FleetCounter.
        /// </summary>
        public long PeekFleetKey()
        {

            return fleetCounter | ((long)empireId << 32); ;
        }




        /// <summary>
        /// Gets the next available Key for the empire.
        /// </summary>
        public long GetNextDesignKey()
        {
            ++designCounter;
            return (long)designCounter | ((long)empireId << 32);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EmpireData(bool loadComponents = true, String RaceHint = "")
        {
            Initialize(loadComponents, RaceHint);
            BattlePlans.Add("Default", new BattlePlan());
        }

        protected virtual void Initialize(bool loadComponents = true, String RaceHint = "")
        {

            if (loadComponents) AvailableComponents = new RaceComponents(RaceHint);
        }

        /// <summary>
        /// Determine if this empire wishes to treat lamb as an enemy.
        /// </summary>
        /// <param name="lamb">The id of the empire who may be attacked.</param>
        /// <returns>true if lamb is one of this empire's enemies, otherwise false.</returns>
        public bool IsEnemy(ushort lamb)
        {
            return EmpireReports[lamb].Relation == PlayerRelation.Enemy;
        }

        private void CalcTotalTerraform(int totalTeraformLevel)
        {
            temperatureModCapability = Math.Max(temperatureModCapability, totalTeraformLevel);
            radiationModCapability = Math.Max(radiationModCapability, totalTeraformLevel);
            gravityModCapability = Math.Max(gravityModCapability, totalTeraformLevel);
        }
        /// <summary>
        /// Load: constructor to load EmpireData from an XmlNode representation.
        /// </summary>
        /// <param name="node">An XmlNode containing a EmpireData representation (from a save file).</param>
        public EmpireData(XmlNode node, String RaceName)
        {
            XmlNode mainNode = node.FirstChild;
            XmlNode subNode;
            while (mainNode != null)
            {
                switch (mainNode.Name.ToLower())
                {
                    case "id":
                        empireId = ushort.Parse(mainNode.FirstChild.Value, System.Globalization.NumberStyles.HexNumber);
                        break;

                    case "fleetcounter":
                        fleetCounter = long.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "designcounter":
                        designCounter = long.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "turnyear":
                        TurnYear = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "turnsubmitted":
                        TurnSubmitted = bool.Parse(mainNode.FirstChild.Value);
                        break;

                    case "lastturnsubmitted":
                        LastTurnSubmitted = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "race":
                        race = new Race();
                        Race.LoadRaceFromXml(mainNode);
                        break;

                    case "research":
                        subNode = mainNode.SelectSingleNode("Budget");
                        ResearchBudget = int.Parse(subNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                        subNode = mainNode.SelectSingleNode("AttainedLevels");
                        ResearchLevels = new TechLevel(subNode);
                        subNode = mainNode.SelectSingleNode("SpentResources");
                        ResearchResources = new TechLevel(subNode);
                        subNode = mainNode.SelectSingleNode("Topics");
                        ResearchTopics = new TechLevel(subNode);
                        break;

                    case "starreports":
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            StarIntel report = new StarIntel(subNode);
                            StarReports.Add(report.Name, report);
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "ownedstars":
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            Star star = new Star(subNode);
                            OwnedStars.Add(star);
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "fleetreports":
                        if (AvailableComponents == null) Initialize(true, race.Name);
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            FleetIntel report = new FleetIntel(subNode);
                            FleetReports.Add(report.Key, report);
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "ownedfleets":
                        if (AvailableComponents == null) Initialize(true, race.Name);
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            Fleet fleet = new Fleet(subNode);
                            OwnedFleets.Add(fleet);
                            subNode = subNode.NextSibling;
                        }

                        break;

                    case "otherempires":
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            EmpireIntel report = new EmpireIntel(subNode);
                            EmpireReports.Add(report.Id, report);
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "battleplan":
                        BattlePlan plan = new BattlePlan(mainNode);
                        BattlePlans[plan.Name] = plan;
                        break;

                    case "availablecomponents":
                        if (AvailableComponents == null) Initialize(true, race.Name);
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            AvailableComponents.Add(new Component(subNode));
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "designs":
                        if (AvailableComponents == null) Initialize(true, race.Name);
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            ShipDesign design = new ShipDesign(subNode);
                            design.Update();
                            Designs.Add(design.Key, design);

                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "allminefields":
                        subNode = mainNode.FirstChild;
                        while (subNode != null)
                        {
                            VisibleMinefields.Add(long.Parse(subNode.Attributes["Key"].Value, System.Globalization.NumberStyles.HexNumber), new Minefield(subNode));
                            subNode = subNode.NextSibling;
                        }
                        break;

                    case "battlereport":
                        if (AvailableComponents == null) Initialize(true, race.Name);
                        BattleReport battle = new BattleReport(mainNode);
                        BattleReports.Add(battle);
                        break;
                }

                // If no orders have ever been turned in then ensure battle plans contain at least the default
                if (BattlePlans.Count == 0)
                {
                    BattlePlans.Add("Default", new BattlePlan());
                }

                mainNode = mainNode.NextSibling;

                //how much can this race terraform at the moment?

                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 1)
                    && (this.ResearchLevels[TechLevel.ResearchField.Weapons] >= 1)) radiationModCapability = 3;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 2)
                    && (this.ResearchLevels[TechLevel.ResearchField.Weapons] >= 5)) radiationModCapability = 7;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 3)
                    && (this.ResearchLevels[TechLevel.ResearchField.Weapons] >= 10)) radiationModCapability = 11;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 4)
                    && (this.ResearchLevels[TechLevel.ResearchField.Weapons] >= 15)) radiationModCapability = 15;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 1)
                    && (this.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 1)) gravityModCapability = 3;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 2)
                    && (this.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 5)) gravityModCapability = 7;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 3)
                    && (this.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 10)) gravityModCapability = 11;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 4)
                    && (this.ResearchLevels[TechLevel.ResearchField.Propulsion] >= 15)) gravityModCapability = 15;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 1)
                    && (this.ResearchLevels[TechLevel.ResearchField.Energy] >= 1)) temperatureModCapability = 3;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 2)
                    && (this.ResearchLevels[TechLevel.ResearchField.Energy] >= 5)) temperatureModCapability = 7;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 3)
                    && (this.ResearchLevels[TechLevel.ResearchField.Energy] >= 10)) temperatureModCapability = 11;
                if ((this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 4)
                    && (this.ResearchLevels[TechLevel.ResearchField.Energy] >= 15)) temperatureModCapability = 15;
                //if (this.Race.Traits.Contains("TT"))
                if (race.HasTrait("Total Terraforming"))
                {
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 1) CalcTotalTerraform(3);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 3) CalcTotalTerraform(5);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 6) CalcTotalTerraform(7);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 9) CalcTotalTerraform(10);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 13) CalcTotalTerraform(15);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 17) CalcTotalTerraform(20);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 22) CalcTotalTerraform(25);
                    if (this.ResearchLevels[TechLevel.ResearchField.Biotechnology] >= 25) CalcTotalTerraform(30);
                }

            }

            LinkReferences();
        }

        /// <summary>
        /// Save: Generate an XmlElement representation of the EmpireData.
        /// </summary>
        /// <param name="xmldoc">The parent XmlDocument.</param>
        /// <returns>An XmlElement representing the EmpireData (to be written to file).</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelEmpireData = xmldoc.CreateElement("EmpireData");

            Global.SaveData(xmldoc, xmlelEmpireData, "Id", empireId.ToString("X"));

            Global.SaveData(xmldoc, xmlelEmpireData, "FleetCounter", fleetCounter.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelEmpireData, "DesignCounter", designCounter.ToString(System.Globalization.CultureInfo.InvariantCulture));

            Global.SaveData(xmldoc, xmlelEmpireData, "TurnYear", TurnYear.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelEmpireData, "TurnSubmitted", TurnSubmitted.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelEmpireData, "LastTurnSubmitted", LastTurnSubmitted.ToString(System.Globalization.CultureInfo.InvariantCulture));

            xmlelEmpireData.AppendChild(race.ToXml(xmldoc));

            XmlElement xmlelResearch = xmldoc.CreateElement("Research");
            Global.SaveData(xmldoc, xmlelResearch, "Budget", ResearchBudget.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlelResearch.AppendChild(ResearchLevels.ToXml(xmldoc, "AttainedLevels"));
            xmlelResearch.AppendChild(ResearchResources.ToXml(xmldoc, "SpentResources"));
            xmlelResearch.AppendChild(ResearchTopics.ToXml(xmldoc, "Topics"));
            xmlelEmpireData.AppendChild(xmlelResearch);

            // Available Components
            XmlElement xmlelAvaiableComponents = xmldoc.CreateElement("AvailableComponents");
            foreach (Component component in AvailableComponents.Values)
            {
                xmlelAvaiableComponents.AppendChild(component.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelAvaiableComponents);

            // Own Designs
            XmlElement xmlelDesigns = xmldoc.CreateElement("Designs");
            foreach (ShipDesign design in Designs.Values)
            {
                design.Update();
                xmlelDesigns.AppendChild(design.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelDesigns);

            XmlElement xmlelStarReports = xmldoc.CreateElement("StarReports");
            foreach (StarIntel report in StarReports.Values)
            {
                xmlelStarReports.AppendChild(report.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelStarReports);

            XmlElement xmlelOwnedStars = xmldoc.CreateElement("OwnedStars");
            foreach (Star star in OwnedStars.Values)
            {
                xmlelOwnedStars.AppendChild(star.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelOwnedStars);

            XmlElement xmlelFleetReports = xmldoc.CreateElement("FleetReports");
            foreach (FleetIntel report in FleetReports.Values)
            {
                if (report.Composition.Count > 0)
                {
                    xmlelFleetReports.AppendChild(report.ToXml(xmldoc));
                }
                else
                {
                    // Game crashes if it tries to write out a fleet report for a fleet with no ships. 
                    // This has been added to avoid the crash, but still let us know if zero ship fleets get this far, so we can find the cause.
                    // Dan 04 May 17 - this is triggered after a battle (and each turn there after) in Rev# 871
                    // Dan 04 May 17 - I think I fixed this with Rev# 872 by updating the attacker's fleet reports after combat.
                    Report.Error("EmpireData.ToXml(): Fleet " + report.Name + " contains no ships.");
                }
            }
            xmlelEmpireData.AppendChild(xmlelFleetReports);

            XmlElement xmlelOnedFleets = xmldoc.CreateElement("OwnedFleets");
            foreach (Fleet fleet in OwnedFleets.Values)
            {
                xmlelOnedFleets.AppendChild(fleet.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelOnedFleets);

            XmlElement xmlelEnemyIntel = xmldoc.CreateElement("OtherEmpires");
            foreach (EmpireIntel report in EmpireReports.Values)
            {
                xmlelEnemyIntel.AppendChild(report.ToXml(xmldoc));
            }
            xmlelEmpireData.AppendChild(xmlelEnemyIntel);

            foreach (string key in BattlePlans.Keys)
            {
                xmlelEmpireData.AppendChild(BattlePlans[key].ToXml(xmldoc));
            }

            // Battles 
            if (BattleReports.Count > 0)
            {
                foreach (BattleReport battle in BattleReports)
                {
                    xmlelEmpireData.AppendChild(battle.ToXml(xmldoc));
                }
            }

            // Store the Minefields
            XmlElement xmlelAllMinefields = xmldoc.CreateElement("AllMinefields");
            foreach (KeyValuePair<long, Minefield> minefield in VisibleMinefields)
            {
                XmlElement child;
                child = minefield.Value.ToXml(xmldoc);
                child.SetAttribute("Key", minefield.Key.ToString("X"));
                xmlelAllMinefields.AppendChild(child);
            }
            xmlelEmpireData.AppendChild(xmlelAllMinefields);


            return xmlelEmpireData;
        }

        public void Clear()
        {
            TurnYear = Global.StartingYear;

            Race = new Race();

            ResearchBudget = 10;
            ResearchLevels = new TechLevel();
            ResearchResources = new TechLevel();
            ResearchTopics = new TechLevel();

            ///           AvailableComponents     = new RaceComponents();
            Designs = new Dictionary<long, ShipDesign>();

            OwnedStars.Clear();
            StarReports.Clear();
            OwnedFleets.Clear();
            FleetReports.Clear();

            EmpireReports.Clear();

            BattlePlans.Clear();
            BattleReports.Clear();
        }


        /// <summary>
        /// Adds a new fleet to this empire. Generates an appropriate report.
        /// </summary>
        /// <param name="fleet">Fleet to add.</param>
        /// <returns>False if the fleet already exists for this empire.</returns>
        public bool AddOrUpdateFleet(Fleet fleet)
        {
            if (OwnedFleets.ContainsKey(fleet.Key))
            {
                FleetReports[fleet.Key].Update(fleet, ScanLevel.Owned, TurnYear);
                return false;
            }

            OwnedFleets.Add(fleet);

            if (FleetReports.ContainsKey(fleet.Key))
            {
                FleetReports[fleet.Key].Update(fleet, ScanLevel.Owned, TurnYear);
            }
            else
            {
                FleetReports.Add(fleet.Key, fleet.GenerateReport(ScanLevel.Owned, TurnYear));
            }

            return true;
        }


        /// <summary>
        /// Creates a brand new Fleet at the position of
        /// an already existing one.
        /// </summary>
        /// <param name="existing">Fleet from which to take a position.</param>
        /// <returns></returns>
        public Fleet MakeNewFleet(Fleet existing)
        {
            Fleet newFleet = new Fleet(GetNextFleetKey());

            newFleet.Type = ItemType.Fleet;

            // Have one waypoint to reflect the fleet's current position and the
            // planet it is in orbit around.
            Waypoint w = new Waypoint();
            w.Position = existing.Waypoints[0].Position;
            w.Destination = existing.Waypoints[0].Destination;
            w.WarpFactor = 0;

            newFleet.Waypoints.Add(w);

            // Inititialise the fleet elements that come from the star.

            newFleet.Position = existing.Position;
            newFleet.InOrbit = existing.InOrbit;

            newFleet.Name = "New Fleet #" + newFleet.Id;

            return newFleet;
        }


        /// <summary>
        /// Removes an existing fleet from this empire. Deletes appropriate report.
        /// </summary>
        /// <param name="fleet">Fleet to remove.</param>
        /// <returns>False if empire does not own the fleet.</returns>
        public bool RemoveFleet(Fleet fleet)
        {
            return RemoveFleet(fleet.Key);
        }


        /// <summary>
        /// Removes an existing fleet from this empire. Deletes appropriate report.
        /// </summary>
        /// <param name="fleet">Fleet Key to remove.</param>
        /// <returns>False if empire does not own the fleet.</returns>
        public bool RemoveFleet(long fleetKey)
        {
            if (!OwnedFleets.ContainsKey(fleetKey))
            {
                return false;
            }

            OwnedFleets.Remove(fleetKey);
            FleetReports.Remove(fleetKey);

            return true;
        }


        /// <summary>
        /// Iterates through all Mappables in this Empire, in order.
        /// </summary>
        /// <returns>An enumerator containing all Mappables belonging to this empire.</returns>
        public IEnumerable<Mappable> IterateAllMappables()
        {
            return OwnedFleets.Values.Select(fleet => fleet as Mappable).Concat(OwnedStars.Values.Select(star => star as Mappable));
        }


        public void removeAllForeignFleets()
        {
            List<long> foreignFleets = new List<long>();
            foreach (long fleetKey in FleetReports.Keys)
            {
                foreignFleets.Add(fleetKey);
            }
            foreach (long fleetKey in foreignFleets)
            {
                long OwnerMask = 0x000000FF00000000;
                if ((ushort)((fleetKey & OwnerMask) >> 32) != empireId) FleetReports.Remove(fleetKey);
            }
        }


    /// <summary>
    /// When state is loaded from file, objects may contain references to other objects.
    /// As these may be loaded in any order (or be cross linked) it is necessary to tidy
    /// up these references once the state is fully loaded and all objects exist.
    /// In most cases a placeholder object has been created with the Key set from the file,
    /// and we need to find the actual reference using this Key.
    /// Objects can't do this themselves as they don't have access to the state data, 
    /// so we do it here.
    /// </summary>
    private void LinkReferences()
        {
            AllComponents allComponents = new AllComponents(true,race.Name);
            
            // HullModule reference to a component
            foreach (ShipDesign design in Designs.Values)
            {
                foreach (HullModule module in design.Hull.Modules)
                {
                    if (module.AllocatedComponent != null && module.AllocatedComponent.Name != null)
                    {
                        module.AllocatedComponent = allComponents.Fetch(module.AllocatedComponent.Name);
                    }
                }
                
                design.Update();
                
                if (design.Id >= designCounter)
                {
                    designCounter = design.Id + 1;
                }
            }
            
            // Link enemy designs too
            foreach (EmpireIntel enemy in EmpireReports.Values)
            {
                foreach (ShipDesign design in enemy.Designs.Values)
                {
                    foreach (HullModule module in design.Hull.Modules)
                    {
                        if (module.AllocatedComponent != null && module.AllocatedComponent.Name != null)
                        {
                            module.AllocatedComponent = allComponents.Fetch(module.AllocatedComponent.Name);
                        }
                    }
                    
                    design.Update();
                }
            }
            
            // Fleet reference to Star
            foreach (Fleet fleet in OwnedFleets.Values)
            {
                if (fleet.InOrbit != null)
                {
                    if (StarReports[fleet.InOrbit.Name].Owner == fleet.Owner)
                    {
                        fleet.InOrbit = OwnedStars[fleet.InOrbit.Name];
                    }
                    else
                    {
                        fleet.InOrbit = StarReports[fleet.InOrbit.Name];
                    }
                }
                
                // Ship reference to Design
                foreach (ShipToken token in fleet.Composition.Values)
                {
                    token.Design = Designs[token.Design.Key];
                }
            }
            
            // Set designs in any Battle Reports
            foreach (BattleReport battle in BattleReports)
            {
                foreach (Stack stack in battle.Stacks.Values)
                {
                    if (stack.Owner == empireId)
                    {
                        stack.Token.Design = Designs[stack.Token.Key];
                    }
                    else if (EmpireReports[stack.Owner].Designs.ContainsKey(stack.Token.Key))
                    {
                        stack.Token.Design = EmpireReports[stack.Owner].Designs[stack.Token.Key];
                    }
                    else
                    {
                        foreach (ShipDesign design in Designs.Values) if (design.Name == "S A L V A G E") stack.Token.Design = design; //we don't know yet so display something
                    }
                }
            }
            
            // Link reports to Designs to get accurate data.
            foreach (FleetIntel report in FleetReports.Values)
            {
                foreach (ShipToken token in report.Composition.Values)
                {
                    if (report.Owner == Id)
                    {
                        token.Design = Designs[token.Design.Key];
                    }
                    else
                    {
                        token.Design = EmpireReports[report.Owner].Designs[token.Design.Key];
                    }
                }   
            }
            
            // Link Star Races and Starbases
            foreach (Star star in OwnedStars.Values)
            {
                if (star.Owner == Id)
                {
                    star.ThisRace = Race;
                }
                else
                {
                    star.ThisRace = null;
                }

                if (star.Starbase != null)
                {
                    if (!OwnedFleets.TryGetValue(star.Starbase.Key, out star.Starbase)) star.Starbase = null; // Something broken on the Server but try to continue
                }
            }    
        }
    }
}


