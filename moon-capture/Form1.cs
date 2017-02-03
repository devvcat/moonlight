using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Moonlight.Helpers;
using Moonlight.Helpers.Unmanaged;

namespace Moonlight
{
    public partial class Form1 : Form
    {
        const int TOP_BLOCK_COUNT = 20;
        const int SIDE_BLOCK_COUNT = 10;

        private Bitmap screenShot = null;

        private Boolean mouseDown = false;
        private Point mousePosition = new Point(0, 0);
        private Rectangle captureRectangle = Rectangle.Empty;

        private Brush overlayBrush = new SolidBrush(Color.FromArgb(50, Color.OrangeRed));
        private Pen overlayPen = new Pen(Color.OrangeRed, 2f);

        private Rectangle screenBounds = Rectangle.Empty;

        private enum CaptureType
        {
            Region,
            Window
        }

        private int hotkeyCounter = 0;

        public Form1()
        {
            InitializeComponent();

            InitCapture(CaptureType.Region);

            // Register hotkeys
            this.UnRegisterHotkeys();
            this.RegisterHotkey("ALT+P");
            this.RegisterHotkey("ALT+O");
        }

        private void InitCapture(CaptureType captureType)
        {
            try
            {
                base.SuspendLayout();
                base.Visible = false;
                base.Bounds = Utils.GetScreenBounds();

#if DEBUG
                if (captureType == CaptureType.Region)
                {
                    System.Threading.Thread.Sleep(500);
                }
#endif

                // take screen shot of the the screens
                screenShot = WindowCapture.CaptureWindow();

                if (screenShot != null)
                {
                    pbCapture.Image = screenShot;

                    base.ResumeLayout(false);
                    base.Visible = true;
                    base.TopMost = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                base.Close();
            }
        }

        #region PictureBox Paint Events

        private void pbCapture_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.screenBounds = Utils.GetScreenBounds();

                this.mouseDown = true;
                this.mousePosition = new Point(e.X, e.Y);
                this.pbCapture_MouseMove(pbCapture, e);
            }
        }

