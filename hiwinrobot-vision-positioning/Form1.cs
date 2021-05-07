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

        private readonly Rectangle _aoi = new Rectangle(500, 400, 1920, 1080);

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

        private void GetCornersOfAruco(out int[] idsOut, out PointF[][] cornersOut)
        {
            var frame = new Mat(Camera.GetImage().ToMat(), _aoi);

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

                idsOut = ids.ToArray();
                cornersOut = corners.ToArrayOfArray();
            }
        }

        private void SaveArucoData(int[] ids, PointF[][] corners)
        {
            var csvData = new List<List<string>>();
            for (int row = 0; row < ids.Length; row++)
            {
                var csvDataRow = new List<string>
                {
                    ids[row].ToString(),
                    corners[row][0].ToString(),
                    corners[row][1].ToString(),
                    corners[row][2].ToString(),
                    corners[row][3].ToString()
                };

                csvData.Add(csvDataRow);
            }

            var csv = new CsvHandler("");
            csv.Write("aruco_data.csv",
                      csvData,
                      new List<string> { "id", "corner_1", "corner_2", "corner_3", "corner_4" });
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
            GetCornersOfAruco(out var ids, out var corners);
            SaveArucoData(ids, corners);
        }

        #endregion
    }
}