using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleTrackBar
{
    public static class ComponentDrawer
    {
        public static Pen PenLight = new Pen(Color.Red);
        public static Pen PenDark = new Pen(Color.Blue);

        public static void Draw3DRectangle(PaintEventArgs e, Rectangle rect, Bitmap fieldImage = null)
        {
            var rectPoints = RectToPoints(rect);
            Draw3DRectangle(e, rectPoints[0], rectPoints[1], rectPoints[2], rectPoints[3], fieldImage, PenLight, PenDark);
        }

        public static void Draw3DRectangle(PaintEventArgs e, Point topL, Point topR, Point bottR, Point bottL, Bitmap fieldImage = null)
        {
            Draw3DRectangle(e, topL, topR, bottL, bottR, fieldImage, PenLight, PenDark);
        }

        public static void Draw3DRectangleInverted(PaintEventArgs e, Point topL, Point topR, Point bottR, Point bottL, Bitmap fieldImage = null)
        {
            Draw3DRectangle(e, topL, topR, bottL, bottR, fieldImage, PenDark, PenLight);
        }

        public static void Draw3DRectangleInverted(PaintEventArgs e, Rectangle rect, Bitmap fieldImage = null)
        {
            var rectPoints = RectToPoints(rect);
            Draw3DRectangle(e, rectPoints[0], rectPoints[1], rectPoints[2], rectPoints[3], fieldImage, PenDark,PenLight);
        }

        private static void Draw3DRectangle(PaintEventArgs e, Point topL, Point topR, Point bottR, Point bottL, Bitmap fieldImage, Pen penLight, Pen penDark)
        {
            if (fieldImage != null)
            {
                e.Graphics.DrawImage(fieldImage, topL.X, topL.Y, topR.X - topL.X, bottL.Y - topL.Y);
            }

            e.Graphics.DrawLine(penDark, topL, topR);	// Top
            e.Graphics.DrawLine(penLight, topR, bottR);	// Right
            e.Graphics.DrawLine(penLight, bottR, bottL);	// Bottom
            e.Graphics.DrawLine(penDark, bottL, topL);	// Left
        }

        public static void Draw3DLineInField(PaintEventArgs e, Rectangle rect, bool lineIsHorizontal = true)
        {

            int offset = (lineIsHorizontal) ? rect.Height/3 : rect.Width / 3;

            Point start = new Point(rect.X + rect.Width/2, rect.Y + offset);
            Point end = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height - offset);

            if (!lineIsHorizontal)
            {
                start = new Point(rect.X + offset, rect.Y + rect.Height / 2);
                end = new Point(rect.X + rect.Width - offset, rect.Y + rect.Height / 2);               
            }

            e.Graphics.DrawLine(PenDark, start, end);
            
            if (lineIsHorizontal)
            {
                e.Graphics.DrawLine(PenLight, start.X - 1, start.Y, end.X - 1, end.Y);          
            }
            else
            {
                e.Graphics.DrawLine(PenLight, start.X, start.Y - 1, end.X, end.Y - 1);                           
            }
        }

        public static void DrawSkala(PaintEventArgs e, Rectangle rect, int linesCount, bool linesIsHorizontal = true)
        {
            double deltaTick;
            int tickpos;

            if (linesCount > 1)
            {
                deltaTick = linesIsHorizontal ? (double)rect.Height / linesCount : (double)rect.Width / linesCount;

                for(int i = 0; i < linesCount + 1; i++)
                {
                    tickpos = (int)Math.Round(i * deltaTick);

                    if (linesIsHorizontal)
                    {
                        e.Graphics.DrawLine(PenDark,
                            rect.X, rect.Y + tickpos,
                            rect.X + rect.Width, rect.Y + tickpos);
                    }
                    else // Vertical bar
                    {
                        e.Graphics.DrawLine(PenDark,
                            rect.X + tickpos, rect.Y,
                            rect.X + tickpos, rect.Y + rect.Height);  
                    }
                }
            }
        }

        public static void DrawFilledRect(PaintEventArgs e, Rectangle rect, Color color)
        {
            SolidBrush brushInner = new SolidBrush(color);

            e.Graphics.FillRectangle(brushInner, rect);
        }

        private static Point[] RectToPoints(Rectangle rect)
        {
            return new []{
                rect.Location, //topL
                new Point(rect.X + rect.Width, rect.Y), //topR
                new Point(rect.X + rect.Width, rect.Y + rect.Height),//bottR
                new Point(rect.X, rect.Y+ rect.Height) //BottL
            };
        }

        public static void DrawString(PaintEventArgs e, string strToDraw, int emSize, float x, float y, StringFormat strformat)
        {
            Font fontMark = new Font("Arial", emSize);
            SolidBrush brushMark = new SolidBrush(PenDark.Color);

            e.Graphics.DrawString(strToDraw, fontMark, brushMark, x, y, strformat);
        }
    }
}
