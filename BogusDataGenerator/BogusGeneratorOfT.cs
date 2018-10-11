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
    public class SourceResult
    {
        public string Source { get; set; }
        public List<string> Namespaces { get; set; }
        public List<string> Assemblies { get; set; }

    }
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
            _bogusData.PropertyRules.Add(new PropertyRule()
            {
                Name = property.GetName(),
                TypeName = typeof(T).ToString(),
                PropertyExpressionText = property.ToExpressionString(),
                SetterExpressionText = setter.ToExpressionString(),
                PropertyExpression = property,
                SetterExpression = setter
            });
            return this;
        }

        public BogusGenerator<T> TypeRuleFor<U>(Expression<Func<Faker, U>> setter)
        {
            _bogusData.TypeRules.Add(new TypeRule
            {
                TypeName = typeof(U).ToString(),
                SetterExpressionText = setter.ToExpressionString(),
                SetterExpression = setter,
                Locales = null
            });
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
                    _bogusData.ConditionalPropertyRules.Add(new ConditionalPropertyRule
                    {
                        TypeName = typeof(T).ToString(),
                        PropertyExpressionText = $"(x) => {prop.ToString()}",
                        SetterExpressionText = setter.ToExpressionString(),
                        SetterExpression = setter,
                        Locales = null,
                        Condition = condition
                    });
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
        /*public BogusGenerator<T> AddText(string text, ExtraTextType appendTextType = ExtraTextType.Before)
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
        }*/

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

        // Priority:
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

            sb.Append(BogusCreator(typeof(T)).Source);
            sb.AppendLine(";", 1);


            if (_bogusData.TextAfter.Count > 0)
            {
                sb.AppendLine(_bogusData.TextAfter.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return sb.ToString();
        }

        internal List<string> Namespaces { get; private set; }
        public List<string> Assemblies { get; private set; }

        private SourceResult BogusCreator(Type type, string variableName = null, List<string> variables = null, List<string> namespaces = null, List<string> assemblies = null)
        {
            var sb = new StringBuilder();
            if (variables == null)
                variables = new List<string>();
            if (namespaces == null)
                namespaces = new List<string>();
            if (assemblies == null)
                assemblies = new List<string>();
            var name = variableName ?? type.Name.Camelize();
            var className = type.Name;
            sb.AppendLine($"var {name} = new Faker<{className}>()");
            if (_bogusData.IsStrictMode)
                sb.AppendLine(".StrictMode(true)", 1);
            else
                sb.AppendLine(".StrictMode(false)", 1);

            var processed = new List<string>();
            var innerTypes = type.GetInnerTypes().Where(x => x.Parent != null && x.Parent == type.FullName).ToList();
            namespaces.AddRange(innerTypes.Select(s => s.Namespace));
            assemblies.AddRange(innerTypes.Select(s => s.Location));

            foreach (var innerType in innerTypes)
            {

                if (innerType.Status == TypeStatus.Class)
                {
                    var varName = innerType.Name.Camelize();
                    variables.Add(varName);
                    var anotherFaker = BogusCreator(innerType.Type, varName, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    sb.Prepend(anotherFaker);
                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate())", 1);
                }

                if (innerType.Status == TypeStatus.Enumerable || innerType.Status == TypeStatus.Collection)
                {
                    var varName = innerType.Name.Camelize();
                    var singularVariableName = varName.Singularize(false);
                    if (variables.Contains(singularVariableName))
                    {
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {singularVariableName}.Generate(100).ToList())", 1);
                    }
                    else
                    {
                        variables.Add(varName);
                        var itemType = innerType.Type.GetGenericArguments()[0];
                        var anotherFaker = BogusCreator(itemType, varName, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                        sb.Prepend(anotherFaker);

                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToList())", 1);
                    }
                }
                if (innerType.Status == TypeStatus.Array)
                {
                    var varName = innerType.Name.Camelize();
                    var singularVariableName = varName.Singularize(false);
                    if (variables.Contains(singularVariableName))
                    {
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {singularVariableName}.Generate(100).ToArray())", 1);
                    }
                    else
                    {
                        variables.Add(varName);
                        var elementType = innerType.Type.GetElementType();
                        var anotherFaker = BogusCreator(elementType, varName, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                        sb.Prepend(anotherFaker);
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToArray())", 1);
                    }
                }
                else
                {
                    foreach (var propRule in _bogusData.PropertyRules)
                    {
                        if (innerType.Name == propRule.Name && !processed.Contains(innerType.UniqueId))
                        {
                            sb.AppendLine($".RuleFor({propRule.PropertyExpressionText}, {propRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var predefinedRules in _bogusData.PredefinedRules)
                    {
                        foreach (var propRule in predefinedRules.PropertyRules)
                        {
                            if (innerType.Name == propRule.Name && !processed.Contains(innerType.UniqueId))
                            {
                                sb.AppendLine($".RuleFor({propRule.PropertyExpressionText}, {propRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var conditionalPropRule in _bogusData.ConditionalPropertyRules)
                    {
                        var status = conditionalPropRule.Condition(innerType.Name);
                        if (status && !processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(conditionalPropRule.Locales))
                        {
                            sb.AppendLine($".RuleFor({conditionalPropRule.PropertyExpressionText}, {conditionalPropRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var predefinedRules in _bogusData.PredefinedRules)
                    {
                        foreach (var conditionalPropRule in predefinedRules.ConditionalPropertyRules)
                        {
                            var status = conditionalPropRule.Condition(innerType.Name);
                            if (status && !processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(conditionalPropRule.Locales))
                            {
                                var prop = conditionalPropRule.PropertyExpressionText == null ? $"(x) => x.{innerType.Name}" : conditionalPropRule.PropertyExpressionText;


                                sb.AppendLine($".RuleFor({prop}, {conditionalPropRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var typeRule in _bogusData.TypeRules)
                    {
                        if (!processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(typeRule.Locales))
                        {
                            if (typeRule.TypeName == innerType.TypeName)
                            {
                                sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, {typeRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var predefinedRules in _bogusData.PredefinedRules)
                    {
                        foreach (var typeRule in predefinedRules.TypeRules)
                        {
                            if (!processed.Contains(innerType.UniqueId) && _bogusData.Locales.ContainsOneOf(typeRule.Locales))
                            {
                                if (typeRule.TypeName == innerType.TypeName)
                                {
                                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, {typeRule.SetterExpressionText})", 1);
                                    processed.Add(innerType.UniqueId);
                                }
                            }
                        }
                    }
                }
            }
            Namespaces = namespaces;
            Assemblies = assemblies;
            return new SourceResult() { Source = sb.ToString(), Namespaces = namespaces, Assemblies = assemblies };
        }


    }
}
