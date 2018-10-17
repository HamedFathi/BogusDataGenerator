using BogusDataGenerator.Test.Dep;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{
    public class Course
    {
        public int CourseID { get; set; }	     
        public string Credits { get; set; }
        public int DepartmentID { get; set; }	       
        public virtual Department Department { get; set; }	       
        public virtual ICollection<Department> Departments { get; set; }

    }
}
