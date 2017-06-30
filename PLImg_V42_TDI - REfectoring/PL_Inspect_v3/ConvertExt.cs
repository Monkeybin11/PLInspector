using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonExtension.PatternMatch;

namespace PLImg_V2
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
}
