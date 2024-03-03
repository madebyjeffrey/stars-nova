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

namespace Nova.WinForms.Gui
{
    using System;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Windows.Forms;

    using Nova.Client;
    using Nova.Common;

    /// <Summary>
    /// The Nova GUI is the form used to play a turn of nova. In a multiplayer/network
    /// game it is the main client side program. The Nova GUI reads in a .intel
    /// file to determine what the player race knows about the universe and when a 
    /// turn is submitted, generates a .orders file for processing of the next game
    /// year. A history is maintained by the ConsoleState object as a .state file.
    ///
    /// This module holds the program entry Point and handles all things related to
    /// the main GUI window.
    /// </Summary>
    [SupportedOSPlatform("windows")]
    public partial class NovaGUI : Form
    {
        public int CurrentTurn;      // control turnvar used for to decide to load new turn... (Thread)
        public string CurrentRace;   // control var used for to decide to load new turn... (Thread)
        protected ClientData clientState;
        
        /// <Summary>
        /// Construct the main window.
        /// </Summary>
        public NovaGUI(string[] argArray)

        {
            System.Windows.Forms.Timer timerNextTurn = new System.Windows.Forms.Timer();
            timerNextTurn.Enabled = true;
            timerNextTurn.Interval = 100;
            timerNextTurn.Tick += new EventHandler(timerNextTurn_Tick);
            timerNextTurn.Start();
            // 
            // timerNextTurn
            // 
            timerNextTurn.Enabled = true;
            timerNextTurn.Tick += new System.EventHandler(this.timerNextTurn_Tick);

            clientState = new ClientData("Client ");
            clientState.Initialize(argArray);
            
            InitializeComponent();
             initializeControls();

            // These used to be in the designer.cs file, but visual studio designer throws a whappy so they are here
            // for now so it works again
            if (SelectionDetails != null)
            {
                SelectionDetails.FleetDetail.StarmapChanged += MapControl.RefreshStarMap;
                SelectionDetails.FleetDetail.FleetSelectionChanged += MapControl.SetCursor;
                SelectionDetails.FleetDetail.FleetSelectionChanged += MapControl.SetCursor;
                SelectionDetails.FleetDetail.WaypointIndexChanged += MapControl.SetCursor;

                SelectionDetails.FleetDetail.FleetSelectionChanged += SelectionSummary.SummaryChangeSelection;
                SelectionDetails.PlanetDetail.PlanetSelectionChanged += SelectionSummary.SummaryChangeSelection;
                MapControl.SelectionChanged += SelectionSummary.SummaryChangeSelection;

                MapControl.SelectionRequested += SelectionDetails.CurrentSelection;
                MapControl.SelectionChanged += SelectionDetails.DetailChangeSelection;
                MapControl.WaypointChanged += SelectionDetails.FleetDetail.UpdateWaypointList;
                Messages.doStarIntelStuff += SelectionDetails.DetailChangeSelection;
                Messages.doStarIntelStuff += MapControl.RefreshStarMap;
                Messages.doStarIntelStuff += MapControl.SetCursor;
                Messages.doMineIntelStuff += SelectionDetails.DetailChangeSelection;
                Messages.doMineIntelStuff += MapControl.RefreshStarMap;
                Messages.doMineIntelStuff += MapControl.SetCursor;
                Messages.doFleetIntelStuff += SelectionDetails.DetailChangeSelection;
                Messages.doFleetIntelStuff += MapControl.RefreshStarMap;
                Messages.doFleetIntelStuff += MapControl.SetCursor;
                Messages.doPositionIntelStuff += MapControl.RefreshStarMap;
                Messages.doPositionIntelStuff += MapControl.SetCursor;
            }
        }

        public SelectionDetail SelectionDetails
        {
            get { return selectionDetail; }
        }

        public StarMap MapControl
        {
            get { return mapControl; }
        }

        public SelectionSummary SelectionSummary
        {
            get { return selectionSummary; }
        }

        public Messages Messages
        {
            get { return messages; }
        }
        public bool nextTurnQueued = false;   //programatically create timer because cant use designer with GUI  !!!!
        private void timerNextTurn_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();
            this.Update();
            //if (nextTurnQueued)
            {
               // nextTurnQueued = false;
              //  this.NextTurn();
            }
        }

        /// <Summary>
        /// Exit menu Item selected.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MenuExit_Click(object sender, System.EventArgs e)
        {
            //clientState.Save();
            Close();
        }

        /// <Summary>
        /// Pop up the ship design dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MenuShipDesign(object sender, System.EventArgs e)
        {
            ShipDesignDialog shipDesignDialog = new ShipDesignDialog(clientState);
            shipDesignDialog.ShowDialog();
            shipDesignDialog.Dispose();
        }

        /// <Summary>
        /// Deal with keys being pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '+':
                    e.Handled = true;
                    MapControl.ZoomInClick(null, null);
                    break;

