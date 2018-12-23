using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rodkulman.RedditReader
{
    class Program
    {
        private static readonly RestClient reddit = new RestClient("https://oauth.reddit.com");

        private static string accessToken = null;
        private static DateTime accessTokenExpiration = DateTime.MinValue;

        async static Task Main(string[] args)
        {
            var watches = GetWatches();

            while (true)
            {
                var authToken = await GetRedditAuthToken();

                var watchesBySubreddit = watches.GroupBy(x => x.Subreddit);

                foreach (var subWatches in watchesBySubreddit)
                {
                    var newRequest = new RestRequest($"r/{subWatches.Key}/new", Method.GET, DataFormat.Json);
                    newRequest.AddHeader("Authorization", $"bearer {authToken}");

                    var response = await reddit.ExecuteTaskAsync(newRequest);

                    if (response.IsSuccessful)
                    {
                        var listing = JObject.Parse(response.Content);

                        foreach (var post in listing["data"]["children"].Select(x => x["data"]))
                        {
                            if (subWatches.Any(x => Regex.IsMatch(post[x.Property].Value<string>(), x.Expression)))
                            {
                                var permalink = post["permalink"].Value<string>();

                                Process.Start($"https://www.reddit.com/{permalink}");
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }            
        }

        private static IEnumerable<SubredditWatch> GetWatches()
        {
            using (var file = File.OpenRead("watches.json"))
            using (var textReader = new StreamReader(file, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                var watches = JArray.Load(jsonReader);

                foreach (var watch in watches)
                {
                    using (var reader = watch.CreateReader())
                    {
                        yield return JsonSerializer.Create().Deserialize<SubredditWatch>(reader);
                    }
                }
            }
        }

        private static async Task<string> GetRedditAuthToken()
        {
            if (!string.IsNullOrWhiteSpace(accessToken) && accessTokenExpiration > DateTime.Now)
            {
                return accessToken;
            }

            var client = new RestClient("https://www.reddit.com/api/v1/access_token");
            var request = new RestRequest(Method.POST)
            {
                Credentials = new NetworkCredential(await Keys.Get("reddit"), string.Empty)
            };

            request.AddParameter("grant_type", "https://oauth.reddit.com/grants/installed_client");
            request.AddParameter("device_id", "DO_NOT_TRACK_THIS_DEVICE");

            var response = await client.ExecuteTaskAsync(request);

            if (response.IsSuccessful)
            {
                var data = JObject.Parse(response.Content);

                accessToken = data.Value<string>("access_token");
                accessTokenExpiration = DateTime.Now.AddSeconds(data.Value<int>());

                return accessToken;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
