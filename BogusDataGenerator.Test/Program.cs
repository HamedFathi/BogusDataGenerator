using Bogus;
using BogusDataGenerator.Enums;
using BogusDataGenerator.Extensions;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

            var rule3 = new BogusGenerator<Course>()
                  .StrictMode()
                  .PropertyRuleFor(x => x.CourseID, (u, v) => u.UniqueIndex)
                  .TypeRuleFor(x => 100)
                  .AddPredefinedRules(rule1, rule2)
                  .Store()
              ;

            var courses = new BogusGenerator<Course>().AutoFaker(200, rule1, rule2, rule3);
            Console.WriteLine(new BogusGenerator<Course>().AddPredefinedRules(rule1, rule2, rule3).Text());
            var validator = new CourseValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(courses[0]);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            Console.ReadKey();
        }
    }
}
