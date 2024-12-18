using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    public static void Main(string[] args)
    {
ServicePointManager
    .ServerCertificateValidationCallback += 
    (sender, cert, chain, sslPolicyErrors) => true;

//        string cookie = args[0];
string cookie ="uaid=CG-vj0nfQJUf3ShDkYRUUAOXs5BjZACCtLZPc0B0euxplmql0sTMFCUrJe_ApPigknK3vHDDoAhvo6SiUPfUxGCX_HTfTHOlWgYA; user_prefs=3eWj-M5G9GmRNmfEvfnTjbKyhWVjZACCtLZPc2B0tFJosIuSTl5pTo6OUmqebmiwko4SiACLGEEoXEQsAwA.; fve=1720119964.0; _fbp=fb.1.1720119964942.4614821615020090; exp_ebid=m=ho%2BKVwYVSZQJDIvBljJ07v8HAEkUPC%2BsBGFfavsHn%2FU%3D,v=f_KY7sJ9PDzKvax5_O6t3KNhuB2Xfcez; datadome=9VuVBts0OIb9f5_5qdF1twe6Za0CIvhBUwF~BXj4d_EIAJS9Dyda2vH_7hk1bsQSkDs~8SKY6CUnAAXSK2_YaKJDmBjRbeXW60WqqecSYmjvjf9ButSZvR8fXXHJ6c7D; ua=531227642bc86f3b5fd7103a0c0b4fd6; _ga_KR3J610VYM=GS1.1.1734549542.277.1.1734550209.60.0.0; _ga=GA1.1.1800482875.1720119965; _pin_unauth=dWlkPVptSXdOVEEzTlRFdFpHUmhNaTAwTVdFM0xXRmxNMkV0WldJek5HVXlNbU14Wm1Fdw; __pdst=5c6bfd5d1b014a8f9e0cf7d41e5e1fd2; _tt_enable_cookie=1; _ttp=m_WXQxIANkHCLlDyNqXN-uq2bq9.tt.1; session-key-www=758983324-1292277030337-b06f2f0b9c070a45b2fc262647cde1aee159d04a1778909f0d115800|1737066463; LD=1; granify.uuid=372faaf7-5cb4-456d-8bf4-c220f591d497; granify.new_user.qivBM=false; granify.session.qivBM=-1; ship_by_date_seller_v_client_timezone_analytics=true; dashboard_stats_range=last_30; _gcl_au=1.1.2093943453.1727900594; wedding_session=FVoFPAf6b0CKVNtveHU3cHcHvgZjZACCdFlHKzgNAA..; sitewide-hum-dismissed=1; lantern=54244693-a3c2-4a32-bd19-fc5384ba6a4a; coupons=6si9j0blYE6qYIW8GXjaL7ZN4SVjZACC9Jgb6TC6Wik5v7QgPy8-OT8ltVjJKrpaqTgjvyA-M0XJysTQzNLC0tJcB1mNkpWSW5Crq5O_v7dSbWwtAwA.; last_browse_page=https%3A%2F%2Fwww.etsy.com%2Fshop%2FWanderingRobotStudio; daily_deals_listings=1596167817,1702238448,1829763022,733032361,1708598599,1694271567,1070745316,1445186936,799365303,1294730520,1601559087,1622608671,1568526634,1270307208,1453716245; _uetsid=23c43020bcc611efa24ef9876c7f23b0; _uetvid=76f1d8503a3811efb9b4abbe80a972f9";
//string result = MakeRequest("/your/shops/me/dashboard", cookie);
//        Console.WriteLine(result);
//        string context = result.Split(System.Environment.NewLine).Where(s => s.Contains("Etsy.Context")).First();
//        string shopID = string.Empty;
        int listingCount = 0;

//        context = context.Split("window.Etsy=window.Etsy||{};Etsy.Context=")[1].Replace(";</script>", string.Empty);

  //      JObject c = JsonConvert.DeserializeObject(context) as JObject;

        string shopID ="41698997";

        JObject listingSummary = JObject.Parse(MakeRequest("/api/v3/ajax/shop/"+shopID+"/summary", cookie));

        foreach (JObject obj in listingSummary["sections"])
            listingCount += obj["active_listing_count"].Value<int>();

        //listingCount = (c["data"]["shop_data"]["all_active_listing_ids"] as JArray).Count;

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

                    //byte[] f = Download(file["url"].Value<string>(), cookie);
                    //File.WriteAllBytes(id + "/" + file["name"].Value<string>(), f);
                }
            }
        }

        static string MakeRequest(string url, string cookie, string verb = "GET", string? body = null)
        {
            int i = 0;
            while (i < 5)
            {
                try {
                Thread.Sleep(1);
                var baseAddress = new Uri("https://www.etsy.com");
                using (var handler = new HttpClientHandler { UseCookies = false })
                {
                
                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

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
                }
                catch (Exception ex)
                {
                    i++;
                    continue;
                }
            }

            return string.Empty;
        }

        static byte[] Download(string url, string cookie, string verb = "GET", string? body = null)
        {
            int i = 0;
            while (i < 5)
            {
                try{
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

                        List<byte> resp = [];
                        Stream s = result.Content.ReadAsStream();
                        using (StreamReader rdr = new(s))
                        {
                            while (true) {
                                int r = rdr.Read();
                                if (r == -1)
                                    break;
                                resp.Add((byte)r);
                            }
                        }
                        return [.. resp];
                    }
                }
                catch (Exception ex)
                {
                    i++;
                    continue;
                }
            }

            return [];
        }
    }
}