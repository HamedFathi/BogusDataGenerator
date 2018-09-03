using Bogus;
using BogusDataGenerator.Enums;
using System;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = typeof(Course).GetInnerTypes().Distinct(RemovingPriority.FromTopLevel);

            var testUsers = new Faker<Course>()
                .RuleFor(x => x.CourseID, (x, y) => x.UniqueIndex);

            var obj = testUsers.Generate(5);



            Console.ReadKey();
        }
    }
}
