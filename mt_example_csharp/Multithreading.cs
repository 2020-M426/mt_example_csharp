using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace mt_example_csharp
{
    internal static class Multithreading
    {
        private static readonly HttpClient Client = new HttpClient();

        private static async Task Main(string[] args)
        {
            await Http();
            Calc();
        }

        private static async Task Http()
        {
            const int httpRequests = 50;
            ICollection<Task<HttpStatusCode>> requestList = new List<Task<HttpStatusCode>>();
            for (var i = 0; i < httpRequests; i++)
            {
                var url = $"https://httpbin.org/status/{200 + i}";
                requestList.Add(ProcessUrlAsync(url));
            }

            var statusCodeSet = new HashSet<int>(50);
            Console.WriteLine();
            Console.WriteLine("== HTTP parallel ==");
            var stopwatch = Stopwatch.StartNew();

            while (requestList.Any())
            {
                var finishedTask = await Task.WhenAny(requestList);
                var httpStatusCode = await finishedTask;
                statusCodeSet.Add((int) httpStatusCode);
                requestList.Remove(finishedTask);
            }

            stopwatch.Stop();

            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}\n");
            Debug.Assert(statusCodeSet.Count == 50);


            Console.WriteLine();
            Console.WriteLine("== HTTP serial ==");
            statusCodeSet = new HashSet<int>(50);
            stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < httpRequests; i++)
            {
                var url = $"https://httpbin.org/status/{200 + i}";
                var httpStatusCode = ProcessUrl(url);
                statusCodeSet.Add((int) httpStatusCode);
            }

            stopwatch.Stop();

            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}\n");
            Debug.Assert(statusCodeSet.Count == 50);
        }

        private static HttpStatusCode ProcessUrl(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            var httpWebResponse = (HttpWebResponse) request.GetResponse();
            return httpWebResponse.StatusCode;
        }

        private static async Task<HttpStatusCode> ProcessUrlAsync(string url)
        {
            var responseMessage = await Client.GetAsync(url);
            return responseMessage.StatusCode;
        }

        private static void Calc()
        {
            var range = Enumerable.Range(30, 12);
            var rangeParallel = Enumerable.Range(30, 12);

            Console.WriteLine();
            Console.WriteLine("== Calc serial ==");
            var stopwatch = Stopwatch.StartNew();
            var sum = range.Sum(Fib);
            stopwatch.Stop();
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}\n");
            Debug.Assert(sum == 432148168);

            Console.WriteLine();
            Console.WriteLine("== Calc parallel ==");
            stopwatch = Stopwatch.StartNew();
            sum = rangeParallel.AsParallel().Sum(Fib);
            stopwatch.Stop();
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}\n");
            Debug.Assert(sum == 432148168);
        }

        private static int Fib(int n)
        {
            if (n <= 1) return n;
            return Fib(n - 1) + Fib(n - 2);
        }
    }
}