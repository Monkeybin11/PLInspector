using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;

namespace MachineControl
{
    public static class Stage_Shape_Ext
    {
        public static Stage_Shape Connect<T>(
           this Stage_Shape src
           , string path
           , ConnectMode mode
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null)
           where T : struct
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                         .Connect( path , mode ) );
        }

        public static Stage_Shape Enable<T>(
           this Stage_Shape src 
           , string axis
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
           where T : struct
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .Enable( axis ) );
        }

        public static Stage_Shape Disable<T>(
           this Stage_Shape src 
           , string axis
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
           where T : struct
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .Disable( axis ) );
        }


        public static Stage_Shape Origin<T>(
          this Stage_Shape src
           , string axis
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
          where T : struct
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                         .Home( axis ) );
        }

        public static Stage_Shape MoveAbsE(
           this Stage_Shape src
           , string axis
           , double pos
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .Moveabs( axis , pos) );
        }

        public static Stage_Shape MoveRelE(
           this Stage_Shape src
           , string axis
           , double pos
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .Moverel( axis , pos ) );
        }

        public static Stage_Shape WaitEpsE(
            this Stage_Shape src
           , string axis
           , double pos
           , double epsilon
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .WaitbyEps( axis ,pos ,epsilon ) );
        }

        public static Stage_Shape WaitStatusE(
            this Stage_Shape src
           , string axis
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .WaitbyStatus( axis ) );
        }


        public static Stage_Shape SetSpeedE(
           this Stage_Shape src
           , string axis
           , double speed
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                        .SetSpeed( axis , speed ) );
        }


        public static double? GetPosE(
           this Stage_Shape src
           , string axis
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChknullClass( callerpath , line , name )
                      .GetPos( axis );
        }

        public static Stage_Shape StartTriggerE(
           this Stage_Shape src
           , int buffnum
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                         .StartTrigger( buffnum ));
        }

        public static Stage_Shape StopTriggerE(
           this Stage_Shape src
           , int buffnum
           , [CallerFilePath] string callerpath = null
           , [CallerLineNumber] int line = 0
           , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult( src.ChknullClass( callerpath , line , name )
                                       .StopTrigger( buffnum ) );
        }
    }

    public static class Decorator_Stg
    {
        // Decorator for Stage_Shape_Ext
        public static T ChknullClass<T>(
            this T src
            , string basepath
            , int baseline
            , string basenmae
            , [CallerMemberName] string name = null )
            where T : class
        {
            Thread.Sleep( 10 );

            if ( src == null )
            {
                "-----------  Error  ----------".Print();
                basepath.Print( "BasePath" );
                baseline.Print( "Code Line" );
                basenmae.Print( "BaseCaller" );
                name.Print( "Caller" );
                "------------------------------".Print();
                return null;
            }
            else return src;
        }

        public static T ChkNullResult<T>(
            this T src
            , bool? result )
            where T : class
        {
            if ( result != null ) return src;
            else return null;
        }
    }

}
