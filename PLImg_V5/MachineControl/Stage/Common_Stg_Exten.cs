using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl.Stage
{
    public static class Common_Stg_Exten
    {
        private static List<IStageExt> StageList;

        private static List<Dictionary<string , Action>> FlowList;

        public static void AddStage( IStageExt linkedStg)
        {
            StageList.Add(linkedStg);
        }

        public static void PopStage()
        {
            StageList.Remove( StageList.Last() );
        }

        public static  List<string> ShowStage( bool printout = false)
        {
            List<string> output = new List<string>();
            foreach ( var stg in StageList )
            {
                output.Add( stg.Name );
            }

            if ( printout )
            {
                int count = 0;
                foreach ( var item in output )
                {
                    Console.WriteLine( count.ToString() + " : " , item );
                    count++;
                }
            }
            return output;
        }

        public static bool Connect<T>(this T connectinfo , int idx = 0)
        {
            return StageList [ idx ].Connect( connectinfo );
        }

    }
}
