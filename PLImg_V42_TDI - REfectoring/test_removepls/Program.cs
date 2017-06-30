using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace test_removepls
{
    class Program
    {

        static void Main( string [ ] args )
        {
            Console.ReadLine();

        }

        public static Task<int> testtask(int input)
        {
            Task<int> output = new Task<int>(()=> {
                Task.Delay( 1000 );
                Console.WriteLine("input : " + input.ToString() );
                return input * 10;
            } );
            return output;
        }



        Func<string , bool> NodeConnection => x => true;
        Func<bool , bool> NodeOrigin => x => true; 
        Func<bool , bool> NodeMoveAbs => x => true; 
        Func<bool , bool> NodeWait2Arraive => x => true; 






        static object runme( int idx , List<Delegate> list , object input  )
        {
            Console.WriteLine( idx );
            if ( idx == list.Count )
            {
                return input;
            }
            var output =  list [ idx ].DynamicInvoke( input );
            return runme( idx + 1 , list , output );
        }

        static void AddNOde( List<Delegate> any , Delegate input)
        {
            any.Add( input );
        }


        //static dynamic runmethod( int idx , List<object> methodlist , string input)
        //{
        //    for ( int i = methodlist.Count ; i > 0  ; i-- )
        //    {
        //        if ( i == 0 )
        //        {
        //            var temp = methodlist [ i ];
        //            return methodlist [ i ]( input );
        //        }
        //        return runmethod( idx - 1 , methodlist , input );
        //    }
        //}


        

    }
}
