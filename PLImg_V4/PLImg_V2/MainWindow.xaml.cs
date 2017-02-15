using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using DALSA.SaperaLT.SapClassBasic;
using DALSA.SaperaLT;
using Accord.Math;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.Wpf;

namespace PLImg_V2
{
    enum StageEnableState {
        Enabled,
        Disabled
        
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string[] XYZ = new string[3] { "X","Y","Z" };
        
        Core Core = new Core();
        public SeriesCollection seriesbox { get; set; }
        public ChartValues<int> chartV { get; set; }
        ImageBox[,] ImgBoxArr;
        StageEnableState XStageState;
        StageEnableState YStageState;
        StageEnableState ZStageState;

        Action<ScanMode> SetScanInfo;

        void InitFunc() {
            SetScanInfo = Core.ScanInfoSet(
                         ( int ) nudStartXPos.Value , ( int ) nudStartYPos.Value , ( int ) nudEndYPos.Value , ( double ) nudXstep.Value ,
                         -1 , -1 ,
                         ( int ) nudScanbuffNum.Value , ( int ) nudScanUnitNum.Value , ( int ) nudScanLineNum.Value ,
                         ( int ) nudScanSpeed.Value );
        }

        public MainWindow()
        {
            InitializeComponent();
            InitImgBox();
            SetImgBoxStretch();
            DataContext = this;
            ConnectionData cd = new ConnectionData();
            //Core.ConnectDevice( cd.CameraPath, cd.ControllerIP, cd.RStage );
            Core.ConnectDevice( cd.CameraPath, cd.DctStagePort, cd.RStage );
            InitMainMod();
        }

        #region Display
        void DisplayAF(double input)
        {
            lblAFV.Content = input.ToString("N4");
        }

        void DisplayRealTime(Image<Gray, byte> img)
        {
            imgboxReal.Image = img;
        }
    
        void DisplayBuffNumber(int num)
        {
            lblBuffNum.BeginInvoke(() => lblBuffNum.Content = num.ToString());
        }

