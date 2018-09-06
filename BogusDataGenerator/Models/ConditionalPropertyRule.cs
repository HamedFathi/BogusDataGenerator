using System;

namespace BogusDataGenerator.Models
{
    public class ConditionalPropertyRule
    {
        public string TypeName { get; set; }
        public string PropertyExpression { get; set; }
        public string SetterExpression { get; set; }
        public Func<string, bool> Condition { get; set; }
        public string[] Locales { get; set; }

    }
}
