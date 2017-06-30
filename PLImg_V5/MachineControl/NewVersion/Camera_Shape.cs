using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl
{
    public class Camera_Shape
    {
        public byte[]  BuffData;

        #region init
        public virtual bool? Connect( string path )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? Disconnect()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        protected virtual bool? LoadConfig( int value = 0 )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        protected virtual bool? CreateCamObj()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual Dictionary<string , int?> GetBuffWH( )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? EvtResist( )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        #endregion



        #region Seetting
        public virtual bool? Exposure( int value )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? Grab()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? Freeze()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? LineRate( int value )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool? ExposureMode( int value )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        #endregion



        #region Function
        public virtual bool? BuffGetAll()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        public virtual bool? BuffGetLine()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        public virtual bool? BuffClear()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        #endregion
    }
}
