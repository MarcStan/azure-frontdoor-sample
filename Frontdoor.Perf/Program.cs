using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontdoor.Perf
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length != 2 || !int.TryParse(args[1], out int testDuration))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("<url> <test duration in seconds>");
                return;
            }
            var url = args[0];
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Ensuring site is warmed up..");
                var endOfWarmup = DateTimeOffset.UtcNow.AddSeconds(5);
                while (endOfWarmup > DateTimeOffset.UtcNow)
                {
                    // force warmup
                    await httpClient.GetAsync(url);
                }
                Console.WriteLine("Preparing testrun..");
                // verify endpoint is up and running
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // begin perf test
                Console.WriteLine($"Running test for {testDuration} seconds..");
                var endOfTest = DateTimeOffset.UtcNow.AddSeconds(testDuration);
                var watch = new Stopwatch();
                var durations = new List<long>();
                while (endOfTest > DateTimeOffset.UtcNow)
                {
                    watch.Restart();
                    response = await httpClient.GetAsync(url);
                    watch.Stop();
                    response.EnsureSuccessStatusCode();
                    durations.Add(watch.ElapsedMilliseconds);
                }
                Console.WriteLine("Test finished!");
                var avg = durations.Average();
                Console.WriteLine($"Avg: {avg:0.000}ms");
            }
        }
    }
}
