using Bogus;
using BogusDataGenerator.Extensions;
using BogusDataGenerator.Models;
using System;
using System.Linq.Expressions;

namespace BogusDataGenerator
{
    public class BogusGenerator
    {
        private RuleSet _ruleSet;
        public BogusGenerator()
        {
            _ruleSet = new RuleSet();
        }

        public BogusGenerator RuleForType<T>(Expression<Func<Faker, T>> setter, string[] locales = null, int repetition = 1)
        {
            var setterExp = setter.ToExpressionString();
            _ruleSet.TypeRules.Add(new TypeRule
            {
                TypeName = typeof(T).ToString(),
                SetterExpression = setter,
                SetterExpressionText = setter.ToExpressionString(),
                Locales = null,
                Repetition = typeof(T).IsAcceptableCollection() ? repetition : 1
            });
            return this;
        }
        public BogusGenerator RuleForConditionalProperty<TProperty>(Func<string, bool> condition, Expression<Func<Faker, TProperty>> setter, string[] locales = null, int repetition = 1)
        {
            var setterExp = setter.ToExpressionString(); ;
            _ruleSet.ConditionalPropertyRules.Add(new ConditionalPropertyRule
            {
                TypeName = null,
                PropertyExpressionText = null,
                PropertyExpression = null,
                SetterExpression = setter,
                SetterExpressionText = setter.ToExpressionString(),
                Locales = locales,
                Condition = condition,
                Repetition = repetition
            });
            return this;
        }

        public BogusGenerator AddRuleSet(RuleSet ruleSet)
        {
            _ruleSet.RuleSets.Add(ruleSet);
            return this;
        }
        public BogusGenerator AddRuleSets(params RuleSet[] ruleSet)
        {
            _ruleSet.RuleSets.AddRange(ruleSet);
            return this;
        }



        public RuleSet Save()
        {
            return _ruleSet;
        }
    }
}
