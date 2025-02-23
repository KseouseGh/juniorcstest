using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary;
using System.Diagnostics;

namespace juniorcstest
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProfileBalance> pBalance { get; set; }
        private BitapiClient_REST RestConnector;

        public ViewModel()
        {
            pBalance = new ObservableCollection<ProfileBalance>();
            RestConnector = new BitapiClient_REST();
            pBalance.Add(new ProfileBalance
            {
                BTC = 1m,
                XRP = 15000m,
                XMR = 50m,
                DASH = 30m,
                USDT = 0m
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private decimal GetValidTickerValue(decimal? value) => value ?? 0m;

        public async Task UpdateBalance()
        {
            try
            {
                var btcTicker = await RestConnector.GetTickerAsync("tBTCUSD");
                var xrpTicker = await RestConnector.GetTickerAsync("tXRPUSD");
                var xmrTicker = await RestConnector.GetTickerAsync("tXMRUSD");
                var dashTicker = await RestConnector.GetTickerAsync("tDSHUSD");
                Debug.WriteLine("API Response Data");
                Debug.WriteLine($"BTC Ticker: {string.Join(", ", btcTicker)}");
                Debug.WriteLine($"XRP Ticker: {string.Join(", ", xrpTicker)}");
                Debug.WriteLine($"XMR Ticker: {string.Join(", ", xmrTicker)}");
                Debug.WriteLine($"DASH Ticker: {string.Join(", ", dashTicker)}");
                var btcToUsdt = GetValidTickerValue(btcTicker.ElementAtOrDefault(0));
                var xrpToUsdt = GetValidTickerValue(xrpTicker.ElementAtOrDefault(0));
                var xmrToUsdt = GetValidTickerValue(xmrTicker.ElementAtOrDefault(0));
                var dashToUsdt = GetValidTickerValue(dashTicker.ElementAtOrDefault(0));
                Debug.WriteLine("\nExtracted Prices");
                Debug.WriteLine($"BTC to USDT: {btcToUsdt}");
                Debug.WriteLine($"XRP to USDT: {xrpToUsdt}");
                Debug.WriteLine($"XMR to USDT: {xmrToUsdt}");
                Debug.WriteLine($"DASH to USDT: {dashToUsdt}");
                var currentBalance = pBalance.FirstOrDefault();

                if (currentBalance != null)
                {
                    decimal btcInUsdt = currentBalance.BTC * btcToUsdt;
                    decimal xrpInUsdt = currentBalance.XRP * xrpToUsdt;
                    decimal xmrInUsdt = currentBalance.XMR * xmrToUsdt;
                    decimal dashInUsdt = currentBalance.DASH * dashToUsdt;
                    Debug.WriteLine($"BTC in USDT: {btcInUsdt}");
                    Debug.WriteLine($"XRP in USDT: {xrpInUsdt}");
                    Debug.WriteLine($"XMR in USDT: {xmrInUsdt}");
                    Debug.WriteLine($"DASH in USDT: {dashInUsdt}");
                    var updatedBalance = new ProfileBalance
                    {
                        BTC = currentBalance.BTC,
                        XRP = currentBalance.XRP,
                        XMR = currentBalance.XMR,
                        DASH = currentBalance.DASH,
                        USDT = btcInUsdt + xrpInUsdt + xmrInUsdt + dashInUsdt
                    };
                    pBalance.Clear();
                    pBalance.Add(updatedBalance);
                    OnPropertyChanged(nameof(pBalance));
                }
                else
                {
                    Debug.WriteLine("Error! No current balance found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during balance update! {ex.Message}");
            }
        }
    }
}