using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Moonlight.Helpers.Unmanaged
{
    public static class GDI32
    {
        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        //public static extern bool BitBlt(SafeHandle hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, SafeHandle hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);

        [DllImport("gdi32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern SafeCompatibleDCHandle CreateCompatibleDC(SafeHandle hDC);

        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern SafeDibSectionHandle CreateDIBSection(SafeHandle hdc, ref GDI32.BitmapInfoHeader bmi, uint usage, out IntPtr bits, IntPtr hSection, uint dwOffset);

        [DllImport("gdi32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        public struct BitmapInfoHeader
        {
            private const int BI_RGB = 0;
            private const int BI_RLE8 = 1;
            private const int BI_RLE4 = 2;
            private const int BI_BITFIELDS = 3;
            public const int DIB_RGB_COLORS = 0;
            
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public int biClrImportant;

            public BitmapInfoHeader(int width, int height, short bpp)
            {
                this.biSize = (uint)Marshal.SizeOf(typeof(GDI32.BitmapInfoHeader));
                this.biPlanes = 1;
                this.biCompression = 0;
                this.biWidth = width;
                this.biHeight = height;
                this.biBitCount = bpp;
                this.biSizeImage = 0;
                this.biXPelsPerMeter = 0;
                this.biYPelsPerMeter = 0;
                this.biClrUsed = 0;
                this.biClrImportant = 0;
            }
        }
    }
}
