using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALSA.SaperaLT.SapClassBasic;
using NationalInstruments.VisaNS;
using CommonExtension;
using MachineControl;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguCV_Extension;
using System.Drawing;
using static EmguCV_Extension.Vision_Tool;

namespace PLImaging
{
    public delegate void TrsBuffData( byte [,,] src);

    public class Flow
    {
        static Cam_Dalsa   Cam;
        static Stg_ACSController Stg;
        static SettingData SetData;

        public event TrsBuffData evtTrsScanedImage;
        SapXferNotifyHandler GrabDoneMethod =>
            ( s , e ) => evtTrsScanedImage( Cam.BuffGetAllE()
                                          .Reshape( Cam.Buffers.Height 
                                                    ,Cam.Buffers.Width
                                                    ,1 ));

        //static SapXferNotifyHandler GrabDoneMethod;
        public Flow()
        {
            Cam = new Cam_Dalsa( GrabDoneMethod );
            Stg = new Stg_ACSController();
        }

        #region Stage
        Task<bool?> ImportStartData = new Task<bool?>(()=>
        {
            return null;
        });

        Task<bool?> CamConnect = new Task<bool?>( () =>
        {
            Cam.ConnectE(SetData.CamPath);
            return true;
        } );

        Task<bool?> CamDisconnect = new Task<bool?>( () =>
        {
            Cam.DisconnectE();
            return true;
        } );

        Task<bool?> StgConnect = new Task<bool?>( () =>
        {
            return null;
        } );

        Task<bool?> CamChangeConfig = new Task<bool?>(()=>
        {

            return null;
        } );

        Task<bool?> StgOrigin =  new Task<bool?>( ()=>
        {

            return null;
        } );

        public Task<bool?> MoveStartOrigin( ScanConfig config , int count)
        {
            Task<bool?> scanStartPos = Task.Run<bool?>(() => {
                Stg.MoveAbsE("X" , SetData.StartXPos[config])
                   .MoveAbsE("Y" , SetData.StartYPos[config] + SetData.XStep_Size * count)
                   .WaitStatusE("X")
                   .WaitStatusE("Y");
                return true;
            } );
            return scanStartPos;
        }

        public Task<bool?> ScanMomenet( ScanConfig config ,int trigerBufNum)
        {
            Task<bool?> startScan =  Task.Run<bool?>( ()=>
                {
                    Cam.Grab();

                    Stg.StartTriggerE( trigerBufNum )
                       .MoveAbsE( "Y" , SetData.EndYPos[config] )
                       .WaitStatusE( "Y" )
                       .StopTriggerE( trigerBufNum );

                    return true;
                } );
            return startScan;
        }
        #endregion

        // ------------------ not ------------ //

        public Task<bool?> StartProcessing()
        {
            // preprocessing => DrawBox
            // Extarct Feature => DrawIndex
            // Draw Contour
            // Draw Index 
            return null;
        }

        // 이걸 하나하나 생성? 플래그로 선택? 
        //public async Task<byte [ , , ]> PreProcessing(byte[,,] input  )
        //{
        //    
        //
        //}

        //public Task<> ExtractFeature = new Task<TResult>

        #region Image
        public Task<byte [ , , ]> ReszieImage( byte [ , , ] input , int h , int w )
        {
            return Task.Run<byte [ , , ]>( () =>
            {
                return new Image<Gray , byte>( input )
                    .Resize( w , h , Emgu.CV.CvEnum.Inter.Nearest )
                    .Data;
            } );
        }

        public Task<byte [ , , ]> HStacks( byte[][ , , ] input )
        {
            return Task.Run<byte [ , , ]>( () =>
            {
               return input.Aggregate( ( f , s ) => 
                                 new Image<Gray , byte>( f )
                                 .HStack( new Image<Gray,byte>(s) ).Data );
            } );
        }

        #endregion

        #region Processing

        public Task<byte[,,]> Threshold( byte [ , , ] input , int thres )
        {
            return Task.Run<byte [ , , ]>( () => 
                FnThreshold( ThresholdMode.Manual )
                           ( new Image<Gray , byte>( input ) , thres )
                .Data );
        }

        public Task<List<System.Drawing.Rectangle>> GetBoxList( byte [ , , ] input , int areaup, int areadw )
        {
            return Task.Run<List<System.Drawing.Rectangle>>( () =>
             {
                 return input.Map( x => new Image<Gray , byte>( x ) )
                             .Map( x => FnFindContour( areaup , areadw )( x ) )
                             .Map( x => FnSortcontours()(x) )
                             .Map( x => FnApplyBox( areaup , areadw )( x ) );
             } );

        }

        public Task<Point[][]> GetContourList( byte [ , , ] input , int areaup , int areadw )
        {
            return Task.Run<Point[][]>( () =>
            {
                return input.Map( x => new Image<Gray , byte>( x ) )
                            .Map( x => FnFindContour( areaup , areadw )( x ) )
                            .ToArrayOfArray();
            } );
        }


        #endregion




    }
}
