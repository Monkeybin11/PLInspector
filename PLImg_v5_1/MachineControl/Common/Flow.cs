using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MachineControl.Common
{
    public class Flow
    {
        public string Name;
        //public string InputType { get {
        //
        //        Type delegateType = typeof()
        //
        //        MethodInfo invoke = NodeList[0].GetMethodInfo();
        //    } }
        public List<Delegate> NodeList; 

        public Flow()
        {
            NodeList = new List<Delegate>();
        }

        public void AddNode( Delegate input )
        {
            NodeList.Add( input );
        }

        public dynamic Run( List<Delegate> list , object input , int idx = 0 )
        {
            if ( idx == list.Count ) return input;
            var output =  list [ idx ].DynamicInvoke( input );
            return Run( list , output , idx + 1 );
        }
    }
}
