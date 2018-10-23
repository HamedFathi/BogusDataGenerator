using System.Collections.Generic;

namespace BogusDataGenerator.Models
{

    public class RuleSet
    {
        public RuleSet()
        {
            PropertyRules = new List<PropertyRule>();
            ConditionalPropertyRules = new List<ConditionalPropertyRule>();
            TypeRules = new List<TypeRule>();
            TextBefore = new List<string>();
            TextAfter = new List<string>();
            RuleSets = new List<RuleSet>();
            DependentRules = new List<DependentRule>();
        }

        public string VariableName { get; set; }
        public bool IsStrictMode { get; set; } = false;
        public List<PropertyRule> PropertyRules { get; set; }

        public List<DependentRule> DependentRules { get; set; }
        public List<ConditionalPropertyRule> ConditionalPropertyRules { get; set; }
        public List<TypeRule> TypeRules { get; set; }
        public List<string> TextBefore { get; set; }
        public List<string> TextAfter { get; set; }
        public List<RuleSet> RuleSets { get; set; }
        public string[] Locales { get; set; } = null;

    }
}
