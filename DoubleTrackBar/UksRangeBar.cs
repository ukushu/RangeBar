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

        public UksRangeBar()
        {
            Name = "UksRangeBar";
            Size = new Size(344, 64);
            KeyPress += OnKeyPress;
            Resize += OnResize;
            Load += OnLoad;
            SizeChanged += OnSizeChanged;
            MouseUp += OnMouseUp;
            Paint += OnPaint;
            Leave += OnLeave;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
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

        public enum ActiveMarkType { None, Left, Right };
        public enum RangeBarOrientation { Horizontal, Vertical };
        public enum TopBottomOrientation { Top, Bottom, Both };

        private Color _colorInner = Color.LightGreen;
        private Color _colorRange = Color.FromKnownColor(KnownColor.Control);
        private Color _colorShadowLight = Color.FromKnownColor(KnownColor.ControlLightLight);
        private Color _colorShadowDark = Color.FromKnownColor(KnownColor.ControlDarkDark);
        private int sizeShadow = 1;
        private double _minimum = 0;
        private double _maximum = 10;
        private double _rangeMin = 3;
        private double _rangeMax = 5;
        private ActiveMarkType _activeMark = ActiveMarkType.None;


        private RangeBarOrientation _orientationBar = RangeBarOrientation.Horizontal; // orientation of range bar
        private TopBottomOrientation _orientationScale = TopBottomOrientation.Bottom;
        private int _barHeight = 8;
        private int _markWidth = 8;
        private int _markHeight = 24;
        private int _tickHeight = 6;
        private int _numAxisDivision = 10;

        private int _pixelPosL, _pixelPosR;
        private int _xPosMin, _xPosMax;

        private Point[] _lMarkPnt = new Point[5];
        private Point[] _rMarkPnt = new Point[5];

        private bool _moveLMark = false;
        private bool _moveRMark = false;
        
        public int HeightOfTick
        {
            set
            {
                _tickHeight = Math.Min(Math.Max(1, value), _barHeight);
                Invalidate();
                Update();
            }
            get
            {
                return _tickHeight;
            }
        }
        public int HeightOfMark
        {
            set
            {
                _markHeight = Math.Max(_barHeight + 2, value);
                Invalidate();
                Update();
            }
            get
            {
                return _markHeight;
            }
        }

        public int HeightOfBar
        {
            set
            {
                _barHeight = Math.Min(value, _markHeight - 2);
                Invalidate();
                Update();
            }
            get
            {
                return _barHeight;
            }

        }

        public RangeBarOrientation Orientation
        {
            set
            {
                _orientationBar = value;
                Invalidate();
                Update();
            }
            get
            {
                return _orientationBar;
            }
        }

        public TopBottomOrientation ScaleOrientation
        {
            set
            {
                _orientationScale = value;
                Invalidate();
                Update();
            }
            get
            {
                return _orientationScale;
            }
        }

        public int RangeMaximum
        {
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
            get { return (int)_rangeMax; }
        }

        public int RangeMinimum
        {
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
            get
            {
                return (int)_rangeMin;
            }
        }

        public int TotalMaximum
        {
            set
            {
                _maximum = (double)value;
                if (_rangeMax > _maximum)
                    _rangeMax = _maximum;
                RangePos2PixelPos();
                Invalidate(true);
            }
            get { return (int)_maximum; }
        }

        public int TotalMinimum
        {
            set
            {
                _minimum = value;
                if (_rangeMin < _minimum)
                    _rangeMin = _minimum;
                RangePos2PixelPos();
                Invalidate(true);
            }
            get { return (int)_minimum; }
        }

        public int DivisionNum
        {
            set
            {
                _numAxisDivision = value;
                Refresh();
            }
            get { return _numAxisDivision; }
        }

        public Color InnerColor
        {
            set
            {
                _colorInner = value;
                Refresh();
            }
            get { return _colorInner; }
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
            int h = Height;
            int w = Width;

            int baryoff, markyoff, tickyoff1, tickyoff2;
            
            double dtick;
            
            int tickpos;
            
            Pen penShadowLight = new Pen(_colorShadowLight);
            Pen penShadowDark = new Pen(_colorShadowDark);
            
            SolidBrush brushShadowLight = new SolidBrush(_colorShadowLight);
            SolidBrush brushShadowDark = new SolidBrush(_colorShadowDark);
            SolidBrush brushInner;
            SolidBrush brushRange = new SolidBrush(_colorRange);

            if (Enabled)
                brushInner = new SolidBrush(_colorInner);
            else
                brushInner = new SolidBrush(Color.FromKnownColor(KnownColor.InactiveCaption));

            // range
            _xPosMin = _markWidth + 1;
            if (_orientationBar == RangeBarOrientation.Horizontal)
                _xPosMax = w - _markWidth - 1;
            else
                _xPosMax = h - _markWidth - 1;

            // range check
            if (_pixelPosL < _xPosMin) _pixelPosL = _xPosMin;
            if (_pixelPosL > _xPosMax) _pixelPosL = _xPosMax;
            if (_pixelPosR > _xPosMax) _pixelPosR = _xPosMax;
            if (_pixelPosR < _xPosMin) _pixelPosR = _xPosMin;

            RangePos2PixelPos();


            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                baryoff = (h - _barHeight) / 2;
                markyoff = baryoff + (_barHeight - _markHeight) / 2 - 1;

                // total range bar frame			
                e.Graphics.FillRectangle(brushShadowDark, 0, baryoff, w - 1, sizeShadow);	// Top
                e.Graphics.FillRectangle(brushShadowDark, 0, baryoff, sizeShadow, _barHeight - 1);	// Left
                e.Graphics.FillRectangle(brushShadowLight, 0, baryoff + _barHeight - 1 - sizeShadow, w - 1, sizeShadow);	// Bottom
                e.Graphics.FillRectangle(brushShadowLight, w - 1 - sizeShadow, baryoff, sizeShadow, _barHeight - 1);	// Right

                // inner region
                e.Graphics.FillRectangle(brushInner, _pixelPosL, baryoff + sizeShadow, _pixelPosR - _pixelPosL, _barHeight - 1 - 2 * sizeShadow);

                // Skala
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    tickyoff1 = tickyoff2 = baryoff + _barHeight + 2;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    tickyoff1 = tickyoff2 = baryoff - _tickHeight - 4;
                }
                else
                {
                    tickyoff1 = baryoff + _barHeight + 2;
                    tickyoff2 = baryoff - _tickHeight - 4;
                }

                if (_numAxisDivision > 1)
                {
                    dtick = (double)(_xPosMax - _xPosMin) / _numAxisDivision;
                    for (int i = 0; i < _numAxisDivision + 1; i++)
                    {
                        tickpos = (int)Math.Round(i * dtick);
                        if (_orientationScale == TopBottomOrientation.Bottom
                            || _orientationScale == TopBottomOrientation.Both)
                        {
                            e.Graphics.DrawLine(penShadowDark, _markWidth + 1 + tickpos,
                                tickyoff1,
                                _markWidth + 1 + tickpos,
                                tickyoff1 + _tickHeight);
                        }
                        if (_orientationScale == TopBottomOrientation.Top
                            || _orientationScale == TopBottomOrientation.Both)
                        {
                            e.Graphics.DrawLine(penShadowDark, _markWidth + 1 + tickpos,
                                tickyoff2,
                                _markWidth + 1 + tickpos,
                                tickyoff2 + _tickHeight);
                        }
                    }
                }

                // Left mark knob			
                _lMarkPnt[0].X = _pixelPosL - _markWidth; _lMarkPnt[0].Y = markyoff + _markHeight / 3;
                _lMarkPnt[1].X = _pixelPosL; _lMarkPnt[1].Y = markyoff;
                _lMarkPnt[2].X = _pixelPosL; _lMarkPnt[2].Y = markyoff + _markHeight;
                _lMarkPnt[3].X = _pixelPosL - _markWidth; _lMarkPnt[3].Y = markyoff + 2 * _markHeight / 3;
                _lMarkPnt[4].X = _pixelPosL - _markWidth; _lMarkPnt[4].Y = markyoff;
                e.Graphics.FillPolygon(brushRange, _lMarkPnt);
                e.Graphics.DrawLine(penShadowDark, _lMarkPnt[3].X - 1, _lMarkPnt[3].Y, _lMarkPnt[1].X - 1, _lMarkPnt[2].Y); // lower Left shadow
                e.Graphics.DrawLine(penShadowLight, _lMarkPnt[0].X - 1, _lMarkPnt[0].Y, _lMarkPnt[0].X - 1, _lMarkPnt[3].Y); // Left shadow				
                e.Graphics.DrawLine(penShadowLight, _lMarkPnt[0].X - 1, _lMarkPnt[0].Y, _lMarkPnt[1].X - 1, _lMarkPnt[1].Y); // upper shadow
                if (_pixelPosL < _pixelPosR)
                    e.Graphics.DrawLine(penShadowDark, _lMarkPnt[1].X, _lMarkPnt[1].Y + 1, _lMarkPnt[1].X, _lMarkPnt[2].Y); // Right shadow
                if (_activeMark == ActiveMarkType.Left)
                {
                    e.Graphics.DrawLine(penShadowLight, _pixelPosL - _markWidth / 2 - 1, markyoff + _markHeight / 3, _pixelPosL - _markWidth / 2 - 1, markyoff + 2 * _markHeight / 3); // active mark
                    e.Graphics.DrawLine(penShadowDark, _pixelPosL - _markWidth / 2, markyoff + _markHeight / 3, _pixelPosL - _markWidth / 2, markyoff + 2 * _markHeight / 3); // active mark			
                }

                // Right mark knob
                _rMarkPnt[0].X = _pixelPosR + _markWidth; _rMarkPnt[0].Y = markyoff + _markHeight / 3;
                _rMarkPnt[1].X = _pixelPosR; _rMarkPnt[1].Y = markyoff;
                _rMarkPnt[2].X = _pixelPosR; _rMarkPnt[2].Y = markyoff + _markHeight;
                _rMarkPnt[3].X = _pixelPosR + _markWidth; _rMarkPnt[3].Y = markyoff + 2 * _markHeight / 3;
                _rMarkPnt[4].X = _pixelPosR + _markWidth; _rMarkPnt[4].Y = markyoff;
                e.Graphics.FillPolygon(brushRange, _rMarkPnt);
                if (_pixelPosL < _pixelPosR)
                    e.Graphics.DrawLine(penShadowLight, _rMarkPnt[1].X - 1, _rMarkPnt[1].Y + 1, _rMarkPnt[2].X - 1, _rMarkPnt[2].Y); // Left shadow
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[2].X, _rMarkPnt[2].Y, _rMarkPnt[3].X, _rMarkPnt[3].Y); // lower Right shadow
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[0].X, _rMarkPnt[0].Y, _rMarkPnt[1].X, _rMarkPnt[1].Y); // upper shadow
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[0].X, _rMarkPnt[0].Y + 1, _rMarkPnt[3].X, _rMarkPnt[3].Y); // Right shadow
                if (_activeMark == ActiveMarkType.Right)
                {
                    e.Graphics.DrawLine(penShadowLight, _pixelPosR + _markWidth / 2 - 1, markyoff + _markHeight / 3, _pixelPosR + _markWidth / 2 - 1, markyoff + 2 * _markHeight / 3); // active mark
                    e.Graphics.DrawLine(penShadowDark, _pixelPosR + _markWidth / 2, markyoff + _markHeight / 3, _pixelPosR + _markWidth / 2, markyoff + 2 * _markHeight / 3); // active mark				
                }

                if (_moveLMark)
                {
                    Font fontMark = new Font("Arial", _markWidth);
                    SolidBrush brushMark = new SolidBrush(_colorShadowDark);
                    StringFormat strformat = new StringFormat();
                    strformat.Alignment = StringAlignment.Center;
                    strformat.LineAlignment = StringAlignment.Near;
                    e.Graphics.DrawString(_rangeMin.ToString(), fontMark, brushMark, _pixelPosL, tickyoff1 + _tickHeight, strformat);
                }

                if (_moveRMark)
                {
                    Font fontMark = new Font("Arial", _markWidth);
                    SolidBrush brushMark = new SolidBrush(_colorShadowDark);
                    StringFormat strformat = new StringFormat();
                    strformat.Alignment = StringAlignment.Center;
                    strformat.LineAlignment = StringAlignment.Near;
                    e.Graphics.DrawString(_rangeMax.ToString(), fontMark, brushMark, _pixelPosR, tickyoff1 + _tickHeight, strformat);
                }

            }
            else // Vertical bar
            {
                baryoff = (w + _barHeight) / 2;
                markyoff = baryoff - _barHeight / 2 - _markHeight / 2;
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    tickyoff1 = tickyoff2 = baryoff + 2;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    tickyoff1 = tickyoff2 = baryoff - _barHeight - 2 - _tickHeight;
                }
                else
                {
                    tickyoff1 = baryoff + 2;
                    tickyoff2 = baryoff - _barHeight - 2 - _tickHeight;
                }

                // total range bar frame			
                e.Graphics.FillRectangle(brushShadowDark, baryoff - _barHeight, 0, _barHeight, sizeShadow);	// Top
                e.Graphics.FillRectangle(brushShadowDark, baryoff - _barHeight, 0, sizeShadow, h - 1);	// Left				
                e.Graphics.FillRectangle(brushShadowLight, baryoff, 0, sizeShadow, h - 1);	// Right
                e.Graphics.FillRectangle(brushShadowLight, baryoff - _barHeight, h - sizeShadow, _barHeight, sizeShadow);	// Bottom

                // inner region
                e.Graphics.FillRectangle(brushInner, baryoff - _barHeight + sizeShadow, _pixelPosL, _barHeight - 2 * sizeShadow, _pixelPosR - _pixelPosL);

                // Skala
                if (_numAxisDivision > 1)
                {
                    dtick = (double)(_xPosMax - _xPosMin) / _numAxisDivision;
                    for (int i = 0; i < _numAxisDivision + 1; i++)
                    {
                        tickpos = (int)Math.Round(i * dtick);
                        if (_orientationScale == TopBottomOrientation.Bottom || _orientationScale == TopBottomOrientation.Both)
                        {
                            e.Graphics.DrawLine(penShadowDark,
                                tickyoff1,
                                _markWidth + 1 + tickpos,
                                tickyoff1 + _tickHeight,
                                _markWidth + 1 + tickpos);
                        }
                        if (_orientationScale == TopBottomOrientation.Top || _orientationScale == TopBottomOrientation.Both)
                        {
                            e.Graphics.DrawLine(penShadowDark,
                                tickyoff2,
                                _markWidth + 1 + tickpos,
                                tickyoff2 + _tickHeight,
                                _markWidth + 1 + tickpos);
                        }
                    }
                }

                // Left(upper) mark knob				
                _lMarkPnt[0].Y = _pixelPosL - _markWidth; _lMarkPnt[0].X = markyoff + _markHeight / 3;
                _lMarkPnt[1].Y = _pixelPosL; _lMarkPnt[1].X = markyoff;
                _lMarkPnt[2].Y = _pixelPosL; _lMarkPnt[2].X = markyoff + _markHeight;
                _lMarkPnt[3].Y = _pixelPosL - _markWidth; _lMarkPnt[3].X = markyoff + 2 * _markHeight / 3;
                _lMarkPnt[4].Y = _pixelPosL - _markWidth; _lMarkPnt[4].X = markyoff;
                e.Graphics.FillPolygon(brushRange, _lMarkPnt);
                e.Graphics.DrawLine(penShadowDark, _lMarkPnt[3].X, _lMarkPnt[3].Y, _lMarkPnt[2].X, _lMarkPnt[2].Y); // Right shadow
                e.Graphics.DrawLine(penShadowLight, _lMarkPnt[0].X - 1, _lMarkPnt[0].Y, _lMarkPnt[3].X - 1, _lMarkPnt[3].Y); // Top shadow				
                e.Graphics.DrawLine(penShadowLight, _lMarkPnt[0].X - 1, _lMarkPnt[0].Y, _lMarkPnt[1].X - 1, _lMarkPnt[1].Y); // Left shadow
                if (_pixelPosL < _pixelPosR)
                    e.Graphics.DrawLine(penShadowDark, _lMarkPnt[1].X, _lMarkPnt[1].Y, _lMarkPnt[2].X, _lMarkPnt[2].Y); // lower shadow
                if (_activeMark == ActiveMarkType.Left)
                {
                    e.Graphics.DrawLine(penShadowLight, markyoff + _markHeight / 3, _pixelPosL - _markWidth / 2, markyoff + 2 * _markHeight / 3, _pixelPosL - _markWidth / 2); // active mark
                    e.Graphics.DrawLine(penShadowDark, markyoff + _markHeight / 3, _pixelPosL - _markWidth / 2 + 1, markyoff + 2 * _markHeight / 3, _pixelPosL - _markWidth / 2 + 1); // active mark			
                }

                // Right mark knob
                _rMarkPnt[0].Y = _pixelPosR + _markWidth; _rMarkPnt[0].X = markyoff + _markHeight / 3;
                _rMarkPnt[1].Y = _pixelPosR; _rMarkPnt[1].X = markyoff;
                _rMarkPnt[2].Y = _pixelPosR; _rMarkPnt[2].X = markyoff + _markHeight;
                _rMarkPnt[3].Y = _pixelPosR + _markWidth; _rMarkPnt[3].X = markyoff + 2 * _markHeight / 3;
                _rMarkPnt[4].Y = _pixelPosR + _markWidth; _rMarkPnt[4].X = markyoff;
                e.Graphics.FillPolygon(brushRange, _rMarkPnt);
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[2].X, _rMarkPnt[2].Y, _rMarkPnt[3].X, _rMarkPnt[3].Y); // lower Right shadow
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[0].X, _rMarkPnt[0].Y, _rMarkPnt[1].X, _rMarkPnt[1].Y); // upper shadow
                e.Graphics.DrawLine(penShadowDark, _rMarkPnt[0].X, _rMarkPnt[0].Y, _rMarkPnt[3].X, _rMarkPnt[3].Y); // Right shadow
                if (_pixelPosL < _pixelPosR)
                    e.Graphics.DrawLine(penShadowLight, _rMarkPnt[1].X, _rMarkPnt[1].Y, _rMarkPnt[2].X, _rMarkPnt[2].Y); // Left shadow
                if (_activeMark == ActiveMarkType.Right)
                {
                    e.Graphics.DrawLine(penShadowLight, markyoff + _markHeight / 3, _pixelPosR + _markWidth / 2 - 1, markyoff + 2 * _markHeight / 3, _pixelPosR + _markWidth / 2 - 1); // active mark
                    e.Graphics.DrawLine(penShadowDark, markyoff + _markHeight / 3, _pixelPosR + _markWidth / 2, markyoff + 2 * _markHeight / 3, _pixelPosR + _markWidth / 2); // active mark				
                }

                if (_moveLMark)
                {
                    Font fontMark = new Font("Arial", _markWidth);
                    SolidBrush brushMark = new SolidBrush(_colorShadowDark);
                    StringFormat strformat = new StringFormat();
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(_rangeMin.ToString(), fontMark, brushMark, tickyoff1 + _tickHeight + 2, _pixelPosL, strformat);
                }

                if (_moveRMark)
                {
                    Font fontMark = new Font("Arial", _markWidth);
                    SolidBrush brushMark = new SolidBrush(_colorShadowDark);
                    StringFormat strformat = new StringFormat();
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(_rangeMax.ToString(), fontMark, brushMark, tickyoff1 + _tickHeight, _pixelPosR, strformat);
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Rectangle lMarkRect = new Rectangle(
                    Math.Min(_lMarkPnt[0].X, _lMarkPnt[1].X),		// X
                    Math.Min(_lMarkPnt[0].Y, _lMarkPnt[3].Y),		// Y
                    Math.Abs(_lMarkPnt[2].X - _lMarkPnt[0].X),	// width
                    Math.Max(Math.Abs(_lMarkPnt[0].Y - _lMarkPnt[3].Y), Math.Abs(_lMarkPnt[0].Y - _lMarkPnt[1].Y)));	// height
                Rectangle rMarkRect = new Rectangle(
                    Math.Min(_rMarkPnt[0].X, _rMarkPnt[2].X),		// X
                    Math.Min(_rMarkPnt[0].Y, _rMarkPnt[1].Y),		// Y
                    Math.Abs(_rMarkPnt[0].X - _rMarkPnt[2].X),	// width
                    Math.Max(Math.Abs(_rMarkPnt[2].Y - _rMarkPnt[0].Y), Math.Abs(_rMarkPnt[1].Y - _rMarkPnt[0].Y)));		// height

                if (lMarkRect.Contains(e.X, e.Y))
                {
                    Capture = true;
                    _moveLMark = true;
                    _activeMark = ActiveMarkType.Left;
                    Invalidate(true);
                }

                if (rMarkRect.Contains(e.X, e.Y))
                {
                    Capture = true;
                    _moveRMark = true;
                    _activeMark = ActiveMarkType.Right;
                    Invalidate(true);
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Capture = false;

                _moveLMark = false;
                _moveRMark = false;

                Invalidate();

                OnRangeChanged(EventArgs.Empty);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Enabled)
            {
                Rectangle lMarkRect = new Rectangle(
                    Math.Min(_lMarkPnt[0].X, _lMarkPnt[1].X),		// X
                    Math.Min(_lMarkPnt[0].Y, _lMarkPnt[3].Y),		// Y
                    Math.Abs(_lMarkPnt[2].X - _lMarkPnt[0].X),		// width
                    Math.Max(Math.Abs(_lMarkPnt[0].Y - _lMarkPnt[3].Y), Math.Abs(_lMarkPnt[0].Y - _lMarkPnt[1].Y)));	// height
                Rectangle rMarkRect = new Rectangle(
                    Math.Min(_rMarkPnt[0].X, _rMarkPnt[2].X),		// X
                    Math.Min(_rMarkPnt[0].Y, _rMarkPnt[1].Y),		// Y
                    Math.Abs(_rMarkPnt[0].X - _rMarkPnt[2].X),		// width
                    Math.Max(Math.Abs(_rMarkPnt[2].Y - _rMarkPnt[0].Y), Math.Abs(_rMarkPnt[1].Y - _rMarkPnt[0].Y)));		// height

                if (lMarkRect.Contains(e.X, e.Y) || rMarkRect.Contains(e.X, e.Y))
                {
                    if (_orientationBar == RangeBarOrientation.Horizontal)
                        Cursor = Cursors.SizeWE;
                    else
                        Cursor = Cursors.SizeNS;
                }
                else Cursor = Cursors.Arrow;

                if (_moveLMark)
                {
                    if (_orientationBar == RangeBarOrientation.Horizontal)
                        Cursor = Cursors.SizeWE;
                    else
                        Cursor = Cursors.SizeNS;
                    if (_orientationBar == RangeBarOrientation.Horizontal)
                        _pixelPosL = e.X;
                    else
                        _pixelPosL = e.Y;
                    if (_pixelPosL < _xPosMin)
                        _pixelPosL = _xPosMin;
                    if (_pixelPosL > _xPosMax)
                        _pixelPosL = _xPosMax;
                    if (_pixelPosR < _pixelPosL)
                        _pixelPosR = _pixelPosL;
                    PixelPos2RangePos();
                    _activeMark = ActiveMarkType.Left;
                    Invalidate(true);

                    OnRangeChanging(EventArgs.Empty);
                }
                else if (_moveRMark)
                {
                    if (_orientationBar == RangeBarOrientation.Horizontal)
                        Cursor = Cursors.SizeWE;
                    else
                        Cursor = Cursors.SizeNS;
                    if (_orientationBar == RangeBarOrientation.Horizontal)
                        _pixelPosR = e.X;
                    else
                        _pixelPosR = e.Y;
                    if (_pixelPosR > _xPosMax)
                        _pixelPosR = _xPosMax;
                    if (_pixelPosR < _xPosMin)
                        _pixelPosR = _xPosMin;
                    if (_pixelPosL > _pixelPosR)
                        _pixelPosL = _pixelPosR;
                    PixelPos2RangePos();
                    _activeMark = ActiveMarkType.Right;
                    Invalidate(true);

                    OnRangeChanging(EventArgs.Empty);
                }
            }
        }

        private void PixelPos2RangePos()
        {
            int w;
            int posw;

            if (_orientationBar == RangeBarOrientation.Horizontal)
                w = Width;
            else
                w = Height;
            posw = w - 2 * _markWidth - 2;

            _rangeMin = _minimum + (int)Math.Round((_maximum - _minimum) * (_pixelPosL - _xPosMin) / posw);
            _rangeMax = _minimum + (int)Math.Round((_maximum - _minimum) * (_pixelPosR - _xPosMin) / posw);
        }

        private void RangePos2PixelPos()
        {
            int w;
            int posw;

            if (_orientationBar == RangeBarOrientation.Horizontal)
                w = Width;
            else
                w = Height;
            posw = w - 2 * _markWidth - 2;

            _pixelPosL = _xPosMin + (int)Math.Round(posw * (_rangeMin - _minimum) / (_maximum - _minimum));
            _pixelPosR = _xPosMin + (int)Math.Round(posw * (_rangeMax - _minimum) / (_maximum - _minimum));
        }

        private void OnResize(object sender, EventArgs e)
        {
            //RangePos2PixelPos();
            Invalidate(true);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (Enabled)
            {
                if (_activeMark == ActiveMarkType.Left)
                {
                    if (e.KeyChar == '+')
                    {
                        _rangeMin++;
                        if (_rangeMin > _maximum)
                            _rangeMin = _maximum;
                        if (_rangeMax < _rangeMin)
                            _rangeMax = _rangeMin;
                        OnRangeChanged(EventArgs.Empty);
                    }
                    else if (e.KeyChar == '-')
                    {
                        _rangeMin--;
                        if (_rangeMin < _minimum)
                            _rangeMin = _minimum;
                        OnRangeChanged(EventArgs.Empty);
                    }
                }
                else if (_activeMark == ActiveMarkType.Right)
                {
                    if (e.KeyChar == '+')
                    {
                        _rangeMax++;

                        if (_rangeMax > _maximum)
                        {
                            _rangeMax = _maximum;
                        }

                        OnRangeChanged(EventArgs.Empty);
                    }
                    else if (e.KeyChar == '-')
                    {
                        _rangeMax--;

                        if (_rangeMax < _minimum)
                        {
                            _rangeMax = _minimum;
                        }

                        if (_rangeMax < _rangeMin)
                        {
                            _rangeMin = _rangeMax;
                        }
                        
                        OnRangeChanged(EventArgs.Empty);
                    }
                }
                Invalidate(true);
            }
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
            _activeMark = ActiveMarkType.None;
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
}
