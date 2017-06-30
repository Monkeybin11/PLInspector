using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using CommonExtension;

namespace PLImg_V2
{
    public delegate void TrsResized( byte [ , ] input );


    public class NewCore
    {
        FlowCombine FC        = new FlowCombine();
        SettingData Setting   = new SettingData();
        ScanTempData Tdata; 

        
        


        public NewCore()
        {
            FC.evtScanedImage += new TrsBuffData( BufferTrsDone );


        }

        public async void DisplayBuf(ImageBrush img)
        {


        }

        public async void Step2()
        {
            //var result = await FC.CombineImage( null , 1 , 2 );

        }

        public async void Processing()
        {

        }


        public async void StartScan(Tuple<ScanConfig,int,int> input)
        {
            Tdata = new ScanTempData( input.Item2  , input.Item1);
            await FC.EpiScanAsync( input.Item1 ) ;
        }

        public async void BufferTrsDone( byte [ , , ] input )
        {
            if ( Tdata.BufferCount >= Tdata.BufferLimit )
            {
                Tdata.BufferCountUpdate();
                return;
            }
            int bufNum  = Tdata.BufferCount;
            Tdata.BufferCountUpdate();

            var setImg = Task.Run(()=>
            {
                Tdata.SeperatedImg [ bufNum ] = input;
                Tdata.SeperatedColorImg [ bufNum] = FC.ToBgrArrayAsync(input).Result;
            } );

            var defectPostask = FC.PreProcessingAsync( input , Tdata.CurrentType )
                                  .ContinueWith( x =>
                                             FC.GetContoursAsync( x.Result , Tdata.CurrentType ) 
                                            ,TaskContinuationOptions.ExecuteSynchronously);
            var resizetask = FC.ResizeAsync(input , Tdata.CurrentType);

            var defectPoslist = defectPostask.CheckTaskException()?.Result;
            Tdata.SeperatedResizedImg[ bufNum ] = resizetask.CheckTaskException()?.Result;


            
            //Tdata.SeperatedBoxedImg [ bufNum ] =

            //box로 드로잉 => 저장 
            //box로 분리된 디펙트 정보 저장 

            // 3개의 결과를 저장한다. ( 버퍼 리사이즈 , 버퍼 프리프로세싱=> 디펙트 위치 찾기)
            Tdata.SeperatedBoxedImg[ bufNum ] = null; 

            // do something

            Tdata.BufferCount++;
            if ( Tdata.BufferCount == Tdata.BufferLimit )
            {
               // Tdata.ResizedImg = FC.CombineImage( Tdata.SeperatedImg , 12000,12000 );
                Tdata.ResizedBoxedImg = null;

            }
            // reszie
            //


            
        }

    }
}