                case '-':
                    e.Handled = true;
                    MapControl.ZoomOutClick(null, null);
                    break;
            }
        }

        /// <Summary>
        /// Display the "About" dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MenuAbout(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
            aboutBox.Dispose();
        }

        /// <Summary>
        /// Display the research dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MenuResearch(object sender, EventArgs e)
        {
            ResearchDialog newResearchDialog = new ResearchDialog(clientState);
            newResearchDialog.ResearchAllocationChangedEvent += new ResearchAllocationChanged(this.UpdateResearchBudgets);
            newResearchDialog.ShowDialog();
            newResearchDialog.Dispose();
        }

        /// <Summary>
        /// Main Window is closing (i.e. the "X" button has been pressed on the frame of
        /// the form. Save the local state data.
        /// </Summary><remarks>
        /// NB: Don't generate the orders file unless Save&Submit is selected.
        /// TODO (priority 7) - ask the user if they want to submit the current turn before closing.
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void NovaGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
               // clientState.Save();  // if we are throwing out the orders then throw out the changes to the client also
            }
            catch (Exception ex)
            {
                Report.Error("Unable to save the client state." + Environment.NewLine + ex.Message);                
            }                
            // OrderWriter.WriteOrders(); // don't do this here, do it only on save & submit.
        }

        /// <Summary>
        /// Pop up the player relations dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void PlayerRelationsMenuItem_Click(object sender, EventArgs e)
        {
            PlayerRelations relationshipDialog = new PlayerRelations(clientState.EmpireState.EmpireReports, clientState.EmpireState.Id);
            relationshipDialog.ShowDialog();
            relationshipDialog.Dispose();
        }

        /// <Summary>
        /// Pop up the battle plans dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void BattlePlansMenuItem(object sender, EventArgs e)
        {
            BattlePlans battlePlans = new BattlePlans(clientState.EmpireState.BattlePlans);
            battlePlans.ShowDialog();
            battlePlans.Dispose();
        }

        /// <Summary>
        /// Pop up the Design Manager Dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void DesignManagerMenuItem_Click(object sender, EventArgs e)
        {
            DesignManager designManager = new DesignManager(clientState);
            designManager.StarmapChanged += MapControl.RefreshStarMap;
            designManager.ShowDialog();
            designManager.Dispose();
        }

        /// <Summary>
        /// Pop up the Planet Report Dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void PlanetReportMenu_Click(object sender, EventArgs e)
        {
            PlanetReport planetReport = new PlanetReport(clientState.EmpireState);
            planetReport.ShowDialog();
            planetReport.Dispose();
        }

        /// <Summary>
        /// Pop up the Fleet Report Dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void FleetReportMenu_Click(object sender, EventArgs e)
        {
            FleetReport fleetReport = new FleetReport(clientState.EmpireState);
            fleetReport.ShowDialog();
            fleetReport.Dispose();
        }

        /// <Summary>
        /// Pop up the battle Report Dialog
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void BattlesReportMenu_Click(object sender, EventArgs e)
        {
            BattleReportDialog battleReport = new BattleReportDialog(clientState.EmpireState);
            battleReport.ShowDialog();
            battleReport.Dispose();
        }

        /// <Summary>
        /// Pop up the score report dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void ScoresMenuItem_Click(object sender, EventArgs e)
        {
            ScoreReport scoreReport = new ScoreReport(clientState.InputTurn.AllScores);
            scoreReport.ShowDialog();
            scoreReport.Dispose();
        }

        /// <Summary>
        /// Menu->Commands->Save & Submit
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void SaveAndSubmitTurnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // clientState.Save();
            OrderWriter orderWriter = new OrderWriter(clientState);
            orderWriter.WriteOrders();
            this.Close();
        }

        /// <Summary>
        /// Load Next Turn
        /// </Summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void LoadNextTurnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NextTurn();
            //this.nextTurnQueued = true;
        }
        public void NextTurn() 
        {
            loadNextTurnToolStripMenuItem.Enabled = false;
            
            // this.Update();
            // clientState.Save();
            OrderWriter orderWriter = new OrderWriter(clientState);
            orderWriter.WriteOrders();



            
            // prepare the arguments that will tell how to re-initialize.
            CommandArguments commandArguments = new CommandArguments();
            Application.DoEvents();
            commandArguments.Add(CommandArguments.Option.RaceName, clientState.EmpireState.Race.Name);
            //Application.DoEvents();
            commandArguments.Add(CommandArguments.Option.Turn, clientState.EmpireState.TurnYear + 1);
            commandArguments.Add(CommandArguments.Option.IntelFileName, clientState.IntelFileName);
            Application.DoEvents();
            string turnFlagFileName = System.IO.Path.Combine(clientState.GameFolder, "Client" + Global.TurnFlagExtension); //little file with just the turnyear
            byte[] turnYear = new byte[10];
            string turnYearStr = "";
            while (turnYearStr.TrimEnd() != (clientState.EmpireState.TurnYear + 1).ToString())
            {
                System.IO.Stream turnFlagFile = new FileStream(turnFlagFileName /*+ ".xml"*/, FileMode.OpenOrCreate);
                turnFlagFile.Read(turnYear, 0, 10);
                turnYearStr = Encoding.Default.GetString(turnYear);
                turnFlagFile.Close();
                Application.DoEvents();
            }
            clientState.Initialize(commandArguments.ToArray(),true);

            // this.selectionDetail.emp = clientState.pl
            // this.selectionDetail. = clientState;
            // this.empireState = clientState.EmpireState;
            // this.commands = clientState.Commands;
            selectionDetail.ReInitialize(clientState.EmpireState, clientState);

            selectionSummary.ReInitialize( clientState.EmpireState);

            reinitializeControls();

            this.doNextTurn();
            Application.DoEvents();
            loadNextTurnToolStripMenuItem.Enabled = true;
            

        }

        /// <Summary>
        /// Reacts to Fleet selection information. 
        /// </Summary>
        /// <param name="sender">
        /// A <see cref="System.Object"/>The source of the event.</param>
        /// <param name="e">A <see cref="FleetSelectionArgs"/> that contains the event data.</param>
        public void DetailChangeSelection(object sender, SelectionArgs e)
        {
            this.SelectionDetails.Value = e.Selection;
        }
        
        /// <Summary>
        /// Reacts to Star selection information. 
        /// </Summary>
        /// <param name="sender">
        /// A <see cref="System.Object"/>The source of the event.</param>
        /// <param name="e">A <see cref="FleetSelectionArgs"/> that contains the event data.</param>
        public void SummaryChangeSelection(object sender, SelectionArgs e)
        {
            this.SelectionSummary.Value = e.Selection;
        }

        /// <Summary>
        /// Load controls with any data we may have for them.
        /// </Summary>
        public void initializeControls()
        {
            this.Messages.Year = clientState.EmpireState.TurnYear;
            this.messages.stars = clientState.EmpireState.StarReports;
            this.messages.visibleMinefields = clientState.EmpireState.VisibleMinefields;
            this.messages.knownFleets = clientState.EmpireState.FleetReports;
            this.Messages.MessageList = clientState.Messages;

            this.CurrentTurn = clientState.EmpireState.TurnYear;
            this.CurrentRace = clientState.EmpireState.Race.Name;

            this.MapControl.initialize(clientState);
            loadNextTurnToolStripMenuItem.Enabled = true;



            // Select a Star owned by the player (if any) as the default display.

            foreach (StarIntel report in clientState.EmpireState.StarReports.Values)
            {
                if (report.Owner == clientState.EmpireState.Id)
                {
                    MapControl.SetCursor(report.Position);
                    MapControl.CenterMapOnPoint(report.Position);
                    if (SelectionDetails != null)
                    {
                        SelectionDetails.Value = report;
                        SelectionSummary.Value = report;
                        break;
                    }
                }
            }
        }
        public void reinitializeControls()
        {
            this.Messages.Year = clientState.EmpireState.TurnYear;
            this.messages.stars = clientState.EmpireState.StarReports;
            this.messages.visibleMinefields = clientState.EmpireState.VisibleMinefields;
            this.messages.knownFleets = clientState.EmpireState.FleetReports;
            this.Messages.MessageList = clientState.Messages;

            this.CurrentTurn = clientState.EmpireState.TurnYear;
            this.CurrentRace = clientState.EmpireState.Race.Name;

            this.MapControl.reinitialize(clientState);
            loadNextTurnToolStripMenuItem.Enabled = true;



            // Select a Star owned by the player (if any) as the default display.

            foreach (StarIntel report in clientState.EmpireState.StarReports.Values)
            {
                if (report.Owner == clientState.EmpireState.Id)
                {
                    MapControl.SetCursor(report.Position);
                    MapControl.CenterMapOnPoint(report.Position);
                    if (SelectionDetails != null)
                    {
                        SelectionDetails.Value = report;
                        SelectionSummary.Value = report;
                        break;
                    }
                }
            }
        }

        /// <Summary>
        /// Refresh the display for a new turn.
        /// </Summary>
        public void doNextTurn()
        {
            Messages.Year = clientState.EmpireState.TurnYear;
            Messages.MessageList = clientState.Messages;

            Invalidate(true);

            MapControl.reinitialize(clientState);
            MapControl.Invalidate();

            // Select a Star owned by the player (if any) as the default display.

            foreach (StarIntel report in clientState.EmpireState.StarReports.Values)
            {
                if (report.Owner == clientState.EmpireState.Id)
                {
                    MapControl.SetCursor((System.Drawing.Point)report.Position);
                    if (SelectionDetails != null)   SelectionDetails.Value = clientState.EmpireState.OwnedStars[report.Name];
                    if (SelectionSummary != null)   SelectionSummary.Value = report;
                    break;
                }
            }
        }

        /// <Summary>
        /// Makes the Planet Detail reflect new research budgets. 
        /// </Summary>
        /// <returns>
        /// A <see cref="System.Boolean"/> indicating if the planet Detail
        /// was updated or not.
        /// </returns>
        private bool UpdateResearchBudgets()
        {
            if (SelectionDetails.isPlanetDetail())
            {
                SelectionDetails.Value = SelectionDetails.Reload();
                return true;
            }
            
            return false;
        }



        private void toolsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //nextTurnQueued = true;
            NextTurn();
        }
    }
}
