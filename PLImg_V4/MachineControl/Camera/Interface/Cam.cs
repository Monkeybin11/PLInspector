using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl.Camera.Interface
{
    public interface Cam<Tbuff,Tout>
    {
        Dictionary<string , int> CamNum { get; set; }
        Action Connect( string camNum );
        Action Disconnect();
        Action Grab();
        Action Freeze();
        Action BuffClear();
        Func<Tout> BuffGetAll( Tbuff buff );
        Func<Tout> BuffGetLine( Tbuff buff );
        Action<double> Exposure();
        Action<double> LineRate();
    }
}
