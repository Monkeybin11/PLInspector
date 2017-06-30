using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl.Common
{
    public interface ILogger
    {
        string SavePath { get; set;  }

        Nullable<bool> MethodLog( string name );
    }
}
