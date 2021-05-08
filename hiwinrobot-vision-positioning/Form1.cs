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

        private void UpdateInfo(PointF nowPoint, PointF error)
        {
            labelInfu.Text = $"Now: {nowPoint.X},{nowPoint.Y}\r\n" +
                             $"Err: {error.X},{error.Y}";
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

            CvInvoke.Line(frame, new Point(centerOfFrame.X, 0), new Point(centerOfFrame.X, frameSize.Height), new MCvScalar(255, 0, 0));
            CvInvoke.Line(frame, new Point(0, centerOfFrame.Y), new Point(frameSize.Width, centerOfFrame.Y), new MCvScalar(255, 0, 0));
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
            float offsetX;
            if (error.X > 100)
                offsetX = 20;
            else if (error.X > 50)
                offsetX = 10;
            else if (error.X > 10)
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
            if (error.Y > 100)
                offsetY = 20;
            else if (error.Y > 50)
                offsetY = 10;
            else if (error.Y > 10)
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

        #region Button

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (checkBoxEnableArm.Checked)
            {
                _arm.Connect();
                _arm.Speed = 10;
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
            buttonStop.Enabled = false;
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

        #endregion
    }
}