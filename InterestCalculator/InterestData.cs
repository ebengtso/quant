using System;
using System.Collections.Generic;
using System.Globalization;
using QuantConnect;
using QuantConnect.Data;

namespace InterestCalculator
{

    /// <summary>
    /// Interest Custom Data Class
    /// </summary>
    public class InterestData : BaseData
    {
        /// <summary>
        /// Bid Rate
        /// </summary>
        public decimal Bid;
        /// <summary>
        /// Ask Rate
        /// </summary>
        public decimal Ask;
        /// <summary>
        /// Instrument
        /// </summary>
        public string Instrument;
        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime DateTime;

        public string InterestSymbol;

        public static string URL = "http://127.0.0.1:8080/interest.csv";
        /// <summary>
        /// Default initializer for Interest.
        /// </summary>
        public InterestData()
        {
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(URL, SubscriptionTransportMedium.RemoteFile);
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object
        /// each time it is called.
        /// </summary>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            //New Interest object
            var interest = new InterestData();

            try
            {
                //Example File Format:
                //CURRENCY,INSTRUMENT,BID,ASK,TEXTDATE,DATE,HOUR
                //Australia 200,AU200,4.1300,6.6800,Wed Dec 19 19:09:25 2018,20181219,19:09:25
                //Australia 200,AU200,1.0500,2.0000,Wed Dec 19 07:04:29 2018,20181207,07:04:29
                var data = line.Split(';');
                interest.Symbol = config.Symbol;
                interest.InterestSymbol = data[1];
                interest.Bid = Convert.ToDecimal(data[2], CultureInfo.InvariantCulture);
                interest.Ask = Convert.ToDecimal(data[3], CultureInfo.InvariantCulture);
                    interest.DateTime = System.DateTime.ParseExact(string.Format("{0} {1} GMT", data[5], data[6]), "yyyyMMdd HH:mm:ss Z", CultureInfo.InvariantCulture);

                interest.Time = System.DateTime.ParseExact(string.Format("{0} {1} GMT", data[5], data[6]), "yyyyMMdd HH:mm:ss Z", CultureInfo.InvariantCulture);
                //interest.Time = System.DateTime.ParseExact(string.Format("{0} GMT", data[5]), "yyyyMMdd Z", CultureInfo.InvariantCulture);
                interest.Value = 1m;
            }
            catch
            {
            }
            return interest;
        }
    }

}
