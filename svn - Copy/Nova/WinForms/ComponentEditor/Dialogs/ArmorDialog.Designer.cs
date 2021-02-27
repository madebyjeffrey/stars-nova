// This file needs -*- c++ -*- mode
// ============================================================================
// Nova. (c) 2008 Ken Reed
//
// The GUI components of the armour creation dialog.
//
// This is free software. You can redistribute it and/or modify it under the
// terms of the GNU General Public License version 2 as published by the Free
// Software Foundation.
// ============================================================================

namespace ComponentEditor
{
    partial class ArmorDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArmorDialog));
            this.DoneButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Shielding = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.ArmourStrength = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.CommonProperties = new ComponentEditor.CommonProperties();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Shielding)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArmourStrength)).BeginInit();
            this.SuspendLayout();
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(499, 426);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 1;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(395, 426);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Shielding);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.ArmourStrength);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(399, 192);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(180, 150);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Armour Properties";
            // 
            // Shielding
            // 
            this.Shielding.Location = new System.Drawing.Point(119, 45);
            this.Shielding.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.Shielding.Name = "Shielding";
            this.Shielding.Size = new System.Drawing.Size(55, 20);
            this.Shielding.TabIndex = 2;
            this.Shielding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Shielding";
            // 
            // ArmourStrength
            // 
            this.ArmourStrength.Location = new System.Drawing.Point(119, 18);
            this.ArmourStrength.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.ArmourStrength.Name = "ArmourStrength";
            this.ArmourStrength.Size = new System.Drawing.Size(55, 20);
            this.ArmourStrength.TabIndex = 1;
            this.ArmourStrength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Strength";
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(395, 391);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 4;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // CommonProperties
            // 
            this.CommonProperties.Location = new System.Drawing.Point(3, 4);
            this.CommonProperties.Name = "CommonProperties";
            this.CommonProperties.Size = new System.Drawing.Size(581, 458);
            this.CommonProperties.TabIndex = 0;
            this.CommonProperties.Value = ((NovaCommon.Component)(resources.GetObject("CommonProperties.Value")));
            // 
            // ArmourDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 469);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.CommonProperties);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ArmourDialog";
            this.Text = "Nova Armour Editor";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Shielding)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArmourStrength)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CommonProperties CommonProperties;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown ArmourStrength;
        private System.Windows.Forms.Label label1;
       private System.Windows.Forms.Button DeleteButton;
       private System.Windows.Forms.NumericUpDown Shielding;
       private System.Windows.Forms.Label label2;
    }
}