        void DisplayFullScanImg(Image<Gray, Byte> img, int lineNum, int unitNum)
        {
            if ( lineNum < 4 && unitNum < 4 )
            {
                ImgBoxArr[unitNum , lineNum].Image = img;
            }
        }
        void SetImgBoxStretch()
        {
            foreach (var item in ImgBoxArr)
            {
                item.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        #endregion

        #region Init
        void InitMainMod( )
        {
            XStageState = StageEnableState.Enabled;
            YStageState = StageEnableState.Enabled;
            ZStageState = StageEnableState.Enabled;

            Core.evtRealimg       += new TferImgArr( DisplayRealTime );
            Core.evtSV            += new TferNumber( DisplayAF );
            Core.evtMapImg        += new TferSplitImgArr( DisplayFullScanImg );
            Core.evtFedBckPos     += new TferFeedBackPos( DisplayPos );
            Core.evtScanStart     += new TferScanStatus( ( ) => { Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait; } );
            Core.evtScanEnd       += new TferScanStatus( ( ) => { Mouse.OverrideCursor = null; } );
            Task.Run(()=>Core.GetFeedbackPos());

            imgboxReal.SizeMode = PictureBoxSizeMode.StretchImage;
            InitViewWin();
        }

        void InitImgBox()
        {
            ImgBoxArr = new ImageBox[4,4];
            ImgBoxArr[0, 0] = imgboxScan00;
            ImgBoxArr[0, 1] = imgboxScan01;
            ImgBoxArr[0, 2] = imgboxScan02;
            ImgBoxArr[0, 3] = imgboxScan03;

            ImgBoxArr[1, 0] = imgboxScan10;
            ImgBoxArr[1, 1] = imgboxScan11;
            ImgBoxArr[1, 2] = imgboxScan12;
            ImgBoxArr[1, 3] = imgboxScan13;

            ImgBoxArr[2, 0] = imgboxScan20;
            ImgBoxArr[2, 1] = imgboxScan21;
            ImgBoxArr[2, 2] = imgboxScan22;
            ImgBoxArr[2, 3] = imgboxScan23;

            ImgBoxArr[3, 0] = imgboxScan30;
            ImgBoxArr[3, 1] = imgboxScan31;
            ImgBoxArr[3, 2] = imgboxScan32;
            ImgBoxArr[3, 3] = imgboxScan33;
        }

        void ClearImgBox()
        {
            for (int i = 0; i < ImgBoxArr.GetLength(0); i++)
            {
                for (int j = 0; j < ImgBoxArr.GetLength(1); j++)
                {
                    ImgBoxArr[i, j].Image = null;
                }
            }
        }

        void InitViewWin( )
        {
            nudStartXPos.Value = 50;
            nudStartYPos.Value = 100;
            nudEndYPos.Value = 170;
            nudXstep.Value = 28.300;

            nudExtime.Value = 400;
            nudlinerate.Value = 4000;

            nudScanbuffNum.Value = 4;
            nudScanUnitNum.Value = 0;
            nudScanLineNum.Value = 0;
            nudScanSpeed.Value = 1;

            nudGoXPos.Value = 100;
            nudGoYPos.Value = 55;
            nudGoZPos.Value = 29.500;
        }

        void DisplayPos(double[] inputPos)
        {
            Task.Run( ( ) => lblXpos.BeginInvoke( (Action)(( ) => lblXpos.Content = inputPos[0].ToString("N4")) ) );
            Task.Run( ( ) => lblYpos.BeginInvoke( (Action)(( ) => lblYpos.Content = inputPos[1].ToString("N4")) ) );
            Task.Run( ( ) => lblZpos.BeginInvoke( (Action)(( ) => lblZpos.Content = inputPos[2].ToString("N4")) ) );
        }
        #endregion

        #region MainWindowEvent
        private void btnLineScan_Click(object sender, RoutedEventArgs e)
        {
            StartScan( ScanMode.SingleLine );
        }
        private void btnFullScan_Click(object sender, RoutedEventArgs e)
        {
            StartScan( ScanMode.MultiLine );
        }
        private void btnTrgScanStart_Click( object sender , RoutedEventArgs e )
        {
            StartScan( ScanMode.TrgCustom );
        }

        void StartScan( ScanMode mode ) {
            ClearImgBox();
            ScanDataSet( mode );
            Core.ReadyPos( mode == ScanMode.MultiLine || mode == ScanMode.SingleLine ? ScanTypes.NonTrig : ScanTypes.Trig );
            Core.ScanStart();
        }
        void ScanDataSet( ScanMode mode ) {
            if ( mode == ScanMode.MultiLine || mode == ScanMode.SingleLine )
            {
                Core.ScanInfoSet(
                ( int ) nudStartXPos.Value , ( int ) nudStartYPos.Value , ( int ) nudEndYPos.Value , ( double ) nudXstep.Value ,
                -1 , -1 ,
                ( int ) nudScanbuffNum.Value , ( int ) nudScanUnitNum.Value , ( int ) nudScanLineNum.Value ,
                ( int ) nudScanSpeed.Value )( mode );
            }
            else if ( mode == ScanMode.TrgCustom )
            {
                Core.ScanInfoSet(
                    ( int ) nudTrgYStart.Value ,
                    ( int ) nudTrgBuffNum.Value ,
                    ( int ) nudTrgXStep.Value ,
                    ( int ) nudTrgLineNum.Value ,
                    ( int ) nudTrgScanSpeed.Value)(mode);
            }
            else
            {
                Core.ScanInfoSet( ( int ) nudTrgYStart.Value )( mode );
            }

        }
            

        void ScanStart( ) { Mouse.OverrideCursor = Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;}
        void ScanEnd( ) { Mouse.OverrideCursor = null; }

        #region Camera
        private void btnGrap_Click(object sender, RoutedEventArgs e)
        {
            if ( Core == null ) Console.WriteLine( "null ");
            Core.Grab();
        }
        private void btnFreeze_Click( object sender, RoutedEventArgs e )
        {
            Core.Freeze();
        }
        private void btnSaveData_Click(object sender, RoutedEventArgs e)
        {
            string savePath = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ( fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                savePath = fbd.SelectedPath;
            }
            Core.SaveImageData( ImgBoxArr, savePath );
        }
        #endregion

        #region Stage
        // common //
        private void btnOrigin_Click( object sender, RoutedEventArgs e ) {
            foreach ( var item in XYZ ) Core.Stg.Home( item )();
        }

        // XYZStage //
        private void btnYMove_Click( object sender, RoutedEventArgs e )
        {
            if ( YStageState == StageEnableState.Enabled ) Core.MoveXYstg( "Y" , ( double ) nudGoYPos.Value );
        }
        private void btnXMove_Click( object sender, RoutedEventArgs e )
        {
            if ( XStageState == StageEnableState.Enabled ) Core.MoveXYstg( "X" , ( double ) nudGoXPos.Value );
        }
        private void btnZMove_Click( object sender, RoutedEventArgs e )
        {
            if(ZStageState == StageEnableState.Enabled) Core.MoveZstg( ( double ) nudGoZPos.Value );
        }

      

        // R Stage //
        private void btnRMove_Click( object sender, RoutedEventArgs e )
        {
            double pulse = (double)nudGoRPos.Value * 400;
            
        }
        private void btnROrigin_Click( object sender, RoutedEventArgs e )
        {
           
        }
        private void btnRForceStop_Click( object sender, RoutedEventArgs e )
        {
            
        }
        #endregion

        #endregion

        #region Motor Enable / Disable // Done
        private void ckbXDisa_Checked( object sender, RoutedEventArgs e ) {
            Core.Stg.Disable("X")();
            XStageState = StageEnableState.Disabled;
        }

        private void ckbYDisa_Checked( object sender, RoutedEventArgs e ) {
            Core.Stg.Disable( "Y" )();
            YStageState = StageEnableState.Disabled;
        }

        private void ckbZDisa_Checked( object sender, RoutedEventArgs e ) {
            Core.Stg.Disable( "Z" )();
            ZStageState = StageEnableState.Disabled;
        }

        private void ckbZDisa_Unchecked( object sender, RoutedEventArgs e ) {
            Core.Stg.Enable( "Y" )();
            ZStageState = StageEnableState.Enabled;
        }

        private void ckbYDisa_Unchecked( object sender, RoutedEventArgs e ) {
            Core.Stg.Enable( "Y" )();
            YStageState = StageEnableState.Enabled;
        }

        private void ckbXDisa_Unchecked( object sender, RoutedEventArgs e ) {
            Core.Stg.Enable( "X" )();
            XStageState = StageEnableState.Enabled;
        }
        #endregion

        #region Sscan data Setting 

        private void nudlinerate_KeyUp( object sender, System.Windows.Input.KeyEventArgs e ) {
            if ( e.Key != System.Windows.Input.Key.Enter ) return;
            Core.LineRate( (int)nudlinerate.Value );
        }

        private void nudExtime_KeyUp( object sender , System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key != System.Windows.Input.Key.Enter ) return;
            Core.Exposure( ( int ) nudExtime.Value );
        }

        #endregion

        #region window Event 
        private void MetroWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            foreach ( var item in XYZ )
            {
                Core.Stg.Disable( item )();
                Core.Stg.Disconnect();
            }
            //Core.RStageClose();
        }
        #endregion

        #region Image event
        private void imgboxScan00_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan01_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan02_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan03_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan10_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan11_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan12_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan13_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan20_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan21_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan22_Click( object sender , EventArgs e )
        {

        }

        private void imgboxScan23_Click( object sender , EventArgs e )
        {

        }


        #endregion

        
    }
}
