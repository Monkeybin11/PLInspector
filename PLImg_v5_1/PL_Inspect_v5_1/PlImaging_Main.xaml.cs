using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro;
using System.Drawing;
using CommonExtension;
using static CommonExtension.PatternMatch.PatternMatch;

namespace PLImaging
{
    /// <summary>
    /// Interaction logic for PlImaging_Main.xaml
    /// </summary>
    public partial class PlImaging_Main : MetroWindow
    {
        NewCore Core = new NewCore();

        public PlImaging_Main()
        {
            InitializeComponent();
            MenuScanMethodRegist();

            Core.evtOriginResized += new TrsResized( x =>
                 this.Dispatcher.BeginInvoke( ( Action )( () => imgLeft.ImageSource = x ) ) );

            Core.evtBoxResized += new TrsResized( x =>
                 this.Dispatcher.BeginInvoke( ( Action )( () => imgRight.ImageSource = x ) ) );
           
        }

        public void MenuClick( object sender , RoutedEventArgs e )
        {
            
            MenuItem mi = sender as MenuItem;
            var procType = mi.Name.Match()
                                  .With( x => x == "mi1inch" , Tuple.Create(ScanConfig.Trigger_1 , 1 ,  3))
                                  .With( x => x == "mi2inch" , Tuple.Create(ScanConfig.Trigger_2 , 1 ,  3) )
                                  .Do();
            Core.StartScan( procType );
        }


        void MenuScanMethodRegist()
        {
            Func<Tuple<ScanConfig,int,int>,Task> scanStart = async config => Core.StartScan(config);
        }

        void Display()
        { }




        public void DisplayImage()
        {
            var img  = new Bitmap("");
            //imgLeft.Source = img.
        }

    }
}
