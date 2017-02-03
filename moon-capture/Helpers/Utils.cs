using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moonlight.Helpers
{
    public static class Utils
    {
        public static Rectangle GetScreenBounds()
        {
            var p = new Point(0, 0);
            var w = new Point(0, 0);

            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.X < p.X) p.X = screen.Bounds.X;
                if (screen.Bounds.Y < p.Y) p.Y = screen.Bounds.Y;
                if (screen.Bounds.Right > w.X) w.X = screen.Bounds.Right;
                if (screen.Bounds.Bottom > w.Y) w.Y = screen.Bounds.Bottom;
            }

            var bounds = new Rectangle(p.X, p.Y, w.X + Math.Abs(p.X), w.Y + Math.Abs(p.Y));
            return bounds;
        }

        public static Rectangle GetGuiRectangle(int x, int y, int w, int h)
        {
            if (w < 0)
            {
                x = x + w;
                w = -w;
            }
            if (h < 0)
            {
                y = y + h;
                h = -h;
            }
            return new Rectangle(x, y, w, h);
        }
    }
}
