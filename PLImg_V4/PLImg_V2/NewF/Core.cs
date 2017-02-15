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
    public partial class Core
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
        public TrgScanInfo TrgInfo   = new TrgScanInfo();
        Indicator Idc = new Indicator();
        

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
                    StartProcess(ScanType);
                    break;

                default:
                    evtRealimg( FnBuff2Img(  Cam.GetBuffWH()["H"] , Cam.GetBuffWH()["W"] )( FullBuffdata() , 1) );
                    var zscore = Idc.Zscore( SingleBuffdata().Cast<double>().ToArray<double>() );
                    Task.Run(()=> evtSV( Idc.Variance( zscore() )() ) );
                    break;
            }
        }

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
        public void GetFeedbackPos()
        {
            try
            {
                while ( true )
                {
                    var yP = Stg.GetPos("Y");
                    var xP = Stg.GetPos("X");
                    var zP = Stg.GetPos("Z");
                    evtFedBckPos( new double[3] { yP(), xP() , zP() } );
                    Task.Delay( 500 ).Wait();
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
            }
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
