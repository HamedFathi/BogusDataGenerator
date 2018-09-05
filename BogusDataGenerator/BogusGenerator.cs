using Bogus;
using BogusDataGenerator.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BogusDataGenerator
{
    public class BogusGenerator
    {
        private BogusData _bogusData;
        public BogusGenerator()
        {
            _bogusData = new BogusData();
        }
        public BogusGenerator TypeRuleFor<U>(Expression<Func<Faker, U>> setter, string locale = null)
        {
            _bogusData.TypeRules.Add(new Tuple<string, string, string>(typeof(U).ToString(), setter.ToString(), locale));
            return this;
        }
        public BogusGenerator ConditionalPropertyRuleFor<TProperty>(Func<string, bool> condition, Expression<Func<Faker, TProperty>> setter, string locale = null)
        {
            _bogusData.ConditionalPropertyRules.Add(new Tuple<string, Func<string, bool>, string, string, string>(null, condition, null, setter.ToString(), locale));

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
