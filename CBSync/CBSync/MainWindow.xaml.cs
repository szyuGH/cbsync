using CBSync.SendModels;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace CBSync
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<NetworkHost> MyList { get; set; } = new ObservableCollection<NetworkHost>();

        private IPNetwork network;
        private ClipboardMonitor clipboardMonitor;
        

        public MainWindow()
        {
            InitializeComponent();

            //Task.Run(() => new HostLoader(pb_LoadStatus, MyList, OnHostsLoaded));
            IntPtr handle = new WindowInteropHelper(this).EnsureHandle();
            clipboardMonitor = new ClipboardMonitor(handle);
            clipboardMonitor.ClipboardChanged += OnClipboardChange;


            dg_Hosts.CellEditEnding += Dg_Hosts_CellEditEnding; ;
        }

        private void Dg_Hosts_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string value = (e.EditingElement as TextBox).Text;
            NetworkHost nwHost = e.Row.Item as NetworkHost;
            if (e.Column.DisplayIndex <= 1)
            {
                try
                {
                    IPHostEntry entry = Dns.GetHostEntry(value);
                    nwHost.HostName = entry.HostName;
                    nwHost.IP = entry.AddressList.Select(a => a.ToString()).Where(a => Regex.IsMatch(a, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).FirstOrDefault();
                    nwHost.SyncState = "-";
                } catch { }
            }
            Task.Delay(100).ContinueWith(t => { Application.Current.Dispatcher.Invoke(() => dg_Hosts.Items.Refresh()); });
        }

        private void OnHostsLoaded(ICollection<NetworkHost> hosts)
        {
            Console.WriteLine("Finished loading hosts");
            // TODO: check hosts, add new ones to mylist, remove deleted ones and maintain the other
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            clipboardMonitor.Dispose();
            base.OnClosing(e);
        }

        private void SyncToButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHost host = EvaluateButtonClickNetworkHost(sender);
            if (host == null)
                return;
            // TODO: Sync To

            HttpWebResponse syncToResponse = new HttpSender(string.Format("http://{0}:9000/api/Sync/RequestSyncTo", host.IP.ToString()), 10000)
                .Send(new SyncRequestData() { Sender = Dns.GetHostName() })
                .Receive();
            if (syncToResponse != null && syncToResponse.StatusCode == HttpStatusCode.OK)
            {
                // TODO: disable other buttons and start timout timer for request
            }
            
        }

        private void SyncFromButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHost host = EvaluateButtonClickNetworkHost(sender);
            if (host == null)
                return;

            HttpWebResponse syncToResponse = new HttpSender(string.Format("http://{0}:9000/api/Sync/RequestSyncFrom", host.IP.ToString()), 10000)
                .Send(new SyncRequestData() { Sender = Dns.GetHostName() })
                .Receive();
            if (syncToResponse != null && syncToResponse.StatusCode == HttpStatusCode.OK)
            {
                // TODO: disable other buttons and start timout timer for request
            }
        }

        private NetworkHost EvaluateButtonClickNetworkHost(object sender)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    if (row.DataContext is NetworkHost)
                        return (NetworkHost)row.DataContext;
                    else
                        return null;
                }
            }
            return null;
        }

        private void btn_Sort_Click(object sender, RoutedEventArgs e)
        {
            MyList.Sort(i => i.IP, new RowComp());
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            List<NetworkHost> refreshedHosts = new List<NetworkHost>();
            Task.Run(() => new HostLoader(pb_LoadStatus, refreshedHosts, OnHostsLoaded));
        }


        private void OnClipboardChange()
        {
            IDataObject dobj = Clipboard.GetDataObject();
            if (dobj != null)
            {
                List<object> objs = new List<object>();
                for (int i = 0; i < dobj.GetFormats().Length; i++)
                {
                    objs.Add(dobj.GetData(dobj.GetFormats()[i]));
                }
                Console.WriteLine("Types: {0}\nData:\n\t- {1}\n=================================\n",
                    string.Join(", ", dobj.GetFormats()), string.Join("\n\t- ", objs.Select(o => o.ToString() ?? "NULL")));
            }
            
            // TODO: sync when synchost available
        }

       

        public class RowComp : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x is string && y is string)
                {
                    IPAddress ip1 = IPAddress.Parse(x);
                    IPAddress ip2 = IPAddress.Parse(y);

                    byte[] ipb1 = ip1.GetAddressBytes();
                    byte[] ipb2 = ip2.GetAddressBytes();
                    return (Math.Pow(ipb1[0], 4) + Math.Pow(ipb1[1], 3) + Math.Pow(ipb1[2], 2) + Math.Pow(ipb1[3], 1)).CompareTo(
                        Math.Pow(ipb2[0], 4) + Math.Pow(ipb2[1], 3) + Math.Pow(ipb2[2], 2) + Math.Pow(ipb2[3], 1));
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
