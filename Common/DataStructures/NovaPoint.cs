#region Copyright Notice
// ============================================================================
// Copyright (C) 2010-2012 The Stars-Nova Project
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

namespace Nova.Common.DataStructures
{
    using System;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// A class to represent a point in space.
    /// Like System.Drawing.Point, with added methods for serialization.
    /// </summary>
    [Serializable]
    [SupportedOSPlatform("windows")]
    public class NovaPoint : ICloneable
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NovaPoint()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="x">The new X coordinate.</param>
        /// <param name="y">The new Y coordinate.</param>
        public NovaPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Initializing constructor from a System.Drawing.Point.
        /// </summary>
        /// <param name="p">The initial position as a <see cref="System.Drawing.Point"/>.</param>
        public NovaPoint(System.Drawing.Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// Initializing constructor from a NovaPoint.
        /// </summary>
        /// <param name="p">The initial position as a <see cref="NovaPoint"/>.</param>
        public NovaPoint(NovaPoint p)
        {
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// Create a copy of this NovaPoint.
        /// </summary>
        /// <returns>A copy of this NovaPoint.</returns>
        public object Clone()
        {
            return (object)new NovaPoint(X, Y);
        }

        /// <summary>
        /// Enable implicit casting from a <see cref="System.Drawing.Point"/>.
        /// </summary>
        /// <param name="p">A <see cref="System.Drawing.Point"/>.</param>
        /// <returns>A NovaPoint with the same x and y coordinates as the <see cref="System.Drawing.Point"/>.</returns>
        public static implicit operator NovaPoint(System.Drawing.Point p)
        {
            return new NovaPoint(p.X, p.Y);
        }

        /// <summary>
        /// Enable explicit casting of a NovaPoint to a System.Drawing.Point.
        /// </summary>
        /// <param name="p">A NovaPoint to cast.</param>
        /// <returns>A System.Drawing.Point with the same coordinates as p.</returns>
        public static explicit operator System.Drawing.Point(NovaPoint p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        /// <summary>
        /// Implement the Equals function.
        /// </summary>
        /// <param name="obj">An object to test for equality with.</param>
        /// <returns>Returns true if this.X == obj.X and this.Y == obj.Y and obj is a NovaPoint or Point.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is NovaPoint)
            {
                return this.X == ((NovaPoint)obj).X && this.Y == ((NovaPoint)obj).Y;
            }
            else if (obj is System.Drawing.Point)
            {
                return this.X == ((System.Drawing.Point)obj).X && this.Y == ((System.Drawing.Point)obj).Y;
            }
            else
            {
                throw new ArgumentException("Cannot compare NovaPoint objects with objects of type " + obj.GetType().ToString());
            }
        }

        /// <summary>
        /// Implement the == operator for NovaPoint.
        /// </summary>
        /// <param name="a">A NovaPoint to compare.</param>
        /// <param name="b">Another NovaPoint to compare.</param>
        /// <returns>Returns true if the points have the same location (X, Y).</returns>
        public static bool operator ==(NovaPoint a, NovaPoint b)
        {
            if (object.ReferenceEquals(a, b)) 
            { 
                return true; 
            }
            
            if (((object)a == null) || ((object)b == null)) 
            { 
                return false; 
            }
            
            return a.Equals(b);
        }

        /// <summary>
        /// Implement the != operator for NovaPoint.
        /// </summary>
        /// <param name="a">A NovaPoint to compare.</param>
        /// <param name="b">Another NovaPoint to compare.</param>
        /// <returns>Returns false if the points have the same location (X, Y).</returns>
        public static bool operator !=(NovaPoint a, NovaPoint b)
        {            
            return !(a == b);
        }

        /// <summary>
        /// Return a hash code with a good chance of separating points.
        /// </summary>
        /// <returns>10000X + Y.</returns>
        public override int GetHashCode()
        {
            return (X * 10000) + Y;
        }
        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public string ToString(Double gridSize)
        {
            return string.Format("({0}, {1})", X/gridSize, Y/gridSize);
        }

        /// <summary>
        /// This method adjusts the X and Y values of this Point to the sum of the X and Y values of this Point and p.
        /// </summary>
        /// <param name="p">An offset to be applied to this point.</param>
        public void Offset(NovaPoint p)
        {
            this.X += p.X;
            this.Y += p.Y;
        }

        /// <summary>
        /// This method adjusts the X and Y values of this Point to the sum of the X and Y values of this Point and p.
        /// </summary>
        /// <param name="x">X offset.</param>
        /// <param name="y">Y offset.</param>
        public void Offset(int x, int y)
        {
            this.X += x;
            this.Y += y;
        }

        // returns a unique string for each distinct NovaPoint
        public string ToHashString()
        {
            return X.ToString() + "#" + Y.ToString();
        }

        /// <summary>
        /// Load from XML: initializing constructor from an XML node.
        /// </summary>
        /// <param name="node">An <see cref="XmlNode"/> within a Nova xml document.</param>
        public NovaPoint(XmlNode node)
        {
            XmlNode mainNode = node.FirstChild;
            while (mainNode != null)
            {
                try
                {
                    switch (mainNode.Name.ToLower())
                    {
                        case "x":
                            {
                                X = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            }
                        case "y":
                            {
                                Y = int.Parse(mainNode.FirstChild.Value, System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    Report.Error(e.Message);
                }
                mainNode = mainNode.NextSibling;
            }
        }

        /// <summary>
        /// Save: Serialize this NovaPoint to an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="xmldoc">The parent <see cref="XmlDocument"/>.</param>
        /// <returns>An <see cref="XmlElement"/> representation of the NovaPoint.</returns>
        public XmlElement ToXml(XmlDocument xmldoc, string nodeName = "Point")
        {
            XmlElement xmlelPoint = xmldoc.CreateElement(nodeName);

            Global.SaveData(xmldoc, xmlelPoint, "X", X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Global.SaveData(xmldoc, xmlelPoint, "Y", Y.ToString(System.Globalization.CultureInfo.InvariantCulture));

            return xmlelPoint;
        }
        public double distanceTo(NovaPoint other)
        {
            return Math.Sqrt((Math.Abs(other.X - X) + Math.Abs(other.Y - Y)) * (Math.Abs(other.X - X) + Math.Abs(other.Y - Y)));
        }
        public double distanceToSquared(NovaPoint other)
        {
            return ((Math.Abs(other.X - X) + Math.Abs(other.Y - Y)) * (Math.Abs(other.X - X) + Math.Abs(other.Y - Y)));
        }
        public NovaPoint Scale(Double scalar)
        {
            NovaPoint scaledVector = new NovaPoint();
            scaledVector.X = (int)(X * scalar);
            scaledVector.Y = (int)(Y * scalar);
            return scaledVector;
        }
        /// <summary>
        /// returns a vector in the same direction as the original but with a length of param name="battleSpeed"
        /// </summary>
        /// <param name="battleSpeed"></param>
        /// <returns></returns>
        public NovaPoint BattleSpeedVector(Double battleSpeed)
        {
            if ((Y == 0) & (X == 0)) return this;
            NovaPoint scaledVector = new NovaPoint(this);
            double scalar =  battleSpeed / Math.Sqrt(X * X + Y * Y) ;
            scaledVector =  scaledVector.Scale(scalar);
            return scaledVector;
        }

        public Double lengthSquared()
        {
            return X * X + Y * Y;
        }

        /// <summary>
        /// angleBetween treats two NovaPoints as vectors and returns the angle between the vectors
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns></returns>
        public int angleBetween(NovaPoint op1, NovaPoint op2)
        {
            if ((op1.X == 0) && (op1.Y == 0)) return 0; //standing start in one direction
            if ((op2.X == 0) && (op2.Y == 0)) return 0; //stopped on a dime
            System.Windows.Vector v1 = new System.Windows.Vector(op1.X, op1.Y); //Sorry Mac and linux guys if this doesn't work on other platforms - this is a quick fix to try to get the Beta out
            System.Windows.Vector v2 = new System.Windows.Vector(op2.X, op2.Y); //TODO remove use of System.Windows.Vector.AngleBetween
            return (int) System.Windows.Vector.AngleBetween(v1, v2);
        }
        /// <summary>
        /// turnAsFastAsPossible - when a target flies by at close range the tagetting routines demand an immediate 180 degree change in velocity of twice the battlespeed
        /// this routine will provide a path that assumes high angular accelleration and a fixed longitudinal acceleration == BattleSpeed
        /// </summary>
        /// <param name="initialDirn"></param>
        /// <param name="reqdDirn"></param>
        /// <returns></returns>
        public NovaPoint turnAsFastAsPossible(NovaPoint initialDirn, NovaPoint reqdDirn)
        {
            // this Token will have started a decelleration burn before the actual flyby which gave it some lateral velocity
            // and reduced its forward momentum (NovaPoint.prepareForFlyby)
            // The turning manouver may choose to continue that turn or reverse its direction: 
            // 2nd step either reverse lateral velocity and accelerate by 87% BattleSpeed (in same vector as target)
            // or bring lateral velocity to zero and accellerate by 97% battlespeed in direction of target (in same vector as target)
            Random random = new Random();
                int leftOrRight = Math.Sign(random.Next(-100, 100));
            NovaPoint newSpeed;
            if (leftOrRight > 0)
            {
                newSpeed = initialDirn.Scale(-1);
                newSpeed += reqdDirn.Scale(0.87);
            }
            else newSpeed = reqdDirn.Scale(0.97);
            return newSpeed;
               
        }
        /// <summary>
        /// called just before fleets fly past their enemy 
        /// </summary>
        /// <param name="initialDirn"></param> initialDirn and reqdDirn will be almost identical on this move but 180 degrees apart next turn
        /// <param name="reqdDirn"></param>    unless the other fleet is also turning
        ///  the magnitude of the direction vectors will == BattleSpeed (as per Nov 2 2020)
        /// <returns></returns>
        public NovaPoint prepareForFlyby(NovaPoint initialDirn, NovaPoint reqdDirn)
        {//NovaPoints are used here to represent velocity vectors!
            Random random = new Random();
            // this Token will start a Manouvre that moves it laterally away from other tokens in preparation for all Tokens
            // rotating their main Propulsion engines (and usually the entire ship) by 180 degrees and engaging maximum thrust
            // to follow the target after it flies past them
            // 1st step decellerate by 97% BattleSpeed and impart 1/4 BattleSpeed as lateral velocity 
            NovaPoint newSpeed = initialDirn.Scale(0.03);
            int leftOrRight = Math.Sign(random.Next(-100, 100));
            newSpeed.Y += leftOrRight * initialDirn.Scale(0.25).X; //X and Y transposed to impart lateral velocity
            newSpeed.X += leftOrRight * initialDirn.Scale(-0.25).Y;
            return newSpeed;
        }

        public static NovaPoint operator +(NovaPoint op1, NovaPoint op2)
        {
            return new NovaPoint(op1.X + op2.X, op1.Y + op2.Y);
        }
        public static NovaPoint operator -(NovaPoint op1, NovaPoint op2)
        {
            return new NovaPoint(op1.X - op2.X, op1.Y - op2.Y);
        }
    }
}
