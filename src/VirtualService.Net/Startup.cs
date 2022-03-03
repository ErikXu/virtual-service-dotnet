using KubeOps.Operator;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VirtualService.Net
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKubernetesOperator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseKubernetesOperator();
        }
    }
}
