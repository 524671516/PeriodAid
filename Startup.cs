using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PeriodAid.Startup))]
namespace PeriodAid
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
