using Bogus;
using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using BogusDataGenerator.Models;
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

            var propExp = property.ToExpressionString();
            var setterExp = setter.ToExpressionString();
            _bogusData.PropertyRules.Add(new Tuple<string, string, string, string>(typeof(T).ToString(), property.GetName(), propExp, setterExp));
            return this;
        }

        public BogusGenerator<T> TypeRuleFor<U>(Expression<Func<Faker, U>> setter)
        {
            var setterExp = setter.ToExpressionString();
            _bogusData.TypeRules.Add(new Tuple<string, string, string[]>(typeof(U).ToString(), setterExp, null));
            return this;
        }
        public BogusGenerator<T> ConditionalPropertyRuleFor<TProperty>(Func<string, bool> condition, Expression<Func<Faker, T, TProperty>> setter)
        {
            var setterExp = setter.ToExpressionString();
            var props = typeof(T).GetProperties().Select(x => x.Name).ToList();
            foreach (var prop in props)
            {
                var status = condition(prop);
                if (status)
                {
                    _bogusData.ConditionalPropertyRules.Add(new Tuple<string, Func<string, bool>, string, string, string[]>(typeof(T).ToString(), condition, $"(z) => {prop.ToString()}", setterExp, null));
                }
            }
            return this;
        }
        public BogusGenerator<T> StrictMode()
        {
            _bogusData.IsStrictMode = true;
            return this;
        }

        public BogusGenerator<T> UseLocales(params string[] locales)
        {
            _bogusData.Locales = locales;
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
        // All PropertyRuleFor
        // All ConditionalPropertyRuleFor
        // All TypeRuleFor

        public string Create()
        {
            var sb = new StringBuilder();
            if (_bogusData.TextBefore.Count > 0)
            {
                sb.AppendLine(_bogusData.TextBefore.Aggregate((a, b) => a + Environment.NewLine + b));
            }

            sb.Append(BogusCreator(typeof(T)));
            sb.AppendLine(";", 1);


            if (_bogusData.TextAfter.Count > 0)
            {
                sb.AppendLine(_bogusData.TextAfter.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return sb.ToString();
        }



        private string BogusCreator(Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name.Camelize();
            var className = type.Name;
            sb.AppendLine($"var {name} = new Faker<{className}>()");
            if (_bogusData.IsStrictMode)
                sb.AppendLine(".StrictMode(true)", 1);
            else
                sb.AppendLine(".StrictMode(false)", 1);

            var processed = new List<string>();
            var innerTypes = type.GetInnerTypes().Where(x => x.Parent != null).ToList();
            foreach (var innerType in innerTypes)
            {

                if (innerType.Status == TypeStatus.Class)
                {
                    var variableName = innerType.Name.Camelize();
                    var anotherFaker = BogusCreator(innerType.Type) + new string('\t', 1) + ";" + Environment.NewLine;
                    sb.Prepend(anotherFaker);
                    sb.AppendLine($".RuleFor((z) => z.{innerType.Name}, (f) => {variableName}.Generate(3).ToList())",1);

                }
                else
                {
                    foreach (var propRule in _bogusData.PropertyRules)
                    {
                        if (innerType.Name == propRule.Item2 && !processed.Contains(innerType.UniqueId))
                        {
                            sb.AppendLine($".RuleFor({propRule.Item3}, {propRule.Item4})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var conditionalPropRule in _bogusData.ConditionalPropertyRules)
                    {
                        var status = conditionalPropRule.Item2(innerType.Name);
                        if (status && !processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(conditionalPropRule.Item5))
                        {
                            sb.AppendLine($".RuleFor({conditionalPropRule.Item3}, {conditionalPropRule.Item4})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var predefinedRules in _bogusData.PredefinedRules)
                    {
                        foreach (var conditionalPropRule in predefinedRules.ConditionalPropertyRules)
                        {
                            var status = conditionalPropRule.Item2(innerType.Name);
                            if (status && !processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(conditionalPropRule.Item5))
                            {
                                var prop = conditionalPropRule.Item3 == null ? $"(x) => x.{innerType.Name}" : conditionalPropRule.Item3;


                                sb.AppendLine($".RuleFor({prop}, {conditionalPropRule.Item4})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var typeRule in _bogusData.TypeRules)
                    {
                        if (!processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(typeRule.Item3))
                        {
                            if (typeRule.Item1 == innerType.TypeName)
                            {
                                sb.AppendLine($".RuleFor((z) => z.{innerType.Name}, {typeRule.Item2})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var predefinedRules in _bogusData.PredefinedRules)
                    {
                        foreach (var typeRule in predefinedRules.TypeRules)
                        {
                            if (!processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(typeRule.Item3))
                            {
                                if (typeRule.Item1 == innerType.TypeName)
                                {
                                    sb.AppendLine($".RuleFor((z) => z.{innerType.Name}, {typeRule.Item2})", 1);
                                    processed.Add(innerType.UniqueId);
                                }
                            }
                        }
                    }
                }




            }

            return sb.ToString();
        }


    }
}
