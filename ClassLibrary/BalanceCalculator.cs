using ConnectorTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class BalanceCalculator
    {
        private ITestConnector connector;

        private BalanceCalculator(ITestConnector Connector)
        {

            connector = Connector;
        }

        public async Task<ProfileBalance> GetBalanceAsync()
        {
            var GeneralBalance = new ProfileBalance
            {
                BTC = 1m,
                XRP = 15000m,
                XMR = 50m,
                DASH = 30m
            };
            decimal GetValidTickerValue(decimal? value) => value ?? 0m;
            var tickerBTCtoUSDT = GetValidTickerValue((await connector.GetTickerAsync("BTCUSDT")).FirstOrDefault());
            var tickerXRPtoUSDT = GetValidTickerValue((await connector.GetTickerAsync("XRPUSDT")).FirstOrDefault());
            var tickerXMRtoUSDT = GetValidTickerValue((await connector.GetTickerAsync("XMRUSDT")).FirstOrDefault());
            var tickerDASHtoUSDT = GetValidTickerValue((await connector.GetTickerAsync("DASHUSDT")).FirstOrDefault());
            var tickerBTCtoXRP = GetValidTickerValue((await connector.GetTickerAsync("BTCXRP")).FirstOrDefault());
            var tickerBTCtoXMR = GetValidTickerValue((await connector.GetTickerAsync("BTCXMR")).FirstOrDefault());
            var tickerBTCtoDASH = GetValidTickerValue((await connector.GetTickerAsync("BTCDASH")).FirstOrDefault());
            GeneralBalance.USDT = (GeneralBalance.BTC * tickerBTCtoUSDT)
                                + (GeneralBalance.XRP * tickerXRPtoUSDT)
                                + (GeneralBalance.XMR * tickerXMRtoUSDT)
                                + (GeneralBalance.DASH * tickerDASHtoUSDT);
            decimal btcFromXRP = tickerBTCtoXRP > 0 ? GeneralBalance.XRP / tickerBTCtoXRP : 0;
            decimal btcFromXMR = tickerBTCtoXMR > 0 ? GeneralBalance.XMR / tickerBTCtoXMR : 0;
            decimal btcFromDASH = tickerBTCtoDASH > 0 ? GeneralBalance.DASH / tickerBTCtoDASH : 0;
            decimal totalBTC = GeneralBalance.BTC + btcFromXRP + btcFromXMR + btcFromDASH;
            GeneralBalance.BTC = totalBTC;
            decimal equivalentXRP = totalBTC * tickerBTCtoXRP;
            decimal equivalentXMR = totalBTC * tickerBTCtoXMR;
            decimal equivalentDASH = totalBTC * tickerBTCtoDASH;
            return new ProfileBalance
            {
                BTC = GeneralBalance.BTC,
                XRP = GeneralBalance.XRP,
                XMR = GeneralBalance.XMR,
                DASH = GeneralBalance.DASH,
                USDT = GeneralBalance.USDT
            };
        }
    }
}