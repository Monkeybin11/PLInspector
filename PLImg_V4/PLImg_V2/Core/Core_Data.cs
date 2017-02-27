using MachineControl.Camera.Dalsa;
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
        ScanConfig CurrentConfig = ScanConfig.nonTrigger;
        Dictionary<string,int> ImgWH;
        int                   BuffCount       ;
        int                   LineCount       ;
        int                   UnitCount       ;
        byte[]                ImgSrcByte      ;
        bool                  NeedClearBuf    ;
        int                   TrigCount       ;
        int                   TrigLimit       ;  



    }
}
