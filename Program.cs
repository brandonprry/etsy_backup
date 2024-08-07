using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        string cookie = args[0];
        string result = MakeRequest("/your/shops/me/dashboard", cookie);
        string context = result.Split(System.Environment.NewLine).Where(s => s.Contains("Etsy.Context")).First();
        string shopID = string.Empty;
        int listingCount = 0;

        context = context.Split("window.Etsy=window.Etsy||{};Etsy.Context=")[1].Replace(";</script>", string.Empty);

        JObject c = JsonConvert.DeserializeObject(context) as JObject;

        shopID = c["data"]["shop_data"]["shop_id"].Value<string>();
        listingCount = (c["data"]["shop_data"]["all_active_listing_ids"] as JArray).Count;

        int page = 0;
        int i = 0;
        Console.WriteLine("Getting " + listingCount + " listings for shop ID " + shopID);
        while (page * 40 < listingCount)
        {
            i = page * 40;

            string item = MakeRequest("/api/v3/ajax/shop/" + shopID + "/listings/search?limit=200&offset=" + i + "&sort_field=ending_date&sort_order=descending&state=active&language_id=0&query=&shop_section_id=&listing_tag=&is_featured=&shipping_profile_id=&return_policy_id=&production_partner_id=&is_retail=true&is_retail_only=&is_pattern=&is_pattern_only=&is_digital=&channels=&is_waitlisted=&has_video=", cookie);

            page++;
            
            Console.WriteLine(page + ".json");
            File.WriteAllText(page + ".json", item);

            JArray listings = JsonConvert.DeserializeObject(File.ReadAllText(page + ".json")) as JArray;

            foreach (JObject o in listings)
            {
                string id = o["listing_id"].Value<string>();
                Directory.CreateDirectory(id);
                string listingDetails = MakeRequest("/api/v3/ajax/bespoke/shop/" + shopID + "/listings/" + id + "/form", cookie);
                File.WriteAllText(id + "/" + id + ".json", listingDetails);
                Console.WriteLine(o["url"].Value<string>());

                JObject o2 = JsonConvert.DeserializeObject(listingDetails) as JObject;
                foreach (JObject file in o2["listing"]["files"] as JArray)
                {
                    Console.WriteLine(file["name"].Value<string>());
                    Console.WriteLine(file["url"].Value<string>());

                    byte[] f = Download(file["url"].Value<string>(), cookie);
                    File.WriteAllBytes(id + "/" + file["name"].Value<string>(), f);
                }
            }
        }

        static string MakeRequest(string url, string cookie, string verb = "GET", string? body = null)
        {
            Thread.Sleep(1);
            var baseAddress = new Uri("https://www.etsy.com");
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:128.0) Gecko/20100101 Firefox/128.0");
                message.Headers.Add("Cookie", cookie);
                var result = client.Send(message);
                result.EnsureSuccessStatusCode();

                return new StreamReader(result.Content.ReadAsStream()).ReadToEnd();
            }
        }

        static byte[] Download(string url, string cookie, string verb = "GET", string? body = null)
        {
            Thread.Sleep(1);
            var baseAddress = new Uri("https://www.etsy.com");
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:128.0) Gecko/20100101 Firefox/128.0");
                message.Headers.Add("Cookie", cookie);
                var result = client.Send(message);
                result.EnsureSuccessStatusCode();

                List<byte> resp = new List<byte>();
                Stream s = result.Content.ReadAsStream();
                using (StreamReader rdr = new StreamReader(s))
                {
                    while (true) {
                        int r = rdr.Read();
                        if (r == -1)
                            break;
                        resp.Add((byte)r);
                    }
                }
                return resp.ToArray();
            }
        }
    }
}