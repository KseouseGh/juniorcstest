using ConnectorTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestHQ;
using Newtonsoft.Json;

namespace ClassLibrary
{
    [Serializable]
    public class BitapiClient_REST
    {
        HttpClient client = new HttpClient();

        public BitapiClient_REST() { }

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            string url = $"https://api-pub.bitfinex.com/v2/trades/{pair}/hist?limit={maxCount}";
            HttpResponseMessage Response=await client.GetAsync(url);

            if (!Response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error in request \n {Response.StatusCode} on url {url}");
                return new List<Trade>();
            }

            string Sresponse=await Response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(Sresponse))
            {
                Debug.WriteLine($"Empty data from API on url {url}");
                return new List<Trade>();
            }

            var TradesList = JsonConvert.DeserializeObject<List<Trade>>(Sresponse);
            return TradesList;
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            string sfrom = from.HasValue ? ((DateTimeOffset)from).ToUnixTimeSeconds().ToString() : "0";
            string sto = to.HasValue ? ((DateTimeOffset)to).ToUnixTimeSeconds().ToString() : DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string url = $"https://api-pub.bitfinex.com/v2/candles/trade:{periodInSec}:{pair}/hist?start={sfrom}&end={sto}&limit={count}";
            HttpResponseMessage Response = await client.GetAsync(url);

            if (!Response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error in request \n {Response.StatusCode} on url {url}");
                return new List<Candle>();
            }

            string Sresponse = await Response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(Sresponse))
            {
                Debug.WriteLine($"Empty data from API on url {url}");
                return new List<Candle>();
            }

            var CandlesList = JsonConvert.DeserializeObject<List<Candle>>(Sresponse);
            return CandlesList;
        }

        public async Task<IEnumerable<decimal>> GetTickerAsync(string pair)
        {
            string url = $"https://api-pub.bitfinex.com/v2/ticker/{pair}";
            HttpResponseMessage Response = await client.GetAsync(url);

            if (!Response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error in request \n {Response.StatusCode} on url {url}");
                return new List<decimal>();
            }

            string Sresponse=await Response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(Sresponse))
            {
                Debug.WriteLine($"Empty data from API on url {url}");
                return new List<decimal>();
            }

            var TickersList=JsonConvert.DeserializeObject<List<decimal>>(Sresponse);
            return TickersList;
        }
    }
}