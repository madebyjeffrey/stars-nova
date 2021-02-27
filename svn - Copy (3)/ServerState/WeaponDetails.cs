#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009-2012 The Stars-Nova Project
//
// This file is part of Stars! Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

namespace Nova.Server
{
    using System;
    using System.Collections.Generic;
    using Nova.Common;
    using Nova.Common.Components;

    /// <summary>
    /// Class to identify weapon capability and their targets which is sortable by
    /// weapon system initiative.
    /// </summary>
    public class WeaponDetails : IComparable
    {
        public struct TargetPercent
        {
            public TargetPercent (Stack target, int percentToFire)
            {
                Target = target;
                PercentToFire = percentToFire;
            }
            public Stack Target { get; }
            public int PercentToFire { get; }
        }

        public TargetPercent TargetStack = new TargetPercent() ;

        public Stack SourceStack;


        public Weapon Weapon;

        public Double beamDispersal(Double distanceSquared)
        {
            return 100.0 - 10 * (distanceSquared / (Weapon.Range * Weapon.Range));  //90% at max range, 100% when at same location
        }
        public Double beamDispersalRon(Double distanceSquared,Double gridScaleSquared)
        { //in Ron Battle Engine real distance = 1/gridScale of a grid step
            return 100.0 - 10 * (distanceSquared / (Weapon.Range * Weapon.Range * gridScaleSquared));  //90% at max range, 100% when at same location
        }
        public Double missileAccuracy(ShipDesign source, ShipDesign target,Double missileBaseAccuracy)
        {
            Double increase = 1;
            Double decrease = 1;
            if (source.Summary.Properties.ContainsKey("Computer")) increase = 1.0 + ((source.Summary.Properties["Computer"] as Computer).Accuracy / 100.0);
            if (source.Summary.Properties.ContainsKey("Jammer"))  decrease = 1.0 - ((source.Summary.Properties["Jammer"] as ProbabilityProperty).Value / 100.0);
            return missileBaseAccuracy * increase * decrease;  //TODO check if computers on target affect accuracy of source
        }

        public int CompareTo(object rightHandSide)
        {
            WeaponDetails rhs = (WeaponDetails)rightHandSide;
            return this.Weapon.Initiative.CompareTo(rhs.Weapon.Initiative);
        }

    }
}

