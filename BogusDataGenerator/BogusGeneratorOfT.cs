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

    public static class FakerExtensions
    {
        public static BogusGeneratorVariable UseBogusGenerator(this Faker faker, Type type)
        {
            return new BogusGeneratorVariable { VariableName = type.Name.Camelize() };
        }
    }
    public class BogusGenerator<T>
        where T : class, new()
    {
        private RuleSet _ruleSet;
        private Expression<Func<Faker, T>> _customInstantiator = null;
        internal List<string> Namespaces { get; private set; }
        internal List<string> Assemblies { get; private set; }

        public BogusGenerator()
        {
            _ruleSet = new RuleSet
            {
                VariableName = typeof(T).Name.Camelize(),
                Type = typeof(T)
            };
        }

        public BogusGenerator<T> CustomInstantiator(Expression<Func<Faker, T>> factoryMethod)
        {
            _customInstantiator = factoryMethod;
            return this;
        }

        public BogusGenerator<T> RuleForProperty<TProperty>(Expression<Func<T, TProperty>> property,
            Expression<Func<Faker, T, TProperty>> setter)
        {
            _ruleSet.PropertyRules.Add(new PropertyRule()
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
        public BogusGenerator<T> RuleForProperty<TProperty>(Expression<Func<T, TProperty>> property, Func<Faker, BogusGeneratorVariable> relatedBogusGenerator, int repetition = 1)
        {
            _ruleSet.DependentRules.Add(new DependentRule()
            {
                PropertyName = property.GetName(),
                Repetition = repetition,
                UsedVariableName = relatedBogusGenerator(new Faker()).VariableName,
                VariableName = property.GetName().Camelize(),
                // RuleSet = ruleSet
            });
            /*if (!_ruleSet.RuleSets.Contains(ruleSet))
            {
                _ruleSet.RuleSets.Add(ruleSet);
            }*/
            return this;
        }
        public BogusGenerator<T> RuleForType<U>(Expression<Func<Faker, U>> setter)
        {
            _ruleSet.TypeRules.Add(new TypeRule
            {
                TypeName = typeof(U).ToString(),
                SetterExpressionText = setter.ToExpressionString(),
                SetterExpression = setter,
                Locales = null
            });
            return this;
        }
        public BogusGenerator<T> RuleForConditionalProperty<TProperty>(Func<string, bool> condition, Expression<Func<Faker, T, TProperty>> setter)
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
                        Condition = condition
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

        public BogusGenerator<T> UseLocales(params string[] locales)
        {
            _ruleSet.Locales = locales;
            return this;
        }

        public BogusGenerator<T> AddRuleSet(params RuleSet[] ruleSet)
        {
            foreach (var rule in ruleSet)
            {
                if (!_ruleSet.RuleSets.Contains(rule))
                {
                    _ruleSet.RuleSets.Add(rule);
                }
            }
            return this;
        }

        public RuleSet Save()
        {
            return _ruleSet;
        }

        // Priority:
        // All PropertyRuleFor
        // All ConditionalPropertyRuleFor
        // All TypeRuleFor

        public override string ToString()
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

        internal SourceResult BogusCreator(Type type, string variableName = null, List<string> processedTypes = null, List<string> variables = null, List<string> namespaces = null, List<string> assemblies = null)
        {
            var sb = new StringBuilder();
            if (variables == null)
                variables = new List<string>();
            if (processedTypes == null)
                processedTypes = new List<string>();
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
            if (_customInstantiator != null)
            {
                sb.AppendLine($".CustomInstantiator({_customInstantiator.ToExpressionString()})", 1);
            }
            var processed = new List<string>();
            var innerTypes = type.GetInnerTypes().Where(x => x.Parent != null && x.Parent == type.FullName).ToList();
            namespaces.AddRange(innerTypes.Select(s => s.TypeNamespace));
            assemblies.AddRange(innerTypes.Select(s => s.Location));
            var depVars = _ruleSet.RuleSets.SelectMany(x => x.DependentRules).ToList();
            var props = _ruleSet.RuleSets.SelectMany(x => x.PropertyRules).Select(y => y.Name.Trim().Replace(".", "").Camelize());
            foreach (var innerType in innerTypes)
            {
                if (innerType.Status == TypeStatus.Class)
                {
                    var fullName = innerType.Type.FullName;
                    if (processedTypes.Contains(fullName))
                    {
                        break;
                    }

                    processedTypes.Add(fullName);
                    var varName = innerType.Name.Trim().Replace(".", "").Camelize();
                    variables.Add(varName);
                    var anotherFaker = BogusCreator(innerType.Type, varName, processedTypes, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    sb.Prepend(anotherFaker);
                    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate())", 1);
                }

                if (innerType.Status == TypeStatus.Enumerable || innerType.Status == TypeStatus.Collection)
                {
                    var varName = innerType.Name.Trim().Replace(".", "").Camelize();
                    var argType = innerType.Type.GetGenericArguments()[0];
                    var itemTypeText = argType.FullName;
                    var itemTypeStatus = argType.IsClass;
                    if (!processedTypes.Contains(itemTypeText) && itemTypeStatus)
                    {
                        variables.Add(varName.Singularize(false));
                        var anotherFaker = BogusCreator(argType, varName.Singularize(false), processedTypes, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                        sb.Prepend(anotherFaker);
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName.Singularize(false)}.Generate(100))", 1);
                    }
                    var isUsedVar = depVars.FirstOrDefault(x => x.VariableName == varName);
                    if (isUsedVar != null)
                    {
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {isUsedVar.UsedVariableName}.Generate({isUsedVar.Repetition}).ToList())", 1);
                    }
                    if (props.Contains(varName))
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
                    }
                    //else
                    //{
                    //    variables.Add(varName);
                    //    var itemType = innerType.Type.GetGenericArguments()[0];
                    //    var anotherFaker = BogusCreator(itemType, varName, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    //    sb.Prepend(anotherFaker);

                    //    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToList())", 1);
                    //}
                }
                if (innerType.Status == TypeStatus.Array)
                {
                    var varName = innerType.Name.Trim().Replace(".", "").Camelize();
                    var isUsedVar = depVars.FirstOrDefault(x => x.VariableName == varName);
                    if (isUsedVar != null)
                    {
                        sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {isUsedVar.UsedVariableName}.Generate({isUsedVar.Repetition}).ToArray())", 1);
                    }
                    if (props.Contains(varName))
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
                                    /*var exp = "";
                                    if (innerType.TypeName.Contains("[]"))
                                    {
                                        exp = propRule.SetterExpression.ToString();
                                    }
                                    else
                                    {
                                        exp = propRule.SetterExpressionText;
                                    }*/
                                    sb.AppendLine($".RuleFor({propRule.PropertyExpressionText}, {/*exp*/propRule.SetterExpression.ToString()})", 1);
                                    processed.Add(innerType.UniqueId);
                                }
                            }
                        }
                    }
                    //else
                    //{
                    //    variables.Add(varName);
                    //    var elementType = innerType.Type.GetElementType();
                    //    var anotherFaker = BogusCreator(elementType, varName, variables, namespaces, assemblies).Source + new string('\t', 1) + ";" + Environment.NewLine;
                    //    sb.Prepend(anotherFaker);
                    //    sb.AppendLine($".RuleFor((x) => x.{innerType.Name}, (f) => {varName}.Generate(100).ToArray())", 1);
                    //}
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
                        var status = conditionalPropRule.Condition == null ? false : conditionalPropRule.Condition(innerType.Name);
                        if (status && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
                        {
                            sb.AppendLine($".RuleFor({conditionalPropRule.PropertyExpressionText}, {conditionalPropRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                        var typeStatus = conditionalPropRule.ConditionByType == null ? false : conditionalPropRule.ConditionByType(innerType.Name, innerType.Type);
                        if (typeStatus && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
                        {
                            var prop = conditionalPropRule.PropertyExpressionText == null ? $"(x) => x.{innerType.Name}" : conditionalPropRule.PropertyExpressionText;


                            sb.AppendLine($".RuleFor({prop}, {conditionalPropRule.SetterExpressionText})", 1);
                            processed.Add(innerType.UniqueId);
                        }
                    }
                    foreach (var rule in _ruleSet.RuleSets)
                    {
                        foreach (var conditionalPropRule in rule.ConditionalPropertyRules)
                        {
                            var status = conditionalPropRule.Condition == null ? false : conditionalPropRule.Condition(innerType.Name);
                            if (status && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
                            {
                                var prop = conditionalPropRule.PropertyExpressionText == null ? $"(x) => x.{innerType.Name}" : conditionalPropRule.PropertyExpressionText;


                                sb.AppendLine($".RuleFor({prop}, {conditionalPropRule.SetterExpressionText})", 1);
                                processed.Add(innerType.UniqueId);
                            }
                            var typeStatus = conditionalPropRule.ConditionByType == null ? false : conditionalPropRule.ConditionByType(innerType.Name, innerType.Type);
                            if (typeStatus && !processed.Contains(innerType.UniqueId) && _ruleSet.Locales.ContainsOneOf(conditionalPropRule.Locales))
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
            var fakerSource = bogusGenerator.ToString();
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