using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MachineControl
{
    public static class Camera_Shape_Ext
    {
        public static Camera_Shape ConnectE(
         this Camera_Shape src
         , string path
         , [CallerFilePath] string callerpath = null
         , [CallerLineNumber] int line = 0
         , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult_Cam( src.ChknullClass_Cam( callerpath , line , name )
                                         .Connect( path ) );
        }

        public static Camera_Shape DisconnectE(
        this Camera_Shape src
        , [CallerFilePath] string callerpath = null
        , [CallerLineNumber] int line = 0
        , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult_Cam( src.ChknullClass_Cam( callerpath , line , name )
                                         .Disconnect() );
        }

        public static Camera_Shape EvtResistE(
        this Camera_Shape src
        , [CallerFilePath] string callerpath = null
        , [CallerLineNumber] int line = 0
        , [CallerMemberName] string name = null )
        {
            return src.ChkNullResult_Cam( src.ChknullClass_Cam( callerpath , line , name )
                                         .EvtResist() );
        }

        public static byte[] BuffGetAllE(
        this Camera_Shape src
        , [CallerFilePath] string callerpath = null
        , [CallerLineNumber] int line = 0
        , [CallerMemberName] string name = null )
        {
            src.ChkNullResult_Cam( src.ChknullClass_Cam( callerpath , line , name )
                                         .BuffGetAll() );
            return src.BuffData;
        }

    }

    public static class Decorator_Cam
    {
        // Decorator for Stage_Shape_Ext
        public static T ChknullClass_Cam<T>(
            this T src
            , string basepath
            , int baseline
            , string basenmae
            , [CallerMemberName] string name = null )
            where T : class
        {
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

        public static T ChkNullResult_Cam<T>(
            this T src
            , bool? result )
            where T : class
        {
            if ( result != null ) return src;
            else return null;
        }
    }


}
