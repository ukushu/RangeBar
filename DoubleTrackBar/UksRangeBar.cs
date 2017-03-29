/*
 * This custom control is customized by Andrew Vynnychenko
 * and based on ZzzzRangeBar by Detlef Neunherz
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleTrackBar
{
    public class UksRangeBar : UserControl
    {
        public delegate void RangeChangedEventHandler(object sender, EventArgs e);

        public delegate void RangeChangingEventHandler(object sender, EventArgs e);

        public event RangeChangedEventHandler RangeChanged;
        public event RangeChangedEventHandler RangeChanging;

        /// <summary> 
        /// designer variable
        /// </summary>
        private System.ComponentModel.Container _components = null;

        private Color _colorInner = Color.LightGreen;
        private Color _colorRange = Color.FromKnownColor(KnownColor.Control);

        private double _minimum;
        private double _maximum = 100;

        private double _rangeMin;
        private double _rangeMax = 10;

        private ActiveMarkType _activeKnob = ActiveMarkType.None;

        private RangeBarOrientation _orientationBar = RangeBarOrientation.Horizontal;
        private TopBottomOrientation _orientationScale = TopBottomOrientation.Bottom;

        private int _barHeight = 8;
        private int _knobWidth = 8;
        
        
        private int _tickHeight = 9;
        private int _numAxisDivision = 100;

        private int _skalaToBackFieldDistance = 3;

        private int _pixelPosL, _pixelPosR;

        private Rectangle _lKnobRect =  new Rectangle();
        private Rectangle _rKnobRect = new Rectangle();

        private bool _lKnobMoveGoing = false;
        private bool _rKnobIsMoving = false;

        private bool _valueShownOnKnobsMove;

        private Bitmap _fieldImage;

        private int KnobHeight {
            get
            {
                return LocalHeight - PixelsOffsetForNumber * 2 - _tickHeight - _skalaToBackFieldDistance;
            }
        }
        private int PixelsOffsetForNumber
        {
            get
            {
                if (_orientationBar == RangeBarOrientation.Vertical)
                {
                    return (6 * TotalMaximum.ToString().Length - 1) + 12;
                }

                return 12;
            }
        }
        private int TopOffsetToBackField
        {
            get { return _tickHeight + PixelsOffsetForNumber + _skalaToBackFieldDistance;}
        }
        private int BottomOffsetToBackField
        {
            get { return LocalHeight - _tickHeight - PixelsOffsetForNumber - _skalaToBackFieldDistance; }
        }

        private int PixelPosXmin
        {
            get { return _knobWidth + 1;}
        }
        private int PixelPosXmax
        {
            get { return LocalWidth - _knobWidth - 1; }
        }

        public Bitmap FieldImage
        {
            get { return _fieldImage; }
            set
            {
                _fieldImage = value;
                Invalidate();
                Update();
            }
        }

        public bool ValueShownOnKnobsMove
        {
            get { return _valueShownOnKnobsMove; }
            set
            {
                _valueShownOnKnobsMove = value;
                Invalidate();
                Update();
            }
        }

        public int HeightOfTick
        {
            get { return _tickHeight; }
            set
            {
                _tickHeight = Math.Min(Math.Max(1, value), _barHeight);
                Invalidate();
                Update();
            }
        }

        public RangeBarOrientation Orientation
        {
            get { return _orientationBar; }
            set
            {
                _orientationBar = value;
                Invalidate();
                Update();
            }
        }

        public TopBottomOrientation ScaleOrientation
        {
            get { return _orientationScale; }
            set
            {
                _orientationScale = value;
                Invalidate();
                Update();
            }
        }

        public int RangeMaximum
        {
            get { return (int)_rangeMax; }
            set
            {
                _rangeMax = value;

                if (_rangeMax < _minimum)
                    _rangeMax = _minimum;
                else if (_rangeMax > _maximum)
                    _rangeMax = _maximum;

                if (_rangeMax < _rangeMin)
                    _rangeMax = _rangeMin;

                RangePos2PixelPos();
                Invalidate(true);
            }
        }

        public int RangeMinimum
        {
            get { return (int)_rangeMin; }
            set
            {
                _rangeMin = value;

                if (_rangeMin < _minimum)
                    _rangeMin = _minimum;
                else if (_rangeMin > _maximum)
                    _rangeMin = _maximum;

                if (_rangeMin > _rangeMax)
                    _rangeMin = _rangeMax;

                RangePos2PixelPos();
                Invalidate(true);
            }
        }

        public int TotalMaximum
        {
            get { return (int)_maximum; }
            set
            {
                _maximum = value;

                if (_rangeMax > _maximum)
                    _rangeMax = _maximum;

                RangePos2PixelPos();

                Invalidate(true);
            }
        }

        public int TotalMinimum
        {
            get { return (int)_minimum; }
            set
            {
                _minimum = value;

                if (_rangeMin < _minimum)
                    _rangeMin = _minimum;

                RangePos2PixelPos();

                Invalidate(true);
            }
        }

        public int DivisionNum
        {
            get { return _numAxisDivision; }
            set
            {
                _numAxisDivision = value;

                Refresh();
            }
        }

        public Color InnerColor
        {
            get { return _colorInner; }
            set
            {
                _colorInner = value;
                Refresh();
            }
        }
        
        private int LocalHeight
        {
            get { return (_orientationBar == RangeBarOrientation.Horizontal) ? Height : Width; }
            set
            {
                if (_orientationBar == RangeBarOrientation.Horizontal)
                {
                    Height = value;
                }
                else
                {
                    Width = value;
                }
            }
        }
        
        private int LocalWidth
        {
            get { return (_orientationBar == RangeBarOrientation.Horizontal) ? Width : Height; }
        }

        private int MinLocalHeight
        {
            get { return 2 * TopOffsetToBackField + 6; }
        }

        public UksRangeBar()
        {
            Name = "UksRangeBar";
            Size = new Size(344, 64);
            Resize += OnResize;
            Load += OnLoad;
            SizeChanged += OnSizeChanged;
            MouseUp += OnMouseUp;
            Paint += OnPaint;
            Leave += OnLeave;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;

            //Maybe not the best solution, but system colors can be changes not soften due to runtime
            var lightShadowColor = Color.FromKnownColor(KnownColor.ControlLightLight);
            var darkhadowColor = Color.FromKnownColor(KnownColor.ControlDarkDark);

            ComponentDrawer.PenLight = new Pen(lightShadowColor);
            ComponentDrawer.PenDark = new Pen(darkhadowColor);
        }

        /// <summary> 
        /// Clean up the resources used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_components != null)
                {
                    _components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public void SelectRange(int left, int right)
        {
            RangeMinimum = left;
            RangeMaximum = right;

            RangePos2PixelPos();

            Invalidate(true);
        }

        public void SetRangeLimit(double left, double right)
        {
            _minimum = left;
            _maximum = right;

            RangePos2PixelPos();
            Invalidate(true);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // range check
            if (_pixelPosL < PixelPosXmin) _pixelPosL = PixelPosXmin;
            if (_pixelPosL > PixelPosXmax) _pixelPosL = PixelPosXmax;
            if (_pixelPosR > PixelPosXmax) _pixelPosR = PixelPosXmax;
            if (_pixelPosR < PixelPosXmin) _pixelPosR = PixelPosXmin;

            RangePos2PixelPos();
            
            DrawSkala(e);
            
            DrawBarField(e);

            DrawSelectedRegion(e);

            RecalcKnobsPos(ref _lKnobRect, _pixelPosL);//do not combine this method calls
            RecalcKnobsPos(ref _rKnobRect, _pixelPosR);

            PaintKnob(e, _lKnobRect, _pixelPosL);//do not combine this method calls
            PaintKnob(e, _rKnobRect, _pixelPosR);
        }

        private void DrawBarField(PaintEventArgs e)
        {
            var componEdgeToSkalaOffset = 9;

            Point backgrTopL;
            Point backgrTopR = new Point();
            Point backgrBottL = new Point();
            Point backgrBottR;

            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                backgrTopL  = new Point(componEdgeToSkalaOffset, TopOffsetToBackField);
                backgrTopR.X = LocalWidth - componEdgeToSkalaOffset;
                backgrBottL.Y = BottomOffsetToBackField;
            }
            else
            {
                backgrTopL  = new Point(TopOffsetToBackField, componEdgeToSkalaOffset);
                backgrTopR.X = BottomOffsetToBackField;
                backgrBottL.Y = LocalWidth - componEdgeToSkalaOffset;
            }

            backgrTopR.Y = backgrTopL.Y;
            backgrBottL.X = backgrTopL.X;
            backgrBottR = new Point(backgrTopR.X, backgrBottL.Y);
            
            ComponentDrawer.Draw3DRectangle(e, backgrTopL, backgrTopR, backgrBottL, backgrBottR, FieldImage);
        }

        private void DrawSelectedRegion(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle();

            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                rect = new Rectangle(_pixelPosL, TopOffsetToBackField + 1,
                    _pixelPosR - _pixelPosL + 1, BottomOffsetToBackField - TopOffsetToBackField - 1);
            }
            else
            {
                rect = new Rectangle(TopOffsetToBackField + 1, _pixelPosL, Width - TopOffsetToBackField*2 - 1,
                    _pixelPosR - _pixelPosL + 1);
            }

            if (Enabled)
            {
                ComponentDrawer.DrawFilledRect(e, rect, _colorInner);
            }
            else
            {
                ComponentDrawer.DrawFilledRect(e, rect, Color.FromKnownColor(KnownColor.InactiveCaption));
            }
        }

        private void CalcSkalaTopAndBottom(out int skalaTop, out int skalaBottom)
        {
            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    skalaTop = skalaBottom = Height - _tickHeight - PixelsOffsetForNumber;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    skalaTop = skalaBottom = PixelsOffsetForNumber;
                }
                else
                {
                    skalaTop = PixelsOffsetForNumber;
                    skalaBottom = Height - _tickHeight - PixelsOffsetForNumber;
                }
            }
            else // Vertical bar
            {
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    skalaTop = skalaBottom = Width - _tickHeight - PixelsOffsetForNumber;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    skalaTop = skalaBottom = PixelsOffsetForNumber;
                }
                else
                {
                    skalaTop = PixelsOffsetForNumber;
                    skalaBottom = Width - _tickHeight - PixelsOffsetForNumber;
                }
            }

        }

        private void DrawSkala(PaintEventArgs e)
        {
            int skalaTopOffset, skalaBottomOffset;//Y for horiz; X for vert;
            CalcSkalaTopAndBottom(out skalaTopOffset, out skalaBottomOffset);
            
            Rectangle rectTopOrLeft = new Rectangle();
            Rectangle rectBottOrRight = new Rectangle();
            
            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                if (_orientationScale == TopBottomOrientation.Top || _orientationScale == TopBottomOrientation.Both)
                {
                    rectTopOrLeft = new Rectangle(9, skalaTopOffset, Width - 18, _tickHeight);
                    ComponentDrawer.DrawSkala(e, rectTopOrLeft, _numAxisDivision, false);
                }

                if (_orientationScale == TopBottomOrientation.Bottom || _orientationScale == TopBottomOrientation.Both)
                {
                    rectBottOrRight = new Rectangle(9, skalaBottomOffset, Width - 18, _tickHeight);
                    ComponentDrawer.DrawSkala(e, rectBottOrRight, _numAxisDivision, false);
                }
            }
            else  // Vertical bar
            {              
                if (_orientationScale == TopBottomOrientation.Top || _orientationScale == TopBottomOrientation.Both)
                {
                    rectTopOrLeft = new Rectangle(skalaTopOffset, 9, _tickHeight, Height - 18);
                    ComponentDrawer.DrawSkala(e, rectTopOrLeft, _numAxisDivision);
                }

                if (_orientationScale == TopBottomOrientation.Bottom || _orientationScale == TopBottomOrientation.Both)
                {
                    rectBottOrRight = new Rectangle(skalaBottomOffset, 9, _tickHeight, Height - 18);
                    ComponentDrawer.DrawSkala(e, rectBottOrRight, _numAxisDivision);
                }
            }

            ShowCurrPosValueIfNeeded(e, skalaTopOffset);
        }

        private void RecalcKnobsPos(ref Rectangle rect, int pixelPos)
        {
            var offsetX = _knobWidth/2;
            var offsetY = (_tickHeight + _skalaToBackFieldDistance)/2;

            Point topL;

            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                topL = new Point(pixelPos - offsetX, TopOffsetToBackField - offsetY);
                
                rect = new Rectangle(topL, new Size(_knobWidth, KnobHeight));
                
                return;
            }

            topL = new Point(TopOffsetToBackField - offsetY, pixelPos - offsetX);

            rect = new Rectangle(topL, new Size(KnobHeight, _knobWidth));
        }

        private void PaintKnob(PaintEventArgs e, Rectangle pos, int pixelPos)
        {
            SolidBrush brushRange = new SolidBrush(_colorRange);

            var rectanglePoints = new Point[]{
                pos.Location, //topL
                new Point(pos.X+pos.Width, pos.Y), //topR
                new Point(pos.X+pos.Width, pos.Y+ pos.Height),//bottR
                new Point(pos.X, pos.Y+ pos.Height) //BottL
            };           

            //Fill range between knobs
            e.Graphics.FillPolygon(brushRange, rectanglePoints);
            
            ComponentDrawer.Draw3DRectangleInverted(e, pos);

            //Draw line in the middle of Knoob       
            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                ComponentDrawer.Draw3DLineInField(e, pos);
            }
            else
            {
                ComponentDrawer.Draw3DLineInField(e, pos, false);
            }
        }

        private void ShowCurrPosValueIfNeeded(PaintEventArgs e, int tickOffset)
        {
            if (ValueShownOnKnobsMove == false)
                return;

            int y = 0;

            switch (ScaleOrientation) 
            {
                case TopBottomOrientation.Bottom:
                y = tickOffset + _tickHeight;
                break;
                case TopBottomOrientation.Top:
                y = LocalHeight - TopOffsetToBackField + 6;
                break;
                case TopBottomOrientation.Both:
                y = LocalHeight - PixelsOffsetForNumber;
                break;
            }

            StringFormat strformat = new StringFormat();

            strformat.Alignment = StringAlignment.Center;
            strformat.LineAlignment = StringAlignment.Near;

            if (_lKnobMoveGoing)
            {  
                if (_orientationBar == RangeBarOrientation.Horizontal)
                {
                    ComponentDrawer.DrawString(e, _rangeMin.ToString(), _knobWidth, _pixelPosL, y, strformat);
                }
                else
                {
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;

                    ComponentDrawer.DrawString(e, _rangeMin.ToString(), _knobWidth, y + 2, _pixelPosL, strformat);
                }
            }

            if (_rKnobIsMoving)
            {
                if (_orientationBar == RangeBarOrientation.Horizontal)
                {
                    ComponentDrawer.DrawString(e, _rangeMax.ToString(), _knobWidth, _pixelPosR, y, strformat);
                }
                else
                {
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;

                    ComponentDrawer.DrawString(e, _rangeMax.ToString(), _knobWidth, y, _pixelPosR, strformat);
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                if (_lKnobRect.Contains(e.X, e.Y))
                {
                    Capture = true;
                    _lKnobMoveGoing = true;
                    _activeKnob = ActiveMarkType.Left;
                    Invalidate(true);
                }
                else if (_rKnobRect.Contains(e.X, e.Y))
                {
                    Capture = true;
                    _rKnobIsMoving = true;
                    _activeKnob = ActiveMarkType.Right;
                    Invalidate(true);
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Capture = false;

                _lKnobMoveGoing = false;
                _rKnobIsMoving = false;

                Invalidate();

                OnRangeChanged(EventArgs.Empty);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                if (_lKnobRect.Contains(e.X, e.Y) || _rKnobRect.Contains(e.X, e.Y))
                {
                    Cursor = (_orientationBar == RangeBarOrientation.Horizontal) ? Cursors.SizeWE : Cursors.SizeNS;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }

                if (_lKnobMoveGoing)
                {
                    Cursor = (_orientationBar == RangeBarOrientation.Horizontal) ? Cursors.SizeWE : Cursors.SizeNS;

                    _pixelPosL = (_orientationBar == RangeBarOrientation.Horizontal) ? e.X : e.Y;
                    
                    if (_pixelPosL < PixelPosXmin)
                        _pixelPosL = PixelPosXmin;                   
                    if (_pixelPosL > PixelPosXmax)
                        _pixelPosL = PixelPosXmax;                   
                    if (_pixelPosR < _pixelPosL)
                        _pixelPosR = _pixelPosL;
                    
                    PixelPos2RangePos();
                    _activeKnob = ActiveMarkType.Left;
                    Invalidate(true);

                    OnRangeChanging(EventArgs.Empty);
                }
                else if (_rKnobIsMoving)
                {
                    Cursor = (_orientationBar == RangeBarOrientation.Horizontal) ? Cursors.SizeWE : Cursors.SizeNS;

                    _pixelPosR = (_orientationBar == RangeBarOrientation.Horizontal) ? e.X : e.Y;
                    
                    if (_pixelPosR < PixelPosXmin)
                        _pixelPosR = PixelPosXmin;                                      
                    if (_pixelPosR > PixelPosXmax)
                        _pixelPosR = PixelPosXmax;                    
                    if (_pixelPosL > _pixelPosR)
                        _pixelPosL = _pixelPosR;
                    
                    PixelPos2RangePos();
                    _activeKnob = ActiveMarkType.Right;
                    Invalidate(true);

                    OnRangeChanging(EventArgs.Empty);
                }
            }
        }

        private void PixelPos2RangePos()
        {
            int posw = LocalWidth - 2 * _knobWidth - 2;

            _rangeMin = _minimum + (int)Math.Round((_maximum - _minimum) * (_pixelPosL - PixelPosXmin) / posw);
            _rangeMax = _minimum + (int)Math.Round((_maximum - _minimum) * (_pixelPosR - PixelPosXmin) / posw);
        }

        private void RangePos2PixelPos()
        {
            int posw = LocalWidth - 2 * _knobWidth - 2;

            _pixelPosL = PixelPosXmin + (int)Math.Round(posw * (_rangeMin - _minimum) / (_maximum - _minimum));
            _pixelPosR = PixelPosXmin + (int)Math.Round(posw * (_rangeMax - _minimum) / (_maximum - _minimum));
        }

        private void OnResize(object sender, EventArgs e)
        {
            //RangePos2PixelPos();

            if (LocalHeight < MinLocalHeight)
            {
                LocalHeight = MinLocalHeight;                
            }
               
            Invalidate(true);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // use double buffering
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            Invalidate(true);
            Update();
        }

        private void OnLeave(object sender, EventArgs e)
        {
            _activeKnob = ActiveMarkType.None;
        }

        public virtual void OnRangeChanged(EventArgs e)
        {
            if (RangeChanged != null)
                RangeChanged(this, e);
        }

        public virtual void OnRangeChanging(EventArgs e)
        {
            if (RangeChanging != null)
                RangeChanging(this, e);
        }
    }

    public enum ActiveMarkType { None, Left, Right };
    public enum RangeBarOrientation { Horizontal, Vertical };
    public enum TopBottomOrientation { Top, Bottom, Both };
}
