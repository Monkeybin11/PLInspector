using Accord.Math;
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
        public void ReadyNonTrigScan()
        {

        }

        public void ScanStart_Non()
        {

        }



        public void StartTrigScan( ScanConfig config )
        {
            CurrentConfig = config;
            TrigLimit = SetTriggerLimit( config );
            TrigCount = 0;
            StgReadyTrigScan( 0, config );

            System.Threading.Thread.Sleep( 100 );
            ResetCamCofnig( config );
            RunStgBuffer( config );
            Grab();
            System.Threading.Thread.Sleep( 100 );


            ScanMoveXYstg( "Y", TrigScanData.EndYPos[config], TrigScanData.Scan_Stage_Speed );
        }

        void StgReadyTrigScan( int triggerNum, ScanConfig config )
        {
            MoveXYstg( "Y", TrigScanData.StartYPos[config] );
            MoveXYstg( "X", TrigScanData.StartXPos[config] + TrigScanData.XStep_Size * triggerNum );
            Stg.WaitEps( "Y" )( TrigScanData.StartYPos[config], 0.005 );
            Stg.WaitEps( "X" )( TrigScanData.StartXPos[config], 0.005 );
            Stg.SetSpeed( "Y" )( TrigScanData.Scan_Stage_Speed );
        }

        void ResetCamCofnig( ScanConfig config )
        {
            Reconnector[config]();
            Cam.EvtResist( Cam.Xfer, GrabDoneEvt_Trg );
        }

        int SetTriggerLimit( ScanConfig config )
        {
            switch ( config )
            {
                case ScanConfig.Trigger_1:
                    return 1;

                case ScanConfig.Trigger_2:
                    return 2;

                case ScanConfig.Trigger_4:
                    return 4;
                default:
                    return 1;
            }
        }
    }
}
