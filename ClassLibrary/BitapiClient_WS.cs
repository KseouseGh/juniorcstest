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
    public class BitapiClient_WS
    {
        public BitapiClient_WS() { }

        private ClientWebSocket WS;

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        private void ProcessResponse_Trades(string Json)
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

        private void ProcessResponse_Candles(string Json)
        {
            Console.WriteLine($"New candle data! {Json}");
            try
            {
                var data = JsonConvert.DeserializeObject<JArray>(Json);
                if (data == null || data.Count < 2) { return; }
                var candlesArray = data[1] as JArray;

                foreach (var candle in candlesArray)
                {
                    Candle newCandle = new Candle
                    {
                        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds((long)candle[0]),
                        OpenPrice = (decimal)candle[1],
                        HighPrice = (decimal)candle[2],
                        LowPrice = (decimal)candle[3],
                        ClosePrice = (decimal)candle[4],
                        TotalPrice = (decimal)candle[5],
                        TotalVolume = (decimal)candle[6],
                        Pair = ""
                    };
                    CandleSeriesProcessing?.Invoke(newCandle);
                    Console.WriteLine($"New candle! Time: {newCandle.OpenTime}, Open: {newCandle.OpenPrice}, Close: {newCandle.ClosePrice}, Volume: {newCandle.TotalVolume}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while processing candle data! {e.Message}");
            }
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
                try
                {
                    var jsonArray = JsonConvert.DeserializeObject<JArray>(Json);
                    if (jsonArray == null || jsonArray.Count < 2) { return; }
                    var channel = jsonArray[1]?.ToString();
                    if (string.IsNullOrEmpty(channel)) { return; }
                    if (channel.Equals("trades", StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessResponse_Trades(Json);
                    }
                    else if (channel.Equals("candles", StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessResponse_Candles(Json);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error processing message! {e.Message}");
                }
            }
        }

        private async Task ConnectTrades(string pair, int maxCount)
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
                await ConnectTrades(pair, maxCount);
            });
        }

        public async Task DisconnectTrades(string pair)
        {
            if (WS.State == WebSocketState.Open)
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
                await DisconnectTrades(pair);
            });
        }

        public async Task ConnectCandles(string pair,int periodInSec, DateTimeOffset? from, DateTimeOffset? to, long? count)
        {
            WS = new ClientWebSocket();
            await WS.ConnectAsync(new Uri("wss://api-pub.bitfinex.com/ws/2"), CancellationToken.None);
            string frequency = periodInSec == 60 ? "1m" : $"{periodInSec / 60}m";
            long? start = from?.ToUnixTimeMilliseconds();
            long? end = to?.ToUnixTimeMilliseconds();
            var SubMessage = new
            {
                @event = "subscribe",
                channel = "candles",
                symbol = $"t{pair.ToUpper()}",
                freq = frequency,
                limit = count ?? 100,
                start = start,
                end = end
            };
            var MessageJson = JsonConvert.SerializeObject(SubMessage);
            var MessageBytes = Encoding.UTF8.GetBytes(MessageJson);
            await WS.SendAsync(new ArraySegment<byte>(MessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Task.Run(async () => await Listener());
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            Task.Run(async () =>
            {
                await ConnectCandles(pair, periodInSec, from, to, count);
            }); 
        }

        private async Task DisconnectCandles(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to, long? count)
        {
            if (WS.State == WebSocketState.Open)
            {
                string frequency = periodInSec == 60 ? "1m" : $"{periodInSec / 60}m";
                long? start = from?.ToUnixTimeMilliseconds();
                long? end = to?.ToUnixTimeMilliseconds();
                var UnsubMessage = new
                {
                    @event = "unsubscribe",
                    channel = "candles",
                    symbol = $"t{pair.ToUpper()}",
                    freq = frequency,
                    limit = count ?? 100,
                    start = start,
                    end = end
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

        public void UnsubscribeCandles(string pair)
        {
            Task.Run(async () =>
            {
                int periodInSec = 60;
                DateTimeOffset? from = null;
                DateTimeOffset? to = null;
                long? count = 100;
                await DisconnectCandles(pair,periodInSec,from,to,count);
            });
        }
    }
}