// ============================================================================
// Nova. (c) 2008 Ken Reed
// 
// This is free software. You can redistribute it and/or modify it under the
// terms of the GNU General Public License version 2 as published by the Free
// Software Foundation.
// ============================================================================

namespace Nova.WinForms.Gui
{
    public partial class ResearchDialog
    {
        /// <Summary>
        /// Required designer variable.
        /// </Summary>
        private System.ComponentModel.IContainer components = null;

        /// <Summary>
        /// Clean up any resources being used.
        /// </Summary>
        /// <param name="disposing">Set to true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <Summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </Summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResearchDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.biotechLevel = new System.Windows.Forms.Label();
            this.electronicsLevel = new System.Windows.Forms.Label();
            this.constructionLevel = new System.Windows.Forms.Label();
            this.propulsionLevel = new System.Windows.Forms.Label();
            this.weaponsLevel = new System.Windows.Forms.Label();
            this.energyLevel = new System.Windows.Forms.Label();
            this.biotechButton = new System.Windows.Forms.RadioButton();
            this.electronicsButton = new System.Windows.Forms.RadioButton();
            this.constructionButton = new System.Windows.Forms.RadioButton();
            this.propulsionButton = new System.Windows.Forms.RadioButton();
            this.weaponsButton = new System.Windows.Forms.RadioButton();
            this.energyButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numericResources = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.budgetPercentage = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.availableResources = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.completionTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.completionResources = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.researchBenefits = new System.Windows.Forms.DataGridView();
            this.Discovery = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RequiredTech = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.budgetPercentage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.researchBenefits)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.biotechLevel);
            this.groupBox1.Controls.Add(this.electronicsLevel);
            this.groupBox1.Controls.Add(this.constructionLevel);
            this.groupBox1.Controls.Add(this.propulsionLevel);
            this.groupBox1.Controls.Add(this.weaponsLevel);
            this.groupBox1.Controls.Add(this.energyLevel);
            this.groupBox1.Controls.Add(this.biotechButton);
            this.groupBox1.Controls.Add(this.electronicsButton);
            this.groupBox1.Controls.Add(this.constructionButton);
            this.groupBox1.Controls.Add(this.propulsionButton);
            this.groupBox1.Controls.Add(this.weaponsButton);
            this.groupBox1.Controls.Add(this.energyButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(190, 245);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Technology Status";
            // 
            // biotechLevel
            // 
            this.biotechLevel.AutoSize = true;
            this.biotechLevel.Location = new System.Drawing.Point(133, 211);
            this.biotechLevel.Name = "biotechLevel";
            this.biotechLevel.Size = new System.Drawing.Size(13, 13);
            this.biotechLevel.TabIndex = 13;
            this.biotechLevel.Text = "0";
            // 
            // electronicsLevel
            // 
            this.electronicsLevel.AutoSize = true;
            this.electronicsLevel.Location = new System.Drawing.Point(133, 180);
            this.electronicsLevel.Name = "electronicsLevel";
            this.electronicsLevel.Size = new System.Drawing.Size(13, 13);
            this.electronicsLevel.TabIndex = 12;
            this.electronicsLevel.Text = "0";
            // 
            // constructionLevel
            // 
            this.constructionLevel.AutoSize = true;
            this.constructionLevel.Location = new System.Drawing.Point(133, 149);
            this.constructionLevel.Name = "constructionLevel";
            this.constructionLevel.Size = new System.Drawing.Size(13, 13);
            this.constructionLevel.TabIndex = 11;
            this.constructionLevel.Text = "0";
            // 
            // propulsionLevel
            // 
            this.propulsionLevel.AutoSize = true;
            this.propulsionLevel.Location = new System.Drawing.Point(133, 118);
            this.propulsionLevel.Name = "propulsionLevel";
            this.propulsionLevel.Size = new System.Drawing.Size(13, 13);
            this.propulsionLevel.TabIndex = 10;
            this.propulsionLevel.Text = "0";
            // 
            // weaponsLevel
            // 
            this.weaponsLevel.AutoSize = true;
            this.weaponsLevel.Location = new System.Drawing.Point(133, 89);
            this.weaponsLevel.Name = "weaponsLevel";
            this.weaponsLevel.Size = new System.Drawing.Size(13, 13);
            this.weaponsLevel.TabIndex = 9;
            this.weaponsLevel.Text = "0";
            // 
            // energyLevel
            // 
            this.energyLevel.AutoSize = true;
            this.energyLevel.Location = new System.Drawing.Point(133, 56);
            this.energyLevel.Name = "energyLevel";
            this.energyLevel.Size = new System.Drawing.Size(13, 13);
            this.energyLevel.TabIndex = 8;
            this.energyLevel.Text = "0";
            // 
            // biotechButton
            // 
            this.biotechButton.AutoSize = true;
            this.biotechButton.Location = new System.Drawing.Point(6, 207);
            this.biotechButton.Name = "biotechButton";
            this.biotechButton.Size = new System.Drawing.Size(92, 17);
            this.biotechButton.TabIndex = 7;
            this.biotechButton.Tag = "6";
            this.biotechButton.Text = "Biotechnology";
            this.biotechButton.UseVisualStyleBackColor = true;
            this.biotechButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // electronicsButton
            // 
            this.electronicsButton.AutoSize = true;
            this.electronicsButton.Location = new System.Drawing.Point(6, 176);
            this.electronicsButton.Name = "electronicsButton";
            this.electronicsButton.Size = new System.Drawing.Size(77, 17);
            this.electronicsButton.TabIndex = 6;
            this.electronicsButton.Tag = "5";
            this.electronicsButton.Text = "Electronics";
            this.electronicsButton.UseVisualStyleBackColor = true;
            this.electronicsButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // constructionButton
            // 
            this.constructionButton.AutoSize = true;
            this.constructionButton.Location = new System.Drawing.Point(6, 145);
            this.constructionButton.Name = "constructionButton";
            this.constructionButton.Size = new System.Drawing.Size(84, 17);
            this.constructionButton.TabIndex = 5;
            this.constructionButton.Tag = "4";
            this.constructionButton.Text = "Construction";
            this.constructionButton.UseVisualStyleBackColor = true;
            this.constructionButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // propulsionButton
            // 
            this.propulsionButton.AutoSize = true;
            this.propulsionButton.Location = new System.Drawing.Point(6, 114);
            this.propulsionButton.Name = "propulsionButton";
            this.propulsionButton.Size = new System.Drawing.Size(74, 17);
            this.propulsionButton.TabIndex = 4;
            this.propulsionButton.Tag = "3";
            this.propulsionButton.Text = "Propulsion";
            this.propulsionButton.UseVisualStyleBackColor = true;
            this.propulsionButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // weaponsButton
            // 
            this.weaponsButton.AutoSize = true;
            this.weaponsButton.Location = new System.Drawing.Point(6, 85);
            this.weaponsButton.Name = "weaponsButton";
            this.weaponsButton.Size = new System.Drawing.Size(71, 17);
            this.weaponsButton.TabIndex = 3;
            this.weaponsButton.Tag = "2";
            this.weaponsButton.Text = "Weapons";
            this.weaponsButton.UseVisualStyleBackColor = true;
            this.weaponsButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // energyButton
            // 
            this.energyButton.AutoSize = true;
            this.energyButton.Location = new System.Drawing.Point(6, 52);
            this.energyButton.Name = "energyButton";
            this.energyButton.Size = new System.Drawing.Size(58, 17);
            this.energyButton.TabIndex = 2;
            this.energyButton.Tag = "1";
            this.energyButton.Text = "Energy";
            this.energyButton.UseVisualStyleBackColor = true;
            this.energyButton.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(102, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Current Level";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(6, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Field of Study";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(556, 318);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OKClicked);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.researchBenefits);
            this.groupBox2.Location = new System.Drawing.Point(208, 149);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(426, 163);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Expected Research Benefits";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericResources);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.budgetPercentage);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.availableResources);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.completionTime);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.completionResources);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(208, 23);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(426, 108);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Resource Allocation";
            // 
            // numericResources
            // 
            this.numericResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericResources.Location = new System.Drawing.Point(358, 86);
            this.numericResources.Name = "numericResources";
            this.numericResources.Size = new System.Drawing.Size(53, 13);
            this.numericResources.TabIndex = 11;
            this.numericResources.Text = "0";
            this.numericResources.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 86);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(148, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Resources allocated per year:";
            // 
            // budgetPercentage
            // 
            this.budgetPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.budgetPercentage.BackColor = System.Drawing.SystemColors.Info;
            this.budgetPercentage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.budgetPercentage.Location = new System.Drawing.Point(370, 70);
            this.budgetPercentage.Name = "budgetPercentage";
            this.budgetPercentage.Size = new System.Drawing.Size(53, 16);
            this.budgetPercentage.TabIndex = 9;
            this.budgetPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.budgetPercentage.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.budgetPercentage.ValueChanged += new System.EventHandler(this.ParameterChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 69);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(185, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Resources budgeted for research (%):";
            // 
            // availableResources
            // 
            this.availableResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.availableResources.Location = new System.Drawing.Point(358, 52);
            this.availableResources.Name = "availableResources";
            this.availableResources.Size = new System.Drawing.Size(53, 13);
            this.availableResources.TabIndex = 7;
            this.availableResources.Text = "0";
            this.availableResources.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(234, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Resources available from all owned planets:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // completionTime
            // 
            this.completionTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.completionTime.Location = new System.Drawing.Point(358, 35);
            this.completionTime.Name = "completionTime";
            this.completionTime.Size = new System.Drawing.Size(53, 13);
            this.completionTime.TabIndex = 4;
            this.completionTime.Text = "0";
            this.completionTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(178, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Estimated time to completion (years):";
            // 
            // completionResources
            // 
            this.completionResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.completionResources.Location = new System.Drawing.Point(358, 18);
            this.completionResources.Name = "completionResources";
            this.completionResources.Size = new System.Drawing.Size(53, 13);
            this.completionResources.TabIndex = 2;
            this.completionResources.Text = "0";
            this.completionResources.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Resources needed to research next level:";
            // 
            // researchBenefits
            // 
            this.researchBenefits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.researchBenefits.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Discovery,
            this.RequiredTech});
            this.researchBenefits.Location = new System.Drawing.Point(6, 19);
            this.researchBenefits.Name = "researchBenefits";
            this.researchBenefits.Size = new System.Drawing.Size(417, 138);
            this.researchBenefits.TabIndex = 0;
            // 
            // Discovery
            // 
            this.Discovery.Frozen = true;
            this.Discovery.HeaderText = "Discovery";
            this.Discovery.Name = "Discovery";
            this.Discovery.ReadOnly = true;
            this.Discovery.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Discovery.Width = 200;
            // 
            // RequiredTech
            // 
            this.RequiredTech.Frozen = true;
            this.RequiredTech.HeaderText = "Required Tech";
            this.RequiredTech.Name = "RequiredTech";
            this.RequiredTech.ReadOnly = true;
            this.RequiredTech.Width = 200;
            // 
            // ResearchDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 353);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResearchDialog";
            this.Text = "Stars! Nova - Research";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.budgetPercentage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.researchBenefits)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

       private System.Windows.Forms.GroupBox groupBox1;
       private System.Windows.Forms.Label label2;
       private System.Windows.Forms.Label label1;
       private System.Windows.Forms.Button okButton;
       private System.Windows.Forms.RadioButton biotechButton;
       private System.Windows.Forms.RadioButton electronicsButton;
       private System.Windows.Forms.RadioButton constructionButton;
       private System.Windows.Forms.RadioButton propulsionButton;
       private System.Windows.Forms.RadioButton weaponsButton;
       private System.Windows.Forms.RadioButton energyButton;
       private System.Windows.Forms.GroupBox groupBox2;
       private System.Windows.Forms.Label biotechLevel;
       private System.Windows.Forms.Label electronicsLevel;
       private System.Windows.Forms.Label constructionLevel;
       private System.Windows.Forms.Label propulsionLevel;
       private System.Windows.Forms.Label weaponsLevel;
       private System.Windows.Forms.Label energyLevel;
       private System.Windows.Forms.GroupBox groupBox3;
       private System.Windows.Forms.Label completionTime;
       private System.Windows.Forms.Label label4;
       private System.Windows.Forms.Label completionResources;
       private System.Windows.Forms.Label label3;
       private System.Windows.Forms.NumericUpDown budgetPercentage;
       private System.Windows.Forms.Label label7;
       private System.Windows.Forms.Label availableResources;
       private System.Windows.Forms.Label label5;
       private System.Windows.Forms.Label numericResources;
       private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView researchBenefits;
        private System.Windows.Forms.DataGridViewTextBoxColumn Discovery;
        private System.Windows.Forms.DataGridViewTextBoxColumn RequiredTech;
    }
}