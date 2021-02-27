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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using Nova.Client;
    using Nova.Common;
    using Nova.Common.Commands;
    using Nova.Common.DataStructures; 
    using Nova.Common.Waypoints;
    using System.Collections.Generic;
    /// <Summary>
    /// StarMap is the control which holds the actual playing map. 
    /// </Summary>
    public partial class StarMap : UserControl
    {
        /// <Summary>
        /// This event should be fired when the StarMap requests the current
        /// selection information. Mostly used to assert where it is
        /// a fleet or a Star.
        /// </Summary>
        public event EventHandler<SelectionArgs> SelectionRequested;

        /// <Summary>
        /// These events should be fired when the users changes the
        /// selection in the map with the mouse. Use it to report
        /// selection changes to other components of the GUI.
        /// </Summary>    
        public event EventHandler<SelectionArgs> SelectionChanged;

        public event EventHandler<EventArgs> WaypointChanged;

        private readonly Point[] triangle =
        {
            new Point(0, 5),
            new Point(-5, -5),
            new Point(5, -5)
        };

        private readonly Point[] cursorShape =
        {
            new Point(0, 0),
            new Point(-5, -12),
            new Point(0, -9),
            new Point(5, -12)
        };

        private readonly Font nameFont;

        private Intel turnData;
        private ClientData clientState;
        private NovaPoint cursorPosition = new Point(0, 0);
        private NovaPoint lastClick = new Point(0, 0);

        private NovaPoint logical = new Point(0, 0);  // Size of the logical coordinate system (size of the game universe).        
        private NovaPoint extent = new Point(0, 0);   // How big is the logical map in terms of Size
        private NovaPoint displayOffset = new NovaPoint(0, 0); // If extent is less then the panel size this is used to center the map in the panel
        private NovaPoint scrollOffset = new NovaPoint(0, 0); // Where the scroll bars are set to
        private double zoomFactor = 1.0;              // Is used to adjust the Extent of the map.

        private readonly NovaPoint extraSpace = new NovaPoint(40, 40); // Extra padding round the map for star names etc.


        private bool isinitialized;
        private bool displayStarNames = true;
        private bool displayBackground = true;
        private bool displayBorders = false;

        private int selection;
        private const double MinZoom = 0.2;
        private const double MaxZoom = 5;


        private Dictionary<int, Bitmap> transparentRaceIcons = new Dictionary<int, Bitmap>();
        private Dictionary<int, Bitmap> redRaceIcons = new Dictionary<int, Bitmap>();
        private string graphicsFilePath = "";
        public string GraphicsPath
        {
            get
            {
                if (graphicsFilePath == "")
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

        /// <Summary>
        /// Initializes a new instance of the StarMap class.
        /// </Summary>
        public StarMap()
        {
            InitializeComponent();

            MouseWheel += StarMap_MouseWheel;
            Resize += delegate { Zoom(); };

            MapPanel.Paint += MapPanel_Paint;


            nameFont = new Font("Arial", (float)7.5, FontStyle.Regular, GraphicsUnit.Point);

            verticalScrollBar.GotFocus += delegate { MapPanel.Focus(); };
            horizontalScrollBar.GotFocus += delegate { MapPanel.Focus(); };

            MapPanel.ArrowKeyPressed += new KeyEventHandler(MapPanel_ArrowKeyPressed);
        }


        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawEverything(e.Graphics);
        }


        /// <Summary>
        /// Post-construction initialisation.
        /// </Summary>
        public void initialize(ClientData clientState)
        {
            this.clientState = clientState;

            GameSettings.Restore();

            // Initial map size
            logical.X = GameSettings.Data.MapWidth;
            logical.Y = GameSettings.Data.MapHeight;

            extent.X = (int)(this.logical.X * this.zoomFactor) + (extraSpace.X * 2);
            extent.Y = (int)(this.logical.Y * this.zoomFactor) + (extraSpace.Y * 2);

            turnData = this.clientState.InputTurn;
            isinitialized = true;

            horizontalScrollBar.Enabled = true;
            verticalScrollBar.Enabled = true;

            zoomFactor = 1.0;
            Zoom();
            Bitmap trans = Posturize(new Bitmap (clientState.EmpireState.Race.Icon.Image));
            trans.MakeTransparent();
            transparentRaceIcons.Add(clientState.EmpireState.Id, trans);
            Bitmap red = PosturizeRed(new Bitmap (clientState.EmpireState.Race.Icon.Image));
            redRaceIcons.Add(clientState.EmpireState.Id, red);
            foreach (EmpireIntel empire in clientState.EmpireState.EmpireReports.Values)
            {
                trans = Posturize(new Bitmap(empire.Icon.Image));
                trans.MakeTransparent();
                transparentRaceIcons.Add(empire.Id, trans);
                red = PosturizeRed(new Bitmap(empire.Icon.Image));
                redRaceIcons.Add(empire.Id, red);
            }

        }
        public void reinitialize(ClientData clientState)
        {
            this.clientState = clientState;

            //GameSettings.Restore();

            turnData = this.clientState.InputTurn;


        }

        /// <param name="graphics"></param>
        /// <Summary>
        /// Draws every object on the playing map to the graphics buffer.
        /// </Summary>
        /// <remarks>
        /// This does not actually render the map. Only draws to
        /// the buffer which is drawn later
        /// 
        /// We just do simple painting so the order that things are drawn is important
        /// (otherwise items may get overwritten and become invisible):
        ///
        /// (1) All long-range scanners (planets and ships) owned by the player.
        /// (2) All short-range scanners (ships only) owned by the player.
        /// (3) Minefields visible to the player (with transparency)
        /// (4) All fleets visible to the player.
        /// (5) Stars (including a starbase and orbiting fleets indication).
        /// (6) The selection cursor.
        /// </remarks>
        private void DrawEverything(Graphics g)
        {
            if (this.isinitialized == false)
            {
                return;
            }

            // Erase previous drawings.
            g.Clear(Color.Black);

            // (0) Draw the image backdrop and universe borders          
            NovaPoint backgroundOrigin = LogicalToDevice(new NovaPoint(0, 0));
            backgroundOrigin.Offset(-20, -20);
            NovaPoint backgroundExtent = LogicalToDevice(logical);
            backgroundExtent.Offset(20, 20);


            Size renderSize = new Size();
            renderSize.Height = backgroundExtent.Y - backgroundOrigin.Y;
            renderSize.Width = backgroundExtent.X - backgroundOrigin.X;

            g.Clip = new Region(new Rectangle((Point)backgroundOrigin, renderSize));


            // This is the specified area which represents the playing universe      
            Rectangle backgroundArea = new Rectangle((Point)backgroundOrigin, renderSize);

            if (this.displayBackground == true)
            {
                Image backdrop = Nova.Properties.Resources.Plasma;
                g.DrawImage(backdrop, backgroundArea);
                // Free the image after using it. This prevents a nasty
                // memory leak under Mono on Linux.
                backdrop.Dispose();
            }

            if (this.displayBorders == true)
            {
                Pen borderPen = new Pen(Brushes.DimGray);
                borderPen.DashStyle = DashStyle.Dot;
                g.DrawRectangle(borderPen, backgroundArea);
                borderPen.Dispose();
            }

            Color lrScancolor = Color.FromArgb(128, 128, 0, 0);
            SolidBrush lrScanBrush = new SolidBrush(lrScancolor);

            Color srScancolor = Color.FromArgb(128, 128, 128, 0);
            SolidBrush srScanBrush = new SolidBrush(srScancolor);

            // (1a) Planetary long-range scanners.

            foreach (Star report in clientState.EmpireState.OwnedStars.Values)
            {
                if (report.Owner == clientState.EmpireState.Id)
                {
                    DrawCircle(g, lrScanBrush, (Point)report.Position, report.ScanRange);
                }
            }

            // (1b) Fleet non-pen scanners.

            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.Owner == clientState.EmpireState.Id)
                {
                    DrawCircle(g, lrScanBrush, (Point)fleet.Position, fleet.ScanRange(clientState.EmpireState.Race, clientState.EmpireState));
                }
            }

            // (2) Fleet pen-scanners scanners.

            foreach (Fleet fleet in clientState.EmpireState.OwnedFleets.Values)
            {
                if (fleet.Owner == clientState.EmpireState.Id)
                {
                    DrawCircle(g, srScanBrush, (Point)fleet.Position, fleet.PenScanRange(clientState.EmpireState.Race, clientState.EmpireState));
                }
            }

            // (3) Minefields

            foreach (Minefield minefield in clientState.EmpireState.VisibleMinefields.Values)
            {
                if (minefield.MineType == 0)
                {
                    Color cb;
                    Color cf;

                    if (minefield.Empire == clientState.EmpireState.Id)
                    {
                        cb = Color.FromArgb(0, 0, 0, 0);
                        cf = Color.FromArgb(128, 0, 128, 0);
                    }
                    else
                    {
                        cb = Color.FromArgb(0, 0, 0, 0);
                        cf = Color.FromArgb(128, 128, 0, 128);
                    }


                    HatchStyle style = HatchStyle.DiagonalCross | HatchStyle.Percent50;
                    HatchBrush srMineBrush = new HatchBrush(style, cf, cb);
                    int radius = minefield.Radius;
                    DrawCircle(g, srMineBrush, (Point)minefield.Position, radius);
                }
                if (minefield.MineType == 1)
                {
                    Bitmap GoldMineField = (Bitmap)Image.FromFile(GraphicsPath+ "\\Mine_Layer\\MediumMineGoldSmall.png");
                    Color background = GoldMineField.GetPixel(0, 0);
                    GoldMineField.MakeTransparent(background);

                    Rectangle mines = new Rectangle(new Point(GoldMineField.Width / 2 - (int)(minefield.Radius * zoomFactor * 2), GoldMineField.Height / 2 - (int)(minefield.Radius * zoomFactor * 2)),new System.Drawing.Size ((int)(minefield.Radius* zoomFactor * 4), (int)(minefield.Radius * zoomFactor * 4 )));
                    Bitmap clipRegion = GoldMineField.Clone(mines, System.Drawing.Imaging.PixelFormat.DontCare);
                    Bitmap circle = new Bitmap(clipRegion.Width, clipRegion.Height, clipRegion.PixelFormat);
                    Graphics G = Graphics.FromImage(circle);
                    G.Clear(background);
                    TextureBrush TB = new TextureBrush(clipRegion);
                    G.FillEllipse(TB, 0, 0, (int)(minefield.Radius * zoomFactor * 4), (int)(minefield.Radius * zoomFactor * 4));

                    G.ScaleTransform((float)zoomFactor, (float)zoomFactor);

                    g.DrawImage(circle,new Point(minefield.Position.X,minefield.Position.Y));
                }
            }


            // (4) Stars plus starbases and orbiting fleet indications that are
            // the results of scans.

            foreach (StarIntel report in clientState.EmpireState.StarReports.Values)
            {
                if (radioButtonGrowth.Checked)
                {
                    DrawStarValueNova(g, report, clientState.EmpireState.Race, clientState.EmpireState.gravityModCapability, clientState.EmpireState.temperatureModCapability, clientState.EmpireState.radiationModCapability);
                    DrawStarMineralConcentration(g, report);
                }
                if (radioButtonMineralConcentration.Checked)
                {
                    DrawStarNormal(g, report);
                    DrawStarMineralConcentration(g, report);
                }
                if (radioButtonNovaValue.Checked) DrawStarValue(g, report, clientState.EmpireState.Race, clientState.EmpireState.gravityModCapability, clientState.EmpireState.temperatureModCapability, clientState.EmpireState.radiationModCapability);
                if (radioButtonNormal.Checked) DrawStarNormal(g, report);
                if (radioButtonPopulation.Checked) DrawStarPopulation(g, report);
                if (radioButtonSurfaceMinerals.Checked)
                {
                    DrawStarNormal(g, report);
                    DrawStarSurfaceMinerals(g, report);
                }
                DrawOrbitingFleets(g, report);
            }

            // (5) Visible fleets.

            foreach (FleetIntel report in clientState.EmpireState.FleetReports.Values)
            {
                if (report.Type != ItemType.Starbase)
                {
                    DrawFleet(g, report);
                }
            }


            // (6) Cursor.

            NovaPoint position = LogicalToDevice(this.cursorPosition);
            position.Y += 5;
            g.TranslateTransform(position.X, position.Y);
            g.RotateTransform(180f);
            g.FillPolygon(Brushes.Yellow, cursorShape);
            g.ResetTransform();
            // g.DrawImage(this.cursorBitmap, (Point)position);

            // (7) Zoom/Scroll/Cursor info for debugging.
            DrawDebugInfo(g);
        }

        [Conditional("DEBUG")]
        private void DrawDebugInfo(Graphics g)
        {
            g.ResetClip();
            Font font = this.Font;

            Color coordcolor = Color.FromArgb(255, 255, 255, 0);
            SolidBrush coordBrush = new SolidBrush(coordcolor);
            string str = "Cursor Location (logical): " + this.cursorPosition.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + this.cursorPosition.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            g.DrawString(str, font, coordBrush, 0, 20);
            str = "Zoom Factor: " + this.zoomFactor.ToString(System.Globalization.CultureInfo.InvariantCulture);
            g.DrawString(str, font, coordBrush, 0, 40);
            str = "ScrollOffset: " + scrollOffset;
            g.DrawString(str, font, coordBrush, 0, 60);
            str = "Extent: " + extent;
            g.DrawString(str, font, coordBrush, 0, 80);

            NovaPoint centerDisplay = new NovaPoint(MapPanel.Width / 2, MapPanel.Height / 2);
            NovaPoint zoomCenter = DeviceToLogical(centerDisplay);
            str = "Center Logical: " + zoomCenter;
            g.DrawString(str, font, coordBrush, 0, 100);
        }

        private void FillRectangle(Graphics g, Brush brush, Point position, int height, int offset)
        {
            g.FillRectangle(
                brush,
                position.X + 5 * offset,
                position.Y - 3 - (int)(height / 3.0 * zoomFactor),
                3,
                (int)(height / 3.0 * zoomFactor)
                );
        }

        Bitmap Posturize(Bitmap input)
        {
            Color background = input.GetPixel(0, 0);
            for (int row = 0; row < input.Height; row++)
            {
                for (int column = 0; column < input.Width; column++)
                    if ((Math.Abs((int)input.GetPixel(row, column).R - (int)background.R) < 8)
                    && (Math.Abs((int)input.GetPixel(row, column).G - (int)background.G) < 8)
                    && (Math.Abs((int)input.GetPixel(row, column).B - (int)background.B) < 8)) input.SetPixel(row, column, background);
            }
            return input;
        }

        Bitmap PosturizeRed(Bitmap input)
        {
            Color background = input.GetPixel(0, 0);
            for (int row = 0; row < input.Height; row++)
            {
                for (int column = 0; column < input.Width; column++)
                    if ((Math.Abs((int)input.GetPixel(row, column).R - (int)background.R) < 8)
                    && (Math.Abs((int)input.GetPixel(row, column).G - (int)background.G) < 8)
                    && (Math.Abs((int)input.GetPixel(row, column).B - (int)background.B) < 8)) input.SetPixel(row, column, Color.Red);
            }
            return input;
        }


        private void DrawRaceIcon(Graphics g, Image icon, Point position, int influence)
        {
            if (influence < 250000) influence = 250000;
            if (influence > 500000) influence = 500000;
            g.DrawImage(
                icon,
                position.X + 5,
                position.Y - 5 - (float)(icon.Height * zoomFactor / 4.0 * influence / 250000.0)
                , (float)(icon.Width * zoomFactor / 4.0 * influence / 250000.0)
                , (float)(icon.Height * zoomFactor / 4.0 * influence / 250000.0)
                );
        }

        private void DrawFleetRaceIcon(Graphics g, Image icon)  //the identifier for enemy fleets
        {
            g.DrawImage(
                icon,
                (float)(icon.Height * zoomFactor / 8.0),
                 0 - (float)(icon.Height * zoomFactor / 16.0)
                , (float)(icon.Width * zoomFactor / 8.0 )
                , (float)(icon.Height * zoomFactor / 8.0 )
                );
        }

        private void DrawIcon(Graphics g, Image icon, Point position)
        {
            Bitmap transparent = new Bitmap(icon);
            Color background = transparent.GetPixel(0, 0);
            transparent = Posturize(transparent); // TODO on large maps doing this multiple times adds extra overhead
            transparent.MakeTransparent(background);
            g.DrawImage(
                transparent,
                position.X,
                position.Y - (float)(icon.Height * zoomFactor / 4.0)
                , (float)(icon.Width * zoomFactor / 4.0)
                , (float)(icon.Height * zoomFactor / 4.0)
                );
        }

        private void DrawIcon(Graphics g, Image icon)
        {

            Bitmap transparent = new Bitmap(icon);
            Color background = transparent.GetPixel(0, 0);
            transparent = Posturize(transparent); // TODO on large maps doing this multiple times adds extra overhead
            transparent.MakeTransparent(background);
            g.DrawImage(
                transparent,
                0,
                0 - (float)(icon.Height * zoomFactor / 4.0)
                , (float)(icon.Width * zoomFactor / 4.0)
                , (float)(icon.Height * zoomFactor / 4.0)
                );
        }

        private void DrawIconRed(Graphics g, Image icon)
        {

            g.DrawImage(
                icon,
                0,
                0 - (float)(icon.Height * zoomFactor / 4.0)
                , (float)(icon.Width * zoomFactor / 4.0)
                , (float)(icon.Height * zoomFactor / 4.0)
                );
        }

        private void DrawEnemyIcon(Graphics g, Image icon)
        {

            int size = (int)(icon.Width * zoomFactor / 3.0);
            g.DrawEllipse(
                new Pen(Color.Yellow,(float)(8 * zoomFactor / 3.0))
                , -(float)(icon.Width * zoomFactor / 6.0)
                , -(float)(icon.Width * zoomFactor / 6.0)
                , size
                , size);
            size = size - 16;
            g.DrawEllipse(
                new Pen(Color.Indigo, (float)(8 * zoomFactor / 3.0))
                , 8 - (float)(icon.Width * zoomFactor / 6.0)
                , 8 - (float)(icon.Width * zoomFactor / 6.0)
                , size
                , size);
            size = size - 16;
            g.DrawEllipse(
                new Pen(Color.Gold, (float)(8 * zoomFactor / 3.0))
                , 16 - (float)(icon.Width * zoomFactor / 6.0)
                , 16 - (float)(icon.Width * zoomFactor / 6.0)
                , size
                , size);

            g.DrawImage(
                icon
                , -(float)(icon.Width * zoomFactor / 8.0)
                , -(float)(icon.Width * zoomFactor / 8.0)
                , (float)(icon.Width * zoomFactor / 4.0)
                , (float)(icon.Height * zoomFactor / 4.0)
                );
        }

        /// <Summary>
        /// Draw a filled circle using device coordinates.
        /// </Summary>
        /// <param name="brush"></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        private void FillCircle(Graphics g, Brush brush, Point position, int radius)
        {
            g.FillEllipse(
                brush,
                position.X - radius,
                position.Y - radius,
                radius * 2,
                radius * 2);
        }




        private void DrawCircle(Graphics g, Pen pen, Point position, int radius)
        {
            g.DrawArc(
                pen,
                position.X - radius,
                position.Y - radius,
                radius * 2,
                radius * 2,
                0,
                360);
        }


        private void FillCircleMinMax(Graphics g, Brush brushMin, Brush brushMax, Point position, int minRadius,int maxRadius)
        {
            g.FillPie(
                brushMin,
                position.X - (int)(minRadius * zoomFactor / 40.0)+1,
                position.Y - (int)(minRadius * zoomFactor / 40.0),
                (int)(minRadius * zoomFactor / 20.0) + 1,
                (int) (minRadius * zoomFactor / 20.0)+1,
                90,
                180);
            g.FillPie(
                brushMax,
                position.X - (int)(maxRadius * zoomFactor / 40.0),
                position.Y - (int)(maxRadius * zoomFactor / 40.0),
                (int)(maxRadius * zoomFactor / 20.0+1),
                (int)(maxRadius * zoomFactor / 20.0+1),
                270,
                180);
        }


        /// <Summary>
        /// Draw a filled circle using logical coordinates.
        /// </Summary>
        /// <param name="brush"></param>
        /// <param name="where"></param>
        /// <param name="logicalRadius"></param>
        private void DrawCircle(Graphics g, Brush brush, NovaPoint where, int logicalRadius)
        {
            if (logicalRadius == 0)
            {
                return;
            }

            NovaPoint position = LogicalToDevice(where);                        

            FillCircle(g, brush, (Point)position, (int)(logicalRadius * zoomFactor));
        }

        /// <Summary>
        /// Draw a fleet. We only draw fleets that are not in orbit. Indications of
        /// orbiting fleets are handled in the drawing of the Star.
        /// </Summary>
        /// <param name="fleet">The fleet to draw.</param>
        private void DrawFleet(Graphics g, FleetIntel report)
        {
            if (report.InOrbit == false)
            {
                NovaPoint position = LogicalToDevice(report.Position);

                if (radioButtonGrowth.Checked)
                {
                    g.TranslateTransform(position.X, position.Y);
                    if (report.Name.Contains("Mineral Packet"))
                    {
                        DrawIcon(g, report.Icon.Image);
                    }
                    else if ((report.Owner == clientState.EmpireState.Id) || (report.Name == "S A L V A G E"))
                    {
                        DrawIcon(g, report.Icon.Image);
                    }
                    else
                    {
                        DrawEnemyIcon(g, report.Icon.Image);
                    }
                }
                else
                {
                    g.TranslateTransform(position.X, position.Y);
                    g.RotateTransform((float)report.Bearing);
                    if (report.Name == "S A L V A G E") DrawIcon(g, report.Icon.Image);
                    else if (report.Owner == clientState.EmpireState.Id)
                    {
                        g.FillPolygon(Brushes.Blue, triangle);
                    }
                    else
                    {
                        g.FillPolygon(Brushes.Red, triangle);
                    }
                }
                g.ResetTransform();
                if (radioButtonGrowth.Checked)
                {
                     if (report.Owner != clientState.EmpireState.Id)
                    {
                        g.TranslateTransform(position.X, position.Y);
                        DrawFleetRaceIcon(g, redRaceIcons[report.Owner]);
                        g.ResetTransform();
                    }

                }

            }

            if (report.Owner == clientState.EmpireState.Id)
            {
                Fleet fleet = clientState.EmpireState.OwnedFleets[report.Key];
                if (fleet.Waypoints.Count > 0)
                {
                    Waypoint first = fleet.Waypoints[0];
                    NovaPoint from = LogicalToDevice(first.Position);

                    foreach (Waypoint waypoint in fleet.Waypoints)
                    {
                        NovaPoint position = waypoint.Position;

                        g.DrawLine(Pens.Blue, (Point)from, (Point)LogicalToDevice(position));
                        from = LogicalToDevice(position);
                    }
                }
            }
        }

        /// <Summary>
        /// Draw a Star. The Star is a circle whose size is proportional to the % value of the star
        /// Red = cannot be terraformed to positive population growth rate, 
        /// 1/2 red 1/2 green can be terraformed to positive population growth rate, left red 1/2 circle shows stars current value and right 1/2 circle shows max terraformed value, 
        /// green already are positive population growth rate
        /// </Summary>
        /// <remarks>
        /// The color of the Star symbol is based on its Star report (reports for stars
        /// owned by the current player are always up-to-date). 
        /// </remarks>
        /// <param name="Star">The Star sytem to draw.</param>
        private void DrawStarValueNova(Graphics g, StarIntel report,Race race, int gravityModCapability, int temperatureModCapability, int radiationModCapability)
        {
            NovaPoint position = LogicalToDevice(report.Position);
            int minValue = 2;
            int maxValue = 2;
            Brush starBrushMin = Brushes.White;
            Brush starBrushMax = Brushes.White;
            int size = 2;
            Brush starBrush = Brushes.White;
            if (report.Year == Global.Unset) FillCircle(g, starBrush, (Point)position, size);

            else
            {
                minValue = report.MinValue(race);
                maxValue = report.MaxValue(race, gravityModCapability, temperatureModCapability, radiationModCapability);
                if (minValue < 0) starBrushMin = Brushes.Red;
                else starBrushMin = Brushes.Green;
                if (maxValue < 0) starBrushMax = Brushes.Red;
                else starBrushMax = Brushes.Green;
                FillCircleMinMax(g, starBrushMin, starBrushMax ,(Point)position, Math.Abs( minValue), Math.Abs(maxValue));
                if (report.Owner != Global.Nobody)
                {
                    DrawRaceIcon(g, transparentRaceIcons[report.Owner], (Point)position, report.Colonists);
                }
            }





            // If the Star name display is turned on then add the name

            if (this.displayStarNames && zoomFactor > 0.5)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(report.Name, this.nameFont, Brushes.White, position.X, position.Y + 5, format);
            }
        }


        private void DrawStarValue(Graphics g, StarIntel report, Race race, int gravityModCapability, int temperatureModCapability, int radiationModCapability)
        {
            NovaPoint position = LogicalToDevice(report.Position);
            int minValue = 2;
            int maxValue = 2;
            Brush starBrushMooshedTogether = Brushes.White;

            int size = 2;
            Brush starBrush = Brushes.White;

            // Bigger symbol for explored stars.

            if (report.Year == Global.Unset) FillCircle(g, starBrush, (Point)position, size);


            else
            {
                minValue = report.MinValue(race);
                maxValue = report.MaxValue(race, gravityModCapability, temperatureModCapability, radiationModCapability);
                if (minValue >= 0)
                {
                    starBrushMooshedTogether = Brushes.Green;
                }


                if (maxValue <= 0)
                {
                    starBrushMooshedTogether = Brushes.Red;
                }

                if ((maxValue > 0) && (minValue < 0))
                {
                    starBrushMooshedTogether = Brushes.Yellow;
                }
                FillCircleMinMax(g, starBrushMooshedTogether, starBrushMooshedTogether, (Point)position, Math.Abs(maxValue), Math.Abs(maxValue));

            }


            // If the Star name display is turned on then add the name

            if (this.displayStarNames && zoomFactor > 0.5)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(report.Name, this.nameFont, Brushes.White, position.X, position.Y + 5, format);
            }
        }
        private void DrawStarPopulation(Graphics g, StarIntel report)
        {
            NovaPoint position = LogicalToDevice(report.Position);
            int minValue = 2;
            int maxValue = 2;
            Brush starBrushPopulation = Brushes.White;

            int size = 2;
            Brush starBrush = Brushes.White;

            // Bigger symbol for explored stars.

            if (report.Owner == (ushort)Global.Nobody) FillCircle(g, starBrush, (Point)position, size);

            else
            {
                if (report.Owner != clientState.EmpireState.Id) starBrushPopulation = Brushes.Red;
                else starBrushPopulation = Brushes.Green;
                FillCircle(g, starBrushPopulation, (Point)position, (int)Math.Sqrt(report.Colonists) / 25);
            }


            // If the Star name display is turned on then add the name

            if (this.displayStarNames && zoomFactor > 0.5)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(report.Name, this.nameFont, Brushes.White, position.X, position.Y + 5, format);
            }
        }


        public void DrawMineField(Graphics g, int circleUpperLeftX, int circleUpperLeftY, int Radius)
        {
            Bitmap SourceImage = new Bitmap(System.Drawing.Image.FromFile(".\\Graphics\\Mine_Layer\\MediumMineGoldSmall.jpg"));

            Rectangle CropRect = new Rectangle(SourceImage.Width/2 - Radius, SourceImage.Height/2 - Radius,2 * Radius, 2 * Radius);
            Bitmap CroppedImage = SourceImage.Clone(CropRect, SourceImage.PixelFormat);
            TextureBrush TB = new TextureBrush(CroppedImage);
            Bitmap FinalImage = new Bitmap( 2 * Radius,  2 * Radius);
            g.FillEllipse(TB, circleUpperLeftX, circleUpperLeftY, 2 * Radius, 2 * Radius);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="report"></param>
        private void DrawStarNormal(Graphics g, StarIntel report)
        {
            NovaPoint position = LogicalToDevice(report.Position);

            foreach (Minefield minefield in  clientState.EmpireState.VisibleMinefields.Values)
            {
                //DrawMineField(g, minefield.Position.X - minefield.Radius, minefield.Position.Y - minefield.Radius, minefield.Radius);
            }



            int size = 2;
            Brush starBrush = Brushes.White;

            // Bigger symbol for explored stars.

            if (report.Year > Global.Unset)
            {
                size = 4;
            }

            // Our stars are greenish, other's are red, unknown or uncolonised
            // stars are white.

            if (report.Owner == clientState.EmpireState.Id)
            {
                starBrush = Brushes.GreenYellow;
            }
            else
            {
                if (report.Owner != Global.Nobody)
                {
                    starBrush = Brushes.Red;
                }
            }

            FillCircle(g, starBrush, (Point)position, size);

            //if (report.HasFleetsInOrbit)
            //{
            //    size = size + size;
            //    Pen starPen = new Pen(Color.White,1);
            //    DrawCircle(g, starPen, (Point)position, size);
            //}

            // If the Star name display is turned on then add the name

            if (this.displayStarNames && zoomFactor > 0.5)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                g.DrawString(report.Name, this.nameFont, Brushes.White, position.X, position.Y + 5, format);
            }
        }

        private void DrawStarMineralConcentration(Graphics g, StarIntel report)
        {
            NovaPoint position = LogicalToDevice(report.Position);


            // If the Star name display is turned on then add the name

            Brush mineralBrush = Brushes.Blue;
            FillRectangle(g, mineralBrush, (Point)position, report.MineralConcentration.Ironium, -3);
            mineralBrush = Brushes.LimeGreen;
            FillRectangle(g, mineralBrush, (Point)position, report.MineralConcentration.Boranium, -2);
            mineralBrush = Brushes.Yellow;
            FillRectangle(g, mineralBrush, (Point)position, report.MineralConcentration.Germanium, -1);

        }

        private void DrawStarSurfaceMinerals(Graphics g, StarIntel report)
        {
            NovaPoint position = LogicalToDevice(report.Position);


            Brush mineralBrush = Brushes.Blue;
            if (report.ResourcesOnHand != null)
            {
                FillRectangle(g, mineralBrush, (Point)position, report.ResourcesOnHand.Ironium/20, -3);
                mineralBrush = Brushes.LimeGreen;
                FillRectangle(g, mineralBrush, (Point)position, report.ResourcesOnHand.Boranium/20, -2);
                mineralBrush = Brushes.Yellow;
                FillRectangle(g, mineralBrush, (Point)position, report.ResourcesOnHand.Germanium/20, -1);
            }
        }


        /// <Summary>
        /// Add an indication of a starbase (circle) or orbiting fleets (smaller
        /// circle) or both.
        /// </Summary>
        /// <param name="Star">The Star being drawn.</param>
        private void DrawOrbitingFleets(Graphics g, StarIntel report)
        {
            NovaPoint position = LogicalToDevice(report.Position);
            
            if (report == null)
            {
                return;
            }

            if (report.Starbase != null)
            {
                g.FillEllipse(
                    Brushes.Yellow,
                    position.X + 6,
                    position.Y - 6,
                    4,
                    4);
            }

            if (report.HasFleetsInOrbit)
            {
                int size = 12;
                g.DrawEllipse(
                    Pens.White,
                    position.X - (size / 2),
                    position.Y - (size / 2),
                    size,
                    size);
            }
        }

        /// <Summary>
        /// Convert logical coordinates to device coordintes.
        /// </Summary>
        /// <param name="p">The Point to convert.</param>
        /// <returns>A converted Point.</returns>
        private NovaPoint LogicalToDevice(NovaPoint p)
        {
            NovaPoint result = LogicalToExtent(p);

            result.X += displayOffset.X - scrollOffset.X;
            result.Y += displayOffset.Y - scrollOffset.Y;

            return result;
        }


        private NovaPoint LogicalToExtent(NovaPoint p)
        {
            NovaPoint result = new NovaPoint();

            result.X = (int)(p.X * zoomFactor) + extraSpace.X;
            result.Y = (int)(p.Y * zoomFactor) + extraSpace.Y;

            return result;
        }

        /// <Summary>
        /// Convert device coordinates to logical coordinates.
        /// </Summary>
        /// <param name="p">The Point to convert.</param>
        /// <returns>The converted Point.</returns>
        private NovaPoint DeviceToLogical(NovaPoint p)
        {
            NovaPoint result = new NovaPoint();

            result.X = (int)((p.X - displayOffset.X + scrollOffset.X - extraSpace.X) / zoomFactor);
            result.Y = (int)((p.Y - displayOffset.Y + scrollOffset.Y - extraSpace.Y) / zoomFactor);

            return result;
        }

        /// <Summary>
        /// Process a request to zoom in the Star map.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        public void ZoomInClick(object sender, System.EventArgs e)
        {
            Zoom(1.4);
        }


        /// <Summary>
        /// Process a request to zoom out the Star map.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        public void ZoomOutClick(object sender, System.EventArgs e)
        {
            Zoom(1 / 1.4);
        }

        /// <summary>
        /// Handle zooming via the mousewheel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StarMap_MouseWheel(object sender, MouseEventArgs e)
        {
            double zoomChange = 1 + (Math.Sign(e.Delta) * 0.15);  
            // This event fires on the StarMap control so we have to remove the mappanel offset to 
            // get the real mouse location
            NovaPoint preserveLocation = new NovaPoint(e.X - MapPanel.Left, e.Y - MapPanel.Top);
            Zoom(zoomChange, preserveLocation);
        }

        /// <Summary>
        /// Zoom in or out of the Star map.
        /// </Summary>
        private void Zoom()
        {
            Zoom(1.0, null);
        }

        private void Zoom(double delta)
        {
            Zoom(delta, null);
        }

        private void Zoom(double delta, NovaPoint preserveDisplayLocation)
        {
            if (System.Object.ReferenceEquals(preserveDisplayLocation, null))
            {
                preserveDisplayLocation = new NovaPoint(MapPanel.Width / 2, MapPanel.Height / 2);
            }
            NovaPoint preserveLogicalLocation = DeviceToLogical(preserveDisplayLocation);

            zoomFactor *= delta;
            this.zoomFactor = Math.Max(MinZoom, this.zoomFactor);
            this.zoomFactor = Math.Min(MaxZoom, this.zoomFactor);
            this.zoomOut.Enabled = zoomFactor > MinZoom;
            this.zoomIn.Enabled = zoomFactor < MaxZoom;

            this.extent.X = (int)(this.logical.X * this.zoomFactor) + (extraSpace.X * 2);
            this.extent.Y = (int)(this.logical.Y * this.zoomFactor) + (extraSpace.X * 2);

            // In the case where the Map Panel is bigger than what we want to display (i.e. extent)
            // then we add an offset to center the displayed map inside the panel
            // If extent is bigger then it's handled by the scroll offsets
            displayOffset.X = Math.Max((MapPanel.Width - extent.X) / 2, 0);
            displayOffset.Y = Math.Max((MapPanel.Height - extent.Y) / 2, 0);

            this.verticalScrollBar.Maximum = Math.Max(0, extent.Y - MapPanel.Height);
            this.horizontalScrollBar.Maximum = Math.Max(0, (extent.X - MapPanel.Width));

            // Try and scroll map back to location
            ScrollToDisplayLocation(preserveDisplayLocation, preserveLogicalLocation);

            this.RefreshStarMap(this, EventArgs.Empty);
        }

        internal void CenterMapOnPoint(NovaPoint pointToCentre)
        {   
            // We want to put the logical point given in the center of the map as much as possible
            NovaPoint centerDisplay = new NovaPoint(MapPanel.Width / 2, MapPanel.Height / 2);
            ScrollToDisplayLocation(centerDisplay, pointToCentre);
        }

        private void ScrollToDisplayLocation(NovaPoint oldDisplay, NovaPoint pointToCentre)
        {
            NovaPoint newCenterDisplay = LogicalToExtent(pointToCentre);
            Debug.WriteLine(String.Format("Center Disp {0}  NewCenterDisp {1}", oldDisplay, newCenterDisplay));

            scrollOffset.X = Math.Min(horizontalScrollBar.Maximum, Math.Max(0, newCenterDisplay.X - oldDisplay.X));
            scrollOffset.Y = Math.Min(verticalScrollBar.Maximum, Math.Max(0, newCenterDisplay.Y - oldDisplay.Y));

            horizontalScrollBar.Value = scrollOffset.X;
            verticalScrollBar.Value = scrollOffset.Y;
        }

        /// <Summary>
        /// Horizontally scroll the Star map.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MapScrollH(object sender, ScrollEventArgs e)
        {
            scrollOffset.X = e.NewValue;
            RefreshStarMap(this, EventArgs.Empty);
        }

        /// <Summary>
        /// Vertically scroll the Star map.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void MapScrollV(object sender, ScrollEventArgs e)
        {
            scrollOffset.Y = e.NewValue;
            RefreshStarMap(this, EventArgs.Empty);
        }

        /// <Summary>
        /// Process a mouse down event.
        /// </Summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StarMapMouse(object sender, MouseEventArgs e)
        {
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                if ((Control.ModifierKeys & Keys.Shift) != 0)
                {
                    if ((Control.ModifierKeys & Keys.Control) != 0)
                    {
                        LeftShiftMouse(e, false);
                    }
                    else
                    {
                        LeftShiftMouse(e, true);
                    }
                }
                else
                {
                    LeftMouse(e);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                RightMouse(e);
            }
        }

        /// <Summary>
        /// Left Shift Mouse: Set Waypoints.
        /// </Summary>
        /// <param name="e"></param>
        /// <param name="snapToObject"></param>
        private void LeftShiftMouse(MouseEventArgs e, bool snapToObject)
        {
            SelectionArgs args = new SelectionArgs(null);       
            OnSelectionRequested(args);
           
            Mappable item = args.Selection;

            if (item == null || !(item is Fleet))
            {
                return;
            }

            NovaPoint click = new NovaPoint(e.X, e.Y);
            Fleet fleet = item as Fleet;
            NovaPoint position = DeviceToLogical(click);
            List<Mappable> nearObjects = FindNearObjects(position);
            Waypoint waypoint = new Waypoint();

            waypoint.Position = position;
            waypoint.WarpFactor = fleet.SlowestEngine;
            waypoint.Task =  new NoTask();

            // If there are no items near the selected position then set the
            // waypoint to just be a position in space. Otherwise, make the target
            // of the waypoint the selected Item.
            //


            if (nearObjects.Count == 0 || snapToObject == false)
            {
                waypoint.Destination = "Space at " + position.ToString();
                waypoint.Position = position;
            }
            else
            {
                Mappable selected = nearObjects[0];
                waypoint.Position = selected.Position;
                waypoint.Destination = selected.Name;
            }
          
            // If the new waypoint is the same as the last one then do nothing.

            int lastIndex = fleet.Waypoints.Count - 1;
            if (fleet.Waypoints.Count > 0)
            {
                Waypoint lastWaypoint = fleet.Waypoints[lastIndex];

                if (waypoint.Destination == lastWaypoint.Destination)
                {
                    return;
                }
            }
            WaypointCommand command = new WaypointCommand(CommandMode.Add, waypoint, fleet.Key);
            
            clientState.Commands.Push(command);

            Nova.Common.Message message;
            if (command.IsValid(clientState.EmpireState, out message))  // TODO priority 5 is adding a new waypoint for a Starbase really valid? Will my starbase fly away when i do it?
            {
                command.ApplyToState(clientState.EmpireState);
            }
            else if (Global.Debug) Report.Information(message.Text);

            RefreshStarMap(this, EventArgs.Empty);

            if (WaypointChanged != null)
            {
                WaypointChanged(this, new EventArgs());
            }
        }


        /// <Summary>
        /// Left mouse button: select objects.
        /// </Summary>
        /// <param name="e"></param>
        private void LeftMouse(MouseEventArgs e)
        {
            NovaPoint position = new NovaPoint();
            NovaPoint click = new NovaPoint(e.X, e.Y);
            position = DeviceToLogical(click);

            List<Mappable> nearObjects = FindNearObjects(position);
            if (nearObjects.Count == 0)
            {
                return;
            }

            // If the mouse hasn't moved since the last selection cycle through
            // the list of near objects. If it has, start at the beginning of the
            // list.

            if ((Math.Abs(lastClick.X - click.X) > 10) ||
                (Math.Abs(lastClick.Y - click.Y) > 10))
            {
                selection = 0;
            }
            else
            {
                selection++;
                if (selection >= nearObjects.Count)
                {
                    selection = 0;
                }
            }

            lastClick = click;
            Mappable item = nearObjects[selection];       

            SetCursor(item.Position);
            
            OnSelectionChanged(new SelectionArgs(item));
        }

        private void RightMouse(MouseEventArgs e)
        {
            NovaPoint position = new NovaPoint();
            NovaPoint click = new NovaPoint(e.X, e.Y);
            position = DeviceToLogical(click);

            List<Mappable> nearObjects = FindNearObjects(position);
            if (nearObjects.Count == 0)
            {
                return;
            }

            selectItemMenu.Items.Clear();
            bool needSep = false;
            bool doneSep = false;
            foreach (Item sortableItem in nearObjects)
            {
                ToolStripItem menuItem = selectItemMenu.Items.Add(sortableItem.Name);
                menuItem.Click += ContextSelect;
                menuItem.Tag = sortableItem;
                if (sortableItem.Type == ItemType.StarIntel)
                {
                    menuItem.Image = Properties.Resources.planeticon;
                    needSep = true;
                }
                else if (sortableItem.Type == ItemType.Salvage)
                {
                    menuItem.Image = Properties.Resources.salvage0000;
                    needSep = true;
                }
                else if (sortableItem.Type == ItemType.FleetIntel)
                {
                    if (sortableItem.Owner == clientState.EmpireState.Id) menuItem.Image = Properties.Resources.fleet;
                    else menuItem.Image = Properties.Resources.fleet2;
                    if (needSep && !doneSep)
                    {
                        selectItemMenu.Items.Insert(selectItemMenu.Items.Count - 1, new ToolStripSeparator());
                        doneSep = true;
                    }
                }
            }
            
            selectItemMenu.Show(this, e.X, e.Y);
        }

        private void ContextSelect(object sender, EventArgs e)
        {
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem == null)
            {
                return;
            }

            Mappable item = menuItem.Tag as Mappable;
            if (item == null)
            {
                return;
            }
            
            SetCursor(item.Position);

            OnSelectionChanged(new SelectionArgs(item));
        }

        /// <Summary>
        /// Set the position of the Star map selection cursor.
        /// </Summary>
        /// <param name="position">Where the cursor is to be put.</param>
        public void SetCursor(NovaPoint position)
        {
            cursorPosition = position;
            CenterMapOnPoint(cursorPosition);
            RefreshStarMap(this, EventArgs.Empty);
        }

        /// <Summary>
        /// Provides a list of objects within a certain distance from a position,
        /// ordered by distance.
        /// </Summary>
        /// <param name="position">Starting Point for the search.</param>
        /// <returns>A list of Fleet and Star objects.</returns>
        private List<Mappable> FindNearObjects(NovaPoint position)
        {
            List<Mappable> nearObjects = new List<Mappable>();

            foreach (FleetIntel report in clientState.EmpireState.FleetReports.Values)
            {
                if ((report.Name == "S A L V A G E") || (!report.IsStarbase))
                {
                    if (PointUtilities.IsNear(report.Position, position))
                    {
                        nearObjects.Add(report);
                    }
                }
            }

            foreach (StarIntel report in clientState.EmpireState.StarReports.Values)
            {
                if (PointUtilities.IsNear(report.Position, position))
                {
                    nearObjects.Add(report);
                }
            }

            nearObjects.Sort(ItemSorter);
            return nearObjects;
        }

        private static int ItemSorter(Item x, Item y)
        {
            if (x.Type == y.Type)
            {
                return x.Name.CompareTo(y.Name);
            }

            switch (x.Type)
            {
                case ItemType.StarIntel:
                    return y.Type == ItemType.FleetIntel ? -1 : 0;
                case ItemType.FleetIntel:
                    return y.Type == ItemType.StarIntel ? 1 : 0;
                default:
                    return x.Name.CompareTo(y.Name);
            }
        }

        /// <Summary>
        /// Toggle the display of the Star names.
        /// </Summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleNames_CheckedChanged(object sender, EventArgs e)
        {
            displayStarNames = toggleNames.Checked;
            RefreshStarMap(this, EventArgs.Empty);
        }
        
        /// <Summary>
        /// Toggle the display of the background image.
        /// </Summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleBackground_CheckedChanged(object sender, EventArgs e)
        {
            displayBackground = toggleBackground.Checked;
            RefreshStarMap(this, EventArgs.Empty);
        }
        
        /// <Summary>
        /// Toggle the display of universe borders.
        /// </Summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleBorders_CheckedChanged(object sender, EventArgs e)
        {
            displayBorders = toggleBorders.Checked;
            RefreshStarMap(this, EventArgs.Empty);
        }
        
        /// <Summary>
        /// This handles external events in which another GUI element
        /// changes the selection. StarMap can react accordingly and
        /// update it's cursor withour exposing internal variables.
        /// </Summary>
        public void SetCursor(object sender, SelectionArgs e)
        {
            if (e == null || e.Selection == null)
            {
                return;
            }
            else
            {
                SetCursor(e.Selection.Position);
            }
        }
        
        public void RefreshStarMap(object sender, EventArgs e)
        {
            MapPanel.Invalidate();
        }

        private void MapPanel_ArrowKeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    verticalScrollBar.Value = Math.Max(verticalScrollBar.Minimum, verticalScrollBar.Value - verticalScrollBar.LargeChange);
                    break;
                case Keys.Down:
                    verticalScrollBar.Value = Math.Min(verticalScrollBar.Maximum, verticalScrollBar.Value + verticalScrollBar.LargeChange);
                    break;
                case Keys.Left:
                    horizontalScrollBar.Value = Math.Max(horizontalScrollBar.Minimum, horizontalScrollBar.Value - horizontalScrollBar.LargeChange);
                    break;
                case Keys.Right:
                    horizontalScrollBar.Value = Math.Min(horizontalScrollBar.Maximum, horizontalScrollBar.Value + horizontalScrollBar.LargeChange);
                    break;
            }
            scrollOffset.X = horizontalScrollBar.Value;
            scrollOffset.Y = verticalScrollBar.Value;
            RefreshStarMap(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionRequested(SelectionArgs e)
        {
            if (SelectionRequested != null) {
                SelectionRequested(this, e);
            }
        }

        protected virtual void OnSelectionChanged(SelectionArgs e)
        {
            if (SelectionChanged != null) {
                SelectionChanged(this, e);
            }
        }

        private void radioButtonNormal_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButtonNormal.Checked)
            {
                radioButtonGrowth.Checked = false;
                radioButtonMineralConcentration.Checked = false;
                radioButtonSurfaceMinerals.Checked = false;
                radioButtonNovaValue.Checked = false;
                radioButtonPopulation.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }
        }

        private void radioButtonGrowth_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonGrowth.Checked)
            {
                radioButtonNormal.Checked = false;
                radioButtonMineralConcentration.Checked = false;
                radioButtonSurfaceMinerals.Checked = false;
                radioButtonNovaValue.Checked = false;
                radioButtonPopulation.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }

        }

        private void radioButtonMineralConcentration_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMineralConcentration.Checked)
            {
                radioButtonGrowth.Checked = false;
                radioButtonNormal.Checked = false;
                radioButtonSurfaceMinerals.Checked = false;
                radioButtonNovaValue.Checked = false;
                radioButtonPopulation.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }

        }

        private void radioButtonSurfaceMinerals_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSurfaceMinerals.Checked)
            {
                radioButtonGrowth.Checked = false;
                radioButtonMineralConcentration.Checked = false;
                radioButtonNormal.Checked = false;
                radioButtonNovaValue.Checked = false;
                radioButtonPopulation.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }

        }

        private void radioButtonNovaValue_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonNovaValue.Checked)
            {
                radioButtonGrowth.Checked = false;
                radioButtonMineralConcentration.Checked = false;
                radioButtonNormal.Checked = false;
                radioButtonSurfaceMinerals.Checked = false;
                radioButtonPopulation.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }

        }
        private void radioButtonPopulation_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPopulation.Checked)
            {
                radioButtonGrowth.Checked = false;
                radioButtonMineralConcentration.Checked = false;
                radioButtonNormal.Checked = false;
                radioButtonSurfaceMinerals.Checked = false;
                radioButtonNovaValue.Checked = false;
                RefreshStarMap(this, EventArgs.Empty);

            }

        }
    }
}
