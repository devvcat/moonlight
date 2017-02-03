using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;

using Moonlight.Helpers.Unmanaged;

namespace Moonlight.Helpers
{
    public interface ICapture
    {
    }

    public static class WindowCapture
    {
        public static Bitmap CaptureWindow()
        {
            Bitmap bitmap = null;

            try
            {
                var captureBounds = Utils.GetScreenBounds();
                bitmap = WindowCapture.CaptureRectangle(captureBounds);
            }
            catch (Exception ex)
            {
                // TODO: handle exception
                Debug.WriteLine(ex.Message);
                throw;
            }

            return bitmap;
        }

        public static Bitmap CaptureRectangle(Rectangle captureBounds)
        {
            IntPtr hDesktop = User32.GetDesktopWindow();
            IntPtr hDC = User32.GetWindowDC(hDesktop);
            IntPtr hDest = GDI32.CreateCompatibleDC(hDC);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hDC, captureBounds.Width, captureBounds.Height);
            IntPtr hOldBitmap = GDI32.SelectObject(hDest, hBitmap);
            
            GDI32.BitBlt(hDest, 0, 0, captureBounds.Width, captureBounds.Height, 
                hDC, captureBounds.X, captureBounds.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

            Bitmap bitmap = null;
            bool isRegionEmpty = true;

            using (var graphics = Graphics.FromHwnd(hDesktop))
            {
                isRegionEmpty = WindowCapture.IsRegionEmpty(graphics, captureBounds);
            }

            if (isRegionEmpty)
            {
                bitmap = Bitmap.FromHbitmap(hBitmap);
            }
            else
            {
                float xDpi = 96F, yDpi = 96F;
                
                using (var tmp = Bitmap.FromHbitmap(hBitmap))
                {
                    xDpi = tmp.HorizontalResolution;
                    yDpi = tmp.VerticalResolution;
                }

                bitmap = WindowCapture.CreateEmpty(captureBounds.Width, captureBounds.Height, PixelFormat.Format32bppArgb, Color.Transparent, xDpi, yDpi);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    foreach(var screen in Screen.AllScreens)
                    {
                        var bounds = screen.Bounds;
                        bounds.Offset(-captureBounds.X, -captureBounds.Y);
                        graphics.DrawImage(bitmap, bounds, bounds.X, bounds.Y, bounds.Width, bounds.Height, GraphicsUnit.Pixel);
                    }
                }
            }
            
            GDI32.SelectObject(hDest, hOldBitmap);
            GDI32.DeleteObject(hBitmap);
            GDI32.DeleteDC(hDest);
            User32.ReleaseDC(hDesktop, hDC);

            return bitmap;
        }

        public static Bitmap CreateEmpty(int width, int height, PixelFormat pixelFormat, Color backgroundColor, float xDpi, float yDpi)
        {
            var bitmap = new Bitmap(width, height, pixelFormat);
            bitmap.SetResolution(xDpi, yDpi);
            
            if (pixelFormat != PixelFormat.Format8bppIndexed)
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    if (!Color.Empty.Equals(backgroundColor))
                    {
                        graphics.Clear(backgroundColor);
                    }
                    else if (!Image.IsAlphaPixelFormat(pixelFormat))
                    {
                        graphics.Clear(Color.White);
                    }
                    else
                    {
                        graphics.Clear(Color.Transparent);
                    }
                }
            }

            return bitmap;
        }

        private static bool IsRegionEmpty(Graphics graphics, Rectangle bounds)
        {
            var region = new Region(bounds);
            foreach(var screen in Screen.AllScreens)
            {
                if (screen.Bounds.IntersectsWith(bounds))
                {
                    region.Exclude(screen.Bounds);
                    Debug.WriteLine(region.GetRegionData());
                }
            }
            return region.IsEmpty(graphics);
        }
    }
}
