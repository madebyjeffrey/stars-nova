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

#region Module Description
// ===========================================================================
// A control to display user messages.
// ===========================================================================
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Nova.Common;
using Nova.Common.DataStructures;
using System.Linq;


namespace Nova.WinForms.Gui
{
    /// <Summary>
    /// A control to display user messages.
    /// </Summary>
    public class Messages : System.Windows.Forms.UserControl
    {
        public Dictionary<string, StarIntel> stars;            //TODO priority(3) is it better to convert the message.event keys in LinkIntelReferences() or here?
        public Dictionary<long, Minefield> visibleMinefields;
        public Dictionary<long, FleetIntel> knownFleets;
        public event EventHandler<SelectionArgs> doStarIntelStuff = null;
        public event EventHandler<SelectionArgs> doFleetIntelStuff = null;
        public event EventHandler<SelectionArgs> doMineIntelStuff = null;
        public event EventHandler<SelectionArgs> doPositionIntelStuff = null;
        private List<Common.Message> messages;
        private int currentMessage;
        private int turnYear;

        #region VS-Generated Variables
        private System.Windows.Forms.GroupBox messageForm;
        private System.Windows.Forms.Label messageBox;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button previousButton;
        private Button gotoButton;
        private Button GotoFleetButton;
        private System.ComponentModel.Container components = null;
        #endregion

        #region Construction and Disposal

        /// <Summary>
        /// Initializes a new instance of the Messages class.
        /// </Summary>
        public Messages()
        {
            InitializeComponent();
        }


