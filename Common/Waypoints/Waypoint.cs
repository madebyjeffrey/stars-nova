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

namespace Nova.Common.Waypoints
{
    using System;
    using System.Drawing;
    using System.Runtime.Versioning;
    using System.Xml;

    using Nova.Common.DataStructures;

    /// <summary>
    /// Waypoints have a position (i.e. where to go), a destination description
    /// (e.g. a star name), a speed to go there and a task to do on arrival (e.g. 
    /// colonise).
    /// and a chronological timestamp for processing SplitMerge orders
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class Waypoint
    {
        public NovaPoint Position
        {
            get;
            set;
        }

        public int WarpFactor
        {
            get;
            set;
        }



        public IWaypointTask Task
        {
            get;
            set;
        }

        public string Destination
        {
            get;
            set;
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public Waypoint()
        {
            WarpFactor = 6;
            Task = new NoTask();
            //guiTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Copies everything about another Waypoint, except the Task.
        /// Used for editing purposes.
        /// </summary>
        /// <param name="other">Waypoint to semi clone.</param>
        public Waypoint(Waypoint other)
        {
            Position = other.Position;
            WarpFactor = other.WarpFactor;
            Destination = other.Destination;
        }
        public NovaPoint destinationCurrentPosition(EmpireData empire)

        {  //TODO refactor the current isolated methods for determining "nearness" into this method
            // when following another fleet it is necessary to look at that fleets velocity and position and "guess" the position of that fleet
            // at the end of the turn and travel to the guessed position, the actual position of the target may differ buy some amount of rounding error to where this fleet went to.
            // so allow a small (rounding error) discrepancy  between the two positions.
            StarIntel star = null;
            empire.StarReports.TryGetValue(Destination, out star);
            if (star == null) // the long search 
                foreach (StarIntel starIntel in empire.StarReports.Values)
                {
                    if (starIntel.Position.distanceToSquared(Position) < 1.4143)
                    {
                        star = starIntel;
                    }
                }

            if (star == null) return null;
            else return star.Position;
        }      

        /// <summary>
        /// Load from XML: initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">A node is a "Waypoint" node Nova save file (xml document).
        /// </param>
        public Waypoint(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "destination":
                            Destination = mainNode.FirstChild.Value;
                            break;

                        case "warpfactor":
                            WarpFactor = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;

                        case "guitimestamp":
                            DateTime.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                            break;

                        case "position":
                            Position = new NovaPoint(mainNode);
                            break;
                            
                        default:
                            LoadTask(mainNode.Name.ToString(), mainNode);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Report.FatalError(e.Message + "\n Details: \n" + e.ToString());
                }

                mainNode = mainNode.NextSibling;
            }
        }

        public IWaypointTask LoadTask(string taskName, XmlNode node)
        {
            if (!taskName.Contains("Task"))
            {
                taskName += "Task";
            }

            taskName.Replace(" ", "");

            switch (taskName.ToLower())
            {
                case "cargotask":
                    Task = new CargoTask(node);
                    break;
                case "colonisetask":
                    Task = new ColoniseTask(node);
                    break;
                case "invadetask":
                    Task = new InvadeTask(node);
                    break;
                case "layminestask":
                    Task = new LayMinesTask(node);
                    break;
                case "scraptask":
                    Task = new ScrapTask(node);
                    break;
                case "splitmergetask":
                    Task = new SplitMergeTask(node);
                    break;
                default:
                    Task = new NoTask();
                    break;
            }

            return Task;
        }
        public IWaypointTask LoadTask(string taskName, XmlNode node, Mappable target)
        {
            if (!taskName.Contains("Task"))
            {
                taskName += "Task";
            }

            taskName.Replace(" ", "");

            switch (taskName.ToLower())
            {
                case "unload cargotask":
                    if (node == null)
                    {
                        CargoTask wpTask = new CargoTask(int.MaxValue);
                        wpTask.Target = target;
                        Task = wpTask;
                    }
                    else Task = new CargoTask(node);
                    break;
                case "cargotask":
                    Task = new CargoTask(node);
                    break;
                case "colonisetask":
                    Task = new ColoniseTask(node);
                    break;
                case "invadetask":
                    if (node == null)
                    {
                        InvadeTask wpTask = new InvadeTask();
                        Task = wpTask;
                    }
                    else Task = new InvadeTask(node);
                    break;
                case "lay minestask":
                    Task = new LayMinesTask(node);
                    break;
                case "scraptask":
                    Task = new ScrapTask(node);
                    break;
                case "splitmergetask":
                    Task = new SplitMergeTask(node);
                    break;
                default:
                    Task = new NoTask();
                    break;
            }

            return Task;
        }


        /// <summary>
        /// Save: Serialize this Waypoint to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the Waypoint.</returns>
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            XmlElement xmlelWaypoint = xmldoc.CreateElement("Waypoint");

            Global.SaveData(xmldoc, xmlelWaypoint, "Destination", Destination);
            
            if (Position != null)
            {
                xmlelWaypoint.AppendChild(Position.ToXml(xmldoc, "Position"));
            }

            Global.SaveData(xmldoc, xmlelWaypoint, "WarpFactor", WarpFactor.ToString(System.Globalization.CultureInfo.InvariantCulture));
            //Global.SaveData(xmldoc, xmlelWaypoint, "guiTimestamp", guiTimestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlelWaypoint.AppendChild(Task.ToXml(xmldoc));

            return xmlelWaypoint;
        }        
    }
}
