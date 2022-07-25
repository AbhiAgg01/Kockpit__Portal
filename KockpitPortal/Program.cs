using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KockpitPortal.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KockpitPortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.ConfigureServices((hostContext, services) =>
                //{
                //    //services.AddHostedService<Worker>();
                //    //services.AddHostedService<Vision>();
                //    services.AddHostedService<MyCronJob3>();
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
