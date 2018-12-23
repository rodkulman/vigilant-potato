using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rodkulman.RedditReader
{
    public class Keys
    {
        public static async Task<string> Get(string keyName)
        {
            using (var file = File.OpenRead("keys.json"))
            using (var textReader = new StreamReader(file, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                var keys = await JObject.LoadAsync(jsonReader);

                return keys.Value<string>(keyName);
            }
        }
    }
}