using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AlgoLucy.Universe;
using AlgoLucy.Universe.Configuration;
using Newtonsoft.Json.Linq;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data.Market;

namespace AlgoLucy
{
    public class TradeSummaryLocal
    {
        public string ID { get; set; }
        public string Symbol { get; set; }
        public decimal quantity { get; set; }
        public long setupTime { get; set; }
        public long entryTime { get; set; }
        public long exitTime { get; set; }
        public decimal entryPrice { get; set; }
        public decimal exitPrice { get; set; }
        public decimal stopLossPrice { get; set; }
        public decimal takeProfitPrice { get; set; }
        public long expirationTime { get; set; }
        public decimal supportPrice { get; set; }
        public decimal support2Price { get; set; }
        public decimal resistancePrice { get; set; }
        public decimal resistance2Price { get; set; }
        public LocalQuote[] Serie { get; set;  }
        public List<LocalQuote> _Serie { get; } = new List<LocalQuote>();
    }
    public class LocalQuote
    {
        public long date;
        public decimal open;
        public decimal high;
        public decimal low;
        public decimal close;
        public long volume;
    }

    public class PostTradeAnalysisAlgo : QCAlgorithm
    {
        public TradeSummaryLocal[] trades;
        public override void Initialize()
        {
            SetTimeZone(NodaTime.DateTimeZone.Utc);
            SetStartDate(2017, 10, 1);  //Set Start Date
            SetEndDate(2018, 6, 1);
            SetCash(1);


            foreach (var symbol in AlgoConfiguration.forexsymbols)
            {
                AddSecurity(SecurityType.Forex, symbol, Resolution.Hour, Market.Oanda, false, 1, false);
            }

            foreach (var symbol in AlgoConfiguration.cfdsymbols)
            {
                AddSecurity(SecurityType.Cfd, symbol, Resolution.Hour, Market.Oanda,false,1,false);
            }
            trades = loadCsvFile("AlgoLucy-1555768468385-trades.csv").ToArray();
        }
        public override void OnEndOfAlgorithm()
        {
            base.OnEndOfAlgorithm();

            foreach (var rule in trades)
            {
                rule.Serie = rule._Serie.ToArray();
                Utils.WriteToFile("AlgoLucy-1555768468385-2",string.Format("json-{0}-trade.json", rule.ID), JObject.FromObject(rule).ToString(), false);

            }
        }

        public List<TradeSummaryLocal> loadCsvFile(string filePath)
        {
            var reader = new StreamReader(File.OpenRead(filePath));
            List<TradeSummaryLocal> list = new List<TradeSummaryLocal>();
            reader.ReadLine(); ;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] str = line.Split(';');
                list.Add(new TradeSummaryLocal

                {
                    ID = str[0],
                    Symbol = str[1],
                    quantity = Convert.ToDecimal(str[3], CultureInfo.InvariantCulture),
                    setupTime = Utils.ToUnixTimestamp(DateTime.Parse(str[7])),//, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                    entryTime = Utils.ToUnixTimestamp(DateTime.Parse(str[9])),//, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                    expirationTime = Utils.ToUnixTimestamp(DateTime.Parse(str[8])),//, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                    exitTime = Utils.ToUnixTimestamp(DateTime.Parse(str[27])),//, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                                                                              //                   setupTime = Utils.ToUnixTimestamp(DateTime.ParseExact(str[7], "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                                                                              //                   entryTime = Utils.ToUnixTimestamp(DateTime.ParseExact(str[9], "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                                                                              //                    exitTime = Utils.ToUnixTimestamp(DateTime.ParseExact(str[27], "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture)),
                    stopLossPrice = Convert.ToDecimal(str[13], CultureInfo.InvariantCulture),
                    takeProfitPrice = Convert.ToDecimal(str[15], CultureInfo.InvariantCulture),
                    entryPrice = Convert.ToDecimal(str[12], CultureInfo.InvariantCulture),
                    exitPrice = Convert.ToDecimal(str[28], CultureInfo.InvariantCulture),
                    supportPrice = Convert.ToDecimal(str[19], CultureInfo.InvariantCulture),
                    support2Price = Convert.ToDecimal(str[20], CultureInfo.InvariantCulture),
                    resistancePrice = Convert.ToDecimal(str[21], CultureInfo.InvariantCulture),
                    resistance2Price = Convert.ToDecimal(str[22], CultureInfo.InvariantCulture)
                });
            }

            return list;
        }

        public void OnData(QuoteBars data)
        {
            long l = (30 * 24 * 60 * 60);
            long le = (10 * 24 * 60 * 60);
            foreach (var rule in trades)
            {
                if (data.ContainsKey(rule.Symbol))
                {
                    var candleStartTime =Utils.ToUnixTimestamp(data[rule.Symbol].Time.ConvertToUtc(Securities[rule.Symbol].Exchange.TimeZone));
                    var candleEndTime = Utils.ToUnixTimestamp(data[rule.Symbol].EndTime.ConvertToUtc(Securities[rule.Symbol].Exchange.TimeZone));

                    if ((rule._Serie.Count==0 && (rule.setupTime-l) > candleStartTime ) || (rule._Serie.Count > 0 && (rule.exitTime+ le > candleStartTime || rule.expirationTime+ le > candleStartTime)))
                    {
                        List<LocalQuote> serie = new List<LocalQuote>();
                        rule._Serie.Add(new LocalQuote
                        {
                            date = Utils.ToUnixTimestamp(data[rule.Symbol].Time.ConvertToUtc(Securities[rule.Symbol].Exchange.TimeZone)),
                            open = data[rule.Symbol].Open,
                            close = data[rule.Symbol].Close,
                            high = data[rule.Symbol].High,
                            low = data[rule.Symbol].Low
                        });
                    }
                }
            }
        }
 
    }


}
