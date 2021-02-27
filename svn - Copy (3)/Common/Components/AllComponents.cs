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
// ===========================================================================
#endregion

namespace Nova.Common.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Provides access to a <see cref="ConcurrentDictionary"/>
    /// containing all <see cref="Component"/>s indexed by their names.
    /// </summary>
    public sealed class AllComponents
    {
        // Contains all components
        private static ConcurrentDictionary<string, Component> components = new ConcurrentDictionary<string, Component>();

        // Data private to this module.
        private static string saveFilePath;
        private static string graphicsFilePath;
        private static bool isLoaded = false;
        private String hint;
        /// <summary>
        /// Returns an IDictionary (Compatible with Dictionary and ConcurrentDictionary)
        /// containing all game components.
        /// </summary>
        public IDictionary<string, Component> GetAll
        {
            get
            {
                return components;
            }
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="restore">If true (by default) it will also restore
        /// all components on creation.</param>
        public AllComponents(bool restore = true,string Hint = "")
        {
            hint = Hint;
            if (restore)
            {
                Restore(hint);
            }
        }

        
        /// <summary>
        /// Check if AllComponents contains a particular Component.
        /// </summary>
        /// <param name="componentName">The Name of the Component to look for.</param>
        /// <returns>True if the component is included.</returns>
        public bool Contains(string componentName)
        {
            return components.ContainsKey(componentName);
        }

        
        /// <summary>
        /// Check if AllComponents contains a particular Component.
        /// </summary>
        /// <param name="component">The Component to look for.</param>
        /// <returns>True if the component is included.</returns>
        public bool Contains(Component component)
        {
            return Contains(component.Name);
        }

        
        /// <summary>
        /// Returns a new instance of the requested component.
        /// </summary>
        /// <param name="componentName">The desired Component's name.</param>
        /// <returns>The new requested Component, or null.</returns>
        public Component Fetch(string componentName)
        {
            if (Contains(componentName))
            {
                return new Component(components[componentName]);
            }
            
            return null;
        }

        public Component FetchBest(string componentName)
        {
            if (Contains(componentName))
            {
                return new Component(components[componentName]);
            }

            return null;
        }



        /// <summary>
        /// Removes and returns a Component.
        /// </summary>
        /// <param name="componentName">Component name to remove.</param>
        /// <returns>The removed Component, or null.</returns>
        public Component Remove(string componentName)
        {
            Component removed = null;
            components.TryRemove(componentName, out removed);
            
            return removed;
        }
        
        
        /// <summary>
        /// Start a new component definition set. This simply wipes all components from
        /// the in memory component definitions.
        /// </summary>
        public void MakeNew()
        {
            components = new ConcurrentDictionary<string, Component>();
            
            using (Config conf = new Config())
            {
                conf.Remove(Global.ComponentFileName);
            }
            
            saveFilePath = null;
            isLoaded = false;
        }
        
        
        /// <summary>
        /// Restore the component definitions.
        /// </summary>
        /// <exception cref="System.Data.OperationAbortedException">
        /// The loading of the component definition was aborted.
        /// </exception>
        public void Restore(string Hint ="")
        {
            hint = Hint;
            // If components are already loaded, GO AWAY DAMN DIALOG -Aeglos 25 Jun 11
            if (isLoaded)
            {
                return;
            }
            
            // Ensure we have the component definition file before starting the worker thread, or die.
            if (string.IsNullOrEmpty(saveFilePath))
            {
                saveFilePath = FileSearcher.GetComponentFile();
                if (string.IsNullOrEmpty(saveFilePath))
                {
                    Report.FatalError("Unable to locate component definition file.");
                }
            }
            else
            {
                // Report.Debug("Components file to be loaded: \"" + saveFilePath + "\"");
            }
            
            ProgressDialog progress = new ProgressDialog();
            progress.Text =  hint;
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadComponents), progress);
            progress.ShowDialog();
            
            if (!progress.Success)
            {
                Report.FatalError("Failed to load component file: ProgressDialog returned false.");
                throw new System.Exception();
            }
            isLoaded = true;
        }


        /// <summary>
        /// Load all the components form the component definition file, nominally components.xml.
        /// </summary>
        /// <param name="status">An <see cref="IProgressCallback"/> used for updating the progress dialog.</param>
        /// <remarks>
        /// This is run in a worker thread and therefore has no direct access to the UI/user.
        /// </remarks>
        private void LoadComponents(object status)
        {
            IProgressCallback callback = status as IProgressCallback;

            // blank the component data
            components = new ConcurrentDictionary<string, Component>();
                
            XmlDocument xmldoc = new XmlDocument();
            bool waitForFile = false;
            double waitTime = 0; // seconds
            do
            {
                try
                {
                    using (FileStream componentFileStream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read))
                    {
                        xmldoc.Load(componentFileStream);

                        XmlNode xmlnode = xmldoc.DocumentElement;

                        int nodesLoaded = 0;
                        while (xmlnode != null)
                        {
                            // Report.Information("node name = '" + xmlnode.Name + "'");
                            if (xmlnode.Name == "ROOT")
                            {
                                callback.Begin(0, xmlnode.ChildNodes.Count);

                                xmlnode = xmlnode.FirstChild;
                            }
                            else if (xmlnode.Name == "Component")
                            {
                                ++nodesLoaded;
                                callback.SetText(string.Format(hint+": Loading component: {0}", nodesLoaded));
                                callback.StepTo(nodesLoaded);
                                Component newComponent = new Component(xmlnode);
                                if (newComponent.Description == "")
                                {

                                    foreach (String propName in newComponent.Properties.Keys)
                                    {
                                        ComponentProperty prop;
                                        if (newComponent.Properties.TryGetValue(propName, out prop))
                                        {
                                            if (prop is IntegerProperty) newComponent.Description += propName + " = " + (prop as IntegerProperty).Value.ToString() + "\r\n";
                                            if (prop is CapacitorProperty) newComponent.Description += propName + " = " + (prop as CapacitorProperty).Value.ToString() + "\r\n";
                                            if (prop is Fuel) newComponent.Description += "Fuel Capacity = " + (prop as Fuel).Capacity.ToString() + "\r\n";
                                            if (prop is Fuel) newComponent.Description += "Fuel Generation = " + (prop as Fuel).Generation.ToString() + "\r\n";
                                            if (prop is DoubleProperty) newComponent.Description += propName + " = " + (prop as DoubleProperty).Value.ToString() + "\r\n";
                                            if (prop is Computer) newComponent.Description += "Computer Accuracy = " + (prop as Computer).Accuracy.ToString() + "\r\n";
                                            if (prop is Computer) newComponent.Description += "Computer Initiative = " + (prop as Computer).Initiative.ToString() + "\r\n";
                                            if (prop is Bomb)
                                            {
                                                if ((prop as Bomb).IsSmart) newComponent.Description += "SMART BOMB" + "\r\n";
                                                newComponent.Description += "Minimum Population Killed = " + (prop as Bomb).MinimumKill.ToString() + "\r\n";
                                                newComponent.Description += "Population Killed = " + (prop as Bomb).PopKill.ToString() + "\r\n";
                                                newComponent.Description += "Minimum Defenses, Factories and mines destroyed = " + (prop as Bomb).Installations.ToString() + "\r\n";
                                            }
                                            if (prop is Gate) newComponent.Description += "Safe Range = " + (prop as Gate).SafeRange.ToString() + "\r\n";
                                            if (prop is Gate) newComponent.Description += "Safe Hull Mass = " + (prop as Gate).SafeHullMass.ToString() + "\r\n";
                                            if (prop is ProbabilityProperty) newComponent.Description += propName + " = " + (prop as ProbabilityProperty).Value.ToString() + "%\r\n";
                                            if (prop is Scanner) newComponent.Description += "Normal Scan Range = " + (prop as Scanner).NormalScan.ToString() + "\r\n";
                                            if (prop is Weapon) if ((prop as Weapon).IsMissile) newComponent.Description += "MISSILE" + "\r\n";
                                            if (prop is Weapon) if ((prop as Weapon).IsBeam) newComponent.Description += "BEAM WEAPON" + "\r\n";
                                            if (prop is Weapon) newComponent.Description +=  "Power = " + (prop as Weapon).Power.ToString() + "\r\n";
                                            if (prop is Weapon) newComponent.Description +=  "Accuracy = " + (prop as Weapon).Accuracy.ToString() + "\r\n";
                                            if (prop is Weapon) newComponent.Description +=  "Range = " + (prop as Weapon).Range.ToString() + "\r\n";
                                            if (prop is Weapon) newComponent.Description +=  "Initiative = " + (prop as Weapon).Initiative.ToString() + "\r\n";
                                            if (prop is MineLayer)
                                            {
                                                if ((prop as MineLayer).HitChance == MineLayer.HeavyHitChance) newComponent.Description += "HEAVY Mine Layer" + "\r\n";
                                                if ((prop as MineLayer).HitChance == MineLayer.StandardHitChance) newComponent.Description += "Standard Mine Layer" + "\r\n";
                                                if ((prop as MineLayer).HitChance == MineLayer.SpeedTrapHitChance) newComponent.Description += "Speed-trap Mine Layer" + "\r\n";
                                                newComponent.Description += "Mines laid per year = " + (prop as MineLayer).LayerRate.ToString() + "\r\n";
                                                newComponent.Description += "Maximum Warp speed = Warp " + (prop as MineLayer).SafeSpeed.ToString() + "\r\n";
                                                newComponent.Description += "Chance per l.y. of a hit = " + (prop as MineLayer).HitChance.ToString() + "%\r\n";
                                                newComponent.Description += "Minimum damage done to a fleet = " + (prop as MineLayer).MinFleetDamage.ToString() + "\r\n";
                                                newComponent.Description += "Minimum damage done to a fleet that contains at least one RamScoop engine = " + (prop as MineLayer).MinRamScoopDamage.ToString() + "\r\n";
                                                newComponent.Description += "Damage done to each ship " + (prop as MineLayer).DamagePerEngine.ToString() + "\r\n";
                                                newComponent.Description += "Damage done to each ship that contains at least one RamScoop engine" + (prop as MineLayer).DamagePerRamScoop.ToString() + "\r\n";
                                            }
                                        }
                                    }
                                }
                                components[newComponent.Name] = newComponent;
                                xmlnode = xmlnode.NextSibling;
                            }
                            else
                            {
                                xmlnode = xmlnode.NextSibling;
                            }

                            // check for user Cancel
                            if (callback.IsAborting)
                            {
                                return;
                            }
                        }
                    }
                    waitForFile = false;
                   
                    callback.Success = true;
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
                catch (System.Threading.ThreadAbortException)
                {
                    // We want to exit gracefully here (if we're lucky)
                    Report.Error("AllComponents: LoadComponents() - Thread Abort Exception.");
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                    // And here, if we can
                    Report.Error("AllComponents: LoadComponents() - Thread Interrupted Exception.");
                }
                catch (Exception e)
                {
                    Report.Error("Failed to load file: \r\n" + e.Message);
                }
                finally
                {
                    if (callback != null)
                    {
                        callback.End();
                    }
                }
            } 
            while (waitForFile);
        }

        
        /// <summary>
        /// Save the component data.
        /// </summary>
        public bool Save()
        {
            bool waitForFile = false;
            double waitTime = 0.0; // seconds
            do
            {
                try
                {
                    // Setup the save location and stream.
                    using (FileStream saveFile = new FileStream(ComponentFile, FileMode.Create))
                    {
                        // Setup the XML document
                        XmlDocument xmldoc = new XmlDocument();
                        Global.InitializeXmlDocument(xmldoc);

                        // add the components to the document
                        foreach (Component thing in components.Values)
                        {
                            xmldoc.ChildNodes.Item(1).AppendChild(thing.ToXml(xmldoc));
                        }

                        xmldoc.Save(saveFile);
                    }

                    Report.Information("Component data has been saved to " + saveFilePath);
                    waitForFile = false;
                }
                catch (System.IO.FileNotFoundException)
                {
                    Report.Error("Error: File path not specified.");
                    return false;
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
                catch (Exception e)
                {
                    Report.Error("Error: Failed to save component definition file. " + e.Message);
                    return false;
                }
            } 
            while (waitForFile);

            return true;
        }

        public Component GetBestEngine(Component hullType, bool preferWarp = true)
        {
            List<Component> suitableEngines = new List<Component>();
            foreach (Component component in components.Values)
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
        /// <summary>
        /// Get the path where the graphics files are stored.
        /// </summary>
        public string Graphics
        {
            get
            {
                if (!Directory.Exists(graphicsFilePath))
                {
                    graphicsFilePath = FileSearcher.GetGraphicsPath();
                    if (!string.IsNullOrEmpty(graphicsFilePath))
                    {
                        using (Config conf = new Config())
                        {
                            conf[Global.GraphicsFolderKey] = graphicsFilePath;
                        }
                    }
                }
                
                return graphicsFilePath;
            }
        }

        
        /// <summary>
        /// Path and file name of the component definition file, automatically located and persisted, or null.
        /// </summary>
        public string ComponentFile
        {
            get
            {
                if ( ! Directory.Exists(saveFilePath))
                {
                    saveFilePath = FileSearcher.GetComponentFile();
                }
                
                return saveFilePath;
            }
        }
    }
}
