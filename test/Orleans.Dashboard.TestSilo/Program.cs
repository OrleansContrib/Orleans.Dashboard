using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;

namespace Orleans.Dashboard.TestSilo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true);
                    configApp.AddCommandLine(args);
                })
                .ConfigureLogging(configLogging =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
                .UseOrleans((context, builder) =>
                {
                    builder.UseLocalhostClustering()
                        .Configure<ClusterOptions>(opt =>
                        {
                            opt.ClusterId = opt.ServiceId = "OrleansDashboard-Cluster-Test";
                        })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Program).Assembly))
                        .AddDashboardAgent()
                        .AddMemoryGrainStorage("PubSubStore")
                        .AddStartupTask<TestStart>()
                        .UseInMemoryReminderService()
                        .AddMemoryGrainStorageAsDefault();
                })
                .AddOrleansDashboard()
                .UseConsoleLifetime()
                .Build();

            await host.StartAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
