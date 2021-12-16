using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SmartPenSketch
{
    public class GestureGrid
    {
        private float[,] matrix;
        private string name;
        private uint size;

        // Empty matrix creation
        public GestureGrid(string name, uint size)
        {
            this.name = name;
            this.size = size;
            this.matrix = new float[size, size];
        }

        // Definitely can be improved, basic attempt at making grading work
        public GestureGrid(Gesture gesture, uint size)
        {
            this.size = size;
            this.name = gesture.Name;
            this.matrix = new float[size, size];
            uint[,] countMatrix = new uint[size, size];

            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;

            for (uint i = 0; i < gesture.Points.Length; i++)
            {
                xmin = Math.Min(xmin, gesture.Points[i].X);
                xmax = Math.Max(xmax, gesture.Points[i].X);
                ymin = Math.Min(ymin, gesture.Points[i].Y);
                ymax = Math.Max(ymax, gesture.Points[i].Y);
            }

            float xrange = xmax - xmin;
            float yrange = ymax - ymin;

            for (uint i = 0; i < gesture.Points.Length; i++)
            {
                uint r = Math.Min((uint)Math.Floor((gesture.Points[i].X - xmin) / xrange * size), size - 1);
                uint c = Math.Min((uint)Math.Floor((gesture.Points[i].Y - ymin) / yrange * size), size - 1);
                countMatrix[r, c]++;
            }

            for (uint i = 0; i < size; i++)
                for (uint j = 0; j < size; j++)
                    matrix[i, j] = (float)countMatrix[i, j] / (float)gesture.Points.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string getName()
        {
            return name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint getSize()
        {
            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float getElement(uint i, uint j)
        {
            return this.matrix[i, j];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void setElement(uint i, uint j, float item)
        {
            this.matrix[i, j] = item;
        }

        // Assumes all provided gestures are of the same type, length, and class
        // Overlays a group of gestures ontop of each other, then normalizes
        public static GestureGrid combineGesturesParams(params GestureGrid[] gestures)
        {
            return combineGestures(gestures);
        }

        public static GestureGrid combineGestures(GestureGrid[] gestures)
        {
            for (uint i = 0; i < gestures[0].size; i++)
                for (uint j = 0; j < gestures[0].size; j++)
                {
                    for (uint k = 1; k < gestures.Length; k++)
                        gestures[0].matrix[i, j] += gestures[k].matrix[i, j];
                    gestures[0].matrix[i, j] /= (float)gestures.Length;
                }
            return gestures[0];
        }

        public void log()
        {
            for (uint i = 0; i < size; i++)
            {
                Debug.Write("| ");
                for (uint j = 0; j < size; j++)
                {
                    Debug.WriteIf(matrix[i, j] > 0, "% ");
                    Debug.WriteIf(matrix[i, j] == 0, "  ");
                }
                Debug.WriteLine("|");

            }
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
        }
    }

    public static class GridScale
    {
        // Provided a linear array of gestures, creates a template based on some offset
        // Assumes that the provided array is a multiple of the provided offset
        public static GestureGrid[] createGestureGridTemplates(Gesture[] gestures, uint offset, uint size)
        {
            GestureGrid[] gestureGrids = (from gesture in gestures select new GestureGrid(gesture, size)).ToArray();
            uint newLength = (uint)gestures.Length / offset;
            GestureGrid[] final = new GestureGrid[newLength];
            GestureGrid[] destArray = new GestureGrid[offset];
            for (uint i = 0; i < newLength; i++)
            {
                Array.Copy(gestureGrids, (int)(i * offset), destArray, 0, (int)offset);
                final[i] = GestureGrid.combineGestures(destArray);
            }
            return final;
        }

        // Embeds a grid inside of one without certain features
        public static GestureGrid displaceGrid(GestureGrid source, GestureGrid embed, long dr, long dc)
        {
            uint k = source.getSize();
            // Embed starting indices 
            uint re_S = (uint)Math.Max(0, dr);
            uint ce_S = (uint)Math.Max(0, dc);

            uint rs_S = (uint)Math.Max(0, -dr);
            uint rs_E = (uint)Math.Min(k, k - dr);
            uint cs_S = (uint)Math.Max(0, -dc);
            uint cs_E = (uint)Math.Min(k, k - dc);

            for (uint i = 0; i < rs_E - rs_S; i++)
                for (uint j = 0; j < cs_E - cs_S; j++)
                    embed.setElement(
                        re_S + i,
                        ce_S + j,
                        source.getElement(
                            rs_S + i,
                            cs_S + j));

            return embed;
        }

        public static float gridCompare(GestureGrid test, GestureGrid template)
        {
            float total = 0;
            for (uint i = 0; i < test.getSize(); i++)
                for (uint j = 0; j < test.getSize(); j++)
                {
                    float rem = Math.Max(template.getElement(i, j) - test.getElement(i, j), 0);
                    total += rem;
                }
            return total;
        }
        public static float bestFit(GestureGrid test, GestureGrid template, uint shift)
        {
            float min = float.MaxValue;
            GestureGrid embed = new GestureGrid(test.getName(), test.getSize());
            for (long sx = -shift; sx < shift; sx++)
                for (long sy = -shift; sy < shift; sy++)
                {
                    embed = displaceGrid(test, embed, sx, sy);
                    min = Math.Min(min, gridCompare(embed, template));
                }
            
            return min;
        }



        public static float evaluateTemplateList(GestureGrid test, GestureGrid[] templates, uint shift)
        {
            float min = float.MaxValue;
            string minTemp="";
            List<GestureGrid> templateSubset = new List<GestureGrid>();
            for(int m = 0; m<templates.Length; m++)
            {
                if (test.getName() == templates[m].getName())
                    templateSubset.Add(templates[m]);
            }

            foreach(GestureGrid tmp in templateSubset)
            {
                float current = gridCompare(test, tmp);
                if (current < min)
                {
                    min = current;
                    minTemp = tmp.getName();
                }
            }
            //min = Math.Min( 
            //min, 
            //gridCompare(test, templates[i]));

            //Debug.Write(test.getName());
            //Debug.Write(" ");
            //Debug.Write("Best Score: ");
            //Debug.Write(min.ToString());
            Debug.WriteLine(test.getName() + " Predicted as: " + minTemp + " with a distance of: " + min);
            //Debug.WriteLine("");
            return min;
        }
    }
}
