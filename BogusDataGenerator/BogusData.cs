using System;
using System.Collections.Generic;

namespace BogusDataGenerator
{
    public class BogusData
    {
        public BogusData()
        {
            ConditionalPropertyRules = new List<Tuple<string, Func<string, bool>, string, string, string>>();
            PropertyRules = new List<Tuple<string, string, string, string>>();
            TypeRules = new List<Tuple<string, string, string>>();
            TextBefore = new List<string>();
            TextAfter = new List<string>();
            PredefinedRules = new List<BogusData>();
        }


        public bool IsStrictMode { get; set; } = false;
        public List<Tuple<string, string, string, string>> PropertyRules { get; set; }
        public List<Tuple<string, Func<string, bool>, string, string, string>> ConditionalPropertyRules { get; set; }

        public List<Tuple<string, string, string>> TypeRules { get; set; }
        public List<string> TextBefore { get; set; }
        public List<string> TextAfter { get; set; }

        public List<BogusData> PredefinedRules { get; set; }

        public string Locale { get; set; } = null;

    }
}
