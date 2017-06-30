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
        ScanConfig CurrentConfig = ScanConfig.nonTrigger;
        Dictionary<string,int> ImgWH;
        int                   TrigCount       ;
        int                   TrigLimit       ;  
    }
}
