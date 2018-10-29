using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test.Dep
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public string[] Phones { get; set; }
        // public virtual ICollection<Course> Courses { get; set; } // Stackoverflow problem :(
    }
}
