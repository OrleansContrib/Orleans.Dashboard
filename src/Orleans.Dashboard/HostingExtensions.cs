using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace Orleans.Dashboard
{
    public static class HostingExtensions
    {
        public static IHostBuilder AddOrleansDashboard(this IHostBuilder builder)
        {
            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://*:8989").UseStartup<DashboardStartup>();
            });

            return builder;
        }
    }
}
