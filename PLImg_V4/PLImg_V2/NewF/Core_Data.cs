using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public partial class Core
    {
        ScanState ScanStatus = ScanState.Wait;
        ScanTypes ScanType = ScanTypes.NonTrig;
        int                   BuffCount       ;
        int                   LineCount       ;
        int                   UnitCount       ;
        byte[]                ImgSrcByte      ;
        bool                  NeedClearBuf    ;
    }
}
