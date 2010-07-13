#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010 stars-nova
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
// This module is invoked (once) when a new turn has been received from the
// Nova Console. All the appropriate fields in the GUI state data are updated
// that are relevant to the player's selected race.
// ===========================================================================
#endregion

#region Using Statements

using System.Collections;
using System.IO;

using Nova.Common;
using Nova.Common.Components;

#endregion

namespace Nova.Client
{

   public static class IntelReader
   {
      private static Intel TurnData = null;
      private static ClientState StateData = null;

      /// ----------------------------------------------------------------------------
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
      ///    and is used to reconstruct the ClientState.Data (we can create a new one with 
      ///    no history if required, such as on the first turn)
      /// 3. process the .intel file to update the ClientState.Data
      ///
      /// Note that this file is not read again after the first time a new turn is
      /// received. Once fully loaded all further processing is done using 
      /// ClientState.Data (which is subsequently used to generate <RaceName>.orders.
      /// </summary>
      /// <param name="turnFileName">Path and file name of the RaceName.intel file.</param>
      /// ----------------------------------------------------------------------------
      public static void ReadIntel(string turnFileName)
      {
          if (!File.Exists(turnFileName))
          {
              Report.FatalError("The Nova GUI cannot start unless a turn file is present");
          }

          using (Stream turnFile = new FileStream(turnFileName, FileMode.Open))
          {
              int turnYearInFile = (int)Serializer.Deserialize(turnFile);


              // check this is a new turn, not the one just played
              if (turnYearInFile != ClientState.Data.TurnYear)
              {
                  Intel newIntel = Serializer.Deserialize(turnFile)
                                            as Intel;
                  ClientState.Data.RaceName = newIntel.MyRace.Name;
                  ClientState.Data.GameFolder = Path.GetDirectoryName(turnFileName);
                  ClientState.Restore();
                  ClientState.Data.InputTurn = newIntel;
                  ProcessIntel();

              }
              else
              {
                  // exit without saving any files
                  throw new System.Exception("Turn Year missmatch");
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// This function processes the ClientState.Data.TurnData for this turn
      /// and updates the ClientState.Data.
      /// </summary>
      /// ----------------------------------------------------------------------------
      public static void ProcessIntel()
      {
         StateData = ClientState.Data;

          // copy the raw data from the intel to StateData
         TurnData  = StateData.InputTurn;
         StateData.TurnYear = TurnData.TurnYear;
         StateData.PlayerRace = TurnData.MyRace;

          // Clear old turn data from StateData
         StateData.DeletedFleets.Clear();
         StateData.DeletedDesigns.Clear();
         StateData.Messages.Clear();
         
          // Process the new intel
         DetermineOrbitingFleets();
         DeterminePlayerStars();
         DeterminePlayerFleets();

         ProcessMessages();
         ProcessFleets();
         ProcessReports();
         ProcessResearch();
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Run through the full list of messages and populate the message store in the
      /// state data with the messages relevant to the player's selected race. The
      /// actual message control will be populated within the main window
      /// initialisation.
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void ProcessMessages()
      {
          foreach (Message message in TurnData.Messages)
          {
              if ((message.Audience == ClientState.Data.RaceName) ||
                  (message.Audience == "*"))
              {
                  StateData.Messages.Add(message);
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// So that we can put an indication of fleets orbiting a star run through all
      // the fleets and, if they are in orbit around a star, set the OrbitingFleets
      // flag in the star.
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void DetermineOrbitingFleets()
      {
          foreach (Star star in TurnData.AllStars.Values)
          {
              star.OrbitingFleets = false;
          }

          foreach (Fleet fleet in TurnData.AllFleets.Values)
          {
              if (fleet.InOrbit != null && fleet.Type != "Starbase")
              {
                  Star star = fleet.InOrbit;
                  star.OrbitingFleets = true;
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Advance the age of all star reports by one year. Then, if a star is owned by
      /// us and has colonists bring the report up to date (just by creating a new
      /// report).
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void ProcessReports()
      {
          foreach (StarReport report in StateData.StarReports.Values)
          {
              report.Age++;
          }

          foreach (Star star in StateData.PlayerStars.Values)
          {
              if (star.Colonists != 0)
              {
                  StateData.StarReports[star.Name] = new StarReport(star);
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      ///  Process Fleet Reports
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void ProcessFleets()
      {
          // update the state data with the current fleets
          foreach (Fleet fleet in StateData.InputTurn.AllFleets.Values)
          {
              if (fleet.Owner == StateData.PlayerRace.Name)
              {
                  StateData.PlayerFleets.Add(fleet);


                  if (fleet.IsStarbase)
                  {
                      // update the reference from the star to its starbase and vice versa (the fleet should know the name of the star, but the reference is a dummy)
                      if ((fleet.InOrbit == null) || (fleet.InOrbit.Name == null))
                      {
                          Report.FatalError("Starbase doesn't know what planet it is orbiting!");
                      }
                      Star star = ClientState.Data.PlayerStars[fleet.InOrbit.Name] as Star;
                      star.Starbase = fleet;
                      fleet.InOrbit = star;
                  }

                  // --------------------------------------------------------------------------------
                  // FIXME (priority 5) - discovery of planetary information should be done by the server. It should not be possible for a hacked client to get this information.

                  if ((fleet.InOrbit != null) && (!fleet.IsStarbase))
                  {

                      // add to orbiting fleets list
                      Star star = fleet.InOrbit;
                      StateData.StarReports[star.Name] = new StarReport(star);


                  }

                  if (fleet.ShortRangeScan != 0)
                  {
                      foreach (Star star in TurnData.AllStars.Values)
                      {
                          if (PointUtilities.Distance(star.Position, fleet.Position)
                              <= fleet.ShortRangeScan)
                          {
                              StateData.StarReports[star.Name] = new StarReport(star);
                          }
                      }
                  }
                  // END OF FIX ME --------------------------------------------------------------------------------


              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Do the research for this year. Research is performed locally once per turn.
      /// </summary>
      /// <remarks>
      /// FIXME (priority 5) Console should determine the results of research and tell
      /// the Nova GUI, not the other way around.      
      /// </remarks>
      /// ----------------------------------------------------------------------------
      private static void ProcessResearch()
      {
          TechLevel.ResearchField area = StateData.ResearchTopic;
          int areaResource = (int)StateData.ResearchResources[area];
          int areaLevel = (int)StateData.ResearchLevel[area];
          areaResource += (int)StateData.ResearchAllocation;

          StateData.ResearchAllocation = 0;
          StateData.ResearchResources[area] = areaResource;

          while (true)
          {
              if (areaResource >= Research.Cost(areaLevel + 1))
              {
                  areaLevel++;
                  ReportLevelUpdate(area, areaLevel);
              }
              else
              {
                  break;
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Report an update in tech level and any new components that have became
      /// available.
      /// </summary>
      /// <param name="area">The field of research.</param>
      /// <param name="level">The new level obtained.</param>
      /// ----------------------------------------------------------------------------
      private static void ReportLevelUpdate(TechLevel.ResearchField area, int level)
      {
          Message techAdvanceMessage = new Message(
              ClientState.Data.RaceName,
              null,
              "Your race has advanced to Tech Level " + level + " in the " + StateData.ResearchTopic + " field");
          StateData.Messages.Add(techAdvanceMessage);

          Hashtable allComponents = AllComponents.Data.Components;
          TechLevel oldResearchLevel = StateData.ResearchLevel;
          TechLevel newResearchLevel = new TechLevel(oldResearchLevel);

          newResearchLevel[area] = level;

          foreach (Component component in allComponents.Values)
          {
              if (component.RequiredTech > oldResearchLevel &&
                  component.RequiredTech <= newResearchLevel)
              {

                  ClientState.Data.AvailableComponents.Add(component);
                  Message newComponentMessage = new Message(
                      ClientState.Data.RaceName,
                      null,
                      "You now have available the " + component.Name + " " + component.Type + " component");
                  ClientState.Data.Messages.Add(newComponentMessage);
              }
          }

          StateData.ResearchLevel = newResearchLevel;
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Determine the fleets owned by the player (this is a convenience function so
      /// that buttons such as "Next" and "Previous" on the ship detail panel are easy
      /// to code.
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void DeterminePlayerFleets()
      {
          StateData.PlayerFleets.Clear();

          foreach (Fleet fleet in TurnData.AllFleets.Values)
          {
              if (fleet.Owner == StateData.RaceName)
              {
                  if (fleet.Type != "Starbase")
                  {
                      StateData.PlayerFleets.Add(fleet);
                  }
              }
          }
      }


      /// ----------------------------------------------------------------------------
      /// <summary>
      /// Determine the star systems owned by the player (this is a convenience
      // function so that buttons such as "Next" and "Previous" on the star detail
      // panel are easy to code,
      /// </summary>
      /// ----------------------------------------------------------------------------
      private static void DeterminePlayerStars()
      {
          StateData.PlayerStars.Clear();

          foreach (Star star in TurnData.AllStars.Values)
          {
              if (star.Owner == StateData.RaceName)
              {
                  StateData.PlayerStars.Add(star);
              }
          }
      }

   }
}
