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
        public Action Connect_NonTrigger;
        public Action Connect_Trigger1;
        public Action Connect_Trigger2;
        public Action Connect_Trigger4;
        public Action Connect_XYZStage;


        public Action<double> LineRate;
        public Action<double> Exposure;
        public Action         Grab;
        public Action         Freeze;
        public Action         BufClear;
        public Func<byte[]>   FullBuffdata;
        public Func<byte[]>   SingleBuffdata;
        public Func<byte[], int, Image<Gray, byte>> Reshape2D;
        public Dictionary<string,Action> StgEnable;

        public void ConnectDevice( string camPath , string stgPath , string rstagPath ) {
            Connect_NonTrigger();
            Connect_XYZStage();
            InitFunc();
            InitData();
            foreach ( var item in StgEnable ) item.Value();
            Reshape2D = FnBuff2Img( ImgWH["H"], ImgWH["W"] );
        }

        /* GFun Create */
        public void Create_Connector( string camPath, string stgPath, string rstagPath )
        {
            Connect_NonTrigger = Cam.Connect( camPath  , ScanConfig.nonTrigger) ;
            Connect_Trigger1  =  Cam.Connect( camPath  , ScanConfig.Trigger_1);
            Connect_Trigger2  =  Cam.Connect( camPath  , ScanConfig.Trigger_2);
            Connect_Trigger4  =  Cam.Connect( camPath, ScanConfig.Trigger_4);
            var stgConnectMode = MachineControl.Stage.Interface.ConnectMode.IP;
            Connect_XYZStage = Stg.Connect( stgPath, stgConnectMode );
        }

        public void InitFunc( ) {
            Cam.EvtResist( Cam.Xfer, GrabDoneEvt );
            Exposure       = Cam.Exposure();
            LineRate       = Cam.LineRate();
            Grab           = Cam.Grab();
            Freeze         = Cam.Freeze();
            BufClear       = Cam.BuffClear();
            FullBuffdata   = Cam.BuffGetAll( Cam.Buffers );
            SingleBuffdata = Cam.BuffGetLine( Cam.Buffers );

            StgEnable = new Dictionary<string, Action>();
            foreach ( var item in GD.YXZ ) StgEnable.Add( item , Stg.Enable( item ) );
        }

        public void InitData( ) {
            ImgWH = Cam.GetBuffWH();
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
                    evtRealimg( Reshape2D( FullBuffdata() , 1) );
                    Task.Run(()=> TferVariance( SingleBuffdata() ) );
                    break;
            }
        }

        public void TferVariance( byte[] src ) {
            try
            {
                double[] dst = new double[src.Length];
                Array.Copy(src,dst,src.Length);
                var zscore = Idc.Zscore( dst );
                var vari = Idc.Variance(zscore());
                Task.Run( ( ) => evtSV( vari() ) );
            }
            catch ( Exception ex)
            {
                Console.WriteLine( ex.ToString() );
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

        #region Minor
        
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

        #endregion

        public void ReadyNonTrigScan( ) {



        }

        public void ScanStart_Non( ) {


        }

        public void ReadyTrigScan(ScanConfig config ) {
        


        }

        public void ScanStart_Trig(ScanConfig config ) {


        }
    }
}
