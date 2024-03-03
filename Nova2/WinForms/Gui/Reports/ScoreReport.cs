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

namespace Nova.WinForms.Gui
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Windows.Forms;
    
    using Nova.Client;
    using Nova.Common;
        
    /// <Summary>
    /// Score Summary report dialog class
    /// </Summary>
    [SupportedOSPlatform("windows")]
    public partial class ScoreReport : Form
    {
        private List<ScoreRecord> allScores;
        
        /// <Summary>
        /// Initializes a new instance of the ScoreReport class.
        /// </Summary>
        public ScoreReport(List<ScoreRecord> allScores)
        {            
            this.allScores = allScores;
            
            InitializeComponent();
        }


        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Populate the display. 
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        /// ----------------------------------------------------------------------------
        private void OnLoad(object sender, EventArgs e)
        {
            this.scoreGridView.AutoSize = true;

            foreach (ScoreRecord score in allScores)
            {

                DataGridViewRow RowDef = new DataGridViewRow();
                DataGridViewCell RaceIcon = new DataGridViewImageCell();
                RowDef.Cells.Add(RaceIcon);
                DataGridViewCell Race = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Race);
                DataGridViewCell Rank = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Rank);
                DataGridViewCell Score = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Score);
                DataGridViewCell Planets = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Planets);
                DataGridViewCell Starbases = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Starbases);
                DataGridViewCell UnarmedShips = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(UnarmedShips);
                DataGridViewCell EscortShips = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(EscortShips);
                DataGridViewCell CapitalShips = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(CapitalShips);
                DataGridViewCell TechLevel = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(TechLevel);
                DataGridViewCell Resources = new DataGridViewTextBoxCell();
                RowDef.Cells.Add(Resources);

                RowDef.Height = 64;

                RowDef.Cells[0].Value = score.EmpireIcon;
                RowDef.Cells[1].Value = score.Empire.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[2].Value = score.Rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[3].Value = score.Score.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[4].Value = score.Planets.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[5].Value = score.Starbases.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[6].Value = score.UnarmedShips.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[7].Value = score.EscortShips.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[8].Value = score.CapitalShips.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[9].Value = score.TechLevel.ToString(System.Globalization.CultureInfo.InvariantCulture);
                RowDef.Cells[10].Value = score.Resources.ToString(System.Globalization.CultureInfo.InvariantCulture);

                this.scoreGridView.Rows.Add(RowDef);
            }

            this.scoreGridView.AutoResizeColumns();
        }

    }
}
