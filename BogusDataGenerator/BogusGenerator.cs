using Bogus;
using BogusDataGenerator.Extensions;
using BogusDataGenerator.Models;
using System;
using System.Linq.Expressions;

namespace BogusDataGenerator
{
    public class BogusGenerator
    {
        private BogusData _bogusData;
        public BogusGenerator()
        {
            _bogusData = new BogusData();
        }
        public BogusGenerator TypeRuleFor<U>(Expression<Func<Faker, U>> setter, params string[] locales)
        {
            var setterExp = setter.ToExpressionString();
            _bogusData.TypeRules.Add(new TypeRule
            {
                TypeName = typeof(U).ToString(),
                SetterExpression = setter.ToExpressionString(),
                Locales = null
            });
            return this;
        }
        public BogusGenerator ConditionalPropertyRuleFor<TProperty>(Func<string, bool> condition, Expression<Func<Faker, TProperty>> setter, params string[] locales)
        {
            var setterExp = setter.ToExpressionString(); ;
            _bogusData.ConditionalPropertyRules.Add(new ConditionalPropertyRule
            {
                TypeName = null,
                PropertyExpression = null,
                SetterExpression = setter.ToExpressionString(),
                Locales = locales,
                Condition = condition
            });
            return this;
        }

        public BogusGenerator AddPredefinedRule(BogusData bogusData)
        {
            _bogusData.PredefinedRules.Add(bogusData);
            return this;
        }
        public BogusGenerator AddPredefinedRules(params BogusData[] bogusData)
        {
            _bogusData.PredefinedRules.AddRange(bogusData);
            return this;
        }

        public BogusData Store()
        {
            return _bogusData;
        }
    }
}
