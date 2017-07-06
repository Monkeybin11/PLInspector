using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using CommonExtension;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Media.Imaging;

namespace PLImaging
{
    public delegate void TrsResized( BitmapSource src );


    public class NewCore
    {
        FlowCombine FC        = new FlowCombine();
        SettingData Setting   = new SettingData();
        ScanTempData Tdata; 
        ResultDataClass Rdata;

        public event TrsResized evtOriginResized;
        public event TrsResized evtBoxResized;
        
        


        public NewCore()
        {
        }

        public void DisplayBuf(ImageBrush img)
        {


        }

        public void Step2()
        {
            //var result = await FC.CombineImage( null , 1 , 2 );
            
        }

        public void Processing()
        {

        }


        public async void StartScan(Tuple<ScanConfig,int,int> input)
        {
            Tdata = new ScanTempData( input.Item1  , input.Item2);
            await FC.EpiScanAsync( input.Item1 ) ;
        }

        public async Task BufferTrsDone( byte [ , , ] input )
        {
            /*
             Need Set 
             1. SeperatedImg;
             2. SeperatedColorImg;
             3. SeperatedBoxedImg;
             4. SeperatedResizedImg;
             5. EpiDefectList
             */

            if ( Tdata.BufferCount >= Tdata.BufferLimit 
                 || Tdata.CurrentStatus == ScanTempData.TaskStatus.Stopped )
            {
                Tdata.BufferCountUpdate();
                return;
            }
            int bufNum  = Tdata.BufferCount;
            Tdata.BufferCountUpdate();

            var setImg = Task.Run(()=>
            {
                Tdata.SeperatedImg [ bufNum ] = input; //1.SeperatedImg
                Tdata.SeperatedColorImg [ bufNum] = FC.ToBgrArrayAsync(input).Result; //2.SeperatedColorImg
            } );

            var defectPostask = FC.PreProcessingAsync( input , Tdata.CurrentType )
                                  .ContinueWith( x =>
                                             FC.GetContoursAsync( x.Result , Tdata.CurrentType ).Result 
                                            ,TaskContinuationOptions.ExecuteSynchronously);
            var resizetask = FC.ResizeAsync(input , Tdata.CurrentType);

            var checkAggregateEx = Task.WhenAll(defectPostask , resizetask);
            try { await checkAggregateEx; }
            catch ( AggregateException aex ) { AfterAggregateException( aex ); return; }

            Tdata.SeperatedResizedImg[ bufNum ] = resizetask.Result; //3.SeperatedResizedImg

            var getdefectInfo = Task.Run( () =>
            {
                Tdata.EpiDefectList[ bufNum ] = defectPostask.Result.Select( x =>
                                                                         x.ToDefectRawInfo() )
                                                                    .ToList();//5.EpiDefectList
                
                
                Tdata.SeperatedBoxedImg [ bufNum ] = Tdata.SeperatedColorImg [ bufNum ].Act( x =>
                                                           DrawCircles( new Image<Bgr , byte>( x )
                                                           , Tdata.EpiDefectList [ bufNum ]
                                                           , Tdata.EpiDefectList [ bufNum ].Count ));
                                                           //4.SeperatedBoxedImg
            });

            //try { getdefectInfo.Wait(); }
            //catch ( AggregateException aex ) { AfterAggregateException( aex ); return; }

            if ( Tdata.BufferCount == Tdata.BufferLimit )
            {
                if(getdefectInfo.CheckTaskException() != null) await getdefectInfo;
                await ProcAfterScanDone().CheckTaskException();
            }
        }

        public async Task ProcAfterScanDone()
        {
            var combineImg    = Tdata.SeperatedImg.Map( x => 
                                                        FC.CombineImageAsync(x 
                                                                             , Tdata.ResizeHeight 
                                                                             , Tdata.ResizeWidth ));
            var shiftedresult = Tdata.EpiDefectList.Map( x => 
                                                         FC.ShiftResult(x) );
            var combineBox    = Tdata.SeperatedBoxedImg.Map( x=> 
                                                             FC.CombineBoxImg(x) );

            var alltask = Task.WhenAll( combineImg , shiftedresult ,combineBox );
            try { await alltask; }
            catch ( AggregateException aex ) { AfterAggregateException( aex ); return; }

            combineBox.Result.Act( x => Tdata.ResizedBoxedImg = x )
                             .Act( x => evtOriginResized( BitmapSrcConvert.ToBitmapSource( new Image<Bgr,byte>(x))) );

            combineImg.Result.Act( x => Tdata.ResizedImg = x )
                             .Act( x => evtBoxResized( BitmapSrcConvert.ToBitmapSource( new Image<Bgr , byte>( x ) ) ) );

            shiftedresult.Result.Act( x => Tdata.EpiFullDefect = x );
        }


        void DrawCircles( Image<Bgr , byte> dst , List<DefectRawData> defectlist , int iterNum )
        {
            foreach ( var df in defectlist )
            {
                MCvScalar color;
                color = new MCvScalar( 0 , 0 , 255 );
                CvInvoke.Circle( dst
                                , new System.Drawing.Point( ( int )df.CenterX , ( int )df.CenterY )
                                , ( int )df.Radius
                                , color
                                , 2 );
            }
        }

        void AfterAggregateException(AggregateException aex)
        {
            Tdata.CurrentStatus = ScanTempData.TaskStatus.Stopped;
            foreach ( var ex in aex.Flatten().InnerExceptions )
            { Console.WriteLine( ex.ToString() ); }
        }

    }
}
