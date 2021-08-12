// #define USE_HIWIN

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
using System.Windows.Forms.VisualStyles;
using RASDK.Arm;
using RASDK.Basic;
using RASDK.Basic.Message;
using RASDK.Vision;
using RASDK.Vision.IDS;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace hiwinrobot_vision_positioning
{
    public partial class Form1 : Form
    {
        private readonly Rectangle _aoi = new Rectangle(500, 400, 1920, 1080);

        private readonly ArmActionFactory _arm;

        private readonly string _armIp
#if USE_HIWIN
            = "192.168.100.111";

#else
            = RASDK.Arm.TMRobot.Default.IpOfArmConnection;
#endif

        private readonly int _armPort = RASDK.Arm.TMRobot.Default.PortOfArmConnection;
        private readonly IDSCamera _camera;
        private readonly DetectorParameters _detectorParameters;

        private Dictionary _dict;

        public Form1()
        {
            InitializeComponent();

            _detectorParameters = DetectorParameters.GetDefault();
            IMessage message = new GeneralMessage(new LogHandler());
#if USE_HIWIN
            _arm = new RASDK.Arm.Hiwin.RoboticArm(_armIp, message);
#else
            _arm = new RASDK.Arm.TMRobot.RoboticArm(_armIp, _armPort, message);
#endif
            _camera = new IDSCamera(message);
            _camera.Init();
        }

        private Dictionary ArucoDictionary
            => _dict ?? (_dict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100));

        private void ArmMove(PointF value)
        {
            if (checkBoxEnableArm.Checked)
            {
                // _arm.Motion().Relative(value.X, value.Y, 0, 0, 0, 0);
                _arm.Motion().Relative(value.Y, -value.X, 0, 0, 0, 0);
                Thread.Sleep(50);
                _arm.Motion().Abort();
            }
        }

        private PointF CalArmOffset(PointF error)
        {
            PointF armOffset = default;

            // X.
            float offsetX;
            if (Math.Abs(error.X) > 100)
                offsetX = 20;
            else if (Math.Abs(error.X) > 50)
                offsetX = 10;
            else if (Math.Abs(error.X) > 10)
                offsetX = 3;
            else
                offsetX = (float)0.5;

            if (error.X > 0)
                armOffset.X = offsetX;
            else if (error.X < 0)
                armOffset.X = -offsetX;
            else
                armOffset.X = 0;

            // Y.
            float offsetY;
            if (Math.Abs(error.Y) > 100)
                offsetY = 20;
            else if (Math.Abs(error.Y) > 50)
                offsetY = 10;
            else if (Math.Abs(error.Y) > 10)
                offsetY = 3;
            else
                offsetY = (float)0.5;

            if (error.Y > 0)
                armOffset.Y = -offsetY;
            else if (error.Y < 0)
                armOffset.Y = offsetY;
            else
                armOffset.Y = 0;

            return armOffset;
        }

        private void DrawArucoMarkers(ref Mat frame, VectorOfInt ids, VectorOfVectorOfPointF corners)
        {
            ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(0, 255, 0));
        }

        private void DrawExtInfo(ref Mat frame, Point nowPoint)
        {
            var frameSize = frame.Size;
            var centerOfFrame = new Point(frameSize.Width / 2, frameSize.Height / 2);

            CvInvoke.PutText(frame,
                             "TARGET",
                             nowPoint,
                             FontFace.HersheyComplex,
                             1.2,
                             new MCvScalar(0, 0, 255));

            // Draw a cross.
            CvInvoke.Line(frame,
                          new Point(centerOfFrame.X - 1, 0),
                          new Point(centerOfFrame.X - 1, frameSize.Height),
                          new MCvScalar(255, 0, 0));
            CvInvoke.Line(frame, new Point(centerOfFrame.X, 0), new Point(centerOfFrame.X, frameSize.Height), new MCvScalar(255, 0, 0));
            CvInvoke.Line(frame,
                          new Point(centerOfFrame.X + 1, 0),
                          new Point(centerOfFrame.X + 1, frameSize.Height),
                          new MCvScalar(255, 0, 0));

            CvInvoke.Line(frame,
                          new Point(0, centerOfFrame.Y - 1),
                          new Point(frameSize.Width, centerOfFrame.Y - 1),
                          new MCvScalar(255, 0, 0));
            CvInvoke.Line(frame, new Point(0, centerOfFrame.Y), new Point(frameSize.Width, centerOfFrame.Y), new MCvScalar(255, 0, 0));
            CvInvoke.Line(frame,
                          new Point(0, centerOfFrame.Y + 1),
                          new Point(frameSize.Width, centerOfFrame.Y + 1),
                          new MCvScalar(255, 0, 0));
        }

        private Mat GetImage()
        {
            return new Mat(_camera.GetImage().ToMat(), _aoi);
        }

        private void GetInfoOfAruco(Mat frame, out VectorOfInt ids, out VectorOfVectorOfPointF corners)
        {
            var idsVector = new VectorOfInt();
            var cornersVector = new VectorOfVectorOfPointF();
            var rejectedVector = new VectorOfVectorOfPointF();

            ArucoInvoke.DetectMarkers(frame,
                                      ArucoDictionary,
                                      cornersVector,
                                      idsVector,
                                      _detectorParameters,
                                      rejectedVector);

            ids = idsVector;
            corners = cornersVector;
        }

        private void ProcessFrame(object sender, EventArgs args)
        {
            var targetArucoId = (int)numericUpDownTargetArucoId.Value;
            var targetArucoIdIndex = 0;

            var frame = GetImage();
            var frameSize = frame.Size;
            var centerOfFrame = new Point(frameSize.Width / 2, frameSize.Height / 2);

            GetInfoOfAruco(frame, out var ids, out var corners);
            if (ids.Size > 0 && Array.IndexOf(ids.ToArray(), targetArucoId) != -1)
            {
                for (int i = 0; i < ids.Size; i++)
                {
                    if (ids[i] == targetArucoId)
                    {
                        targetArucoIdIndex = i;
                        break;
                    }
                }

                var nowPoint = new PointF(corners[targetArucoIdIndex][0].X,
                                          corners[targetArucoIdIndex][0].Y);
                var error = new PointF(nowPoint.X - centerOfFrame.X,
                                       nowPoint.Y - centerOfFrame.Y);

                UpdateInfo(nowPoint, error);
                DrawArucoMarkers(ref frame, ids, corners);
                DrawExtInfo(ref frame, Point.Round(nowPoint));

                if (Math.Abs(error.X) > (double)numericUpDownAllowableError.Value ||
                    Math.Abs(error.Y) > (double)numericUpDownAllowableError.Value)
                {
                    ArmMove(CalArmOffset(error));
                    Thread.Sleep(10);
                }
            }

            pictureBoxMain.Image = frame.Clone().ToBitmap();
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

        private void UpdateInfo(PointF nowPoint, PointF error)
        {
            labelInfu.Text = $"Now: {nowPoint.X},{nowPoint.Y}\r\n" +
                             $"Err: {error.X},{error.Y}";
        }

        #region Button

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Connection().Open();
                _arm.Speed = 10;
                buttonHoming.Enabled = true;
            }

            _camera.Connect();
            buttonStart.Enabled = true;
            checkBoxEnableArm.Enabled = false;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Connection().Close();
            }
            _camera.Disconnect();

            buttonStart.Enabled = false;
            buttonStop.Enabled = false;
            buttonHoming.Enabled = false;
            checkBoxEnableArm.Enabled = true;
        }

        private void buttonHoming_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Motion().Homing();
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Application.Idle += ProcessFrame;
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Application.Idle -= ProcessFrame;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        #endregion Button
    }
}