        /// <Summary>
        /// Clean up any resources being used.
        /// </Summary>
        /// <param name="disposing">Set to true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Component Designer generated code
        /// <Summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </Summary>
        private void InitializeComponent()
        {
            this.messageForm = new System.Windows.Forms.GroupBox();
            this.gotoButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.messageBox = new System.Windows.Forms.Label();
            this.GotoFleetButton = new System.Windows.Forms.Button();
            this.messageForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // messageForm
            // 
            this.messageForm.Controls.Add(this.GotoFleetButton);
            this.messageForm.Controls.Add(this.gotoButton);
            this.messageForm.Controls.Add(this.previousButton);
            this.messageForm.Controls.Add(this.nextButton);
            this.messageForm.Controls.Add(this.messageBox);
            this.messageForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageForm.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.messageForm.Location = new System.Drawing.Point(0, 0);
            this.messageForm.Name = "messageForm";
            this.messageForm.Size = new System.Drawing.Size(450, 180);
            this.messageForm.TabIndex = 0;
            this.messageForm.TabStop = false;
            this.messageForm.Text = "Year 2100 - No Messages";
            // 
            // gotoButton
            // 
            this.gotoButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.gotoButton.Location = new System.Drawing.Point(8, 84);
            this.gotoButton.Name = "gotoButton";
            this.gotoButton.Size = new System.Drawing.Size(75, 23);
            this.gotoButton.TabIndex = 3;
            this.gotoButton.Text = "Go To";
            this.gotoButton.Click += new System.EventHandler(this.GotoButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.previousButton.Location = new System.Drawing.Point(8, 53);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 2;
            this.previousButton.Text = "Previous";
            this.previousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.nextButton.Location = new System.Drawing.Point(8, 24);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 1;
            this.nextButton.Text = "Next";
            this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // messageBox
            // 
            this.messageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.messageBox.BackColor = System.Drawing.Color.White;
            this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.messageBox.Location = new System.Drawing.Point(104, 25);
            this.messageBox.Name = "messageBox";
            this.messageBox.Size = new System.Drawing.Size(340, 146);
            this.messageBox.TabIndex = 0;
            // 
            // GotoFleetButton
            // 
            this.GotoFleetButton.Enabled = false;
            this.GotoFleetButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GotoFleetButton.Location = new System.Drawing.Point(8, 116);
            this.GotoFleetButton.Name = "GotoFleetButton";
            this.GotoFleetButton.Size = new System.Drawing.Size(75, 23);
            this.GotoFleetButton.TabIndex = 5;
            this.GotoFleetButton.Text = "Go To Fleet";
            this.GotoFleetButton.Visible = false;
            this.GotoFleetButton.Click += new System.EventHandler(this.GotoFleetButton_Click);
            // 
            // Messages
            // 
            this.Controls.Add(this.messageForm);
            this.Name = "Messages";
            this.Size = new System.Drawing.Size(450, 180);
            this.messageForm.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Event Methods

        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Process the Next button being pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        /// ----------------------------------------------------------------------------
        private void NextButton_Click(object sender, System.EventArgs e)
        {
            if (currentMessage < messages.Count - 1)
            {
                currentMessage++;
                SetMessage();

                if (currentMessage == 1)
                {
                    previousButton.Enabled = true;
                }

                if (currentMessage == messages.Count - 1)
                {
                    nextButton.Enabled = false;
                }
            }
        }


        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Process the previous button being pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        /// ----------------------------------------------------------------------------
        private void PreviousButton_Click(object sender, System.EventArgs e)
        {
            if (currentMessage > 0)
            {
                currentMessage--;
                SetMessage();

                if (currentMessage == 0)
                {
                    previousButton.Enabled = false;
                }
            }
            nextButton.Enabled = true;
        }


        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Go to event button pressed.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        /// ----------------------------------------------------------------------------
        private void GotoButton_Click(object sender, EventArgs e)
        {
            Nova.Common.Message thisMessage = messages[currentMessage];

            if (thisMessage.Type == "BattleReport") //an enumerated type would be less error prone
            {
                DoDialog(new BattleViewer(thisMessage.Event as BattleReport));
            }
            string[] minefields = { "Minefield", "New Minefield", "Increase Minefield" };
            if (minefields.Contains(thisMessage.Type))
            {
                try
                {
                    if (thisMessage.Event is Minefield)
                    {// our minefield or the fleet hit the minefield but still has at least one ship with a scanner
                        SelectionArgs positionArg = new SelectionArgs((thisMessage.Event as Minefield));
                        if (doMineIntelStuff != null) doMineIntelStuff(sender, positionArg);
                    }
                    else
                    {// the fleet hit the minefield and blew up so we can't see minefield and we have no VisibleMinefield record for the minefield (just the last reported position)
                        SelectionArgs positionArg = new SelectionArgs((thisMessage.Event as Mappable));
                        if (doPositionIntelStuff != null) doPositionIntelStuff(sender, positionArg);
                    }
                }
                catch { }
            }
            if (thisMessage.Event is FleetIntel)
            {
                SelectionArgs positionArg = new SelectionArgs((thisMessage.Event as FleetIntel));
                if (doFleetIntelStuff != null) doFleetIntelStuff(sender, positionArg);
            }
            if (thisMessage.Event is StarIntel)
            {
                SelectionArgs positionArg = new SelectionArgs(thisMessage.Event as StarIntel);
                if (doStarIntelStuff != null) doStarIntelStuff(sender, positionArg);
            }

        }
        /// <summary>
        /// For some events there is a Main Event Key and also a FleetID for a second fleet involved
        /// this button appears for those events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GotoFleetButton_Click(object sender, EventArgs e)
        {
            Nova.Common.Message thisMessage = messages[currentMessage];


            if ((thisMessage.FleetKey != null) && (thisMessage.FleetKey != 0))
            {
                FleetIntel fleet;
                knownFleets.TryGetValue(thisMessage.FleetKey, out fleet);
                if (fleet != null)
                {
                    SelectionArgs positionArg = new SelectionArgs(fleet);
                    if (doFleetIntelStuff != null) doFleetIntelStuff(sender, positionArg);
                }
            }


        }

        #endregion

        #region Utility Methods

        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Display a message in the message control.
        /// </Summary>
        /// ----------------------------------------------------------------------------
        public void SetMessage()
        {
            this.gotoButton.Enabled = false;
            this.GotoFleetButton.Enabled = false;
            this.GotoFleetButton.Visible = false;

            StringBuilder title = new StringBuilder();
            title.AppendFormat("Year {0} - ", turnYear);

            if (messages.Count != 0)
            {
                title.AppendFormat("Message {0} of {1}", currentMessage + 1, messages.Count);
            }
            else
            {
                title.AppendFormat("No Messages");
            }

            this.messageForm.Text = title.ToString();

            if (messages.Count > 0)
            {
                Nova.Common.Message thisMessage = new Nova.Common.Message();
                thisMessage = messages[currentMessage];
                messageBox.Text = thisMessage.Text;
                if (thisMessage.EventString != null)
                { 
                if (thisMessage.EventString.Contains("Star:"))
                {
                    StarIntel star = null;
                    stars.TryGetValue(thisMessage.EventString.Substring(6, thisMessage.EventString.Length-6), out star);
                    if (star != null) thisMessage.Event = star;
                }
                else
                {
                        long key = 0;
                        long.TryParse(thisMessage.EventString, out key);
                        if ((key != 0) && (key >= 0x40000000000000))
                        {
                            if (visibleMinefields.ContainsKey(key)) thisMessage.Event = visibleMinefields[key];  //minefield key
                            else
                            {
                                int Y = Global.MineFieldSnapToGridSize * (int)(key % 0x4000000);
                                int X = Global.MineFieldSnapToGridSize * (int)((key / 0x10000000) % 0x4000000);

                                thisMessage.Event = new Mappable(new NovaPoint(X, Y));
                            }
                        }
                        else if ((key != 0) && (key < 0x40000000000000))
                        {
                            if (knownFleets.ContainsKey(key)) thisMessage.Event = knownFleets[key];  //fleet key
                        }
                        //else thisMessage.Event = (object)thisMessage.EventString;  // BattleReport was loaded early so LinkIntelReferences() could populate it
                    }
                }

                if (thisMessage.Event != null)
                {
                    this.gotoButton.Enabled = true;
                }
                if ((thisMessage.FleetKey != null) && (thisMessage.FleetKey != 0))
                {
                    this.GotoFleetButton.Visible = true;
                    this.GotoFleetButton.Enabled = true;
                }
            }
        }

        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Clear a message in the message control at yearend.
        /// </Summary>
        /// ----------------------------------------------------------------------------
        public void ReSetMessage()
        {
             messageBox.Text = "";
        }


        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// General dialog handling
        /// </Summary>
        /// <param name="dialog">A dialog Form.</param>
        /// ----------------------------------------------------------------------------
        private void DoDialog(Form dialog)
        {
            dialog.ShowDialog();
            dialog.Dispose();
        }

        #endregion

        #region Properties

        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Get and set the turn year in the message control
        /// </Summary>
        /// ----------------------------------------------------------------------------
        public int Year
        {
            get { return turnYear; }
            set { turnYear = value; }
        }


        /// ----------------------------------------------------------------------------
        /// <Summary>
        /// Set the messages to be displayed.
        /// </Summary>
        /// ----------------------------------------------------------------------------
        public List<Common.Message> MessageList
        {
            set
            {
                messages = value;
                currentMessage = 0;
                previousButton.Enabled = false;

                if (messages.Count > 1)
                {
                    nextButton.Enabled = true;
                }
                else
                {
                    nextButton.Enabled = false;
                }
                ReSetMessage();
                SetMessage();
            }
        }

        #endregion
    }
}
