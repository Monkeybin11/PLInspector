using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineControl.Camera.Dalsa;
using MachineControl.Stage.ACSController;
using DALSA.SaperaLT.SapClassBasic;
using Emgu.CV;
using Emgu.CV.Structure;
using Accord.Math;
using MachineControl.MathClac;

namespace PLImg_V2
{
    public delegate void TferbyteArr( byte[] imgarr );
    public delegate void TferImgArr( Image<Gray , Byte> img );
    public delegate void TferScanStatus();
    public delegate void TferSplitImgArr( Image<Gray , Byte> img , int lineNum , int unitNum );
    public delegate void TferFeedBackPos( double[] XYZPos );
    public delegate void TferNumber( double num );
    
    public enum ScanMode { MultiLine, SingleLine };
    enum ScanState { Start, Pause, Stop, Wait }

    public class Core
    {
        #region Event
        public event TferImgArr        evtRealimg   ;
        public event TferSplitImgArr   evtMapImg    ;
        public event TferFeedBackPos   evtFedBckPos ;
        public event TferScanStatus    evtScanStart ;
        public event TferScanStatus    evtScanEnd   ;
        public event TferNumber        evtSV        ;     
        #endregion  


        public DalsaPiranha3_12k Cam = new DalsaPiranha3_12k();
        public AcsCtrlXYZ Stg        = new AcsCtrlXYZ();
        public ScanInfo Info         = new ScanInfo();
        Indicator Idc = new Indicator();
        ScanState ScanStatus = ScanState.Wait;
        int                   BuffCount       ;
        int                   LineCount       ;
        int                   UnitCount       ;
        byte[]                ImgSrcByte      ;
        bool                  NeedClearBuf    ;

        /*GFunc*/
        public Action<double> LineRate;
        public Action<double> Exposure;
        public Action         Grab;
        public Action         Freeze;
        public Action         BufClear;
        public Func<byte[]>   FullBuffdata;
        public Func<byte[]>   SingleBuffdata;

        public void ConnectDevice( string camPath , string stgPath , string rstagPath ) {
            Cam.Connect(camPath)();
            var stgConnectMode = MachineControl.Stage.Interface.ConnectMode.Com;
            Stg.Connect(stgPath, stgConnectMode )();
            InitFunc();
        }

        /* GFun Create */
        public void InitFunc() {
            Cam.EvtResist( Cam.Xfer, GrabDoneEvt );
            Exposure = Cam.Exposure();
            LineRate = Cam.LineRate();
            Grab     = Cam.Grab();
            Freeze   = Cam.Freeze();
            BufClear = Cam.BuffClear();
            FullBuffdata   = Cam.BuffGetAll( Cam.Buffers );
            SingleBuffdata = Cam.BuffGetLine( Cam.Buffers );
        }

        void GrabDoneEvt( object sender , SapXferNotifyEventArgs evt ) {
            Console.WriteLine( "IN Evt" );
            switch ( ScanStatus )
            {
                case ScanState.Stop:
                    ScanStatus = ScanState.Wait;
                    
                    break;

                case ScanState.Pause:
                    PauseProcess( LineCount );
                    break;

                case ScanState.Start:
                    StartProcess();
                    break;

                default:
                    evtRealimg( FnBuff2Img(  Cam.GetBuffWH()["H"] , Cam.GetBuffWH()["W"] )( FullBuffdata() , 1) );
                    var zscore = Idc.Zscore( SingleBuffdata().Cast<double>().ToArray<double>() );
                    Task.Run(()=> evtSV( Idc.Variance( zscore() )() ) );
                    break;
            }
        }

