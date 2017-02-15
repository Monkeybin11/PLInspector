using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public enum WaferSize { Size2, Size4 }
    public class TrgScanInfo
    {
        public readonly double PsXStart = 50 ;
        public const int BuffW = 12288;

        public double PsXEnd   ;
        public double PsYStart ;

        public int BuffH     = 12288;
        public double YStep  = 28.3; 
        public int LineNum   = 3;
        public int ScanSpeed = 1;

        WaferInfo size2 = new WaferInfo(2);
        WaferInfo size4 = new WaferInfo(4);

        public void SetTrgInfo( int ystart , int bufH = 12288 , double ystep = 28.3 , int lineNum = 3, int scanspeed = 1 )
        {
            if ( ystart > 0 )    PsYStart = ystart;
            if ( bufH > 0 )      BuffH = bufH;
            if ( ystep > 0 )     YStep = ystep;
            if ( lineNum > 0 )   LineNum = lineNum;
            if ( scanspeed > 0 ) ScanSpeed = scanspeed;
        }

        public void SetTrgInfo( WaferSize wafersize )
        {
            if ( wafersize == WaferSize.Size2 ) {
                PsYStart    = size2.PsYStart;
                BuffH       = size2.BuffH;
                LineNum     = size2.LineNum;
                ScanSpeed   = size2.ScanSpeed;
            }
            else if ( wafersize == WaferSize.Size4 ) {
                PsYStart    = size4.PsYStart;
                BuffH       = size4.BuffH;
                LineNum     = size4.LineNum;
                ScanSpeed   = size4.ScanSpeed;
            }
        }

        public class WaferInfo{
            public int PsYStart   ; 
            public int BuffH      ; 
            public int LineNum    ; 
            public int ScanSpeed  ;

            public WaferInfo( int size ) {
                if ( size == 2 ){
                    PsYStart = 0;
                    BuffH    = 0;
                    LineNum  = 0;
                    ScanSpeed= 0;
                }
                if ( size == 4 ){
                    PsYStart = 0;
                    BuffH    = 0;
                    LineNum  = 0;
                    ScanSpeed= 0;
                }
            }
        }
    }
}
