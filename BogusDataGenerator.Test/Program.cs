using BogusDataGenerator.Test.Dep;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{
    public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator()
        {
            RuleFor(x => x.CourseID).NotEmpty();
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            var rule1 = new BogusGenerator()
                .RuleForConditionalProperty(x => x.Contains("Credit"), y => "Hello", new[] { "kr", "en" })
                .RuleForType<string>(x => "HERE")
                .RuleForType<DateTime>(x => DateTime.Now)
                .RuleForType<int>(x => x.Random.Int(400, 40000))
                .Store()
                ;
            var rule2 = new BogusGenerator<Department>()
                .RuleForProperty(x => x.Budget, (u, v) => u.Random.Decimal(60, 55555))
                .RuleForProperty(x => x.Administrator, (u, v) => 0)
                .RuleForProperty(x => x.Phones, (u, v) => new[] { "1" })

                .Store()
                ;

            var rule3 = new BogusGenerator<Course>()
                  .StrictMode()
                  .RuleForProperty(x => x.CourseID, (u, v) => u.UniqueIndex)
                  .RuleForProperty(x => x.Departments, rule2, 1)
                  .RuleForType(x => 100)
                  .AddRuleSet(rule1, rule2)
                  .Store()
              ;

            var courses = new BogusGenerator<Course>().AutoFaker(200, rule1, rule2, rule3);
            Console.WriteLine(new BogusGenerator<Course>().AddRuleSet(rule1, rule2, rule3).Text());
            var courses2 = new BogusGenerator<Course>().AutoFaker(1000, rule1, rule2, rule3);
            var validator = new CourseValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(courses[0]);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            Console.ReadKey();
        }
    }
}
