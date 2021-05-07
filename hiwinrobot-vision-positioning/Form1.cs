﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFUIRSL.HRTK;
using NFUIRSL.HRTK.Vision;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace hiwinrobot_vision_positioning
{
    public partial class Form1 : Form
    {
        private readonly string ArmIp = "192.168.0.3";
        private IMessage Message;
        private IArmController Arm;
        private IDSCamera Camera;

        public Form1()
        {
            InitializeComponent();

            Message = new NormalMessage(new LogHandler());
            Arm = new ArmController(ArmIp, Message);
            Camera = new IDSCamera(Message);
            Camera.Init();
        }

        #region Button

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Arm.Connect();
            Camera.Open();

            buttonStart.Enabled = true;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Arm.Disconnect();
            Camera.Exit();

            buttonStart.Enabled = false;
        }

        private void buttonHoming_Click(object sender, EventArgs e)
        {
            Arm.Do(new Homing());
        }

        private void buttonStart_Click(object sender, EventArgs e)
        { }

        #endregion
    }
}