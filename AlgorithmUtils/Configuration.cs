using System.Collections.Generic;

namespace AlgorithmUtils.Utilities.Configuration
{

    public static class Configuration
    {


        public static string[] forexsymbols = TradingSymbols.OandaFXMajors2;
        public static string[] cfdsymbols = TradingSymbols.OandaCFDSelection; //OandaCFDSelection;
        //liquidity provided by oanda
        public static Dictionary<string, int> maximumOrderSize = new Dictionary<string, int>()
                                                            {
                                                                {"NATGASUSD",100000},
                                                                {"JP225USD",75},
                                                                {"NAS100USD", 200},
                                                                {"DE30EUR", 200},
                                                                {"UK100GBP", 200},
                                                                {"WHEATUSD", 100000},
                                                                {"SUGARUSD", 2000000},
                                                                {"SOYBNUSD", 60000},
                                                                {"WTICOUSD", 30000},
                                                                {"BCOUSD", 25000},
                                                                {"XAUAUD", 2000},
                                                                {"XAUUSD", 5000},
                                                                {"XAUHKD", 2000},
                                                                {"XAUGBP", 2000},
                                                                {"XAGGBP", 100000},
                                                                {"XAUSGD", 2000},
                                                                {"XAGUSD", 100000},
                                                                {"XAGAUD", 100000},
                                                                {"XAGHKD", 100000},
                                                                {"XAGSGD", 100000},
                                                                {"FR40EUR", 100},
                                                                {"EU50EUR", 300},
                                                                {"AU200AUD", 125},
                                                                {"SPX500USD", 1000},
                                                                {"US30USD", 200},
                                                                {"DE10YBEUR", 15000},
                                                                {"US2000USD", 500},
                                                                {"XCUUSD", 150000},
                                                                {"CORNUSD",150000},
                                                                {"HK33HKD",200},
                                                                {"NL25EUR",1200},
                                                                {"SG30SGD",100},
                                                                {"UK10YBGBP",10000},
                                                                {"USB02YUSD",20000},
                                                                {"USB05YUSD",20000},
                                                                {"USB10YUSD",20000},
                                                                {"USB30YUSD",20000},
                                                                {"XAGCAD",100000},
                                                                {"XAGCHF",100000},
                                                                {"XAGEUR",100000},
                                                                {"XAGJPY",100000},
                                                                {"XAGNZD",100000},
                                                                {"XAUCAD",100000},
                                                                {"XAUCHF",2000},
                                                                {"XAUEUR",2000},
                                                                {"XAUJPY",2000},
                                                                {"XAUNZD",2000},
                                                                {"XAUXAG",2000},
                                                                {"XPDUSD",500},
                                                                {"XPTUSD",500}


                                                            };
    }
}