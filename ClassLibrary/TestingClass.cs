using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ClassLibrary
{
    [TestFixture]
    public class TestingClass
    {
        private BitapiClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = new BitapiClient();
        }

        [Test]
        public async Task GetNewTradesAsync_t()
        {
            string pair = "BTCUSD";
            int maxCount = 5;
            var trades = await _client.GetNewTradesAsync(pair, maxCount);
            Assert.That(trades, Is.Not.Null);
            Assert.That(trades, Is.Not.Empty);
            Assert.That(trades.Count(), Is.EqualTo(maxCount));
        }

        [Test]
        public async Task GetCandleSeriesAsync_t()
        {
            string pair = "BTCUSD";
            int periodInSec = 60;
            DateTimeOffset from = DateTimeOffset.Now.AddDays(-1);
            var candles = await _client.GetCandleSeriesAsync(pair, periodInSec, from);
            Assert.That(candles, Is.Not.Null);
            Assert.That(candles, Is.Not.Empty);
        }

        [Test]
        public async Task GetTickerAsync_t()
        {
            string pair = "BTCUSD";
            var ticker = await _client.GetTickerAsync(pair);
            Assert.That(ticker, Is.Not.Null);
            Assert.That(ticker.Count(), Is.EqualTo(5));
        }

        [Test]
        public void SubscribeTrades_tfBuy()
        {
            string pair = "BTCUSD";
            bool buyTradeReceived = false;
            _client.NewBuyTrade += (trade) =>
            {
                if (trade.Side == "buy")
                {
                    buyTradeReceived = true;
                }
            };
            _client.SubscribeTrades(pair);
            Thread.Sleep(5000);
            Assert.That(buyTradeReceived, Is.True);
        }

        [Test]
        public void SubscribeTrades_tfSell()
        {
            string pair = "BTCUSD";
            bool sellTradeReceived = false;
            _client.NewSellTrade += (trade) =>
            {
                if (trade.Side == "sell")
                {
                    sellTradeReceived = true;
                }
            };
            _client.SubscribeTrades(pair);
            Thread.Sleep(5000);
            Assert.That(sellTradeReceived, Is.True);
        }

        [Test]
        public void SubscribeCandles_t()
        {
            string pair = "BTCUSD";
            int periodInSec = 60;
            bool candleReceived = false;
            _client.CandleSeriesProcessing += (candle) =>
            {
                candleReceived = true;
            };
            _client.SubscribeCandles(pair, periodInSec);
            Thread.Sleep(5000);
            Assert.That(candleReceived, Is.True);
        }

        [Test]
        public void UnsubscribeTrades_t()
        {
            string pair = "BTCUSD";
            bool tradeReceived = false;
            _client.NewBuyTrade += (trade) =>
            {
                tradeReceived = true;
            };
            _client.SubscribeTrades(pair);
            Thread.Sleep(5000);
            _client.UnsubscribeTrades(pair);
            Thread.Sleep(5000);
            Assert.That(tradeReceived, Is.False);
        }
    }
}