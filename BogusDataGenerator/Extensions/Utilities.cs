using BogusDataGenerator.Enums;
using BogusDataGenerator.Models;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BogusDataGenerator.Extensions
{
    internal static class Utilities
    {
        public static List<InnerTypeResult> GetInnerTypes(this Type type, params Type[] predefinedTypes)
        {
            var id = type.FullName + "-" + type.GetHashCode();
            var result = Cache.CacheManager.Instance.GetOrSet<List<InnerTypeResult>>(id, GetInnerTypesInfo(type, null, 0, "", predefinedTypes));
            return result;
        }
        public static List<InnerTypeResult> Sort(this List<InnerTypeResult> innerTypeResults, SortType sortType)
        {
            List<InnerTypeResult> sorted = null;
            if (sortType == SortType.Ascending)
            {
                sorted = innerTypeResults.OrderBy(x => x.Level).ToList();
            }
            else
            {
                sorted = innerTypeResults.OrderByDescending(x => x.Level).ToList();
            }
            return sorted;
        }
        public static List<InnerTypeResult> Distinct(this List<InnerTypeResult> innerTypeResults, RemovingPriority removingPriority)
        {
            var sortType = removingPriority == RemovingPriority.FromLowerLevel ? SortType.Descending : SortType.Ascending;
            var sorted = innerTypeResults.Sort(sortType);
            var newResult = new List<InnerTypeResult>();
            foreach (var result in sorted)
            {
                var currentResult = newResult.Select(x => x.Type.FullName).ToList();
                if (!currentResult.Contains(result.Type.FullName))
                {
                    newResult.Add(result);
                }
            }
            return newResult;
        }
        internal static string GetFullName(this Type type)
        {
            var name = type.ToString()
                           .Replace('[', '<')
                           .Replace(']', '>');
            name = Regex.Replace(name, @"`[0-9][0-9]*", "");
            return name;
        }
        internal static string Repeat(this string input, int count)
        {
            if (!string.IsNullOrEmpty(input))
            {
                StringBuilder builder = new StringBuilder(input.Length * count);

                for (int i = 0; i < count; i++) builder.Append(input);

                return builder.ToString();
            }

            return string.Empty;
        }
        internal static string GetFriendlyTypeName(this Type type)
        {
            var mscorlib = Assembly.GetAssembly(typeof(int));
            using (var provider = new CSharpCodeProvider())
            {
                foreach (var definedType in mscorlib.DefinedTypes)
                {
                    if (string.Equals(definedType.Namespace, "System"))
                    {
                        var typeRef = new CodeTypeReference(definedType);
                        var csTypeName = provider.GetTypeOutput(typeRef);
                        if (csTypeName.IndexOf('.') == -1 && definedType.FullName == type.ToString())
                        {
                            return csTypeName.ToString();
                        }
                    }
                }
            }
            return null;
        }

        internal static string GetFriendlyTypeName(this string typeName)
        {
            var mscorlib = Assembly.GetAssembly(typeof(int));
            using (var provider = new CSharpCodeProvider())
            {
                foreach (var definedType in mscorlib.DefinedTypes)
                {
                    if (string.Equals(definedType.Namespace, "System"))
                    {
                        var typeRef = new CodeTypeReference(definedType);
                        var csTypeName = provider.GetTypeOutput(typeRef);
                        if (csTypeName.IndexOf('.') == -1 && definedType.FullName == typeName)
                        {
                            return csTypeName.ToString();
                        }
                    }
                }
            }
            return null;
        }
        internal static int CountOfSubstring(this string text, string value)
        {
            int count = 0;
            int minIndex = text.IndexOf(value, 0);
            while (minIndex != -1)
            {
                minIndex = text.IndexOf(value, minIndex + value.Length);
                count++;
            }
            return count;
        }
        private static List<InnerTypeResult> GetInnerTypesInfo(this Type type, List<string> processedTypes, int prevLevel = 0, string propertyName = "", params Type[] predefinedTypes)
        {
            if (processedTypes == null)
            {
                processedTypes = new List<string>();
            }
            int level = prevLevel;
            var typeList = new List<InnerTypeResult>();
            var isSimpleType = type.IsSimpleType(predefinedTypes); // Simple types no need any investigation.
            if (isSimpleType && type.IsNullableValueType()) // Nullable value types should check again like int? or Nullable<int>
            {
                level = prevLevel + 1;
                var underlyingType = Nullable.GetUnderlyingType(type);
                typeList.Add(new InnerTypeResult() { Type = underlyingType, Level = level, Name = propertyName, Status = GetTypeStatus(type) });
            }
            else // Complex types
            {
                if (type.IsGenericType) // Generic type
                {
                    if (type.IsDictionary())
                    {
                        level = prevLevel + 1;
                        Type keyType = type.GetGenericArguments()[0];
                        typeList.Add(new InnerTypeResult() { Type = keyType, Level = level, Name = propertyName, Status = TypeStatus.DictionaryKey });
                        typeList.AddRange(GetInnerTypesInfo(keyType, processedTypes, level, propertyName, predefinedTypes));

                        Type valueType = type.GetGenericArguments()[1];
                        typeList.Add(new InnerTypeResult() { Type = valueType, Level = level, Name = propertyName, Status = TypeStatus.DictionaryValue });
                        typeList.AddRange(GetInnerTypesInfo(valueType, processedTypes, level, propertyName, predefinedTypes));
                    }
                    if ((type.IsEnumerable() || type.IsCollection()) && !type.IsDictionary())
                    {
                        level = prevLevel + 1;
                        Type itemType = type.GetGenericArguments()[0];
                        propertyName = string.IsNullOrEmpty(propertyName) ? itemType.Name : propertyName;
                        typeList.Add(new InnerTypeResult() { Type = itemType, Level = level, Parent = type.ToString(), Name = propertyName, Status = GetTypeStatus(itemType) });
                        typeList.AddRange(GetInnerTypesInfo(itemType, processedTypes, level, propertyName, predefinedTypes));
                    }
                    if (type.IsTuple())
                    {
                        level = prevLevel + 1;
                        var tupleArgs = type.GetGenericArguments();
                        foreach (var arg in tupleArgs)
                        {
                            typeList.Add(new InnerTypeResult() { Type = arg, Level = level, Name = propertyName, Status = TypeStatus.TupleArgument });
                            typeList.AddRange(GetInnerTypesInfo(arg, processedTypes, level, propertyName, predefinedTypes));
                        }
                    }
                }
                else // Non generic type
                {
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType();
                        level = prevLevel + 1;
                        propertyName = string.IsNullOrEmpty(propertyName) ? elementType.Name : propertyName;
                        typeList.Add(new InnerTypeResult() { Type = elementType, Level = level, Parent = type.ToString(), Name = propertyName, Status = TypeStatus.ArrayElement });
                        typeList.AddRange(GetInnerTypesInfo(elementType, processedTypes, level, propertyName, predefinedTypes));
                    }
                    if (type.IsInterface) // I am not sure!
                    {
                        // TODO
                    }
                    if (type.IsClassOnly() && !processedTypes.Contains(type.FullName))
                    {
                        level = prevLevel + 1;
                        var result = type.GetProperties().Select(x => new InnerTypeResult()
                        {
                            Type = x.PropertyType,
                            Level = level,
                            Status = GetTypeStatus(x.PropertyType),
                            Name = x.Name,
                            Parent = type.ToString()
                        })
                            .ToList();
                        typeList.AddRange(result);
                        processedTypes.Add(type.FullName);
                        var newResult = result.Where(x => x.Type != type).ToList();
                        foreach (var currentResult in newResult)
                        {
                            if (!processedTypes.Contains(currentResult.Type.FullName))
                                typeList.AddRange(GetInnerTypesInfo(currentResult.Type, processedTypes, level, currentResult.Name, predefinedTypes));
                        }
                    }
                }
            }

            return typeList.ToList();
        }

        private static TypeStatus GetTypeStatus(this Type type, params Type[] predefinedTypes)
        {
            if (predefinedTypes.Contains(type))
                return TypeStatus.Predefined;
            if (type.IsPrimitive)
                return TypeStatus.Primitive;
            if (type.IsArray)
                return TypeStatus.Array;
            if (type.IsString())
                return TypeStatus.String;
            if (type.IsDateTime())
                return TypeStatus.DateTime;
            if (type.IsEnum)
                return TypeStatus.Enum;
            if (type.IsValueType)
                return TypeStatus.ValueType;
            if (type.IsDictionary())
                return TypeStatus.Dictionary;
            if (type.IsEnumerable())
                return TypeStatus.Enumerable;
            if (type.IsCollection())
                return TypeStatus.Collection;
            if (type.IsTuple())
                return TypeStatus.Tuple;
            if (type.IsInterface)
                return TypeStatus.Interface;
            if (type.IsClass)
                return TypeStatus.Class;

            return TypeStatus.Unknown;
        }

        private static bool IsClassOnly(this Type type)
        {
            var result = type.IsClass
                && !type.IsArray
                && !type.IsDictionary()
                && !type.IsCollection()
                && !type.IsEnumerable()
                && !type.IsTuple();
            return result;
        }

        private static bool IsSimpleType(this Type type, params Type[] predefinedTypes)
        {
            var isPrimitive = type.IsPrimitive;
            var isString = type.IsString();
            var isEnum = type.IsEnum;
            var isValueType = type.IsValueType;
            var isPredefinedTypes = predefinedTypes.Contains(type);
            var isValidTypes = isPrimitive || isString || isEnum || isValueType || isPredefinedTypes; // Is simple

            var finalResult = isValidTypes;
            return finalResult;
        }
        private static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
        private static bool IsDateTime(this Type type)
        {
            return type == typeof(DateTime);
        }
        internal static bool IsCollection(this Type type)
        {
            return type.GetInterface("ICollection") != null;
        }
        private static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
        internal static bool IsEnumerable(this Type type)
        {
            return type.GetInterface("IEnumerable") != null;
        }
        private static bool IsTuple(this Type type)
        {
            return type.FullName.StartsWith("System.Tuple`", StringComparison.Ordinal);
        }
        internal static StringBuilder AppendLine(this StringBuilder sb, string content, int tab)
        {
            return sb.AppendLine(new string('\t', tab) + content);
        }
        internal static StringBuilder Append(this StringBuilder sb, string content, int tab)
        {
            return sb.Append(new string('\t', tab) + content);
        }

        internal static StringBuilder Prepend(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content);
        }

        internal static StringBuilder Prepend(this StringBuilder sb, string content, int tab)
        {
            return sb.Insert(0, new string('\t', tab) + content);
        }

        internal static string GetName<TSource, TField>(this Expression<Func<TSource, TField>> field)
        {
            if (object.Equals(field, null))
            {
                throw new NullReferenceException("Field is required");
            }

            MemberExpression expr = null;

            if (field.Body is MemberExpression)
            {
                expr = (MemberExpression)field.Body;
            }
            else if (field.Body is UnaryExpression)
            {
                expr = (MemberExpression)((UnaryExpression)field.Body).Operand;
            }
            else
            {
                const string Format = "Expression '{0}' not supported.";
                string message = string.Format(Format, field);

                throw new ArgumentException(message, "Field");
            }

            return expr.Member.Name;
        }

        internal static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
        private static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
        internal static string ToExpressionString(this Expression expression, bool trimLongArgumentList = false)
        {
            return ExpressionStringBuilder.ToString(expression, trimLongArgumentList);
        }
        private static bool IsNotType(this object source, Type targetType)
        {
            return source.GetType() != targetType;
        }
        private static bool IsType(this object source, Type targetType)
        {
            return source.GetType() == targetType;
        }

        internal static bool IsAcceptableCollection(this Type type)
        {
            var isString = type.IsNotType(typeof(string));
            var isCollection = type.IsArray || type.IsCollection() || type.IsEnumerable();
            return !isString && isCollection;
        }

        internal static bool ContainsOneOf(this string[] array, string[] items)
        {
            if (array == null || items == null)
            {
                return true;
            }
            if (array.Length == 0 || items.Length == 0)
            {
                return true;
            }
            else
            {
                foreach (var arr in array)
                {
                    foreach (var item in items)
                    {
                        if (arr == item)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
