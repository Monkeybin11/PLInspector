using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl
{
    public enum ConnectMode { IP, Com }
    public class Stage_Shape
    {
        public Dictionary<string , int> Axis;
        public Stage_Shape()
        {
            Axis = new Dictionary<string , int>();
            Axis.Add( "Y" , 0 );
            Axis.Add( "X" , 1 );
            Axis.Add( "Z" , 2 );
        }

        bool?  Error { get; set; }

        // if Fail => return null
        public virtual bool?  Connect( string path , ConnectMode mode )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  Disable( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  Disconnect()
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  Enable( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual double? GetPos( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        public virtual double? GetPos_Debug( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        public virtual bool?  Home( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  Moveabs( string axis , double pos )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  Moverel( string axis , double pos )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  SetSpeed( string axis , double speed )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  StartTrigger( int buffnum )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  StopTrigger( int buffnum )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }
        public virtual bool?  WaitbyEps( string axis , double pos , double epsilon )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

        public virtual bool? WaitbyStatus( string axis )
        {
            Console.WriteLine( "Not Implemented" );
            return null;
        }

    }
}
