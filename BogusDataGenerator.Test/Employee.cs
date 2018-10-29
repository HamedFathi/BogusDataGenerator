using BogusDataGenerator.Test.Dep;
using System;
using System.Collections.Generic;

namespace BogusDataGenerator.Test
{

    public class Employee : Human
    {
        public int EmployeeId { get; set; }        
        public decimal MonthlySalary { get; set; }
        public DateTime HireDate { get; set; }
        public int DepartmentID { get; set; }
        public Department Department { get; set; }

        public int? ManagerId { get; set; }

        public Employee Manager { get; set; }

        public ICollection<Employee> Subordinates { get; set; }

    }
}
