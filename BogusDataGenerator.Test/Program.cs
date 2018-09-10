using Bogus;
using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var rule1 = new BogusGenerator()
                .ConditionalPropertyRuleFor(x => x.Contains("Credit"), y => "Hello", "kr", "en")
                .TypeRuleFor<string>(x => "HERE")
                .TypeRuleFor<DateTime>(x => DateTime.Now)
                .TypeRuleFor<int>(x => x.Random.Int(400, 40000))
                .Store()
                ;
            var rule2 = new BogusGenerator<Department>()
                .PropertyRuleFor(x => x.Budget, (u, v) => u.Random.Decimal(60, 55555))
                .PropertyRuleFor(x => x.Administrator, (u, v) => 0)
                .Store()
                ;

            var text = new BogusGenerator<Course>()
                  .StrictMode()
                  .PropertyRuleFor(x => x.CourseID, (u, v) => u.UniqueIndex)
                  .TypeRuleFor(x => 100)
                  .AddPredefinedRules(rule1, rule2)
                  .Create()
              ;


            var assemblies = new List<string>();
            assemblies.Add(typeof(object).Assembly.Location);
            assemblies.Add(typeof(List<>).Assembly.Location);
            assemblies.Add(typeof(Faker<>).Assembly.Location);
            assemblies.Add(typeof(Course).Assembly.Location);
            assemblies.Add(typeof(Department).Assembly.Location);
            assemblies.Add(typeof(Expression<>).Assembly.Location);
            assemblies.Add(typeof(System.Collections.IEnumerable).Assembly.Location);
            assemblies.Add(typeof(System.Linq.Enumerable).Assembly.Location);


            var courses = RuntimeBogusGenerator<Course>.AutoFaker(200, assemblies, rule1, rule2);
            Console.WriteLine(text);
            Console.ReadKey();
        }
    }
}
