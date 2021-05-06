using System;
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

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Arm.Connect();
            Camera.Open();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Arm.Disconnect();
            Camera.Exit();
        }

        private void buttonHoming_Click(object sender, EventArgs e)
        {
            Arm.Do(new Homing());
        }
    }
}