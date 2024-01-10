using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace milano88.UI.Controls
{
    public class MSImageButton : Control
    {
        private BufferedGraphics _bufGraphics;
        private bool _isMouseOver = false;
        private bool _isMouseDown = false;
        private Rectangle _imageRect;

        public MSImageButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(120, 35);
            BackColor = Color.Transparent;
            UpdateGraphicsBuffer();
        }

        private Image _imageNormal;
        [Category("Custom Properties")]
        [DefaultValue(null)]
        public Image ImageNormal
        {
            get => _imageNormal;
            set
            {
                _imageNormal = value;
                if (_imageNormal != null)
                {
                    _imageRect = new Rectangle(Point.Empty, _imageNormal.Size);
                    this.Size = _imageRect.Size;
                }
                else
                {
                    Size = new Size(120, 35);
                    _imageRect = new Rectangle(Point.Empty, Size.Empty);
                }
                Invalidate();
            }
        }

        private Image _imageHover;
        [Category("Custom Properties")]
        [DefaultValue(null)]
        public Image ImageHover
        {
            get => _imageHover;
            set
            {
                _imageHover = value;
                Invalidate();
            }
        }

        private Image _imageDown;
        [Category("Custom Properties")]
        [DefaultValue(null)]
        public Image ImageDown
        {
            get => _imageDown;
            set
            {
                _imageDown = value;
                Invalidate();
            }
        }

        private int _imageOpacity = 100;
        [Category("Custom Properties")]
        [DefaultValue(100)]
        public int Opacity
        {
            get { return _imageOpacity; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value must be less than or equal to 100 and greater than or equal to 1");

                bool changed = value != _imageOpacity;
                if (changed)
                {
                    _imageOpacity = value;
                    Invalidate();
                }
            }
        }

        private Size _hoverImageSize;
        [Category("Custom Properties")]
        [DefaultValue(typeof(Size), "0,0")]
        public Size HoverImageSize
        {
            get { return _hoverImageSize; }
            set { _hoverImageSize = value; Invalidate(); }
        }

        private int _rotate = 0;
        [Category("Custom Properties")]
        [DefaultValue(0)]
        public int Rotate
        {
            get { return _rotate; }
            set
            {
                if (value < 0 || value > 360)
                    throw new ArgumentOutOfRangeException("value must be less than or equal to 360 and greater than or equal to 0");

                bool changed = value != _rotate;
                if (changed)
                {
                    _rotate = value;
                    Invalidate();
                }
            }
        }


        [Browsable(false)]
        public override Image BackgroundImage { get => base.BackgroundImage; set { } }
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout { get => base.BackgroundImageLayout; set { } }
        [Browsable(false)]
        public override Font Font { get => base.Font; set { } }
        [Browsable(false)]
        public override string Text { get => base.Text; set { } }
        [Browsable(false)]
        public override Color ForeColor { get => base.ForeColor; set { } }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_imageRect.Width > 0)
            {
                if (!_isMouseOver)
                    _bufGraphics.Graphics.DrawImage(SetImageOpacity(_imageNormal, (float)_imageOpacity / 100), _imageRect);

                if (_isMouseOver && !_isMouseDown)
                {
                    if (_imageHover != null)
                    {
                        if (_hoverImageSize.Width > 0 && _hoverImageSize.Height > 0)
                            _bufGraphics.Graphics.DrawImage(SetImageOpacity(_imageHover, (float)_imageOpacity / 100),
                                (_imageRect.Width / 2) - (_hoverImageSize.Width / 2), (_imageRect.Height / 2) - (_hoverImageSize.Height / 2),
                                _hoverImageSize.Width, _hoverImageSize.Height);
                        else _bufGraphics.Graphics.DrawImage(SetImageOpacity(_imageHover, (float)_imageOpacity / 100), _imageRect);
                    }
                    else _bufGraphics.Graphics.DrawImage(SetImageOpacity(_imageNormal, (float)_imageOpacity / 100), _imageRect);
                }

                if (_isMouseDown)
                {
                    if (_imageDown != null)
                        _bufGraphics.Graphics.DrawImage(SetImageOpacity(_imageDown, (float)_imageOpacity / 100), _imageRect);
                }
            }
            else
            {
                _bufGraphics.Graphics.FillRectangle(Brushes.White, ClientRectangle);
                TextRenderer.DrawText(_bufGraphics.Graphics, "please load image...", new Font("Segoe UI", 9F), ClientRectangle, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            _bufGraphics.Render(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (Parent != null && BackColor == Color.Transparent)
            {
                Rectangle rect = new Rectangle(Left, Top, Width, Height);
                _bufGraphics.Graphics.TranslateTransform(-rect.X, -rect.Y);
                try
                {
                    using (PaintEventArgs pea = new PaintEventArgs(_bufGraphics.Graphics, rect))
                    {
                        pea.Graphics.SetClip(rect);
                        InvokePaintBackground(Parent, pea);
                        InvokePaint(Parent, pea);
                    }
                }
                finally
                {
                    _bufGraphics.Graphics.TranslateTransform(rect.X, rect.Y);
                }
            }
            else
            {
                using (SolidBrush backColor = new SolidBrush(this.BackColor))
                    _bufGraphics.Graphics.FillRectangle(backColor, ClientRectangle);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_imageRect.Width > 0)
                this.Size = _imageRect.Size;

            UpdateGraphicsBuffer();
            base.OnSizeChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (_isMouseDown) return;
            _isMouseOver = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isMouseOver = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _isMouseDown = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isMouseDown = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        private void UpdateGraphicsBuffer()
        {
            if (Width > 0 && Height > 0)
            {
                BufferedGraphicsContext context = BufferedGraphicsManager.Current;
                context.MaximumBuffer = new Size(Width + 1, Height + 1);
                _bufGraphics = context.Allocate(CreateGraphics(), ClientRectangle);
                _bufGraphics.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                _bufGraphics.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                _bufGraphics.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }
        }

        private Bitmap SetImageOpacity(Image image, float opacityvalue)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                ColorMatrix colormatrix = new ColorMatrix { Matrix33 = opacityvalue };
                ImageAttributes imgAttribute = new ImageAttributes();
                imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttribute);
            }
            return RotateImage(bmp, _rotate);
        }

        private Bitmap RotateImage(Image image, float angle)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TranslateTransform((float)image.Width / 2, (float)image.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-(float)image.Width / 2, -(float)image.Height / 2);
                graphics.DrawImage(image, new PointF(0, 0));
            }
            return bmp;
        }
    }
}
