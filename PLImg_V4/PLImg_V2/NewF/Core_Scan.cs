using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public partial class Core
    {
        public Action<ScanMode> ScanInfoSet(
            int xstart , int ystart , int yend , double xStep ,
            int bufW , int bufH ,
            int buflimit , int unitlimit , int linelimit ,
            int sped )
        {
            return new Action<ScanMode>( ( mode ) =>
            {
                ScanType = ScanTypes.NonTrig;
                Info.SetBufInfo( bufW , bufH );
                Info.SetScanSpeed( sped );
                switch ( mode )
                {
                    case ScanMode.MultiLine:
                        Info.SetPos( xstart , ystart , yend , xStep );
                        Info.SetLimit( buflimit , unitlimit , linelimit );
                        break;

                    case ScanMode.SingleLine:
                        Info.SetPos( xstart , ystart , yend );
                        Info.SetLimit( buflimit , unitlimit );
                        break;
                }
            } );
        }

        public Action<ScanMode> ScanInfoSet(
            int ystart , int bufH = -1　,int ystep = -1 , int linenum = -1 ,
            int sped = -1 )
        {
            return new Action<ScanMode>( ( mode ) =>{
                ScanType = ScanTypes.Trig;
                switch ( mode )
                {
                    case ScanMode.TrgCustom:
                        TrgInfo.SetTrgInfo( ystart , bufH , ystep , linenum , sped );
                        break;

                    case ScanMode.Trg2Inch:
                        TrgInfo.SetTrgInfo( WaferSize.Size2 );
                        break;

                    case ScanMode.Trg4Inch:
                        TrgInfo.SetTrgInfo( WaferSize.Size4 );
                        break;
                }
            } );
        }
        public bool ReadyPos(ScanTypes type){
            try
            {
                var xSpeed =  Stg.SetSpeed( "X" );
                var ySpeed =  Stg.SetSpeed( "Y" );
                xSpeed( 200 );
                ySpeed( 200 );
                switch ( type ) {
                    case ScanTypes.NonTrig:
                        Stg.Moveabs( "X" )( Info.PsXStart );
                        Stg.Moveabs( "Y" )( Info.PsYStart );
                        Stg.WaitEps( "X" )( Info.PsXStart , 0.005 );
                        Stg.WaitEps( "Y" )( Info.PsYStart , 0.005 );
                        xSpeed( Info.ScanSpeed );
                        return true;

                    case ScanTypes.Trig:
                        Stg.Moveabs( "X" )( TrgInfo.PsXStart );
                        Stg.Moveabs( "Y" )( TrgInfo.PsYStart );
                        Stg.WaitEps( "X" )( TrgInfo.PsXStart , 0.005 );
                        Stg.WaitEps( "Y" )( TrgInfo.PsYStart , 0.005 );
                        xSpeed( TrgInfo.ScanSpeed );
                        return true;
                    default:
                        return false;
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        public bool ScanStart(  )
        {
            try
            {
                InitCount();
                ImgSrcByte = new byte[0];
                ScanStatus = ScanState.Start;
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }

        // 여기의 함수들을 스캔 타입에따라 정해준다. 논트리거 방식이냐 트리거 방식이냐 
        /*Scan process func*/
        bool StopProcess()
        {
            try
            {
                ScanStatus = ScanState.Wait;
                NeedClearBuf = true;
                Freeze();
                System.Threading.Thread.Sleep( 100 );
                Grab();
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        bool PauseProcess( int linecount )
        {
            try
            {
                var xTargetPos = Info.PsXStart - Info.XStep * linecount;
                Stg.WaitEps( "Y" )( Info.PsYEnd , 0.01 );
                MoveXYstg( "Y" , Info.PsYStart );
                MoveXYstg( "X" , xTargetPos );
                Stg.WaitEps( "Y" )( Info.PsYStart , 0.01 );
                Stg.WaitEps( "X" )( xTargetPos , 0.01 );
                Stg.SetSpeed( "Y" )( Info.ScanSpeed );
                Stg.Moveabs( "Y" )( Info.PsYEnd );
                ScanStatus = ScanState.Start;
                NeedClearBuf = true;
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }
        bool StartProcess( ScanTypes type )
        {
            try
            {
                Stg.Moveabs( "Y" )( Info.PsYEnd );
                if ( NeedClearBuf )
                {
                    Freeze();
                    System.Threading.Thread.Sleep( 30 );
                    Cam.BuffClear()();
                    NeedClearBuf = false;
                    Grab();
                }

                /*Create Func*/
                var Buf2Img = FnBuff2Img( Cam.GetBuffWH()["H"] , Cam.GetBuffWH()["W"] );
                var currentbuff = FullBuffdata();

                evtRealimg( Buf2Img( currentbuff , 1 ) );
                ImgSrcByte = Matrix.Concatenate<byte>( ImgSrcByte , currentbuff );
                //SaveFullDat(ImgSrcByte,LineCount,UnitCount,BuffCount);
                evtMapImg( Buf2Img( ImgSrcByte , BuffCount + 1 ) , LineCount , UnitCount );
                Update( BuffCount , UnitCount , LineCount );
                return true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                return false;
            }
        }

        /*minor func*/
        void InitCount()
        {
            BuffCount = 0;
            UnitCount = 0;
            LineCount = 0;
        }
        void Update( int BuffCount , int UnitCount , int LineCount )
        {
            if ( BuffCount != Info.BuffLimit ) { BuffCount += 1; }
            else
            {
                ImgSrcByte = null;
                ImgSrcByte = new byte[0];
                BuffCount = 0;
                if ( UnitCount == Info.UnitLimit )
                {
                    UnitCount = 0;
                    if ( LineCount == Info.LineLimit )
                    {
                        ScanStatus = ScanState.Stop;
                    }
                    else
                    {
                        LineCount += 1;
                        ScanStatus = ScanState.Pause;
                    }
                }
                else { UnitCount += 1; }
            }
        }

        void ScanStart_non() { }

        void ScanStart_trg() { }


    }
}
