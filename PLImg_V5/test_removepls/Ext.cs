using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace test_removepls
{
    public static class Decorator
    {
        // Decorator for Stage_Shape_Ext
        public static void ck1(
            this bool? src
            , string basepath
            , int baseline
            , string basenmae
            , [CallerMemberName] string name = null)
        {
            Thread.Sleep( 10 );

            if ( src == null )
            {

                Console.WriteLine( $" FilePath : {basepath} " );
                Console.WriteLine( $" Line : {baseline} " );
                Console.WriteLine( $" Caller : {basenmae} " );
                Console.WriteLine( $" From : {name} " );
                Console.WriteLine( name );


                StackTrace stackTrace = new StackTrace();
                StackFrame[] framelist = stackTrace.GetFrames();
                for ( int i = 0 ; i < framelist.GetLength(0) ; i++ )
                {
                    StackFrame ff = stackTrace.GetFrames()[i];
                    //var filename = ff.attribute();
                    var line = ff.GetMethod();
                    var test = ff.GetFileColumnNumber();
                    var test2 = ff.GetFileLineNumber();
                }




                StackFrame fram = stackTrace.GetFrames()[1];
                MethodBase method = fram.GetMethod();
                string methodName = method.Name;

                if ( stackTrace.GetFrames() [ 2 ] != null )
                {
                    Console.WriteLine( "Still parent " );
                }


                Console.WriteLine( methodName );
            }
            else
            {
            }
        }
    }


    public static class Ext
    {
        public static int DogMethod(
            this bool? src
            , [CallerFilePath] string path = null
            , [CallerLineNumber] int line = 0
            , [CallerMemberName] string name = null
            )
        {
            src.ck1(path , line , name);
            return 3;
        }
    }
}
