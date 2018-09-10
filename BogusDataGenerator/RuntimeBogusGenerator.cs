using BogusDataGenerator.Models;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Linq;
using System.Reflection;

namespace BogusDataGenerator
{
    public static class RuntimeBogusGenerator<T> where T : class, new()
    {
        public static List<T> AutoFaker(int count = 1, List<string> assembliesLocations = null, params BogusData[] bogusData)
        {
            var name = typeof(T).Name;
            var variableName = name.Camelize();
            var className = $"{name}TestData";
            var fakerSource = new BogusGenerator<T>().AddPredefinedRules(bogusData).Create();
            var source = $@"
            using System;
using System.Collections.Generic;
using Bogus;
using BogusDataGenerator.Test;
using System.Linq;

public class {className}
{{
	public List<{name}> Get()
	{{
        {fakerSource}
        var result = {variableName}.Generate({count});
        return result;
    }}
}}


";
            var errors = new List<string>();
            var type = source.ToType(null, className, out errors, assembliesLocations);

            if (errors == null)
            {
                var testData = (List<T>)type.GetMethod("Get").Invoke(Activator.CreateInstance(type), null);
                return testData;
            }

            return null;
        }

    }
}
