using System;

namespace BogusDataGenerator.Test
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public int? Administrator { get; set; }

        public int[][][] CourseID2 { get; set; }

        // public virtual ICollection<Course> Courses { get; set; } // Stackoverflow problem :(
    }
}
