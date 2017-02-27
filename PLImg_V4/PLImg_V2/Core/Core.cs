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
using PLImg_V2.Data;

namespace PLImg_V2
{
    public partial class Core
    {
        #region Event
        public event TferImgArr        evtRealimg   ;
        public event TferSplitImgArr   evtMapImg    ;
        public event TferTrgImgArr     evtTrgImg    ;
        public event TferFeedBackPos   evtFedBckPos ;
        public event TferScanStatus    evtScanEnd   ;
        public event TferNumber        evtSV        ;     
        #endregion  

        public DalsaPiranha3_12k Cam = new DalsaPiranha3_12k();
        public AcsCtrlXYZ Stg        = new AcsCtrlXYZ();
        public ScanInfo Info         = new ScanInfo();
        public TrgScanInfo TrgInfo   = new TrgScanInfo();
        public TriggerScanData TrigScanData = new TriggerScanData();
        Indicator Idc = new Indicator();


        /*GFunc*/
        public Dictionary<ScanConfig , Action> Reconnector = new Dictionary<ScanConfig, Action>();
        public Dictionary<ScanConfig , Action> ExposureMode = new Dictionary<ScanConfig, Action>();
        public Dictionary<string,Action> StgEnable;

        public Action Connect_XYZStage;
        public Action<double> LineRate;
        public Action<double> Exposure;
        public Action         Exposure_NonTrgMode; 
        public Action         Exposure_TrgMode; 
        public Action         Grab;
        public Action         Freeze;
        public Action         BufClear;

        public Func<byte[]>   FullBuffdata;
        public Func<byte[]>   SingleBuffdata;
        public Func<byte[], int, Image<Gray, byte>> Reshape2D;

        #region Init
        public Action<ScanConfig> ConnectDevice( string camPath , string stgPath , string rstagPath )
        {
            Create_Connector( camPath , stgPath , rstagPath );
            return new Action<ScanConfig>( ( config ) => {
                Reconnector[config]();
                ExposureMode[config]();
                Connect_XYZStage();
                InitFunc();
                InitData();
                foreach ( var item in StgEnable ) item.Value();
                Reshape2D = FnBuff2Img( ImgWH["H"], ImgWH["W"] );
            } );
        }


        public void Create_Connector( string camPath , string stgPath , string rstagPath )
        {
            Reconnector.Add( ScanConfig.nonTrigger , Cam.Connect( camPath , ScanConfig.nonTrigger ) );
            Reconnector.Add( ScanConfig.Trigger_1 ,  Cam.Connect( camPath , ScanConfig.Trigger_1 ) );
            Reconnector.Add( ScanConfig.Trigger_2 ,  Cam.Connect( camPath , ScanConfig.Trigger_2 ) );
            Reconnector.Add( ScanConfig.Trigger_4 ,  Cam.Connect( camPath , ScanConfig.Trigger_4 ) );

            ExposureMode.Add( ScanConfig.nonTrigger, Cam.ExposureMode( 2 ) );
            ExposureMode.Add( ScanConfig.Trigger_1,  Cam.ExposureMode( 6 ) ); 
            ExposureMode.Add( ScanConfig.Trigger_2,  Cam.ExposureMode( 6 ) ); 
            ExposureMode.Add( ScanConfig.Trigger_4,  Cam.ExposureMode( 6 ) ); 


            var stgConnectMode = MachineControl.Stage.Interface.ConnectMode.IP;
            Connect_XYZStage = Stg.Connect( stgPath , stgConnectMode );
        }


        public void InitFunc()
        {
            Cam.EvtResist( Cam.Xfer , GrabDoneEvt_Non );
            Exposure = Cam.Exposure();
            LineRate = Cam.LineRate();
            Exposure_NonTrgMode = Cam.ExposureMode( 2 );
            Exposure_TrgMode    = Cam.ExposureMode( 6 );


            Grab = Cam.Grab();
            Freeze = Cam.Freeze();
            BufClear = Cam.BuffClear();
            FullBuffdata = Cam.BuffGetAll( Cam.Buffers );
            SingleBuffdata = Cam.BuffGetLine( Cam.Buffers );

            StgEnable = new Dictionary<string , Action>();
            foreach ( var item in GD.YXZ ) StgEnable.Add( item , Stg.Enable( item ) );
        }

        public void InitData()
        {
            ImgWH = Cam.GetBuffWH();
        }

        #endregion

        #region GrabDoneEvent Method
        void GrabDoneEvt_Non( object sender , SapXferNotifyEventArgs evt )
        {
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
                    StartProcess( ScanType );
                    break;

                default:
                    evtRealimg( Reshape2D( FullBuffdata() , 1 ) );
                    Task.Run( () => TferVariance( SingleBuffdata() ) );
                    break;
            }
        }

        void GrabDoneEvt_Trg( object sender , SapXferNotifyEventArgs evt )
        {
            var Buf2Img = FnBuff2Img( Cam.GetBuffWH()["H"] , Cam.GetBuffWH()["W"] );
            var currentbuff = FullBuffdata();
            evtTrgImg( Buf2Img( currentbuff , 1 ) , TrigCount ); // 1 Trigger = 1 Buffer
            Freeze();
            TrigCount += 1;
            if ( TrigCount < TrigLimit )
            {
                StgReadyTrigScan( TrigCount );
                Grab();
                System.Threading.Thread.Sleep( 100 );
                MoveXYstg( "Y" , TrigScanData.EndYPos[CurrentConfig] );
            }
            else {
                evtScanEnd();
            }
        }
        #endregion




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

     

        public void StartTrigScan( ScanConfig config ) {
            CurrentConfig = config;
            TrigLimit = SetTriggerLimit( config );
            TrigCount = 0;
            StgReadyTrigScan( 0 );

            System.Threading.Thread.Sleep( 100 );
            ResetCamCofnig( config );

            Grab();
            System.Threading.Thread.Sleep( 100 );

            MoveXYstg( "Y" , TrigScanData.EndYPos[config] );
        }

        void StgReadyTrigScan(int triggerNum)
        {
            MoveXYstg( "Y" , TrigScanData.StartYPos );
            MoveXYstg( "X" , TrigScanData.StartXPos + TrigScanData.XStep_Size* triggerNum );
            Stg.WaitEps( "Y" )( TrigScanData.StartYPos , 0.005 );
            Stg.WaitEps( "X" )( TrigScanData.StartYPos , 0.005 );
            Stg.SetSpeed( "Y" )( TrigScanData.Scan_Stage_Speed);
        }

        void ResetCamCofnig(ScanConfig config) {
            Reconnector[config]();
            Cam.EvtResist(Cam.Xfer , GrabDoneEvt_Trg);
        }

        int SetTriggerLimit( ScanConfig config ) {
            switch ( config ) {
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
