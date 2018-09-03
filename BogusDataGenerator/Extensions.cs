using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BogusDataGenerator
{
    public static class Extensions
    {
        public static List<InnerTypeResult> GetInnerTypes(this Type type, params Type[] predefinedTypes)
        {
            return GetInnerTypesInfo(type, 1, predefinedTypes);
        }

        public static List<InnerTypeResult> SortResult(this List<InnerTypeResult> innerTypeResults, SortType sortType = SortType.Ascending)
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


        public static List<InnerTypeResult> DistinctResult(this List<InnerTypeResult> innerTypeResults, RemovingPriority removingPriority = RemovingPriority.FromBottom)
        {
            var sorted = innerTypeResults.SortResult(removingPriority == RemovingPriority.FromBottom ? SortType.Descending : SortType.Ascending);
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
            var name = type.ToString().Replace('[', '<').Replace(']', '>');
            name = Regex.Replace(name, @"`[0-9][0-9]*", "");
            return name;
        }

        private static List<InnerTypeResult> GetInnerTypesInfo(this Type type, int prevLevel = 1, params Type[] predefinedTypes)
        {
            int level = prevLevel;
            var typeList = new List<InnerTypeResult>();
            var isSimpleType = type.IsSimpleType(predefinedTypes); // Simple types no need any investigation.
            if (isSimpleType && type.IsNullableValueType()) // Nullable value types should check again like int? or Nullable<int>
            {
                level = prevLevel + 1;
                var underlyingType = Nullable.GetUnderlyingType(type);
                typeList.Add(new InnerTypeResult() { Type = underlyingType, Level = level, Status = GetTypeStatus(type) });
            }
            else // Complex types
            {
                if (type.IsGenericType) // Generic type
                {
                    if (type.IsDictionary())
                    {
                        level = prevLevel + 1;
                        Type keyType = type.GetGenericArguments()[0];
                        typeList.Add(new InnerTypeResult() { Type = keyType, Level = level, Status = TypeStatus.DictionaryKey });
                        typeList.AddRange(GetInnerTypesInfo(keyType, level, predefinedTypes));

                        Type valueType = type.GetGenericArguments()[1];
                        typeList.Add(new InnerTypeResult() { Type = valueType, Level = level, Status = TypeStatus.DictionaryValue });
                        typeList.AddRange(GetInnerTypesInfo(keyType, level, predefinedTypes));
                    }
                    if (type.IsEnumerable() && !type.IsDictionary())
                    {
                        level = prevLevel + 1;
                        Type itemType = type.GetGenericArguments()[0];
                        typeList.Add(new InnerTypeResult() { Type = type, Level = level, Status = TypeStatus.Enumerable });
                        typeList.AddRange(GetInnerTypesInfo(itemType, level, predefinedTypes));
                    }
                    if (type.IsCollection() && !type.IsDictionary())
                    {
                        level = prevLevel + 1;
                        Type itemType = type.GetGenericArguments()[0];
                        typeList.Add(new InnerTypeResult() { Type = type, Level = level, Status = TypeStatus.Collection });
                        typeList.AddRange(GetInnerTypesInfo(itemType, level, predefinedTypes));
                    }
                    if (type.IsTuple())
                    {
                        level = prevLevel + 1;
                        var tupleArgs = type.GetGenericArguments();
                        foreach (var arg in tupleArgs)
                        {
                            typeList.Add(new InnerTypeResult() { Type = arg, Level = level, Status = TypeStatus.TupleArgument });
                            typeList.AddRange(GetInnerTypesInfo(arg, level, predefinedTypes));
                        }
                    }
                }
                else // Non generic type
                {
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType();
                        level = prevLevel + 1;
                        typeList.Add(new InnerTypeResult() { Type = elementType, Level = level, Status = TypeStatus.ArrayElement });
                        typeList.AddRange(GetInnerTypesInfo(elementType, level, predefinedTypes));
                    }
                    if (type.IsInterface) // I am not sure!
                    {
                        // TODO
                    }
                    if (type.IsClassOnly())
                    {
                        var result = type.GetProperties().Select(x => new InnerTypeResult()
                        {
                            Type = x.PropertyType,
                            Level = level,
                            Status = GetTypeStatus(x.PropertyType)
                        })
                            .ToList();
                        typeList.AddRange(result);
                        foreach (var currentResult in result)
                        {
                            typeList.AddRange(GetInnerTypesInfo(currentResult.Type, level, predefinedTypes));
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

            // var isInvalidType = type.IsDictionary() || type.IsEnumerable() || type.IsCollection() || type.IsTuple() || type.IsArray; // Should not be complex.
            var isValidTypes = isPrimitive || isString || isEnum || isValueType || isPredefinedTypes; // Is simple

            var finalResult = isValidTypes; // && !isInvalidType;
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
        private static bool IsCollection(this Type type)
        {
            return type.GetInterface("ICollection") != null;
        }
        private static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
        private static bool IsEnumerable(this Type type)
        {
            return type.GetInterface("IEnumerable") != null;
        }
        private static bool IsTuple(this Type type)
        {
            return type.FullName.StartsWith("System.Tuple`", StringComparison.Ordinal);
        }
        private static StringBuilder AppendLine(this StringBuilder sb, string value, int tab)
        {
            return sb.AppendLine(new string('\t', tab) + value);
        }
        private static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
