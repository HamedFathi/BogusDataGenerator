using Bogus;
using BogusDataGenerator.Models;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BogusDataGenerator
{
    public static class RuntimeBogusGenerator<T> where T : class, new()
    {
        public static List<T> AutoFaker(int count = 1, params BogusData[] bogusData)
        {
            var name = typeof(T).Name;
            var variableName = name.Camelize();
            var className = $"{name}TestData";
            var bogusGenerator = new BogusGenerator<T>().AddPredefinedRules(bogusData);
            var fakerSource = bogusGenerator.Create();
            var assemblies = bogusGenerator.Assemblies.Distinct().ToList();
            assemblies.Add(typeof(Faker<>).Assembly.Location);
            var namespaces = bogusGenerator.Namespaces.Select(x => "using " + x + ";").Aggregate((a, b) => a + Environment.NewLine + b);
            var source = $@"
using System;
using System.Collections.Generic;
using Bogus;
{namespaces}
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
