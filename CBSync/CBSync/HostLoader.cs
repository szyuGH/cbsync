using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CBSync
{
    public delegate void HostLoaderEventHandler<T>(ICollection<T> list);
    public class HostLoader
    {
        private IPNetwork network;
        private ProgressBar progressBar;
        private HostLoaderEventHandler<NetworkHost> finishedHandler;

        public HostLoader(ProgressBar progressBar, ICollection<NetworkHost> list, HostLoaderEventHandler<NetworkHost> finishedHandler)
        {
            this.progressBar = progressBar;
            this.finishedHandler = finishedHandler;

            LoadData(list);
        }

        private async void LoadData(ICollection<NetworkHost> list)
        {
            Application.Current.Dispatcher.Invoke(() => progressBar.Maximum = 0);
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.OperationalStatus == OperationalStatus.Up))
            {
                UnicastIPAddressInformation ownIp = ni.GetIPProperties().UnicastAddresses
                    .Where(i => i.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .FirstOrDefault();
                if (ownIp != null)
                {
                    network = new IPNetwork(ownIp.Address, ownIp.IPv4Mask);

                    Application.Current.Dispatcher.Invoke(() => progressBar.Maximum += network.HostCount);
                    foreach (IPAddress ip in network.IterateUsableIPs())
                    {
                        await Task.Factory.StartNew(() => LoadTask(ip, list));
                    }
                }
            }
        }

        private async void LoadTask(IPAddress ip, ICollection<NetworkHost> list)
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
                catch (Exception)
                {
                    hostname = "";
                }

                //HttpWebRequest pingRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://{0}:9000/api/Sync/GetPing", ip.ToString()));
                //pingRequest.ContentType = "application/json";
                //pingRequest.Method = "POST";
                //pingRequest.Timeout = 200;
                //try
                //{
                //    using (var sw = new StreamWriter(pingRequest.GetRequestStream()))
                //    {
                //        string req = "";
                //        sw.Write(req);
                //        sw.Flush();
                //    }
                //    HttpWebResponse pingResponse = (HttpWebResponse)pingRequest.GetResponse();
                //    if (pingResponse.StatusCode == HttpStatusCode.OK)
                //    {
                        Application.Current.Dispatcher.Invoke(() => list.Add(new NetworkHost()
                        {
                            IP = ip.ToString(),
                            HostName = hostname,
                            SyncState = "-",
                        }));
                    //}
                //}
                //catch (Exception)
                //{
                //}
                
            }
            Application.Current.Dispatcher.Invoke(() => progressBar.Value++);


            Application.Current.Dispatcher.Invoke(() => {
                if (progressBar.Value >= progressBar.Maximum)
                {
                    Application.Current.Dispatcher.Invoke(() => progressBar.Value = 0);
                    finishedHandler?.Invoke(list);
                }
            });

        }
    }
}
