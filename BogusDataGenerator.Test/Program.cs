using Bogus;
using BogusDataGenerator.Enums;
using System;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = typeof(Course).GetInnerTypes();
            var testUsers = new Faker<Course>()
                .RuleFor(x => x.CourseID, (x, y) => x.UniqueIndex);

            var obj = testUsers.Generate(5);


            var r1 = new BogusGenerator()
                .ConditionalPropertyRuleFor(x => x.Contains("Credit"), y => y.Address.BuildingNumber(), "kr")
                .TypeRuleFor<decimal>(x => x.IndexVariable)
                .Store()
                ;

            var r = new BogusGenerator<Course>()
                  .StrictMode()
                  .PropertyRuleFor(x => x.CourseID, (u, v) => u.UniqueIndex)
                  .ConditionalPropertyRuleFor(g => g.Contains("ID"), (i, j) => i.Locale)
                  .TypeRuleFor(x => x.Locale)
                  .AddText("111", ExtraTextType.Before)
                  .AddText("222", ExtraTextType.After)
                  .AddText("333", ExtraTextType.Before)
                  .AddText("444", ExtraTextType.After)
                  .AddPredefinedRule(r1)
                  .Create()
              ;

            Console.ReadKey();
        }
    }
}
