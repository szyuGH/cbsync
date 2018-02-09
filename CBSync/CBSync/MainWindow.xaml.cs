using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
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
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

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

        Notifier not;

        public MainWindow()
        {
            InitializeComponent();
            not = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));
                cfg.Dispatcher = Application.Current.Dispatcher;
            });

            

            //Task.Run(() => LoadData());
            IntPtr handle = new WindowInteropHelper(this).EnsureHandle();
            clipboardMonitor = new ClipboardMonitor(handle);
            clipboardMonitor.ClipboardChanged += OnClipboardChange;
        }
        

        protected override void OnClosing(CancelEventArgs e)
        {
            clipboardMonitor.Dispose();
            not.Dispose();
            base.OnClosing(e);
        }

        private async void LoadData()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.OperationalStatus == OperationalStatus.Up))
            {
                UnicastIPAddressInformation ownIp = ni.GetIPProperties().UnicastAddresses
                    .Where(i => i.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .FirstOrDefault();

                network = new IPNetwork(ownIp.Address, ownIp.IPv4Mask);

                Application.Current.Dispatcher.Invoke(() => pb_LoadStatus.Maximum = network.HostCount);
                foreach (IPAddress ip in network.IterateUsableIPs())
                {
                    await Task.Factory.StartNew(() => LoadTask(ip));
                }
            }
        }

        private async void LoadTask(IPAddress ip)
        {
            Ping ping = new Ping();
            PingReply res = await ping.SendPingAsync(ip, 100);
            if (res.Status == IPStatus.Success)
            {
                string hostname = "";
                try
                {
                    IPHostEntry entry = await Dns.GetHostEntryAsync(ip);
                    hostname = entry.HostName;
                }
                catch (Exception e)
                {
                    hostname = "";
                }
                Application.Current.Dispatcher.Invoke(() => MyList.Add(new NetworkHost()
                {
                    IP = ip.ToString(),
                    HostName = hostname,
                    SyncState = "X",
                }));
                
            }
            Application.Current.Dispatcher.Invoke(() => pb_LoadStatus.Value++);


            Application.Current.Dispatcher.Invoke(() => {
                if (pb_LoadStatus.Value >= pb_LoadStatus.Maximum)
                {
                    Application.Current.Dispatcher.Invoke(() => pb_LoadStatus.Value = 0);
                }
            });
            
        }

        private void SyncToButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHost host = EvaluateButtonClickNetworkHost(sender);
            // TODO: Sync To
        }

        private void SyncFromButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHost host = EvaluateButtonClickNetworkHost(sender);
            
            // TODO: Sync From
        }

        private NetworkHost EvaluateButtonClickNetworkHost(object sender)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    return (NetworkHost)row.DataContext;
                }
            }
            return null;
        }

        private void btn_Sort_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList stringElements = doc.GetElementsByTagName("text");
            for (int i = 0; i < stringElements.Count; i++)
            {
                stringElements[i].AppendChild(doc.CreateTextNode("Line " + i));
            }

            ToastNotification toast = new ToastNotification(doc);
            toast.Activated += ToastActivated;
            ToastNotificationManager.CreateToastNotifier("BLUBB").Show(toast);
            //MyList.Sort(i => i.IP, new RowComp());
        }

        private void ToastActivated(ToastNotification sender, object e)
        {
            Dispatcher.Invoke(() =>
            {
                Activate();
            });
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

        public class NetworkHost
        {
            public string IP { get; set; }
            public string HostName { get; set; }
            public string SyncState { get; set; }
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
