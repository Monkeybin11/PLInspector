using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImaging
{
    public class ScanTempData
    {
        public  enum TaskStatus { Noraml, Stopped }
        private object key = new object();

        public TaskStatus CurrentStatus = TaskStatus.Noraml;
        public int BufferCount = 0;
        public int BufferLimit = 0;
        public byte[][,,] SeperatedImg;
        public byte[][,,] SeperatedColorImg;
        public byte[][,,] SeperatedBoxedImg;
        public byte[][,,] SeperatedResizedImg;
        public byte[,,]   ResizedImg;
        public byte[,,]   ResizedBoxedImg;
        public List<DefectRawData>[] EpiDefectList;
        public List<DefectRawData> EpiFullDefect;
        public int[] OffsetData;
        public ScanConfig CurrentType;
        public int ResizeWidth;
        public int ResizeHeight;

        // DefectInfo

        public ScanTempData( ScanConfig type, int bufflimit ) 
        {
            CurrentType = type;
            BufferLimit = bufflimit;
            SeperatedImg = new byte [ bufflimit ] [ , , ];
            SeperatedBoxedImg = new byte [ bufflimit ] [ , , ];
            SeperatedResizedImg = new byte [ bufflimit ] [ , , ];
            EpiDefectList = new List<DefectRawData> [ bufflimit ];
            OffsetData = new int[ bufflimit ];
            OffsetData [ 0 ] = 0;

            ResizeWidth = BufferLimit * 12000 < 40000 ? 12000 : 40000 / BufferLimit;
            ResizeHeight = 40000;
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
