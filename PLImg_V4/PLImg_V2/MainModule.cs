using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ISigmaKokiStage;
using DALSA.SaperaLT.SapClassBasic;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Accord.Math;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;


namespace PLImg_V2
{
    enum GrabStatus { Frea,Grab }
    enum ScanDirection { Forward,Back }

    //enum ScanState { Start, Pause, Stop, Wait}

    public delegate void TransbyteArr(byte[] imgarr);
    public delegate void TransImgArr(Image<Gray,Byte> img);
    public delegate void TransNumber(int num);
    public delegate void TransDoubleNumber(double num);
    public delegate void TransScanStatus(); 
    public delegate void TransSplitImgArr(Image<Gray, Byte> img,int lineNum,int unitNum);
    public delegate void TransLineBuffNum(int linenum, int unitnum);
    public delegate void TransFeedBackPos(double[] XYZPos);

    public class MainModule
    {
        readonly int YStep = 100;

        public event TransbyteArr       evtByteArrOneLine     ;
        public event TransImgArr        evtRealimg            ;
        public event TransSplitImgArr   evtFScanImgOnGoing    ;
        public event TransScanStatus    evtScanStart          ;
        public event TransScanStatus    evtScanEnd            ;
        public event TransDoubleNumber  evtVarianceValue      ;
        public event TransFeedBackPos   evtFeedbackPos        ;

        public FullScanData          DataFullScan    ;

        Timer                 ProfileTimer    ;
        MessageBasedSession   MbSession       ;
        AFCalc                AFMeasrue       ;
        
        GrabMana              GrabM           ;
        DalsaMember           DalsaMemObj     ;
        GrabStatus            StatusGrab      ;
        ScanState             ScanStatus      ;
        CameraSetting         CameraSet       ;
        SigmaRStageControl    RStageController;
        AcsContol             AcsXYZControl   ;

        Image<Gray, byte>     CurrentImg      ;
        byte[]                ImgSrcByte      ;
        int                   BuffCount       ;
        int                   LineCount       ;
        int                   UnitCount       ;
        double[]              FeedBackPos     ;
        int StartP;
        int EndP;

        string ImgBasePath = "C:\\ImgFullScanTemp\\" ;

        public MainModule()
        {
            DalsaMemObj              = new DalsaMember()        ;
            GrabM                    = new GrabMana(ImgBasePath);
            CameraSet                = new CameraSetting()      ;
            Connection DalsaConnect  = new Connection()         ;
            AFMeasrue                = new AFCalc()             ;
            DataFullScan             = new FullScanData()       ;
            AcsXYZControl             = new AcsContol()          ;
            
            CreateDeviceMana(DalsaConnect);
            TimerSetting();
            ScanStatus = ScanState.Wait;
            FeedBackPos = new double[3];
            
        }


        #region Event Method
        Image<Gray, byte> Buff2Img(byte[] data, int count)
        {
            Image<Gray, byte> buffImgData = new Image<Gray, byte>(DalsaMemObj.Buffers.Width, DalsaMemObj.Buffers.Height * count);
            buffImgData.Bytes = data;
            return buffImgData;
        }

        bool needClearBuf = true;

