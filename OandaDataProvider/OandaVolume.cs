/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.Lean.Engine.DataFeeds
{

    // <seealso cref="QuantConnect.Data.BaseData" />
    public abstract class OandaVolume : BaseData
    {
        private string __url = "https://api-fxtrade.oanda.com/v3/instruments/{0}/candles?price={1}&from={2}&to={3}&granularity={4}";
        private string _url = "https://api-fxtrade.oanda.com/v3/instruments/{0}/candles?price={1}&from={2}&count=5000&granularity={3}";
        private string liveurl = "https://api-fxtrade.oanda.com/v3/instruments/{0}/candles?price={1}&count=1&granularity={3}";
        private string ___url = "https://api-fxtrade.oanda.com/v3/instruments/{0}/candles?price={1}&from={2}&to={3}&granularity={4}";
        private string _price = "BA";
        private DateTime _previousDate = DateTime.MinValue;

        /// <summary>
        ///     Sum of opening and closing Volume for the entire time interval.
        ///     The volume measured in the QUOTE CURRENCY.
        /// </summary>
        /// <remarks>Please remember to convert this data to a common currency before making comparison between different pairs.</remarks>
        public long Volume { get; set; }
        private readonly string _token = Config.Get("oanda-data-access-token", "");
        private SecurityType _SecurityType = SecurityType.Base;
        public SecurityType SecurityType
        {
            get
            {
                return _SecurityType;
            }
            set
            {
                _SecurityType = value;
            }
        }

        /// <summary>
        ///     Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>
        ///     String URL of source file.
        /// </returns>
        /// <exception cref="System.NotImplementedException">FOREX Volume data is not available in live mode, yet.</exception>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            var utcDate = date.ConvertToUtc(config.ExchangeTimeZone);
            string r = "S5";
            string filename = "";
            string id = config.Symbol.ID.Symbol.Split('/')[1];
            switch (config.Resolution)
            {
                case Resolution.Minute:
                    r = "M1";
                    filename = string.Format("{0}{1:D2}{2:D2}_{3}_minute_quote.csv", new object[] { utcDate.Year, utcDate.Month, utcDate.Day, id });
                    break;
                case Resolution.Second:
                    r = "S5";
                    filename = string.Format("{0}{1:D2}{2:D2}_{3}_second_quote.csv", new object[] { utcDate.Year, utcDate.Month, utcDate.Day, id });
                    break;
                case Resolution.Tick:
                    r = "S5";
                    filename = string.Format("{0}{1:D2}{2:D2}_{3}_tick_quote.csv", new object[] { utcDate.Year, utcDate.Month, utcDate.Day, id });
                    break;
                case Resolution.Hour:
                    r = "H1";
                    filename = string.Format("{0}.csv", id);
                    break;
                case Resolution.Daily:
                    r = "D";
                    filename = string.Format("{0}.csv", id);
                    break;
            }

            string s = id.Insert(id.Length - 3, "_");
            DateTime todate = utcDate.AddDays(1d);
            if (todate > DateTime.UtcNow)
            {
                todate = DateTime.UtcNow;
            }
            //string urlold = string.Format(__url, s, _price, ToUnixTimestamp(date.ToUniversalTime()), ToUnixTimestamp(todate), r);
            string url = string.Format(isLiveMode?liveurl:_url, s, _price, ToUnixTimestamp(utcDate), r);

            var auth = "Bearer " + _token;
            List<KeyValuePair<string, string>> header = new List<KeyValuePair<string, string>>();
            header.Add(new KeyValuePair<string, string>("content-type", "application/json"));
            header.Add(new KeyValuePair<string, string>("Authorization", auth));

            if (isLiveMode)
            {
                var source = url;
                return new SubscriptionDataSource(source, SubscriptionTransportMedium.Rest, FileFormat.Csv, header);
            }
            else
            {
                var source = GenerateZipFilePath(config, utcDate);
                return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile);
            }
        }

        public abstract OandaVolume CreateData(SubscriptionDataConfig config);

        /// <summary>
        ///     Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method,
        ///     and returns a new instance of the object
        ///     each time it is called. The returned object is assumed to be time stamped in the config.ExchangeTimeZone.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>
        ///     Instance of the T:BaseData object generated by this line of the CSV
        /// </returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var brokerVolume = CreateData(config);

            if (isLiveMode)
            {
                try
                {
                    Instrument ins = Instrument.FromJson(line);
                    foreach (Candle candle in ins.Candles)
                    {
                        brokerVolume.Time = candle.Time.ConvertFromUtc(config.ExchangeTimeZone);
                        brokerVolume.Volume = candle.Volume;
                    }

                    if(brokerVolume.Time == _previousDate)
                    {
                        return null;
                    }
                   
                    if (ins.Candles.Length==0)
                    {
                        return null;
                    }
                    _previousDate = brokerVolume.Time;
                }
                catch (Exception exception)
                {
                    //Logging.Log.Error($"Invalid data. Line: {line}. Exception: {exception.Message}");
                    return null;
                }
            }
            else
            {
                var obs = line.Split(',');
                if (config.Resolution == Resolution.Minute)
                {
                    brokerVolume.Time = date.Date.AddMilliseconds(int.Parse(obs[0])).ConvertFromUtc(config.ExchangeTimeZone);
                }
                else
                {
                    brokerVolume.Time = DateTime.ParseExact(obs[0], "yyyyMMdd HH:mm", CultureInfo.InvariantCulture).ConvertFromUtc(config.ExchangeTimeZone);
                }
                brokerVolume.Volume = long.Parse(obs[5]);
            }
            return brokerVolume;
        }

        private string GenerateZipFilePath(SubscriptionDataConfig config, DateTime date)
        {
            string id = config.Symbol.ID.Symbol.Split('/')[1];
            var source = Path.Combine(new[] { Globals.DataFolder, SecurityType.ToLower(), "oanda", config.Resolution.ToLower() });
            string filename;

            var symbol = id.Split('_').First().ToLower();
            if (config.Resolution == Resolution.Minute)
            {
                filename = string.Format("{0:yyyyMMdd}_quote.zip", date);
                source = Path.Combine(source, symbol, filename);
            }
            else
            {
                filename = string.Format("{0}_quote.zip", symbol);
                source = Path.Combine(source, filename);
            }
            return source;
        }

        private string JSonToCSV(DateTime date, string json)
        {

            string content = "";
            string row = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}";

            Instrument ins = Instrument.FromJson(json);
            foreach (Candle candle in ins.Candles)
            {
                content = content + string.Format(row, new object[] { (candle.Time.ToUniversalTime() - date.ToUniversalTime()).TotalMilliseconds, candle.Bid.O, candle.Bid.H, candle.Bid.L, candle.Bid.C, candle.Volume, candle.Ask.O, candle.Ask.H, candle.Ask.L, candle.Ask.C, candle.Volume }) + System.Environment.NewLine;
            }
            return content;
        }

        private int ToUnixTimestamp(DateTime dateTime)
        {
            return (int)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                     new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }

    public class CfdOandaVolume : OandaVolume
    {
        public CfdOandaVolume()
        {
            SecurityType = SecurityType.Cfd;
        }

        public override OandaVolume CreateData(SubscriptionDataConfig config)
        {
            return new CfdOandaVolume { DataType = MarketDataType.Base, Symbol = config.Symbol };

        }
    }

    public class ForexOandaVolume : OandaVolume
    {
        public ForexOandaVolume()
        {
            SecurityType = SecurityType.Forex;
        }

        public override OandaVolume CreateData(SubscriptionDataConfig config)
        {
            return new ForexOandaVolume { DataType = MarketDataType.Base, Symbol = config.Symbol };
        }
    }
}
