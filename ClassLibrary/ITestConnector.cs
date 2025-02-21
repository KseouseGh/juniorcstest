using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestHQ;
using System.Net.Http;

namespace ConnectorTest
{
    interface ITestConnector
    {
        #region Rest

        Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
        Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
        Task<IEnumerable<decimal>> GetTickerAsync(string pair);

        #endregion

        #region Socket


        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        void SubscribeTrades(string pair, int maxCount = 100);
        void UnsubscribeTrades(string pair);

        event Action<Candle> CandleSeriesProcessing;
        void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        void UnsubscribeCandles(string pair);

        #endregion

    }
}