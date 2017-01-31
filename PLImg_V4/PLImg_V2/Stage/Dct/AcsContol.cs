﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPIIPLUSCOM660Lib;
using System.Runtime.InteropServices;

namespace PLImg_V2
{
    public class AcsContol
    {
        object pWait = 0;
        Dictionary<string,int> Axis;
        SPIIPLUSCOM660Lib.AsyncChannel Ch;

        public AcsContol( )
        {
            Ch = new SPIIPLUSCOM660Lib.AsyncChannel();
            Axis = new Dictionary<string, int>();
            Axis.Add( "X", 0 );
            Axis.Add( "Y", 1 );
            Axis.Add( "Z", 2 );
        }

        public void Connect(string addIP )
        {
            try
            {
                Ch.OpenCommEthernetTCP( addIP , Ch.ACSC_SOCKET_STREAM_PORT);
                EnableMotor( 0 );
                EnableMotor( 1 );
                EnableMotor( 2 );
            }
            catch ( COMException ex )
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void ConnectCom( string addIP ) {
            try
            {
                Ch.OpenCommSerial( Convert.ToInt32(addIP), -1 );
                EnableMotor(0);
                EnableMotor(1);
                EnableMotor(2);
            }
            catch ( COMException ex )
            {
                Console.WriteLine( ex.ToString() );
            }
        }

        public void EnableMotor( int axisindex )
        {
            Ch.Enable( Axis.Values.ElementAt( axisindex ), Ch.ACSC_ASYNCHRONOUS, ref pWait );
        }

        public void DisableMotor( int axisindex ) {
            Ch.Disable(Axis.Values.ElementAt(axisindex),Ch.ACSC_ASYNCHRONOUS, ref pWait);
        }

        public void DisZ( ) {
            Ch.Disable( 2, Ch.ACSC_ASYNCHRONOUS, ref pWait );
        }

        public void Home( )
        {
            Ch.RunBuffer(0, "", Ch.ACSC_ASYNCHRONOUS, ref pWait );
            Ch.RunBuffer(1, "", Ch.ACSC_ASYNCHRONOUS, ref pWait );
            Ch.RunBuffer(2, "", Ch.ACSC_ASYNCHRONOUS, ref pWait );
        }

        public void BuffClear( ) {
            Ch.ClearBuffer( Axis["X"] ,1,Ch.ACSC_MAX_LINE, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            System.Threading.Thread.Sleep( 100 );
            Ch.ClearBuffer( Axis["Y"] ,1,Ch.ACSC_MAX_LINE, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            System.Threading.Thread.Sleep( 100 );
        }

        /*Move*/
        public void XMove( double pos )
        {
            var xmove = FnMove(Axis["X"]);
            xmove( pos );
            System.Threading.Thread.Sleep( 100 );
        }

        public void YMove( double pos )
        {
            FnMove(Axis["Y"])( pos );
            System.Threading.Thread.Sleep( 100 );
        }

        public void ZMove( double pos )
        {
            FnMove(Axis["Z"])( pos );
            System.Threading.Thread.Sleep( 100 );
        }

        public void ZMoveRel( double pos ) {
            FnMoveRel(Axis["Z"])( pos);
            System.Threading.Thread.Sleep( 100 );
        }

        

        public void Wait2ArriveEpsilon( string axis, double targetPos, double epsilon ) {
            System.Threading.Thread.Sleep( 300 );
            while ( true )
            {
                double error = Math.Abs( targetPos - Ch.GetFPosition( Axis[axis], Ch.ACSC_SYNCHRONOUS, ref pWait ) );
                if ( error < epsilon ) break;
            }
        }


        /* Speed */
        public void SetSpeed( int xspeed, int yspeed, int zspeed ) {
            Ch.SetVelocity( Axis["X"], xspeed, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            System.Threading.Thread.Sleep( 100 );
            Ch.SetVelocity( Axis["Y"], yspeed, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            System.Threading.Thread.Sleep( 100 );
            Ch.SetVelocity( Axis["Z"], zspeed, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            System.Threading.Thread.Sleep( 100 );
        }

        public void SetXSpeed( int xspeed ) {
            FnSetSpeed( Axis["X"] )( xspeed );
            System.Threading.Thread.Sleep( 100 );
        }

        public void SetYSpeed( int yspeed ) {
            FnSetSpeed( Axis["Y"] )( yspeed );
            System.Threading.Thread.Sleep( 100 );
        }

        public void SetZSpeed( int zspeed ) {
            FnSetSpeed( Axis["Z"] )( zspeed );
            System.Threading.Thread.Sleep( 100 );
        }

        /* Report */

        public void Halt( ) {
            for ( int i = 0; i < 3; i++ )
            {
                Ch.Halt( Axis.Values.ElementAt( i ), Ch.ACSC_ASYNCHRONOUS, ref pWait );
            }
        }

        public double[] GetMotorFPos( )
        {
            try
            {
                double[] output = new double[3];

                for ( int i = 0; i < 3; i++ )
                {
                    output[i] = Ch.GetFPosition( Axis.Values.ElementAt( i ), Ch.ACSC_SYNCHRONOUS, ref pWait );
                }
                return output;

            }
            catch ( Exception )
            {
                double[] temp = new double[3] { 0,0,0 };
                return temp;
            }
        }


        
        public void Dispose( ) {
            Ch.CloseComm();
        }

        #region func
        Action<int> FnSetSpeed( int axis )
        {
            Action<int> setspeed = speed =>
            {
                Ch.SetVelocity( axis, speed, Ch.ACSC_ASYNCHRONOUS, ref pWait );
            };
            return setspeed;
        }

        Action<double> FnMove( int axis )
        {
            Action<double> move = point =>
            {
                Ch.ToPoint( 0,axis, point,Ch.ACSC_ASYNCHRONOUS,  ref pWait  );
            };
            return move;
        }

        Action<double> FnMoveRel( int axis ) {
            Action<double> move = point =>
            {
                Ch.ToPoint( Ch.ACSC_AMF_RELATIVE,axis, point,Ch.ACSC_ASYNCHRONOUS,  ref pWait  );
            };
            return move;
        }
        #endregion




    }
}
