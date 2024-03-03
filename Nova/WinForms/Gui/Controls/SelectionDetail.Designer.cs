using Nova.Common;
using System;

namespace Nova.WinForms.Gui
{
   public partial class SelectionDetail
   {
      /// <Summary> 
      /// Required designer variable.
      /// </Summary>
      private System.ComponentModel.IContainer components = null;

      /// <Summary> 
      /// Clean up any resources being used.
      /// </Summary>
      /// <param name="disposing">Set to true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null)) 
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

        #region Component Designer generated code

        /// <Summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </Summary>
        private void InitializeComponent()
        {
            this.planetDetail = new PlanetDetail(empireState, clientState);
            this.fleetDetail = new FleetDetail(clientState);
            this.fleetDetail.setPlanetMode += this.setPlanetDetail;
            this.fleetDetail.setFleetMode += this.setFleetDetail;
            this.planetDetail.SetPlanetMode += this.setPlanetDetail;
            this.planetDetail.SetFleetMode += this.setFleetDetail;
            this.SuspendLayout();
            // 
            // planetDetail1
            // 
            this.planetDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.planetDetail.Location = new System.Drawing.Point(0, 0);
            this.planetDetail.Margin = new System.Windows.Forms.Padding(0);
            this.planetDetail.Name = "planetDetail1";
            this.planetDetail.Size = new System.Drawing.Size(461, 460);
            this.planetDetail.TabIndex = 0;
            // 
            // fleetDetail1
            // 
            this.fleetDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fleetDetail.Location = new System.Drawing.Point(0, 0);
            this.fleetDetail.Margin = new System.Windows.Forms.Padding(0);
            this.fleetDetail.Name = "fleetDetail1";
            this.fleetDetail.Size = new System.Drawing.Size(461, 460);
            this.fleetDetail.TabIndex = 1;
            // 
            // SelectionDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.planetDetail);
            this.Controls.Add(this.fleetDetail);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SelectionDetail";
            this.Size = new System.Drawing.Size(461, 460);
            this.ResumeLayout(false);

        }
        private void ReInitializeComponent()
        {
            this.planetDetail.ReInitialise(empireState, clientState);
            this.fleetDetail.ReInitialise(clientState);
            this.SuspendLayout();

        }
        private void setPlanetDetail(object sender, SelectionArgs e)
        {
            planetMode = true;
            fleetMode = false;
            this.Value = e.Selection;
            //planetDetail.SetStarDetails(e.Selection as Star);
            //fleetDetail.SetFleetDetails (null);
        }
        private void setFleetDetail(object sender, SelectionArgs e)
        {
            planetMode = false;
            fleetMode = true;
            this.Value = e.Selection;
            //planetDetail.ClearStar();
            //fleetDetail.SetFleetDetails(e.Selection as Fleet);
        }


        #endregion

        private PlanetDetail planetDetail;
        private FleetDetail fleetDetail;
        private bool planetMode;
        private bool fleetMode;


   }
}
