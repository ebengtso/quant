using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AlgorithmUtils;
using Newtonsoft.Json.Linq;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data.Custom;
using QuantConnect.Data.Market;

namespace PostTradeAnalysis
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
        public decimal predictedPriceLow { get; set; }
        public decimal predictedPriceHigh { get; set; }
        // public LocalQuote[] Serie { get { return _Serie.ToArray(); } }
        public List<LocalQuote> Serie { get; } = new List<LocalQuote>();
       // public CalendarEvent[] Events { get { return _Events.ToArray(); } }
        public List<CalendarEvent> Events { get; } = new List<CalendarEvent>();
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

    public class CalendarEvent
    {
        public decimal price;
        public string forecast;
        public string actual;
        public long date;
        public string title;
        public string currency;
        public string meaning;
    }

    public class PostTradeAnalysisAlgo : QCAlgorithm
    {
        private static string csvFileName = "trades.csv";
        public TradeSummaryLocal[] trades;
        public override void Initialize()
        {
            SetTimeZone(NodaTime.DateTimeZone.Utc);
            //SetStartDate(2017, 10, 1);  //Set Start Date
            //SetEndDate(2018, 6, 1);
            SetStartDate(2018, 11, 1);  //Set Start Date
            SetEndDate(2019, 5, 10);
            SetCash(1);

            AddData<DailyFx>("DFX", Resolution.Minute, TimeZones.Utc);

            foreach (var symbol in TradingSymbols.OandaFXSymbols)
            {
                AddSecurity(SecurityType.Forex, symbol, Resolution.Hour, Market.Oanda, false, 1, false);
            }

            foreach (var symbol in TradingSymbols.OandaCFDAll)
            {
                AddSecurity(SecurityType.Cfd, symbol, Resolution.Hour, Market.Oanda,false,1,false);
            }
            trades = loadCsvFile(GetParameter("directory"), csvFileName).ToArray();
        }
        public override void OnEndOfAlgorithm()
        {
            base.OnEndOfAlgorithm();

            foreach (var rule in trades)
            {
                Utils.WriteToFile(GetParameter("directory"),string.Format("json-{0}-trade.json", rule.ID), JObject.FromObject(rule).ToString(), false);

            }
            ;
            Utils.WriteToFile(GetParameter("directory"), "list.json", JArray.FromObject(Directory.EnumerateFiles(GetParameter("directory"),"*.json").Select(Path.GetFileName)).ToString(), false);
        }

        public List<TradeSummaryLocal> loadCsvFile(string directory, string fileName)
        {
            Log(string.Format("Loading trades from directory {0}/{1}", directory, fileName));
            var reader = new StreamReader(File.OpenRead(string.Format("{0}/{1}", directory, fileName)));
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
                    resistance2Price = Convert.ToDecimal(str[22], CultureInfo.InvariantCulture),
                    predictedPriceLow = Convert.ToDecimal(str[37], CultureInfo.InvariantCulture),
                    predictedPriceHigh = Convert.ToDecimal(str[38], CultureInfo.InvariantCulture)
                });
            }

            return list;
        }

    public void OnData(DailyFx calendar)
    {
        if (calendar.Importance != FxDailyImportance.High) return;


            long l = (30 * 24 * 60 * 60);
            long le = (10 * 24 * 60 * 60);
            foreach (var rule in trades)
            {
                var currency = Securities[rule.Symbol].QuoteCurrency.Symbol;
                if (calendar.Currency.ToUpper().Equals(currency))
                {
  
                    if (rule.setupTime - l < Utils.ToUnixTimestamp(calendar.EventDateTime) && (rule.exitTime + le > Utils.ToUnixTimestamp(calendar.EventDateTime) || rule.expirationTime + le > Utils.ToUnixTimestamp(calendar.EventDateTime)))
                    {
                        rule.Events.Add(new CalendarEvent
                        {
                            date = Utils.ToUnixTimestamp(calendar.EventDateTime),
                            forecast = calendar.Forecast,
                            actual = calendar.Actual,
                            title = calendar.Title,
                            currency = currency,
                            price = Securities[rule.Symbol].Price,
                            meaning = calendar.Meaning.ToString()
                        });
                    }
                }
            }
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

                    if (candleStartTime > (rule.setupTime-l) && (candleStartTime<Math.Max(rule.exitTime, rule.expirationTime )+ le))
                    {
                        rule.Serie.Add(new LocalQuote
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
