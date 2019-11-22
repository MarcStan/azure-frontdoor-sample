using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontdoor.Ping
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("<url>");
                return;
            }
            var url = args[0];
            using (var httpClient = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
