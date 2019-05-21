using System;
using System.Collections.Generic;
using QuantConnect.Securities;

namespace InterestCalculator
{
    public class Calculator
    {
        public Dictionary<string, InterestData> interestList = new Dictionary<string, InterestData>();

        public Calculator()
        {
        }

        //For a long position:
        //Financing Charges on Base = units * ({BASE}Interest Rate %) * (Time in years) * ({BASE}/Primary Currency)
        //Financing Charges on Quote = (converted units) * ({QUOTE} Interest Rate %) * (Time in years) * ({QUOTE}/Primary Currency)
        //Total Financing = Financing Charges on Base - Financing Charges on Quote
        //For a short position:
        //Financing Charges on Quote = (converted units) * ({QUOTE} Interest Rate %) * (Time in years) * ({QUOTE}/Primary Currency)
        //Financing Charges on Base = units * ({BASE} Interest Rate %) * (Time in years) * ({BASE}/Primary Currency)
        //Total Interest = Financing Charges on Quote - Financing Charges on Base

        public decimal CalculateInterest(Security security, decimal accountConversionRate)
        {
                    var units = security.Holdings.AbsoluteQuantity;
                    InterestData quotesymbol;
                    InterestData basesymbol;

                    interestList.TryGetValue(security.QuoteCurrency.Symbol, out quotesymbol);
                    interestList.TryGetValue(security.Symbol.ID.Symbol.Replace(security.QuoteCurrency.Symbol, ""), out basesymbol);
                    if (basesymbol == null)
                    {
                throw new Exception("base symbol " + security.QuoteCurrency.Symbol);
                    }
            if (quotesymbol == null)
            {
                throw new Exception("quotesymbol symbol " + security.Symbol.ID.Symbol.Replace(security.QuoteCurrency.Symbol, ""));
            }
            if (security.Holdings.Quantity > 0) //long
                    {

                        var baseInterest = basesymbol.Bid; //borrow
                        var yearsecs = 365 * 24 * 60 * 60m;
                        var timeyears = (decimal)security.Exchange.Hours.RegularMarketDuration.TotalSeconds / yearsecs;
                        var tradePrice = security.Price;
                        //var accountCurrency = accountConversionRate;// /Portfolio.Securities["EURUSD"].Price; //convert usd to eur
                        var quoteInterest = quotesymbol.Ask; //lend
                        var value = units * timeyears * (baseInterest / 100) * tradePrice * accountConversionRate;
                        var v2 = units * timeyears * (quoteInterest / 100) * tradePrice * accountConversionRate;
                        var result = value - v2;
                        return result;
                       //Debug(string.Format("{0} {1} {2} {3:N2}", basesymbol.Bid, quotesymbol.Ask, Time, result));
                        //Portfolio.CashBook["USD"].AddAmount(result);

                        //security.Holdings.AddNewFee(result);
                        //Debug(string.Format("{0:N2}", Portfolio.CashBook["EUR"].Amount));
                    }
                    else //short
                    {
                        var baseInterest = basesymbol.Ask; //lend
                        var yearsecs = 365 * 24 * 60 * 60m;
                        var timeyears = (decimal)security.Exchange.Hours.RegularMarketDuration.TotalSeconds / yearsecs;
                        var tradePrice = security.Price;
                        //var accountCurrency = accountConversionRate;// / Portfolio.Securities["EURUSD"].Price; //convert usd to eur
                        var quoteInterest = quotesymbol.Bid; //borrow
                        var value = units * timeyears * (baseInterest / 100) * tradePrice * accountConversionRate;
                        var v2 = units * timeyears * (quoteInterest / 100) * tradePrice * accountConversionRate;
                        var result = v2 - value;
                        return result;
                        //Debug(string.Format("{0} {1} {2} {3:N2}", basesymbol.Ask, quotesymbol.Bid, Time, result));
                        //Portfolio.CashBook["USD"].AddAmount(result);
                        //security.Holdings.AddNewFee(result);
                        //Debug(string.Format("{0:N2}", Portfolio.CashBook["EUR"].Amount));
                    }
        }

    }
}
