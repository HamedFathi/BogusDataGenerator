using System.Linq.Expressions;

namespace BogusDataGenerator.Models
{
    public class TypeRule
    {
        public string TypeName { get; set; }
        public string SetterExpressionText { get; set; }
        public LambdaExpression SetterExpression { get; set; }
        public string[] Locales { get; set; }
        public int Repetition { get; set; }

    }
}
