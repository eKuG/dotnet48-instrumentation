using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Net48AutoDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Net48AutoDemo starting...");

            using (var http = new HttpClient())
            {
                for (int i = 0; i < 5; i++)
                {
                    logger.LogInformation("Iteration {iter}: calling https://example.com/ ...", i);
                    var resp = await http.GetAsync("https://example.com/");
                    logger.LogInformation("Iteration {iter}: status {statusCode}", i, (int)resp.StatusCode);
                    await Task.Delay(1000);
                }
            }

            logger.LogWarning("Net48AutoDemo finished. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
