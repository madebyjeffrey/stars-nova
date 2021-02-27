#region Copyright Notice
// ============================================================================
// Copyright (C) 2012 The Stars-Nova Project
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

namespace Nova.Common
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using System.Xml;
    
    using Nova.Common;
    using Nova.Common.DataStructures;

    /// <summary>
    /// A special Fleet used in the battle engine and battle viewer. It contains
    /// only one ShipToken of Quantity ships of a single design, and holds the key
    /// of the Fleet that spawned it.
    /// </summary>
    public class Stack : Fleet
    {
        /// <summary>
        /// Gets or sets the Stack to target in battle.
        /// </summary>
        public Stack Target
        {
            get;
            set;
        }
        public System.Collections.Generic.List<Stack> TargetList
        {
            get;
            set;
        }

        /// <summary>
        /// The Key of the Fleet which originated this Stack.
        /// </summary>
        public long ParentKey
        {
            get;
            private set;            
        }

        public Bitmap StackIcon = null;

        /// <summary>
        /// Returns this Stack's battle speed.
        /// </summary>
        public double BattleSpeed
        {
            get
            {
                return Token.Design.BattleSpeed;
            }
        }

        public NovaPoint VelocityVector
        {
            get;
            set;    
        }
        /// <summary>
        /// Return the current Defense capability of a stack.
        /// </summary>
        public double Defenses
        {
            get
            {
                 return Token.Armor + Token.Shields;
            }
        }

        /// <summary>
        /// Quick check if a Stack has been destroyed.
        /// </summary>
        public bool IsDestroyed
        {
            get
            {
                return Token == null || Token.Quantity <= 0 || Token.Armor <= 0;
            }
        }

        /// <summary>
        /// Sets or Gets the single ShipToken this Stack is allowed to have. This is a reference so the Fleet token is modified when this token is modified.
        /// </summary>
        public ShipToken Token
        {
            get
            {
                if (Composition.Count == 0)
                {
                    return null;
                }

                return Composition.First().Value;
            }
            
            private set
            {
                Composition.Clear();
                Composition.Add(value.Key, value);
            }
        }
        
        /// <summary>
        /// Generates a Stack from a fleet and a specified ShipToken.
        /// </summary>
        /// <param name="fleet">Parent Fleet.</param>
        /// <param name="stackId">Unique Battle Engine ID.</param>
        /// <param name="token">Ship Token for this Stack.</param>
        public Stack(Fleet fleet, uint stackId, ShipToken token)
            : base(fleet)
        {
            Id = stackId;
            ParentKey = fleet.Key;
            Name = "Stack #" + stackId.ToString("X");
            BattlePlan = fleet.BattlePlan;
            InOrbit = fleet.InOrbit;
            Token = token;  // note this is a reference to the actual token in the fleet


            Bitmap transparent = new Bitmap(fleet.Icon.Image);
            Color background = transparent.GetPixel(0, 0);
            transparent = Posturize(transparent); // TODO on large maps doing this multiple times adds extra overhead
            transparent.MakeTransparent(background);

            int stackDiameter = (int)Math.Ceiling(Math.Sqrt(token.Quantity));
            StackIcon = new Bitmap(transparent, fleet.Icon.Image.Width * stackDiameter, fleet.Icon.Image.Height * stackDiameter);
            int shipNumber = 1;
            int row = 0;
            int column = 0;
            while (shipNumber < token.Quantity)
            {
                Paste(StackIcon, transparent, row, column);
                shipNumber++;
                row = shipNumber % stackDiameter;
                column = shipNumber / stackDiameter;
            }
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
        Bitmap Clear(Bitmap input)
        {
            Color background = input.GetPixel(0, 0);
            for (int row = 0; row < input.Height; row++)
            {
                for (int column = 0; column < input.Width; column++) input.SetPixel(row, column, background);
            }
            return input;
        }

        Bitmap Paste(Bitmap dest, Bitmap source, int tokenRow, int tokenColumn)
        {
            int left = source.Width * (tokenColumn );   
            int top = source.Height * (tokenRow );
            Bitmap result = new Bitmap(dest);
            Color background = dest.GetPixel(0, 0);
            int row = 0;
            int column = 0;
            for (row = 0; row < source.Height; row++)
            {
                for (column = 0; column < source.Width; column++)
                {
                    if (source.GetPixel(column, row) != background) dest.SetPixel(column+left, row+top, source.GetPixel(column, row));
                }
            }

            return result;
        }
        /// <summary>
        /// Copy constructor. This is only used by the battle engine so only the fields
        /// used by it in creating stacks need to be copied. Note that we copy the
        /// token as well. Be careful when using the copy; It is a different object.
        /// </summary>
        /// <param name="copy">The fleet to copy.</param>
        /// <remarks>
        /// Why are we copying Stacks? 
        /// For the battle viewer, copies are required so the originals are not destroyed in the battle report. 
        /// This allows the battle to be replayed multiple times.
        /// In the battle engine, copies are required to create the battle report, so all stacks present at the start of the battle are represented.
        /// </remarks>
        public Stack(Stack copy)
            : base(copy)
        {
            ParentKey = copy.ParentKey;
            Key = copy.Key;
            BattlePlan = copy.BattlePlan;            
            Target = copy.Target;
            TargetList = copy.TargetList;
            InOrbit = copy.InOrbit;
            Token = new ShipToken(copy.Token.Design, copy.Token.Quantity, copy.Token.Armor);
            Token.Shields = copy.Token.Shields;
            //StackIcon is too large to save in the XML so recreate it here - it contains no unique information
            Bitmap transparent = new Bitmap(copy.Icon.Image);
            Color background = transparent.GetPixel(0, 0);
            transparent = Posturize(transparent); // TODO on large maps doing this multiple times adds extra overhead
            transparent.MakeTransparent(background);
            if (copy.Token.Design.IsStarbase)
            {
                StackIcon = new Bitmap(transparent, copy.Icon.Image.Width * 4, copy.Icon.Image.Height * 4);
            }
            else
            {
                int stackDiameter = (int)Math.Ceiling(Math.Sqrt(copy.Token.Quantity));
                StackIcon = new Bitmap(transparent, copy.Icon.Image.Width * stackDiameter, copy.Icon.Image.Height * stackDiameter);
                StackIcon = Clear(StackIcon);
                int shipNumber = 0;
                int row = 0;
                int column = 0;
                while (shipNumber < copy.Token.Quantity)
                {
                    Paste(StackIcon, transparent, row, column);
                    shipNumber++;
                    row = shipNumber % stackDiameter;
                    column = shipNumber / stackDiameter;
                }
            }
        }
        
        /// <summary>
        /// Load: initializing constructor to load a Stack from an XmlNode (save file).
        /// </summary>
        /// <param name="node">An XmlNode representing the Stack.</param>
        public Stack(XmlNode node)
            : base(node)
        {
        }
    }
}
