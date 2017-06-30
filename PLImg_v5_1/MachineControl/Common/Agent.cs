using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace MachineControl.Common
{
    [Synchronization]
    public class Agent : ContextBoundObject
    {
        public Dictionary<string,Flow> FlowList;

        public void AddFlow( string name , Flow flow )
        {
            if ( FlowList == null )
            {
                FlowList = new Dictionary<string , Flow>();
            }
            FlowList.Add( name , flow );
        }

        public void AddFlow(Flow flow )
        {
            if ( FlowList == null )
            {
                FlowList = new Dictionary<string , Flow>();
            }
            FlowList.Add( flow.Name , flow );
        }


        public void PopFlow()
        {
            if ( FlowList != null )
            {
                FlowList.Remove( FlowList.Last().Key );
            }
        }

        public void RemoveFlow(string name)
        {
            if ( FlowList != null )
            {
                FlowList.Remove( name );
            }
        }

        public void ClearFlow()
        {
            FlowList = new Dictionary<string , Flow>();
        }

        public List<string> ShowFlowList(bool print = true)
        {
            var keylist = FlowList.Select( x  =>
                                            {
                                                if(print) Console.WriteLine(x.Key);
                                                return x.Key;
                                            })
                                  .ToList();
            return keylist;
        }

    }
}
