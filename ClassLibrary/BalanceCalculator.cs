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
            var tickerBTCtoUSDT = (await connector.GetTickerAsync("BTCUSDT")).FirstOrDefault();
            var tickerXRPtoUSDT = (await connector.GetTickerAsync("XRPUSDT")).FirstOrDefault();
            var tickerXMRtoUSDT = (await connector.GetTickerAsync("XMRUSDT")).FirstOrDefault();
            var tickerDASHtoUSDT = (await connector.GetTickerAsync("DASHUSDT")).FirstOrDefault();
            var tickerBTCtoXRP = (await connector.GetTickerAsync("BTCXRP")).FirstOrDefault();
            var tickerBTCtoXMR = (await connector.GetTickerAsync("BTCXMR")).FirstOrDefault();
            var tickerBTCtoDASH = (await connector.GetTickerAsync("BTCDASH")).FirstOrDefault();
            GeneralBalance.USDT = (GeneralBalance.BTC * tickerBTCtoUSDT) + (GeneralBalance.XRP * tickerXRPtoUSDT) + (GeneralBalance.XMR * tickerXMRtoUSDT) + (GeneralBalance.DASH * tickerDASHtoUSDT);
            decimal totalBTC = GeneralBalance.BTC + (GeneralBalance.XRP / tickerBTCtoXRP) + (GeneralBalance.XMR / tickerBTCtoXMR) + (GeneralBalance.DASH / tickerBTCtoDASH);
            GeneralBalance.XRP = totalBTC * tickerBTCtoXRP;
            GeneralBalance.XMR = totalBTC * tickerBTCtoXMR;
            GeneralBalance.DASH = totalBTC * tickerBTCtoDASH;
            GeneralBalance.BTC = totalBTC;
            return GeneralBalance;
        }
    }
}