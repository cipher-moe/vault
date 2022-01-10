using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            dotenv.net.DotEnv.Load();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    var port = Environment.GetEnvironmentVariable("PORT");
                    if (port != null)
                    {
                        var serverPort = int.Parse(port);
                        webBuilder.UseUrls($"http://*:{serverPort}");
                    }
                });
    }
}
