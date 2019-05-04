# OandaDataProvider

Library to download quotebars and volumes from Oanda

To download volumes a few changes to the QuantConnect source code are necessary

- update the market hours database, because the data retrieved from oanda must be translated from UTC time into exchange hours
    
    check the example file in github. The modifications needed can be found by searching forexvolumedata and cfdvolumedata
    
In the algo, add this:        
    Market.Add("forexvolumedata", 998);
    Market.Add("cfdvolumedata", 999);
    
- change to public the method QuantConnect.Algorithm/QCAlgorithm.Universe.AddToUserDefinedUniverse
- add the method to your algorithm

        /// <summary>
        /// AddData<typeparam name="T"/> a new user defined data source, requiring only the minimum config options.
        /// </summary>
        /// <param name="symbol">Key/Symbol for data</param>
        /// <param name="resolution">Resolution of the Data Required</param>
        /// <param name="market">Specifies the market name</param>
        /// <param name="fillDataForward">When no data available on a tradebar, return the last data that was generated</param>
        /// <param name="leverage">Custom leverage per security</param>
        /// <returns>The new <see cref="Security"/></returns>
        /// <remarks>Generic type T must implement base data</remarks>
        public Security AddData<T>(string symbol, Resolution resolution, string market, bool fillDataForward = false, decimal leverage = 1.0m)
            where T : IBaseData, new()
        {
            //Add this to the data-feed subscriptions
            var symbolObject = new Symbol(SecurityIdentifier.GenerateBase(symbol, market), symbol);

            //Add this new generic data as a tradeable security:
            var config = SubscriptionManager.SubscriptionDataConfigService.Add(typeof(T),
                symbolObject,
                resolution,
                fillDataForward,
                extendedMarketHours: false,
                isCustomData: true);
            var security = Securities.CreateSecurity(symbolObject, config, leverage);
            
            AddToUserDefinedUniverse(security, new List<SubscriptionDataConfig> { config });
            return security;
        }

- add the Symbols using the following
  AddData<CfdOandaVolume>(string.Format("OANDA/{0}", symbol), Resolution.Minute, "cfdvolumedata",false);
  AddData<ForexOandaVolume>(string.Format("OANDA/{0}", symbol), Resolution.Minute, "forexvolumedata",false);
  
- In QuantConnect.Engine TextSubscriptionDataSourceReader.Read add the check on instance==null

...
                    // while the reader has data
                    while (!reader.EndOfStream)
                    {
                        // read a line and pass it to the base data factory
                        var line = reader.ReadLine();
                        BaseData instance = null;
                        try
                        {
                            instance = _factory.Reader(_config, line, _date, _isLiveMode);
                        }
                        catch (Exception err)
                        {
                            OnReaderError(line, err);
                        }
                        
                        //ADD THIS block here
                        if (instance==null)
                        {
                            yield break;
                        }
                        
...                      

- in the algo add:
        public void OnData(OandaVolume data)
        {
            Log("volume " + data.Symbol+ " "+data.Volume + " "+data.Time);

        }