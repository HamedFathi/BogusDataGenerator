using Bogus;
using BogusDataGenerator.Enums;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BogusDataGenerator
{
    public class BogusGenerator<T> where T : class, new()
    {
        private BogusData _bogusData;

        public BogusGenerator()
        {
            _bogusData = new BogusData();
        }
        public BogusGenerator<T> PropertyRuleFor<TProperty>(Expression<Func<T, TProperty>> property,
            Expression<Func<Faker, T, TProperty>> setter)
        {
            _bogusData.PropertyRules.Add(new Tuple<string, string, string, string>(typeof(T).ToString(), property.GetName(), property.ToString(), setter.ToString()));

            return this;
        }

        public BogusGenerator<T> TypeRuleFor<U>(Expression<Func<Faker, U>> setter)
        {
            _bogusData.TypeRules.Add(new Tuple<string, string, string>(typeof(U).ToString(), setter.ToString(), null));
            return this;
        }
        public BogusGenerator<T> ConditionalPropertyRuleFor<TProperty>(Func<string, bool> condition, Expression<Func<Faker, T, TProperty>> setter)
        {
            var props = typeof(T).GetProperties().Select(x => x.Name).ToList();
            foreach (var prop in props)
            {
                var status = condition(prop);
                if (status)
                {
                    _bogusData.ConditionalPropertyRules.Add(new Tuple<string, Func<string, bool>, string, string, string>(typeof(T).ToString(), condition, prop.ToString(), setter.ToString(), null));
                }
            }
            return this;
        }
        public BogusGenerator<T> StrictMode()
        {
            _bogusData.IsStrictMode = true;
            return this;
        }

        public BogusGenerator<T> UseLocale(string locale)
        {
            _bogusData.Locale = locale;
            return this;
        }
        public BogusGenerator<T> AddText(string text, ExtraTextType appendTextType = ExtraTextType.Before)
        {
            if (appendTextType == ExtraTextType.Before)
            {
                _bogusData.TextBefore.Add(text);
            }
            else
            {
                _bogusData.TextAfter.Add(text);
            }
            return this;
        }

        public BogusGenerator<T> AddPredefinedRule(BogusData bogusData)
        {
            _bogusData.PredefinedRules.Add(bogusData);
            return this;
        }
        public BogusGenerator<T> AddPredefinedRules(params BogusData[] bogusData)
        {
            _bogusData.PredefinedRules.AddRange(bogusData);
            return this;
        }

        public BogusData Store()
        {
            return _bogusData;
        }

        // Priority
        // Own PropertyRuleFor
        // Own ConditionalPropertyRuleFor
        // Own TypeRuleFor
        // PredefinedRule
        //      Its PropertyRuleFor
        //      Its ConditionalPropertyRuleFor
        //      Its TypeRuleFor
        // Next PredefinedRule
        //      Its PropertyRuleFor
        //      Its ConditionalPropertyRuleFor
        //      Its TypeRuleFor
        public string Create()
        {
            var sb = new StringBuilder();
            if (_bogusData.TextBefore.Count > 0)
            {
                sb.AppendLine(_bogusData.TextBefore.Aggregate((a, b) => a + Environment.NewLine + b));
            }

            var name = typeof(T).Name.Camelize();
            var className = typeof(T).Name;
            sb.AppendLine($"var {name} = new Faker<{className}>()");
            if (_bogusData.IsStrictMode)
                sb.AppendLine(".StrictMode(true)", 1);
            else
                sb.AppendLine(".StrictMode(false)", 1);


            var innerTypes = typeof(T).GetInnerTypes();
            foreach (var type in innerTypes)
            {
                foreach (var propRule in _bogusData.PropertyRules)
                {

                }
                foreach (var conditionalPropRule in _bogusData.ConditionalPropertyRules)
                {

                }
                foreach (var typeRule in _bogusData.TypeRules)
                {

                }
                foreach (var predefinedRules in _bogusData.PredefinedRules)
                {
                    foreach (var propRule in predefinedRules.PropertyRules)
                    {

                    }
                    foreach (var conditionalPropRule in predefinedRules.ConditionalPropertyRules)
                    {

                    }
                    foreach (var typeRule in predefinedRules.TypeRules)
                    {

                    }
                }
            }

            if (_bogusData.TextAfter.Count > 0)
            {
                sb.AppendLine(_bogusData.TextAfter.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return sb.ToString();
        }


    }
}
