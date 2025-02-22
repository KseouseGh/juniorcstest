using ConnectorTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TestHQ;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ClassLibrary
{
    [Serializable]
    internal class BitapiClient_WS
    {
        public BitapiClient_WS() { }

        private ClientWebSocket WS;

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        private void ProcessResponse(string Json)
        {
            Console.WriteLine($"New message! {Json}");
            try
            {
                var data = JsonConvert.DeserializeObject<JArray>(Json);
                if (data == null || data.Count<2) { return; }
                var TradesArray = data[1] as JArray;

                foreach (var trade in TradesArray)
                {
                    Trade newTrade = new Trade
                    {
                        Id = trade[0].ToString(),
                        Time = DateTimeOffset.FromUnixTimeMilliseconds((long)trade[1]),
                        Price = (decimal)trade[2],
                        Amount = Math.Abs((decimal)trade[3]),
                        Side = (decimal)trade[3] > 0 ? "buy" : "sell",
                        Pair = ""
                    };

                    if (newTrade.Side == "buy")
                    {
                        NewBuyTrade?.Invoke(newTrade);
                    }
                    else { NewSellTrade?.Invoke(newTrade); }

                    Console.WriteLine($"New trade! {newTrade.Price} USD, {newTrade.Amount} BTC");
                }

            }
            catch (Exception e) { Console.WriteLine($"Error while processing of data! {e.Message}"); }
        }

        private async Task Listener()
        {
            var data = new List<byte>();

            while (WS.State == WebSocketState.Open)
            {
                var temp = new byte[1024];
                WebSocketReceiveResult res;
                do
                {
                    res = await WS.ReceiveAsync(new ArraySegment<byte>(temp), CancellationToken.None);
                    data.AddRange(temp.Take(res.Count));

                } while (!res.EndOfMessage);

                string Json = Encoding.UTF8.GetString(data.ToArray());
                data.Clear();
                ProcessResponse(Json);
            }
        }

        private async Task Connect(string pair, int maxCount)
        {
            WS = new ClientWebSocket();
            await WS.ConnectAsync(new Uri("wss://api-pub.bitfinex.com/ws/2"), CancellationToken.None);
            var SubMessage = new
            {
                @event = "subscribe",
                channel = "trades",
                symbol = $"t{pair.ToUpper()}"
            };
            var MessageJson = JsonConvert.SerializeObject(SubMessage);
            var MessageBytes = Encoding.UTF8.GetBytes(MessageJson);
            await WS.SendAsync(new ArraySegment<byte>(MessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Task.Run(async () => await Listener());
        }

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            Task.Run(async () =>
            {
                await Connect(pair, maxCount);
            });
        }

        public async Task Disconnect(string pair)
        {
            if (WS?.State == WebSocketState.Open)
            {
                var UnsubMessage = new
                {
                    @event = "unsubscribe",
                    channel = "trades",
                    symbol = $"t{pair.ToUpper()}"
                };
                var MessageJson = JsonConvert.SerializeObject(UnsubMessage);
                var MessageBytes = Encoding.UTF8.GetBytes(MessageJson);
                await WS.SendAsync(new ArraySegment<byte>(MessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("Closed WebSocket cannot be unsubscribed.");
            }
        }

        public void UnsubscribeTrades(string pair)
        {
            Task.Run(async () =>
            {
                await Disconnect(pair);
            });
        }

    }
}