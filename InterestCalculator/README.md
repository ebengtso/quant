calculate overnight interests

How to:
        public override void Initialize()
        {
            if (!LiveMode)
            {
                AddData<InterestData>("DUMMY", Resolution.Daily);
            }
            var history = History<InterestData>(TimeSpan.FromDays(30*12),Resolution.Daily);
            foreach (var data in history)
            {
                foreach (var d in data.Values)
                {
                    if (calculator.interestList.ContainsKey(d.InterestSymbol))
                    {
                        calculator.interestList.Remove(d.InterestSymbol);
                    }
                    calculator.interestList.Add(d.InterestSymbol, d);

                }
            }
        }
        
        public void OnData(InterestData data)
        {
            try
            {
                if (calculator.interestList.ContainsKey(data.InterestSymbol))
                {
                    calculator.interestList.Remove(data.InterestSymbol);
                }
                calculator.interestList.Add(data.InterestSymbol, data);

                //Debug(string.Format("{0} {1} {2} {3} {4}", data.Symbol.ID.Symbol, data.Bid, data.Ask, data.DateTime, Time));
            }
            catch (Exception err)
            {
                Debug("Error: " + err.Message);
            }
        }

        public override void OnEndOfDay()
        {
            foreach (var security in Portfolio.Securities.Values)
            {

                if (security.Invested)
                {
                    try
                    {
                        var amount = calculator.CalculateInterest(security, security.QuoteCurrency.ConversionRate);
                    Log(string.Format("Interest symbol {0} amount {1} time {2}", security.Symbol, amount, UtcTime));
                    Portfolio.CashBook[Portfolio.CashBook.AccountCurrency].AddAmount(amount);

                    }

                    catch(Exception e) { Log(e.Message); }
                }
            }
        }
