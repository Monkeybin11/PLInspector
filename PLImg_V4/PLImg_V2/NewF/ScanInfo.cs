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
            if(bufH > 0 ) BuffH = bufH;
            if(BuffW > 0 )BuffW = bufW;
        }

        public void SetLimit(int buf,int unit, int line) {
            if( buf  > 0 )  BuffLimit = buf; 
            if( unit > 0 )  UnitLimit = unit;
            if( line > 0 ) LineLimit = line; 
        }
        public void SetScanSpeed( int sped ) {
            if ( sped > 0 ) ScanSpeed = sped;
        }
    }
}
