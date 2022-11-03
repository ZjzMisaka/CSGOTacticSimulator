using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HltvSharp.Parsing
{
    public static partial class HltvParser
    {
        public static async Task<T> FetchPage<T>(string url, Func<Task<HttpResponseMessage>, T> continueWith, WebProxy proxy = null)
        {
            var httpClientHandler = new HttpClientHandler();

            if (proxy != null)
            {
                httpClientHandler.UseProxy = true;
                httpClientHandler.Proxy = proxy;
            }

            var client = new HttpClient(httpClientHandler);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://www.hltv.org/" + url),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("Referer", "https://www.hltv.org/");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            T result = await client.SendAsync(request).ContinueWith((response) => continueWith(response));

            return result;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        public static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }
    }
}
