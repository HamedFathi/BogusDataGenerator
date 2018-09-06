using Bogus;
using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using System;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = typeof(Course).GetInnerTypes();
            var testUsers = new Faker<Department>();

            var obj = testUsers.Generate(5);


            var r1 = new BogusGenerator()
                .ConditionalPropertyRuleFor(x => x.Contains("Credit"), y => y.Address.BuildingNumber(), "kr","en")
                .TypeRuleFor<string>(x => x.IndexVariable.ToString())
                .Store()
                ;

            var r = new BogusGenerator<Course>()
                  .StrictMode()
                  .PropertyRuleFor(x => x.CourseID, (u, v) => u.UniqueIndex)
                  //.ConditionalPropertyRuleFor(g => g.Contains("Credit"), (i, j) => i.Locale)
                  //.TypeRuleFor(x => x.Locale)
                  .TypeRuleFor(x => x.IndexGlobal)
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
