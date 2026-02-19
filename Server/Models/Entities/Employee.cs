namespace EmployeeAttendance.Models.Entities;

public class Employee
{
    required public int Id {get; set;}
    required public string Name {get; set;} = default!;
    required public string Sex {get; set;} = default!;
    required public string Department {get; set;} = default!;
    required public string Gmail {get; set;} = default!;
    required public string Code {get; set;} = default!;
}