using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImg_V2
{
    public class ScanTempData
    {
        private object key = new object();

        public int BufferCount = 0;
        public int BufferLimit = 0;
        public byte[][,,] SeperatedImg;
        public byte[][,,] SeperatedColorImg;
        public byte[][,,] SeperatedBoxedImg;
        public byte[][,,] SeperatedResizedImg;
        public byte[,,]   ResizedImg;
        public byte[,,]   ResizedBoxedImg;
        public ScanConfig CurrentType;

        // DefectInfo

        public ScanTempData(int bufflimit , ScanConfig type) 
        {
            CurrentType = type;
            BufferLimit = bufflimit;
            SeperatedImg = new byte [ bufflimit ] [ , , ];
            SeperatedBoxedImg = new byte [ bufflimit ] [ , , ];
            SeperatedResizedImg = new byte [ bufflimit ] [ , , ];
        }

        public void BufferCountUpdate()
        {
            lock ( key )
            {
                this.BufferCount++;
            }
        }


    }
}
