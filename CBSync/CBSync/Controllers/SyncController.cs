using CBSync.SendModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Windows.Forms;

namespace CBSync.Controllers
{
    public class SyncController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage GetPing()
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> RequestSyncTo(HttpRequestMessage msg)
        {
            SyncRequestData syncRequest = JsonConvert.DeserializeObject<SyncRequestData>(await msg.Content.ReadAsStringAsync());
            IPHostEntry host = Dns.GetHostEntry(syncRequest.Sender);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyIcon ni = new NotifyIcon();
                ni.BalloonTipClicked += (sender, e) =>
                {
                    HttpWebResponse response = new HttpSender($"http://{host.AddressList.Last().ToString()}:9000/api/Sync/SyncRequestAccepted", 2000)
                        .Send(new SyncRequestData() { Sender = Dns.GetHostName() }).Receive();
                    ni.Dispose();
                };
                ni.BalloonTipClosed += (sender, e) =>
                {
                    HttpWebResponse response = new HttpSender($"http://{host.AddressList.Last().ToString()}:9000/api/Sync/SyncRequestDenied", 2000)
                        .Send(new SyncRequestData() { Sender = Dns.GetHostName() }).Receive();
                    ni.Dispose();
                };
                ni.Icon = SystemIcons.Application;
                ni.Visible = true;
                ni.ShowBalloonTip(5000, "Sync-To Request", $"{host.HostName} has requested to sync his Clipboard to you.", System.Windows.Forms.ToolTipIcon.Info);
            });

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
        // TODO: Establish Connection / Store Sync state 
        [HttpPost]
        public async Task<HttpResponseMessage> RequestSyncFrom(HttpRequestMessage msg)
        {
            SyncRequestData syncRequest = JsonConvert.DeserializeObject<SyncRequestData>(await msg.Content.ReadAsStringAsync());
            IPHostEntry host = Dns.GetHostEntry(syncRequest.Sender);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyIcon ni = new NotifyIcon();
                ni.BalloonTipClicked += (sender, e) =>
                {
                    HttpWebResponse response = new HttpSender($"http://{host.AddressList.Last().ToString()}:9000/api/Sync/SyncRequestAccepted", 2000)
                        .Send(new SyncRequestData() { Sender = Dns.GetHostName() }).Receive();
                    ni.Dispose();
                };
                ni.BalloonTipClosed += (sender, e) =>
                {
                    HttpWebResponse response = new HttpSender($"http://{host.AddressList.Last().ToString()}:9000/api/Sync/SyncRequestDenied", 2000)
                        .Send(new SyncRequestData() { Sender = Dns.GetHostName() }).Receive();
                    ni.Dispose();
                };
                ni.Icon = SystemIcons.Application;
                ni.Visible = true;
                ni.ShowBalloonTip(5000, "Sync-From Request", $"{host.HostName} has requested to sync his Clipboard from you.", System.Windows.Forms.ToolTipIcon.Info);
            });

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> SyncRequestDenied(HttpRequestMessage msg)
        {
            SyncRequestData data = JsonConvert.DeserializeObject<SyncRequestData>(await msg.Content.ReadAsStringAsync());
            IPHostEntry host = Dns.GetHostEntry(data.Sender);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyIcon ni = new NotifyIcon();
                ni.Icon = SystemIcons.Application;
                ni.Visible = true;
                ni.ShowBalloonTip(2000, "Sync Request Denied", $"{host.HostName} has denied your Sync request!", System.Windows.Forms.ToolTipIcon.Info);
            });
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> SyncRequestAccepted(HttpRequestMessage msg)
        {
            SyncRequestData data = JsonConvert.DeserializeObject<SyncRequestData>(await msg.Content.ReadAsStringAsync());
            IPHostEntry host = Dns.GetHostEntry(data.Sender);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyIcon ni = new NotifyIcon();
                ni.Icon = SystemIcons.Application;
                ni.Visible = true;
                ni.ShowBalloonTip(2000, "Sync Request Accepted", $"{host.HostName} has accepted your Sync request!", System.Windows.Forms.ToolTipIcon.Info);
            });
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}
