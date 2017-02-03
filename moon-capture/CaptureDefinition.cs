using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlight
{
    public class CaptureDefinition
    {
        const int DEFAULT_TOP_BLOCK_COUNT = 20;
        const int DEFAULT_SIDE_BLOCK_COUNT = 10;

        private Rectangle captureRectangle;
        private int topBlockCount = DEFAULT_TOP_BLOCK_COUNT;
        private int sideBlockCount = DEFAULT_SIDE_BLOCK_COUNT;

        public CaptureDefinition(
            int topBlockCount, int sideBlockCount, Rectangle captureRectangle)
        {
            this.topBlockCount = topBlockCount;
            this.sideBlockCount = sideBlockCount;
            this.captureRectangle = captureRectangle;

            RecalculateBlocks();
        }

        public CaptureDefinition(Rectangle captureRectangle)
            : this(DEFAULT_TOP_BLOCK_COUNT, DEFAULT_SIDE_BLOCK_COUNT, captureRectangle)
        { }

        private void RecalculateBlocks()
        {
            var w = captureRectangle.Width;
            var h = captureRectangle.Height;

            this.BlockWidth = w / topBlockCount;
            this.BlockHeight = h / sideBlockCount;
        }

        public int TopBlockCount
        {
            get
            {
                return this.topBlockCount;
            }
            set
            {
                this.topBlockCount = value;
                RecalculateBlocks();
            }
        }

        public int SideBlockCount
        {
            get
            {
                return this.sideBlockCount;
            }
            set
            {
                this.sideBlockCount = value;
                RecalculateBlocks();
            }
        }

        public int BlockWidth { get; private set; }
        public int BlockHeight { get; private set; }
    }
}
