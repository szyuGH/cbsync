using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System.Web.Http;
using System.Windows;

namespace CBSync
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string baseWebAPIAddress = "http://localhost:9000/";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            WebApp.Start<OWINWebAPIConfig>(url: baseWebAPIAddress);
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
