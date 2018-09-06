using Bogus;
using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var rule1 = new BogusGenerator()
                .ConditionalPropertyRuleFor(x => x.Contains("Credit"), y => y.Random.Long(5, 200000), "kr", "en")
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
                  .AddText("Before", ExtraTextType.Before)
                  .AddText("After", ExtraTextType.After)
                  .AddPredefinedRules(rule1, rule2)
                  .Create()
              ;

            Console.WriteLine(text);
            Console.ReadKey();
        }
    }
}
