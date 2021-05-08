using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly string _armIp = "192.168.0.3";
        private readonly float _allowableError = 10;
        private readonly IArmController _arm;
        private readonly IDSCamera _camera;
        private readonly Rectangle _aoi = new Rectangle(500, 400, 1920, 1080);
        private readonly DetectorParameters _detectorParameters;

        private Dictionary _dict;

        private Dictionary ArucoDictionary
            => _dict ?? (_dict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100));

        public Form1()
        {
            InitializeComponent();

            _detectorParameters = DetectorParameters.GetDefault();
            IMessage message = new NormalMessage(new LogHandler());
            _arm = new ArmController(_armIp, message);
            _camera = new IDSCamera(message);
            _camera.Init();
        }

        private void ProcessFrame(object sender, EventArgs args)
        {
            var frame = GetImage();
            GetInfoOfAruco(out var ids, out var corners, out var frameSize);
            if (ids.Size > 0)
            {
                DrawArucoMarkers(ref frame, ids, corners);
            }
            pictureBoxMain.Image = frame.Clone().ToBitmap();
        }

        private Mat GetImage()
        {
            return new Mat(_camera.GetImage().ToMat(), _aoi);
        }

        private void GetInfoOfAruco(out VectorOfInt ids, out VectorOfVectorOfPointF corners, out Size frameSize)
        {
            var frame = GetImage();
            frameSize = frame.Size;

            using (var idsVector = new VectorOfInt())
            using (var cornersVector = new VectorOfVectorOfPointF())
            using (var rejectedVector = new VectorOfVectorOfPointF())
            {
                ArucoInvoke.DetectMarkers(frame,
                                          ArucoDictionary,
                                          cornersVector,
                                          idsVector,
                                          _detectorParameters,
                                          rejectedVector);

                ids = idsVector;
                corners = cornersVector;
            }
        }

        private void DrawArucoMarkers(ref Mat frame, VectorOfInt ids, VectorOfVectorOfPointF corners)
        {
            if (ids.Size > 0)
            {
                ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(0, 255, 0));
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

        private void AutoPositioning()
        {
            var positionDone = false;
            while (!positionDone)
            {
                GetInfoOfAruco(out var ids, out var corners, out var frameSize);
                if (ids.Size > 0)
                {
                    var centerOfFrame = new Point(frameSize.Width / 2, frameSize.Height / 2);
                    var error = new PointF(corners[0][0].X - centerOfFrame.X,
                                           corners[0][0].Y - centerOfFrame.Y);

                    if (Math.Abs(error.X) < _allowableError && Math.Abs(error.Y) < _allowableError)
                    {
                        positionDone = true;
                    }
                    else
                    {
                        ArmMove(CalArmOffset(error));
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private void ArmMove(PointF value)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Do(new RelativeMotion(value.X, value.Y, 0));
            }
        }

        private PointF CalArmOffset(PointF error)
        {
            PointF armOffset = default;

            // X.
            if (error.X > 0)
                armOffset.X = 1;
            else if (error.X < 0)
                armOffset.X = -1;
            else
                armOffset.X = 0;

            // Y.
            if (error.Y > 0)
                armOffset.Y = 1;
            else if (error.Y < 0)
                armOffset.Y = -1;
            else
                armOffset.Y = 0;

            return armOffset;
        }

        #region Button

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Connect();
                buttonHoming.Enabled = true;
            }

            _camera.Open();
            buttonStart.Enabled = true;
            checkBoxEnableArm.Enabled = false;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Disconnect();
            }
            _camera.Exit();

            buttonStart.Enabled = false;
            buttonHoming.Enabled = false;
            checkBoxEnableArm.Enabled = true;
        }

        private void buttonHoming_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Do(new Homing());
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            AutoPositioning();
            Application.Idle += ProcessFrame;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Application.Idle -= ProcessFrame;
        }

        #endregion
    }
}