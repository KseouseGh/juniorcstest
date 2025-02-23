using ConnectorTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHQ;

namespace ClassLibrary
{
    public class BitapiClient : ITestConnector
    {
        private BitapiClient_REST REST;
        private BitapiClient_WS WS;

        public BitapiClient()
        {
            REST = new BitapiClient_REST();
            WS = new BitapiClient_WS();
        }

        #region Rest

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            return await REST.GetNewTradesAsync(pair, maxCount);
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            return await REST.GetCandleSeriesAsync(pair, periodInSec, from, to, count);
        }

        public async Task<IEnumerable<decimal>> GetTickerAsync(string pair)
        {
            return await REST.GetTickerAsync(pair);
        }

        #endregion

        #region Socket

        public event Action<Trade> NewBuyTrade
        {
            add { WS.NewBuyTrade += value; }
            remove { WS.NewBuyTrade -= value; }
        }

        public event Action<Trade> NewSellTrade
        {
            add { WS.NewSellTrade += value; }
            remove { WS.NewSellTrade -= value; }
        }

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            WS.SubscribeTrades(pair, maxCount);
        }

        public void UnsubscribeTrades(string pair)
        {
            WS.UnsubscribeTrades(pair);
        }

        public event Action<Candle> CandleSeriesProcessing
        {
            add { WS.CandleSeriesProcessing += value; }
            remove { WS.CandleSeriesProcessing -= value; }
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            WS.SubscribeCandles(pair, periodInSec, from, to, count);
        }

        public void UnsubscribeCandles(string pair)
        {
            WS.UnsubscribeCandles(pair);
        }

        #endregion
    }
}