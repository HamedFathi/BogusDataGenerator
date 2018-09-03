using System;

namespace BogusDataGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = typeof(Course).GetInnerTypes();
            Console.ReadKey();
        }
    }
}
