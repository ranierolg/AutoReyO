using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking; //we need to add this name space in order to have many functions  

using Windows.UI.Xaml.Shapes;

using Neosmartpen.Net;
using Neosmartpen.Net.Bluetooth;

using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;

using static SmartPenSketch.SketchRecTools;

using Csv;
using System.Xml;
using static SmartPenSketch.DetailDetector;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartPenSketch
{

    public enum FileType
    {
        BACKGROUND_IMAGE,
        PEN_DATA_WRITE,
        PEN_DATA_READ,
        CSV_RUBINE_WRITE,
        PDOLLAR_WRITE,
        PDOLLAR_READ
    }

    public class SketchPoint
    {
        public double x;
        public double y;
        public int force;
        public long milliseconds;
        private const double N_UNIT_SIZE = 2.37;

        public SketchPoint(double x, double y, int force, long milliseconds)
        {
            this.x = x;
            this.y = y;
            this.force = force;
            this.milliseconds = milliseconds;
        }

        public SketchPoint(SketchPoint sp)
        {
            this.x = sp.x;
            this.y = sp.y;
            this.force = sp.force;
            this.milliseconds = sp.milliseconds;
        }

        public void setSketchPoint(double x, double y, int force, long milliseconds)
        {
            this.x = x;
            this.y = y;
            this.force = force;
            this.milliseconds = milliseconds;
        }

        public long getTime()
        {
            return milliseconds;
        }
    }

    public sealed partial class MainPage : Page
    {
        double A4_WIDTH_MM = 210.0;
        double A4_HEIGHT_MM = 297.0;
        private const double N_UNIT_SIZE = 2.37;

        private PenController pControl;
        private BluetoothPenClient bConnect;
        private List<List<SketchPoint>> strokes;
        private List<List<SketchPoint>> resampledStrokes;

        //this is the new Sketch Data Structure so we don't have to mess with Lists of Lists (NOT CURRENTLY IN USE)
        private List<SketchSubstroke> penSketch;

        //boolean program state flags
        private bool pressed = false;
        private uint mouseId;
        private Point previousContactPt;
        private Point currentContactPt;

        private ObservableCollection<PenInformation> foundDevices = new ObservableCollection<PenInformation>();
        private int pageCount;
        private double savedX;
        private double savedY;
        private bool penDown = false;
        private int[] calibrationData = { 0, 0, 0, 0, 0, 0 };
        private string userID = "USERID";
        private string userBirthDay = "01/01/2000 01:00:00 AM";
        private string userGender = "F";
        private string userEducation = "HS";
        private string userLanguage = "EN";
        private string userDiagnosis = "N/A";
        private string userNotes = "Great Job";
        private double replaySpeed = 1.0;
        private bool searchInit = false;
        private bool allowWait = false;

        //"C:\\Users\\ranie\\source\\repos\\SmartPenUWP\\savedGestures"
        //"C:\\Users\\bgues\\source\\repos\\SmartPenSketch\\savedGestures"
        //"E:\\Github\\SmartPenUWP\\savedGestures"

        //private string default_gesture_txt = "C:\\Users\\bgues\\source\\repos\\SmartPenSketch\\savedGestures";

        private string default_gesture_txt = "C:\\Users\\ranie\\source\\repos\\SmartPenUWP\\savedGestures";
        private string loadedFileName = "";

        #region Detail Tuples
        List<Tuple<int, int>> detail1Path = new List<Tuple<int, int>>();
        private List<Tuple<int, int>> detail2Keys = new List<Tuple<int, int>>();
        List<int> detail3Path_back = new List<int>();
        List<int> detail3Path_forward = new List<int>();
        List<Tuple<int, int>> detail3Path = new List<Tuple<int, int>>();
        List<int> detail4Path = new List<int>();
        List<int> detail5Path = new List<int>();
        List<Tuple<int, int>> detail6Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail7Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail8Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail9Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail10Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail11Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail12Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail13Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail14Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail15Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail16Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail17Path = new List<Tuple<int, int>>();
        List<Tuple<int, int>> detail18Path = new List<Tuple<int, int>>();
        #endregion

        static Detail2Box det2Box = new Detail2Box();

        #region Detail Rubine Data
        List<FeatureDetection.RubineBasic> detail1Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail2Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail3Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail4Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail5Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail6Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail7Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail8Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail9Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail10Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail11Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail12Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail13Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail14Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail15Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail16Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail17Rubine = new List<FeatureDetection.RubineBasic>();
        List<FeatureDetection.RubineBasic> detail18Rubine = new List<FeatureDetection.RubineBasic>();
        #endregion


        //Drawn Lines
        private List<Line> drawnLines = new List<Line>();
        private List<Ellipse> highlightedPoints = new List<Ellipse>();
        private List<Ellipse> drawnPoints = new List<Ellipse>();
        private bool drawGraph = true;

        //=====THESE ARE USED FOR SAVING=====
        private List<Ellipse> savedEllipses = new List<Ellipse>();     //For Drawing
        private List<SketchPoint> savedPointsCircle = new List<SketchPoint>();    //For Debugging
        private List<SketchPoint> savedPointsDiamond = new List<SketchPoint>();
        private List<List<SketchPoint>> gestureToSave = new List<List<SketchPoint>>();
        //=======================================

        //=====GESTURE DATA=====
        List<List<SketchPoint>> det1Template = new List<List<SketchPoint>>();
        Gesture[] trainingDetails;
        //======================

        private int highlightCounter = -1;
        private int[] grade = new int[18];

        private SketchGraph mainGraph;

        public enum Direction { Up, Down, Left, Right, SlantNE, SlantNW, SlantSE, SlantSW };

        // Rubine features
        List<FeatureDetection.RubineBasic> rubineScores;
        List<FeatureDetection.RubineBasic> rubineIdentCurves;
        FeatureDetection.RubineBasic flattenedRubine;

        // GestureGrid constants
        uint GRID_SIZE = 12;
        uint TEMPLATES_PER_DETAIL = 5;
        uint SHIFT_DISTANCE = 6;

        public MainPage()
        {
            pageCount = 0;
            strokes = new List<List<SketchPoint>>();

            this.InitializeComponent();

            this.InitPenVars();

            penCanvas.PointerPressed += new PointerEventHandler(Canvas_PointerPressed);

            this.loadGesture();
        }

        #region Pen Connection
        private void InitPenVars()
        {
            //Initiates all pen variables that deal with connection handling and data transfer
            pControl = new PenController();
            bConnect = new BluetoothPenClient(pControl);

            bConnect.onAddPenController += MClient_onAddPenController;
            bConnect.onRemovePenController += MClient_onRemovePenController;
            bConnect.onStopSearch += MClient_onStopSearch;
            bConnect.onUpdatePenController += MClient_onUpdatePenController;

            pControl.PenStatusReceived += MController_PenStatusReceived;
            pControl.Connected += MController_Connected;
            pControl.Disconnected += MController_Disconnected;
            pControl.Authenticated += MController_Authenticated;
            pControl.DotReceived += MController_DotReceived;
            pControl.PasswordRequested += MController_PasswordRequested;
            pControl.OfflineDataListReceived += MController_OfflineDataListReceived;

            pControl.AutoPowerOffTimeChanged += MController_AutoPowerOffTimeChanged;
            pControl.AutoPowerOnChanged += MController_AutoPowerOnChanged;

            pControl.BatteryAlarmReceived += MController_BatteryAlarmReceived;
            pControl.RtcTimeChanged += MController_RtcTimeChanged;
            pControl.SensitivityChanged += MController_SensitivityChanged;
            pControl.PasswordChanged += MController_PasswordChanged;
            pControl.BeepSoundChanged += MController_BeepSoundChanged;
            pControl.PenColorChanged += MController_PenColorChanged;

            pControl.OfflineDataDownloadStarted += MController_OfflineDataDownloadStarted;
            pControl.OfflineStrokeReceived += MController_OfflineStrokeReceived;
            pControl.OfflineDownloadFinished += MController_OfflineDownloadFinished;

            pControl.FirmwareInstallationStarted += MController_FirmwareInstallationStarted;
            pControl.FirmwareInstallationStatusUpdated += MController_FirmwareInstallationStatusUpdated;
            pControl.FirmwareInstallationFinished += MController_FirmwareInstallationFinished;

            pControl.PenProfileReceived += Mcontroller_PenProfileReceived;

        }


        private async void searchPen(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PenInformation> foundDevices = new ObservableCollection<PenInformation>();
            Debug.WriteLine("Linking Pen");
            if (!searchInit)
            {
                bConnect.StartWatcher();
                searchInit = true;
            }

            List<PenInformation> penList = await bConnect.FindDevices();

        }

        private async void connectPen(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Connecting Pen");
            bool result = false;

            PenInformation penv1 = (PenInformation)DetectedDevices.SelectedItem;

            try
            {
                Debug.WriteLine(penv1.Id);
                Debug.WriteLine(penv1.MacAddress);
                Debug.WriteLine(penv1.Name);
                Debug.WriteLine(penv1.Rssi);
                Debug.WriteLine(penv1.BluetoothAddress);
                {

                    Debug.WriteLine(await bConnect.Pairing(penv1));

                    result = await bConnect.Connect(penv1);

                    if (!result)
                    {
                        Debug.WriteLine("Connection is failure");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("conection exception : " + ex.Message);
                Debug.WriteLine("conection exception : " + ex.StackTrace);

            }

        }
        #endregion

        private void sendToCanvas(Dot dot)
        {
            //Debug.WriteLine(dot.X);
            //Debug.WriteLine(dot.Y);
            //Debug.WriteLine(dot.Timestamp);
            //Debug.WriteLine(dot.Force);           


            double xscalar = this.penCanvas.Width * ((dot.X * N_UNIT_SIZE) / 210.0);
            double yscalar = this.penCanvas.Height * ((dot.Y * N_UNIT_SIZE) / 297.0);



            double force = dot.Force / 500.00;

            byte red = (byte)(force * 255.0);
            //byte blue = (byte)((1 - force) * 255.0);


            if (!penDown)
            {
                this.savedX = xscalar;
                this.savedY = yscalar;
                this.penDown = true;
                this.strokes.Add(new List<SketchPoint>());
            }



            else
            {
                Line l = new Line
                {
                    X1 = this.savedX,
                    Y1 = this.savedY,
                    X2 = xscalar,
                    Y2 = yscalar,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, red, 0, 0))
                };

                penCanvas.Children.Add(l);

                this.savedX = xscalar;
                this.savedY = yscalar;

                this.strokes[strokes.Count - 1].Add(new SketchPoint(dot.X * N_UNIT_SIZE, dot.Y * N_UNIT_SIZE, dot.Force, dot.Timestamp));
            }
        }

        private void newCalibrationData(object sender, TextChangedEventArgs e)
        {
            int val;
            int.TryParse((sender as TextBox).Text, out val);
            switch ((sender as TextBox).Name[7])
            {
                case '0':
                    calibrationData[0] = val;
                    break;
                case '1':
                    calibrationData[1] = val;
                    break;
                case '2':
                    calibrationData[2] = val;
                    break;
                case '3':
                    calibrationData[3] = val;
                    break;
                case '4':
                    calibrationData[4] = val;
                    break;
                case '5':
                    calibrationData[5] = val;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < 6; i++)
            {
                Debug.Write(calibrationData[i]);
                Debug.Write(" ");
            }
            Debug.WriteLine("");

            pControl.SetPressureCalibrateFactor(calibrationData[0], calibrationData[1], calibrationData[2], calibrationData[3], calibrationData[4], calibrationData[5]);
        }

        #region File Read and Write
        private async void writeFile1_0(object sender, RoutedEventArgs e)
        //Writer version 1.0
        {

            System.DateTime time;
            System.DateTime startTime;
            System.DateTime endTime;

            LinkedList<string> lines = new LinkedList<string>();

            Windows.Storage.StorageFile file = await produceFileHandler(FileType.PEN_DATA_WRITE);
            if (file != null && strokes != null)
            {

                Windows.Storage.CachedFileManager.DeferUpdates(file);

                lines.AddLast("1.00");

                lines.AddLast(
                    String.Format(
                        "{0} - {1} - {2} - {3} - {4} - {5}",
                        userID,
                        userBirthDay,
                        userGender,
                        userEducation,
                        userLanguage,
                        userDiagnosis
                   )
                );


                time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                startTime = time.AddMilliseconds(strokes[0][0].milliseconds).ToLocalTime();

                int index1 = strokes.Count - 1;
                int index2 = strokes[index1].Count - 1;

                endTime = time.AddMilliseconds(strokes[index1][index2].milliseconds).ToLocalTime();

                lines.AddLast(String.Format("{0}/{1}/{2} {3}:{4}:{5}:{6}", startTime.Month, startTime.Day, startTime.Year, startTime.Hour, startTime.Minute, startTime.Second, startTime.Millisecond));

                lines.AddLast(String.Format("{0}/{1}/{2} {3}:{4}:{5}:{6}", endTime.Month, endTime.Day, endTime.Year, endTime.Hour, endTime.Minute, endTime.Second, endTime.Millisecond));




                foreach (List<SketchPoint> ls in strokes)
                {

                    foreach (SketchPoint p in ls)
                    {
                        lines.AddLast(String.Format("{0} {1} {2} {3}", p.x, p.y, p.force, p.milliseconds));
                    }
                    lines.AddLast("END STROKE");
                }


                lines.AddLast("===NOTES===");
                lines.AddLast(userNotes);

                await Windows.Storage.FileIO.WriteLinesAsync(file, lines);

            }

        }

        private async void readFile1_0(object sender, RoutedEventArgs e)
        {
            mainGraph = new SketchGraph();
            double width = penCanvas.Width;
            double height = penCanvas.Height;

            //Get data from the file
            Windows.Storage.StorageFile file = await produceFileHandler(FileType.PEN_DATA_READ);
            loadedFileName = file.DisplayName;
            if (file != null)
            {
                IList<string> lines = await Windows.Storage.FileIO.ReadLinesAsync(file);

                strokes = new List<List<SketchPoint>>();
                strokes.Add(new List<SketchPoint>());

                int index = 4;
                string[] tempStrs;
                while (true)
                {
                    if (lines[index].Substring(0, 10) == "END STROKE")
                    {
                        if (lines[index + 1].Substring(0, 11) == "===NOTES===")
                        {
                            break;
                        }
                        else
                        {
                            strokes.Add(new List<SketchPoint>());
                        }
                    }

                    else
                    {
                        tempStrs = lines[index].Split(' ');

                        if (index > 4 && lines[index - 1].Substring(0, 10) != "END STROKE")
                        {
                            string[] tempStrsPrev = lines[index - 1].Split(' ');
                            double prevX = Convert.ToDouble(tempStrsPrev[0]);
                            double prevY = Convert.ToDouble(tempStrsPrev[1]);
                            double currX = Convert.ToDouble(tempStrs[0]);
                            double currY = Convert.ToDouble(tempStrs[1]);

                            if (CalculateDistanceSimple(prevX, prevY, currX, currY) > 20)
                            {
                                index++;
                                continue;
                            }

                        }

                        strokes[strokes.Count - 1].Add(
                            new SketchPoint(
                                Convert.ToDouble(tempStrs[0]),
                                Convert.ToDouble(tempStrs[1]),
                                Convert.ToInt32(tempStrs[2]),
                                Convert.ToInt64(tempStrs[3])
                            )
                        );
                    }

                    index += 1;
                }
            }

            //-------Scaling Code--------
            
            SketchBox preScaleBox = calculateBoundingBox(strokes);

            //double scaleFactor = Math.Min((penCanvas.Width / 1.75) / preScaleBox.boxWidth, (penCanvas.Height / 1.75) / preScaleBox.boxHeight);
            double scaleFactor = Math.Min((penCanvas.Width / 3.5) / preScaleBox.boxWidth, (penCanvas.Height / 3.5) / preScaleBox.boxHeight);
            //double scaleFactor = 1 + preScaleBox.boxWidth / (penCanvas.Width) ;
            foreach (List<SketchPoint> stroke in strokes)
            {
                foreach (SketchPoint pt in stroke)
                {
                    pt.x = pt.x * scaleFactor;
                    pt.y = pt.y * scaleFactor;
                }
            }
            SketchBox skBox = calculateBoundingBox(strokes);

            double canvasCenterX = A4_WIDTH_MM / 2;
            double canvasCenterY = A4_HEIGHT_MM / 2;

            double moveX = canvasCenterX - skBox.centerX;
            double moveY = canvasCenterY - skBox.centerY;

            foreach (List<SketchPoint> stroke in strokes)
            {
                foreach (SketchPoint pt in stroke)
                {
                    pt.x = pt.x + moveX;
                    pt.y = pt.y + moveY;
                }
            }

            //CanvasBox.Width = BackgroundBox.Width / 2;
            //CanvasBox.Height = BackgroundBox.Height / 2;
            
            //--------End Scaling Code--------

            //penCanvas.Width = CanvasBox.Width;
            //penCanvas.Height = CanvasBox.Height;

            //Puts data onto screen
            allowWait = false;
            Task.Factory.StartNew(() => beginReplay());
        }

        private async void autoReadAll(object sender, RoutedEventArgs e)
        {
            string testFolder = "E:\\Skool\\IJCAI20\\SeparatedTests\\DetectedDetail2";
            string outFolder = "E:\\Github\\SmartPenUWP\\results";
            string readText;

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(testFolder);
            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            StorageFolder csvFolder = await StorageFolder.GetFolderFromPathAsync(outFolder);

            foreach (StorageFile sampleFile in fileList)
            {
                mainGraph = new SketchGraph();
                string readFile = sampleFile.Name.Split('.')[0];
                string condition = readFile.Split('_')[1];
                double width = penCanvas.Width;
                double height = penCanvas.Height;
                StorageFile csvFile = await csvFolder.CreateFileAsync(readFile+".csv", CreationCollisionOption.ReplaceExisting);

                //Get data from the file
                //Windows.Storage.StorageFile file = await produceFileHandler(FileType.PEN_DATA_READ);
                if (sampleFile != null)
                {
                    IList<string> lines = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);

                    strokes = new List<List<SketchPoint>>();
                    strokes.Add(new List<SketchPoint>());

                    int index = 4;
                    string[] tempStrs;
                    while (true)
                    {
                        if (lines[index].Substring(0, 10) == "END STROKE")
                        {
                            if (lines[index + 1].Substring(0, 11) == "===NOTES===")
                            {
                                break;
                            }
                            else
                            {
                                strokes.Add(new List<SketchPoint>());
                            }
                        }

                        else
                        {
                            tempStrs = lines[index].Split(' ');

                            if (index > 4 && lines[index - 1].Substring(0, 10) != "END STROKE")
                            {
                                string[] tempStrsPrev = lines[index - 1].Split(' ');
                                double prevX = Convert.ToDouble(tempStrsPrev[0]);
                                double prevY = Convert.ToDouble(tempStrsPrev[1]);
                                double currX = Convert.ToDouble(tempStrs[0]);
                                double currY = Convert.ToDouble(tempStrs[1]);

                                if (CalculateDistanceSimple(prevX, prevY, currX, currY) > 20)
                                {
                                    index++;
                                    continue;
                                }

                            }

                            strokes[strokes.Count - 1].Add(
                                new SketchPoint(
                                    Convert.ToDouble(tempStrs[0]),
                                    Convert.ToDouble(tempStrs[1]),
                                    Convert.ToInt32(tempStrs[2]),
                                    Convert.ToInt64(tempStrs[3])
                                )
                            );
                        }

                        index += 1;
                    }
                }

                //Scale
                SketchBox preScaleBox = calculateBoundingBox(strokes);

                double scaleFactor = Math.Min((penCanvas.Width / 1.75) / preScaleBox.boxWidth, (penCanvas.Height / 1.75) / preScaleBox.boxHeight);

                //double scaleFactor = 1;
                foreach (List<SketchPoint> stroke in strokes)
                {
                    foreach (SketchPoint pt in stroke)
                    {
                        pt.x = pt.x * scaleFactor;
                        pt.y = pt.y * scaleFactor;
                    }
                }
                SketchBox skBox = calculateBoundingBox(strokes);

                double canvasCenterX = A4_WIDTH_MM / 2;
                double canvasCenterY = A4_HEIGHT_MM / 2;

                double moveX = canvasCenterX - skBox.centerX;
                double moveY = canvasCenterY - skBox.centerY;

                foreach (List<SketchPoint> stroke in strokes)
                {
                    foreach (SketchPoint pt in stroke)
                    {
                        pt.x = pt.x + moveX;
                        pt.y = pt.y + moveY;
                    }
                }

                CanvasBox.Width = BackgroundBox.Width / 2;
                CanvasBox.Height = BackgroundBox.Height / 2;

                int[] testGrade = new int[18];

                drawGraph = false;
                buildVertexGraph(sender, e);
                detectDetails(sender, e);

                string[] headers = { "testType", "det1", "det2", "det3", "det4", "det5", "det6", "det7", "det8", "det9", "det10", "det11", "det12", "det13", "det14", "det15", "det16", "det17", "det18" };
                List<List<string>> csvData = new List<List<string>>();
                List<string> dataLine = new List<string>();
                dataLine.Add(condition+"_auto");
                for(int i=0; i<grade.Length; i++)
                    dataLine.Add(grade[i].ToString());
                csvData.Add(dataLine);

                var csv = CsvWriter.WriteToText(
                headers,
                csvData.Select(item => item.ToArray()).ToArray(),
                ',');

                await Windows.Storage.FileIO.WriteTextAsync(csvFile, csv);
            }

            //Puts data onto screen
            //allowWait = false;
            //Task.Factory.StartNew(() => beginReplay());
            drawGraph = true;
        }

        private async void autoRubineCSVAll(object sender, RoutedEventArgs e)
        {
            string testFolder = "E:\\Skool\\IJCAI20\\JoinedTests";
            string outFolder = "E:\\Github\\SmartPenUWP\\rubineCSVs";

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(testFolder);
            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            StorageFolder csvFolder = await StorageFolder.GetFolderFromPathAsync(outFolder);

            foreach (StorageFile sampleFile in fileList)
            {
                mainGraph = new SketchGraph();
                string readFile = sampleFile.Name.Split('.')[0];
                string condition = readFile.Split('_')[1];
                double width = penCanvas.Width;
                double height = penCanvas.Height;
                StorageFile csvFile = await csvFolder.CreateFileAsync(readFile + ".csv", CreationCollisionOption.ReplaceExisting);

                //Get data from the file
                //Windows.Storage.StorageFile file = await produceFileHandler(FileType.PEN_DATA_READ);
                if (sampleFile != null)
                {
                    IList<string> lines = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);

                    strokes = new List<List<SketchPoint>>();
                    strokes.Add(new List<SketchPoint>());

                    int index = 4;
                    string[] tempStrs;
                    while (true)
                    {
                        if (lines[index].Substring(0, 10) == "END STROKE")
                        {
                            if (lines[index + 1].Substring(0, 11) == "===NOTES===")
                            {
                                break;
                            }
                            else
                            {
                                strokes.Add(new List<SketchPoint>());
                            }
                        }

                        else
                        {
                            tempStrs = lines[index].Split(' ');

                            if (index > 4 && lines[index - 1].Substring(0, 10) != "END STROKE")
                            {
                                string[] tempStrsPrev = lines[index - 1].Split(' ');
                                double prevX = Convert.ToDouble(tempStrsPrev[0]);
                                double prevY = Convert.ToDouble(tempStrsPrev[1]);
                                double currX = Convert.ToDouble(tempStrs[0]);
                                double currY = Convert.ToDouble(tempStrs[1]);

                                if (CalculateDistanceSimple(prevX, prevY, currX, currY) > 20)
                                {
                                    index++;
                                    continue;
                                }

                            }

                            strokes[strokes.Count - 1].Add(
                                new SketchPoint(
                                    Convert.ToDouble(tempStrs[0]),
                                    Convert.ToDouble(tempStrs[1]),
                                    Convert.ToInt32(tempStrs[2]),
                                    Convert.ToInt64(tempStrs[3])
                                )
                            );
                        }

                        index += 1;
                    }
                }

                //Scale
                SketchBox preScaleBox = calculateBoundingBox(strokes);

                double scaleFactor = Math.Min((penCanvas.Width / 1.75) / preScaleBox.boxWidth, (penCanvas.Height / 1.75) / preScaleBox.boxHeight);

                //double scaleFactor = 1;
                foreach (List<SketchPoint> stroke in strokes)
                {
                    foreach (SketchPoint pt in stroke)
                    {
                        pt.x = pt.x * scaleFactor;
                        pt.y = pt.y * scaleFactor;
                    }
                }
                SketchBox skBox = calculateBoundingBox(strokes);

                double canvasCenterX = A4_WIDTH_MM / 2;
                double canvasCenterY = A4_HEIGHT_MM / 2;

                double moveX = canvasCenterX - skBox.centerX;
                double moveY = canvasCenterY - skBox.centerY;

                foreach (List<SketchPoint> stroke in strokes)
                {
                    foreach (SketchPoint pt in stroke)
                    {
                        pt.x = pt.x + moveX;
                        pt.y = pt.y + moveY;
                    }
                }

                CanvasBox.Width = BackgroundBox.Width / 2;
                CanvasBox.Height = BackgroundBox.Height / 2;

                int[] testGrade = new int[18];

                drawGraph = false;
                buildVertexGraph(sender, e);

                //Windows.Storage.StorageFile file = await produceFileHandler(FileType.CSV_RUBINE_WRITE);

                string[] names = { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "f10", "f11", "f12", "f13", "curve" };

                List<List<string>> rows = new List<List<string>>(2);
                Debug.WriteLine(this.rubineScores.Count);
                for (int i = 0; i < 1; i++)
                {
                    rows.Add(new List<string>(names.Length));
                    rows[i].Add(this.flattenedRubine.f1.ToString());
                    rows[i].Add(this.flattenedRubine.f2.ToString());
                    rows[i].Add(this.flattenedRubine.f3.ToString());
                    rows[i].Add(this.flattenedRubine.f4.ToString());
                    rows[i].Add(this.flattenedRubine.f5.ToString());
                    rows[i].Add(this.flattenedRubine.f6.ToString());
                    rows[i].Add(this.flattenedRubine.f7.ToString());
                    rows[i].Add(this.flattenedRubine.f8.ToString());
                    rows[i].Add(this.flattenedRubine.f9.ToString());
                    rows[i].Add(this.flattenedRubine.f10.ToString());
                    rows[i].Add(this.flattenedRubine.f11.ToString());
                    rows[i].Add(this.flattenedRubine.f12.ToString());
                    rows[i].Add(this.flattenedRubine.f13.ToString());
                    rows[i].Add("0");
                }

                var csv = CsvWriter.WriteToText(names, rows.Select(item => item.ToArray()).ToArray(), ',');

                await Windows.Storage.FileIO.WriteTextAsync(csvFile, csv);

                //====================================
            }

            //Puts data onto screen
            //allowWait = false;
            //Task.Factory.StartNew(() => beginReplay());
            drawGraph = true;
        }

        private async void saveAsImage(object sender, RoutedEventArgs e)
        {
            string outFolder = "E:\\Skool\\ReyO\\TestImages";
            StorageFolder imgFolder = await StorageFolder.GetFolderFromPathAsync(outFolder);
            StorageFile imgFile = await imgFolder.CreateFileAsync(loadedFileName + ".jpg", CreationCollisionOption.ReplaceExisting);

            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(CanvasBox);

            var pixelBuffer = await bitmap.GetPixelsAsync();

            using (var fileStream = await imgFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixelBuffer.ToArray());

                //encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;

                await encoder.FlushAsync();
            }
        }

        public async Task<Windows.Storage.StorageFile> produceFileHandler(FileType filetype)
        {
            switch (filetype)
            {
                case FileType.BACKGROUND_IMAGE:
                    Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
                    picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                    string[] mimetypes = { ".jpg", ".jpeg", ".png" };
                    foreach (string mimetype in mimetypes)
                    {
                        picker.FileTypeFilter.Add(mimetype);
                    }
                    return await picker.PickSingleFileAsync();

                case FileType.PEN_DATA_READ:
                    Windows.Storage.Pickers.FileOpenPicker readPicker = new Windows.Storage.Pickers.FileOpenPicker();
                    readPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    readPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    readPicker.FileTypeFilter.Add(".txt");

                    return await readPicker.PickSingleFileAsync();

                case FileType.PEN_DATA_WRITE:
                    Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                    savePicker.DefaultFileExtension = ".txt";

                    return await savePicker.PickSaveFileAsync();
                case FileType.CSV_RUBINE_WRITE:
                    Windows.Storage.Pickers.FileSavePicker rubineCSVPicker = new Windows.Storage.Pickers.FileSavePicker();
                    rubineCSVPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    rubineCSVPicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
                    rubineCSVPicker.DefaultFileExtension = ".csv";

                    return await rubineCSVPicker.PickSaveFileAsync();
                case FileType.PDOLLAR_WRITE:
                    Windows.Storage.Pickers.FileSavePicker pdollarSavePicker = new Windows.Storage.Pickers.FileSavePicker();
                    pdollarSavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    pdollarSavePicker.FileTypeChoices.Add("XML File", new List<string>() { ".xml" });
                    pdollarSavePicker.DefaultFileExtension = ".xml";

                    return await pdollarSavePicker.PickSaveFileAsync();

                case FileType.PDOLLAR_READ:
                    Windows.Storage.Pickers.FileOpenPicker pdollarReadPicker = new Windows.Storage.Pickers.FileOpenPicker();
                    pdollarReadPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    pdollarReadPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    pdollarReadPicker.FileTypeFilter.Add(".xml");

                    return await pdollarReadPicker.PickSingleFileAsync();

                default:
                    throw new NotImplementedException();
            }
        }

        private async void outputRubineCurves(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFile file = await produceFileHandler(FileType.CSV_RUBINE_WRITE);

            string[] names = { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "f10", "f11", "f12", "f13", "curve" };

            /*List<List<string>> rows = new List<List<string>>(this.rubineScores.Count);
            Debug.WriteLine(this.rubineScores.Count);
            for (int i = 0; i < this.rubineScores.Count; i++)
            {
                rows.Add(new List<string>(names.Length));
                rows[i].Add(this.rubineScores[i].f1.ToString());
                rows[i].Add(this.rubineScores[i].f2.ToString());
                rows[i].Add(this.rubineScores[i].f3.ToString());
                rows[i].Add(this.rubineScores[i].f4.ToString());
                rows[i].Add(this.rubineScores[i].f5.ToString());
                rows[i].Add(this.rubineScores[i].f6.ToString());
                rows[i].Add(this.rubineScores[i].f7.ToString());
                rows[i].Add(this.rubineScores[i].f8.ToString());
                rows[i].Add(this.rubineScores[i].f9.ToString());
                rows[i].Add(this.rubineScores[i].f10.ToString());
                rows[i].Add(this.rubineScores[i].f11.ToString());
                rows[i].Add(this.rubineScores[i].f12.ToString());
                rows[i].Add(this.rubineScores[i].f13.ToString());
                rows[i].Add("0");
            }

            foreach (FeatureDetection.RubineBasic curve in this.rubineIdentCurves)
            {]
                rows[curve.strokeIndex][13] = "1";
            }

            var csv = CsvWriter.WriteToText(names, rows.Select(item => item.ToArray()).ToArray(), ',');

            await Windows.Storage.FileIO.WriteTextAsync(file, csv);*/

            List<List<string>> rows = new List<List<string>>(2);
            Debug.WriteLine(this.rubineScores.Count);
            for (int i = 0; i < 1; i++)
            {
                rows.Add(new List<string>(names.Length));
                rows[i].Add(this.flattenedRubine.f1.ToString());
                rows[i].Add(this.flattenedRubine.f2.ToString());
                rows[i].Add(this.flattenedRubine.f3.ToString());
                rows[i].Add(this.flattenedRubine.f4.ToString());
                rows[i].Add(this.flattenedRubine.f5.ToString());
                rows[i].Add(this.flattenedRubine.f6.ToString());
                rows[i].Add(this.flattenedRubine.f7.ToString());
                rows[i].Add(this.flattenedRubine.f8.ToString());
                rows[i].Add(this.flattenedRubine.f9.ToString());
                rows[i].Add(this.flattenedRubine.f10.ToString());
                rows[i].Add(this.flattenedRubine.f11.ToString());
                rows[i].Add(this.flattenedRubine.f12.ToString());
                rows[i].Add(this.flattenedRubine.f13.ToString());
                rows[i].Add("0");
            }

            //foreach (FeatureDetection.RubineBasic curve in this.rubineIdentCurves)
            //{
            //    rows[curve.strokeIndex][13] = "1";
            //}

            var csv = CsvWriter.WriteToText(names, rows.Select(item => item.ToArray()).ToArray(), ',');

            await Windows.Storage.FileIO.WriteTextAsync(file, csv);
        }


        private async void setBackgroundImage(object sender, RoutedEventArgs e)
        // help from https://stackoverflow.com/questions/22713569/received-system-runtime-interopservices-comexception-in-windows-app
        {
            Windows.Storage.StorageFile file = await produceFileHandler(FileType.BACKGROUND_IMAGE);
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage img = new BitmapImage();
                    ImageBrush imgbrush = new ImageBrush();
                    await img.SetSourceAsync(fileStream);
                    imgbrush.ImageSource = img;
                    penCanvas.Background = imgbrush;
                }
            }
            return;
        }

        private async void writeGesture(object sender, RoutedEventArgs e)
        {

            Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();

            String btnName = ((Button)sender).Name;

            string gestureFolder = default_gesture_txt; //"C:\\Users\\ranie\\source\\repos\\SmartPenUWP\\savedGestures";
            string fileName = btnName + ".txt";

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(gestureFolder);
            StorageFile sampleFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            //await FileIO.WriteTextAsync(sampleFile, "Swift as a shadow");
            RemoveSkAdjDupes(graph_nodes);

            System.DateTime time;
            System.DateTime startTime;
            System.DateTime endTime;
            
            List<List<SketchPoint>> gestureToWrite = new List<List<SketchPoint>>();
            switch (btnName)
            { 
                //parameters: detail path, GesturetoWrite, graph_nodes
                case "det1":
                    gestureToWrite = ExtractSamples(detail1Path);
                    break;
                case "det2":
                    gestureToWrite = ExtractSamples(detail2Keys);
                    break;
                case "det3":
                    gestureToWrite = ExtractSamples(detail3Path);
                    break;
                case "det4":
                    gestureToWrite = ExtractSamples(Crawler.TupleListConstructor(mainGraph, detail4Path));
                    break;
                case "det5":
                    gestureToWrite = ExtractSamples(Crawler.TupleListConstructor(mainGraph, detail5Path));
                    break;
                case "det6":
                    gestureToWrite = ExtractSamples(detail6Path);
                    break;
                case "det7":
                    gestureToWrite = ExtractSamples(detail7Path);
                    break;
                case "det8":
                    gestureToWrite = ExtractSamples(detail8Path);
                    break;
                case "det9":
                    gestureToWrite = ExtractSamples(detail9Path);
                    break;
                case "det10":
                    gestureToWrite = ExtractSamples(detail10Path);
                    break;
                case "det11":
                    List<List<SketchPoint>> tempSkPtC = new List<List<SketchPoint>>();
                    tempSkPtC.Add(savedPointsCircle);
                    gestureToWrite = tempSkPtC;
                    //gestureToWrite = ExtractSamples(detail11Path);
                    break;
                case "det12":
                    gestureToWrite = ExtractSamples(detail12Path);
                    break;
                case "det13":
                    gestureToWrite = ExtractSamples(detail13Path);
                    break;
                case "det14":
                    List<List<SketchPoint>> tempSkPtD = new List<List<SketchPoint>>();
                    tempSkPtD.Add(savedPointsDiamond);
                    gestureToWrite = tempSkPtD;
                    //gestureToWrite = ExtractSamples(detail14Path);
                    break;
                case "det15":
                    gestureToWrite = ExtractSamples(detail15Path);
                    break;
                case "det16":
                    gestureToWrite = ExtractSamples(detail16Path);
                    break;
                case "det17":
                    gestureToWrite = ExtractSamples(detail17Path);
                    break;
                case "det18":
                    gestureToWrite = ExtractSamples(detail18Path);
                    break;
            }

            LinkedList<string> lines = new LinkedList<string>();
            if (gestureToWrite != null)
            {
                lines.AddLast("1.00");

                lines.AddLast(
                    String.Format(
                        "{0} - {1} - {2} - {3} - {4} - {5}",
                        userID,
                        userBirthDay,
                        userGender,
                        userEducation,
                        userLanguage,
                        userDiagnosis
                   )
                );


                time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                startTime = time.AddMilliseconds(strokes[0][0].milliseconds).ToLocalTime();

                for(int i=0; i<gestureToWrite.Count; i++)
                {
                    if(gestureToWrite[i].Count == 0)
                    {
                        gestureToWrite.RemoveAt(i);
                        i--;
                    }
                }

                int index1 = gestureToWrite.Count - 1;
                int index2 = gestureToWrite[index1].Count - 1;

                endTime = time.AddMilliseconds(gestureToWrite[index1][index2].milliseconds).ToLocalTime();

                lines.AddLast(String.Format("{0}/{1}/{2} {3}:{4}:{5}:{6}", startTime.Month, startTime.Day, startTime.Year, startTime.Hour, startTime.Minute, startTime.Second, startTime.Millisecond));

                lines.AddLast(String.Format("{0}/{1}/{2} {3}:{4}:{5}:{6}", endTime.Month, endTime.Day, endTime.Year, endTime.Hour, endTime.Minute, endTime.Second, endTime.Millisecond));

                foreach (List<SketchPoint> ls in gestureToWrite)
                {

                    foreach (SketchPoint p in ls)
                    {
                        lines.AddLast(String.Format("{0} {1} {2} {3}", p.x, p.y, p.force, p.milliseconds));
                    }
                    lines.AddLast("END STROKE");
                }

                lines.AddLast("===NOTES===");
                lines.AddLast(userNotes);

                var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        foreach (string str in lines)
                        {
                            dataWriter.WriteString(str + "\n");
                        }
                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }
                stream.Dispose();

            }
        }
        private async void loadGesture()
        {
            List<Gesture> readGestures = new List<Gesture>(); 
            string gestureFolder = default_gesture_txt; //"C:\\Users\\ranie\\source\\repos\\SmartPenUWP\\savedGestures";
            string fileName = "gesture_test.txt";
            string readText;
            
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(gestureFolder);
            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            //StorageFile sampleFile = await folder.GetFileAsync(fileName);
            foreach (StorageFile sampleFile in fileList)
            {
                var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                fileName = sampleFile.Name;
                string[] splitName = fileName.Split('_');
                string detName = splitName[0];

                ulong size = stream.Size;

                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    using (var dataReader = new Windows.Storage.Streams.DataReader(inputStream))
                    {
                        uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
                        readText = dataReader.ReadString(numBytesLoaded);
                    }
                }

                string[] readSplit = readText.Split('\n');

                List<List<SketchPoint>> readGesture = new List<List<SketchPoint>>();
                List<List<SketchPoint>> tempGesture = new List<List<SketchPoint>>();
                readGesture.Add(new List<SketchPoint>());

                int readInd = 4;
                string[] tempStrs;
                while (true)
                {
                    if (readSplit[readInd].Substring(0, 10) == "END STROKE")
                    {
                        if (readSplit[readInd + 1].Substring(0, 4) == "===N")
                        {
                            break;
                        }
                        else
                        {
                            readGesture.Add(new List<SketchPoint>());
                        }
                    }

                    else
                    {
                        tempStrs = readSplit[readInd].Split(' ');

                        if (readInd > 4 && readSplit[readInd - 1].Substring(0, 10) != "END STROKE")
                        {
                            string[] tempStrsPrev = readSplit[readInd - 1].Split(' ');
                            double prevX = Convert.ToDouble(tempStrsPrev[0]);
                            double prevY = Convert.ToDouble(tempStrsPrev[1]);
                            double currX = Convert.ToDouble(tempStrs[0]);
                            double currY = Convert.ToDouble(tempStrs[1]);

                            if (CalculateDistanceSimple(prevX, prevY, currX, currY) > 20)
                            {
                                readInd++;
                                continue;
                            }

                        }

                        readGesture[readGesture.Count - 1].Add(
                            new SketchPoint(
                                Convert.ToDouble(tempStrs[0]),
                                Convert.ToDouble(tempStrs[1]),
                                Convert.ToInt32(tempStrs[2]),
                                Convert.ToInt64(tempStrs[3])
                            )
                        );
                    }

                    readInd += 1;
                }

                det1Template.AddRange(readGesture);
                readGestures.Add(GestureIO.SkPtGestureConverter(readGesture, detName));
            }

            trainingDetails = readGestures.ToArray();
            //convertGesturesCSV();
        }
        private async void convertGesturesCSV()
        {
            //Converts all saved gestures into a CSV
            //THIS ONLY NEEDS TO BE RUN ONCE, EVER. SHOULD NOT BE USED
            string gestureFolder = default_gesture_txt; //"C:\\Users\\ranie\\source\\repos\\SmartPenUWP\\savedGestures";
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(gestureFolder);

            for(int i = 0; i<trainingDetails.Count(); i++)
            {
                string fileName = trainingDetails[i].Name + ".csv";
                StorageFile sampleFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                string[] names = { "X", "Y"};
                List<List<string>> data = new List<List<string>>();
                for (int j = 0; j < trainingDetails[i].Points.Count(); j++)
                {
                    //string row = trainingDetails[i].Points[j].X + "," + trainingDetails[i].Points[j].Y;
                    data[0].Add(trainingDetails[i].Points[j].X.ToString());
                    data[1].Add(trainingDetails[i].Points[j].Y.ToString());
                }

                var csv = CsvWriter.WriteToText(
                names,
                data.Select(item => item.ToArray()).ToArray(),
                ',');

                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, csv);
            }
        }
        #endregion

        #region Replay
        private void beginReplayThread(object sender, RoutedEventArgs e)
        {
            //Begins the update feature without halting the main
            allowWait = true;
            Task.Factory.StartNew(() => beginReplay());
            return;
        }

        private async void beginReplay()
        {
            //allowDrawing = false;
            //Starts a video thread

            //
            IEnumerator<List<SketchPoint>> firstLevelIt;
            IEnumerable<SketchPoint> secondLevelIt;


            //Adjusted height for each direction stored in the video file
            double width = 0;
            double height = 0;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => penCanvas.Children.Clear()
            );

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => width = penCanvas.Width
            );

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => height = penCanvas.Height
            );

            if (strokes.Count == 0)
                return;

            double start = strokes[0][0].milliseconds;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //Starts the actual replay
            for (int i = 0; i < strokes.Count; i++)
            {
                for (int j = 1; j < strokes[i].Count; j++)
                {
                    if (allowWait) while (timer.ElapsedMilliseconds * replaySpeed + start < strokes[i][j].milliseconds) { }

                    replayUIUpdateWrapper(
                        width * strokes[i][j - 1].x / 210.0,
                        height * strokes[i][j - 1].y / 297.0,
                        width * strokes[i][j].x / 210.0,
                        height * strokes[i][j].y / 297.0
                    );
                }
            }

            //allowDrawing = true;
            return;
        }

        private async void replayUIUpdateWrapper(double x1, double y1, double x2, double y2)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => penCanvas.Children.Add(
                    new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        StrokeThickness = 2.0,
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                    }
                )
            );
        }


        private void setAllowWaitFalse(object sender, RoutedEventArgs e) {
            allowWait = false;
        }

        private void changePlaybackSpeed(object sender, RoutedEventArgs e)
        {
            replaySpeed = replaySlider.Value;
        }
        #endregion

        #region Sketch Rec Tests
        //Calculate global bound box
        private void boundBoxTest(object sender, RoutedEventArgs e)
        {
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
            SketchBox skBox = calculateBoundingBox(convertedStrokes);
            displayRectangle(skBox);
        }

        //Calculate point resampling
        private void resampleSketch(object sender, RoutedEventArgs e)
        {
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
            List<List<SketchPoint>> resampledSketch = resampleByDistanceAuto(convertedStrokes);
            //List<List<SketchPoint>> resampledSketch = resampleByDistance(convertedStrokes, 3);
            displayPoints(resampledSketch, Windows.UI.Colors.SteelBlue);
        }

        private void findCorners(object sender, RoutedEventArgs e)
        {
            //List<int> corners = new List<int>();
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
            List<List<SketchPoint>> resampledSketch = resampleByDistanceAuto(convertedStrokes);
            //List<List<SketchPoint>> resampledSketch = resampleByDistance(convertedStrokes, 3);

            List<List<int>> sketchCornerIndices = ShortStraw.RunShortStraw(resampledSketch);


            List<SketchPoint> corners = new List<SketchPoint>();
            //for (int i = 0; i < resampledSketch.strokes.length; i++)
            int ind = 0;
            foreach (List<SketchPoint> points in resampledSketch)
            {
                List<SketchPoint> resampledPoints = points;
                List<int> strokeCornerIndices = sketchCornerIndices[ind];
                for (int j = 0; j < strokeCornerIndices.Count; j++)
                {
                    corners.Add(resampledPoints[strokeCornerIndices[j]]);
                }
                ind++;
            }

            displayPoints(corners, Windows.UI.Colors.Red);
            //foreach (int i in corners)
            //cornerPoints.Add(resampledSketch[i]);

        }

        private double calculateDistance(SketchPoint skPt0, SketchPoint skPt1)
        {
            return Math.Sqrt((skPt1.x - skPt0.x) * (skPt1.x - skPt0.x) + (skPt1.y - skPt0.y) * (skPt1.y - skPt0.y));
        }


        private void buildVertexGraph(object sender, RoutedEventArgs e)
        {
            //ScaleAndCenter();
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
            List<List<SketchPoint>> resampledSketch = SketchRecTools.resampleByDistanceAuto(convertedStrokes);
            resampledStrokes = new List<List<SketchPoint>>(resampledSketch);

            //List<List<int>> sketchCornerIndices = IStraw.RunIStraw(resampledSketch);

            // Rubine stuff
            List<FeatureDetection.RubineBasic> features = FeatureDetection.rubineFStrokes(resampledSketch);
            FeatureDetection.RubineBasic flattenedFeatures = FeatureDetection.singleRubineStroke(resampledSketch);
            List<FeatureDetection.RubineBasic> labelledCurves = FeatureDetection.labelTrueCurves(resampledSketch, 5);

            /*for (int i = 0; i < labelledCurves.Count; i++)
                displayPoints(
                    labelledCurves[i].stroke,
                    Windows.UI.Color.FromArgb(255, 0, 0, 255));*/

            //Temporairly disabling removing of curves
            //foreach (FeatureDetection.RubineBasic curve in labelledCurves)
                //resampledSketch[curve.strokeIndex] = null;
            //resampledSketch.RemoveAll((stroke) => stroke == null);

            this.rubineScores = features;
            this.rubineIdentCurves = labelledCurves;
            this.flattenedRubine = flattenedFeatures;

            // Start straws and vertex detection
            List<List<int>> sketchCornerIndices = ShortStraw.RunShortStraw(resampledSketch);
            List<List<SketchSubstroke>> allSubstrokes = resampledSketch
                .Zip(sketchCornerIndices, (List<SketchPoint> points, List<int> indices) =>
                {
                    return SketchSubstroke.createSubstrokesFromIndices(points, indices);
                })
                .ToList();


            //Flatten the list of substrokes by corner
            List<SketchSubstroke> sub_strokes = new List<SketchSubstroke>();
            foreach (List<SketchSubstroke> ss in allSubstrokes)
                foreach (SketchSubstroke s in ss)
                {
                    sub_strokes.Add(s);
                }

            sub_strokes = SegmentSplit.splitByIntersection(sub_strokes);

            SketchGraph graph = SegmentSplit.convertToGraph(sub_strokes, 0);

            graph = VertexCoalesce.clusterCoalesce(graph, 18.0);

            graph = VertexCoalesce.linkNearPoints(graph, 10.0, VertexCoalesce.DistanceType.SINGLE);

            //Here we'll need a hack-y solution to restructure the dictionary to match the vertices with their indeces

            QuickDrawVertexGraph(graph);

            mainGraph = graph;
        }

        private void QuickDrawVertexGraph(SketchGraph graph)
        {
            //Draw the shapes sampled (Note that since the sampled strokes are already in pixel values there is no need to convert x values)
            Dictionary<int, SketchAdjacencyList> graph_nodes = graph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = graph.getSubstrokes();
            foreach (Tuple<int, int> key in graph.getSubstrokes().Keys)
            {
                //Get the necessary points
                double x1 = graph_nodes[key.Item1].node.avgx; //penCanvas.Width * (graph_nodes[key.Item1].node.avgx / A4_WIDTH_MM);
                double x2 = graph_nodes[key.Item2].node.avgx; //penCanvas.Width * (graph_nodes[key.Item2].node.avgx / A4_WIDTH_MM);
                double y1 = graph_nodes[key.Item1].node.avgy; //penCanvas.Height * (graph_nodes[key.Item1].node.avgy / A4_HEIGHT_MM);
                double y2 = graph_nodes[key.Item2].node.avgy; //penCanvas.Height * (graph_nodes[key.Item2].node.avgy / A4_HEIGHT_MM);

                //Add the line
                Line l = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
                };
                if(drawGraph)
                    penCanvas.Children.Add(l);


                //Add the Endpoints
                Ellipse ellipse1 = new Ellipse();
                Ellipse ellipse2 = new Ellipse();
                TextBlock ellipse1Text = new TextBlock();
                TextBlock ellipse2Text = new TextBlock();
                ellipse1.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                ellipse2.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                ellipse1.Width = 10;
                ellipse1.Height = 10;
                ellipse2.Width = 10;
                ellipse2.Height = 10;
                ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse1.VerticalAlignment = VerticalAlignment.Top;
                ellipse2.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse2.VerticalAlignment = VerticalAlignment.Top;
                ellipse1.Margin = new Thickness(x1 - ellipse1.Width / 2, y1 - ellipse1.Height / 2, 0, 0);
                ellipse2.Margin = new Thickness(x2 - ellipse1.Width / 2, y2 - ellipse1.Height / 2, 0, 0);

                ellipse1.Opacity = 0.5;
                ellipse2.Opacity = 0.5;

                ellipse1Text.Text = key.Item1.ToString();
                ellipse1Text.HorizontalAlignment = ellipse1.HorizontalAlignment;
                ellipse1Text.VerticalAlignment = ellipse1.VerticalAlignment;
                ellipse1Text.Margin = ellipse1.Margin;
                //ellipse1Text.Margin = new Thickness(ellipse1.Margin.Left+20, ellipse1.Margin.Top + 20, 0, 0);
                ellipse1Text.FontSize = 24;
                ellipse1Text.Opacity = 0.5;
                //ellipse1Text.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);

                ellipse2Text.Text = key.Item2.ToString();
                ellipse2Text.HorizontalAlignment = ellipse2.HorizontalAlignment;
                ellipse2Text.VerticalAlignment = ellipse2.VerticalAlignment;
                ellipse2Text.Margin = ellipse2.Margin;
                //ellipse2Text.Margin = new Thickness(ellipse2.Margin.Left + 20, ellipse2.Margin.Top + 20, 0, 0);
                ellipse2Text.FontSize = 24;
                ellipse2Text.Opacity = 0.5;
                //ellipse2Text.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);

                if (drawGraph)
                {
                    penCanvas.Children.Add(ellipse1);
                    penCanvas.Children.Add(ellipse2);
                    penCanvas.Children.Add(ellipse1Text);
                    penCanvas.Children.Add(ellipse2Text);
                }
            }

            /*double dist;
            dist = CalculateDistanceSimple(graph_nodes[49].node.avgx, graph_nodes[49].node.avgy, graph_nodes[4].node.avgx, graph_nodes[4].node.avgy);
            Debug.WriteLine("DISTANCE:::: " + dist.ToString());*/

            //distance to add might want to be 15-20
        }

        private async Task SlowDrawVertexGraph(SketchGraph graph)
        {
            //Draw the shapes sampled (Note that since the sampled strokes are already in pixel values there is no need to convert x values)
            Dictionary<int, SketchAdjacencyList> graph_nodes = graph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = graph.getSubstrokes();

            //We need a link between what we are currently drawing/have already drawn and the actual graph node data. Currently there is nothing that links them,
            //so debugging and changing line colors through whatever graph-crawling algorithms we write would be impossible.


            List<Line> graphLines = new List<Line>();
            List<Ellipse> graphEllipses = new List<Ellipse>();
            //Tuple<int, Line> tupleLines;
            //Tuple<int, int, int> tupleSubstrokes;
            foreach (Tuple<int, int> key in graph.getSubstrokes().Keys)
            {
                //It may be easier to just draw a new line or a new node as we crawl through these, on top of an existing one. 

                //Get the necessary points
                double x1 = graph_nodes[key.Item1].node.avgx; //penCanvas.Width * (graph_nodes[key.Item1].node.avgx / A4_WIDTH_MM);
                double x2 = graph_nodes[key.Item2].node.avgx; //penCanvas.Width * (graph_nodes[key.Item2].node.avgx / A4_WIDTH_MM);
                double y1 = graph_nodes[key.Item1].node.avgy; //penCanvas.Height * (graph_nodes[key.Item1].node.avgy / A4_HEIGHT_MM);
                double y2 = graph_nodes[key.Item2].node.avgy; //penCanvas.Height * (graph_nodes[key.Item2].node.avgy / A4_HEIGHT_MM);

                //Add the line
                Line l = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
                };
                penCanvas.Children.Add(l);
                graphLines.Add(l);
                //tupleLines.Item1 = 
                await Task.Delay(500);

                //Add the Endpoints
                Ellipse ellipse1 = new Ellipse();
                Ellipse ellipse2 = new Ellipse();
                TextBlock ellipse1Text = new TextBlock();
                TextBlock ellipse2Text = new TextBlock();
                ellipse1.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                ellipse2.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                ellipse1.Width = 10;
                ellipse1.Height = 10;
                ellipse2.Width = 10;
                ellipse2.Height = 10;
                ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse1.VerticalAlignment = VerticalAlignment.Top;
                ellipse2.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse2.VerticalAlignment = VerticalAlignment.Top;
                ellipse1.Margin = new Thickness(x1 - ellipse1.Width / 2, y1 - ellipse1.Height / 2, 0, 0);
                ellipse2.Margin = new Thickness(x2 - ellipse1.Width / 2, y2 - ellipse1.Height / 2, 0, 0);

                ellipse1Text.Text = key.Item1.ToString();
                ellipse1Text.HorizontalAlignment = ellipse1.HorizontalAlignment;
                ellipse1Text.VerticalAlignment = ellipse1.VerticalAlignment;
                ellipse1Text.Margin = ellipse1.Margin;

                ellipse2Text.Text = key.Item2.ToString();
                ellipse2Text.HorizontalAlignment = ellipse2.HorizontalAlignment;
                ellipse2Text.VerticalAlignment = ellipse2.VerticalAlignment;
                ellipse2Text.Margin = ellipse2.Margin;

                penCanvas.Children.Add(ellipse1);
                //Possible linkage data structure: Tuple<graphNodes[Item], ellipse>
                await Task.Delay(500);
                penCanvas.Children.Add(ellipse2);
                await Task.Delay(500);
                penCanvas.Children.Add(ellipse1Text);
                await Task.Delay(500);
                penCanvas.Children.Add(ellipse2Text);
                await Task.Delay(500);
                graphLines[graphLines.Count - 1].Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));

                //graph_nodes[key.Item1].node.adj
                //graph.addNodeLink(new Tuple<int, SketchAdjacencyList, ellipse1>);
                //graph.addLineLink(new Tuple<Tuple<int, int>, SketchSubstroke, Line>)
            }
        }

        private void ScaleAndCenter()
        {
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);

            foreach (List<SketchPoint> stroke in convertedStrokes)
            {
                foreach (SketchPoint pt in stroke)
                {
                    pt.x = pt.x * 2;
                    pt.y = pt.y * 2;
                }
            }

            strokes.Clear();
            strokes.AddRange(convertedStrokes);
            allowWait = false;
            //Task.Factory.StartNew(() => beginReplay());
        }
        private List<List<SketchPoint>> ExtractSamples(List<Tuple<int, int>> detPath)
        {
            List<List<SketchPoint>> gestureToWrite = new List<List<SketchPoint>>();

            Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();

            foreach (Tuple<int, int> detTup in detPath)
                gestureToWrite.Add(graph_temp[detTup].get_points());

            return gestureToWrite;
        }
        #endregion

        #region Detail Detection
        private void detectDetails(object sender, RoutedEventArgs e)
        {
            Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();
            //int[] grade = new int[18];
            Array.Clear(grade, 0, grade.Length);
            float distance;

            RemoveSkAdjDupes(graph_nodes);
            clearAllPaths();
            DetailDetector.initDetailData();

            //DETAIL 2
            detail2Keys = DetailDetector.Detail2Alt_2(graph_nodes, mainGraph, out det2Box);
            validateDetail(detail2Keys, "det2");
            if (detail2Keys.Count > 0)
                grade[1] = 2;
            if (detail2Keys.Count == 0)
                return;

            //DETAIL 1
            detail1Path = DetailDetector.Detail1(graph_nodes, mainGraph);
            distance = validateDetail(detail1Path, "det1");
            if (distance < 0.6)
                grade[0] = 2;
            else if (distance < 0.8)
                grade[0] = 1;
            else
                grade[0] = 0;

            //DETAIL 3
            //Diagonal (one of them)
            detail3Path_back = DetailDetector.Detail3_Back(graph_nodes, graph_temp, mainGraph);

            //Diagonal (the other)
            detail3Path_forward = DetailDetector.Detail3_Forward(graph_nodes, graph_temp, mainGraph);

            //Combining both
            List<int> det3Combined = new List<int>(detail3Path_back);
            det3Combined.AddRange(detail3Path_forward);
            detail3Path.AddRange(Crawler.TupleListConstructor(mainGraph, det3Combined));
            distance = validateDetail(detail3Path, "det3");
            if (distance <= 0.55)
                grade[2] = 2;
            else if (distance < 9)
                grade[2] = 1;
            else
                grade[2] = 0;

            //DETAIL 4
            detail4Path = DetailDetector.Detail4_Alt(graph_nodes, graph_temp, mainGraph);
            distance = validateDetailPDollar(Crawler.TupleListConstructor(mainGraph, detail4Path), "det4");
            if (distance < 6)
                grade[3] = 2;
            else if (distance < 9)
                grade[3] = 1;
            else
                grade[3] = 0;

            //DETAIL 5
            detail5Path = DetailDetector.Detail5_Alt(graph_nodes, graph_temp, mainGraph);
            distance = validateDetailPDollar(Crawler.TupleListConstructor(mainGraph, detail5Path), "det5");
            if (distance < 6)
                grade[4] = 2;
            else if (distance < 9)
                grade[4] = 1;
            else
                grade[4] = 0;

            //DETAIL 7
            detail7Path = DetailDetector.Detail7(graph_nodes, graph_temp, mainGraph, detail6Path);
            distance = validateDetailPDollar(detail7Path, "det7");
            if (distance < 6)
                grade[6] = 2;
            else if (distance < 9)
                grade[6] = 1;
            else
                grade[6] = 0;

            //DETAIL 8
            detail8Path = DetailDetector.Detail8(graph_nodes, graph_temp, mainGraph);
            distance = validateDetail(detail8Path, "det8");
            if (distance < 0.6)
                grade[7] = 2;
            else if (distance < 0.9)
                grade[7] = 1;
            else
                grade[7] = 0;

            //DETAIL 9
            detail9Path = DetailDetector.Detail9(graph_nodes, mainGraph);
            distance = validateDetail(detail9Path, "det9");
            if (distance < 0.4)
                grade[8] = 2;
            else if (distance < 0.5)
                grade[8] = 1;
            else
                grade[8] = 0;

            //DETAIL 10
            detail10Path = DetailDetector.Detail10(graph_nodes, mainGraph);
            distance = validateDetailPDollar(detail10Path, "det10");
            if (distance < 6)
                grade[9] = 2;
            else if (distance < 9)
                grade[9] = 1;
            else
                grade[9] = 0;

            //DETAIL 11
            detail11Path = DetailDetector.Detail11(graph_nodes, mainGraph);
            distance = validateDetail(detail11Path, "det11");
            if (distance < 0.75)
                grade[10] = 2;
            else if (distance < 0.9)
                grade[10] = 1;
            else
                grade[10] = 0;

            //DETAIL 12
            detail12Path = DetailDetector.Detail12Alt_2(graph_nodes, mainGraph);
            distance = validateDetail(detail12Path, "det12");
            if (distance < 0.6)
                grade[11] = 2;
            else if (distance < 0.85)
                grade[11] = 1;
            else
                grade[11] = 0;

            //DETAIL 13
            detail13Path = DetailDetector.Detail13(graph_nodes, mainGraph);
            distance = validateDetail(detail13Path, "det13");
            if (distance < .7)
                grade[12] = 2;
            else if (distance < 0.85)
                grade[12] = 1;
            else
                grade[12] = 0;

            //DETAIL 15
            detail15Path = DetailDetector.Detail15(graph_nodes, mainGraph);
            distance = validateDetailPDollar(detail15Path, "det15");
            if (distance < 6)
                grade[14] = 2;
            else if (distance < 9)
                grade[14] = 1;
            else
                grade[14] = 0;

            //DETAIL 16
            detail16Path = DetailDetector.Detail16(graph_nodes, mainGraph);
            distance = validateDetailPDollar(detail16Path, "det16");
            if (distance < 6)
                grade[15] = 2;
            else if (distance < 9)
                grade[15] = 1;
            else
                grade[15] = 0;

            //DETAIL 14
            detail14Path = DetailDetector.Detail14(graph_nodes, mainGraph);
            distance = validateDetail(detail14Path, "det14");
            if (distance < 0.75)
                grade[13] = 2;
            else if (distance < 0.9)
                grade[13] = 1;
            else
                grade[13] = 0;

            //DETAIL 18
            detail18Path = DetailDetector.Detail18(graph_nodes, mainGraph);
            distance = validateDetail(detail18Path, "det18");
            if (distance < .5)
                grade[17] = 2;
            else if (distance < 0.9)
                grade[17] = 1;
            else
                grade[17] = 0;

            //DETAIL 17
            detail17Path = DetailDetector.Detail17(graph_nodes, mainGraph);
            distance = validateDetail(detail17Path, "det17");
            if (distance < 0.7)
                grade[16] = 2;
            else if (distance < 0.9)
                grade[16] = 1;
            else
                grade[16] = 0;

            //DETAIL 6
            detail6Path = DetailDetector.Detail6Alt(graph_nodes, mainGraph);
            distance = validateDetail(detail6Path, "det6");
            if (distance < 0.6)
                grade[5] = 2;
            else if (distance < 0.95)
                grade[5] = 1;
            else
                grade[5] = 0;

            //DrawSketchPath(detail2Keys, Colors.Blue);

            disableAllButtons();
            //Enable the buttons
            if (detail1Path.Count>0)
                det1.IsEnabled = true;
            if (detail2Keys.Count > 0)
                det2.IsEnabled = true;
            if (detail3Path.Count > 0)
                det3.IsEnabled = true;
            if (detail4Path.Count > 0)
                det4.IsEnabled = true;
            if (detail5Path.Count > 0)
                det5.IsEnabled = true;
            if (detail6Path.Count > 0)
                det6.IsEnabled = true;
            if (detail7Path.Count > 0)
                det7.IsEnabled = true;
            if (detail8Path.Count > 0)
                det8.IsEnabled = true;
            if (detail9Path.Count > 0)
                det9.IsEnabled = true;
            if (detail10Path.Count > 0)
                det10.IsEnabled = true;
            if (detail11Path.Count > 0)
                det11.IsEnabled = true;
            if (detail12Path.Count > 0)
                det12.IsEnabled = true;
            if (detail13Path.Count > 0)
                det13.IsEnabled = true;
            if (detail14Path.Count > 0)
                det14.IsEnabled = true;
            if (detail15Path.Count > 0)
                det15.IsEnabled = true;
            if (detail16Path.Count > 0)
                det16.IsEnabled = true;
            if (detail17Path.Count > 0)
                det17.IsEnabled = true;
            if (detail18Path.Count > 0)
                det18.IsEnabled = true;
        }

        private void clearAllPaths()
        {
            detail1Path.Clear();
            detail2Keys.Clear();
            detail3Path_back.Clear();
            detail3Path_forward.Clear();
            detail3Path.Clear();
            detail4Path.Clear();
            detail5Path.Clear();
            detail6Path.Clear();
            detail7Path.Clear();
            detail8Path.Clear();
            detail9Path.Clear();
            detail10Path.Clear();
            detail11Path.Clear();
            detail12Path.Clear();
            detail13Path.Clear();
            detail14Path.Clear();
            detail15Path.Clear();
            detail16Path.Clear();
            detail17Path.Clear();
            detail18Path.Clear();
        }
        private void disableAllButtons()
        {
            det1.IsEnabled = false;
            det2.IsEnabled = false;
            det3.IsEnabled = false;
            det4.IsEnabled = false;
            det5.IsEnabled = false;
            det6.IsEnabled = false;
            det7.IsEnabled = false;
            det8.IsEnabled = false;
            det9.IsEnabled = false;
            det10.IsEnabled = false;
            det11.IsEnabled = false;
            det12.IsEnabled = false;
            det13.IsEnabled = false;
            det14.IsEnabled = false;
            det15.IsEnabled = false;
            det16.IsEnabled = false;
            det17.IsEnabled = false;
            det18.IsEnabled = false;
        }

        private float validateDetail(List<Tuple<int, int>> detailInput, string detName)
        {
            // OLD CODE BEFORE REALLY BAD MERGE
            /*
            List<List<SketchPoint>> detSkPt = new List<List<SketchPoint>>();
            foreach (Tuple<int, int> detTup in detailInput)
                detSkPt.Add(graph_temp[detTup].get_points());
            Gesture gesture = GestureIO.SkPtGestureConverter(detSkPt, detName);
            PointCloudRecognizerPlus.Classify(gesture, trainingDetails);*/

            ///////// Gesture Stuff that I added /////////////
            /*GestureGrid[] grids = GridScale.createGestureGridTemplates(trainingDetails, 5, 12);
            Gesture gesture = GestureIO.SkPtGestureConverter(detSkPt, detName);
            GestureGrid testGrid = new GestureGrid(gesture, 12);
            GridScale.evaluateTemplateList(testGrid, grids, 6);

            GestureGrid shift = new GestureGrid("detNULL", 12);
            shift = GridScale.displaceGrid(grids[1], shift, 0, 0);*/
            //
            // Didnt know what to do with the new code so i commented this out and just left it
            if (detailInput != null && detailInput.Count > 0)
            {
                Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
                Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();
                string gestureOutput;

                List<List<SketchPoint>> detSkPt = new List<List<SketchPoint>>();
                foreach (Tuple<int, int> detTup in detailInput)
                {
                    if (detTup.Item1 != -1 && detTup.Item2 != -1)
                    {
                        if (graph_temp[detTup].get_points().Count > 0)
                            detSkPt.Add(graph_temp[detTup].get_points());
                    }
                }
                if(detSkPt.Count == 0)
                {
                    Debug.WriteLine(detName + " not detected! No validation done!");
                    return 999;
                }
                GestureGrid[] grids = GridScale.createGestureGridTemplates(trainingDetails, 5, 12);
                Gesture gesture = GestureIO.SkPtGestureConverter(detSkPt, detName);
                GestureGrid testGrid = new GestureGrid(gesture, 12);
                //GridScale.evaluateTemplateList(testGrid, grids, 6);

                //GestureGrid shift = new GestureGrid("detNULL", 12);
                //shift = GridScale.displaceGrid(grids[1], shift, 0, 0);
                return GridScale.evaluateTemplateList(testGrid, grids, 6);
                //return PointCloudRecognizerPlus.Classify(GestureIO.SkPtGestureConverter(detSkPt, detName), trainingDetails, out gestureOutput);
            }
            else
            {
                Debug.WriteLine(detName + " not detected! No validation done!");
                return 999;
            }
        }
        private float validateDetailPDollar(List<Tuple<int, int>> detailInput, string detName)
        {
            if (detailInput != null && detailInput.Count > 0)
            {
                Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
                Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();
                string gestureOutput;

                List<List<SketchPoint>> detSkPt = new List<List<SketchPoint>>();
                foreach (Tuple<int, int> detTup in detailInput)
                {
                    if (graph_temp[detTup].get_points().Count > 0)
                        detSkPt.Add(graph_temp[detTup].get_points());
                }
                if (detSkPt.Count == 0)
                {
                    Debug.WriteLine(detName + " not detected! No validation done!");
                    return 999;
                }
                return PointCloudRecognizerPlus.Classify(GestureIO.SkPtGestureConverter(detSkPt, detName), trainingDetails, out gestureOutput);
            }
            else
            {
                Debug.WriteLine(detName + " not detected! No validation done!");
                return 999;
            }
        }
        #endregion

        #region Drawing functions
        private List<Ellipse> displayPoints(List<SketchPoint> points, Windows.UI.Color color)
        {
            List<Ellipse> drawnEllipses = new List<Ellipse>();
            foreach (SketchPoint point in points)
            {
                        var ellipse1 = new Ellipse();
                        ellipse1.Fill = new SolidColorBrush(color);
                        ellipse1.Width = 10;
                        ellipse1.Height = 10;
                        ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
                        ellipse1.VerticalAlignment = VerticalAlignment.Top;
                        //double scaledTLx = penCanvas.Width * (point.x) / 210;
                        //double scaledTLy = penCanvas.Height * (point.y) / 297;
                        ellipse1.Margin = new Thickness(point.x - ellipse1.Width / 2, point.y - ellipse1.Height / 2, 0, 0);
                        drawnEllipses.Add(ellipse1);
                        penCanvas.Children.Add(ellipse1);
            }
            return drawnEllipses;
        }

        private void highlightPoints(List<SketchPoint> points, Windows.UI.Color color, List<Ellipse> preservedPoints)
        {
            foreach (SketchPoint point in points)
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () => {
                        var ellipse1 = new Ellipse();
                        ellipse1.Fill = new SolidColorBrush(color);
                        ellipse1.Width = 5;
                        ellipse1.Height = 5;
                        ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
                        ellipse1.VerticalAlignment = VerticalAlignment.Top;
                        //double scaledTLx = penCanvas.Width * (point.x) / 210;
                        //double scaledTLy = penCanvas.Height * (point.y) / 297;
                        ellipse1.Margin = new Thickness(point.x - ellipse1.Width / 2, point.y - ellipse1.Height / 2, 0, 0);
                        penCanvas.Children.Add(ellipse1);
                        preservedPoints.Add(ellipse1);
                    }
                );
            }
        }

        private void displayPoints(SketchPoint point, Windows.UI.Color color)
        {
            var ellipse1 = new Ellipse();
            ellipse1.Fill = new SolidColorBrush(color);
            ellipse1.Width = 5;
            ellipse1.Height = 5;
            ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
            ellipse1.VerticalAlignment = VerticalAlignment.Top;
            //double scaledTLx = penCanvas.Width * (point.x) / 210;
            //double scaledTLy = penCanvas.Height * (point.y) / 297;
            ellipse1.Margin = new Thickness(point.x - ellipse1.Width / 2, point.y - ellipse1.Height / 2, 0, 0);
            penCanvas.Children.Add(ellipse1);
        }

        private void displayPoints(List<List<SketchPoint>> sketch, Windows.UI.Color color)
        {
            foreach (List<SketchPoint> points in sketch)
            {
                foreach (SketchPoint point in points)
                {
                    var ellipse1 = new Ellipse();
                    ellipse1.Fill = new SolidColorBrush(color);
                    ellipse1.Width = 5;
                    ellipse1.Height = 5;
                    ellipse1.HorizontalAlignment = HorizontalAlignment.Left;
                    ellipse1.VerticalAlignment = VerticalAlignment.Top;
                    //double scaledTLx = penCanvas.Width * (point.x) / 210;
                    //double scaledTLy = penCanvas.Height * (point.y) / 297;
                    ellipse1.Margin = new Thickness(point.x - ellipse1.Width / 2, point.y - ellipse1.Height / 2, 0, 0);
                    penCanvas.Children.Add(ellipse1);
                }
            }
        }

        private void displayRectangle(SketchRecTools.SketchBox skBox)
        {
            double canvasWidth = penCanvas.Width;
            double canvasHeight = penCanvas.Height;

            Rectangle boundBox = new Rectangle();
            //boundBox.Width = canvasWidth * skBox.boxWidth / 210;
            //boundBox.Height = canvasHeight * skBox.boxHeight / 297;
            boundBox.Width = skBox.boxWidth;
            boundBox.Height = skBox.boxHeight;
            boundBox.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
            penCanvas.Children.Add(boundBox);
            boundBox.HorizontalAlignment = HorizontalAlignment.Left;
            boundBox.VerticalAlignment = VerticalAlignment.Top;
            boundBox.StrokeThickness = 1;
            //double scaledTLx = canvasWidth * (skBox.topLeft.X) / 210;
            //double scaledTLy = canvasHeight * (skBox.topRight.Y) / 297;
            boundBox.Margin = new Thickness(skBox.topLeft.X, skBox.topRight.Y, 0, 0);
        }

        private void Det_Button_Enter(object sender, RoutedEventArgs s)
        {
            String btnName = ((Button)sender).Name;
            Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();
            switch (btnName)
            {
                case "det1":
                    //DrawKeyPath(graph_nodes, detail1Path, Colors.Yellow);
                    DrawSketchPath(detail1Path, Colors.Blue);
                    break;
                case "det2":
                    //DrawKeyPath(graph_nodes, detail2Keys, Colors.Yellow);
                    DrawSketchPath(detail2Keys, Colors.Blue);
                    break;
                case "det3":
                    //DrawKeyPath(graph_nodes, detail3Path_back, Colors.Yellow);
                    //DrawKeyPath(graph_nodes, detail3Path_forward, Colors.Yellow);
                    DrawSketchPath(Crawler.TupleListConstructor(mainGraph, detail3Path_back), Colors.Blue);
                    DrawSketchPath(Crawler.TupleListConstructor(mainGraph, detail3Path_forward), Colors.Blue);
                    break;
                case "det4":
                    //DrawKeyPath(graph_nodes, detail4Path, Colors.Yellow);
                    DrawSketchPath(Crawler.TupleListConstructor(mainGraph, detail4Path), Colors.Blue);
                    break;
                case "det5":
                    //DrawKeyPath(graph_nodes, detail5Path, Colors.Yellow);
                    DrawSketchPath(Crawler.TupleListConstructor(mainGraph, detail5Path), Colors.Blue);
                    break;
                case "det6":
                    //DrawKeyPath(graph_nodes, detail6Path, Colors.Blue);
                    DrawSketchPath(detail6Path, Colors.Blue);
                    break;
                case "det7":
                    //DrawKeyPath(graph_nodes, detail7Path, Colors.Yellow);
                    DrawSketchPath(detail7Path, Colors.Blue);
                    break;
                case "det8":
                    //DrawKeyPath(graph_nodes, detail8Path, Colors.Yellow);
                    DrawSketchPath(detail8Path, Colors.Blue);
                    break;
                case "det9":
                    //DrawKeyPath(graph_nodes, detail9Path, Colors.Yellow);
                    DrawSketchPath(detail9Path, Colors.Blue);
                    break;
                case "det10":
                    //DrawKeyPath(graph_nodes, detail10Path, Colors.Yellow);
                    DrawSketchPath(detail10Path, Colors.Blue);
                    break;
                case "det11":
                    //DrawKeyPath(graph_nodes, detail10Path, Colors.Yellow);
                    DrawSketchPath(detail11Path, Colors.Blue);
                    break;
                case "det12":
                    //DrawKeyPath(graph_nodes, detail12Path, Colors.Yellow);
                    DrawSketchPath(detail12Path, Colors.Blue);
                    break;
                case "det13":
                    //DrawKeyPath(graph_nodes, detail13Path, Colors.Yellow);
                    DrawSketchPath(detail13Path, Colors.Blue);
                    break;
                case "det14":
                    //DrawKeyPath(graph_nodes, detail13Path, Colors.Yellow);
                    DrawSketchPath(detail14Path, Colors.Blue);
                    break;
                case "det15":
                    //DrawKeyPath(graph_nodes, detail15Path, Colors.Yellow);
                    DrawSketchPath(detail15Path, Colors.Blue);
                    break;
                case "det16":
                    //DrawKeyPath(graph_nodes, detail16Path, Colors.Yellow);
                    DrawSketchPath(detail16Path, Colors.Blue);
                    break;
                case "det17":
                    //DrawKeyPath(graph_nodes, detail17Path, Colors.Yellow);
                    DrawSketchPath(detail17Path, Colors.Blue);
                    break;
                case "det18":
                    //DrawKeyPath(graph_nodes, detail18Path, Colors.Yellow);
                    DrawSketchPath(detail18Path, Colors.Blue);
                    break;
            }
        }

        private void Det_Exit(object sender, RoutedEventArgs e)
        {
            ClearKeyPath();
            ClearSketchPath();
        }

        //Runs the Resampling on defered thread
        //Allows for large samples to occur without locking up a user device
        //private void asyncResampleSketch(object sender, RoutedEventArgs e)
        //{
        //    Task.Factory.StartNew(() => resampleSketch(sender, e));
        //}

        private void DrawKeyPath(Dictionary<int, SketchAdjacencyList> g_adj, List<Tuple<int, int>> crawledKeys, Color col)
        {
            foreach (Tuple<int, int> key in crawledKeys)
            {
                double x1, x2, y1, y2;
                x1 = g_adj[key.Item1].node.avgx; x2 = g_adj[key.Item2].node.avgx; y1 = g_adj[key.Item1].node.avgy; y2 = g_adj[key.Item2].node.avgy;
                Line l = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    StrokeThickness = 8.0,
                    Stroke = new SolidColorBrush(col)
                };
                drawnLines.Add(l);
                penCanvas.Children.Add(l);
            }
        }

        private void DrawKeyPath(Dictionary<int, SketchAdjacencyList> g_adj, List<int> nodePath, Color col)
        {
            int nodeIndex = 0;
            foreach (int node in nodePath)
            {
                if (nodeIndex + 1 >= nodePath.Count)
                    break;
                int nodeNext = nodePath[nodeIndex + 1];
                double x1, x2, y1, y2;
                x1 = g_adj[node].node.avgx; x2 = g_adj[nodeNext].node.avgx; y1 = g_adj[node].node.avgy; y2 = g_adj[nodeNext].node.avgy;
                Line l = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    StrokeThickness = 3.0,
                    Stroke = new SolidColorBrush(col)
                };
                drawnLines.Add(l);
                penCanvas.Children.Add(l);
                nodeIndex++;
            }
        }

        private void ClearKeyPath()
        {
            foreach (Line lin in drawnLines)
                penCanvas.Children.Remove(lin);
        }

        private void DrawSketchPath(List<Tuple<int, int>> detPath, Color col)
        {
            List<List<SketchPoint>> gestureToWrite = new List<List<SketchPoint>>();

            Dictionary<int, SketchAdjacencyList> graph_nodes = mainGraph.getNodes();
            Dictionary<Tuple<int, int>, SketchSubstroke> graph_temp = mainGraph.getSubstrokes();

            foreach (Tuple<int, int> detTup in detPath)
            {
                if (detTup.Item1 != -1 && detTup.Item2 != -1)
                {
                    gestureToWrite.Add(graph_temp[detTup].get_points());
                }
            }

            for (int i = 0; i < gestureToWrite.Count; i++)
                drawnPoints.AddRange(displayPoints(gestureToWrite[i], col));
        }

        private void ClearSketchPath()
        {
            foreach (Ellipse cir in drawnPoints)
                penCanvas.Children.Remove(cir);
        }

        #endregion

        #region Math Helper Functions

        private double CalculateDistanceSimple(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        #endregion


        #region Mouse Events
        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Accept input only from a pen or mouse with the left button pressed. 
            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;

            if (pointerDevType == PointerDeviceType.Touch)
            {
                // Process touch input (from finger)
                pressed = false;
                e.Handled = true;
            }

            //if (pointerDevType == PointerDeviceType.Pen || (pointerDevType == PointerDeviceType.Mouse && pt.Properties.IsLeftButtonPressed))
            if (pointerDevType == PointerDeviceType.Pen)
            {
                // Get information about the pointer location.
                PointerPoint pt = e.GetCurrentPoint(penCanvas);   //CANVAS CHANGE

                previousContactPt = pt.Position;

                mouseId = pt.PointerId;

                e.Handled = true;
                pressed = true;
            }
        }
        #endregion

        #region Miscellaneous Debug Functions

        private void checkNodeAdjs(object sender, RoutedEventArgs e)
        {
            int nodeCheck = Int32.Parse(nodeCheckTxt.Text);
            string checkOutput = "";
            Dictionary<int, SketchAdjacencyList> g_adj = mainGraph.getNodes();
            SketchAdjacencyList skAdj;
            if (g_adj.ContainsKey(nodeCheck))
            {
                g_adj.TryGetValue(nodeCheck, out skAdj);
                foreach (int adjNode in skAdj.adjacent)
                    checkOutput += adjNode.ToString() + "\n";
            }

            nodeCheckOut.Text = checkOutput;
        }

        private void RemoveSkAdjDupes(Dictionary<int, SketchAdjacencyList> graph_nodes)
        {
            foreach (KeyValuePair<int, SketchAdjacencyList> kvPair in graph_nodes)
            {
                List<int> noDupeList = kvPair.Value.adjacent.Distinct().ToList();
                kvPair.Value.adjacent.Clear();
                kvPair.Value.adjacent.AddRange(noDupeList);
            }

        }

        private void iterateStroke(object sender, RoutedEventArgs e)
        {
            String btnName = ((Button)sender).Name;

            foreach (Ellipse el in highlightedPoints)
                penCanvas.Children.Remove(el);

            if (resampledStrokes == null)
            {
                List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
                resampledStrokes = resampleByDistanceAuto(convertedStrokes);

            }
            List<SketchPoint> indStroke = new List<SketchPoint>(); ;

            if (btnName == "PrevStroke")
            {
                if (highlightCounter == -1 || highlightCounter == 0)
                    highlightCounter = resampledStrokes.Count - 1;
                else
                    highlightCounter--;
            }
            if (btnName == "NextStroke")
            {
                if (highlightCounter == -1 || highlightCounter == resampledStrokes.Count - 1)
                    highlightCounter = 0;
                else
                    highlightCounter++;
            }

            StrokeID.Text = highlightCounter.ToString();
            indStroke.AddRange(resampledStrokes[highlightCounter]);
            double width = penCanvas.Width;
            double height = penCanvas.Height;

            highlightPoints(
                    indStroke,
                    Windows.UI.Color.FromArgb(255, 255, 0, 255), highlightedPoints);
        }

        private void preserveStroke(object sender, RoutedEventArgs e)
        {
            List<List<SketchPoint>> convertedStrokes = ConvertToPixels(SketchRecTools.copySubstrokes(strokes), this.penCanvas.Width, this.penCanvas.Height);
            String btnName = ((Button)sender).Name;

            if (resampledStrokes != null)
            {
                if (btnName == "AddToCir")
                {
                    savedEllipses.AddRange(highlightedPoints);
                    savedPointsCircle.AddRange(resampledStrokes[highlightCounter]);
                    det11.IsEnabled = true;

                    highlightPoints(
                            savedPointsCircle,
                            Windows.UI.Color.FromArgb(255, 255, 0, 0), savedEllipses);
                }
                if (btnName == "AddToDmd")
                {
                    savedEllipses.AddRange(highlightedPoints);
                    savedPointsDiamond.AddRange(resampledStrokes[highlightCounter]);
                    det14.IsEnabled = true;

                    highlightPoints(
                            savedPointsDiamond,
                            Windows.UI.Color.FromArgb(255, 0, 0, 255), savedEllipses);
                }
            }
        }

        private void clearPreserved(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse el in savedEllipses)
                penCanvas.Children.Remove(el);

            savedEllipses.Clear();
            savedPointsCircle.Clear();
            savedPointsDiamond.Clear();
        }

        #endregion


        //PEN RELATED QUERIES FOR DEALING WITH THE DAMN THING
        #region Pen Queries
        private void MController_Connected(IPenClient sender, ConnectedEventArgs args)
        {
            bConnect.StopWatcher();
            Debug.WriteLine(String.Format("Mac : {0}\r\n\r\nName : {1}\r\n\r\nSubName : {2}\r\n\r\nFirmware Version : {3}\r\n\r\nProtocol Version : {4}", args.MacAddress, args.DeviceName, args.SubName, args.FirmwareVersion, args.ProtocolVersion));
            pControl.SetSensitivity(0);
        }


        private async void MClient_onAddPenController(BluetoothPenClient sender, PenInformation args)
        {

            Debug.WriteLine("MClient_onAddPenController");
            //penv1 = args;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => foundDevices.Add(args)
            );


        }

        // Event that is called when a device is updated 
        private async void MClient_onUpdatePenController(BluetoothPenClient sender, PenUpdateInformation args)
        {
            Debug.WriteLine("MClient_onUpdatePenController");
            //penv1.Update(args);
        }

        // Event that is called when a device is removed
        private async void MClient_onRemovePenController(BluetoothPenClient sender, PenUpdateInformation args)
        {
            Debug.WriteLine("MClient_onRemovePenController");
            Debug.WriteLine(sender);
            Debug.WriteLine(args);
        }

        // Event that is called when the watcher operation has been stopped
        private async void MClient_onStopSearch(BluetoothPenClient sender, Windows.Devices.Bluetooth.BluetoothError args)
        {
            Debug.WriteLine("MClient_onStopSearch");

            Debug.WriteLine(sender);
            Debug.WriteLine(args);
        }
        #endregion

        #region Write-to-Console Debug Functions
        private void MController_PenStatusReceived(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_Connected(IPenClient sender, SimpleResultEventArgs args)
        {
            Debug.WriteLine(args);
        }

        private void MController_Disconnected(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_Authenticated(IPenClient sender, object args)
        {
            pControl.AddAvailableNote();
        }

        private void MController_DotReceived(IPenClient sender, DotReceivedEventArgs args)
        {
            if (args.Dot.DotType == DotTypes.PEN_DOWN)
            {
                penDown = false;
            }
            sendToCanvas(args.Dot);
        }

        private void MController_PasswordRequested(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_OfflineDataListReceived(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_AutoPowerOffTimeChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_AutoPowerOnChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_BatteryAlarmReceived(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_RtcTimeChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_SensitivityChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_PasswordChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_BeepSoundChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_PenColorChanged(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_OfflineDataDownloadStarted(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_OfflineStrokeReceived(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_OfflineDownloadFinished(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void MController_FirmwareInstallationStarted(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_FirmwareInstallationStatusUpdated(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        private void MController_FirmwareInstallationFinished(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void Mcontroller_PenProfileReceived(IPenClient sender, object args)
        {
            Debug.WriteLine(args);
        }
        #endregion
    }
}
