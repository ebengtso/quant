using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlgorithmUtils.Tests
{
    public class TestConfiguration
    {
        public static Dictionary<string, string> Parameters = new Dictionary<string, string>();

        static TestConfiguration()
        {
            foreach (var row in File.ReadAllLines("test.settings"))
                Parameters.Add(row.Split('=')[0], row.Split('=')[1]);
        }

    }
}
