
namespace hiwinrobot_vision_positioning
{
    partial class Form1
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
            if (disposing && (components != null))
            {
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
            this.pictureBoxMain = new System.Windows.Forms.PictureBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonHoming = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxEnableArm = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxMain
            // 
            this.pictureBoxMain.Location = new System.Drawing.Point(368, 12);
            this.pictureBoxMain.Name = "pictureBoxMain";
            this.pictureBoxMain.Size = new System.Drawing.Size(1344, 756);
            this.pictureBoxMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxMain.TabIndex = 0;
            this.pictureBoxMain.TabStop = false;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(3, 3);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(164, 67);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(3, 76);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(164, 67);
            this.buttonDisconnect.TabIndex = 1;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonHoming
            // 
            this.buttonHoming.Enabled = false;
            this.buttonHoming.Location = new System.Drawing.Point(3, 149);
            this.buttonHoming.Name = "buttonHoming";
            this.buttonHoming.Size = new System.Drawing.Size(164, 67);
            this.buttonHoming.TabIndex = 1;
            this.buttonHoming.Text = "Homing";
            this.buttonHoming.UseVisualStyleBackColor = true;
            this.buttonHoming.Click += new System.EventHandler(this.buttonHoming_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Enabled = false;
            this.buttonStart.Location = new System.Drawing.Point(3, 222);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(164, 67);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonConnect);
            this.panel1.Controls.Add(this.buttonStart);
            this.panel1.Controls.Add(this.buttonDisconnect);
            this.panel1.Controls.Add(this.buttonHoming);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(208, 323);
            this.panel1.TabIndex = 3;
            // 
            // checkBoxEnableArm
            // 
            this.checkBoxEnableArm.AutoSize = true;
            this.checkBoxEnableArm.Checked = true;
            this.checkBoxEnableArm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableArm.Location = new System.Drawing.Point(15, 433);
            this.checkBoxEnableArm.Name = "checkBoxEnableArm";
            this.checkBoxEnableArm.Size = new System.Drawing.Size(201, 36);
            this.checkBoxEnableArm.TabIndex = 4;
            this.checkBoxEnableArm.Text = "Enable Arm";
            this.checkBoxEnableArm.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1950, 1158);
            this.Controls.Add(this.checkBoxEnableArm);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureBoxMain);
            this.Name = "Form1";
            this.Text = "Vision Positioning";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxMain;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Button buttonHoming;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBoxEnableArm;
    }
}