        void GrabDoneEventMethod(object sender, SapXferNotifyEventArgs evt)
        {
            #region New
            switch (ScanStatus) {
                case ScanState.Stop:
                    ScanStatus = ScanState.Wait;
                    needClearBuf = true;
                    Freeze();
                    System.Threading.Thread.Sleep( 100 );
                    Grab();
                    //evtScanEnd();
                    break;

                case ScanState.Pause:
                    double currentYPos = AcsXYZControl.GetMotorFPos()[1];
                    AcsXYZControl.Wait2ArriveEpsilon( "X", DataFullScan.PosXEnd, 0.02 );
                    AcsXYZControl.SetXSpeed( 200 );
                    System.Threading.Thread.Sleep( 80 );
                    AcsXYZControl.XMove( DataFullScan.PosXStart );
                    AcsXYZControl.Wait2ArriveEpsilon( "X", DataFullScan.PosXStart, 0.02 );
                    AcsXYZControl.YMove( DataFullScan.PosYStart - DataFullScan.YStep * LineCount );
                    AcsXYZControl.Wait2ArriveEpsilon( "Y", DataFullScan.PosYStart - DataFullScan.YStep * LineCount, 0.02);
                    AcsXYZControl.SetXSpeed( DataFullScan.ScanSpeed );
                    AcsXYZControl.XMove( DataFullScan.PosXEnd );
                    ScanStatus = ScanState.Start;
                    needClearBuf = true;
                    break;

                case ScanState.Start:
                    Console.WriteLine( "start" );
                    //evtScanStart();
                    AcsXYZControl.XMove( DataFullScan.PosXEnd );
                    if ( needClearBuf ) {
                        Freeze();
                        System.Threading.Thread.Sleep( 30 );
                        DalsaMemObj.Buffers.Clear();
                        needClearBuf = false;
                        Grab();
                        
                    }
                    byte[] buffData3 = GrabM.DataTransFromBuffer(DalsaMemObj.Buffers);
                    evtRealimg( Buff2Img( buffData3, 1 ) );
                    ImgSrcByte = Matrix.Concatenate<byte>(ImgSrcByte, buffData3);
                    //SaveFullDat(ImgSrcByte,LineCount,UnitCount,BuffCount);
                    evtFScanImgOnGoing(Buff2Img(ImgSrcByte, BuffCount+1), LineCount, UnitCount);
                    //evtlineUnitNum(LineCount, UnitCount); // for Watch

                    if (BuffCount == DataFullScan.BuffLimit)
                    {
                        ImgSrcByte = null;
                        ImgSrcByte = new byte[0];
                        BuffCount = 0;

                        if (UnitCount == DataFullScan.UnitLimit)
                        {
                            UnitCount = 0;
                            if (LineCount == DataFullScan.LineLimit)
                            {
                                ScanStatus = ScanState.Stop;
                                break;
                            }
                            else
                            {
                                LineCount += 1;
                                ScanStatus = ScanState.Pause;
                            }
                            break;
                        }
                        else { UnitCount += 1; }
                    }
                    else { BuffCount++; }
                    break;

                default:
                    Console.WriteLine( "Defualt Grab" );
                    byte[] buffData4 = GrabM.DataTransFromBuffer(DalsaMemObj.Buffers);
                    evtRealimg( Buff2Img( buffData4, 1 ) );
                    break;

            }
            #endregion
        }

        async void TempSaveCurrentImg(byte[] data, int count)
        {
            CurrentImg = await Task.Run(()=> Buff2Img(data, count));
        }

        void ProfileEventMethod(object sender, EventArgs e)
        {
            byte[] sliced = GrabM.DataTransFromBufferOneLine(DalsaMemObj.Buffers);
            evtVarianceValue( AFMeasrue.CalcAFV( sliced ) );
            evtByteArrOneLine( sliced );
        }

        async void SaveLineDat(byte[] inputArr, int buffNum, int scanNum)
        {
            await Task.Run(() => GrabM.TempSaveGrabDataLineMode(inputArr, buffNum, scanNum));
        }
        async void SaveFullDat(byte[] inputArr, int lineNum, int unitNum, int buffNum)
        {
            await Task.Run(() => GrabM.TempSaveGrabDataFullMode(inputArr, lineNum, unitNum, buffNum));
        }
        #endregion

        #region Feedback
        public void GetFeedbackPos()
        {
            while ( true )
            {
                evtFeedbackPos( AcsXYZControl.GetMotorFPos() );
                Task.Delay( 500 ).Wait();
            }
        }
        #endregion

