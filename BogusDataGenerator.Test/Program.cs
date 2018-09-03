using BogusDataGenerator.Enums;
using System;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = typeof(Course).GetInnerTypes().Distinct(RemovingPriority.FromTopLevel);
            Console.ReadKey();
        }
    }
}
