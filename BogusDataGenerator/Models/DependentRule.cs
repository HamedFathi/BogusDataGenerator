namespace BogusDataGenerator.Models
{
    public class DependentRule
    {
        public string PropertyName { get; set; }
        public string VariableName { get; set; }
        public string UsedVariableName { get; set; }

        public int Repetition { get; set; }
        public RuleSet RuleSet { get; set; }
    }
}