        #region Scanning
        public void SetScanData(double xs,double ys,double xe ) {
            DataFullScan.PosXStart = xs;
            DataFullScan.PosYStart = ys;
            DataFullScan.PosXEnd = xe;
        }

        void SetStartEnd( int startposX , int endposX )
        {
            StartP = startposX;
            EndP = endposX;
        }

        public async void StartLineScan(int startposX, int endposX , int speed)
        {
            ReadyLineScan( startposX );
            DataFullScan.LineLimit = 0;
            InitCount();
            SetDir();
            await Task.Run(()=> {
                ImgSrcByte = new byte[0];
                System.Threading.Thread.Sleep( 2000 );
                AcsXYZControl.XMove( endposX );
                System.Threading.Thread.Sleep( 2500 );
                ScanStatus = ScanState.Start;
            });
        }

        async public void ReadyLineScan( int startpos )
        {
            AcsXYZControl.SetXSpeed( 200 );
            AcsXYZControl.XMove( startpos );
            AcsXYZControl.Wait2ArriveEpsilon( "X", startpos, 1.0 );
            AcsXYZControl.SetXSpeed( DataFullScan.ScanSpeed );
            await Task.Delay( 1500 );
        }

        async public void ReadyFullScan( int startxpos , int startypos ) {
            AcsXYZControl.SetXSpeed( 200 );
            AcsXYZControl.XMove( startxpos );
            AcsXYZControl.YMove( startypos );
            AcsXYZControl.Wait2ArriveEpsilon( "X", startxpos, 0.02 );
            AcsXYZControl.Wait2ArriveEpsilon( "Y", startypos, 0.02 );
            AcsXYZControl.SetXSpeed( DataFullScan.ScanSpeed );
            Console.WriteLine( "Full scan ready done" );
            await Task.Delay( 1500 );
        }

        public async void StartFullScan(int startposX, int startposY,int endposX, int xSpeed)
        {
            Console.WriteLine( "Full scan start" );
            InitCount();
            SetDir();
            SetScanData( startposX, startposY, endposX );
            ReadyFullScan( startposX, startposY );
            await Task.Run(() => {
                ImgSrcByte = new byte[0];
                //AcsXYZControl.XMove( endposX );
                BeginScanning(endposX, startposY);
            });
        }

        public void SetLinelimit(int value ) {
            DataFullScan.LineLimit = value;
        }

        void SetDir()
        {
            string dirTempPath = String.Format(ImgBasePath + DateTime.Now.ToString("MM/dd/HH/mm/ss"));
            CheckAndCreateFolder cacf = new CheckAndCreateFolder(dirTempPath);
            cacf.SettingFolder( dirTempPath );
            GrabM.SetDirPath( dirTempPath );
        }

        void LlineScanInit( int startposX )
        {
            AcsXYZControl.SetSpeed( 100 , 100 , 100 );
            AcsXYZControl.XMove( startposX );
            System.Threading.Thread.Sleep( 2000 );
        }

        public void FullscanInit( double ysstep ) {
            DataFullScan.YStep = ysstep;
        }

        void CheckGrabStatus()
        {
            if (StatusGrab == GrabStatus.Frea)
            {
                if (DalsaMemObj.Xfer != null)
                {
                    StatusGrab = GrabStatus.Grab;
                    DalsaMemObj.Xfer.Grab();
                }
            }
        }

        void BeginScanning(int posX,int posY)
        {
            Console.WriteLine( "Full scan / startus = start" );
            ScanStatus = ScanState.Start;
        }

        #endregion

        #region Order
        public void Grab()
        {
            if (DalsaMemObj.Xfer != null)
            {
                Console.WriteLine( "grab Start" );
                StatusGrab = GrabStatus.Grab;
                DalsaMemObj.Xfer.Grab();
                //ProfileTimer.Start();
            }
        }

