using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace test_removepls
{
    class Program
    {

        static void Main( string [ ] args )
        {
            try
            {


                var task1 = new Task<int>(()=>
                    {
                         Thread.Sleep(2000);
                         throw new ArgumentException("task1 ex");
                    });

                var task2 = new Task<double>(()=>
                {
                    Thread.Sleep(3000);
                    throw new ArgumentException("task1 ex");
                } );

                var alltask = Task.WhenAny(task1,task2,Task.Delay(5000));
                Console.WriteLine( "done1" );
                var result1 = task1.Result;
                var result2 = task2.Result;
                Console.WriteLine( result1 );
                Console.WriteLine( result2 );
            }
            catch ( Exception ex)
            {
                Console.WriteLine( ex.ToString() );
            }


            //var task1 = Task.Run<int>(()=> 1);
            //var task2 = Task.Run<double>(()=> 2);
            //var ctask = Task.WhenAll(task1,task2)
            //    .ContinueWith( x =>
            //    {
            //        var result1 = task1.Result;
            //        var result2 = task2.Result;
            //        Console.WriteLine(result1);
            //        Console.WriteLine(result2);
            //    } , TaskContinuationOptions.ExecuteSynchronously);
            //
            //ctask.Wait();
            Console.WriteLine( "done" );
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
