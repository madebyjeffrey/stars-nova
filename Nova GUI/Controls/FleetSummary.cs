// ============================================================================
// Nova. (c) 2008 Ken Reed
//
// Fleet summary display panel.
//
// This is free software. You can redistribute it and/or modify it under the
// terms of the GNU General Public License version 2 as published by the Free
// Software Foundation.
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using NovaCommon;
using NovaClient;

// ============================================================================
// The fleet summary panel.
// ============================================================================

namespace Nova
{
   public partial class FleetSummary : UserControl
   {

// ============================================================================
// Construction.
// ============================================================================

      public FleetSummary()
      {
         InitializeComponent();
      }


// ============================================================================
// Display the fleet summary
// ============================================================================

      private void DisplaySummary(Fleet fleet)
      {
         string race           = fleet.Owner;
         FleetShipCount.Text   = fleet.Composition.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
         FleetMass.Text        = fleet.TotalMass.ToString(System.Globalization.CultureInfo.InvariantCulture);
         FleetSpeed.Text       = fleet.Speed.ToString(System.Globalization.CultureInfo.InvariantCulture);
         FleetImage.Image      = fleet.Image;
         FleetOwner.Text       = race;

         RaceIcon.Image = ClientState.Data.InputTurn.RaceIcons[race] as Image;
      }


// ============================================================================
// Select the fleet whose details are to be displayed
// ============================================================================

      public Fleet Value {
         set { DisplaySummary(value); }
      }
   }
}
