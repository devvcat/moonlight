using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Moonlight.Helpers.Unmanaged
{
    public abstract class SafeDCHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeDCHandle(bool ownsHandle)
            : base(ownsHandle)
        { }
    }

    public class SafeCompatibleDCHandle : SafeDCHandle
    {
        private SafeCompatibleDCHandle()
            : base(true)
        {
        }

        public SafeCompatibleDCHandle(IntPtr preexistingHandle)
            : base(true)
        {
            base.SetHandle(preexistingHandle);
        }

        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hDC);

        protected override bool ReleaseHandle()
        {
            return SafeCompatibleDCHandle.DeleteDC(this.handle);
        }

        public SafeSelectObjectHandle SelectObject(SafeHandle newHandle)
        {
            return new SafeSelectObjectHandle(this, newHandle);
        }
    }

    public class SafeSelectObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeHandle hdc;

        private SafeSelectObjectHandle()
            : base(true)
        { }

        public SafeSelectObjectHandle(SafeDCHandle hdc, SafeHandle newHandle)
            : base(true)
        {
            this.hdc = hdc;
            base.SetHandle(SafeSelectObjectHandle.SelectObject(hdc.DangerousGetHandle(), newHandle.DangerousGetHandle()));
        }

        protected override bool ReleaseHandle()
        {
            SafeSelectObjectHandle.SelectObject(this.hdc.DangerousGetHandle(), this.handle);
            return true;
        }

        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }

    public abstract class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeObjectHandle(bool ownsHandle)
            : base(ownsHandle)
        {
        }

        [DllImport("gdi32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        protected override bool ReleaseHandle()
        {
            return SafeObjectHandle.DeleteObject(this.handle);
        }
    }

    public class SafeDibSectionHandle : SafeObjectHandle
    {
        private SafeDibSectionHandle()
            : base(true)
        { }

        public SafeDibSectionHandle(IntPtr preexistingHandle)
            : base(true)
        {
            base.SetHandle(preexistingHandle);
        }
    }

    public class SafeWindowDCHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private IntPtr hWnd;

        private SafeWindowDCHandle()
            : base(true)
        {
        }

        public SafeWindowDCHandle(IntPtr hWnd, IntPtr preexistingHandle)
            : base(true)
        {
            this.hWnd = hWnd;
            base.SetHandle(preexistingHandle);
        }

        public static SafeWindowDCHandle FromDesktop()
        {
            IntPtr desktopWindow = User32.GetDesktopWindow();
            return new SafeWindowDCHandle(desktopWindow, SafeWindowDCHandle.GetWindowDC(desktopWindow));
        }

        [DllImport("user32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        protected override bool ReleaseHandle()
        {
            return SafeWindowDCHandle.ReleaseDC(this.hWnd, this.handle);
        }
    }
}
