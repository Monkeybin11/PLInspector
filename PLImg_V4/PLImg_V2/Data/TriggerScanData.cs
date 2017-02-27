using MachineControl.Camera.Dalsa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2.Data
{
    public class TriggerScanData
    {
        public double StartYPos        = 49;
        public double StartXPos        = 49;
        public Dictionary<ScanConfig,double> EndYPos;
        public double XStep_Size       = 28.35;
        public double Scan_Stage_Speed = 1;
        public double Camera_Exposure  = 2400;
        public double Camera_LineRate  = 400;

        public TriggerScanData() { CreateEndPoint(); }
        public TriggerScanData(
            double startYPos        = 49 ,
            double startXPos        = 49 ,
            double xStep_Size       = 28.35 ,
            double scan_Stage_Speed = 1,
            double camera_Exposure  = 2400,
            double camera_LineRate  = 400
            )
        {
            StartYPos        = startYPos  ;
            StartXPos        = startXPos  ;
            XStep_Size       = xStep_Size ;
            Scan_Stage_Speed = scan_Stage_Speed;
            Camera_Exposure  = camera_Exposure;
            Camera_LineRate  = camera_LineRate;

            CreateEndPoint();
        }

        void CreateEndPoint() {
            EndYPos = new Dictionary<ScanConfig , double>();
            EndYPos.Add( ScanConfig.Trigger_1 , 100 );
            EndYPos.Add( ScanConfig.Trigger_2 , 150 );
            EndYPos.Add( ScanConfig.Trigger_4 , 200 );

        }
    }
}
