using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    public class GestureIO
    {
        /// <summary>
        /// Reads a multistroke gesture from an XML file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// 

        public static Gesture SkPtGestureConverter(List<List<SketchPoint>> SkPtInput, string gestureName)
        {
            List<DollarPoint> dpPoints = new List<DollarPoint>();
            int currentStrokeIndex = 0;
            foreach(List<SketchPoint> stroke in SkPtInput)
            {
                foreach(SketchPoint skPoint in stroke)
                {
                    dpPoints.Add(new DollarPoint((float)skPoint.x, (float)skPoint.y, currentStrokeIndex));
                }
                currentStrokeIndex++;
            }
            return new Gesture(dpPoints.ToArray(), gestureName);
        }

        /*
        public static Gesture ReadGesture(string fileName)
        {
            List<Point> points = new List<Point>();
            XmlReader xmlReader = null;
            int currentStrokeIndex = -1;
            string gestureName = "";
            try
            {
                xmlReader = XmlReader.Create(fileName);
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType != XmlNodeType.Element) continue;
                    switch (xmlReader.Name)
                    {
                        case "Gesture":
                            gestureName = xmlReader["Name"];
                            if (gestureName.Contains("~")) // '~' character is specific to the naming convention of the MMG set
                                gestureName = gestureName.Substring(0, gestureName.LastIndexOf('~'));
                            if (gestureName.Contains("_")) // '_' character is specific to the naming convention of the MMG set
                                gestureName = gestureName.Replace('_', ' ');
                            break;
                        case "Stroke":
                            currentStrokeIndex++;
                            break;
                        case "Point":
                            points.Add(new Point(
                                float.Parse(xmlReader["X"]),
                                float.Parse(xmlReader["Y"]),
                                currentStrokeIndex
                            ));
                            break;
                    }
                }
            }
            finally
            {
                if (xmlReader != null)
                    xmlReader.Dispose();
            }
            return new Gesture(points.ToArray(), gestureName);
        }

        /// <summary>
        /// Writes a multistroke gesture to an XML file
        /// </summary>
        public static void WriteGesture(Point[] points, string gestureName, string fileName)
        {
            Windows.Storage.StorageFile file = await MainPage.produceFileHandler(FileType.PDOLLAR_WRITE);
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                sw.WriteLine("<Gesture Name = \"{0}\">", gestureName);
                int currentStroke = -1;
                for (int i = 0; i < points.Length; i++)
                {
                    if (points[i].StrokeID != currentStroke)
                    {
                        if (i > 0)
                            sw.WriteLine("\t</Stroke>");
                        sw.WriteLine("\t<Stroke>");
                        currentStroke = points[i].StrokeID;
                    }

                    sw.WriteLine("\t\t<Point X = \"{0}\" Y = \"{1}\" T = \"0\" Pressure = \"0\" />",
                        points[i].X, points[i].Y
                    );
                }
                sw.WriteLine("\t</Stroke>");
                sw.WriteLine("</Gesture>");
            }
        }
        */
    }
}