        #region Scan Process
        public bool ReadyPos() {
            try
            {
                var xSpeed =  Stg.SetSpeed( "X" );
                var ySpeed =  Stg.SetSpeed( "Y" );
                xSpeed( 200 );
                ySpeed( 200 );
                Stg.Moveabs( "X" )(Info.PsXStart);
                Stg.Moveabs( "Y" )(Info.PsYStart);
                Stg.WaitEps( "X" )( Info.PsXStart , 0.005 );
                Stg.WaitEps( "Y" )( Info.PsYStart , 0.005 );
                xSpeed( Info.ScanSpeed );
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        public bool ScanStart() {
            try
            {
                InitCount();
                ImgSrcByte = new byte[0];
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }

        /*Scan process func*/
        bool StopProcess() {
            try
            {
                ScanStatus = ScanState.Wait;
                NeedClearBuf = true;
                Freeze();
                System.Threading.Thread.Sleep( 100 );
                Grab();
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        bool PauseProcess(int linecount) {
            try
            {
                var yTargetPos = Info.PsYStart - Info.YStep * linecount;
                Stg.WaitEps( "X" )( Info.PsXEnd , 0.01 );
                MoveXYstg( "X" , Info.PsXStart );
                MoveXYstg( "Y" , yTargetPos );
                Stg.WaitEps( "X" )( Info.PsXStart , 0.01 );
                Stg.WaitEps( "Y" )( yTargetPos , 0.01 );
                Stg.SetSpeed( "X" )( Info.ScanSpeed );
                Stg.Moveabs( "X" )( Info.PsXEnd );
                ScanStatus = ScanState.Start;
                NeedClearBuf = true;
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        bool StartProcess() {
            try
            {
                Stg.Moveabs( "X" )( Info.PsXEnd );
                if ( NeedClearBuf )
                {
                    Freeze();
                    System.Threading.Thread.Sleep( 30 );
                    Cam.BuffClear()();
                    NeedClearBuf = false;
                    Grab();
                }

                /*Create Func*/
                var Buf2Img = FnBuff2Img( Cam.GetBuffWH()["H"] , Cam.GetBuffWH()["W"] );
                var currentbuff = FullBuffdata();

                evtRealimg( Buf2Img( currentbuff , 1 ) );
                ImgSrcByte = Matrix.Concatenate<byte>( ImgSrcByte , currentbuff );
                //SaveFullDat(ImgSrcByte,LineCount,UnitCount,BuffCount);
                evtMapImg( Buf2Img( ImgSrcByte , BuffCount + 1 ) , LineCount , UnitCount );
                Update( BuffCount , UnitCount , LineCount );
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }

        /*minor func*/
        void InitCount()
        {
            BuffCount = 0;
            UnitCount = 0;
            LineCount = 0;
        }
        void Update( int BuffCount , int UnitCount , int LineCount )
        {
            if ( BuffCount == Info.BuffLimit )
            {
                ImgSrcByte = null;
                ImgSrcByte = new byte[0];
                BuffCount = 0;
                if ( UnitCount == Info.UnitLimit )
                {
                    UnitCount = 0;
                    if ( LineCount == Info.LineLimit )
                    {
                        ScanStatus = ScanState.Stop;
                    }
                    else
                    {
                        LineCount += 1;
                        ScanStatus = ScanState.Pause;
                    }
                }
                else { UnitCount += 1; }
            }
        }
        #endregion

        #region Stage Control
        public void MoveXYstg( string axis , double point ) {
            Stg.SetSpeed( axis )( 200 );
            Stg.Moveabs ( axis )( point );
        }

        public void MoveZstg( double point )
        {
            Stg.SetSpeed( "Z" )( 10 );
            Stg.Moveabs( "Z" )( point );
        }


        #endregion

        #region indicator
        double SV(double[] input) {
            var zscore = Idc.Zscore(input);
            var vari = Idc.Variance(zscore());
            return vari();

        }

        #endregion


        #region Minor
        public Action<ScanMode> ScanInfoSet(
            int xstart , int ystart , int xend , double yStep ,
            int bufW , int bufH ,
            int buflimit , int unitlimit , int linelimit ,
            int sped )
        {
            return new Action<ScanMode>( ( mode ) =>
            {
                Info.SetBufInfo( bufW , bufH );
                Info.SetScanSpeed( sped );
                switch ( mode )
                {
                    case ScanMode.MultiLine:
                        Info.SetPos( xstart , ystart , xend , yStep );
                        Info.SetLimit( buflimit , unitlimit , linelimit );
                        break;

                    case ScanMode.SingleLine:
                        Info.SetPos( xstart , ystart , xend , 0 );
                        Info.SetLimit( buflimit , unitlimit , 0 );
                        break;
                }
            } );
        }

        public void GetFeedbackPos(){
            try
            {
                while ( true )
                {
                    var xP = Stg.GetPos("X");
                    var yP = Stg.GetPos("Y");
                    var zP = Stg.GetPos("Z");
                    evtFedBckPos( new double[3] { xP(), yP(), zP() } );
                    Task.Delay( 500 ).Wait();
                }
            }
            catch ( Exception ex)
            {
                Console.WriteLine( ex.ToString() );
            }
        }

        Func<byte[],int , Image<Gray , byte>> FnBuff2Img( int bufH, int bufW )
        {
            Func<byte[],int,Image<Gray , byte>> output = new Func<byte[],int,Image<Gray, byte>>((data,bufcount)=> {
                Image<Gray, byte> buffImgData = new Image<Gray, byte>(bufW,bufH*bufcount);
                buffImgData.Bytes = data;
                return buffImgData;
            } );
            return output;
        }

        void LoadSetting() {

        }
        void SaveSetting() {

        }

        void SetDir()
        {
            //string dirTempPath = String.Format(ImgBasePath + DateTime.Now.ToString("MM/dd/HH/mm/ss"));
            //CheckAndCreateFolder cacf = new CheckAndCreateFolder(dirTempPath);
            //cacf.SettingFolder( dirTempPath );
            //GrabM.SetDirPath( dirTempPath );
        }

        public void SaveImageData( Emgu.CV.UI.ImageBox[,] imgbox , string savepath )
        {
            try
            {

                for ( int i = 0 ; i < imgbox.GetLength( 0 ) ; i++ )
                {
                    for ( int j = 0 ; j < imgbox.GetLength( 1 ) ; j++ )
                    {
                        if ( imgbox[i , j].Image != null )
                        {
                            string temp = i.ToString( "D2" ) + "_"+j.ToString( "D3" );
                            string outpath = System.IO.Path.Combine( savepath, temp );
                            imgbox[i , j].Image.Save( String.Format( outpath + ".bmp" ) );
                        }
                    }
                }

            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );

            }
        }

        #endregion
    }
}
