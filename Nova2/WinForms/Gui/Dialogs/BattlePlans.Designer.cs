namespace Nova.WinForms.Gui
{
    using Nova.Common;
   public partial class BattlePlans
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BattlePlans));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.planList = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.modifyPlan = new System.Windows.Forms.Button();
            this.newPlan = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.planName = new System.Windows.Forms.TextBox();
            this.secondaryTarget = new System.Windows.Forms.ComboBox();
            this.attack = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tactic = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.primaryTarget = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.doneButton = new System.Windows.Forms.Button();
            this.tertiaryTarget = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.quaternaryTarget = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.quinaryTarget = new System.Windows.Forms.ComboBox();
            this.quinaryTarget.Items.Add(Global.VictimNames[0]);
            this.quinaryTarget.Items.Add(Global.VictimNames[1]);
            this.quinaryTarget.Items.Add(Global.VictimNames[2]);
            this.quinaryTarget.Items.Add(Global.VictimNames[3]);
            this.quinaryTarget.Items.Add(Global.VictimNames[4]);
            this.quinaryTarget.Items.Add(Global.VictimNames[5]);
            this.quinaryTarget.Items.Add(Global.VictimNames[6]);
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.planList);
            this.groupBox1.Location = new System.Drawing.Point(13, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(216, 298);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Available Plans";
            // 
            // planList
            // 
            this.planList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.planList.FormattingEnabled = true;
            this.planList.Location = new System.Drawing.Point(3, 16);
            this.planList.Name = "planList";
            this.planList.Size = new System.Drawing.Size(210, 279);
            this.planList.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.quinaryTarget);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.quaternaryTarget);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.tertiaryTarget);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.modifyPlan);
            this.groupBox2.Controls.Add(this.newPlan);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.planName);
            this.groupBox2.Controls.Add(this.secondaryTarget);
            this.groupBox2.Controls.Add(this.attack);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tactic);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.primaryTarget);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(242, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(216, 364);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plan Details";
            // 
            // modifyPlan
            // 
            this.modifyPlan.Enabled = false;
            this.modifyPlan.Location = new System.Drawing.Point(125, 335);
            this.modifyPlan.Name = "modifyPlan";
            this.modifyPlan.Size = new System.Drawing.Size(75, 23);
            this.modifyPlan.TabIndex = 10;
            this.modifyPlan.Text = "Modify";
            this.modifyPlan.UseVisualStyleBackColor = true;
            // 
            // newPlan
            // 
            this.newPlan.Enabled = false;
            this.newPlan.Location = new System.Drawing.Point(9, 335);
            this.newPlan.Name = "newPlan";
            this.newPlan.Size = new System.Drawing.Size(75, 23);
            this.newPlan.TabIndex = 0;
            this.newPlan.Text = "New";
            this.newPlan.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Name";
            // 
            // planName
            // 
            this.planName.Location = new System.Drawing.Point(6, 33);
            this.planName.Name = "planName";
            this.planName.Size = new System.Drawing.Size(194, 20);
            this.planName.TabIndex = 1;
            // 
            // secondaryTarget
            // 
            this.secondaryTarget.FormattingEnabled = true;
            this.secondaryTarget.Items.Add(Global.VictimNames[0]);
            this.secondaryTarget.Items.Add(Global.VictimNames[1]);
            this.secondaryTarget.Items.Add(Global.VictimNames[2]);
            this.secondaryTarget.Items.Add(Global.VictimNames[3]);
            this.secondaryTarget.Items.Add(Global.VictimNames[4]);
            this.secondaryTarget.Items.Add(Global.VictimNames[5]);
            this.secondaryTarget.Items.Add(Global.VictimNames[6]);
            this.secondaryTarget.Location = new System.Drawing.Point(6, 109);
            this.secondaryTarget.Name = "secondaryTarget";
            this.secondaryTarget.Size = new System.Drawing.Size(194, 21);
            this.secondaryTarget.TabIndex = 8;
            // 
            // attack
            // 
            this.attack.FormattingEnabled = true;
            this.attack.Items.AddRange(new object[] {
            "Enemies",
            "Enemies and Neutrals",
            "Everyone"});
            this.attack.Location = new System.Drawing.Point(6, 302);
            this.attack.Name = "attack";
            this.attack.Size = new System.Drawing.Size(194, 21);
            this.attack.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 286);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Attack";
            // 
            // tactic
            // 
            this.tactic.FormattingEnabled = true;
            this.tactic.Items.AddRange(new object[] {
            "Disengage",
            "Disengage if Challenged",
            "Maximise Damage",
            "Maximise Damage Ratio",
            "Maximise Net Damage",
            "Minimise Damage to Self"});
            this.tactic.Location = new System.Drawing.Point(6, 262);
            this.tactic.Name = "tactic";
            this.tactic.Size = new System.Drawing.Size(194, 21);
            this.tactic.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 246);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Tactic";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Secondary Target";
            // 
            // primaryTarget
            // 
            this.primaryTarget.FormattingEnabled = true;
            this.primaryTarget.Items.Add(Global.VictimNames[0]);
            this.primaryTarget.Items.Add(Global.VictimNames[1]);
            this.primaryTarget.Items.Add(Global.VictimNames[2]);
            this.primaryTarget.Items.Add(Global.VictimNames[3]);
            this.primaryTarget.Items.Add(Global.VictimNames[4]);
            this.primaryTarget.Items.Add(Global.VictimNames[5]);
            this.primaryTarget.Items.Add(Global.VictimNames[6]);
            this.primaryTarget.Location = new System.Drawing.Point(6, 73);
            this.primaryTarget.Name = "primaryTarget";
            this.primaryTarget.Size = new System.Drawing.Size(194, 21);
            this.primaryTarget.TabIndex = 1;
            this.primaryTarget.SelectedIndexChanged += new System.EventHandler(this.primaryTarget_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Primary Target";
            // 
            // doneButton
            // 
            this.doneButton.Location = new System.Drawing.Point(370, 378);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 2;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // tertiaryTarget
            // 
            this.tertiaryTarget.FormattingEnabled = true;
            this.tertiaryTarget.Items.Add(Global.VictimNames[0]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[1]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[2]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[3]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[4]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[5]);
            this.tertiaryTarget.Items.Add(Global.VictimNames[6]);
            this.tertiaryTarget.Location = new System.Drawing.Point(6, 148);
            this.tertiaryTarget.Name = "tertiaryTarget";
            this.tertiaryTarget.Size = new System.Drawing.Size(194, 21);
            this.tertiaryTarget.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Tertiary Target";
            // 
            // quaternaryTarget
            // 
            this.quaternaryTarget.FormattingEnabled = true;
            this.quaternaryTarget.Items.Add(Global.VictimNames[0]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[1]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[2]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[3]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[4]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[5]);
            this.quaternaryTarget.Items.Add(Global.VictimNames[6]);
            this.quaternaryTarget.Location = new System.Drawing.Point(6, 186);
            this.quaternaryTarget.Name = "quaternaryTarget";
            this.quaternaryTarget.Size = new System.Drawing.Size(194, 21);
            this.quaternaryTarget.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 172);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Quaternary Target";
            // 
            // quinaryTarget
            // 
            this.quinaryTarget.FormattingEnabled = true;
            this.quinaryTarget.Items.Add(Global.VictimNames[0]);
            this.quinaryTarget.Items.Add(Global.VictimNames[1]);
            this.quinaryTarget.Items.Add(Global.VictimNames[2]);
            this.quinaryTarget.Items.Add(Global.VictimNames[3]);
            this.quinaryTarget.Items.Add(Global.VictimNames[4]);
            this.quinaryTarget.Items.Add(Global.VictimNames[5]);
            this.quinaryTarget.Items.Add(Global.VictimNames[6]);
            this.quinaryTarget.Location = new System.Drawing.Point(6, 222);
            this.quinaryTarget.Name = "quinaryTarget";
            this.quinaryTarget.Size = new System.Drawing.Size(194, 21);
            this.quinaryTarget.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 207);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Quinary Target";
            // 
            // BattlePlans
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 404);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BattlePlans";
            this.Text = "Stars! Nova - Battle Plans";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.ListBox planList;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.ComboBox attack;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.ComboBox tactic;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.ComboBox primaryTarget;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ComboBox secondaryTarget;
      private System.Windows.Forms.Button doneButton;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox planName;
      private System.Windows.Forms.Button newPlan;
      private System.Windows.Forms.Button modifyPlan;
        private System.Windows.Forms.ComboBox quinaryTarget;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox quaternaryTarget;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox tertiaryTarget;
        private System.Windows.Forms.Label label6;
    }
}