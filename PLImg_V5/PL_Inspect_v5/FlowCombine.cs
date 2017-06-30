using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PLImg_V2
{
    public class FlowCombine
    {
        static Flow FL = new Flow();
        public event TrsBuffData evtScanedImage
        { add { FL.evtTrsScanedImage += value; } remove { } }

        public FlowCombine()
        {
        }

        public async void EpiScan( ScanConfig config )
        {
            await FL.MoveStartOrigin( config , 0 );
            bool? result;
            switch ( config )
            {
                case ScanConfig.Trigger_1:
                    result = await FL.ScanMomenet( config , 4 );
                    break;


                case ScanConfig.Trigger_2:
                    result =await FL.ScanMomenet( config , 4 );
                    break;

                case ScanConfig.Trigger_4:
                    result = await await FL.ScanMomenet( config , 4 )
                                .ContinueWith( x => FL.MoveStartOrigin( config , 1 ) )
                                .ContinueWith( x => FL.ScanMomenet( config , 4 ) )
                                .ContinueWith( x => FL.MoveStartOrigin( config , 2 ) )
                                .ContinueWith( x => FL.ScanMomenet( config , 4 ) )
                                .ContinueWith( x => FL.MoveStartOrigin( config , 3 ) );
                    break;
            }
        }

        public async void DisplayBuff( byte[,,] input  )
        {

            //  여기서 동작 을 서술
            // 컨트롤에서 동작에 참여할 컴포넌트들 골라서 넣어준다. 
        }

   



    }
}
