using System.Linq.Expressions;

namespace BogusDataGenerator.Models
{
    public class PropertyRule
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string PropertyExpressionText { get; set; }
        public string SetterExpressionText { get; set; }
        public LambdaExpression PropertyExpression { get; set; }
        public LambdaExpression SetterExpression { get; set; }
    }
}
