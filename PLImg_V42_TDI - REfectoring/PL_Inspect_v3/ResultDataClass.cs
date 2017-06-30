using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    class ResultDataClass
    {
    }

    public class EpiResult
    {


    }
    public class DefectData
    {
        private double Resolution;
        public double CenterY , CenterX, Size;
        public double RealY { get { return CenterY * Resolution; } }
        public double RealX { get { return CenterX * Resolution; } }

        private double Radius { get { return Math.Sqrt( Size ); } }

        public double RealSize { get { return Size * Resolution * Resolution; } }

        public DefectData( double centery , double centerx , double defectSize , double resol )
        {
            CenterY = centery;
            CenterX = centerx;
            Size = defectSize;
            Resolution = resol;
        }
    }

}