        public void Freeze()
        {
            if (DalsaMemObj.Xfer != null)
            {
                Console.WriteLine( "freeze" );
                StatusGrab = GrabStatus.Frea;
                DalsaMemObj.Xfer.Freeze();
                DalsaMemObj.Xfer.Wait(800);
                ProfileTimer.Stop();
            }
        }

        public void SaveImg(string path)
        {
            if (CurrentImg != null) CurrentImg.ToBitmap().Save(path);
            else
            {
                MessageBox.Show("Image is not Saved");
            }
        }

        public void DisposeMem()
        {
            var creatobject = new CreatesObjects();
            creatobject.DestroysObjects(DalsaMemObj);
        }

        #endregion

        #region XYStageOrder
        public void EnableStage( int axis ) { AcsXYZControl.EnableMotor(axis); }

        public void DisableStage( int axis ) {
                AcsXYZControl.DisableMotor( axis );
        }

        public void XYOrigin()
        {
        }

        public void XMoveAbsPos( double posX )
        {
            AcsXYZControl.SetXSpeed( 400 );
            System.Threading.Thread.Sleep( 100 );
            AcsXYZControl.XMove( posX );
            AcsXYZControl.Wait2ArriveEpsilon( "X", posX, 0.2 );
            AcsXYZControl.SetXSpeed( DataFullScan.ScanSpeed );

        }

        public void disz( ) {
            AcsXYZControl.DisZ();
        }

        public void YMoveAbsPos( double posY )
        {
            AcsXYZControl.SetYSpeed( 400 );
            System.Threading.Thread.Sleep( 100 );
            AcsXYZControl.YMove( posY );
            AcsXYZControl.Wait2ArriveEpsilon( "Y", posY, 0.2 );
        }

        public void XYWait2Arrive(int targetPosX,int targetPosY)
        {
            Console.WriteLine( "arrived come in " );
        }

        public void XYSetSpeed(int speedX, int speedY)
        {
            AcsXYZControl.SetSpeed( speedX, speedY , 100 );
        }

        public void HaltStage( ) {
            AcsXYZControl.Halt();
        }

        public void Home( ) {
            AcsXYZControl.Home();
        }

        #endregion

        #region Camera Setting
        public void SetExposure(double inputValue)
        {
            CameraSet.SetExposureTime(MbSession, inputValue);
        }

        public void SetLineRate( int inputValue)
        {
            CameraSet.SetLineRate(MbSession, inputValue);
        }


        #endregion

        #region ZStageOrder
        public void ZOrigin()
        {
            
        }

        public void ZMoveAbsPos(double posZ)
        {
            AcsXYZControl.ZMove( posZ );
        }

        public void ZMoveRelPos(double posZ ) {
            AcsXYZControl.ZMoveRel( posZ );
        }

        public void ZWait2Arrive(int targetPosZ)
        {
            
        }

        public void SetSpeed(int speedX,int speedY, int speedZ)
        {
            AcsXYZControl.SetSpeed(speedX,speedY,speedZ);
        }

        public void Buffclear( ) { }

        #endregion

        #region RStage Order
        public void ROrigin( )
        {
            RStageController.Origin();
        }

        public void RMoveAbsPos( double posR )
        {
            RStageController.MoveAbsPos( posR );
        }

        public void RSetSpeed( int speedR, int accR )
        {
            RStageController.SetRSpeed( speedR, accR );
        }

        public string RGetPosition( ) {
            return RStageController.GetPosition();
        }

        public string RPositionRead( ) {
            string fir = RStageController.GetPosition();
            var splitarr = fir.Split( ',' );
            try
            {
                var output = Convert.ToDouble(splitarr[0]) / 400 ;
                return output.ToString();
            }
            catch ( Exception )
            {
                return  "999.99";
            }
        }

        public void RStageClose( ) {
            RStageController.RstageRelease();
        }
        #endregion

