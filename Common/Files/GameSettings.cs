﻿#region Copyright Notice
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

namespace Nova.Common
{
    using System;    
    using System.IO;  
    using System.Windows.Forms;    
    using System.Xml.Serialization;
    
    /// <summary>
    /// Records the settings specific to a running game (map, victory conditions)
    /// This module is implemented as a singleton. 
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        // Map settings

        public int MapWidth = 800;
        public int MapHeight = 600;

        public int StarSeparation = 10;
        public int StarDensity = 40;
        public int StarUniformity = 60;

        public int NumberOfStars = 50;

        // Victory conditions (with initial default values)

        public EnabledValue PlanetsOwned        = new EnabledValue(true, 60);
        public EnabledValue TechLevels          = new EnabledValue(false, 22);
        public EnabledValue NumberOfFields      = new EnabledValue(false, 4);
        public EnabledValue TotalScore          = new EnabledValue(false, 1000);
        public EnabledValue SecondPlaceScore    = new EnabledValue(false, 0);
        public EnabledValue ProductionCapacity  = new EnabledValue(false, 1000);
        public EnabledValue CapitalShips        = new EnabledValue(false, 100);
        public EnabledValue HighestScore        = new EnabledValue(false, 100);
        public int TargetsToMeet = 1;
        public int MinimumGameTime = 50;

        public string SettingsPathName;
        public string GameName = "Feel the Nova";

        public bool AcceleratedStart = false;

        public bool UseRonBattleEngine = true;
        #region Singleton

        // ============================================================================
        // Data private to this module.
        // ============================================================================

        private static GameSettings instance;
        private static object padlock = new object();


        // ============================================================================
        // Private constructor to prevent anyone else creating instances of this class.
        // ============================================================================

        private GameSettings() 
        { 
        }


        // ============================================================================
        // Provide a mechanism of accessing the single instance of this class that we
        // will create locally. Creation of the data is thread-safe.
        // ============================================================================

        public static GameSettings Data
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new GameSettings();
                        }
                    }
                }
                return instance;
            }

            // ----------------------------------------------------------------------------

            set
            {
                instance = value;
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Restore the persistent data.
        /// </summary>
        public static void Restore()
        {
            string fileName = Data.SettingsPathName;
            if (fileName == null)
            {
                fileName = FileSearcher.GetSettingsFile();
            }
            if (File.Exists(fileName))  //TODO Priority 8 I am playing a game in C:\Users\John\Documents\Stars! Nova\Feel the Nova\ but the "Continue game" function loads my 
                                        // gamesettings from C:\Users\John\Documents\Stars!Nova\FeeltheNova\Feel the Nova.settings 
            {
                bool waitForFile = false;
                double waitTime = 0.0; // seconds
                do
                {
                    try
                    {
                        using (FileStream state = new FileStream(fileName, FileMode.Open))
                        {
                            // Data = Serializer.Deserialize(state) as GameSettings;
                            XmlSerializer s = new XmlSerializer(typeof(GameSettings));
                            Data = (GameSettings)s.Deserialize(state);
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
        }

        /// <summary>
        /// Save the console persistent data.
        /// </summary>
        public static void Save()
        {
            if (Data.SettingsPathName == null)
            {
                // TODO (priority 5) add the nicities. Update the game files location.
                SaveFileDialog fd = new SaveFileDialog();
                fd.Title = "Choose a location to save the game settings.";

                DialogResult result = fd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Data.SettingsPathName = fd.FileName;
                }
                else
                {
                    throw new System.IO.IOException("File dialog cancelled.");
                }
            }
            using (Stream stream = new FileStream(Data.SettingsPathName, FileMode.Create))
            {
                // Serializer.Serialize(stream, GameSettings.Data);
                XmlSerializer s = new XmlSerializer(typeof(GameSettings));
                s.Serialize(stream, GameSettings.Data);
            }
        }

        #endregion
    }
}
