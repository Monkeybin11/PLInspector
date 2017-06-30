using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl.Common
{
    public class SimpleLogger : ILogger
    {
        public string SavePath { get; set; }
        public SimpleLogger(string savepath)
        {
            SavePath = savepath;
        }

        public Nullable<bool> MethodLog( string name )
        {
            try
            {
                using ( System.IO.StreamWriter file =
                                new System.IO.StreamWriter( SavePath + "SimpleLog.txt" ) )
                {
                    file.WriteLine( "Problem in method : " + name );
                }
                return true;
            }
            catch ( Exception ex)
            {
                Console.WriteLine( "Logger Problem : " + ex.ToString() );
                return null;
            }
        }
    }
}
