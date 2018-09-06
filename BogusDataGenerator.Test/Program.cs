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
            var result = typeof(Department).GetInnerTypes();

            var result24 = typeof(List<Department>).GetInnerTypes();
            var departmentss = new Faker<Department>()
                .StrictMode(true)
                .RuleFor((z) => z.DepartmentID, (x) => x.IndexGlobal)
                .RuleFor((z) => z.Name, (x) => x.IndexVariable.ToString())
                .RuleFor((x) => x.Budget, (u, v) => u.Random.Decimal(60, 55555))
                .RuleFor((z) => z.StartDate, (x) => System.DateTime.Now)
                .RuleFor((x) => x.Administrator, (u, v) => 0)
                ;
            var department = new Faker<Department>()
                .StrictMode(true)
                .RuleFor((z) => z.DepartmentID, (x) => x.IndexGlobal)
                .RuleFor((z) => z.Name, (x) => x.IndexVariable.ToString())
                .RuleFor((x) => x.Budget, (u, v) => u.Random.Decimal(60, 55555))
                .RuleFor((z) => z.StartDate, (x) => System.DateTime.Now)
                .RuleFor((x) => x.Administrator, (u, v) => 0)
                ;
            var course = new Faker<Course>()
                .StrictMode(true)
                .RuleFor((x) => x.CourseID, (u, v) => u.UniqueIndex)
                .RuleFor((x) => x.Credits, (y) => y.Address.BuildingNumber())
                .RuleFor((z) => z.DepartmentID, (x) => x.IndexGlobal)
                .RuleFor((z) => z.Department, (f) => department.Generate())
                .RuleFor((z) => z.Departments, (f) => departmentss.Generate(100).ToList())
                ;

            var rrrr = course.Generate(100);

            var r1 = new BogusGenerator()
                .ConditionalPropertyRuleFor(x => x.Contains("Credit"), y => y.Address.BuildingNumber(), "kr", "en")
                .TypeRuleFor<string>(x => x.IndexVariable.ToString())
                .TypeRuleFor<DateTime>(x => DateTime.Now)
                .TypeRuleFor<int>(x => x.Random.Int(400, 40000))

                .Store()
                ;

            var r2 = new BogusGenerator<Department>()
                .PropertyRuleFor(x => x.Budget, (u, v) => u.Random.Decimal(60, 55555))
                .PropertyRuleFor(x => x.Administrator, (u, v) => 0)
                .Store();

            var r = new BogusGenerator<Course>()
                  .StrictMode()
                  .PropertyRuleFor(x => x.CourseID, (u, v) => u.UniqueIndex)
                  .TypeRuleFor(x => x.IndexGlobal)

                  .AddText("111", ExtraTextType.Before)
                  .AddText("222", ExtraTextType.After)
                  .AddText("333", ExtraTextType.Before)
                  .AddText("444", ExtraTextType.After)
                  .AddPredefinedRules(r1, r2)
                  .Create()
              ;

            Console.ReadKey();
        }
    }
}
