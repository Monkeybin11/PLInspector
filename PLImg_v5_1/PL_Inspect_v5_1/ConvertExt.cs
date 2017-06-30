using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonExtension.PatternMatch;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Windows;

namespace PLImaging
{
    public static class ConvertExt
    {
        public static int ToResizeW(
            this ScanConfig src)
        {
            return src.Match()
                      .With( x => x == ScanConfig.Trigger_1 , 12000 )
                      .With( x => x == ScanConfig.Trigger_2 , 12000 )
                      .With( x => x == ScanConfig.Trigger_4 , 6000 )
                      .Do();
        }

        public static int ToResizeH(
            this ScanConfig src)
        {
            return src.Match()
                     .With( x => x == ScanConfig.Trigger_1 , 24000 )
                     .With( x => x == ScanConfig.Trigger_2 , 36000 )
                     .With( x => x == ScanConfig.Trigger_4 , 36000 )
                     .Do();

        }

    }

    public static class BitmapSrcConvert
    {
        [DllImport( "gdi32" )]
        private static extern int DeleteObject( IntPtr o );

        public static BitmapSource ToBitmapSource( IImage image )
        {
            try
            {
                using ( System.Drawing.Bitmap source = image.Bitmap )
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject( ptr );
                    return bs;
                }
            }
            catch ( Exception )
            {
                return null;
            }

        }

    }

}
