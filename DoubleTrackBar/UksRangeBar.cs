/*
 * This custom control is customized by Andrew Vynnychenko
 * and based on ZzzzRangeBar by Detlef Neunherz
 */

using System;
using System.Collections.Generic;
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

        private Point[] _lMarkPnt = new Point[4];
        private Point[] _rMarkPnt = new Point[4];

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

            int barOffset, markOffset, tickyoff1, tickyoff2;
            
            double deltaTick;
            
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
            _xPosMax = (_orientationBar == RangeBarOrientation.Horizontal) ?(w - _markWidth - 1)  : (h - _markWidth - 1);

            // range check
            if (_pixelPosL < _xPosMin) _pixelPosL = _xPosMin;
            if (_pixelPosL > _xPosMax) _pixelPosL = _xPosMax;
            if (_pixelPosR > _xPosMax) _pixelPosR = _xPosMax;
            if (_pixelPosR < _xPosMin) _pixelPosR = _xPosMin;

            RangePos2PixelPos();

            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                barOffset = (h - _barHeight) / 2;
                markOffset = barOffset + (_barHeight - _markHeight) / 2 - 1;

                // total range bar frame			
                e.Graphics.FillRectangle(brushShadowDark, 0, barOffset, w - 1, sizeShadow);	// Top
                e.Graphics.FillRectangle(brushShadowDark, 0, barOffset, sizeShadow, _barHeight - 1);	// Left
                e.Graphics.FillRectangle(brushShadowLight, 0, barOffset + _barHeight - 1 - sizeShadow, w - 1, sizeShadow);	// Bottom
                e.Graphics.FillRectangle(brushShadowLight, w - 1 - sizeShadow, barOffset, sizeShadow, _barHeight - 1);	// Right

                // inner region
                e.Graphics.FillRectangle(brushInner, _pixelPosL, barOffset + sizeShadow, _pixelPosR - _pixelPosL, _barHeight - 1 - 2 * sizeShadow);

                // Skala
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    tickyoff1 = tickyoff2 = barOffset + _barHeight + 2;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    tickyoff1 = tickyoff2 = barOffset - _tickHeight - 4;
                }
                else
                {
                    tickyoff1 = barOffset + _barHeight + 2;
                    tickyoff2 = barOffset - _tickHeight - 4;
                }

                if (_numAxisDivision > 1)
                {
                    deltaTick = (double)(_xPosMax - _xPosMin) / _numAxisDivision;
                    for (int i = 0; i < _numAxisDivision + 1; i++)
                    {
                        tickpos = (int)Math.Round(i * deltaTick);
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

                RecalcKnobsPos(markOffset, ref _lMarkPnt, _pixelPosL);
                RecalcKnobsPos(markOffset, ref _rMarkPnt, _pixelPosR);
                
                PaintKnob(e, _lMarkPnt, _pixelPosL, brushRange, penShadowLight, penShadowDark, markOffset);
                PaintKnob(e, _rMarkPnt, _pixelPosR, brushRange, penShadowLight, penShadowDark, markOffset);

                ShowTextOfCurrPosition(e, tickyoff1);
            }
            else // Vertical bar
            {
                barOffset = (w + _barHeight) / 2;
                markOffset = barOffset - _barHeight / 2 - _markHeight / 2;
                if (_orientationScale == TopBottomOrientation.Bottom)
                {
                    tickyoff1 = tickyoff2 = barOffset + 2;
                }
                else if (_orientationScale == TopBottomOrientation.Top)
                {
                    tickyoff1 = tickyoff2 = barOffset - _barHeight - 2 - _tickHeight;
                }
                else
                {
                    tickyoff1 = barOffset + 2;
                    tickyoff2 = barOffset - _barHeight - 2 - _tickHeight;
                }

                // total range bar frame			
                e.Graphics.FillRectangle(brushShadowDark, barOffset - _barHeight, 0, _barHeight, sizeShadow);	// Top
                e.Graphics.FillRectangle(brushShadowDark, barOffset - _barHeight, 0, sizeShadow, h - 1);	// Left				
                e.Graphics.FillRectangle(brushShadowLight, barOffset, 0, sizeShadow, h - 1);	// Right
                e.Graphics.FillRectangle(brushShadowLight, barOffset - _barHeight, h - sizeShadow, _barHeight, sizeShadow);	// Bottom

                // inner region
                e.Graphics.FillRectangle(brushInner, barOffset - _barHeight + sizeShadow, _pixelPosL, _barHeight - 2 * sizeShadow, _pixelPosR - _pixelPosL);

                // Skala
                if (_numAxisDivision > 1)
                {
                    deltaTick = (double)(_xPosMax - _xPosMin) / _numAxisDivision;
                    for (int i = 0; i < _numAxisDivision + 1; i++)
                    {
                        tickpos = (int)Math.Round(i * deltaTick);
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

                RecalcKnobsPos(markOffset, ref _lMarkPnt, _pixelPosL);
                RecalcKnobsPos(markOffset, ref _rMarkPnt, _pixelPosR);

                PaintKnob(e, _lMarkPnt, _pixelPosL, brushRange, penShadowLight, penShadowDark, markOffset);
                PaintKnob(e, _rMarkPnt, _pixelPosR, brushRange, penShadowLight, penShadowDark, markOffset);

                ShowTextOfCurrPosition(e, tickyoff1);
            }
        }

        private void RecalcKnobsPos(int markOffset, ref Point[] pos, int pixelPos)
        {
            int markWidth = (pos == _lMarkPnt) ? _markWidth : -_markWidth;

            var offsetX = markWidth / 2;

            pos[0].X = pixelPos - offsetX; pos[0].Y = markOffset + _markHeight / 50;
            pos[1].X = pixelPos + offsetX; pos[1].Y = markOffset;
            pos[2].X = pixelPos + offsetX; pos[2].Y = markOffset + _markHeight;
            pos[3].X = pixelPos - offsetX; pos[3].Y = markOffset + _markHeight;

            if (_orientationBar == RangeBarOrientation.Vertical)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    var tmp = pos[i].Y;
                    pos[i].Y = pos[i].X;
                    pos[i].X = tmp;
                }
            }

            // Lame fix for the same positions of points in both L and R arrays
            var posToDraw = new List<Point>();
            if (pos == _rMarkPnt)
            {
                posToDraw.Add(pos[1]);
                posToDraw.Add(pos[0]);
                posToDraw.Add(pos[3]);
                posToDraw.Add(pos[2]);

                pos = posToDraw.ToArray();
            }
        }

        private void PaintKnob(PaintEventArgs e, Point[] pos, int pixelPos, Brush brushRange, Pen penShadowLight, Pen penShadowDark, int markOffset)
        {
            e.Graphics.FillPolygon(brushRange, pos);

            //Draw line in the middle of Knoob
            if (_orientationBar == RangeBarOrientation.Horizontal)
            {
                e.Graphics.DrawLine(penShadowLight, pixelPos, markOffset + _markHeight, pixelPos,
                    markOffset + _markHeight);
                e.Graphics.DrawLine(penShadowDark, pixelPos, markOffset + _markHeight / 3, pixelPos,
                    markOffset + 2 * _markHeight / 3);
            }
            else
            {
                e.Graphics.DrawLine(penShadowLight, markOffset + _markHeight / 3, pixelPos, 
                    markOffset + 2 * _markHeight / 3, pixelPos);
                e.Graphics.DrawLine(penShadowDark, markOffset + _markHeight / 3, pixelPos, 
                    markOffset + 2 * _markHeight / 3, pixelPos);                
            }


            e.Graphics.DrawLine(penShadowLight, pos[0], pos[1]); // upper shadow
            e.Graphics.DrawLine(penShadowDark, pos[1], pos[2]); // Right shadow
            e.Graphics.DrawLine(penShadowDark, pos[2], pos[3]); // lower shadow
            e.Graphics.DrawLine(penShadowLight, pos[3], pos[0]); // Left shadow
        }

        private void ShowTextOfCurrPosition(PaintEventArgs e, int tickOffset)
        {
            Font fontMark = new Font("Arial", _markWidth);
            SolidBrush brushMark = new SolidBrush(_colorShadowDark);
            StringFormat strformat = new StringFormat();

            if (_moveLMark)
            {
                if (_orientationBar == RangeBarOrientation.Horizontal)
                {
                    strformat.Alignment = StringAlignment.Center;
                    strformat.LineAlignment = StringAlignment.Near;
                    e.Graphics.DrawString(_rangeMin.ToString(), fontMark, brushMark, _pixelPosL, tickOffset + _tickHeight, strformat);
                }
                else
                {
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(_rangeMin.ToString(), fontMark, brushMark, tickOffset + _tickHeight + 2, _pixelPosL, strformat);
                }
            }

            if (_moveRMark)
            {
                if (_orientationBar == RangeBarOrientation.Horizontal)
                {
                    strformat.Alignment = StringAlignment.Center;
                    strformat.LineAlignment = StringAlignment.Near;
                    e.Graphics.DrawString(_rangeMax.ToString(), fontMark, brushMark, _pixelPosR, tickOffset + _tickHeight, strformat);
                }
                else
                {
                    strformat.Alignment = StringAlignment.Near;
                    strformat.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(_rangeMax.ToString(), fontMark, brushMark, tickOffset + _tickHeight, _pixelPosR, strformat);
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