        private void pbCapture_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseDown)
            {
                var rectangle = Utils.GetGuiRectangle(e.X + base.Left, e.Y + base.Top, mousePosition.X - e.X, mousePosition.Y - e.Y);
                this.captureRectangle = rectangle;

                pbCapture.Invalidate();
            }
        }

        private void pbCapture_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.mouseDown)
            {
                this.mouseDown = false;
                pbCapture.Refresh();


                var blockWidth = this.captureRectangle.Width / TOP_BLOCK_COUNT;
                var blockHeight = this.captureRectangle.Height / SIDE_BLOCK_COUNT;

                if (blockHeight < 10 || blockWidth < 10)
                {
                    return;
                }

                // create memory bitmat that will hold the captured region
                var bitmap = new Bitmap(
                    blockWidth * TOP_BLOCK_COUNT, blockHeight * SIDE_BLOCK_COUNT);

                // take only the region of the capture where the average color will be calculated
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.DrawImage(
                        this.screenShot, 
                        new Rectangle(0, 0, this.captureRectangle.Width, this.captureRectangle.Height), 
                        this.captureRectangle, 
                        GraphicsUnit.Pixel);
                    
                    g.Flush();
                }

                DivideImageToRegions(bitmap, blockWidth, blockHeight);
            }
        }

        private void pbCapture_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            int margin = 8, offset = 3;

            if (this.mouseDown)
            {
                this.captureRectangle.Intersect(this.screenBounds);

                var rectangle = new Rectangle(this.captureRectangle.X + Math.Abs(screenBounds.X),
                    this.captureRectangle.Y + Math.Abs(screenBounds.Y), this.captureRectangle.Width, this.captureRectangle.Height);

                graphics.FillRectangle(overlayBrush, rectangle);
                graphics.DrawRectangle(overlayPen, rectangle);

                var pen = new Pen(Color.White);
                var solidBrush = new SolidBrush(Color.OrangeRed);
                var font = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);

                var w = TextRenderer.MeasureText(this.captureRectangle.Width.ToString(), font).Width + offset;
                var h = TextRenderer.MeasureText(this.captureRectangle.Height.ToString(), font).Height + offset;

                // draw dimentions on top the region rectangle
                if (rectangle.Width > w)
                {
                    //
                    // |------------------- 640 ----------------------|
                    //

                    // draw the backgroung of the dimention
                    var textRect = new Rectangle(rectangle.X + (rectangle.Width / 2 - w / 2) + offset, rectangle.Y - 2 * margin + 1, w - 2 * offset, h - offset - 1);
                    graphics.FillRectangle(solidBrush, textRect);
                    graphics.DrawRectangle(pen, textRect);

                    // draw width
                    graphics.DrawString((this.captureRectangle.Width + 1).ToString(), font, pen.Brush, (float)(rectangle.X + (rectangle.Width / 2 - w / 2) + offset), (float)(rectangle.Y - 2 * margin + 1));

                    // left horizontal line
                    graphics.DrawLine(pen, rectangle.X, rectangle.Y - margin, rectangle.X + (rectangle.Width / 2 - w / 2), rectangle.Y - margin);

                    // right horizontal line
                    graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2 + w / 2, rectangle.Y - margin, rectangle.X + rectangle.Width, rectangle.Y - margin);

                    // left vertical line
                    graphics.DrawLine(pen, rectangle.X, rectangle.Y - margin - offset, rectangle.X, rectangle.Y - margin + offset);

                    // right vertical line
                    graphics.DrawLine(pen, rectangle.X + rectangle.Width, rectangle.Y - margin - offset, rectangle.X + rectangle.Width, rectangle.Y - margin + offset);
                }

                // draw dimentions on the left of the region rectangle
                if (rectangle.Height > h)
                {
                    //
                    //  _
                    //  |
                    //  |
                    // 480
                    //  |
                    //  |
                    //  -
                    // 

                    int width = TextRenderer.MeasureText(this.captureRectangle.Height.ToString(), font).Width;

                    // draw the background of the dimention
                    var textRect = new Rectangle(rectangle.X - width, rectangle.Y + (rectangle.Height / 2 - h / 2) + 2, width - offset, h - offset - 1);
                    graphics.FillRectangle(solidBrush, textRect);
                    graphics.DrawRectangle(pen, textRect);

                    // draw height
                    graphics.DrawString((rectangle.Height + 1).ToString(), font, pen.Brush, (float)(rectangle.X - width + 1), (float)(rectangle.Y + (rectangle.Height / 2 - h / 2) + 2));

                    // top vertical line 
                    graphics.DrawLine(pen, rectangle.X - margin, rectangle.Y, rectangle.X - margin, rectangle.Y + (rectangle.Height / 2 - h / 2));

                    // bottom vertical line
                    graphics.DrawLine(pen, rectangle.X - margin, rectangle.Y + rectangle.Height / 2 + h / 2, rectangle.X - margin, rectangle.Y + rectangle.Height);

                    // top horizontal line
                    graphics.DrawLine(pen, rectangle.X - margin - 3, rectangle.Y, rectangle.X - margin + 3, rectangle.Y);

                    // bottom horizontal line
                    graphics.DrawLine(pen, rectangle.X - margin - 3, rectangle.Y + rectangle.Height, rectangle.X - margin + 3, rectangle.Y + rectangle.Height);
                }

                // draw the dementions on the center of the overlay rectangle
                font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Bold);
                string str1 = string.Format("{0} x {1}", rectangle.Width + 1, rectangle.Height + 1); //string.Concat((int)Utilityhelper.GetDIPDependentHorizontal((double)this.captureRect.Width), " x ", (int)Utilityhelper.GetDIPDependentVertical((double)this.captureRect.Height));
                SizeF sizeF = graphics.MeasureString(str1, font);
                float rh = (float)rectangle.Height / (sizeF.Height * 2);
                float rw = (float)rectangle.Width / (sizeF.Width * 2);
                float propSize = font.Size * (rh < rw ? rh : rw);
                if (propSize >= 4)
                {
                    if (propSize > 20f)
                    {
                        propSize = 20f;
                    }

                    font = new Font(FontFamily.GenericSansSerif, propSize, FontStyle.Bold);

                    int x = (rectangle.X + rectangle.Width / 2) - (TextRenderer.MeasureText(str1, font).Width / 2);
                    graphics.DrawString(str1, font, Brushes.WhiteSmoke, new PointF(x, (float)(rectangle.Y + rectangle.Height / 2f) - font.GetHeight() / 2f));
                }
            }
        }

        #endregion

        #region Hotkeys

        private void RegisterHotkey(string hotkey)
        {
            try
            {
                // Format <modifier> + <key>
                var keys = hotkey.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

                var modifier = Hotkey.GetKey(keys[0]);
                var key = Hotkey.GetKey(keys[1]);

                if (!RegisterHotkey(modifier, key))
                {
                    throw new Exception(
                        string.Format("Fail to register hotkey {0}", hotkey));
                }
            }
            catch (Exception ex)
            {
                // TODO: log the error
            }
        }

        private bool RegisterHotkey(int modifier, int key)
        {
            var success = User32.RegisterHotKey(
                this.Handle.ToInt32(), this.hotkeyCounter, modifier, key) != 0;

            if (success) { this.hotkeyCounter += 1; }
            
            return success;
        }

        private void UnRegisterHotkeys()
        {
            for (var id=0; id < this.hotkeyCounter; id++)
            {
                User32.UnregisterHotKey(this.Handle.ToInt32(), id);
            }
            this.hotkeyCounter = 0;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x312;

            if (m.Msg == WM_HOTKEY)
            {
                var param = m.WParam.ToString();
                Debug.WriteLine(param);
            }

            m.Result = IntPtr.Zero;
            base.WndProc(ref m);
        }

        #endregion

        // Devide image in regions
        public void DivideImageToRegions(Bitmap bitmap, int blockWidth, int blockHeight)
        {
            const byte RED = 0;
            const byte GREEN = 1;
            const byte BLUE = 2;
            const int BYTES_PER_PIXEL = 3;

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(rect, 
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var stride = bitmapData.Stride;
            var pixelBuffer = new byte[bitmapData.Height * stride];

            var topArr = new int[TOP_BLOCK_COUNT + 1, BYTES_PER_PIXEL];
            var leftArr = new int[SIDE_BLOCK_COUNT + 1, BYTES_PER_PIXEL];
            var rightArr = new int[SIDE_BLOCK_COUNT + 1, BYTES_PER_PIXEL];

            Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            bitmap.UnlockBits(bitmapData);

            var w = bitmap.Width;
            var h = bitmap.Height;

            for (var row = 0; row < h; row++)
            {
                var pxl = 0;

                var blockX = 0;
                var blockY = 0;

                while (pxl < w)
                {
                    blockX = pxl / blockWidth;
                    blockY = row / blockHeight;

                    if (blockX >= TOP_BLOCK_COUNT) blockX = TOP_BLOCK_COUNT;
                    if (blockY >= SIDE_BLOCK_COUNT) blockY = SIDE_BLOCK_COUNT;

                    var offset = (row * stride) + (pxl * BYTES_PER_PIXEL);

                    if (row < blockHeight)
                    {
                        topArr[blockX, RED] += pixelBuffer[offset + RED];
                        topArr[blockX, GREEN] += pixelBuffer[offset + GREEN];
                        topArr[blockX, BLUE] += pixelBuffer[offset + BLUE];
                    }

                    if (pxl < blockWidth)
                    {
                        leftArr[blockY, RED] += pixelBuffer[offset + RED];
                        leftArr[blockY, GREEN] += pixelBuffer[offset + GREEN];
                        leftArr[blockY, BLUE] += pixelBuffer[offset + BLUE];
                    }
                    else
                    {
                        if (pxl >= blockWidth && pxl < (w - blockWidth))
                        {
                            pxl = w - blockWidth;
                        }

                        rightArr[blockY, RED] += pixelBuffer[offset + RED];
                        rightArr[blockY, GREEN] += pixelBuffer[offset + GREEN];
                        rightArr[blockY, BLUE] += pixelBuffer[offset + BLUE];
                    }

                    pxl += 1; // next pixel
                }
            }

            


            for (var block = 0; block < TOP_BLOCK_COUNT; block++)
            {
                topArr[block, RED] /= blockHeight * blockWidth;
                topArr[block, GREEN] /= blockHeight * blockWidth;
                topArr[block, BLUE] /= blockHeight * blockWidth;
            }

            for (var block = 0; block < SIDE_BLOCK_COUNT; block++)
            {
                leftArr[block, RED] /= blockHeight * blockWidth;
                leftArr[block, GREEN] /= blockHeight * blockWidth;
                leftArr[block, BLUE] /= blockHeight * blockWidth;

                rightArr[block, RED] /= blockHeight * blockWidth;
                rightArr[block, GREEN] /= blockHeight * blockWidth;
                rightArr[block, BLUE] /= blockHeight * blockWidth;
            }

            //bitmap.Save(@"d:\temp\test.bmp");
            //screenShot.Save(@"d:\temp\test1.bmp");
        }
    }
}
