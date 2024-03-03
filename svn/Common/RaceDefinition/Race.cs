#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010, 2011, 2012 The Stars-Nova Project
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
    using System.IO;
    using System.Xml;

    using Nova.Common.RaceDefinition;

    /// <summary>
    /// This Class defines all the parameters that define the characteristics of a
    /// race. These values are all set in the race designer. This object also manages
    /// the loading and saving of race data to a file.
    /// </summary>
    [Serializable]
    public class Race
    {
        public bool encrypted = false;
        public int newvalue = 0;
        public EnvironmentTolerance GravityTolerance        = new GravityTolerance();
        public EnvironmentTolerance RadiationTolerance      = new RadiationTolerance();
        public EnvironmentTolerance TemperatureTolerance    = new TemperatureTolerance();

        public TechLevel ResearchCosts = new TechLevel(0);

        public RacialTraits Traits = new RacialTraits(); // Collection of all the race's traits, including the primary.

        public string PluralName;
        public string Name;
        public string Password;
        public RaceIcon Icon = new RaceIcon();

        // These parameters affect the production rate of each star (used in the
        // Star class Update method).
        public int FactoryBuildCost;        // defined in the Race Designer as the amount of Resourcesrequired to build one factory
        public int ColonistsPerResource;
        public int FactoryProduction;    // defined in the Race Designer as the amount of resources produced by 10 factories
        public int OperableFactories;

        public int MineBuildCost;
        public int MineProductionRate;   // defined in the Race Designer as the amount of minerals (kT) mined by every 10 mines
        public int OperableMines;

        public string LeftoverPointTarget;

        // Growth goes from 3 to 20 and is not normalized here.
        public double GrowthRate;

        // AI characteristics that modify the default AI behaviour
        public byte AI_proclivities_Research = 50;
        public byte AI_proclivities_Factories = 50;
        public byte AI_proclivities_Starbases = 50;
        public byte AI_proclivities_Interceptors = 50;
        public byte AI_proclivities_Bombers = 50;
        public byte AI_proclivities_Escorts = 50;
        public byte AI_proclivities_MineLayers = 20;
        public byte AI_proclivities_Colonizers = 50;
        public byte AI_proclivities_Aggression = 50;
        public byte AI_proclivities_Hit_Unarmed = 50;
        public byte AI_proclivities_Hit_Minefields = 50;
        public byte AI_proclivities_Hit_Humanses = 50;
        public byte AI_proclivities_Terraform = 50;
        public byte AI_proclivities_MineralPackets= 20;
        public byte AI_proclivities_Unused1 = 50;
        public byte AI_proclivities_Unused2 = 50;
        public byte AI_proclivities_Unused3 = 50;
        public byte AI_proclivities_Unused4 = 50;
        public byte AI_proclivities_Unused5 = 50;




        // required for searializable class
        public Race() 
        { 
        }

        /// <summary>
        /// Constructor for Race. 
        /// Reads all the race data in from an xml formatted save file.
        /// </summary>
        /// <param name="fileName">A nova save file containing a race.</param>
        public Race(string fileName)
        {
            XmlDocument xmldoc = new XmlDocument();
            bool waitForFile = false;
            double waitTime = 0; // seconds
            do
            {
                try
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        xmldoc.Load(fileName);
                        XmlNode xmlnode = xmldoc.DocumentElement;
                        LoadRaceFromXml(xmlnode);
                    }
                    waitForFile = false;
                }
                catch (System.IO.IOException)
                {
                    // IOException. Is the file locked? Try waiting.
                    if (waitTime < Global.TotalFileWaitTime)
                    {
                        waitForFile = true;
                        System.Threading.Thread.Sleep(Global.FileWaitRetryTime);
                        waitTime += 0.1;
                    }
                    else
                    {
                        // Give up, maybe something else is wrong?
                        throw;
                    }
                }
            } 
            while (waitForFile);
        }




        public double HabValue(Star star, bool maxTerraformed = false, int gravityModCapability = 0, int temperatureModCapability = 0, int radiationModCapability = 0)
        {
            double r = 0.0;
            double g = 0.0;
            double t = 0.0;
            if (maxTerraformed)
            {
                //r = NormalizeHabitabilityDistance(RadiationTolerance, star.baseRadiation, radiationModCapability);// should use unterraformed stats but not implemented yet
                // g = NormalizeHabitabilityDistance(GravityTolerance, star.baseGravity, gravityModCapability);
                //t = NormalizeHabitabilityDistance(TemperatureTolerance, star.baseTemperature, temperatureModCapability);
                r = NormalizeHabitabilityDistance(RadiationTolerance, star.OriginalRadiation , radiationModCapability);
                g = NormalizeHabitabilityDistance(GravityTolerance, star.OriginalGravity, gravityModCapability);
                t = NormalizeHabitabilityDistance(TemperatureTolerance, star.OriginalTemperature, temperatureModCapability);
            }
            else
            {
                r = NormalizeHabitabilityDistance(RadiationTolerance, star.Radiation);
                g = NormalizeHabitabilityDistance(GravityTolerance, star.Gravity);
                t = NormalizeHabitabilityDistance(TemperatureTolerance, star.Temperature);
            }

            if (r > 1 || g > 1 || t > 1)
            {
                // currently not habitable
                int result = 0;
                int maxMalus = GetMaxMalus();
                if (r > 1)
                {
                    result -= GetMalusForEnvironment(RadiationTolerance, star.Radiation, maxMalus);
                }
                if (g > 1)
                {
                    result -= GetMalusForEnvironment(GravityTolerance, star.Gravity, maxMalus);
                }
                if (t > 1)
                {
                    result -= GetMalusForEnvironment(TemperatureTolerance, star.Temperature, maxMalus);
                }
                return result / 100.0;
            }

            double x = 0;
            double y = 0;
            double z = 0;

            if (g > 0.5)
            {
                x = g - 0.5;
            }
            if (t > 0.5)
            {
                y = t - 0.5;
            }
            if (r > 0.5)
            {
                z = r - 0.5;
            }

            double h = Math.Sqrt(
                            ((1 - g) * (1 - g)) + ((1 - t) * (1 - t)) + ((1 - r) * (1 - r))) * (1 - x) * (1 - y) * (1 - z)
                                 / Math.Sqrt(3.0);
            return h;
        }
        public double HabValue(StarIntel star, bool maxTerraformed = false, int gravityModCapability = 0, int temperatureModCapability = 0, int radiationModCapability = 0)
        {

            double r = 0.0;
            double g = 0.0;
            double t = 0.0;
            if (maxTerraformed)
            {

                r = NormalizeHabitabilityDistance(RadiationTolerance, star.baseRadiation, radiationModCapability);
                g = NormalizeHabitabilityDistance(GravityTolerance, star.baseGravity, gravityModCapability);
                t = NormalizeHabitabilityDistance(TemperatureTolerance, star.baseTemperature, temperatureModCapability);
               // r = NormalizeHabitabilityDistance(RadiationTolerance, star.Radiation, radiationModCapability);
               // g = NormalizeHabitabilityDistance(GravityTolerance, star.Gravity, gravityModCapability);
               // t = NormalizeHabitabilityDistance(TemperatureTolerance, star.Temperature, temperatureModCapability);
            }
            else
            {
                r = NormalizeHabitabilityDistance(RadiationTolerance, star.Radiation);
                g = NormalizeHabitabilityDistance(GravityTolerance, star.Gravity);
                t = NormalizeHabitabilityDistance(TemperatureTolerance, star.Temperature);
            }

            if (r > 1 || g > 1 || t > 1)
            {
                // currently not habitable
                int result = 0;
                int maxMalus = GetMaxMalus();
                if (r > 1)
                {
                    result -= GetMalusForEnvironment(RadiationTolerance, star.Radiation, maxMalus);
                }
                if (g > 1)
                {
                    result -= GetMalusForEnvironment(GravityTolerance, star.Gravity, maxMalus);
                }
                if (t > 1)
                {
                    result -= GetMalusForEnvironment(TemperatureTolerance, star.Temperature, maxMalus);
                }
                return result / 100.0;
            }

            double x = 0;
            double y = 0;
            double z = 0;

            if (g > 0.5)
            {
                x = g - 0.5;
            }
            if (t > 0.5)
            {
                y = t - 0.5;
            }
            if (r > 0.5)
            {
                z = r - 0.5;
            }

            double h = Math.Sqrt(
                            ((1 - g) * (1 - g)) + ((1 - t) * (1 - t)) + ((1 - r) * (1 - r))) * (1 - x) * (1 - y) * (1 - z)
                                 / Math.Sqrt(3.0);
            return h;
        }

        /// <summary>
        /// Calculate this race's Habitability for a given star report.
        /// </summary>
        /// <param name="report">The star report for which the Habitability is being determined.</param>
        /// <returns>The normalized habitability of the star (-1 to +1).</returns>
        public double HabitalValue(StarIntel report, bool maxTerraformed = false, int gravityModCapability = 0, int temperatureModCapability = 0, int radiationModCapability = 0)
        {
            if (report.Year == Global.Unset) return -1;
            Star star = new Star();
            star.Gravity = report.Gravity;
            star.Radiation = report.Radiation;
            star.Temperature = report.Temperature;
            star.OriginalTemperature = report.baseTemperature;
            star.OriginalRadiation = report.baseRadiation;
            star.OriginalGravity = report.baseGravity;
            return HabValue(star,true, gravityModCapability, temperatureModCapability, radiationModCapability);
        }
        public double HabitalValue(StarIntel report)
        {
            if (report.Year == Global.Unset) return -1;
            Star star = new Star();
            star.Gravity = report.Gravity;
            star.Radiation = report.Radiation;
            star.Temperature = report.Temperature;

            return HabValue(star);
        }

        public virtual int GetAdvantagePoints()
        {
            RaceAdvantagePointCalculator calculator = new RaceAdvantagePointCalculator();
            return calculator.calculateAdvantagePoints(this);
        }

        public int GetLeftoverAdvantagePoints()
        {
            int advantagePoints = GetAdvantagePoints();
            advantagePoints = Math.Max(0, advantagePoints); // return Advantage Points only if >= 0
            advantagePoints = Math.Min(50, advantagePoints); // return not more than 50
            return advantagePoints;
        }

        private int GetMaxMalus()
        {
            int maxMalus = 15;
            if (HasTrait("TT"))
            {
                maxMalus = 30;
            }
            return maxMalus;
        }

        private int GetMalusForEnvironment(EnvironmentTolerance tolerance, int starValue, int maxMalus)
        {
            if (starValue > tolerance.MaximumValue)
            {
                return Math.Min(maxMalus, starValue - tolerance.MaximumValue);
            }
            else if (starValue < tolerance.MinimumValue)
            {
                return Math.Min(maxMalus, tolerance.MinimumValue - starValue);
            }
            else
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Clicks_from_center / Total_clicks_from_center_to_edge .
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="starValue"></param>
        /// <returns></returns>
        private double NormalizeHabitabilityDistance(EnvironmentTolerance tol, int starValue, int maxTerraformed = 0)
        {
            if (tol.Immune)
            {
                return 0.0;
            }

            int minv = tol.MinimumValue;
            int maxv = tol.MaximumValue;
            int span = Math.Abs(maxv - minv);
            double totalClicksFromCenterToEdge = span / 2.0;
            double centre = minv + totalClicksFromCenterToEdge;
            double clicksFromCenter = Math.Abs(centre - starValue);

            if (maxTerraformed > clicksFromCenter) clicksFromCenter = 0;
            else clicksFromCenter = clicksFromCenter - maxTerraformed;

            return clicksFromCenter / totalClicksFromCenterToEdge;
        }
        
        /// <summary>
        /// Calculate the number of resources this race requires to construct a factory.
        /// </summary>
        /// <returns>The number of resources this race requires to construct a factory.</returns>
        public Resources GetFactoryResources()
        {
            int factoryBuildCostGerm = HasTrait("CF") ? 3 : 4;
            return new Resources(0, 0, factoryBuildCostGerm, FactoryBuildCost);
        }

        /// <summary>
        /// Calculate the number of resources this race requires to construct a mine.
        /// </summary>
        public Resources GetMineResources()
        {
            return new Resources(0, 0, 0, MineBuildCost);
        }

        /// <summary>
        /// Determine if this race has a given trait.
        /// </summary>
        /// <param name="trait">A string representing a primary or secondary trait. 
        /// See AllTraits.TraitKeys for examples.</param>
        /// <returns>true if this race has the given trait.</returns>
        public bool HasTrait(string trait)
        {
            if (trait == Traits.Primary)
            {
                return true;
            }

            if (Traits == null)
            {
                return false;
            }
            return this.Traits.Contains(trait);
        }

        /// <summary>
        /// The maximum planetary population for this race.
        /// </summary>
        public int MaxPopulation
        {
            get
            {
                int maxPop = Global.NominalMaximumPlanetaryPopulation;
                if (HasTrait("HE"))
                {
                    maxPop = (int)(maxPop * Global.PopulationFactorHyperExpansion);
                }
                if (HasTrait("JOAT"))
                { 
                    maxPop = (int)(maxPop * Global.PopulationFactorJackOfAllTrades);
                }
                if (HasTrait("OBRM")) 
                {
                    maxPop = (int)(maxPop * Global.PopulationFactorOnlyBasicRemoteMining);
                }
                return maxPop;
            }
        }

        /// <summary>
        /// Get the starting population for this race.
        /// </summary>
        /// <returns>The starting population.</returns>
        /// <remarks>
        /// TODO (priority 4) - Implement starting populations for races with two starting planets.
        /// </remarks>
        public int GetStartingPopulation()
        {
            int population = Global.StartingColonists;
            
            if (GameSettings.Data.AcceleratedStart)
            {
                population = Global.StartingColonistsAcceleratedBBS;
            }

            if (HasTrait("LSP"))
            {
                population = (int)(population * Global.LowStartingPopulationFactor);
            }

            return population;
        }

        // Quick and dirty way to clone a race but has the big advantage
        // of picking up XML changes automagically
        public Race Clone()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement ele = ToXml(doc);
            Race ret = new Race();
            ret.LoadRaceFromXml(ele);
            return ret;
        }

        
        public string obfuscated (int source)
        {
            Random random = new Random();
            char[] numerals = { 'X', 'x', '0', 'g', '7', 'd', 'K' };
            long obfuscate = long.MaxValue - (source + 31) * 23;
            string dest = "";
            while (obfuscate > 0)
            {// do a base7 conversion
                Char next7 = numerals[obfuscate % 7];
                if ((random.Next(0, 99) > 67) && next7 == 'K') next7 = 's';
                if ((random.Next(0, 99) > 45) && next7 == '0') next7 = '1';
                if ((random.Next(0, 99) > 34) && next7 == 'x') next7 = 'Y';
                dest = next7 + dest;
                obfuscate = obfuscate / 7;
            }
            return dest;
        }

        public int deobfuscate (string source)
        {
            char[] numerals = { 'X', 'x', '0', 'g', '7', 'd', 'K' };
            long obfuscate = 0;
            while (source.Length > 0)
            {// do a base7 conversion
                Char next7 = source[0];
                source = source.Substring(1, source.Length - 1);
                if (next7 == 's') next7 = 'K';
                if (next7 == '1') next7 = '0';
                if (next7 == 'Y') next7 = 'x';
                obfuscate = obfuscate * 7;
                int i = 0; while (numerals[i] != next7) i++;
                obfuscate += i;
            }
            long dest = long.MaxValue - obfuscate;
            dest = (dest / 23) - 31;

            return (int) dest;
        }
        /// <summary>
        /// Save: Serialize this Race to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Race.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelRace = xmldoc.CreateElement("Race");
            if (this.encrypted)
            {
                Random random = new Random();
                int Barkininy = random.Next(1, 1000000);
                Global.SaveData(xmldoc, xmlelRace, "Barkininy", obfuscated(Barkininy));
                // OperableFactories
                Global.SaveData(xmldoc, xmlelRace, "La", obfuscated(OperableFactories));
                // ColonistsPerResource
                Global.SaveData(xmldoc, xmlelRace, "pit", obfuscated(ColonistsPerResource));
                // MineProductionRate
                Global.SaveData(xmldoc, xmlelRace, "m-ga", obfuscated(MineProductionRate));
                // MaxPopulation
                Global.SaveData(xmldoc, xmlelRace, "kai", obfuscated(MaxPopulation));
                // GrowthRate
                Global.SaveData(xmldoc, xmlelRace, "bigan", obfuscated((int)GrowthRate));
                // FactoryProduction
                Global.SaveData(xmldoc, xmlelRace, "At", obfuscated(FactoryProduction*1000));
                // MineBuildCost
                Global.SaveData(xmldoc, xmlelRace, "making", obfuscated(MineBuildCost));
                // Factory Build Cost
                Global.SaveData(xmldoc, xmlelRace, "kayo", obfuscated(FactoryBuildCost));
                // OperableMines
                Global.SaveData(xmldoc, xmlelRace, "a",obfuscated(OperableMines));





            }
            else
            {


                // MineBuildCost
                Global.SaveData(xmldoc, xmlelRace, "MineBuildCost", MineBuildCost.ToString(System.Globalization.CultureInfo.InvariantCulture));

                // Factory Build Cost
                Global.SaveData(xmldoc, xmlelRace, "FactoryBuildCost", FactoryBuildCost.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // ColonistsPerResource
                Global.SaveData(xmldoc, xmlelRace, "ColonistsPerResource", ColonistsPerResource.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // FactoryProduction
                Global.SaveData(xmldoc, xmlelRace, "FactoryProduction", FactoryProduction.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // OperableFactories
                Global.SaveData(xmldoc, xmlelRace, "OperableFactories", OperableFactories.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // MineProductionRate
                Global.SaveData(xmldoc, xmlelRace, "MineProductionRate", MineProductionRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // OperableMines
                Global.SaveData(xmldoc, xmlelRace, "OperableMines", OperableMines.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // MaxPopulation
                Global.SaveData(xmldoc, xmlelRace, "MaxPopulation", MaxPopulation.ToString(System.Globalization.CultureInfo.InvariantCulture));
                // GrowthRate
                Global.SaveData(xmldoc, xmlelRace, "GrowthRate", GrowthRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            // Tech
            xmlelRace.AppendChild(ResearchCosts.ToXml(xmldoc));

            // Type; // Primary Racial Trait.
            Global.SaveData(xmldoc, xmlelRace, "PRT", Traits.Primary.Code);
            // Traits
            foreach (TraitEntry trait in Traits)
            {
                if (AllTraits.Data.Primary.Contains(trait.Code))
                {
                    continue; // Skip the PRT, just add LRTs here.
                }
                Global.SaveData(xmldoc, xmlelRace, "LRT", trait.Code);
            }

            // Plural Name
            if (!string.IsNullOrEmpty(PluralName))
            {
                Global.SaveData(xmldoc, xmlelRace, "PluralName", PluralName);
            }
            // Name
            if (!string.IsNullOrEmpty(Name))
            {
                Global.SaveData(xmldoc, xmlelRace, "Name", Name);
            }
            // Password 
            if (!string.IsNullOrEmpty(Password))
            {
                Global.SaveData(xmldoc, xmlelRace, "Password", Password);
            }
            // RaceIconName
            if (!string.IsNullOrEmpty(Icon.Source))
            {
                Global.SaveData(xmldoc, xmlelRace, "RaceIconName", Icon.Source);
            }
            xmlelRace.AppendChild(GravityTolerance.ToXml(xmldoc, "GravityTolerance"));
            xmlelRace.AppendChild(RadiationTolerance.ToXml(xmldoc, "RadiationTolerance"));
            xmlelRace.AppendChild(TemperatureTolerance.ToXml(xmldoc, "TemperatureTolerance"));
            // LeftoverPointTarget
            if ("".Equals(LeftoverPointTarget) || LeftoverPointTarget == null)
            {
                LeftoverPointTarget = "Surface minerals";
            }
            Global.SaveData(xmldoc, xmlelRace, "LeftoverPoints", LeftoverPointTarget.ToString(System.Globalization.CultureInfo.InvariantCulture));

            //The AI proclivities
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Research", AI_proclivities_Research.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Factories", AI_proclivities_Factories.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Starbases", AI_proclivities_Starbases.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Interceptors", AI_proclivities_Interceptors.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Bombers", AI_proclivities_Bombers.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Escorts", AI_proclivities_Escorts.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_MineLayers", AI_proclivities_MineLayers.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Colonizers", AI_proclivities_Colonizers.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Aggression", AI_proclivities_Aggression.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Hit_Unarmed", AI_proclivities_Hit_Unarmed.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Hit_Minefields", AI_proclivities_Hit_Minefields.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Hit_Humanses", AI_proclivities_Hit_Humanses.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Terraform", AI_proclivities_Terraform.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_MineralPackets", AI_proclivities_MineralPackets.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Unused1", AI_proclivities_Unused1.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Unused2", AI_proclivities_Unused2.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Unused3", AI_proclivities_Unused3.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Unused4", AI_proclivities_Unused4.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelRace, "AI_proclivities_Unused5", AI_proclivities_Unused5.ToString(System.Globalization.CultureInfo.InvariantCulture));

            return xmlelRace;
        }

        /// <summary>
        /// Load a Race from an xml document.
        /// </summary>
        /// <param name="xmlnode">An XmlNode, see Race constructor for generation.</param>
        public void LoadRaceFromXml(XmlNode xmlnode)
        {
            while (xmlnode != null)
            {
                try
                {
                    switch (xmlnode.Name.ToLower())
                    {
                        case "root":
                            xmlnode = xmlnode.FirstChild;
                            continue;
                        case "race":
                            xmlnode = xmlnode.FirstChild;
                            continue;
                        case "gravitytolerance":
                            GravityTolerance.FromXml(xmlnode);
                            break;
                        case "barkininy":
                            newvalue = deobfuscate(xmlnode.FirstChild.Value);
                            encrypted = true;
                            break;
                        case "radiationtolerance":
                            RadiationTolerance.FromXml(xmlnode);
                            break;
                        case "temperaturetolerance":
                            TemperatureTolerance.FromXml(xmlnode);
                            break;
                        case "tech":
                            ResearchCosts = new TechLevel(xmlnode);
                            break;

                        case "lrt":
                            Traits.Add(xmlnode.FirstChild.Value);
                            break;

                        case "minebuildcost":
                            MineBuildCost = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "making":
                            MineBuildCost = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "prt":
                            Traits.SetPrimary(xmlnode.FirstChild.Value);
                            break;
                        case "pluralname":
                            if (xmlnode.FirstChild != null)
                            {
                                PluralName = xmlnode.FirstChild.Value;
                            }
                            break;
                        case "name":
                            if (xmlnode.FirstChild != null)
                            {
                                Name = xmlnode.FirstChild.Value;
                            }
                            break;
                        case "password":
                            if (xmlnode.FirstChild != null)
                            {
                                Password = xmlnode.FirstChild.Value;
                            }
                            break;

                        // TODO (priority 5) - load the RaceIcon
                        case "raceiconname":
                            if (xmlnode.FirstChild != null)
                            {
                                Icon.Source = xmlnode.FirstChild.Value;
                            }
                            break;

                        case "factorybuildcost":
                            FactoryBuildCost = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "kayo":
                            FactoryBuildCost = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "colonistsperresource":
                            ColonistsPerResource = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "pit":
                            ColonistsPerResource = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "factoryproduction":
                            FactoryProduction = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "at":
                            FactoryProduction = deobfuscate(xmlnode.FirstChild.Value)/1000;
                            break;
                        case "operablefactories":
                            OperableFactories = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "la":
                            OperableFactories = deobfuscate(xmlnode.FirstChild.Value);
                            break; 
                        case "m-ga":
                            MineProductionRate = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "mineproductionrate":
                            MineProductionRate = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "operablemines":
                            OperableMines = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "a":
                            OperableMines = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "growthrate":
                            GrowthRate = int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "bigan":
                            GrowthRate = deobfuscate(xmlnode.FirstChild.Value);
                            break;
                        case "leftoverpoints":
                            this.LeftoverPointTarget = xmlnode.FirstChild.Value;
                            break;
                        case "ai_proclivities_research":
                            AI_proclivities_Research = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_factories":
                            AI_proclivities_Factories = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_starbases":
                            AI_proclivities_Starbases = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_interceptors":
                            AI_proclivities_Interceptors = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_bombers":
                            AI_proclivities_Bombers = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_escorts":
                            AI_proclivities_Escorts = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_mineLayers":
                            AI_proclivities_MineLayers = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_colonizers":
                            AI_proclivities_Colonizers = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_aggression":
                            AI_proclivities_Aggression = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_hit_unarmed":
                            AI_proclivities_Hit_Unarmed = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_hit_minefields":
                            AI_proclivities_Hit_Minefields = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_hit_humanses":
                            AI_proclivities_Hit_Humanses = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_terraform":
                            AI_proclivities_Terraform = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_mineralpackets":
                            AI_proclivities_MineralPackets = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_unused1":
                            AI_proclivities_Unused1 = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_unused2":
                            AI_proclivities_Unused2 = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_unused3":
                            AI_proclivities_Unused3 = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_unused4":
                            AI_proclivities_Unused4 = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "ai_proclivities_unused5":
                            AI_proclivities_Unused5 = (byte)int.Parse(xmlnode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;

                        default: break;
                    }
                }
                catch (Exception e)
                {
                    Report.FatalError(e.Message + "\n Details: \n" + e);
                }

                xmlnode = xmlnode.NextSibling;
            }

            // if an old version of the race file is loaded and there is no leftover point target then select standard leftover point target.
            if ("".Equals(LeftoverPointTarget) || LeftoverPointTarget == null)
            {
                this.LeftoverPointTarget = "Surface minerals";
            }
        }

        public int LowerHab(int habIndex)
        {
            switch (habIndex)
            {
                case 0:
                    return GravityTolerance.MinimumValue;
                case 1:
                    return TemperatureTolerance.MinimumValue;
                case 2:
                    return RadiationTolerance.MinimumValue;
            }
            return 0;
        }

        public int UpperHab(int habIndex)
        {
            switch (habIndex)
            {
                case 0:
                    return GravityTolerance.MaximumValue;
                case 1:
                    return TemperatureTolerance.MaximumValue;
                case 2:
                    return RadiationTolerance.MaximumValue;
            }
            return 0;
        }

        public int CenterHab(int habIndex)
        {
            switch (habIndex)
            {
                case 0:
                    return GravityTolerance.OptimumLevel;
                case 1:
                    return TemperatureTolerance.OptimumLevel;
                case 2:
                    return RadiationTolerance.OptimumLevel;
            }
            return 0;
        }

        public bool IsImmune(int habIndex)
        {
            switch (habIndex)
            {
                case 0:
                    return GravityTolerance.Immune;
                case 1:
                    return TemperatureTolerance.Immune;
                case 2:
                    return RadiationTolerance.Immune;
            }
            return false;
        }
    }
}