using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CBSync
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<NetworkHost> MyList { get; set; } = new ObservableCollection<NetworkHost>();

        public MainWindow()
        {
            InitializeComponent();
            
            MyList.Add(new NetworkHost()
            {
                IP = "123.123.123.123",
                HostName = "Bum",
                SyncState = "X",
            });
            

        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHost host = null;
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    host = (NetworkHost)row.DataContext;
                    break;
                }
            }


            // TODO: Sync
        }

        public class NetworkHost
        {
            public string IP { get; set; }
            public string HostName { get; set; }
            public string SyncState { get; set; }
        }

        
    }
}
