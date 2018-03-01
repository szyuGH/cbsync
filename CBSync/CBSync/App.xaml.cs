using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Net;
using System.Web.Http;
using System.Windows;

namespace CBSync
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        //public static string baseWebAPIAddress = "http://" + Dns.GetHostAddresses(Dns.GetHostName())[1] + ":9000/";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:9000");
            foreach (var addr in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (addr.IsIPv6LinkLocal)
                    continue;
                options.Urls.Add(string.Format("http://{0}:9000", addr.ToString()));
            }
            options.Urls.Add(string.Format("http://{0}:9000", Environment.MachineName));
            WebApp.Start<OWINWebAPIConfig>(options);
        }
    }

    public class OWINWebAPIConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
