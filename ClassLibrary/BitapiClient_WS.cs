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

namespace ClassLibrary
{
    internal class BitapiClient_WS
    {
        public BitapiClient_WS() { }

        private ClientWebSocket WS;

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        private async Task Connect(string pair, int maxCount)
        {
            WS = new ClientWebSocket();
            await WS.ConnectAsync(new Uri("wss://api-pub.bitfinex.com/ws/2"), CancellationToken.None);
            
        }

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            Task.Run(async () =>
            {
                await Connect(pair, maxCount);
            });
        }
    }
}