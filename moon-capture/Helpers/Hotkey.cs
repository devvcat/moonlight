using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moonlight.Helpers
{
    public class Hotkey
    {
        public bool Alt { get; set; }
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }

        public Keys Key { get; set; }

        public static int GetKey(string key)
        {
            key = key.Trim().ToUpper();

            switch (key.Trim().ToUpper())
            {
                case "ALT": return 1;
                case "SHIFT": return 4;
                case "CTRL": return 2;
                case "PRNTSCR": return (int)Keys.PrintScreen;
            }

            if (key.Length == 1)
            {
                var ch = key[0];
                return Convert.ToInt32(ch);
            }

            return 0;
        }

        public override string ToString()
        {
            var output = string.Concat(
                this.Ctrl ? "Ctrl+" : "",
                this.Alt ? "Alt+" : "",
                this.Shift ? "Shift+" : "",
                this.Key.ToString());
            return output;
        }
    }
}
