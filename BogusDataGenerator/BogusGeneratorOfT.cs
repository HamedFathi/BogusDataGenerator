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
        private RuleSet _ruleSet;
        internal List<string> Namespaces { get; private set; }
        internal List<string> Assemblies { get; private set; }
        public BogusGenerator()
        {
            _ruleSet = new RuleSet();
        }

        public BogusGenerator<T> RuleForProperty<TProperty>(Expression<Func<T, TProperty>> property,
            Expression<Func<Faker, T, TProperty>> setter, int repetition = 1)
        {
            _ruleSet.PropertyRules.Add(new PropertyRule()
            {
                Name = property.GetName(),
                TypeName = typeof(T).ToString(),
                PropertyExpressionText = property.ToExpressionString(),
                SetterExpressionText = setter.ToExpressionString(),
                PropertyExpression = property,
                SetterExpression = setter,
                Repetition = typeof(T).IsAcceptableCollection() ? repetition : 1
            });
            return this;
        }

        public BogusGenerator<T> RuleForType<U>(Expression<Func<Faker, U>> setter, int repetition = 1)
        {
            _ruleSet.TypeRules.Add(new TypeRule
            {
                TypeName = typeof(U).ToString(),
                SetterExpressionText = setter.ToExpressionString(),
                SetterExpression = setter,
                Locales = null,
                Repetition = typeof(T).IsAcceptableCollection() ? repetition : 1
            });
            return this;
        }
        public BogusGenerator<T> RuleForConditionalProperty<TProperty>(Func<string, bool> condition, Expression<Func<Faker, T, TProperty>> setter, int repetition = 1)
        {
            var props = typeof(T).GetProperties().Select(x => x.Name).ToList();
            foreach (var prop in props)
            {
                var status = condition(prop);
                if (status)
                {
                    _ruleSet.ConditionalPropertyRules.Add(new ConditionalPropertyRule
                    {
                        TypeName = typeof(T).ToString(),
                        PropertyExpressionText = $"(x) => {prop.ToString()}",
                        SetterExpressionText = setter.ToExpressionString(),
                        SetterExpression = setter,
                        Locales = null,
                        Condition = condition,
                        Repetition = typeof(T).IsAcceptableCollection() ? repetition : 1
                    });
                }
            }
            return this;
        }
        public BogusGenerator<T> StrictMode()
        {
            _ruleSet.IsStrictMode = true;
            return this;
        }

        public BogusGenerator<T> UseLocales(string[] locales = null)
        {
            _ruleSet.Locales = locales;
            return this;
        }

        public BogusGenerator<T> AddRuleSet(params RuleSet[] ruleSet)
        {
            _ruleSet.RuleSets.AddRange(ruleSet);
            return this;
        }

        public RuleSet Store()
        {
            return _ruleSet;
        }

        // Priority:
        // All PropertyRuleFor
        // All ConditionalPropertyRuleFor
        // All TypeRuleFor

        public string Text()
        {
            var sb = new StringBuilder();
            if (_ruleSet.TextBefore.Count > 0)
            {
                sb.AppendLine(_ruleSet.TextBefore.Aggregate((a, b) => a + Environment.NewLine + b));
            }

            sb.Append(BogusCreator(typeof(T)).Source);
            sb.AppendLine(";", 1);


            if (_ruleSet.TextAfter.Count > 0)
            {
                sb.AppendLine(_ruleSet.TextAfter.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return sb.ToString();
        }



        private SourceResult BogusCreator(Type type, string variableName = null, List<string> namespaces = null, List<string> assemblies = null)
        {
            var sb = new StringBuilder();
            if (namespaces == null)
                namespaces = new List<string>();
            if (assemblies == null)
                assemblies = new List<string>();
            var name = variableName ?? type.Name.Camelize();
            var className = type.Name;
            sb.AppendLine($"var {name} = new Faker<{className}>()");
            if (_ruleSet.IsStrictMode)
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
                    var anotherFaker = BogusCreator(innerType.Type, varName, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    sb.Prepend(anotherFaker);
                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate())", 1);
                }

                if (innerType.Status == TypeStatus.Enumerable || innerType.Status == TypeStatus.Collection)
                {
                    var varName = innerType.Name.Camelize();
                    var singularVariableName = varName.Singularize(false);
                    var itemType = innerType.Type.GetGenericArguments()[0];
                    var anotherFaker = BogusCreator(itemType, varName, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    sb.Prepend(anotherFaker);

                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToList())", 1);

                }
                if (innerType.Status == TypeStatus.Array)
                {
                    var varName = innerType.Name.Camelize();
                    var singularVariableName = varName.Singularize(false);
                    var key = typeof(T).FullName + "-" + innerType.Name + "-" + innerType.Type;

                    var elementType = innerType.Type.GetElementType();
                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToArray())", 1);

                }
                else
                {
                    foreach (var propRule in _ruleSet.PropertyRules)
                    {
                        if (innerType.Name == propRule.Name && !processed.Contains(innerType.UniqueId))
                        {
                            sb.AppendLine($".RuleFor({propRule.PropertyExpressionText}, {propRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var rule in _ruleSet.RuleSets)
                    {
                        foreach (var propRule in rule.PropertyRules)
                        {
                            if (innerType.Name == propRule.Name && !processed.Contains(innerType.UniqueId))
                            {
                                sb.AppendLine($".RuleFor({propRule.PropertyExpressionText}, {propRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var conditionalPropRule in _ruleSet.ConditionalPropertyRules)
                    {
                        var status = conditionalPropRule.Condition(innerType.Name);
                        if (status && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
                        {
                            sb.AppendLine($".RuleFor({conditionalPropRule.PropertyExpressionText}, {conditionalPropRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var rule in _ruleSet.RuleSets)
                    {
                        foreach (var conditionalPropRule in rule.ConditionalPropertyRules)
                        {
                            var status = conditionalPropRule.Condition(innerType.Name);
                            if (status && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
                            {
                                var prop = conditionalPropRule.PropertyExpressionText == null ? $"(x) => x.{innerType.Name}" : conditionalPropRule.PropertyExpressionText;


                                sb.AppendLine($".RuleFor({prop}, {conditionalPropRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var typeRule in _ruleSet.TypeRules)
                    {
                        if (!processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(typeRule.Locales))
                        {
                            if (typeRule.TypeName == innerType.TypeName)
                            {
                                sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, {typeRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                        }
                    }
                    foreach (var rule in _ruleSet.RuleSets)
                    {
                        foreach (var typeRule in rule.TypeRules)
                        {
                            if (!processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(typeRule.Locales))
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

        public List<T> AutoFaker(int count = 1, params RuleSet[] ruleSet)
        {
            var name = typeof(T).Name;
            var variableName = name.Camelize();
            var className = $"{name}TestData";
            var bogusGenerator = new BogusGenerator<T>().AddRuleSet(ruleSet);
            var fakerSource = bogusGenerator.Text();
            var assemblies = bogusGenerator.Assemblies.Distinct().ToList();
            assemblies.Add(typeof(Faker<>).Assembly.Location);
            var namespaces = bogusGenerator.Namespaces.Select(x => "using " + x + ";").Aggregate((a, b) => a + Environment.NewLine + b);
            var source = $@"
using System;
using System.Collections.Generic;
using Bogus;
using System.Linq;
using {typeof(T).Namespace};
{namespaces}

public class {className}
{{
	public List<{name}> Get()
	{{
        {fakerSource}
        var result = {variableName}.Generate({count});
        return result;
    }}
}}";
            var errors = new List<string>();
            var type = source.ToType(null, className, out errors, assemblies);

            if (errors == null)
            {
                var testData = (List<T>)type.GetMethod("Get").Invoke(Activator.CreateInstance(type), null);
                return testData;
            }

            return null;
        }
    }
}
