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

namespace Nova.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using Nova.Common;
    using Nova.Common.Components;
    using Nova.Common.DataStructures;

    public class IntelReader
    {
        private ClientData clientState;

        public IntelReader(ClientData clientState)
        {
            this.clientState = clientState;
        }

        /// <summary>
        /// Read and process the <RaceName>.intel generated by the Nova
        /// Console. 
        /// This file must be present before the GUI will run, since it
        /// contains key data, such as the race name, as well as any information about
        /// events that happend in the previous game year (battles, mine hits, etc. 
        /// It is also used for a kind of boot strapping process as there may or may not
        /// be a <RaceName>.state file to load:
        /// 1. open the .intel and determine the race, and hence the name of any .state file.
        /// 2. open the .state file, if any. This contains any historical information, 
        ///    and is used to reconstruct the stateData (we can create a new one with 
        ///    no history if required, such as on the first turn)
        /// 3. process the .intel file to update the stateData
        ///
        /// Note that this file is not read again after the first time a new turn is
        /// received. Once fully loaded all further processing is done using 
        /// stateData (which is subsequently used to generate <RaceName>.orders.
        /// </summary>
        /// <param name="turnFileName">Path and file name of the RaceName.intel file.</param>
        public void ReadIntel(string turnFileName, bool nextTurn = false)
        {
            //if (yearToLoad == 0) yearToLoad = clientState.EmpireState.TurnYear;
            if (!File.Exists(turnFileName))
            {
                Report.FatalError("The Nova GUI cannot start unless a turn file is present");
            }

            bool waitForFile = true;
            double waitTime = 0.0; // seconds
            int activitySpinner = 1;
            ProgressDialog spinner = new ProgressDialog();

            spinner.Text = "Waiting for year " + (clientState.EmpireState.TurnYear + 1).ToString();
            spinner.Show();
            spinner.Begin(0, 100);

            do
            {
                try
                {
                    using (Stream turnFile = new FileStream(turnFileName, FileMode.Open))
                    {
                        XmlDocument xmldoc = new XmlDocument();

                        xmldoc.Load(turnFile);
                        Intel newIntel = new Intel(xmldoc);
                        waitForFile = (nextTurn && (newIntel.EmpireState.TurnYear == clientState.EmpireState.TurnYear));
                        // check this is a new turn, not an old one or the same one.
                        if ((!nextTurn && (newIntel.EmpireState.TurnYear >= clientState.EmpireState.TurnYear)) || (nextTurn && (newIntel.EmpireState.TurnYear > clientState.EmpireState.TurnYear)))
                        {
                            clientState.GameFolder = Path.GetDirectoryName(turnFileName);
                            clientState.Restore();
                            clientState.InputTurn = newIntel;
                            
                            spinner.End();
                            ProcessIntel();
                        }
                        else
                        {
                            // exit without saving any files
                            if (!nextTurn)
                            {
                                throw new System.Exception("Turn Year missmatch");
                            }
                            else
                            {
                                //System.Windows.Forms.Application.DoEvents();  //dont DoEvents here because we still have the .intel file open
                                spinner.StepTo(activitySpinner);
                                activitySpinner++;
                                if (activitySpinner > 98) activitySpinner = 1;
                                if (spinner.IsAborting) waitForFile = false; //exit without loading anything
                            }
                        }
                    }
                }
                catch (System.IO.IOException)
                {
                    // IOException. Is the file locked? Try waiting.
                    if (!nextTurn && (waitTime < Global.TotalFileWaitTime))
                    {
                        waitForFile = true;
                        System.Threading.Thread.Sleep(Global.FileWaitRetryTime);
                        waitTime += 0.1;
                    }
                    else
                    {
                        // Give up, maybe something else is wrong?
                        if (!nextTurn) throw;
                    }
                }
                if (nextTurn) System.Windows.Forms.Application.DoEvents(); //give some cpu cycles to the nextturn function
                //spinner.Invalidate();
            }
            while (waitForFile && !spinner.IsAborting);
            spinner.End();
        }

        /// <summary>
        /// This function processes the stateData.TurnData for this turn
        /// and updates the stateData.
        /// </summary>
        public void ProcessIntel()
        {
            clientState.EmpireState = clientState.InputTurn.EmpireState;

            // Clear old turn data from StateData
            clientState.Messages.Clear();

            // fix object references after loading
            LinkIntelReferences();

            ProcessMessages();
        }

        /// <summary>
        /// When intel is loaded from file, objects may contain references to other objects.
        /// As these may be loaded in any order (or be cross linked) it is necessary to tidy
        /// up these references once the file is fully loaded and all objects exist.
        /// In most cases a placeholder object has been created with the Key set from the file,
        /// and we need to find the actual reference using this Key.
        /// Objects can't do this themselves as they don't have access to the state data, 
        /// so we do it here.
        /// </summary>
        private void LinkIntelReferences()
        {
            // Messages to Event objects
            foreach (Message message in clientState.InputTurn.Messages)
            {
                switch (message.Type)
                {
                    case "TechAdvance":
                        // TODO (priority 5) - link the tech advance message to the research control panel.
                        break;

                    case "NewComponent":
                        // TODO (priority 5) - Link the new component message to the technology browser (when it is available in game).
                        break;

                    case "BattleReport":
                        // The message is loaded such that the Event is a string containing the BattleReport.Key.
                        // FIXME (priority 4) - linking battle messages to the right battle report is inefficient because the turnData.Battles does not have a meaningful key.
                        foreach (BattleReport battle in clientState.EmpireState.BattleReports)
                        {
                            if (battle.Key == ((string)message.Event))
                            {
                                message.Event = battle;
                                break;
                            }
                        }
                        break;

                    default:
                        message.Event = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Run through the full list of messages and populate the message store in the
        /// state data with the messages relevant to the player's selected race. The
        /// actual message control will be populated within the main window
        /// initialization.
        /// </summary>
        private void ProcessMessages()
        {
            foreach (Message message in clientState.InputTurn.Messages)
            {
                if ((message.Audience == clientState.EmpireState.Id) ||
                    (message.Audience == Global.Everyone))
                {
                    clientState.Messages.Add(message);
                }
            }
        }
    }
}
