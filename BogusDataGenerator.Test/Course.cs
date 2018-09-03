using System;
using System.Collections.Generic;
using System.Text;

namespace BogusDataGenerator.Test
{
    public class Course
    {
        public Dictionary<int, Guid> Credits1 { get; set; }
        public IList<decimal> Credits2 { get; set; }

        public string[][][] Credits { get; set; }

        public Tuple<Dictionary<int, Guid>, decimal, string> Credits3 { get; set; }

        public int DepartmentID { get; set; }
        public virtual Department Department { get; set; }
    }
}
