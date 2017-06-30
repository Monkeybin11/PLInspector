using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;
using System.Runtime.InteropServices;
using SPIIPLUSCOM660Lib;
using System.Threading;

namespace MachineControl
{
    public class Stg_ACSController : Stage_Shape
    {
        SPIIPLUSCOM660Lib.AsyncChannel Ch;
        object pWait = 0;

        #region Global
        public override bool? Connect( string path , ConnectMode mode )
        {
            switch ( mode )
            {
                case ConnectMode.IP:
                    Ch.CloseComm();
                    Ch.OpenCommEthernetTCP( path , Ch.ACSC_SOCKET_STREAM_PORT );
                    return true;

                case ConnectMode.Com:
                    Ch.CloseComm();
                    Ch.OpenCommSerial( Convert.ToInt32( path ) , -1 );
                    return null;
            }
            return null;
        }

        public override bool? Disconnect()
        {
            Ch.CloseComm();
            return true;
        }

        public override bool? Disable( string axis )
        {
            Ch.Disable( Axis [ axis ] , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return null;
        }
       
        public override bool? Enable( string axis )
        {
            Ch.Enable( Axis [ axis ] , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override double? GetPos( string axis )
        {
            return Ch.GetFPosition( Axis.Values.ElementAt( Axis [ axis ] ) , Ch.ACSC_SYNCHRONOUS , ref pWait );
        }

        public override double? GetPos_Debug( string axis )
        {
            return Ch.GetFPosition( Axis.Values.ElementAt( Axis [ axis ] ) , Ch.ACSC_SYNCHRONOUS , ref pWait ).Print( $" {Axis [ axis ].ToString()}  Position " );
        }

        public override bool? Home( string axis )
        {
            Ch.RunBuffer( Axis [ axis ] , "" , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override bool? Moveabs( string axis , double pos )
        {
            Ch.ToPoint( 0 , Axis [ axis ] , pos , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override bool? Moverel( string axis , double pos )
        {
            Ch.ToPoint( Ch.ACSC_AMF_RELATIVE , Axis [ axis ] , pos , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override bool? SetSpeed( string axis , double speed )
        {
            Ch.SetVelocity( Axis [ axis ] , speed , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override bool? StartTrigger( int buffnum )
        {
            Ch.RunBuffer( buffnum , "" , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return true;
        }
        public override bool? StopTrigger( int buffnum )
        {
            Ch.StopBuffer( buffnum , Ch.ACSC_ASYNCHRONOUS , ref pWait );
            return null;
        }
        public override bool? WaitbyEps( string axis , double pos , double epsilon )
        {
            while ( true )
            {
                Thread.Sleep( 8 );
                double error = Math.Abs( pos - Ch.GetFPosition( Axis[axis], Ch.ACSC_SYNCHRONOUS, ref pWait ) );
                if ( error < epsilon ) break;
            }
            return true;
        }

        public override bool? WaitbyStatus( string axis )
        {
            while ( true )
            {
                Thread.Sleep( 30 );
                var result = Ch.GetMotorState( Axis [ axis ] , Ch.ACSC_SYNCHRONOUS , ref pWait );
                if ( result == 0x00000001 ) break;
            }
            return true;
        }
        #endregion


 
    }
}
