using BogusDataGenerator.Test.Dep;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{
    public class Course
    {
        public int CourseID { get; set; }	     
        public string Name { get; set; }
        public int DepartmentID { get; set; }	       
        public Department Department { get; set; }	       
        public Department[] Departments { get; set; }

    }
}
