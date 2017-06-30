using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALSA.SaperaLT.SapClassBasic;
using NationalInstruments.VisaNS;
using CommonExtension;
using MachineControl;

namespace PLImg_V2
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
        public Task<byte [ , , ]> PreProcessing(byte[,,] input  )
        {
            return new Task<byte [ , , ]>(()=> {


                return null; 
            } );

        }

        //public Task<> ExtractFeature = new Task<TResult>






    }
}
