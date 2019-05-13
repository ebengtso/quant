using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using Tests;
namespace OandaDataProviderTests
{
    public class DownloaderTests
    {

        private QuantConnect.Lean.Engine.DataFeeds.OandaDataProvider _dataProvider;
        private string filePath;
        private string filePathDaily;
        private string filePathCfdDaily;
        [Test]
        public void DataProvider_CanReadDataThatExists()
        {
            var stream = _dataProvider.Fetch("./20171022_quote.zip");

            Assert.IsNotNull(stream);
        }

        [Test]
        public void DataProvider_Download()
        {
            var symbol = QuantConnect.Symbol.Create("EURUSD", QuantConnect.SecurityType.Forex, Market.Oanda);
            var downloader = new Downloader();
            downloader.Initialize(TestConfiguration.Parameters["token"], TestConfiguration.Parameters["dataPath"]);
            var downloaded = downloader.DownloadData(symbol, Resolution.Minute, new DateTime(2017, 10, 22));

            Assert.IsTrue(downloaded);
            Assert.IsTrue(File.Exists(filePath));
            Assert.IsTrue(FileEquals(filePath, "./20171022_quote.zip"));

        }

        [Test]
        public void DataProvider_DownloadDaily()
        {
            var symbol = QuantConnect.Symbol.Create("USDCHF", QuantConnect.SecurityType.Forex, Market.Oanda);
            var downloader = new Downloader();
            downloader.Initialize(TestConfiguration.Parameters["token"], TestConfiguration.Parameters["dataPath"]);
            var downloaded = downloader.DownloadData(symbol, Resolution.Daily, new DateTime(2004, 1, 1));

            Assert.IsTrue(downloaded);
            Assert.IsTrue(File.Exists(filePathDaily));


        }


        [Test]
        public void DataProvider_DownloadDailyCfd()
        {
            var symbol = QuantConnect.Symbol.Create("BCOUSD", QuantConnect.SecurityType.Cfd, Market.Oanda);
            var downloader = new Downloader();
            downloader.Initialize(TestConfiguration.Parameters["token"], TestConfiguration.Parameters["dataPath"]);
            var downloaded = downloader.DownloadData(symbol, Resolution.Daily, new DateTime(2004, 1, 1));

            Assert.IsTrue(downloaded);
            Assert.IsTrue(File.Exists(filePathCfdDaily));


        }

        private bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        [Test]
        public void DataIsCorrect()
        {
            var date = new DateTime(2017, 10, 22);
            var config = new SubscriptionDataConfig(
                    typeof(QuoteBar),
                    Symbol.Create("OANDA/EURUSD", SecurityType.Forex, Market.Oanda),
                    Resolution.Minute,
                    TimeZones.NewYork,
                    TimeZones.NewYork,
                    true,
                    true,
                    false);
            //var dataCacheProvider = new CustomEphemeralDataCacheProvider { IsDataEphemeral = true };
            var dataProvider = new QuantConnect.Lean.Engine.DataFeeds.OandaDataProvider();
            //dataProvider.Initialize(TestConfiguration.Parameters["token"], TestConfiguration.Parameters["dataPath"]);

            var dataCacheProvider = new SingleEntryDataCacheProvider(dataProvider);
            var reader = new TextSubscriptionDataSourceReader(
                dataCacheProvider,
                config,
                date,
                false);
            Config.Set("oanda-data-access-token", TestConfiguration.Parameters["token"]);
            Config.Set("data-folder", TestConfiguration.Parameters["dataPath"]);
            Globals.Reset();
            var source = (new ForexOandaVolume()).GetSource(config, date, false);

            var dataBars = reader.Read(source);
            decimal[] prices = { 1.176455m, 1.17648m };

            BaseData[] data = dataBars.ToArray();
            Assert.AreEqual(data[0].Price,prices[0]);
            Assert.AreEqual(data[1].Price, prices[1]);


        }

        private class TestTradeBarFactory : QuoteBar
        {
            /// <summary>
            /// Will be true when data is created from a parsed file line
            /// </summary>
            public static bool ReaderWasCalled { get; set; }

            public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
            {
                ReaderWasCalled = true;
                return base.Reader(config, line, date, isLiveMode);
            }
        }

        private class CustomEphemeralDataCacheProvider : IDataCacheProvider
        {
            public string Data { set; get; }
            public bool IsDataEphemeral { set; get; }

            public Stream Fetch(string key)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(Data);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
            public void Store(string key, byte[] data)
            {
            }
            public void Dispose()
            {
            }
        }
        [SetUp]
        public void SetUp()
        {
            filePath = string.Format("{0}/{1}/{2}/{3}/{4}/{5}.zip", TestConfiguration.Parameters["dataPath"], "forex", "oanda", "minute", "eurusd", "20171022_quote");
            filePathDaily = string.Format("{0}/{1}/{2}/{3}/{4}.zip", TestConfiguration.Parameters["dataPath"], "forex", "oanda", "daily", "eurusd");
            filePathCfdDaily = string.Format("{0}/{1}/{2}/{3}/{4}.zip", TestConfiguration.Parameters["dataPath"], "cfd", "oanda", "daily", "bcousd");

            _dataProvider = new QuantConnect.Lean.Engine.DataFeeds.OandaDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
        //    File.Delete(filePath);
        }

    }
}