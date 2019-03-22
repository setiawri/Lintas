using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LintasMVC.Startup))]
namespace LintasMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
