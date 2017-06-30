using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace PLImaging
{
    public static class Epi_Ext
    {
        private static DefectRawData PointArr2DefectInfo(
            this System.Drawing.Point [ ] src )
        {
            var ptFarr = src.Select( ptf => new System.Drawing.PointF( ptf.X , ptf.Y )).ToArray();
            var pt = src.PointArrMean();
            var  size = CvInvoke.MinAreaRect( ptFarr ).Size;
            var radius = Math.Sqrt( size.Width *size.Height );
            return new DefectRawData(
                pt.Y
                , pt.X
                , radius
                , ( double )( size.Height * size.Width ) );
        }

        private static System.Drawing.Point PointArrMean(
           this System.Drawing.Point [ ] src )
        {
            var ymean = src.Select( x => x.Y ).Sum() / src.GetLength( 0 );
            var xmean = src.Select( x => x.X ).Sum() / src.GetLength( 0 );
            return new System.Drawing.Point( xmean , ymean ); // For Get Center Point
        }

        public static DefectRawData ToDefectRawInfo(
            this System.Drawing.Point[ ] src)
        {
            var ptFarr = src.Select( ptf => new System.Drawing.PointF( ptf.X , ptf.Y )).ToArray();
            var pt = src.PointArrMean();
            var  size = CvInvoke.MinAreaRect( ptFarr ).Size;
            var radius = Math.Sqrt( size.Width *size.Height );
            return new DefectRawData(
                pt.Y
                , pt.X
                , radius
                , ( double )( size.Height * size.Width ) );
        }

        public static Task<List<DefectRawData>> ToDefectRawData(
            this System.Drawing.Point[][] src)
        {
            return Task.Run<List<DefectRawData>>( () =>
             {
                 return src.Select( x =>x.ToDefectRawInfo() )
                            .ToList();

             } );
        }
      
        public static List<DefectRawData> ShiftDefect(
            this List<DefectRawData> src
            , int count
            , int shiftLen
            , int resol )
        {
            return src.Select( s => new DefectRawData( s.CenterY 
                                                       , s.CenterX + count* shiftLen
                                                       , s.Size
                                                       , resol ) )
                       .ToList();
        }






    }
}