        #region Init
        void TimerSetting()
        {
            ProfileTimer = new Timer();
            ProfileTimer.Interval = 500;
            ProfileTimer.Tick += new EventHandler(ProfileEventMethod);
        }

        bool CreateDeviceMana(Connection connectModule)
        {
            try
            {
                DalsaMemObj.ServerLocation = new SapLocation(connectModule.ServerName, connectModule.ResourceIndex);
                DalsaMemObj.Acquisition = new SapAcquisition(DalsaMemObj.ServerLocation, connectModule.ConfigFile);

                if (SapBuffer.IsBufferTypeSupported(DalsaMemObj.ServerLocation, SapBuffer.MemoryType.ScatterGather))
                    DalsaMemObj.Buffers = new SapBufferWithTrash(2, DalsaMemObj.Acquisition, SapBuffer.MemoryType.ScatterGather);
                else
                    DalsaMemObj.Buffers = new SapBufferWithTrash(2, DalsaMemObj.Acquisition, SapBuffer.MemoryType.ScatterGatherPhysical);

                var objSetting = new ObjectSetting();
                objSetting.AcqusitionSetting(DalsaMemObj.Acquisition);

                DalsaMemObj.Xfer = new SapAcqToBuf(DalsaMemObj.Acquisition, DalsaMemObj.Buffers);
                DalsaMemObj.Xfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;

                DalsaMemObj.View = new SapView(DalsaMemObj.Buffers);

                DalsaMemObj.Xfer.XferNotify += new SapXferNotifyHandler(GrabDoneEventMethod);
                DalsaMemObj.Xfer.XferNotifyContext = DalsaMemObj.View;

                var creatobject = new CreatesObjects();
                creatobject.CreatEndSqObject(DalsaMemObj.Buffers, DalsaMemObj.Xfer, DalsaMemObj.View);
                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public void XYZStageInit(string addIP)
        {
            AcsXYZControl.Connect( addIP );
            //AcsXYZControl.Home();
        }

        public void XYZStageInitCom( string comport ) {
            AcsXYZControl.ConnectCom( comport );
            //AcsXYZControl.Home();
        }

        public void RStageInit( int port )
        {
            //RStageControler = new DummySigmaRStageControl();
            RStageController = new SigmaRStageControl();
            RStageController.RStageConnect( port.ToString() );
            //if ( !RStageController.RStageConnect( port.ToString() ) ) { RStageController = new DummySigmaRStageControl(); }
        }

        public void ConnectVISA2Cam(string path)
        {
            ConnectVISA visaConnection = new ConnectVISA();
            visaConnection.Connect2VISA(ref MbSession, path);
        }

        void InitCount()
        {
            BuffCount = 0;
            UnitCount = 0;
            LineCount = 0;
        }
       
        #endregion

        #region Save
        public void SaveImageData( Emgu.CV.UI.ImageBox[,] imgbox,string savepath )
        {
            try
            {

                for ( int i = 0; i < imgbox.GetLength( 0 ); i++ )
                {
                    for ( int j = 0; j < imgbox.GetLength( 1 ); j++ )
                    {
                        if ( imgbox[i, j].Image != null )
                        {
                            string temp = i.ToString( "D2" ) + "_"+j.ToString( "D3" );
                            string outpath = System.IO.Path.Combine( savepath, temp );
                            imgbox[i, j].Image.Save( String.Format( outpath + ".bmp" ) );
                        }
                    }
                }

            }
            catch ( Exception ex)
            {
                MessageBox.Show( ex.ToString() );
            }
        }
        #endregion

        #region fn 


        #endregion

        public void TestMethod() {
            DataFullScan.LineLimit = 0;
            ScanStatus = ScanState.Start;
            DalsaMemObj.Xfer.Grab();
            System.Threading.Thread.Sleep( 3000 );
            DalsaMemObj.Xfer.Freeze();
            System.Threading.Thread.Sleep( 100 );
            DalsaMemObj.Buffers.Clear();
        }

    }
}
