using Bogus;
using BogusDataGenerator.Test.Dep;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{
    /*public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator()
        {
            RuleFor(x => x.CourseID).NotEmpty();
        }
    }*/



    class Program
    {
        static void Main(string[] args)
        {
            var generalRules = new BogusGenerator()
                .RuleForConditionalProperty((x, y) => x.Contains("Id") && y == typeof(Guid), y => Guid.NewGuid())
                .RuleForConditionalProperty((x, y) => x.Contains("Id") && y == typeof(int), y => y.Random.Int(0, int.MaxValue))
                .RuleForConditionalProperty((x, y) => x.Contains("Id") && y == typeof(long), y => y.Random.Long(0, long.MaxValue))
                .RuleForType<DateTime>(x => x.Date.Between(new DateTime(1900, 1, 1), DateTime.Now))
                .RuleForType<Nationality[]>(x => new[] { x.PickRandom<Nationality>() })
                .RuleForType<decimal>(x => x.Random.Decimal(30, 700000))
                .RuleForType<int>(x => x.Random.Int(600, 600000))

                .Save();

            var departmentRule = new BogusGenerator<Department>()
                .RuleForProperty(x => x.Name, (u, v) => "MyDep")
                .RuleForProperty(x => x.Phones, (u, v) => new string[] { "1", "2", "3", "4", "5" })
                .Save()
                ;

            var humanRule = new BogusGenerator<Human>()
                .StrictMode()
                .RuleForProperty(x => x.FirstName, (u, v) => u.Person.FirstName)
                .RuleForProperty(x => x.LastName, (u, v) => u.Person.LastName)
                .RuleForProperty(x => x.ChildrenNames, (u, v) => new[] { "Ali", "Hasan", "Javad" })
                .RuleForProperty(x => x.SpouseId, (u, v) => u.Random.Int(0, int.MaxValue))
                .RuleForProperty(x => x.Spouse, y => y.UseBogusGenerator(typeof(Human)), 100)
                .RuleForProperty(x => x.Addresses, y => y.UseBogusGenerator(typeof(Address)), 300)
                .Save();

            var employeeRule = new BogusGenerator<Employee>()
                .StrictMode()
                .RuleForProperty(x => x.EmployeeId, (u, v) => u.UniqueIndex)
                .RuleForProperty(x => x.Department, y => y.UseBogusGenerator(typeof(Department)), 150)
                .RuleForProperty(x => x.Manager, y => y.UseBogusGenerator(typeof(Employee)), 150)
                .RuleForProperty(x => x.Subordinates, y => y.UseBogusGenerator(typeof(Employee)), 150)
                .Save();


            var data = new BogusGenerator<Employee>().AutoFaker(1000, employeeRule, humanRule, departmentRule, generalRules);


            /*var rule1 = new BogusGenerator()
                .RuleForConditionalProperty(x => x.Contains("Credit"), y => "Hello", new[] { "en" })
                .RuleForConditionalProperty(x => x.Contains("Credit"), y => "Hello", new[] { "kr", "en" })
                .RuleForConditionalProperty((x, y) => x.Contains("Rank") && y == typeof(double), y => 0.1, new[] { "en" })
                .RuleForType<string>(x => "HERE")
                .RuleForType<DateTime>(x => DateTime.Now)
                .RuleForType<int>(x => x.Random.Int(400, 40000))
                .Save()
                ;

            var rule2 = new BogusGenerator<Department>()
                .RuleForProperty(x => x.Rank, (u, v) => u.Random.Decimal(60, 55555))
                .RuleForProperty(x => x.Administrator, (u, v) => 0)
                .RuleForProperty(x => x.Phones, (u, v) => new string[] { "1", "2", "3", "4", "5" })
                .Save()
                ;

            var rule3 = new BogusGenerator<Course>()
                .StrictMode()
                .RuleForProperty(x => x.CourseID, (u, v) => u.UniqueIndex)
                .RuleForProperty(x => x.Departments, rule2, 50)
                .RuleForType(x => 100)
                .AddRuleSet(rule1, rule2)
                .UseLocales("en")
                .Save()
                ;*/

            //var courses = new BogusGenerator<Course>().AutoFaker(200, rule1, rule2, rule3);
            //Console.WriteLine(new BogusGenerator<Course>().AddRuleSet(rule1, rule2, rule3).ToString());
            //var courses2 = new BogusGenerator<Course>().AutoFaker(1000, rule1, rule2, rule3);
            //var validator = new CourseValidator();
            //FluentValidation.Results.ValidationResult results = validator.Validate(courses[0]);

            //bool success = results.IsValid;
            //IList<ValidationFailure> failures = results.Errors;
            Console.ReadKey();
        }
    }
}
