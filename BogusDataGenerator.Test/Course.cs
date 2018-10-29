using BogusDataGenerator.Test.Dep;
using System;
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

    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public byte[] Photo { get; set; }
        public decimal Height { get; set; }
        public float Weight { get; set; }
        public Grade Grade { get; set; }
    }

    public class Grade
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public string Section { get; set; }

        public ICollection<Student> Students { get; set; }
    }
}
