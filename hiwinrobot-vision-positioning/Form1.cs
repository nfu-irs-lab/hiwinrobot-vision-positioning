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
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace hiwinrobot_vision_positioning
{
    public partial class Form1 : Form
    {
        private readonly string ArmIp = "192.168.0.3";
        private IMessage Message;
        private IArmController Arm;
        private IDSCamera Camera;

        private DetectorParameters _detectorParameters;

        private Dictionary _dict;

        private Dictionary ArucoDictionary
        {
            get
            {
                if (_dict == null)
                {
                    _dict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100);
                }
                return _dict;
            }
        }

        public Form1()
        {
            InitializeComponent();

            _detectorParameters = DetectorParameters.GetDefault();
            Message = new NormalMessage(new LogHandler());
            Arm = new ArmController(ArmIp, Message);
            Camera = new IDSCamera(Message);
            Camera.Init();
        }

        private void GetCornersOfAruco()
        {
            var frame = Camera.GetImage().ToMat();

            using (var ids = new VectorOfInt())
            using (var corners = new VectorOfVectorOfPointF())
            using (var rejected = new VectorOfVectorOfPointF())
            {
                ArucoInvoke.DetectMarkers(frame, ArucoDictionary, corners, ids, _detectorParameters, rejected);

                if (ids.Size > 0)
                {
                    ArucoInvoke.DrawDetectedDiamonds(frame, corners, ids, new MCvScalar(0, 255, 0));
                }

                pictureBoxMain.Image = frame.Clone().ToBitmap();
            }
        }

        #region Button

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Arm.Connect();
            Camera.Open();

            buttonStart.Enabled = true;
            buttonHoming.Enabled = true;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Arm.Disconnect();
            Camera.Exit();

            buttonStart.Enabled = false;
            buttonHoming.Enabled = false;
        }

        private void buttonHoming_Click(object sender, EventArgs e)
        {
            Arm.Do(new Homing());
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            GetCornersOfAruco();
        }

        #endregion
    }
}