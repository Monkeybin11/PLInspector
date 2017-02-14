using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public class FullScanData
    {
        public double PosXStart;
        public double PosYStart;
        public double PosXEnd;
        
        public readonly int BuffW = 12288;
        public readonly int BuffH = 1024;
        public readonly int OneUnitBuffNum = 12;
        public readonly int OneLineBuffNum = 48;
        public double YStep = 0; // Unit um
        public int BuffLimit = 12;
        public int UnitLimit = 0;
        public int LineLimit = 3;

        public int ScanSpeed = 1;
    }
}
