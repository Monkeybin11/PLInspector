using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALSA.SaperaLT.SapClassBasic;
using NationalInstruments.VisaNS;
using CommonExtension;
using System.Runtime.InteropServices;

namespace MachineControl
{
    public enum ScanConfig { Free , Area , Trigger_1, Trigger_2, Trigger_4 }

    public class Cam_Dalsa : Camera_Shape
    {
        #region data
        MessageBasedSession mbSession       ;
        SapLocation     ServerLocation      ;
        SapAcqDevice    AcqDevice           ;
        SapAcquisition  Acquisition         ;
        SapView         View                ;
        public SapBuffer       Buffers      ;
        public SapAcqToBuf     Xfer         ;

        public string ConfigFileName        ;
        public string ConfigFile            ;

        SapXferNotifyHandler GrabDoneFunc   ;

        static readonly string ServerName     = "Xcelera-HS_PX8_1";
        static readonly int    ResourceIndex  = 0;
        static readonly string ConfigFile_Non   =  "T__tdi_non.ccf";
        static readonly string ConfigFile_Area  =  "T__tdi_Area.ccf";
        static readonly string ConfigFile_1     =  "T__tdi_1inch.ccf";
        static readonly string ConfigFile_2     =  "T__tdi_2inch.ccf";
        static readonly string ConfigFile_4     =  "T__tdi_4inch.ccf";
        static readonly string ConfigFileNameBase = @"C:\Program Files\Teledyne DALSA\Sapera\CamFiles\User\";
        #endregion

        #region init
        public Cam_Dalsa( SapXferNotifyHandler grabdoneEvtfun )
        {
            GrabDoneFunc = grabdoneEvtfun;
        }

        public override bool? Connect( string path )
        {
            Disconnect();
            LoadConfig( 0 );
            CreateCamObj();
            return true;
        }
        public override bool? Disconnect()
        {
            try
            {
                if ( Xfer != null )
                {
                    Xfer.Destroy();
                    Xfer.Dispose();
                }

                if ( AcqDevice != null )
                {
                    AcqDevice.Destroy();
                    AcqDevice.Dispose();
                }

                if ( Acquisition != null )
                {
                    Acquisition.Destroy();
                    Acquisition.Dispose();
                }

                if ( Buffers != null )
                {
                    Buffers.Destroy();
                    Buffers.Dispose();
                }

                if ( View != null )
                {
                    View.Destroy();
                    View.Dispose();
                }
                if ( ServerLocation != null ) ServerLocation.Dispose();
                return true;

            }
            catch ( Exception ex)
            {
                ex.ToString().Print("Disconnect Error");
                return null;
            }


        }
        protected override bool? LoadConfig( int value = 0 )
        {
            switch ( value )
            {
                case 0:
                    ConfigFileName = ConfigFile_Area;
                    ConfigFile = ConfigFileNameBase + ConfigFileName;
                    break;

                case 1:
                    ConfigFileName = ConfigFile_Non;
                    ConfigFile = ConfigFileNameBase + ConfigFileName;
                    break;

                case 2:
                    ConfigFileName = ConfigFile_1;
                    ConfigFile = ConfigFileNameBase + ConfigFileName;
                    break;

                case 3:
                    ConfigFileName = ConfigFile_2;
                    ConfigFile = ConfigFileNameBase + ConfigFileName;
                    break;
                case 4:
                    ConfigFileName = ConfigFile_4;
                    ConfigFile = ConfigFileNameBase + ConfigFileName;
                    break;
            }
            return true;
        }

        protected override bool? CreateCamObj()
        {
            ServerLocation = new SapLocation( ServerName , ResourceIndex );
            Acquisition = new SapAcquisition( ServerLocation , ConfigFile );

            if ( SapBuffer.IsBufferTypeSupported( ServerLocation , SapBuffer.MemoryType.ScatterGather ) )
                Buffers = new SapBufferWithTrash( 2 , Acquisition , SapBuffer.MemoryType.ScatterGather );
            else
                Buffers = new SapBufferWithTrash( 2 , Acquisition , SapBuffer.MemoryType.ScatterGatherPhysical );

            Acquisition.Create();

            Xfer = new SapAcqToBuf( Acquisition , Buffers );
            Xfer.Pairs [ 0 ].EventType = SapXferPair.XferEventType.EndOfFrame;

            View = new SapView( Buffers );
            return true;
        }

        public override Dictionary<string , int?> GetBuffWH()
        {
            var output = new Dictionary<string,int?>();
            output.Add( "H" , Buffers?.Height );
            output.Add( "W" , Buffers?.Width );
            return output;
        }
        public override bool? EvtResist( )
        {
            if( GrabDoneFunc == null ) return null;
            Xfer.XferNotify += new SapXferNotifyHandler( GrabDoneFunc );
            Xfer.XferNotifyContext = View;
            Buffers.Create();
            Xfer.Create();
            return true;
        }
        #endregion

        #region Seetting
        public override bool? Exposure( int value )
        {
            //mbSession.Query( "set " + value.ToString() + "\r" );
            return true;
        }
        public override bool? Grab()
        {
            if ( !Xfer.Grabbing ) Xfer.Grab();
            return null;
        }
        public override bool? Freeze()
        {
            if ( Xfer.Grabbing ) Xfer.Freeze();
            return null;
        }
        public override bool? LineRate( int value )
        {
            //mbSession.Query( "ssf " + value.ToString() + "\r" );
            return null;
        }
        public override bool? ExposureMode( int value )
        {
            //mbSession.Query( "sem " + value.ToString() + "\r" );
            return null;
        }
        #endregion

        #region Function
        public override bool? BuffGetAll()
        {
            try
            {
                BuffData = new byte [ Buffers.Width * Buffers.Height ];
                GCHandle outputAddr = GCHandle.Alloc( BuffData, GCHandleType.Pinned);
                IntPtr pointer = outputAddr.AddrOfPinnedObject(); 
                Buffers.ReadRect( 0 , 0 , Buffers.Width , Buffers.Height , pointer );
                Marshal.Copy( pointer , BuffData , 0 , BuffData.Length );
                outputAddr.Free();
                return true;
            }
            catch ( Exception ex )
            {
                BuffData = null;
                return null;
            }
        }

        public override bool? BuffGetLine()
        {
            try
            {
                BuffData = new byte [ Buffers.Width ];
                GCHandle outputAddr = GCHandle.Alloc(BuffData, GCHandleType.Pinned);
                IntPtr pointer = outputAddr.AddrOfPinnedObject();
                int readnum = 0;
                Buffers.ReadLine( 0 , 0 , Buffers.Width - 1 , 0 , pointer , out readnum );
                Marshal.Copy( pointer , BuffData , 0 , BuffData.Length );
                outputAddr.Free();
                return true;

            }
            catch ( Exception ex )
            {
                BuffData = null;
                return null;
            }
        }
        #endregion





    }
}
