using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PLImg_V2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void App_Startup( object sender , StartupEventArgs e )
        {
            bool startMinimized = false;
            for ( int i = 0 ; i < e.Args.Length ; i++ )
            {
                if ( e.Args [ i ] == "/STartMinimized" )
                {
                    startMinimized = true;
                }
            }

            PlImaging_Main plmain = new PlImaging_Main();
            if ( startMinimized )
            {
                plmain.WindowState = WindowState.Minimized;
            }
            plmain.Show();
        }
    }
}
