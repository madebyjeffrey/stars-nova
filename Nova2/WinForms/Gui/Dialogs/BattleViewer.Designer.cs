namespace Nova.WinForms.Gui
{
   public partial class BattleViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BattleViewer));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.battlePanel = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ZoomLevel = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.stackGroupBox = new System.Windows.Forms.GroupBox();
            this.stackQuantity = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.stackKey = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.stackArmor = new System.Windows.Forms.Label();
            this.stackOwner = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.stackShields = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.stackDesign = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.damage = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.componentTarget = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.weaponPower = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.Play = new System.Windows.Forms.Button();
            this.stepNumber = new System.Windows.Forms.Label();
            this.nextStep = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.targetQuantity = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.targetKey = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.targetArmor = new System.Windows.Forms.Label();
            this.targetShields = new System.Windows.Forms.Label();
            this.targetOwner = new System.Windows.Forms.Label();
            this.targetDesign = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.movedFrom = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.movedTo = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.battleLocation = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timerNext = new System.Windows.Forms.Timer(this.components);
            this.trackBarBattle = new System.Windows.Forms.TrackBar();
            this.numericUpDownSpeed = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.imageListDamage = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.stackGroupBox.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBattle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.battlePanel);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(870, 652);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Battle View";
            // 
            // battlePanel
            // 
            this.battlePanel.BackColor = System.Drawing.Color.Black;
            this.battlePanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.battlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.battlePanel.Location = new System.Drawing.Point(3, 16);
            this.battlePanel.Name = "battlePanel";
            this.battlePanel.Size = new System.Drawing.Size(864, 633);
            this.battlePanel.TabIndex = 0;
            this.battlePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.numericUpDownSpeed);
            this.groupBox2.Controls.Add(this.trackBarBattle);
            this.groupBox2.Controls.Add(this.ZoomLevel);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.stackGroupBox);
            this.groupBox2.Controls.Add(this.groupBox6);
            this.groupBox2.Controls.Add(this.groupBox5);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.battleLocation);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(902, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(472, 658);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Battle Details";
            // 
            // ZoomLevel
            // 
            this.ZoomLevel.FormattingEnabled = true;
            this.ZoomLevel.Items.AddRange(new object[] {
            "Show all ships",
            "Show all Armed ships",
            "Follow the selected ship ",
            "Maintain initial Zoom Setting"});
            this.ZoomLevel.Location = new System.Drawing.Point(10, 631);
            this.ZoomLevel.Name = "ZoomLevel";
            this.ZoomLevel.Size = new System.Drawing.Size(259, 21);
            this.ZoomLevel.TabIndex = 9;
            this.ZoomLevel.Text = "Select Zoom Level";
            this.ZoomLevel.SelectedIndexChanged += new System.EventHandler(this.ZoomLevel_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(19, 49);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(241, 53);
            this.textBox1.TabIndex = 8;
            this.textBox1.Tag = "";
            this.textBox1.Text = "Extra fields added for debugging the combat engine. Will clean up later.";
            // 
            // stackGroupBox
            // 
            this.stackGroupBox.Controls.Add(this.stackQuantity);
            this.stackGroupBox.Controls.Add(this.label16);
            this.stackGroupBox.Controls.Add(this.stackKey);
            this.stackGroupBox.Controls.Add(this.label12);
            this.stackGroupBox.Controls.Add(this.label15);
            this.stackGroupBox.Controls.Add(this.stackArmor);
            this.stackGroupBox.Controls.Add(this.stackOwner);
            this.stackGroupBox.Controls.Add(this.label11);
            this.stackGroupBox.Controls.Add(this.label2);
            this.stackGroupBox.Controls.Add(this.stackShields);
            this.stackGroupBox.Controls.Add(this.label13);
            this.stackGroupBox.Controls.Add(this.stackDesign);
            this.stackGroupBox.Location = new System.Drawing.Point(10, 116);
            this.stackGroupBox.Name = "stackGroupBox";
            this.stackGroupBox.Size = new System.Drawing.Size(252, 181);
            this.stackGroupBox.TabIndex = 7;
            this.stackGroupBox.TabStop = false;
            this.stackGroupBox.Text = "Active stack / Attacker";
            // 
            // stackQuantity
            // 
            this.stackQuantity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackQuantity.Location = new System.Drawing.Point(105, 62);
            this.stackQuantity.Name = "stackQuantity";
            this.stackQuantity.Size = new System.Drawing.Size(141, 18);
            this.stackQuantity.TabIndex = 13;
            this.stackQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 65);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 13);
            this.label16.TabIndex = 12;
            this.label16.Text = "# of ships";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stackKey
            // 
            this.stackKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackKey.Location = new System.Drawing.Point(105, 35);
            this.stackKey.Name = "stackKey";
            this.stackKey.Size = new System.Drawing.Size(141, 18);
            this.stackKey.TabIndex = 11;
            this.stackKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 38);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "stack.Key";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 142);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(95, 13);
            this.label15.TabIndex = 9;
            this.label15.Text = "Total Token Armor";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stackArmor
            // 
            this.stackArmor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackArmor.Location = new System.Drawing.Point(105, 139);
            this.stackArmor.Name = "stackArmor";
            this.stackArmor.Size = new System.Drawing.Size(141, 18);
            this.stackArmor.TabIndex = 8;
            this.stackArmor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // stackOwner
            // 
            this.stackOwner.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackOwner.Location = new System.Drawing.Point(105, 13);
            this.stackOwner.Name = "stackOwner";
            this.stackOwner.Size = new System.Drawing.Size(141, 18);
            this.stackOwner.TabIndex = 1;
            this.stackOwner.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 117);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Stack Shields";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Owner";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stackShields
            // 
            this.stackShields.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackShields.Location = new System.Drawing.Point(105, 114);
            this.stackShields.Name = "stackShields";
            this.stackShields.Size = new System.Drawing.Size(141, 18);
            this.stackShields.TabIndex = 6;
            this.stackShields.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 91);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(40, 13);
            this.label13.TabIndex = 5;
            this.label13.Text = "Design";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stackDesign
            // 
            this.stackDesign.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stackDesign.Location = new System.Drawing.Point(105, 88);
            this.stackDesign.Name = "stackDesign";
            this.stackDesign.Size = new System.Drawing.Size(141, 18);
            this.stackDesign.TabIndex = 4;
            this.stackDesign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.damage);
            this.groupBox6.Controls.Add(this.label10);
            this.groupBox6.Controls.Add(this.componentTarget);
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.weaponPower);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Location = new System.Drawing.Point(287, 317);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(170, 236);
            this.groupBox6.TabIndex = 6;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Weapon Discharge";
            // 
            // damage
            // 
            this.damage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.damage.Location = new System.Drawing.Point(11, 214);
            this.damage.Name = "damage";
            this.damage.Size = new System.Drawing.Size(141, 18);
            this.damage.TabIndex = 13;
            this.damage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(53, 198);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "Damage";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // componentTarget
            // 
            this.componentTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.componentTarget.Location = new System.Drawing.Point(11, 167);
            this.componentTarget.Name = "componentTarget";
            this.componentTarget.Size = new System.Drawing.Size(141, 18);
            this.componentTarget.TabIndex = 11;
            this.componentTarget.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(33, 150);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Component Target";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // weaponPower
            // 
            this.weaponPower.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.weaponPower.Location = new System.Drawing.Point(11, 116);
            this.weaponPower.Name = "weaponPower";
            this.weaponPower.Size = new System.Drawing.Size(141, 18);
            this.weaponPower.TabIndex = 9;
            this.weaponPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(37, 95);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Weapon Power";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.Play);
            this.groupBox5.Controls.Add(this.stepNumber);
            this.groupBox5.Controls.Add(this.nextStep);
            this.groupBox5.Location = new System.Drawing.Point(10, 495);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(259, 87);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Replay Control";
            // 
            // Play
            // 
            this.Play.Image = global::Nova.Properties.Resources.copyright_free_play_button;
            this.Play.Location = new System.Drawing.Point(138, 20);
            this.Play.Name = "Play";
            this.Play.Size = new System.Drawing.Size(98, 61);
            this.Play.TabIndex = 4;
            this.Play.Text = "  Play";
            this.Play.UseVisualStyleBackColor = true;
            this.Play.Click += new System.EventHandler(this.Play_Click);
            // 
            // stepNumber
            // 
            this.stepNumber.Location = new System.Drawing.Point(11, 20);
            this.stepNumber.Name = "stepNumber";
            this.stepNumber.Size = new System.Drawing.Size(100, 23);
            this.stepNumber.TabIndex = 3;
            this.stepNumber.Text = "Step 1 of 10";
            // 
            // nextStep
            // 
            this.nextStep.Location = new System.Drawing.Point(11, 58);
            this.nextStep.Name = "nextStep";
            this.nextStep.Size = new System.Drawing.Size(75, 23);
            this.nextStep.TabIndex = 2;
            this.nextStep.Text = "Next";
            this.nextStep.UseVisualStyleBackColor = true;
            this.nextStep.Click += new System.EventHandler(this.NextStep_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.targetQuantity);
            this.groupBox4.Controls.Add(this.label19);
            this.groupBox4.Controls.Add(this.targetKey);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.targetArmor);
            this.groupBox4.Controls.Add(this.targetShields);
            this.groupBox4.Controls.Add(this.targetOwner);
            this.groupBox4.Controls.Add(this.targetDesign);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Location = new System.Drawing.Point(10, 316);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(259, 173);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Weapons Target";
            // 
            // targetQuantity
            // 
            this.targetQuantity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetQuantity.Location = new System.Drawing.Point(109, 65);
            this.targetQuantity.Name = "targetQuantity";
            this.targetQuantity.Size = new System.Drawing.Size(141, 18);
            this.targetQuantity.TabIndex = 17;
            this.targetQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 68);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(53, 13);
            this.label19.TabIndex = 16;
            this.label19.Text = "# of ships";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // targetKey
            // 
            this.targetKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetKey.Location = new System.Drawing.Point(109, 43);
            this.targetKey.Name = "targetKey";
            this.targetKey.Size = new System.Drawing.Size(141, 18);
            this.targetKey.TabIndex = 15;
            this.targetKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 46);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(55, 13);
            this.label17.TabIndex = 14;
            this.label17.Text = "target.Key";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // targetArmor
            // 
            this.targetArmor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetArmor.Location = new System.Drawing.Point(109, 137);
            this.targetArmor.Name = "targetArmor";
            this.targetArmor.Size = new System.Drawing.Size(141, 18);
            this.targetArmor.TabIndex = 8;
            this.targetArmor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // targetShields
            // 
            this.targetShields.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetShields.Location = new System.Drawing.Point(109, 113);
            this.targetShields.Name = "targetShields";
            this.targetShields.Size = new System.Drawing.Size(141, 18);
            this.targetShields.TabIndex = 7;
            this.targetShields.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // targetOwner
            // 
            this.targetOwner.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetOwner.Location = new System.Drawing.Point(109, 18);
            this.targetOwner.Name = "targetOwner";
            this.targetOwner.Size = new System.Drawing.Size(141, 18);
            this.targetOwner.TabIndex = 6;
            this.targetOwner.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // targetDesign
            // 
            this.targetDesign.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetDesign.Location = new System.Drawing.Point(109, 88);
            this.targetDesign.Name = "targetDesign";
            this.targetDesign.Size = new System.Drawing.Size(141, 18);
            this.targetDesign.TabIndex = 5;
            this.targetDesign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Design";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 140);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Armor";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Shields";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Owner";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.movedFrom);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.movedTo);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(290, 49);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(167, 202);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Movement";
            // 
            // movedFrom
            // 
            this.movedFrom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.movedFrom.Location = new System.Drawing.Point(9, 108);
            this.movedFrom.Name = "movedFrom";
            this.movedFrom.Size = new System.Drawing.Size(141, 18);
            this.movedFrom.TabIndex = 5;
            this.movedFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(40, 83);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(66, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "Moved From";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // movedTo
            // 
            this.movedTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.movedTo.Location = new System.Drawing.Point(9, 171);
            this.movedTo.Name = "movedTo";
            this.movedTo.Size = new System.Drawing.Size(141, 18);
            this.movedTo.TabIndex = 3;
            this.movedTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Moved To";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // battleLocation
            // 
            this.battleLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.battleLocation.Location = new System.Drawing.Point(102, 21);
            this.battleLocation.Name = "battleLocation";
            this.battleLocation.Size = new System.Drawing.Size(364, 13);
            this.battleLocation.TabIndex = 1;
            this.battleLocation.Text = "Location";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Battle Location:";
            // 
            // timerNext
            // 
            this.timerNext.Tick += new System.EventHandler(this.timerNext_Tick);
            // 
            // trackBarBattle
            // 
            this.trackBarBattle.Location = new System.Drawing.Point(10, 583);
            this.trackBarBattle.Name = "trackBarBattle";
            this.trackBarBattle.Size = new System.Drawing.Size(259, 45);
            this.trackBarBattle.TabIndex = 10;
            this.trackBarBattle.DragLeave += new System.EventHandler(this.trackBarBattle_DragLeave);
            // 
            // numericUpDownSpeed
            // 
            this.numericUpDownSpeed.Location = new System.Drawing.Point(319, 629);
            this.numericUpDownSpeed.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSpeed.Name = "numericUpDownSpeed";
            this.numericUpDownSpeed.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownSpeed.TabIndex = 11;
            this.numericUpDownSpeed.ValueChanged += new System.EventHandler(this.numericUpDownSpeed_ValueChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(358, 613);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(38, 13);
            this.label18.TabIndex = 12;
            this.label18.Text = "Speed";
            // 
            // imageListDamage
            // 
            this.imageListDamage.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListDamage.ImageStream")));
            this.imageListDamage.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListDamage.Images.SetKeyName(0, "Mole-skin_Shield.png");
            // 
            // BattleViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1386, 677);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BattleViewer";
            this.Text = "Stars! Nova - Battle Viewer";
            this.Load += new System.EventHandler(this.OnLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.stackGroupBox.ResumeLayout(false);
            this.stackGroupBox.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBattle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSpeed)).EndInit();
            this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Panel battlePanel;
      private System.Windows.Forms.GroupBox groupBox2;
       private System.Windows.Forms.Label battleLocation;
       private System.Windows.Forms.Label label1;
       private System.Windows.Forms.Button nextStep;
       private System.Windows.Forms.GroupBox groupBox3;
       private System.Windows.Forms.Label movedTo;
       private System.Windows.Forms.Label label4;
       private System.Windows.Forms.Label stackOwner;
       private System.Windows.Forms.Label label2;
       private System.Windows.Forms.GroupBox groupBox4;
       private System.Windows.Forms.Label targetArmor;
       private System.Windows.Forms.Label targetShields;
       private System.Windows.Forms.Label targetOwner;
       private System.Windows.Forms.Label targetDesign;
       private System.Windows.Forms.Label label7;
       private System.Windows.Forms.Label label6;
       private System.Windows.Forms.Label label5;
       private System.Windows.Forms.Label label3;
       private System.Windows.Forms.GroupBox groupBox5;
       private System.Windows.Forms.Label stepNumber;
       private System.Windows.Forms.GroupBox groupBox6;
       private System.Windows.Forms.Label weaponPower;
       private System.Windows.Forms.Label label8;
       private System.Windows.Forms.Label componentTarget;
       private System.Windows.Forms.Label label9;
       private System.Windows.Forms.Label damage;
       private System.Windows.Forms.Label label10;
       private System.Windows.Forms.GroupBox stackGroupBox;
       private System.Windows.Forms.Label label15;
       private System.Windows.Forms.Label stackArmor;
       private System.Windows.Forms.Label label11;
       private System.Windows.Forms.Label stackShields;
       private System.Windows.Forms.Label label13;
       private System.Windows.Forms.Label stackDesign;
       private System.Windows.Forms.TextBox textBox1;
       private System.Windows.Forms.Label stackKey;
       private System.Windows.Forms.Label label12;
       private System.Windows.Forms.Label movedFrom;
       private System.Windows.Forms.Label label14;
       private System.Windows.Forms.Label stackQuantity;
       private System.Windows.Forms.Label label16;
       private System.Windows.Forms.Label targetQuantity;
       private System.Windows.Forms.Label label19;
       private System.Windows.Forms.Label targetKey;
       private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox ZoomLevel;
        private System.Windows.Forms.Button Play;
        private System.Windows.Forms.Timer timerNext;
        private System.Windows.Forms.NumericUpDown numericUpDownSpeed;
        private System.Windows.Forms.TrackBar trackBarBattle;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ImageList imageListDamage;
    }
}