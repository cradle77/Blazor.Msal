using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Blazor.Msal.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:5100;https://localhost:5101");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
