using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl
{
    public interface IStageExt
    {
        string Name { get; set; }
        Dictionary<string , int> Axis { get; set; }
        bool Connect<T>( T connectionInfo );
        bool Disconnect();
        bool Enable( string axis );
        bool Disable( string axis );
        bool Origin( string axis );
        Action<double> Moveabs( string axis );
        Action<double> Moverel( string axis );
        Action<double , double> WaitEps( string axis );
        Action<double> SetSpeed( string axis );
        Func<double> GetPos( string axis );

    }
}
