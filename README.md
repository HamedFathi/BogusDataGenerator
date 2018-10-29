## How BogusDataGenerator works?

`BogusDataGenerator` is a library based on [Bogus](https://github.com/bchavez/Bogus) to achieve two goals.
1. Generate random data in runtime.
2. Generate `Bogus` data generator source code for compile time.

Consider the following example:

```cs
public class Course
{
    public int CourseID { get; set; }	     
    public string Name { get; set; }
    public int DepartmentID { get; set; }	       
    public Department Department { get; set; }	       
    public Department[] Departments { get; set; }
}

public class Department
{
    public int DepartmentID { get; set; }
    public string Name { get; set; }
    public decimal Rank { get; set; }
    public DateTime StartDate { get; set; }
    public int? Administrator { get; set; }
    public string[] Phones { get; set; }
}
```


