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
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    
    using Nova.Common;
    using Nova.Common.DataStructures;

    /// <Summary>
    /// Dialog for viewing battle progress and outcome.
    /// </Summary>
    public partial class BattleViewer : Form
    {
        private readonly BattleReport theBattle;
        private readonly Dictionary<long, Stack> myStacks = new Dictionary<long, Stack>();
        private int eventCount;
        private int initialSize = 0;
        /// <Summary>
        /// Initializes a new instance of the BattleViewer class.
        /// </Summary>
        /// <param name="thisBattle">The <see cref="BattleReport"/> to be displayed.</param>
        public BattleViewer(BattleReport report)
        {
            InitializeComponent();
            theBattle = report;
            eventCount = 0;

            // Take a copy of all of the stacks so that we can mess with them
            // without disturbing the master copy in the global turn file.

            foreach (Stack stack in theBattle.Stacks.Values)
            {
                myStacks[stack.Key] = new Stack(stack);
            }
        }

        /// <Summary>
        /// Initialisation performed on a load of the whole dialog.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void OnLoad(object sender, EventArgs e)
        {
            battleLocation.Text = theBattle.Location;

            battlePanel.BackgroundImage = Nova.Properties.Resources.Plasma;
            battlePanel.BackgroundImageLayout = ImageLayout.Stretch;
            SetStepNumber(theBattle.Steps[eventCount]);
            ZoomLevel.SelectedIndex = 1;
        }

        /// <Summary>
        /// Draw the battle panel by placing the images for the stacks in the
        /// appropriate position.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void OnPaint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e); // added

            Graphics graphics = e.Graphics;
            int MaxX = int.MinValue;
            int MaxY = int.MinValue;
            int MinX = int.MaxValue;
            int MinY = int.MaxValue;
            int MaxArmedX = int.MinValue;
            int MaxArmedY = int.MinValue;
            int MinArmedX = int.MaxValue;
            int MinArmedY = int.MaxValue;
            Stack selectedStack = null;
            foreach (Stack stack in myStacks.Values)
            {
                if (selectedStack == null) selectedStack = stack;  // TODO priority 0 add a way of selecting which stack the UI zooms in on when "Follow selected stack" is selected.
                if (stack.Position.X > MaxX) MaxX = stack.Position.X;
                if (stack.Position.Y > MaxY) MaxY = stack.Position.Y;
                if (stack.Position.X < MinX) MinX = stack.Position.X;
                if (stack.Position.Y < MinY) MinY = stack.Position.Y;
                if (stack.IsArmed)
                {
                    if (stack.Position.X > MaxArmedX) MaxArmedX = stack.Position.X;
                    if (stack.Position.Y > MaxArmedY) MaxArmedY = stack.Position.Y;
                    if (stack.Position.X < MinArmedX) MinArmedX = stack.Position.X;
                    if (stack.Position.Y < MinArmedY) MinArmedY = stack.Position.Y;
                }
            }

            Size panelSize = battlePanel.Size;
            if (initialSize == 0) initialSize  =  MaxX;


            //if (ZoomLevel.SelectedIndex == 3) graphics.PageScale = (float)((Double)panelSize.Height / (Double)initialSize);
            //if (ZoomLevel.SelectedIndex == 0) graphics.PageScale = (float)((Double)panelSize.Height / (Double)MaxX - MinX);
            //if (ZoomLevel.SelectedIndex == 1) graphics.PageScale = (float)((Double)panelSize.Height / (Double)MaxArmedX - MinArmedX);
            //if (ZoomLevel.SelectedIndex == 2) graphics.PageScale = (float)((Double)panelSize.Height / (Double)2 * Global.MaxWeaponRange);
            if (ZoomLevel.SelectedIndex == 3) graphics.PageScale = (float)((Double)panelSize.Height / Math.Max(8*Global.MaxWeaponRange, initialSize));
            if (ZoomLevel.SelectedIndex == 0)
            {
                graphics.TranslateTransform(-MinX,-MinY); 
                graphics.ScaleTransform ((float)((Double)panelSize.Height / Math.Max(8*Global.MaxWeaponRange, (MaxX - MinX))), (float)((Double)panelSize.Height / Math.Max(8*Global.MaxWeaponRange, (MaxX - MinX))), MatrixOrder.Append);// maintain Aspect Ratio
            }
            if (ZoomLevel.SelectedIndex == 1)
            {
                graphics.TranslateTransform(-MinArmedX,-MinArmedY); 
                graphics.ScaleTransform((float)((Double)panelSize.Height / Math.Max(8*Global.MaxWeaponRange,(MaxArmedX - MinArmedX))),(float)((Double)panelSize.Height / Math.Max(8*Global.MaxWeaponRange, (MaxArmedX - MinArmedX))), MatrixOrder.Append);// maintain Aspect Ratio
            }
            if (ZoomLevel.SelectedIndex == 2)
            {
                graphics.TranslateTransform(-selectedStack.Position.X, -selectedStack.Position.Y);
                graphics.ScaleTransform  ((float)((Double)panelSize.Height / ((Double)16 * Global.MaxWeaponRange)), (float)((Double)panelSize.Height / ((Double)16 * Global.MaxWeaponRange)), MatrixOrder.Append);
                graphics.TranslateTransform((float)((Double)panelSize.Height / ((Double)32 * Global.MaxWeaponRange)), (float)((Double)panelSize.Height / ((Double)32 * Global.MaxWeaponRange)));

                // graphics.TranslateTransform(Global.MaxWeaponRange, Global.MaxWeaponRange,System.Drawing.Drawing2D.MatrixOrder.Append);
            }
            graphics.ScaleTransform(0.85F, 0.85F, MatrixOrder.Append); //put about 5% free space around the selected squares
            graphics.TranslateTransform(0.05F, 0.05F, MatrixOrder.Append);

            foreach (Stack stack in myStacks.Values)
            {
                if (stack.Token.Armor > 0)
                {
                    double scale = graphics.PageScale;
                    // Create parallelogram for drawing image.
                    PointF ulCorner = new PointF(stack.Position.X, stack.Position.Y);
                    PointF urCorner = new PointF(stack.Position.X + stack.Icon.Image.Width / 4, stack.Position.Y);
                    PointF llCorner = new PointF(stack.Position.X, stack.Position.Y + stack.Icon.Image.Height / 4);
                    PointF[] destPara = { ulCorner, urCorner, llCorner };
                    graphics.DrawImage(stack.Icon.Image, destPara);
                }
            }
        }

        /// <Summary>
        /// Step through each battle event.
        /// </Summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private void NextStep_Click(object sender, EventArgs e)
        {
            object thisStep = theBattle.Steps[eventCount];
            SetStepNumber((BattleStep) thisStep);

            if (thisStep is BattleStepMovement)
            {
                DoBattleStepMovement(thisStep as BattleStepMovement);
            }
            else if (thisStep is BattleStepTarget)
            {
                DoBattleStepTarget(thisStep as BattleStepTarget);
            }
            else if (thisStep is BattleStepWeapons)
            {
                DoBattleStepFireWeapon(thisStep as BattleStepWeapons);
            }
            else if (thisStep is BattleStepDestroy)
            {
                UpdateDestroy(thisStep as BattleStepDestroy);
            }

            if (eventCount < theBattle.Steps.Count - 1)
            {
                eventCount++;
            }
            else
            {
                nextStep.Enabled = false;
            }

        }


        /// <Summary>
        /// Update the movement of a stack.
        /// </Summary>
        /// <param name="battleStep">Movement to display.</param>
        private void DoBattleStepMovement(BattleStepMovement battleStep)
        {
            Stack stack = null;
            myStacks.TryGetValue(battleStep.StackKey, out stack);

            if (stack != null)
            {
                UpdateStackDetails(stack);
                stack.Position = battleStep.Position; // move the icon
            }
            else
            {
                ClearStackDetails();
            }

            if (stack != null) movedFrom.Text = stack.Position.ToString();
            else movedFrom.Text = "";
            movedTo.Text = battleStep.Position.ToString();
            stack.Position = battleStep.Position;

            // We have moved, clear out the other fields as they are not relevant to this step.
            ClearTargetDetails();
            ClearWeapons();

            battlePanel.Invalidate();
        }

        /// <Summary>
        /// Update the current target (and stack) details.
        /// </Summary>
        /// <param name="target">Target ship to display.</param>
        private void DoBattleStepTarget(BattleStepTarget battleStep)
        {
            if (battleStep == null)
            {
                Report.Error("BattleViewer.cs DoBattleStepTarget(): battleStep is null.");
                ClearTargetDetails();
            }
            else
            {
                Stack lamb = null;
                Stack wolf = null;

                theBattle.Stacks.TryGetValue(battleStep.TargetKey, out lamb);
                theBattle.Stacks.TryGetValue(battleStep.StackKey, out wolf);

                UpdateStackDetails(wolf);
                ClearMovementDetails();
                ClearWeapons();
                UpdateTargetDetails(lamb);
            }
        }

        /// <Summary>
        /// Deal with weapons being fired.
        /// </Summary>
        /// <param name="weapons">Weapon to display.</param>
        private void DoBattleStepFireWeapon(BattleStepWeapons weapons)
        {
            if (weapons == null)
            {
                Report.Error("BattleViewer.cs DoBattleStepFireWeapon() weapons is null.");
                ClearWeapons();
            }
            else
            {
                BattleStepTarget target = weapons.WeaponTarget;

                Stack lamb = null;
                Stack wolf = null;

                theBattle.Stacks.TryGetValue(target.TargetKey, out lamb);
                theBattle.Stacks.TryGetValue(target.StackKey, out wolf);

                UpdateStackDetails(wolf);
                UpdateTargetDetails(lamb);
                ClearMovementDetails();

                // damge taken
                weaponPower.Text = weapons.Damage.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // "Damage to shields" or "Damage to armor"
                if (weapons.Targeting == BattleStepWeapons.TokenDefence.Shields)
                {
                    componentTarget.Text = "Damage to shields";
                    damage.Text = weaponPower.Text + " " + componentTarget.Text;
                    lamb.Token.Shields -= weapons.Damage;
                    UpdateTargetDetails(lamb);
                }
                else
                {
                    componentTarget.Text = "Damage to armor";
                    damage.Text = weaponPower.Text + " " + componentTarget.Text;
                    lamb.Token.Armor -= weapons.Damage;
                    UpdateTargetDetails(lamb);
                }


            }
        }

        /// <summary>
        /// Clear the details of the stack in the BattleViewer->BattleDetails->Stack
        /// </summary>
        private void ClearStackDetails()
        {
            stackKey.Text = "";
            stackOwner.Text = "";
            stackDesign.Text = "";
            stackShields.Text = "";
            stackArmor.Text = "";

        }

        /// <summary>
        /// Write out the Battle viewer stack details
        /// </summary>
        /// <param name="wolf"></param>
        private void UpdateStackDetails(Stack wolf)
        {
            if (wolf != null)
            {
                stackOwner.Text = wolf.Owner.ToString("X");
                stackKey.Text = wolf.Key.ToString("X");
                stackQuantity.Text = wolf.Token.Quantity.ToString();
                stackDesign.Text = wolf.Token.Design.Name;
                stackShields.Text = wolf.TotalShieldStrength.ToString();
                stackArmor.Text = wolf.TotalArmorStrength.ToString();
            }
            else
            {
                ClearStackDetails();
            }
        }

        /// <summary>
        /// Write out the target details
        /// </summary>
        /// <param name="lamb"></param>
        private void UpdateTargetDetails(Stack lamb)
        {
            if (lamb != null)
            {
                targetOwner.Text = lamb.Owner.ToString("X");
                targetKey.Text = lamb.Key.ToString("X");
                targetQuantity.Text = lamb.Token.Quantity.ToString();
                targetDesign.Text = lamb.Token.Design.Name;

                targetShields.Text = lamb.TotalShieldStrength.ToString(System.Globalization.CultureInfo.InvariantCulture);
                targetArmor.Text = lamb.TotalArmorStrength.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                ClearTargetDetails();
            }
        }
        
        /// <Summary>
        /// Set the details for the target to "" on the UI.
        /// </Summary>
        private void ClearTargetDetails()
        {
            targetDesign.Text = "";
            targetOwner.Text = "";
            targetShields.Text = "";
            targetArmor.Text = "";
        }

        
        /// <summary>
        /// Clear the BattleViewer weapon details.
        /// </summary>
        private void ClearWeapons()
        {
            weaponPower.Text = "";
            componentTarget.Text = "";
            damage.Text = "";
        }

        /// <summary>
        /// Clear the BattleViewer movement details.
        /// </summary>
        private void ClearMovementDetails()
        {
            movedFrom.Text = "";
            movedTo.Text = "";
               
        }

        /// <Summary>
        /// Deal with a ship being destroyed. Remove it from the containing stack and,
        /// if the token count drops to zero, destroy the whole stack.
        /// </Summary>
        /// <param name="destroy"></param>
        private void UpdateDestroy(BattleStepDestroy destroy)
        {
            damage.Text = "Ship destroyed";

            // Stacks have 1 token, so remove the stack at once.
            // Not sure they do - see ShipToken.Quantity - Dan 17 Apr 17

            myStacks.Remove(destroy.StackKey);
            battlePanel.Invalidate();
        }
        

        /// <Summary>
        /// Just display the currrent step number in the battle replay control panel.
        /// </Summary>
        private void SetStepNumber(BattleStep thisStep)
        {
            StringBuilder title = new StringBuilder();

            title.AppendFormat(
                "Step {0} of {1}: {2}",
                eventCount + 1,
                theBattle.Steps.Count,
                thisStep.Type
                );


            stepNumber.Text = title.ToString();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
