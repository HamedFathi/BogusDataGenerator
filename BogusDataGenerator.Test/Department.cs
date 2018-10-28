﻿using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test.Dep
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public int? Administrator { get; set; }
        public int[] Phones { get; set; }
        // public virtual ICollection<Course> Courses { get; set; } // Stackoverflow problem :(
    }
}
