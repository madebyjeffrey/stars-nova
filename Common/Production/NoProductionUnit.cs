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
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// Class representing an empty production unit. Useful for
    /// the production list header.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class NoProductionUnit : IProductionUnit
    {
        public Resources Cost
        {
            private set;
            get;
        }
                
        public Resources RemainingCost
        {
            private set;
            get;
        }
        
        public string Name
        {
            get { return "None";}
        }
                
        public NoProductionUnit()
        {
            Cost = new Resources();
            RemainingCost = Cost;
        }

        public bool IsSkipped(Star star, Race race = null, int gravityModCapability = 0, int radiationModCapability = 0, int temperatureModCapability = 0)
        {
            return true;
        }

        public bool Construct(Star star,out Message message, Race race = null, int gravityModCapability = 0, int radiationModCapability = 0, int temperatureModCapability = 0)
        {
            message = null;
            return false;
        }
        
        public XmlElement ToXml(XmlDocument xmldoc)
        {
            return null;
        }
    }
}
