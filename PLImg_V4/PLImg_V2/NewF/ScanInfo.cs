using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public class ScanInfo
    {
        public double PsXStart;
        public double PsYStart;
        public double PsXEnd  ;

        public int BuffW = 12288;
        public int BuffH = 1024;

        public readonly int OneUnitBuffNum = 12;
        public readonly int OneLineBuffNum = 48;
        public double YStep = 28.3; // Unit um

        public int BuffLimit = 11;
        public int UnitLimit = 3;
        public int LineLimit = 3;

        public int ScanSpeed = 1;

        public void SetPos(int xstart,int ystart,int xend,double yStep) {
            PsXStart = xstart;
            PsYStart = ystart;
            PsXEnd   = xend;
            YStep    = yStep;
        }
        public void SetBufInfo(int bufW,int bufH) {
            BuffH = bufH;
            BuffW = bufW;
        }

        public void SetLimit(int buf,int unit, int line) {
            BuffLimit = buf; 
            UnitLimit = unit; 
            LineLimit = line; 
        }
        public void SetScanSpeed( int sped ) {
            ScanSpeed = sped;
        }
    }
}
