using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PMDWebApplication.Startup))]
namespace PMDWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //To use SignalR
            app.MapSignalR();
        }
    }
}
