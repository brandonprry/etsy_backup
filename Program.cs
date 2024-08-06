using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        string cookie = args[0];

        System.Console.WriteLine("Hello, World!");
        string result = MakeRequest("/your/shops/me/dashboard", cookie);
        string context = result.Split(System.Environment.NewLine).Where(s => s.Contains("Etsy.Context")).First();


        Regex count = new Regex("\\\"active_listing_count\\\":.*?,");
        Regex shop = new Regex("\\\"shop_id\\\":.*?,");

        string shopID = string.Empty;
        int listingCount = 0;

        context=context.Split("window.Etsy=window.Etsy||{};Etsy.Context=")[1].Replace(";</script>", string.Empty);

        JObject c = JsonConvert.DeserializeObject(context) as JObject;

        shopID = c["data"]["shop_data"]["shop_id"].Value<string>();
        listingCount = (c["data"]["shop_data"]["all_active_listing_ids"] as JArray).Count;
        
        int page = 0;
        int i = 0;
        Console.WriteLine("Getting "+listingCount+" listings for shop ID " + shopID);
        while (page * 40 < listingCount)
        {
            i = page*40;
            string item = MakeRequest("/api/v3/ajax/shop/"+shopID+"/listings/search?limit=200&offset="+i+"&sort_field=ending_date&sort_order=descending&state=active&language_id=0&query=&shop_section_id=&listing_tag=&is_featured=&shipping_profile_id=&return_policy_id=&production_partner_id=&is_retail=true&is_retail_only=&is_pattern=&is_pattern_only=&is_digital=&channels=&is_waitlisted=&has_video=", cookie);
            
            page = page+1;
            Console.WriteLine(page+".json");
            File.WriteAllText(page+".json", item);
        }

    }

 static string MakeRequest(string url, string cookie, string verb = "GET", string? body = null)
 {
    var baseAddress = new Uri("https://www.etsy.com");
using (var handler = new HttpClientHandler { UseCookies = false })
using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
{
    var message = new HttpRequestMessage(HttpMethod.Get, url);
    message.Headers.Add("Cookie", cookie);
    var result = client.Send(message);
    result.EnsureSuccessStatusCode();

    return new StreamReader(result.Content.ReadAsStream()).ReadToEnd();
}
 }
 
}